using System;

namespace FooFramework
{
  public abstract class InPort<T> : Port
  {
    public delegate void DataHandler(T data);
    protected DataHandler handler;

    protected InPort(string name, Component parent, DataHandler handler ) : base(name,parent)
    {
      this.handler = handler;
    }

    public void Connect(OutPort<T> outPort)
    {
      if (outPort == null)
        throw new Exception("Tried to connect null port");
      outPort.Connect(this);
    }

    public override void Connect(Port other)
    {
      if (other is OutPort<T>)
        Connect((OutPort<T>)other);
      else
        throw new Exception("Wrong port type");
    }

    public void Disconnect(OutPort<T> outPort)
    {
      if (outPort == null)
        throw new Exception("Tried to disconnect null port");
      outPort.Disconnect(this);
    }

    public abstract void PutData(T data);
  }
}

