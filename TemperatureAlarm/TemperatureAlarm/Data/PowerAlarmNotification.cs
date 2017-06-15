namespace TemperatureAlarm
{
  public enum PowerAlarmNotificationType
  {
    PowerOn, PowerOff
  }

  public struct PowerAlarmNotification : Notification
  {

    public PowerAlarmNotificationType Type
    {
      get; set;
    }

    public override string ToString ()
    {
      return string.Format ("[PowerAlarmNotification: Type={0}]", Type);
    }
  }
}

