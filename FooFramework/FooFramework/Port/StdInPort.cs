namespace FooFramework
{
  public class StdInPort<T> : InPort<T>
  {
    public StdInPort(string name, Component parent, DataHandler handler) : base(name,parent, handler)
    {
    }
    public override void PutData(T data)
    {
      lock (parent)
        handler(data);
    }
  }
}

