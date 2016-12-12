using System;
using System.Reflection;
using FooFramework;

namespace TemperatureAlarm
{
  class Program
  {
    const string DEFAULT_CONF = "Settings.xml";

    public static void Main (string[] args)
    {
      string confFile;
      switch (args.Length)
      {
        case 1:
          confFile = args[0];
          break;
        case 0:
          confFile = DEFAULT_CONF;
          break;
        default:
          Usage();
          return;
      }
      //FakeTempSensorHandler.SetSensors(2);
      //FakeTempSensorHandler.SetTemperature(0, 10.5f);
      //FakeTempSensorHandler.SetTemperature(1, 10.5f);

      Root root = new Root();
      ConfigurationProvider conf = new XmlConfigurationProvider(confFile);

      root.Configure(conf);
      root.Initialize();
    }

    public static void Usage()
    {
      string cmdPath = Assembly.GetExecutingAssembly().Location;
      Console.WriteLine("USAGE:\n{0} configFile.xml(DEFAULT {1})", cmdPath, DEFAULT_CONF);
    }
  }
}
