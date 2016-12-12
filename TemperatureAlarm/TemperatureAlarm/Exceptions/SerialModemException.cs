using System;
using System.Text;

namespace TemperatureAlarm
{
  public enum SerialModemExceptionType
  {
    PortFail, InitFail, SmsSendFail, SmsRcvFail
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
          case SerialModemExceptionType.InitFail:
            res.AppendLine("Modem initalization failed !");
            break;
          case SerialModemExceptionType.PortFail:
            res.AppendLine("Port initialization failed !");
            break;
          case SerialModemExceptionType.SmsRcvFail:
            res.AppendLine("Sms Receiving failed !");
            break;
          case SerialModemExceptionType.SmsSendFail:
            res.AppendLine("Sms Sending failed !");
            break;
        }
        res.AppendLine("Additional info:");
        res.AppendLine(additionalInfo);
        return res.ToString();
      }
    }
  }
}

