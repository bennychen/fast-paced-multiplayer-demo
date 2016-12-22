using System.Collections.Generic;
using UnityEngine;

public class Proxy : MonoBehaviour
{
    public void Awake()
    {
        _demo = FindObjectOfType<Demo>();
    }

    public void FixedUpdate()
    {
        if (_states.Count == 0)
        {
            return;
        }
        else if (_states.Count == 1)
        {
            transform.position = _currentLastKnownState.Position;
            return;
        }

        if (_needCorrectionOnNextState)
        {
            return;
        }

        float interpolationStartTime = GetServerTime() - (float)_demo.InterpolationDelayTime / 1000;

        if (interpolationStartTime < _currentLastKnownState.Timestamp)
        {
            DoInterpolation(interpolationStartTime);
        }
        else if (interpolationStartTime < _currentLastKnownState.Timestamp + (float)_demo.MaxExtrapolationTime / 1000)
        {
            DoExtrapolation();
        }
    }

    // algorithm is based on https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking
    private void DoInterpolation(float interpolationStartTime)
    {
        for (int i = 0; i < _states.Count; i++)
        {
            if (_states[i].Timestamp <= interpolationStartTime || i == _states.Count - 1)
            {
                Snapshot latterState = _states[Mathf.Max(i - 1, 0)];
                Snapshot earlierState = _states[i];
                float length = latterState.Timestamp - earlierState.Timestamp;
                float t = 0;
                if (length > 0.0001f)
                {
                    t = (interpolationStartTime - earlierState.Timestamp) / length;
                }
                Vector3 targetPosition = Vector3.Lerp(earlierState.Position, latterState.Position, t);
                if (Vector3.SqrMagnitude(targetPosition - transform.position) < 5f)
                {
                    transform.position = targetPosition;
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, targetPosition, 0.2f);
                }
                return;
            }
        }
    }

    private void DoExtrapolation()
    {
        Vector3 targetPosition = transform.position + _currentLastKnownState.Velocity * Time.fixedDeltaTime;

        if (Vector3.Distance(targetPosition, _currentLastKnownState.Position) > _demo.TolerantDistance)
        {
            _needCorrectionOnNextState = true;
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    private void SyncSnapshot(Snapshot snapshot)
    {
        if (_needCorrectionOnNextState == true)
        {
            _needCorrectionOnNextState = false;
            transform.position = snapshot.Position;
            _states.Clear();
        }


        if (_states.Count == 0)
        {
            _states.Add(snapshot);
            _currentLastKnownState = _states[0];
        }
        else
        {
            if (snapshot.Timestamp < _currentLastKnownState.Timestamp)
            {
                return;
            }

            if (_states.Count >= StateBufferSize)
            {
                _states.RemoveAt(StateBufferSize - 1);
            }
            _states.Insert(0, snapshot);
            _currentLastKnownState = _states[0];
        }
    }

    private float GetServerTime()
    {
        return Time.fixedTime - (float)_demo.LatencyMilliseconds / 1000;
    }

    private Demo _demo;
    private Vector3 _velocity;

    private List<Snapshot> _states = new List<Snapshot>();
    private Snapshot _currentLastKnownState;
    private bool _needCorrectionOnNextState;
    private float _currentSimulateTime;

    private const int StateBufferSize = 20;
}
