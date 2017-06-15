using System;
using System.Text;

namespace TemperatureAlarm
{
  public enum SerialModemExceptionType
  {
    PortFail, NotOk
  }
  public class SerialModemException : Exception
  {
    SerialModemExceptionType type;
    readonly string additionalInfo;

    public SerialModemExceptionType Type
    {
      get { return type; }
    }

    public string AdditionalInfo
    {
      get { return additionalInfo; }
    }

    public SerialModemException (SerialModemExceptionType type, string additionalInfo)
    {
      this.type = type;
      this.additionalInfo = additionalInfo;
    }

    public override string Message
    {
      get
      {
        StringBuilder res = new StringBuilder();
        switch (type)
        {
          case SerialModemExceptionType.NotOk:
            res.AppendLine("AT command not confirmed!");
            break;
          case SerialModemExceptionType.PortFail:
            res.AppendLine("Port initialization failed !");
            break;
        }
        res.AppendLine("Additional info:");
        res.AppendLine(additionalInfo);
        res.AppendLine("(Additional info end)");
        return res.ToString();
      }
    }
  }
}

