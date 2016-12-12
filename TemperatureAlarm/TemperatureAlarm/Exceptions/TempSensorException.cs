using System;
using System.Text;

namespace TemperatureAlarm
{
  public enum TempSensorExceptionType
  {
    SensorDead, SensorReadError, Unknown
  }
  public class TempSensorException : Exception
  {
    TempSensorExceptionType type;
    string additionalInfo;

    public TempSensorExceptionType Type
    {
      get {return type;}
    }

    public string AdditionalInfo
    {
      get {return additionalInfo; }
    }

    public TempSensorException (TempSensorExceptionType type, string additionalInfo)
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
          case TempSensorExceptionType.SensorDead:
            res.AppendLine("Sensor died !");
            break;
          case TempSensorExceptionType.SensorReadError:
            res.AppendLine("Sensor readout failed !");
            break;
          case TempSensorExceptionType.Unknown:
            res.AppendLine("Unknown sensor error !");
            break;
        }
        res.AppendLine("Additional info:");
        res.AppendLine(additionalInfo);
        return res.ToString();
      }
    }
  }
}

