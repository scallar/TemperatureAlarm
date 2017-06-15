using System;
using System.Reflection;
using FooFramework;
using Mono.Unix;
using System.Threading;

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
      //FakeTempSensorHandler.SetTemperature(0, 5.0f);
      //FakeTempSensorHandler.SetTemperature(1, 5.5f);

      Root root = new Root();
      ConfigurationProvider conf = new XmlConfigurationProvider(confFile);
	    UnixSignal[] signals = new UnixSignal[] 
	    {
	      new UnixSignal(Mono.Unix.Native.Signum.SIGABRT),
		    new UnixSignal(Mono.Unix.Native.Signum.SIGTERM),
	    	new UnixSignal(Mono.Unix.Native.Signum.SIGINT),
	    	new UnixSignal(Mono.Unix.Native.Signum.SIGUSR1),
	    	new UnixSignal(Mono.Unix.Native.Signum.SIGQUIT)
	    };

      root.Configure(conf);
      root.Initialize();

      /*Thread.Sleep(5000);
      Notificator noti = (Notificator)root.GetChildByName("Notificator");
      SmsMessage msg = new SmsMessage();
      msg.Number = "+48503601714";
      msg.Text = "Status";
      noti.SmsPort.PutData(msg);*/

	    UnixSignal.WaitAny (signals);
	    root.Dispose();
	    Environment.Exit(0);
    }

    public static void Usage()
    {
      string cmdPath = Assembly.GetExecutingAssembly().Location;
      Console.WriteLine("USAGE:\n{0} configFile.xml(DEFAULT {1})", cmdPath, DEFAULT_CONF);
    }
  }
}
