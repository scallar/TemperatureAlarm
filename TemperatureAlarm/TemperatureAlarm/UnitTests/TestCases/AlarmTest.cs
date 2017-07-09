using NUnit.Framework;
using System;
using System.Collections.Generic;
using FooFramework;

namespace TemperatureAlarm
{
  [TestFixture()]
  public class AlarmTest
  {
    Alarm alarm;
    FakeConfigurationProvider confProvider;
    OutPort<TempData> alarmInPort;
    InPort<Notification> alarmOutPort;
    List<Notification> outData;
    ComponentStub compStub;

    [SetUp]
    public void Configure()
    {
      confProvider = new FakeConfigurationProvider();
      alarm = new Alarm();
      compStub = new ComponentStub();
      outData = new List<Notification>();

      alarmInPort = new StdOutPort<TempData>("AlarmInPort", compStub);
      alarmOutPort = new StdInPort<Notification>("AlarmOutPort", compStub, outData.Add);

      alarm.LoggingLevel = LogLevel.Low;
      alarm.NotificationPort.Connect(alarmOutPort);
      alarm.TempPort.Connect(alarmInPort);
    }

    [Test]
    public void DoNotTriggerAlarm()
    {
      confProvider.SetElement("/Alarm/MaxTemp", 10f);
      confProvider.SetElement("/Alarm/MinTemp", -1f);
      confProvider.SetElement("/Alarm/Histeresis/Hour", 0);
      confProvider.SetElement("/Alarm/Histeresis/Minute", 0);
      confProvider.SetElement("/Alarm/Histeresis/Second", 4);

      alarm.Configure(confProvider);
      alarm.Initialize();

      float[] temps = {1f,2.34f,3f,4f,5f,5f,5f,100f,100f,100f,100f,2f,2f };
      TimeSpan span = new TimeSpan(0, 0, 1);
      DateTime dt = DateTime.MinValue;
      foreach (float temp in temps)
      {
        TempData data = new TempData();
        data.Temperature = new float[]{temp};
        data.Timestamp = dt;
        dt += span;
        alarmInPort.PutData(data);
      }
          
      System.Threading.Thread.Sleep(100);

      Assert.AreEqual(outData.Count, 0);
    }

    [Test]
    public void TriggerAlarm()
    {
      confProvider.SetElement("/Alarm/MaxTemp", 10f);
      confProvider.SetElement("/Alarm/MinTemp", -1f);
      confProvider.SetElement("/Alarm/Histeresis/Hour", 0);
      confProvider.SetElement("/Alarm/Histeresis/Minute", 0);
      confProvider.SetElement("/Alarm/Histeresis/Second", 3);

      alarm.Configure(confProvider);
      alarm.Initialize();

      float[] temps = {1f,2.34f,3f,40f,50f,50f,50f,100f,100f,100f,100f };
      TimeSpan span = new TimeSpan(0, 0, 1);
      DateTime dt = DateTime.MinValue;
      foreach (float temp in temps)
        {
          TempData data = new TempData();
          data.Temperature = new float[]{temp};
          data.Timestamp = dt;
          dt += span;
          alarmInPort.PutData(data);
        }

      System.Threading.Thread.Sleep(100);

      Assert.AreEqual(outData.Count, 1);
      Assert.AreEqual(((AlarmNotification)outData[0]).Type, AlarmNotificationType.NotOK);
    }

    [Test]
    public void TriggerAndDisableAlarm()
    {
      confProvider.SetElement("/Alarm/MaxTemp", 10f);
      confProvider.SetElement("/Alarm/MinTemp", -1f);
      confProvider.SetElement("/Alarm/Histeresis/Hour", 0);
      confProvider.SetElement("/Alarm/Histeresis/Minute", 0);
      confProvider.SetElement("/Alarm/Histeresis/Second", 3);

      alarm.Configure(confProvider);
      alarm.Initialize();

      float[] temps = {1f,2.34f,3f,40f,50f,50f,50f,1f,1f,1f,1f };
      TimeSpan span = new TimeSpan(0, 0, 1);
      DateTime dt = DateTime.MinValue;
      foreach (float temp in temps)
        {
          TempData data = new TempData();
          data.Temperature = new float[]{temp};
          data.Timestamp = dt;
          dt += span;
          alarmInPort.PutData(data);
        }

      System.Threading.Thread.Sleep(100);

      Assert.AreEqual(outData.Count, 2);;
      Assert.AreEqual(((AlarmNotification)outData[0]).Type, AlarmNotificationType.NotOK);
      Assert.AreEqual(((AlarmNotification)outData[1]).Type, AlarmNotificationType.OK);
    }
  }
}

