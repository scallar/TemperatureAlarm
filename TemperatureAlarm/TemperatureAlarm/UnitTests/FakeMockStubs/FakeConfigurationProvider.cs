using System;
using System.Collections.Generic;
using FooFramework;

#if DEBUG

namespace TemperatureAlarm
{
  public class FakeConfigurationProvider : ConfigurationProvider
  {
    readonly Dictionary<string,object> configuration;

    static string GetFullPath(Component component, string name)
    {
      return string.Format("/{0}/{1}", component.FullPath, name);
    }
      
    public FakeConfigurationProvider()
    {
      configuration = new Dictionary<string, object>();
    }

    public T GetElement<T>(Component component,string name)
    {
      string path = GetFullPath(component, name);

      if (configuration.ContainsKey(path))
        return (T)configuration[path];
      else
        throw new Exception(String.Format("{0} key is not defined !",path));
    }

    public List<T> GetElements<T>(Component component,string name)
    {
      string path = GetFullPath(component, name);

      if (configuration.ContainsKey(path))
        return (List<T>)configuration[path];
      else
        throw new Exception(String.Format("{0} key is not defined !",path));
    }

    public void SetElement(string path, object value)
    {
      configuration[path] = value;
    }
  }
}

#endif