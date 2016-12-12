namespace TemperatureAlarm
{
  public interface CellCommand
  {
  }

  public struct SendSmsCommand : CellCommand
  {
    public string Number
    {
      get; set;
    }

    public string Text
    {
      get; set;
    }

    public SendSmsCommand (string number, string text) : this ()
    {
      Number = number;
      Text = text;
    }

    public override string ToString()
    {
      return string.Format("[SendSmsCommand: Number={0}, Text={1}]", Number, Text);
    }
  }


  public struct DialCommand : CellCommand
  {
    public string Number
    {
      get; set;
    }
    public int Duration
    {
      get; set;
    }

    public DialCommand(string number, int duration) : this()
    {
      Number = number;
      Duration = duration;
    }

    public override string ToString()
    {
      return string.Format("[DialCommand: Number={0}, Duration={1}]", Number, Duration);
    }
  }
}

