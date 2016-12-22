using UnityEngine;

public class Client : MonoBehaviour
{
    public void Awake()
    {
        _demo = FindObjectOfType<Demo>();
        _lastInput = 0;
    }

    public void FixedUpdate()
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

        if (input != _lastInput)
        {
            _lastInputCommandID++;
            _demo.Call(Target.Server, "SyncInput", new InputData
            {
                Input = input,
                CommandID = _lastInputCommandID,
            });
            _lastInput = input;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _demo.Call(Target.Server, "Jump");
            
            if (_demo.EnablePrediction)
            {
                PredictJump();
            }
        }

        if (_demo.EnablePrediction)
        {
            PredictMove(input);
        }
    }

    public void SyncSnapshot(Snapshot s)
    {
        if (_demo.EnablePrediction)
        {
            Vector3 position = transform.position;
            if (Mathf.Abs(position.x - s.Position.x) > _demo.TolerantDistance)
            {
                if (_demo.EnableReconciliation)
                {
                    if (_lastInputCommandID == s.CommandID)
                    {
                        position.x = s.Position.x;
                    }
                }
                else
                {
                    position.x = s.Position.x;
                }
            }
            else if (_lastInput == 0)
            {
                if (_demo.EnableReconciliation)
                {
                    if (_lastInputCommandID == s.CommandID)
                    {
                        position.x = Mathf.Lerp(position.x, s.Position.x, 0.3f);
                    }
                }
            }
            transform.position = position;
        }
        else
        {
            transform.position = s.Position;
            _velocity = s.Velocity;
        }
    }

    public void PredictMove(int input)
    {
        _isGrounded = Physics.Raycast(new Ray(transform.position, Vector3.down), 1.05f, 
            Fangtang.Layers.onlyIncluding(Fangtang.Layers.CLIENT));

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity = new Vector3(_velocity.x, 0, _velocity.z);
        }

        if (input != 0 )
        {
            _velocity.x = Mathf.Sign(input) * _demo.HorizontalSpeed;
        }
        else
        {
            _velocity.x = 0;
        }
        transform.position += _velocity * Time.fixedDeltaTime;

        if (!_isGrounded)
        {
            _velocity.y -= _demo.Gravity * Time.fixedDeltaTime;
        }
    }

    public void PredictJump()
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

    private void OnGUI()
    {
        GUILayout.Label("I am Client");
    }

    private int _lastInput;
    private Demo _demo;
    private Vector3 _velocity;

    // For Prediction
    private bool _isGrounded;

    // For Reconciliation
    private int _lastInputCommandID = 0;
    private int _needToReconcileID;
    private float _targetX;
}
