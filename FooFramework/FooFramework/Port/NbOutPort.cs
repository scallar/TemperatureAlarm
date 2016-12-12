using System.Threading;
using System.Collections.Concurrent;

namespace FooFramework
{
  public class NbOutPort<T> : OutPort<T>
  {
    readonly ConcurrentQueue<T> dataQueue;
    readonly ManualResetEvent resetEvent;

    Thread portThread;

    public NbOutPort(string name, Component parent) : base (name, parent)
    {
      dataQueue = new ConcurrentQueue<T>();
      resetEvent = new ManualResetEvent(false);
      portThread = new Thread(HandleData);
      portThread.Start();
    }

    public void HandleData()
    {
      while (true)
      {
        resetEvent.WaitOne();
        T data;
        bool dataAvailable = dataQueue.TryDequeue(out data);
        if (dataAvailable)
          foreach (InPort<T> inPort in inPorts)
            inPort.PutData(data);
        if (dataQueue.IsEmpty)
          resetEvent.Reset();
      }
    }


    public override void PutData(T data)
    {
      dataQueue.Enqueue(data);
      resetEvent.Set();
    }
  }
}

