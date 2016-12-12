using System;
using System.Collections.Generic;

namespace FooFramework
{
  public abstract class OutPort<T> : Port
  {
    protected List<InPort<T>> inPorts;

    protected OutPort(string name, Component parent ) : base(name,parent)
    {
      inPorts = new List<InPort<T>>();
    }

    public void Connect(InPort<T> inPort)
    {
      if (inPort == null)
        throw new Exception("Tried to connect null port");
      if (inPorts.Contains(inPort))
        throw new Exception("Port already connected");
      inPorts.Add(inPort);
    }

    public override void Connect(Port other)
    {
      if (other is InPort<T>)
        Connect((InPort<T>)other);
      else
        throw new Exception("Wrong port type");
    }

    public void Disconnect(InPort<T> inPort)
    {
      if (!inPorts.Contains(inPort))
        throw new Exception("No such subscriber");
      inPorts.Remove(inPort);
    }

    public abstract void PutData(T data);
  }
}

