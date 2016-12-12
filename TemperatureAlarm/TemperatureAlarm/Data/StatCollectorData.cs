using System;

namespace TemperatureAlarm
{
  public interface StatCollectorData
  {
  }

  public class AlarmStatData : StatCollectorData
  {
    public enum AlarmStatDataType
    {
      TempNotOk, AlarmOn
    }

    public AlarmStatDataType Type
    {
      get; set;
    }

    public TimeSpan Span
    {
      get; set;
    }

    public override string ToString()
    {
      return string.Format("[AlarmStatData: Type={0}, Span={1}]", Type, Span);
    }
  }
}

