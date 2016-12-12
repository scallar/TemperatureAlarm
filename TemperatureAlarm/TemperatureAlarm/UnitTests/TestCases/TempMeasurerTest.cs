using NUnit.Framework;
using System;
using System.Collections.Generic;
using FooFramework;

namespace TemperatureAlarm
{
  [TestFixture()]
  public class TempMeasurerTest
  {
    TempMeasurer measurer;
    FakeConfigurationProvider confProvider;
    InPort<TempData> tempPort;

    List<TempData> outData;
    ComponentStub compStub;

    [SetUp]
    public void Configure()
    {
      measurer = new TempMeasurer();
      confProvider = new FakeConfigurationProvider();
      compStub = new ComponentStub();
      outData = new List<TempData>();
      tempPort = new StdInPort<TempData>("TempPort", compStub, outData.Add);

      tempPort.Connect(measurer.TempPort);
    }

    [Test]
    public void ReadFrom1Sensor()
    {
      float temp = 5f;
      var count = 3;
      int measurementPeriod = 100;
      FakeTempSensorHandler.SetSensors(1);
      FakeTempSensorHandler.SetTemperature(0, temp);
      confProvider.SetElement("/TempMeasurer/MeasurementPeriod", measurementPeriod);
      measurer.Configure(confProvider);
      measurer.Initialize();

      System.Threading.Thread.Sleep(count * measurementPeriod + 20);
      measurer.TempPort.Disconnect(tempPort);

      Assert.AreEqual(outData.Count, count);
      for (int i = 0; i < count; i++)
      {
        Assert.AreEqual(outData[i].Temperature.Length, 1);
        Assert.AreEqual(outData[i].Temperature[0], temp);
        Console.WriteLine(outData[i]);
      }
    }

    [Test]
    public void ReadFrom3Sensors()
    {
      int sensors = 3;
      float[] temp = {1f,2f,3f};
      int count = 3;
      int measurementPeriod = 100;
      FakeTempSensorHandler.SetSensors(3);
      for (int i = 0; i < sensors; i++)
        FakeTempSensorHandler.SetTemperature(i, temp[i]);
      confProvider.SetElement("/TempMeasurer/MeasurementPeriod", measurementPeriod);
      measurer.Configure(confProvider);
      measurer.Initialize();

      System.Threading.Thread.Sleep(count * measurementPeriod + 20);
      measurer.TempPort.Disconnect(tempPort);

      Assert.AreEqual(outData.Count, count);
      for (int i = 0; i < count; i++)
      {
          Assert.AreEqual(outData[i].Temperature.Length, sensors);
          Assert.AreEqual(outData[i].Temperature, temp);
          Console.WriteLine(outData[i]);
      }
    }
  }
}

