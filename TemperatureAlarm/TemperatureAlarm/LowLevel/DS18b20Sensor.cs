using System;
using System.Collections.Generic;
using System.IO;

namespace TemperatureAlarm
{
  public class DS18b20Sensor : TempSensor
  {
    readonly Stream stream;
    readonly StreamReader reader;
    string txtBuf;

    public DS18b20Sensor (Stream stream)
    {
      this.stream = stream;
      reader = new StreamReader(stream);
    }

    public float GetTemperature()
    {
      stream.Position = 0;
      reader.DiscardBufferedData();
      try
      {
        txtBuf = reader.ReadToEnd();
      }
      catch (Exception e)
      {
        throw new TempSensorException (TempSensorExceptionType.SensorDead, e.Message);
      }
      if (!txtBuf.Contains("YES"))
        throw new TempSensorException(TempSensorExceptionType.SensorReadError, txtBuf);
      string temp = txtBuf.Substring (txtBuf.IndexOf ("t=") + 2);
      float res = float.Parse (temp) / 1000f;
      return res;
    }
  }

  public class DS18b20SensorDetector : TempSensorDetector
  {
    public override List<TempSensor> DetectSensors()
    {
      List<TempSensor> res = new List<TempSensor>();

      if (!Directory.Exists("/sys/bus/w1/devices/"))
        return res;
      string[] dirs = Directory.GetDirectories("/sys/bus/w1/devices/","28-*");
      foreach (string dir in dirs)
        res.Add(new DS18b20Sensor(new FileStream(dir + "/w1_slave", FileMode.Open, FileAccess.Read)));

      return res;
    }
  }
}

