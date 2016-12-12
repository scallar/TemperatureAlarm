using System;

namespace TemperatureAlarm
{
  public struct StatCollectorNotification : Notification
  {
    public float LowestTemp
    { get; 
      set;
    }

    public float HighestTemp
    {
      get;
      set;
    }

    public TimeSpan WrongTempSpan
    {
      get;
      set;
    }
    public TimeSpan AlarmTempSpan
    {
      get;
      set;
    }

    public override string ToString()
    {
      return string.Format("[StatCollectorNotification: LowestTemp={0}, HighestTemp={1}, WrongTempSpan={2}, AlarmTempSpan={3}]", LowestTemp, HighestTemp, WrongTempSpan, AlarmTempSpan);
    }
  }
}

