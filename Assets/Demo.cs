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
    public bool EnablePrediction = false;
    [SerializeField]
    public bool EnableReconciliation = false;
    [SerializeField]
    public float TolerantDistance = 5;
    [SerializeField]
    [Range(0, 1000)]
    public int LatencyMilliseconds;

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
                _client.SendMessage(methodName, value);
                break;
            case Target.Server:
                _server.SendMessage(methodName, value);
                break;
            case Target.Proxy:
                _proxy.SendMessage(methodName, value);
                break;
        }
    }

    [SerializeField]
    private GameObject _client;

    [SerializeField]
    private GameObject _server;

    [SerializeField]
    private GameObject _proxy;
}
