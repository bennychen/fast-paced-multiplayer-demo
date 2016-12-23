using System.Collections;
using UnityEngine;

public enum Target
{
    Client,
    Server,
    Proxy,
}

public class Demo : MonoBehaviour
{
    [SerializeField]
    public float Gravity = -30;
    [SerializeField]
    public float HorizontalSpeed = 15;
    [SerializeField]
    public float JumpSpeed = 15;
    [SerializeField]
    public float Tickrate = 60;
    [SerializeField]
    public bool EnableClientPrediction = false;
    [SerializeField]
    public bool EnableServerReconciliation = false;
    [SerializeField]
    public bool EnableProxyInterpolation = false;
    [SerializeField]
    [Range(0, 1000)]
    public int LatencyMilliseconds;
    [SerializeField]
    [Range(0, 1000)]
    public int InterpolationDelayTime = 100; // in miliseconds
    [SerializeField]
    [Range(0, 1000)]
    public int MaxExtrapolationTime = 500; // in miliseconds
    [SerializeField]
    public float TolerantDistance = 5;

    public float GetFrameTime()
    {
        return 1f / Tickrate;
    }

    public void Call(Target target, string methodName, object value = null)
    {
        if (LatencyMilliseconds > 0)
        {
            Job.Make(DelayedSendMessageToTarget(target, methodName, value));
        }
        else
        {
            SendMessageToTarget(target, methodName, value);
        }
    }

    public IEnumerator DelayedSendMessageToTarget(Target target, string methodName, object value)
    {
        yield return new WaitForSeconds((float)LatencyMilliseconds / 1000);
        SendMessageToTarget(target, methodName, value);
    }

    private void SendMessageToTarget(Target target, string methodName, object value)
    {
        switch (target)
        {
            case Target.Client:
                if (_client != null)
                {
                    _client.SendMessage(methodName, value);
                }
                break;
            case Target.Server:
                if (_server != null)
                {
                    _server.SendMessage(methodName, value);
                }
                break;
            case Target.Proxy:
                if (_proxy != null)
                {
                    _proxy.SendMessage(methodName, value);
                }
                break;
        }
    }

    private void Update()
    {
        if (EnableServerReconciliation == true)
        {
            EnableClientPrediction = true;
        }
    }

    [SerializeField]
    private GameObject _client;

    [SerializeField]
    private GameObject _server;

    [SerializeField]
    private GameObject _proxy;
}
