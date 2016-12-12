using System.Timers;
using System.Threading;
using Timer = System.Timers.Timer;

namespace FooFramework
{
  public class PeriodicEvent
  {
    public delegate void Handler();

    readonly Handler handler;
    readonly Timer timer;
    readonly Component parent;
    readonly ManualResetEvent doneEvent;

    public PeriodicEvent(Component parent, Handler handler, 
                         bool autoReset = true, double interval = 0)
    {
      timer = new Timer();
      if (interval > 0)
        timer.Interval = interval;
      timer.AutoReset = autoReset;
      timer.Elapsed += TimerElapsed;
      doneEvent = new ManualResetEvent(true);

      this.parent = parent;
      this.handler = handler;
    }

    public double Interval
    {
      get { return timer.Interval; }
      set { timer.Interval = value; }
    }

    public void Start()
    {
      timer.Start();
    }

    public void Stop()
    {
      timer.Stop();
    }

    public void Trigger()
    {
      TimerElapsed(null, null);
    }

    void TimerElapsed (object sender, ElapsedEventArgs e)
    {
      if (doneEvent.WaitOne(0))
      {
        doneEvent.Reset();
        lock (parent)
          handler();
        doneEvent.Set();
      }
    }
  }
}

