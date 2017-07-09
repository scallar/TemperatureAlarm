using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;

namespace TemperatureAlarm
{

  public class SerialModem : IDisposable
  {
    readonly SerialPort port;

    const string AT_CONFIG_GSM = "AT^SYSCFG=13,1,3FFFFFFF,2,4\r";
    const string AT_DISABLE_HUAWEI_SHIT = "AT^CURC=0\r";

    //const string AT_PIN = "AT+CPIN=\"{0}\""; //pin
    const string AT_PIN = "AT+CLCK=\"SC\",0,\"{0}\"";
    const string AT_SEND_SMS1 = "AT+CMGS=\"{0}\"\r"; // number
    const string AT_SEND_SMS2 = "{0}\u001a"; //message
    const string AT_DEL_ALL_SMS = "AT+CMGD=0,4\r";
    const string AT_DIAL = "ATD{0};\r"; //number
    //const string AT_HOLD = "AT+CHUP\r";
    const string AT_HOLD = "ATH\r";
    const string AT_RECEIVE_SMS = "AT+CMGL=\"ALL\"\r";
    const string AT_BEGIN = "AT\r";
    const string AT_TEXT_MODE = "AT+CMGF=1\r";
    const string AT_OK = "OK";

    //+CMGL: 1,"REC READ","+4911234567890",,"13/07/11,19:48:31+08"
    const string SMS_HEADER_REGEX = @"\+CMGL: \d+,""[^""]*"",""(?<number>[^""]+)""";
    Regex smsRegex;
    int pin;

    public SerialModem (string path, int baudRate, int readTimeout, int pin)
    {
      this.pin = pin;
      port = new SerialPort (path, baudRate);
      port.ReadTimeout = readTimeout;

      smsRegex = new Regex (SMS_HEADER_REGEX, RegexOptions.Compiled);
    }

    public void Reinitialize()
    {
      bool success = false;
      do
      {
        try
        {
          Thread.Sleep(5000);
          if (port.IsOpen)
            port.Close();
          Initialize();
          success = true;
        }
        catch (Exception)
        {
          success = false;
        }
      } while (!success);
    }

    public void Initialize ()
    {
      try
      {
        port.Open();
      }
      catch (Exception e)
      {
        throw new SerialModemException (SerialModemExceptionType.PortFail, e.Message);
      }
      //SendATCommand(AT_PIN, pin);
      SendATCommand(AT_CONFIG_GSM);
      SendATCommand(AT_DISABLE_HUAWEI_SHIT);
      SendATCommand(AT_TEXT_MODE);
    }

    public void Dispose()
    {
      if (port.IsOpen)
        port.Close();
    }

    public void Dial (string number, int duration)
    {
      //SendATCommand (AT_DIAL, number);
      /*WriteData (string.Format(AT_DIAL, number));
      ReadData();
      Thread.Sleep (duration);
      WriteData(AT_HOLD);
      ReadData();*/
      //SendATCommand (AT_HOLD);
    }

    public void SendSms (string number, string message)
    {
      WriteData (string.Format(AT_SEND_SMS1, number));
      ReadData((byte)'>');
      WriteData (string.Format(AT_SEND_SMS2, message));
      string res = ReadData (); //SMS data

      int retry = 0;
      do
      {
        res += ReadData ();
      } while (!res.Contains(AT_OK) && ++retry < 6);
      if (retry == 6)
        throw new SerialModemException (SerialModemExceptionType.NotOk, res);
    }

    public List<SmsMessage> ReceiveSms ()
    {
      string res = SendATCommand (AT_RECEIVE_SMS);
      List<SmsMessage> resSms = ParseSmsData (res);
      if (resSms != null)
        SendATCommand (AT_DEL_ALL_SMS);
      return resSms;
    }

    List<SmsMessage> ParseSmsData (string data)
    {
      if (!data.Contains("+CMGL:"))
        return null;

      List<SmsMessage> res = new List<SmsMessage>();

      string[] tmp = data.Split ('\r');
      for (int i = 0; i < tmp.Length; i++) 
      {
        if (tmp [i].Trim().StartsWith ("+CMGL:")) 
        {
          SmsMessage msg = new SmsMessage();
          ParseSmsHeader (tmp[i++], ref msg);
          msg.Text = tmp[i].Trim();
          res.Add (msg);
        } 
      }

      return res;
    }

    void ParseSmsHeader (string header, ref SmsMessage target)
    {
      Match m = smsRegex.Match (header);
      target.Number = m.Groups["number"].Value;
    }

    string SendATCommand (string command, params object[] args)
    {
      if (args.Length > 0)
        WriteData(string.Format(command, args));
      else
        WriteData(command);
      string res = ReadData();
      if (!res.Contains(AT_OK))
        throw new SerialModemException (SerialModemExceptionType.NotOk, res);        
      return res;
    }


    void WriteData(string data)
    {
      try 
      {
        port.Write(data);
      }
      catch (Exception e)
      {
        throw new SerialModemException(SerialModemExceptionType.PortFail, 
                                       e.Message);
      }
    }

    string ReadData(byte endChar = 255)
    {
      byte rxByte;
      string res = "";

      try
      {
        rxByte = (byte) port.ReadByte();
        while (rxByte != endChar) 
        {
           res += ((char) rxByte);
           rxByte = (byte) port.ReadByte();
        }
      } 
      catch (Exception e)
      {
        if (!(e is TimeoutException))
          throw new SerialModemException(SerialModemExceptionType.PortFail, 
                                         e.Message);
      }

      #if DEBUG_MODEM
      Console.WriteLine(res);
      #endif

      return res;
    }
  }
}

