using System;
using Raspberry.IO.GeneralPurpose;

namespace TemperatureAlarm
{
  public class PowerMonitor : IDisposable
  {
    IGpioConnectionDriver driver;
    ProcessorPin powerPin;

    public PowerMonitor ()
    {
    }

    public void Initialize()
    {
      ConnectorPin userPin = ConnectorPin.P1Pin29; //gpio5
      //userPin = ConnectorPin.P1Pin32; //gpio12
      powerPin = userPin.ToProcessor();

      driver = new GpioConnectionDriver();
      driver.Allocate(powerPin, PinDirection.Input);      
      driver.SetPinResistor(powerPin, PinResistor.PullDown);
    }

    public bool CheckPower ()
    {
      //return true;
      return !driver.Read(powerPin);
    }

    public void Dispose()
    {
      driver.Release(powerPin);
    }
  }
}

