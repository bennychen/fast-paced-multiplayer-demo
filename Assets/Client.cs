using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{
    public void Awake()
    {
        _demo = FindObjectOfType<Demo>();
    }

    public void FixedUpdate()
    {
        while (_states.Count > 0)
        {
            Snapshot state = _states[0];
            _states.RemoveAt(0);

            transform.position = new Vector3(state.Position.x, transform.position.y, state.Position.z);

            if (_demo.EnableReconciliation)
            {
                int i = 0;
                while (i < _pendingInputs.Count)
                {
                    var input = _pendingInputs[i];
                    if (input.InputID <= state.LastProcessedInput)
                    {
                        // Already processed by server
                        _pendingInputs.RemoveAt(i);
                    }
                    else
                    {
                        // Not processed by server
                        ApplyHorizontalMove(input);
                        i++;
                    }
                }
            }
            else
            {
                _pendingInputs.Clear();
            }
        }

        InputData data = ProcessHorizontalInput();

        if (_demo.EnableServerPrediction)
        {
            ApplyVerticalMove();
            ApplyHorizontalMove(data);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _demo.Call(Target.Server, "Jump");

            if (_demo.EnableServerPrediction)
            {
                Jump();
            }
        }
    }

    public void SyncSnapshot(Snapshot s)
    {
        _states.Add(s);

        if (!_demo.EnableServerPrediction)
        {
            transform.position = new Vector3(transform.position.x, s.Position.y, transform.position.z);
        }
    }

    private InputData ProcessHorizontalInput()
    {
        int input = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            input = -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            input = 1;
        }
        else
        {
            return null;
        }

        _currentInputID++;
        InputData data = new InputData()
        {
            Input = input,
            InputID = _currentInputID,
        };
        _demo.Call(Target.Server, "SyncInput", data);

        _pendingInputs.Add(data);
        return data;
    }

    private void ApplyHorizontalMove(InputData data)
    {
        if (data != null && data.Input != 0)
        {
            _velocity.x = Mathf.Sign(data.Input) * _demo.HorizontalSpeed;
        }
        else
        {
            _velocity.x = 0;
        }
        transform.position += Vector3.Scale(_velocity, new Vector3(1, 0, 1)) * Time.fixedDeltaTime;
    }

    private void ApplyVerticalMove()
    {
        _isGrounded = Physics.Raycast(new Ray(transform.position, Vector3.down), 1.1f,
            Fangtang.Layers.onlyIncluding(Fangtang.Layers.CLIENT));

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = 0;
            transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
        }

        transform.position += Vector3.up * _velocity.y * Time.fixedDeltaTime;

        if (!_isGrounded)
        {
            _velocity.y -= _demo.Gravity * Time.fixedDeltaTime;
        }
    }

    private void Jump()
    {
        if (_isGrounded)
        {
            _velocity.y = _demo.JumpSpeed;
        }
    }

    private float GetServerTime()
    {
        return Time.fixedTime - (float)_demo.LatencyMilliseconds / 1000;
    }

    private Demo _demo;
    private Vector3 _velocity;
    private List<InputData> _pendingInputs = new List<InputData>();
    private List<Snapshot> _states = new List<Snapshot>();

    private bool _isGrounded;
    private int _currentInputID = 0;
}
