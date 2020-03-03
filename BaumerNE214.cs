using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace QualityControl.cls
{
    class BaumerNE214 : IIndikator
    {
        string ReturnedValue = "";
        private string MeterRead = "";
        public BaumerNE214()
        {
            try
            {
                Sp = new SerialPort();
                Sp.DataReceived += Sp_DataReceived;
            }
            catch (Exception ex)
            {


            }

        }
        string SuccessValue = "";
        string _Direction = "";
        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string ReadedValue = ((SerialPort)sender).ReadExisting();
                if (SuccessValue.IndexOf("0001R") == -1)
                    SuccessValue = ReadedValue;
                else if (ReadedValue.IndexOf("0001R") != -1)
                    _Direction = ReadedValue.Substring(SuccessValue.IndexOf("R") + 1);

                if (SuccessValue.IndexOf("0001R") == -1 && !string.IsNullOrEmpty(SuccessValue))
                {
                    Regex r = new Regex(@".*[\d]");
                    Match M = r.Match(SuccessValue);
                    string Value = M.Value;

                    if (double.TryParse(_Direction + Value, out double LastValue))
                        Meter = LastValue;
                }
            }
            catch
            {
            }
        }

        public override void MeterOku()
        {
            if (Status != ConnectionStatus.Opened)
            {
                Bits_Per_Second = 4800;
                Data_Bits = 7;
                Port_No = clsAyar.MeterPortAdi;
                Parity = System.IO.Ports.Parity.Even;
                Stop_Bits = StopBits.One;

                Connect();
                if (Status != ConnectionStatus.Opened)
                    return;
            }

            clsGeneral.MeterConnect = "";
            Sp.DiscardInBuffer();
            Sp.DiscardOutBuffer();
            Sp.DataReceived -= Sp_DataReceived;
            Sp.DataReceived += Sp_DataReceived;
            byte[] Dizi = new byte[] { 0x02, 0x30, 0x30, 0x30, 0x31, 0x52, 0x30, 0x30, 0x31, 0x35, 0x33, 0x37, 0x03, 0x0D }; //Encoding.ASCII.GetBytes(txtGiden.Text);
            Sp.Write(Dizi, 0, Dizi.Length);

        }


        public override void ManuelProcess(string Islem, string PlcCommand)
        {
            if (Status != ConnectionStatus.Opened)
            {
                Bits_Per_Second = 4800;
                Data_Bits = 7;
                Parity = System.IO.Ports.Parity.Even;
                Stop_Bits = StopBits.One;
                Connect();
                if (Status != ConnectionStatus.Opened)
                    return;
            }
            MeterRead = Sp.ReadExisting();

            if (MeterRead == "")
            {
                Status = ConnectionStatus.Error;
                clsMessage.ErrorTanim = "Connection Problem";
                clsGeneral.MeterConnect = "!";
                Meter = 0;
                Sp.Close();
                return;
            }
            else
            {
                if (Status != ConnectionStatus.Opened)
                {
                    Bits_Per_Second = 4800;
                    Data_Bits = 7;
                    Parity = System.IO.Ports.Parity.Even;
                    Stop_Bits = StopBits.One;
                    Connect();
                    if (Status != ConnectionStatus.Opened)
                        return;
                }

                if (clsAyar.MeterFormat.Length > MeterRead.Length) return;
                if (!clsGeneral.WriteMeterIndicator(Sp, "Meter", PlcCommand, out ReturnedValue)) return;
                clsGeneral.MeterConnect = "";
            }

            try
            {
                Meter = ReturnedValue.ToDouble();
                Sp.DiscardInBuffer();
            }
            catch { }
            Sp.DiscardInBuffer();
            Sp.DiscardOutBuffer();
        }

        
    }
}
