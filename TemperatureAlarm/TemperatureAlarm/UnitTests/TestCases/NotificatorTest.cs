using NUnit.Framework;
using System;
using System.Collections.Generic;
using FooFramework;

namespace TemperatureAlarm
{
  [TestFixture]
  public class NotificatorTest
  {
    Notificator notificator;
    FakeConfigurationProvider confProvider;
    OutPort<Notification> alarmPort;
    InPort<CellCommand> cmdPort;
    OutPort<TempData> tempPort;
    OutPort<SmsMessage> smsPort;

    List<CellCommand> outData;
    ComponentStub compStub;

    [SetUp]
    public void Configure()
    {
      notificator = new Notificator();
      confProvider = new FakeConfigurationProvider();
      outData = new List<CellCommand>();
      compStub = new ComponentStub();

      alarmPort = new StdOutPort<Notification>("AlarmPort", compStub);
      tempPort = new StdOutPort<TempData>("TempPort", compStub);
      cmdPort = new StdInPort<CellCommand>("CmdPort", compStub, outData.Add);
      smsPort = new StdOutPort<SmsMessage>("SmsPort", compStub);

      alarmPort.Connect(notificator.NotificationPort);
      cmdPort.Connect(notificator.CommandPort);
      tempPort.Connect(notificator.TempPort);
      smsPort.Connect(notificator.SmsPort);

      confProvider.SetElement("/Notificator/TempNotOkMsg", "Temp NOK {0}");
      confProvider.SetElement("/Notificator/TempOkMsg", "Temp OK {0}");
      confProvider.SetElement("/Notificator/AckCmd", "ok");
      confProvider.SetElement("/Notificator/StatusReqCmd", "status");
      confProvider.SetElement("/Notificator/StatusMsg", "alarm on: {0} ,tmp: {1}");
      confProvider.SetElement("/Notificator/DisableAlarmCmd", "off");
      confProvider.SetElement("/Notificator/EnableAlarmCmd", "on");
      confProvider.SetElement("/Notificator/Numbers/Number", new List<string>(new string[]{"123","456"}));
      confProvider.SetElement("/Notificator/DialDuration", 100);
      confProvider.SetElement("/Notificator/NextDialInterval", 100);
      confProvider.SetElement("/Notificator/SensorMalfMsg", "sensor malfunction");
      confProvider.SetElement("/Notificator/StatisticsMsg", "alarm {0}, wrong temp {1}, lowest {2}, highest {3}");

      notificator.Configure(confProvider);
      notificator.Initialize();
    }

    [Test]
    public void AlarmIsOn()
    {
      AlarmNotification ad = new AlarmNotification();
      TempData td = new TempData();
      td.Temperature = new float[]{ 1f, 2f, 3f };
      td.Timestamp = DateTime.Now;
      ad.Data = td;
      ad.Type = AlarmNotificationType.NotOK;

      alarmPort.PutData(ad);

      System.Threading.Thread.Sleep(350);

      Assert.AreEqual(outData.Count, 5);
      foreach (var d in outData)
        Console.WriteLine(d);
    }

    [Test]
    public void AlarmIsOff()
    {
      AlarmNotification ad = new AlarmNotification();
      TempData td = new TempData();
      td.Temperature = new float[]{ 1f, 2f, 3f };
      td.Timestamp = DateTime.Now;
      ad.Data = td;
      ad.Type = AlarmNotificationType.NotOK;

      alarmPort.PutData(ad);

      System.Threading.Thread.Sleep(150);

      ad = new AlarmNotification();
      td = new TempData();
      td.Temperature = new float[]{ 0f, 0f, 0f };
      td.Timestamp = DateTime.Now;
      ad.Data = td;
      ad.Type = AlarmNotificationType.OK;

      alarmPort.PutData(ad);

      System.Threading.Thread.Sleep(150);

      Assert.AreEqual(outData.Count, 5);
      foreach (var d in outData)
        Console.WriteLine(d);
    }

    [Test]
    public void StatusRequested()
    {
      TempData data = new TempData();
      data.Temperature = new float[]{ 1f, 2f };
      data.Timestamp = DateTime.Now;
      tempPort.PutData(data);

      SmsMessage msg = new SmsMessage();
      msg.Number = "123456";
      msg.Text = "status";
      smsPort.PutData(msg);

      System.Threading.Thread.Sleep(150);

      Assert.AreEqual(outData.Count, 1);
      foreach (var d in outData)
        Console.WriteLine(d);
    }

    [Test]
    public void AlarmAcknowledged()
    {
      AlarmNotification ad = new AlarmNotification();
      TempData td = new TempData();
      td.Temperature = new float[]{ 1f, 2f, 3f };
      td.Timestamp = DateTime.Now;
      ad.Data = td;
      ad.Type = AlarmNotificationType.NotOK;

      alarmPort.PutData(ad);

      System.Threading.Thread.Sleep(150);

      SmsMessage msg = new SmsMessage();
      msg.Number = "123456";
      msg.Text = "ok";

      smsPort.PutData(msg);

      System.Threading.Thread.Sleep(200);

      Assert.AreEqual(outData.Count, 3);
      foreach (var d in outData)
        Console.WriteLine(d);
    }

    [Test]
    public void AlarmingDisabledTempNok()
    {
      SmsMessage msg = new SmsMessage();
      msg.Number = "123456";
      msg.Text = "off";

      smsPort.PutData(msg);

      AlarmNotification ad = new AlarmNotification();
      TempData td = new TempData();
      td.Temperature = new float[]{ 1f, 2f, 3f };
      td.Timestamp = DateTime.Now;
      ad.Data = td;
      ad.Type = AlarmNotificationType.NotOK;

      alarmPort.PutData(ad);

      System.Threading.Thread.Sleep(350);

      Assert.AreEqual(outData.Count, 0);
    }

    [Test]
    public void AlarmingDisabledDuringTempNok()
    {
      AlarmNotification ad = new AlarmNotification();
      TempData td = new TempData();
      td.Temperature = new float[]{ 1f, 2f, 3f };
      td.Timestamp = DateTime.Now;
      ad.Data = td;
      ad.Type = AlarmNotificationType.NotOK;

      alarmPort.PutData(ad);

      System.Threading.Thread.Sleep(150);

      SmsMessage msg = new SmsMessage();
      msg.Number = "123456";
      msg.Text = "off";

      smsPort.PutData(msg);

      System.Threading.Thread.Sleep(250);

      Assert.AreEqual(outData.Count, 3);
      foreach (var d in outData)
        Console.WriteLine(d);
    }

    [Test]
    public void AlarmingReenabledTempNok()
    {
      TempData data = new TempData();
      data.Temperature = new float[]{ 1f, 2f };
      data.Timestamp = DateTime.Now;
      tempPort.PutData(data);

      SmsMessage msg = new SmsMessage();
      msg.Number = "123456";
      msg.Text = "off";

      smsPort.PutData(msg);

      AlarmNotification ad = new AlarmNotification();
      TempData td = new TempData();
      td.Temperature = new float[]{ 1f, 2f, 3f };
      td.Timestamp = DateTime.Now;
      ad.Data = td;
      ad.Type = AlarmNotificationType.NotOK;

      alarmPort.PutData(ad);

      System.Threading.Thread.Sleep(150);

      Assert.AreEqual(outData.Count, 0);

      msg = new SmsMessage();
      msg.Number = "123456";
      msg.Text = "on";

      smsPort.PutData(msg);

      System.Threading.Thread.Sleep(150);

      Assert.AreEqual(outData.Count, 3);
      foreach (var d in outData)
        Console.WriteLine(d);
    }

  }
}

