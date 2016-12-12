using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;

namespace TemperatureAlarm
{

  public class SerialModem
  {
    readonly SerialPort port;

    const string AT_SEND_SMS1 = "AT+CMGS=\"{0}\"\r"; // number
    const string AT_SEND_SMS2 = "{0}\u001a"; //message
    const string AT_DEL_ALL_SMS = "AT+CMGD=0,4\r";
    const string AT_DIAL = "ATD {0}\r"; //number
    const string AT_HOLD = "H\r";
    const string AT_RECEIVE_SMS = "AT+CMGL=\"ALL\"\r";
    const string AT_BEGIN = "AT\r";
    const string AT_TEXT_MODE = "AT+CMGF=1\r";
    const string AT_OK = "OK";

    //+CMGL: 1,"REC READ","+4911234567890",,"13/07/11,19:48:31+08"
    const string SMS_HEADER_REGEX = @"\+CMGL: \d+,""[^""]*"",""(?<number>[^""]+)""";

    public SerialModem (string path, int baudRate, int readTimeout)
    {
      port = new SerialPort (path, baudRate);
      port.ReadTimeout = readTimeout;
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
      WriteData(AT_TEXT_MODE);
      string res = ReadData();
      if (!res.Contains(AT_OK))
        throw new SerialModemException (SerialModemExceptionType.InitFail, res);
    }

    public void Dial (string number, int duration)
    {
      WriteData (string.Format(AT_DIAL, number));
      Thread.Sleep (duration);
      WriteData (AT_HOLD);
    }

    public void SendSms (string number, string message)
    {
      WriteData (string.Format(AT_SEND_SMS1, number));
      WriteData (string.Format(AT_SEND_SMS2, message));
      string res = ReadData ();
      if (!res.Contains(AT_OK))
        throw new SerialModemException (SerialModemExceptionType.SmsSendFail, res);
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
    public static void ParseSmsHeader (string header, ref SmsMessage target)
    {
      Regex reg = new Regex (SMS_HEADER_REGEX, RegexOptions.Compiled);
      Match m = reg.Match (header);
      target.Number = m.Groups["number"].Value;
    }
    

    public List<SmsMessage> ReceiveSms ()
    {
      WriteData (AT_RECEIVE_SMS);
      string res = ReadData();
      if (!res.Contains(AT_OK))
        throw new SerialModemException (SerialModemExceptionType.SmsRcvFail, res);
      List<SmsMessage> resSms = ParseSmsData (res);
      if (resSms != null)
        WriteData (AT_DEL_ALL_SMS);
      return resSms;
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

    string ReadData()
    {
      byte rxByte;
      string res = "";

      try
      {
        rxByte = (byte) port.ReadByte();
        while (rxByte != 255) 
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

