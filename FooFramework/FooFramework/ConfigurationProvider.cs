using System.Collections.Generic;

namespace FooFramework
{
  public interface ConfigurationProvider
  {
    T GetElement<T>(Component component, string path);
    List<T> GetElements<T>(Component component, string path);
  }
}

