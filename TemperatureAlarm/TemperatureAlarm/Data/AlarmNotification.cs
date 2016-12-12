namespace TemperatureAlarm
{
  public enum AlarmNotificationType
  {
    OK, NotOK
  }

  public struct AlarmNotification : Notification
  {

    public AlarmNotificationType Type
    {
      get; set;
    }

    public TempData Data
    {
      get; set;
    }

    public override string ToString()
    {
      return string.Format("[AlarmNotification: Type={0}, Data={1}]", Type, Data);
    }
  }
}

