using System.Collections.Generic;
using System;

namespace FooFramework
{
  public abstract class Component : IDisposable
  {
    string name;
    Component parent;
    Logger logger;
    readonly List<Component> children;
    readonly List<Port> ports;
    LogLevel loggingLevel;

    protected Component (string name, Component parent, LogLevel loggingLevel = LogLevel.Medium)
    {
      this.name = name;
      this.parent = parent;
      this.loggingLevel = loggingLevel;

      logger = Logger.GetInstance();
      children = new List<Component>();
      ports = new List<Port>();

      if (parent != null)
        parent.children.Add(this);
    }

    ~Component()
    {
      if (parent != null && parent.children.Contains(this))
        parent.children.Remove(this);
    }

    public virtual void Initialize()
    {
      Log("Initializing component", LogLevel.Medium);
      foreach (Component child in children)
        child.Initialize();
    }

    public Component GetChildByName (string name)
    {
      foreach (Component c in children)
        if (c.Name == name)
          return c;

      throw new Exception("Child not found !!!");
    }

    public Port GetPortByName (string name)
    {
      foreach (Port p in ports)
        if (p.Name == name)
          return p;

      throw new Exception("Port not found !!!");
    }

    public virtual void Configure(ConfigurationProvider cp)
    {
      Log("Configuring component", LogLevel.Medium);
      foreach (Component child in children)
        child.Configure(cp);    
    }

	public virtual void Dispose()
	{
	  Log("Removing component", LogLevel.Medium);
	  foreach (Component child in children)
	    child.Dispose ();
	}

    public virtual void Log(string message, 
                            LogLevel level = LogLevel.Low)
    {
      if ((int)level >= (int)loggingLevel)
        logger.Log(this, message, level);
    }

    public string FullPath
    {
      get
      {
        Stack<string> hierarchy = new Stack<string>();
        Component c = this;
        do 
        {
          hierarchy.Push(c.Name);
          c = c.Parent;
        } while (c != null);
        return string.Join("/", hierarchy);
      }
    }

    public LogLevel LoggingLevel
    {
      get { return loggingLevel; }
      set { loggingLevel = value; }
    }

    public string Name
    {
      get { return name; }
    }

    public Component Parent
    {
      get { return parent; }
    }

    public List<Component> Children
    {
      get { return children; }
    }

    public List<Port> Ports
    {
      get { return ports; }
    }
  }
}

