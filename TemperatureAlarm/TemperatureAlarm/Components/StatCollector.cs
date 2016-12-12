using System;
using FooFramework;

namespace TemperatureAlarm
{
  public class StatCollector : Component
  {
    readonly InPort<StatCollectorData> dataPort;
    readonly InPort<TempData> tempPort;
    readonly OutPort<StatCollectorNotification> notificationPort;

    PeriodicEvent publishStats;

    float lowestTemp;
    float highestTemp;
    TimeSpan wrongTempSpan;
    TimeSpan alarmTempSpan;

    const double TIME_DAYS = 1000d * 60d * 60d * 24d;

    public StatCollector(string name = "StatCollector", Component parent = null) : base(name,parent)
    {
      dataPort = new StdInPort<StatCollectorData>("DataPort", this, HandleData);
      tempPort = new StdInPort<TempData>("TempPort", this, HandleTempData);
      notificationPort = new StdOutPort<StatCollectorNotification>("NotificationPort", this);
      publishStats = new PeriodicEvent(this, PublishStatsAndReset);    
    }

    public InPort<StatCollectorData> DataPort
    {
      get { return dataPort; }
    }

    public InPort<TempData> TempPort
    {
      get { return tempPort; }
    }

    public OutPort<StatCollectorNotification> NotificationPort
    {
      get { return notificationPort; }
    }

    public override void Configure(ConfigurationProvider cp)
    {
      base.Configure(cp);
      double interval = cp.GetElement<double>(this, "PublishPeriod") * TIME_DAYS;
      if (interval <= 0f)
        publishStats.Interval = 100f;
      else
        publishStats.Interval = interval;
    }

    public override void Initialize ()
    {
      base.Initialize();
      ResetData();
      publishStats.Start();
    }

    void ResetData()
    {
      lowestTemp = float.MaxValue;
      highestTemp = float.MinValue;     
      wrongTempSpan = new TimeSpan(0, 0, 0);
      alarmTempSpan = new TimeSpan(0, 0, 0);
    }

    void HandleData(StatCollectorData data)
    {
      if (data is AlarmStatData)
        HandleAlarmStatData((AlarmStatData)data);
      else
        Log(string.Format("Unknown notification: {0}", data), LogLevel.Error);
    }

    void HandleAlarmStatData (AlarmStatData data)
    {
      switch (data.Type)
      {
        case AlarmStatData.AlarmStatDataType.TempNotOk:
          wrongTempSpan += data.Span;
          break;
        case AlarmStatData.AlarmStatDataType.AlarmOn:
          alarmTempSpan += data.Span;
          break;
      }
    }

    void HandleTempData(TempData data)
    {
      foreach (float temp in data.Temperature)
      {
        if (temp > highestTemp)
          highestTemp = temp;
        else if (temp < lowestTemp)
          lowestTemp = temp;
      }
    }

    void PublishStatsAndReset()
    {
      Log("Publishing & resetting statistics", LogLevel.Low);

      StatCollectorNotification res = new StatCollectorNotification();

      res.AlarmTempSpan = alarmTempSpan;
      res.WrongTempSpan = wrongTempSpan;
      res.HighestTemp = highestTemp;
      res.LowestTemp = lowestTemp;

      notificationPort.PutData(res);
      ResetData();
    }
  }
}

