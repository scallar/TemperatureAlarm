using System.Collections.Generic;
using System;

namespace TemperatureAlarm
{
  public interface TempSensor : IDisposable
  {
    float GetTemperature();
  }

  public abstract class TempSensorDetector
  {
    public abstract List<TempSensor> DetectSensors();
  }
}

