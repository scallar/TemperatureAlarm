namespace FooFramework
{
  public abstract class Port
  {
    protected string name;
    protected Component parent;

    public Port(string name, Component parent)
    {
      this.name = name;
      this.parent = parent;

      parent.Ports.Add(this);
    }

    ~Port()
    {
      if (parent != null && parent.Ports.Contains(this))
        parent.Ports.Remove(this);
    }

    public abstract void Connect(Port other);

    public string Name
    {
      get { return name; }
    }
  }
}
