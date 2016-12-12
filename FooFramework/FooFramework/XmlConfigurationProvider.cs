using System;
using System.Collections.Generic;
using System.Xml.XPath;

namespace FooFramework
{
  public class XmlConfigurationProvider : ConfigurationProvider
  {
    XPathNavigator navigator;

    protected string GetFullPath(Component component, string name)
    {
      return string.Format("/{0}/{1}", component.FullPath, name);
    }

    public XmlConfigurationProvider(string path = "")
    {
      if (path != "")
        LoadFile(path);
    }

    public void LoadFile(string path)
    {
      XPathDocument doc = new XPathDocument(path);
      navigator = doc.CreateNavigator();
    }

    public T GetElement<T>(Component component, string path)
    {
      if (navigator == null)
        throw new Exception("Configuration not loaded.");

      string fullPath = GetFullPath(component, path);
      XPathExpression expr = navigator.Compile(fullPath);
      XPathNodeIterator iterator = navigator.Select(expr);
      iterator.MoveNext ();
      return (T)Convert.ChangeType(iterator.Current.ValueAs(typeof(T)),typeof(T));
    }

    public List<T> GetElements<T>(Component component, string path)
    {
      string fullPath = GetFullPath(component, path);
      XPathExpression expr = navigator.Compile(fullPath);
      XPathNodeIterator iterator = navigator.Select(expr);
      List<T> res = new List<T>();
      while (iterator.MoveNext ()) 
        res.Add ((T)Convert.ChangeType (iterator.Current.ValueAs (typeof(T)), typeof(T)));
      return res;
    }
  }
}

