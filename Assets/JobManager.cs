using System.Collections;

public class JobManager : MonoSingleton<JobManager>
{
}

public class Job
{
    public event System.Action<bool> OnComplete;

    public bool Running { get; private set; }
    public bool Paused { get; private set; }

    public Job(IEnumerator coroutine)
        : this(coroutine, true)
    { }

    public Job(IEnumerator coroutine, bool shouldStart)
    {
        _coroutine = coroutine;

        if (shouldStart)
            Start();
    }

    public void Start()
    {
        Running = true;
        JobManager.Instance.StartCoroutine(DoWork());
    }

    public IEnumerator StartAsCoroutine()
    {
        Running = true;
        yield return JobManager.Instance.StartCoroutine(DoWork());
    }

    public void Pause()
    {
        Paused = true;
    }

    public void Unpause()
    {
        Paused = false;
    }

    public void Kill()
    {
        _jobWasKilled = true;
        Running = false;
        Paused = false;
    }

    public void Kill(float delayInSeconds)
    {
        var delay = (int)(delayInSeconds * 1000);
        new System.Threading.Timer(obj =>
        {
            lock (this)
            {
                Kill();
            }
        }, null, delay, System.Threading.Timeout.Infinite);
    }

    public static Job Make(IEnumerator coroutine)
    {
        return new Job(coroutine);
    }

    public static Job Make(IEnumerator coroutine, bool shouldStart)
    {
        return new Job(coroutine, shouldStart);
    }

    private IEnumerator DoWork()
    {
        // null out the first run through in case we start paused
        yield return null;

        while (Running)
        {
            if (Paused)
            {
                yield return null;
            }
            else
            {
                // run the next iteration and stop if we are done
                if (_coroutine.MoveNext())
                {
                    yield return _coroutine.Current;
                }
                else
                {
                    Running = false;
                }
            }
        }

        if (OnComplete != null)
        {
            OnComplete(_jobWasKilled);
        }
    }

    private IEnumerator _coroutine;
    private bool _jobWasKilled;
}