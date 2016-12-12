using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FooFramework
{
  class ComponentManager : Component
  {


    public ComponentManager(string name, Component parent, LogLevel loggingLevel = LogLevel.Medium) : base(name, parent, loggingLevel)
    {

    }

    Component GetComponentByPath (string path)
    {
      Component res = Parent;

      foreach (string name in path.Substring(1).Split('/'))
        res = res.GetChildByName(name);

      return res;
    }

    Port GetPortByPath(string path)
    {
      Component comp = Parent;
      Port res = null;
      string[] splitted = path.Substring(1).Split('/');

      for (int i = 0; i < splitted.Length - 1 ; i++)
        comp = comp.GetChildByName(splitted[i]);

      res = comp.GetPortByName(splitted[splitted.Length - 1]);

      return res;
    }

  }
}
