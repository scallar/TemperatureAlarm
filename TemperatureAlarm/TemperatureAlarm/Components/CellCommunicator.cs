using System.Collections.Generic;
using FooFramework;

namespace TemperatureAlarm
{
  public class CellCommunicator : Component
  {
    readonly InPort<CellCommand> commandPort;
    readonly OutPort<SmsMessage> smsPort;

    PeriodicEvent checkSmsEvent;
    SerialModem modem;

    public CellCommunicator(string name = "CellCommunicator", Component parent = null) : base(name,parent)
    {
      commandPort = new NbInPort<CellCommand>("CommandPort", this, HandleCellCommand);
      smsPort = new NbOutPort<SmsMessage>("SmsPort", this);
      checkSmsEvent = new PeriodicEvent(this, CheckSms);
    }

    public override void Configure(ConfigurationProvider cp)
    {
      base.Configure(cp);
      modem = new SerialModem (cp.GetElement<string>(this,"SerialPort"),
                               cp.GetElement<int>(this,"SerialBaudRate"),
                               cp.GetElement<int>(this,"SerialReadTimeout"));

      checkSmsEvent.Interval = cp.GetElement<int>(this, "SmsCheckPeriod");
    }

    public override void Initialize ()
    {
      base.Initialize();
      try
      {
        modem.Initialize();
        checkSmsEvent.Start();
      }
      catch (SerialModemException e)
      {
        Log(e.Message, LogLevel.Fatal);
      }
    }

    public InPort<CellCommand> CommandPort
    {
      get { return commandPort; }
    }

    public OutPort<SmsMessage> SmsPort
    {
      get { return smsPort; }
    }

    void Dial(DialCommand cmd)
    {
      Log(string.Format("Calling number: {0} for {1} [ms]", 
                         cmd.Number, cmd.Duration));
      modem.Dial(cmd.Number,cmd.Duration);
    }

    void SendSms(SendSmsCommand cmd)
    {
      Log(string.Format("Sending sms: to {0}, text {1}", 
                         cmd.Number, cmd.Text));
      modem.SendSms(cmd.Number, cmd.Text);
    }

    void CheckSms()
    {
      Log("Checking sms inbox");
      List<SmsMessage> res;
      try
      {
        res = modem.ReceiveSms();
        if (res != null)
          foreach (SmsMessage msg in res)
            smsPort.PutData(msg);
      } 
      catch (SerialModemException e)
      {
        Log(e.Message, LogLevel.Error);
      }
    }

    void HandleCellCommand(CellCommand cmd)
    {
      try
      {
        if (cmd is DialCommand)
          Dial((DialCommand)cmd);
        else if (cmd is SendSmsCommand)
          SendSms((SendSmsCommand)cmd);
        else
          Log("Unsupported CellCommand type !");
      } 
      catch (SerialModemException e)
      {
        Log(e.Message, LogLevel.Error);
      }
    }
  }
}

