using System;
using System.Collections.Generic;
using System.IO;
using FooFramework;

#if DEBUG

namespace TemperatureAlarm
{
  public class FakeTempSensorHandler : TempSensorDetector
  {
    static List<MemoryStream> streams;
    static List<StreamWriter> writers;

    public static void SetSensors(int number)
    {
      streams = new List<MemoryStream>();
      writers = new List<StreamWriter>();

      for (int i = 0; i < number; i++)
      {
        MemoryStream ms = new MemoryStream();
        streams.Add(ms);
        writers.Add(new StreamWriter(ms));
      }
    }

    public static void SetTemperature (int number, float value, bool crcok = true)
    {
      streams[number].Position = 0;
      writers[number].WriteLine("20 01 4b 46 7f ff 10 10 fc : crc=fc {0}",crcok ? "YES" : "NO");
      writers[number].WriteLine("20 01 4b 46 7f ff 10 10 fc t={0}", Convert.ToInt32(value * 1000f));
      writers[number].Flush();
    }

    public FakeTempSensorHandler()
    {
    }

    public override List<TempSensor> DetectSensors()
    {
      List<TempSensor> res = new List<TempSensor>();
      if (streams == null)
        return res;
      foreach (MemoryStream ms in streams)
        res.Add(new DS18b20Sensor(ms));

      return res;
    }
  }
}

#endif