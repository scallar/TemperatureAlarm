namespace TemperatureAlarm
{
  public enum TempMeasurerNotificationType
  {
    NoSensors, SensorMalfunction
  }

  public struct TempMeasurerNotification : Notification
  {
    public TempMeasurerNotificationType Type
    {
      get; set;
    }

    public int SensorCount
    {
      get; set;
    }

    public override string ToString()
    {
      return string.Format("[TempMeasurerNotification: Type={0}]", Type);
    }
  }
}

