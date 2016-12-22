using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour
{
    public void Awake()
    {
        _demo = FindObjectOfType<Demo>();
    }

    public void FixedUpdate()
    {
        ApplyVerticalMove();

        if (Time.fixedTime - _lastTickTime > _demo.GetFrameTime())
        {
            while (_pendingInputs.Count > 0)
            {
                InputData data = _pendingInputs[0];
                _pendingInputs.RemoveAt(0);

                if (data != null)
                {
                    ApplyHorizontalInput(data);
                    _lastProcessedInputID = data.InputID;
                }
            }


            Tick();
            _lastTickTime = Time.fixedTime;
        }
    }

    public void Tick()
    {
        _demo.Call(Target.Client, "SyncSnapshot", new Snapshot()
        {
            Position = transform.position,
            Velocity = _velocity,
            Timestamp = Time.fixedTime,
            LastProcessedInput = _lastProcessedInputID,
        });

        _demo.Call(Target.Proxy, "SyncSnapshot", new Snapshot()
        {
            Position = transform.position,
            Velocity = _velocity,
            Timestamp = Time.fixedTime,
            LastProcessedInput = _lastProcessedInputID,
        });
    }

    public void SyncInput(InputData data)
    {
        _pendingInputs.Add(data);
    }

    public void Jump()
    {
        if (_isGrounded)
        {
            _velocity.y = _demo.JumpSpeed;
        }
    }

    private void ApplyVerticalMove()
    {
        _isGrounded = Physics.Raycast(new Ray(transform.position, Vector3.down), 1.1f,
            Fangtang.Layers.onlyIncluding(Fangtang.Layers.SERVER));

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

    private void ApplyHorizontalInput(InputData data)
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

    private Demo _demo;
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _lastTickTime = float.MinValue;
    private int _lastProcessedInputID;

    private List<InputData> _pendingInputs = new List<InputData>();
}
