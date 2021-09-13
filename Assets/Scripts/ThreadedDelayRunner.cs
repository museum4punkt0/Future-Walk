using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ThreadedDelayRunner
{
    private bool paused = false;
    private readonly List<CancellationTokenSource> cancelTokens = new List<CancellationTokenSource>();
    private bool cancellingAllThreads = false;
    private string name;

    public int ThreadCount {
        get {
            int count = 0;
            lock(cancelTokens)
            {
                count = cancelTokens.Count;
            }
            return count;
        }
    }

    public ThreadedDelayRunner(string _name)
    {
        name = _name;
    }

    public void CancelAllThreads()
    {
        lock(cancelTokens)
        {
            if (cancelTokens.Count > 0) Log.d("!! CancelAllThreads [" + name + "]: " + cancelTokens.Count);

            cancellingAllThreads = true;
            foreach (var item in cancelTokens)
            {
                item.Cancel();
            }

            cancelTokens.Clear();
            cancellingAllThreads = false;
        }
    }

    public async void RunThreadDelayed(float time, Action func)
    {
        if (func != null
            && time >= 0.0F)
        {
            var cancel = new CancellationTokenSource();
            lock(cancelTokens)
            {
                cancelTokens.Add(cancel);
            }
            await Task.Run(() => _sleepAndExecute(time, func, cancel));
        }
    }

    private async void _sleepAndExecute(float time, Action func, CancellationTokenSource source)
    {
        try
        {
            int totalTime = (int)(time * 1000);

            while (totalTime > 0)
            {
                if (totalTime > 100)
                {
                    await Task.Delay(100, source.Token);
                    if (!paused)
                    {
                        totalTime -= 100;
                    }
                }
                else
                {
                    await Task.Delay(totalTime, source.Token);
                    if (!paused)
                    {
                        totalTime = 0;
                    }
                }
            }

            // func != null !!
            func();
        }
        catch (TaskCanceledException e)
        {
            // nop
        }
        finally
        {
            if (!cancellingAllThreads)
            {
                lock(cancelTokens)
                {
                    cancelTokens.Remove(source);
                }
            }
        }
    }

    public void PauseThreads(bool state)
    {
        paused = state;
    }
}
