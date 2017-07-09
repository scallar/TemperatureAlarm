using FooFramework;

namespace TemperatureAlarm
{
  public class Root : Component
  {
    readonly Alarm alarm;
    readonly CellCommunicator cellCommunicator;
    readonly TempMeasurer tempMeasurer;
    readonly Notificator notificator;
    readonly PowerAlarm powerAlarm;
    readonly StatCollector statCollector;

    public Root(string name = "Root", Component parent = null) : base(name,parent)
    {
      cellCommunicator = new CellCommunicator("CellCommunicator", this);
      alarm = new Alarm("Alarm", this);
      tempMeasurer = new TempMeasurer("TempMeasurer", this);
      notificator = new Notificator("Notificator", this);
      powerAlarm = new PowerAlarm("PowerAlarm", this);
      statCollector = new StatCollector("StatCollector", this);

      /*
       * TempMeasurer <=> Alarm
       *              <=> Notificator
       * 
       * Alarm <=> Notificator
       * Notificator <=> CellCommunicator
       * 
       * PowerAlarm <=> Notificator
       */
      alarm.TempPort.Connect(tempMeasurer.TempPort);
      alarm.NotificationPort.Connect(notificator.NotificationPort);
      tempMeasurer.NotificationPort.Connect(notificator.NotificationPort);
      notificator.SmsPort.Connect(cellCommunicator.SmsPort);
      notificator.CommandPort.Connect(cellCommunicator.CommandPort);
      notificator.TempPort.Connect(tempMeasurer.TempPort);
      powerAlarm.NotificationPort.Connect(notificator.NotificationPort);
      statCollector.TempPort.Connect(tempMeasurer.TempPort);
      statCollector.DataPort.Connect(alarm.StatData);
      statCollector.NotificationPort.Connect(notificator.NotificationPort);
    }
  }
}
  