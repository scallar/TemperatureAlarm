using System;
using FooFramework;

namespace TemperatureAlarm 
{
  public class Alarm : Component
  {
    enum AlarmState 
    {
      Idle, TempNotOk, AlarmTriggered
    }

    readonly InPort<TempData> tempPort;
    readonly OutPort<Notification> notificationPort;
    readonly OutPort<StatCollectorData> statData;

    float maxTemp;
    float minTemp;
    TimeSpan histeresis;

    DateTime start;
    AlarmState state;
    bool wrongTemp;
    TempData currentData;

    public Alarm (string name = "Alarm", Component parent = null) : base(name,parent)
    {
      tempPort = new StdInPort<TempData>("TempPort", this, HandleTempData);
      notificationPort = new StdOutPort<Notification>("NotificationPort", this);
      statData = new StdOutPort<StatCollectorData>("StatData", this);
    }

    public override void Configure(ConfigurationProvider cp)
    {
      base.Configure(cp);
      maxTemp = cp.GetElement<float>(this, "MaxTemp");
      minTemp = cp.GetElement<float>(this, "MinTemp");
      histeresis = new TimeSpan(cp.GetElement<int>(this, "Histeresis/Hour"),
                                cp.GetElement<int>(this, "Histeresis/Minute"),
                                cp.GetElement<int>(this, "Histeresis/Second"));
    }

    public override void Initialize ()
    {
      base.Initialize();
      state = AlarmState.Idle;
    }

    public InPort<TempData> TempPort
    {
      get {return tempPort;}
    }

    public OutPort<Notification> NotificationPort
    {
      get {return notificationPort;}
    }

    public OutPort<StatCollectorData> StatData
    {
      get { return statData;}
    }

    void Alert()
    {
      AlarmNotification res = new AlarmNotification();
      res.Data = currentData;
      res.Type = AlarmNotificationType.NotOK;
      notificationPort.PutData(res);
    }
    void CancelAlert()
    {
      AlarmNotification res = new AlarmNotification();
      res.Data = currentData;
      res.Type = AlarmNotificationType.OK;
      notificationPort.PutData(res);
    }

    void PublishStatData(AlarmStatData.AlarmStatDataType type, TimeSpan span)
    {
      AlarmStatData res = new AlarmStatData();
      res.Span = span;
      res.Type = type;
      statData.PutData(res);
    }
      
    void HandleIdleState()
    {
      if (wrongTemp)
      {
         Log(string.Format("Temperature NOT OK - {0}",currentData));
         start = currentData.Timestamp;
         state = AlarmState.TempNotOk;
      }
    }

    void HandleTempNotOkState()
    {
      TimeSpan span = currentData.Timestamp - start;
      if (wrongTemp && span >= histeresis)
      {
        Log(string.Format("Calling alarm - {0}", currentData), 
          LogLevel.Medium);
        Alert();
        start = currentData.Timestamp;
        state = AlarmState.AlarmTriggered;
      }
      else if (!wrongTemp)
      {
        state = AlarmState.Idle;
        PublishStatData(AlarmStatData.AlarmStatDataType.TempNotOk, span);
      }
    }

    void HandleAlarmTriggeredState()
    {
      TimeSpan span = currentData.Timestamp - start;
      if (!wrongTemp && span >= histeresis)
      {
        Log(string.Format("Cancelling alarm - {0}", currentData),
                           LogLevel.Medium);
        CancelAlert();
        state = AlarmState.Idle;
        PublishStatData(AlarmStatData.AlarmStatDataType.AlarmOn, span);
      }
      else if (wrongTemp)
        start = currentData.Timestamp;
    }

    bool CheckTemperature(TempData data)
    {
      foreach (float temp in data.Temperature)
        if (temp > maxTemp || temp < minTemp)
          return true;
      return false;
    }

    void HandleTempData(TempData data)
    {
      currentData = data;
      wrongTemp = CheckTemperature(data);
      switch (state)
      {
        case AlarmState.Idle:
          HandleIdleState();
          break;
        case AlarmState.TempNotOk:
          HandleTempNotOkState();
          break;
        case AlarmState.AlarmTriggered:
          HandleAlarmTriggeredState();
          break;
        }
    }
  }
}

