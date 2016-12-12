using System.Collections.Generic;
using FooFramework;

namespace TemperatureAlarm
{
  public class Notificator : Component
  {
    enum NotificatorState
    {
      Idle, AlarmOn, AlarmAcknowledged
    }

    readonly InPort<Notification> notificationPort;
    readonly InPort<SmsMessage> smsPort;
    readonly InPort<TempData> tempPort;
    readonly OutPort<CellCommand> commandPort;

    const string TEMP_DELIMETER = "; ";

    string tempNotOkMsg;
    string tempOkMsg;
    string statusMsg;
    string sensorMalfMsg;
    string statisticsMsg;
    string ackCmd;
    string statusReqCmd;
    string disableAlarmCmd;
    string enableAlarmCmd;
    List<string> numbers;
    int dialDuration;

    NotificatorState state;
    bool alarmingEnabled;
    PeriodicEvent periodicDialEvent;
    int periodicDialIndex;
    TempData lastTemp;

    public Notificator (string name = "Notificator", 
                        Component parent = null) : base (name, parent)
    {
      smsPort = new StdInPort<SmsMessage> ("SmsPort", this, HandleSms);
      notificationPort = new StdInPort<Notification> ("NotificationPort", this, HandleNofication);
      commandPort = new StdOutPort<CellCommand> ("CommandPort", this);
      tempPort = new StdInPort<TempData>("TempPort", this, HandleTempData);
      periodicDialEvent = new PeriodicEvent(this, PeriodicDial);
    }

    public override void Configure (ConfigurationProvider cp)
    {
      base.Configure (cp);
      tempNotOkMsg = cp.GetElement<string> (this, "TempNotOkMsg");
      tempOkMsg = cp.GetElement<string> (this, "TempOkMsg");
      sensorMalfMsg = cp.GetElement<string> (this, "SensorMalfMsg");
      statusMsg = cp.GetElement<string> (this, "StatusMsg");
      statisticsMsg = cp.GetElement<string> (this, "StatisticsMsg");

      ackCmd = cp.GetElement<string> (this, "AckCmd");
      statusReqCmd = cp.GetElement<string> (this, "StatusReqCmd");
      disableAlarmCmd = cp.GetElement<string> (this, "DisableAlarmCmd");
      enableAlarmCmd = cp.GetElement<string> (this, "EnableAlarmCmd");

      numbers = cp.GetElements<string> (this, "Numbers/Number");
      dialDuration = cp.GetElement<int> (this, "DialDuration");

      periodicDialEvent.Interval = cp.GetElement<int>(this, "NextDialInterval");
    }

    public override void Initialize ()
    {
      base.Initialize();
      state = NotificatorState.Idle;
      alarmingEnabled = true;
    }

    public InPort<Notification> NotificationPort 
    {
      get { return notificationPort; }
    }

    public OutPort<CellCommand> CommandPort 
    {
      get { return commandPort; }
    }

    public InPort<SmsMessage> SmsPort
    {
      get { return smsPort;  }
    }

    public InPort<TempData> TempPort
    {
      get { return tempPort; }
    }

    void SendTempNotOkMsg (TempData data)
    {
      Log("Sending alarm message to subscribers",
           LogLevel.Medium);
      string tmps = string.Join (TEMP_DELIMETER, data.Temperature);
      string msgTxt = string.Format (tempNotOkMsg, tmps);
      foreach (string number in numbers) 
      {
        SendSmsCommand cmd = new SendSmsCommand(number, msgTxt);
        commandPort.PutData (cmd);
      }
    }

    void SendTempOkMsg (TempData data)
    {
      Log("Sending alarm(disabled) message to subscribers",
           LogLevel.Medium);
      string tmps = string.Join (TEMP_DELIMETER, data.Temperature);
      string msgTxt = string.Format (tempOkMsg, tmps);
      foreach (string number in numbers) 
      {
        SendSmsCommand cmd = new SendSmsCommand(number, msgTxt);
        commandPort.PutData (cmd);
      }
    }

    void SendStatusMsg(string number)
    {
      Log(string.Format("Sending status to number: {0}",number),LogLevel.Medium);
      string tmps = string.Join (TEMP_DELIMETER, lastTemp.Temperature);
      string msgTxt = string.Format (statusMsg, alarmingEnabled ? 1 : 0, tmps);
      SendSmsCommand cmd = new SendSmsCommand(number, msgTxt);
      commandPort.PutData(cmd);
    }

