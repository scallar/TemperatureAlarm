namespace TemperatureAlarm
{
  public struct SmsMessage
  {

    public string Number
    {
      get; set;
    }

    public string Text
    {
      get; set;
    }

    public override string ToString()
    {
      return string.Format("[SmsMessage: Number={0}, Text={1}]", Number, Text);
    }
  }
}

