using UnityEngine;

public class Server : MonoBehaviour
{
    public void Awake()
    {
        _demo = FindObjectOfType<Demo>();
    }

    public void FixedUpdate()
    {
        if (Time.fixedTime - _lastTickTime > _demo.GetFrameTime())
        {
            _isGrounded = Physics.Raycast(new Ray(transform.position, Vector3.down), 1.05f,
                Fangtang.Layers.onlyIncluding(Fangtang.Layers.SERVER));

            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = 0;
            }

            if (_input != 0)
            {
                _velocity.x = Mathf.Sign(_input) * _demo.HorizontalSpeed;
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
            CommandID = _lastInputCommandID,
        });

        _demo.Call(Target.Proxy, "SyncSnapshot", new Snapshot()
        {
            Position = transform.position,
            Velocity = _velocity,
            Timestamp = Time.fixedTime,
            CommandID = _lastInputCommandID,
        });
    }

    public void SyncInput(InputData data)
    {
        _input = data.Input;
        _lastInputCommandID = data.CommandID;
    }

    public void Jump()
    {
        if (_isGrounded)
        {
            _velocity.y = _demo.JumpSpeed;
        }
    }

    private Demo _demo;
    private Vector3 _velocity;
    private bool _isGrounded;
    private int _input;
    private float _lastTickTime = float.MinValue;
    private int _lastInputCommandID;
}