    void SendSensorMalfMsg()
    {
      Log(string.Format("Sending sensor malfunction msg to all subscribers"),LogLevel.Medium);
      foreach (string number in numbers) 
      {
        SendSmsCommand cmd = new SendSmsCommand(number, sensorMalfMsg);
        commandPort.PutData(cmd);
      }
    }

    void PeriodicDial ()
    {
      DialCommand cmd = new DialCommand(numbers[periodicDialIndex], dialDuration);
      periodicDialIndex = (periodicDialIndex + 1) % numbers.Count;
      commandPort.PutData (cmd);
    }

    void TogglePeriodicDial ()
    {
      periodicDialIndex = 0;
      periodicDialEvent.Start();
    }

    void DisablePeriodicDial ()
    {
      periodicDialEvent.Stop();
    }

    void AcknowledgeAlarm (string number)
    {
      if (state == NotificatorState.AlarmOn)
      {
        Log(string.Format("Alarm is acknowledged by: {0}", number), 
          LogLevel.Medium);
        DisablePeriodicDial();
        state = NotificatorState.AlarmAcknowledged;
      }
      else
      {
        Log(string.Format("Alarm is acknowledged by: {0} (unexpected !)", number), 
          LogLevel.Medium);
      }
    }

    void DisableAlarming(string number)
    {
      Log(string.Format("Alarm is disabled by: {0}", number), 
        LogLevel.Medium);
      alarmingEnabled = false;
      DisablePeriodicDial();
    }

    void EnableAlarming(string number)
    {
      Log(string.Format("Alarm is enabled by: {0}", number), 
        LogLevel.Medium);
      alarmingEnabled = true;
      if (state == NotificatorState.AlarmOn)
      {
        SendTempNotOkMsg(lastTemp);
        TogglePeriodicDial();
      }
    }

    void HandleNofication (Notification data)
    {
      if (data is AlarmNotification)
        HandleAlarmNofication((AlarmNotification)data);
      else if (data is TempMeasurerNotification)
        HandleTempMeasurerNotification((TempMeasurerNotification)data);
      else if (data is StatCollectorNotification)
        HandleStatCollectorNotification((StatCollectorNotification)data);
      else
        Log(string.Format("Unknown notification: {0}", data), LogLevel.Error);
    }

    void HandleAlarmNofication(AlarmNotification data)
    {
      switch (data.Type) 
      {
        case AlarmNotificationType.NotOK:
          Log("Temperature is unacceptable", LogLevel.Medium);
          if (alarmingEnabled)
          {
            SendTempNotOkMsg(data.Data);
            TogglePeriodicDial();
          }
          state = NotificatorState.AlarmOn;
          break;
        case AlarmNotificationType.OK:
          Log("Temperature is back to normal", LogLevel.Medium);
          if (alarmingEnabled)
          {
            SendTempOkMsg(data.Data);
            DisablePeriodicDial();
          }
          state = NotificatorState.Idle;
          break;
      }
    }

    void HandleTempMeasurerNotification(TempMeasurerNotification data)
    {
      switch (data.Type)
      {
        case TempMeasurerNotificationType.NoSensors:
          //SendSensorMalfMsg();
          break;
        case TempMeasurerNotificationType.SensorMalfunction:
          //SendSensorMalfMsg();
          break;
      }
    }

    void HandleStatCollectorNotification (StatCollectorNotification data)
    {
      Log("Sending periodic statistics", LogLevel.Low);
      string number = numbers[0];
      string msgTxt = string.Format(statisticsMsg, data.WrongTempSpan,
                         data.AlarmTempSpan, data.LowestTemp, data.HighestTemp);


      SendSmsCommand cmd = new SendSmsCommand(number, msgTxt);
      commandPort.PutData(cmd);
    }

    void HandleSms(SmsMessage data)
    {
      string txt = data.Text.ToLower();

      if (txt == ackCmd)
        AcknowledgeAlarm(data.Number);
      else if (txt == statusReqCmd)
        SendStatusMsg(data.Number);
      else if (txt == disableAlarmCmd)
        DisableAlarming(data.Number);
      else if (txt == enableAlarmCmd)
        EnableAlarming(data.Number);
      else
        Log(string.Format("Omitted unrecognized msg: {0}", data),
            LogLevel.Medium);
    }

    void HandleTempData (TempData data)
    {
      lastTemp = data;
    }
  }
}