using System;

namespace TemperatureAlarm
{
  public struct TempData
  {

    public float[] Temperature
    {
      get; set;
    }

    public DateTime Timestamp
    {
      get; set;
    }

    public override string ToString()
    {
      return string.Format("[TempData: Temperature={0}, Timestamp={1}]", string.Join(",", Temperature), Timestamp);
    }
  }
}

