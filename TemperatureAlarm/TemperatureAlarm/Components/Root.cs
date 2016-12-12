using FooFramework;

namespace TemperatureAlarm
{
  public class Root : Component
  {
    readonly Alarm alarm;
    readonly CellCommunicator cellCommunicator;
    readonly TempMeasurer tempMeasurer;
    readonly Notificator notificator;

    public Root(string name = "Root", Component parent = null) : base(name,parent)
    {
      alarm = new Alarm("Alarm", this);
      cellCommunicator = new CellCommunicator("CellCommunicator", this);
      tempMeasurer = new TempMeasurer("TempMeasurer", this);
      notificator = new Notificator("Notificator", this);
      /*
       * TempMeasurer <=> Alarm
       *              <=> Notificator
       * 
       * Alarm <=> Notificator
       * Notificator <=> CellCommunicator
       * 
       */
      alarm.TempPort.Connect(tempMeasurer.TempPort);
      alarm.NotificationPort.Connect(notificator.NotificationPort);
      tempMeasurer.NotificationPort.Connect(notificator.NotificationPort);
      notificator.SmsPort.Connect(cellCommunicator.SmsPort);
      notificator.CommandPort.Connect(cellCommunicator.CommandPort);
      notificator.TempPort.Connect(tempMeasurer.TempPort);
    }
  }
}
  