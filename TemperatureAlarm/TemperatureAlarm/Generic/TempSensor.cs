using System.Collections.Generic;

namespace TemperatureAlarm
{
  public interface TempSensor
  {
    float GetTemperature();
  }

  public abstract class TempSensorDetector
  {
    public abstract List<TempSensor> DetectSensors();
  }
}

