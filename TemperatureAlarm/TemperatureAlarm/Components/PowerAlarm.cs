using FooFramework;

namespace TemperatureAlarm
{
  public class PowerAlarm : Component
  {
    enum PowerAlarmState 
    {
      Idle, Triggered
    }

    readonly OutPort<Notification> notificationPort;

    PeriodicEvent checkPowerEvent;

    readonly PowerMonitor monitor;
    PowerAlarmState state;
    bool powerOn;

    public PowerAlarm (string name = "PowerAlarm", Component parent = null) : base (name, parent)
    {
      notificationPort = new StdOutPort<Notification>("NotificationPort", this);
      checkPowerEvent = new PeriodicEvent(this, CheckPower);
      monitor = new PowerMonitor();
      state = PowerAlarmState.Idle;
      powerOn = true;
    }

    public override void Configure (ConfigurationProvider cp)
    {
      base.Configure (cp);
      checkPowerEvent.Interval = cp.GetElement<int>(this, "MeasurementPeriod");
    }

    public override void Initialize ()
    {
      base.Initialize();
      monitor.Initialize();
      checkPowerEvent.Start();
    }

    public override void Dispose ()
    {
      base.Dispose();
      checkPowerEvent.Stop();
      monitor.Dispose();
    }

    public OutPort<Notification> NotificationPort
    {
      get { return notificationPort;}
    }

    void HandleIdleState ()
    {
      if (powerOn)
        return;

      Log("Device is working on backup supply.", LogLevel.Medium);

      PowerAlarmNotification notification = new PowerAlarmNotification();
      notification.Type = PowerAlarmNotificationType.PowerOff;

      notificationPort.PutData(notification);
      state = PowerAlarmState.Triggered;
    }

    void HandleTriggeredState ()
    {
      if (!powerOn)
        return;

      Log("Power is back again.", LogLevel.Medium);

      PowerAlarmNotification notification = new PowerAlarmNotification();
      notification.Type = PowerAlarmNotificationType.PowerOn;

      notificationPort.PutData(notification);
      state = PowerAlarmState.Idle;
    }

    void CheckPower ()
    {
      powerOn = monitor.CheckPower();
      switch (state)
      {
      case PowerAlarmState.Idle:
        HandleIdleState();
        break;
      case PowerAlarmState.Triggered:
        HandleTriggeredState();
        break;
      }
    }
  }
}

