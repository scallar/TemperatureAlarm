using NUnit.Framework;
using System.Collections.Generic;
using System;
using FooFramework;

namespace TemperatureAlarm
{
  [TestFixture]
  public class StatCollectorTest
  {
    StatCollector statCollector;
    FakeConfigurationProvider confProvider;
    List<Notification> outData;
    ComponentStub compStub;

    OutPort<StatCollectorData> dataPort;
    OutPort<TempData> tempPort;
    InPort<Notification> notificationPort;

    [SetUp]
    public void Configure()
    {
      statCollector = new StatCollector();
      confProvider = new FakeConfigurationProvider();
      compStub = new ComponentStub();
      outData = new List<Notification>();

      dataPort = new StdOutPort<StatCollectorData>("DataPort", compStub);
      tempPort = new StdOutPort<TempData>("TempPort", compStub);
      notificationPort = new StdInPort<Notification>("NotificationPort", compStub, outData.Add);

      dataPort.Connect(statCollector.DataPort);
      tempPort.Connect(statCollector.TempPort);
      notificationPort.Connect(statCollector.NotificationPort);

      confProvider.SetElement("/StatCollector/PublishPeriod", -1d);
    }

    [Test]
    public void NoData()
    {
      statCollector.Configure(confProvider);
      statCollector.Initialize();

      TimeSpan zeroTime = new TimeSpan(0, 0, 0);

      System.Threading.Thread.Sleep(205);
      Assert.AreEqual(outData.Count, 2);
      Assert.AreEqual(outData[0], outData[1]);
      Assert.AreEqual(((StatCollectorNotification)outData[0]).AlarmTempSpan, zeroTime);
      Assert.AreEqual(((StatCollectorNotification)outData[0]).WrongTempSpan, zeroTime);
      Assert.AreEqual(((StatCollectorNotification)outData[0]).HighestTemp, float.MinValue);
      Assert.AreEqual(((StatCollectorNotification)outData[0]).LowestTemp, float.MaxValue);

      Console.WriteLine(outData[0]);
      Console.WriteLine(outData[1]);
    }

    [Test]
    public void AlarmData()
    {
      statCollector.Configure(confProvider);
      statCollector.Initialize();

      TempData tempData = new TempData();
      TimeSpan oneSecond = new TimeSpan(0, 0, 1);
      TimeSpan twoSeconds = new TimeSpan(0, 0, 2);

      tempData.Temperature = new float[] {-1f, 1f, 2f};
      tempData.Timestamp = DateTime.Now;
      tempPort.PutData(tempData);

      tempData.Temperature = new float[] {-3f, -3f, -2f};
      tempData.Timestamp = DateTime.Now;
      tempPort.PutData(tempData);

      tempData.Temperature = new float[] {6f, 6f,-6f};
      tempData.Timestamp = DateTime.Now;
      tempPort.PutData(tempData);

      tempData.Temperature = new float[] {-1f, 0f,-1f};
      tempData.Timestamp = DateTime.Now;
      tempPort.PutData(tempData);

      AlarmStatData alarmData = new AlarmStatData();
      alarmData.Type = AlarmStatData.AlarmStatDataType.TempNotOk;
      alarmData.Span = twoSeconds;
      dataPort.PutData(alarmData);

      alarmData = new AlarmStatData();
      alarmData.Type = AlarmStatData.AlarmStatDataType.AlarmOn;
      alarmData.Span = oneSecond;
      dataPort.PutData(alarmData);

      System.Threading.Thread.Sleep(205);

      TimeSpan zeroTime = new TimeSpan(0, 0, 0);


      Assert.AreEqual(outData.Count, 2);

      //alarm time - 2s
      //wrong temp time - 1s
      //lowest - -1
      //highest - 6

      Assert.AreEqual(((StatCollectorNotification)outData[0]).AlarmTempSpan, oneSecond);
      Assert.AreEqual(((StatCollectorNotification)outData[0]).WrongTempSpan, twoSeconds);
      Assert.AreEqual(((StatCollectorNotification)outData[0]).HighestTemp, 6f);
      Assert.AreEqual(((StatCollectorNotification)outData[0]).LowestTemp, -6f);

      //should reset afterwards

      Assert.AreEqual(((StatCollectorNotification)outData[1]).AlarmTempSpan, zeroTime);
      Assert.AreEqual(((StatCollectorNotification)outData[1]).WrongTempSpan, zeroTime);
      Assert.AreEqual(((StatCollectorNotification)outData[1]).HighestTemp, float.MinValue);
      Assert.AreEqual(((StatCollectorNotification)outData[1]).LowestTemp, float.MaxValue);

      Console.WriteLine(outData[0]);
      Console.WriteLine(outData[1]);
    }

  }
}

