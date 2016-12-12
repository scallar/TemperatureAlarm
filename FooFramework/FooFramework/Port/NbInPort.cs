using System.Threading;
using System.Collections.Concurrent;

namespace FooFramework
{
  public class NbInPort<T> : InPort<T>
  {
    readonly ConcurrentQueue<T> dataQueue;
    readonly ManualResetEvent resetEvent;

    Thread portThread;

    public NbInPort(string name, Component parent, DataHandler handler) : base (name, parent, handler)
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
          lock (parent)
            handler(data);
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

