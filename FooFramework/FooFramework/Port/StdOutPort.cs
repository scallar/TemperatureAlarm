namespace FooFramework
{
  public class StdOutPort<T> : OutPort<T>
  {
    public StdOutPort (string name,Component parent) : base (name,parent)
    {
      this.parent = parent;
    }

    public override void PutData(T data)
    {
      foreach (InPort<T> inPort in inPorts)
        inPort.PutData(data);
    }
  }
}

