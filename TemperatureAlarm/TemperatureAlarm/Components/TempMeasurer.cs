using System.Collections.Generic;
using System;
using System.Reflection;
using FooFramework;

namespace TemperatureAlarm
{
  public class TempMeasurer : Component
  {
    readonly OutPort<TempData> tempPort;
    readonly OutPort<Notification> notificationPort;

    readonly List<TempSensor> sensors;
    PeriodicEvent measureEvent;
 
    public TempMeasurer(string name = "TempMeasurer", Component parent = null) : base(name,parent)
    {
      sensors = new List<TempSensor>();
      tempPort = new NbOutPort<TempData>("TempPort", this);
      notificationPort = new StdOutPort<Notification>("NotificationPort", this);
      measureEvent = new PeriodicEvent(this, DoMeasurements);
    }

    public override void Configure(ConfigurationProvider cp)
    {
      base.Configure(cp);
      measureEvent.Interval = cp.GetElement<int>(this, "MeasurementPeriod");
    }
      
    public override void Initialize ()
    {
      base.Initialize();
      DetectSensors();
      measureEvent.Start();
    }

    public override void Dispose()
    {
      base.Dispose();
      measureEvent.Stop();
      foreach (TempSensor sensor in sensors)
        sensor.Dispose();
    }

    void DetectSensors()
    {
      Type[] types = Assembly.GetExecutingAssembly().GetTypes();
      foreach (Type type in types)
      {
        if (type.IsSubclassOf(typeof(TempSensorDetector)))
        {
          TempSensorDetector detector = (TempSensorDetector)type.GetConstructor(Type.EmptyTypes).Invoke(null);
          sensors.AddRange(detector.DetectSensors());
        }
      }
      Log(string.Format("Sensors found: {0}", sensors.Count), 
          sensors.Count > 0 ? LogLevel.Medium : LogLevel.Fatal);
      if (sensors.Count == 0)
        SendSensorFailureNotification(TempMeasurerNotificationType.NoSensors);
    }

    public OutPort<TempData> TempPort
    {
      get { return tempPort; }
    }

    public OutPort<Notification> NotificationPort
    {
      get { return notificationPort; }
    }

    void SendSensorFailureNotification (TempMeasurerNotificationType type)
    {
      TempMeasurerNotification notification = new TempMeasurerNotification();
      notification.Type = type;
      notification.SensorCount = sensors.Count;
      notificationPort.PutData(notification);
    }

    void DoMeasurements ()
    {
      TempData res = new TempData();
      res.Timestamp = DateTime.Now;
      res.Temperature = new float[sensors.Count];
      int i = 0;
      try 
      {
        foreach (TempSensor sensor in sensors)
          res.Temperature[i++] = sensor.GetTemperature();
        tempPort.PutData(res);
      }
      catch (TempSensorException e)
      {
        Log(e.Message);
        if (e.Type == TempSensorExceptionType.SensorDead)
        {
          Log("Sensor malfunction. Detecting sensors again...", LogLevel.Error);
          TempMeasurerNotification notification = new TempMeasurerNotification();
          notification.Type = TempMeasurerNotificationType.SensorMalfunction;
          notificationPort.PutData(notification);
          sensors.Clear();
          DetectSensors();
        }
      }
    }
  }
}

