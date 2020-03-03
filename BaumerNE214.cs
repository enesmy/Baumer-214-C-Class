using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace KaliteKontrol.cls
{
    class BaumerNE214 : IIndikator
    {
        string GelenDeger = "";
        private string BaglantiMetre = "";
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
        string Başarı = "";
        string Yon = "";
        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string test = ((SerialPort)sender).ReadExisting();
                if (test.IndexOf("0001R") == -1)
                    Başarı = test;
                else if (test.IndexOf("0001R") != -1)
                    Yon = test.Substring(test.IndexOf("R") + 1);

                if (Başarı.IndexOf("0001R") == -1 && !string.IsNullOrEmpty(Başarı))
                {
                    Regex r = new Regex(@".*[\d]");
                    Match M = r.Match(Başarı);
                    string Değer = M.Value;

                    if (double.TryParse(Yon + Değer, out double SonDeger))
                        Metre = SonDeger;
                }
            }
            catch
            {
            }
        }

        public override void MetreOku()
        {
            if (Durum != BaglantiDurumlari.Acik)
            {
                Bits_Per_Second = 4800;
                Data_Bits = 7;
                Port_No = clsAyar.MetrePortAdi;
                Parity = System.IO.Ports.Parity.Even;
                Stop_Bits = StopBits.One;

                Baglan();
                if (Durum != BaglantiDurumlari.Acik)
                    return;
            }

            clsGenel.MetreBaglanti = "";
            Sp.DiscardInBuffer();
            Sp.DiscardOutBuffer();
            Sp.DataReceived -= Sp_DataReceived;
            Sp.DataReceived += Sp_DataReceived;
            byte[] Dizi = new byte[] { 0x02, 0x30, 0x30, 0x30, 0x31, 0x52, 0x30, 0x30, 0x31, 0x35, 0x33, 0x37, 0x03, 0x0D }; //Encoding.ASCII.GetBytes(txtGiden.Text);
            Sp.Write(Dizi, 0, Dizi.Length);

        }


        public override void TartiOku()
        {
            Kilo = 0;
        }

        public override void GecikmeliYazdirBitir()
        { }

        public override void EnOku()
        { }

        public override void ManuelIslem(string Islem, string PlcKomut)
        {
            if (Durum != BaglantiDurumlari.Acik)
            {
                Bits_Per_Second = 4800;
                Data_Bits = 7;
                Parity = System.IO.Ports.Parity.Even;
                Stop_Bits = StopBits.One;
                Baglan();
                if (Durum != BaglantiDurumlari.Acik)
                    return;
            }
            BaglantiMetre = Sp.ReadExisting();

            if (BaglantiMetre == "")
            {
                Durum = BaglantiDurumlari.Hata;
                clsMesaj.HataTanim = "Bağlantı Sorunu";
                clsGenel.MetreBaglanti = "!";
                Metre = 0;
                Sp.Close();
                return;
            }
            else
            {
                if (Durum != BaglantiDurumlari.Acik)
                {
                    Bits_Per_Second = 4800;
                    Data_Bits = 7;
                    Parity = System.IO.Ports.Parity.Even;
                    Stop_Bits = StopBits.One;
                    Baglan();
                    if (Durum != BaglantiDurumlari.Acik)
                        return;
                }

                if (clsAyar.MetreFormat.Length > BaglantiMetre.Length) return;
                if (!clsGenel.TurmetreYaz(Sp, "metre", PlcKomut, out GelenDeger)) return;
                clsGenel.MetreBaglanti = "";
            }

            try
            {
                Metre = GelenDeger.ToDouble();
                Sp.DiscardInBuffer();
            }
            catch { }
            Sp.DiscardInBuffer();
            Sp.DiscardOutBuffer();
        }

        public override string ManuelOku(string Islem, string PlcKomut)
        {
            throw new NotImplementedException();
        }

        public override int GecikmeliYazdirOku()
        {
            //throw new NotImplementedException();
            return 0;
        }

        public override void GecikmeliYazdir(float TopNo)
        {
            //throw new NotImplementedException();

        }

        public override void StaticsYenile()
        {
            throw new NotImplementedException();
        }

        public override void YukelemeEmriver(int TopID, int LabNumuneID, int MusteriNumuneID, int LabNumuneCM, int MusteriNumuneCM, int KaliteSirasi, int YuklemeAdres)
        {
            throw new NotImplementedException();
        }

        public override void Devir(bool DevrilsinMi)
        {
            throw new NotImplementedException();
        }
    }
}
