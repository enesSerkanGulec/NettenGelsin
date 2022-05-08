using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using System.Xml;
using System.IO;
using System.Globalization;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;

namespace NettenGelsin
{
    class logType
    {
        public DateTime zaman;
        public string ifade;
        public string tip;
        public logType(string ifade, string tip = "Bilgi")
        {
            this.zaman = DateTime.Now;
            this.ifade = ifade;
            this.tip = tip;
        }
        public string toString()
        {
            return "('" + zaman.ToString("yyyy-MM-dd HH:mm:ss") + "','" + ifade.Replace("'", "\\'") + "','" + tip + "')";
        }
    }
    public delegate void tetikleme();
    public static class log
    {
        public static MySqlConnection con;
        static MySqlCommand cmd = new MySqlCommand();
        //static int sayaç = 1;
        //static int gün = -1;
        //static string klasörİsmi { get { DateTime t = DateTime.Now; return t.Year.ToString() + "_" + (new string[] { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" })[t.Month - 1]; } }
        //static string dosyaİsmi 
        //{ 
        //    get 
        //    {
        //        if (gün != DateTime.Now.Day) sayaç = 1;
        //        string s = DateTime.Now.Day.ToString(); 
        //        return "Gün_" + (s.Length == 1 ? "0" : "") + s + "_" + sayaç.ToString() + ".txt"; 
        //    } 
        //}
        static List<logType> yazılacaklar = new List<logType>();
        static Thread anaThread = new Thread(() => kontrol());
        static Thread yazmaişlemi;
        public static bool kapanacak = false;
        private static void kontrol()
        {
            while (true)
            {
                if (yazılacaklar.Count > 0 && !(yazmaişlemi != null && yazmaişlemi.IsAlive))
                {
                    yazmaişlemi = new Thread(() => Yaz_());
                    yazmaişlemi.Start();
                    yazmaişlemi.Join();
                    Thread.Sleep(10);
                }
                if (kapanacak) break;
                Thread.Sleep(5000);
            }
        }
        public static void Yaz(string ifade, string tip = "")
        {
            yazılacaklar.Add(new logType(ifade, tip));
            if (anaThread.ThreadState == System.Threading.ThreadState.Unstarted) anaThread.Start();
        }
        static void Yaz_()
        {
            int max = yazılacaklar.Count;
            string s = "";
            lock (yazılacaklar)
            {
                for (int i = 0; i < max; i++) s += (s == "" ? "" : ",") + yazılacaklar[i].toString();
                cmd.CommandText = "insert into logs(zaman,mesaj,tip) values " + s;
                veritabanı.cmdExecute(cmd);
                yazılacaklar.RemoveRange(0, max);
            }
            //tetiklenecek();
        }
        public static void Başla()
        {
            con = new MySqlConnection(veritabanı.connectionString);
            cmd.Connection = con;
            cmd.CommandTimeout = 300;
            con.Open();
            cmd.CommandText = "DELETE FROM logs WHERE DATEDIFF(CURRENT_DATE(),logs.zaman)>5";
            veritabanı.cmdExecute(cmd);
        }
        //public static void değiştir()
        //{
        //    dosya = new FileInfo(dizin + "\\log" + (++sayaç).ToString() + ".txt");
        //}
        //public static void listeyiYaz()
        //{
        //    File.WriteAllText(dosya.FullName, string.Join("\n", liste.AsParallel().ToArray()));
        //    değiştir();
        //    liste.Clear();
        //}
    }

    public static class email
    {
        static string senderAdres = "enesserkangulec@gmail.com";
        static string senderPwd = "Enes.072";
        public static string[] receivers = { "enesgulec@gmail.com", "engin@ortaklarbilgisayar.com" };

        public static void send(string konu, string gövdeMetni)
        {
            var fromAddress = new MailAddress(senderAdres, "__nettengelsin__");
            string fromPassword = senderPwd;
            string subject = konu;
            string body = gövdeMetni;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            foreach (string receiver in receivers)
            {
                var toAddress = new MailAddress(receiver, receiver.Substring(0, receiver.IndexOf('@')));
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception hata)
                    {
                        log.Yaz("e-posta gönderilemedi. (gönderilen:" + toAddress + ", Mesaj metni:[" + subject + "] " + body + ") Hata Mesajı:" + hata.Message);
                    }
                }
            }
        }
    }


    //public static class şifreleme
    //{
    //    public static string Sifrele(string data)
    //    {
    //        byte[] tempDizi = System.Text.ASCIIEncoding.ASCII.GetBytes(data);// şifrelenecek veri byte dizisine çevrilir
    //        string finalData = System.Convert.ToBase64String(tempDizi);//Base64 ile şifrelenir
    //        return finalData;
    //    }
    //    public static string SifreCoz(string data)
    //    {
    //        try
    //        {
    //            byte[] tempDizi = System.Convert.FromBase64String(data);
    //            string finalData = System.Text.ASCIIEncoding.ASCII.GetString(tempDizi);
    //            return finalData;
    //        }
    //        catch { return data; }
    //    }
    //}
    //public class Motoraşin_kayıt_sınıfı
    //{
    //    public int id;
    //    public string ManufacturerCode;
    //    public string Name;
    //    public string Manufacturer;
    //    public string Quantity;
    //    public double Price;
    //    public string PriceCurrency;
    //    public string VehicleType;
    //    public string VehicleBrand;
    //    public string OrginalNo;
    //    public string Picture;
    //    public double MinOrder;

    //    public Motoraşin_kayıt_sınıfı(dynamic data)
    //    {
    //        ManufacturerCode = data["ManufacturerCode"].Value;
    //        Name = data["Name"].Value;
    //        Manufacturer = data["Manufacturer"].Value;
    //        Quantity = data["Quantity"].Value;
    //        Price = data["Price"].Value;
    //        PriceCurrency = data["PriceCurrency"].Value;
    //        VehicleType = data["VehicleType"].Value;
    //        VehicleBrand = data["VehicleBrand"].Value;
    //        OrginalNo = data["OrginalNo"].Value;
    //        Picture = data["Picture"].Value;
    //        MinOrder = data["MinOrder"].Value;
    //    }
    //    public MySqlCommand insertCommand(string tabloAdı, MySqlConnection con)
    //    {
    //        MySqlCommand c = new MySqlCommand("insert into " + tabloAdı + "(ManufacturerCode, Name, Manufacturer, Quantity, Price, PriceCurrency, VehicleType, VehicleBrand, OrginalNo, Picture, MinOrder) values(@mc,@n,@m,@q,@pr,@pc,@vt,@vb,@on,@pi,@mo)", con);
    //        c.Parameters.AddWithValue("@mc", ManufacturerCode);
    //        c.Parameters.AddWithValue("@n", Name);
    //        c.Parameters.AddWithValue("@m", Manufacturer);
    //        c.Parameters.AddWithValue("@q", Quantity);
    //        c.Parameters.AddWithValue("@pr", Price);
    //        c.Parameters.AddWithValue("@pc", PriceCurrency);
    //        c.Parameters.AddWithValue("@vt", VehicleType);
    //        c.Parameters.AddWithValue("@vb", VehicleBrand);
    //        c.Parameters.AddWithValue("@on", OrginalNo);
    //        c.Parameters.AddWithValue("@pi", Picture);
    //        c.Parameters.AddWithValue("@mo", MinOrder);
    //        return c;
    //        //string x = Price.ToString().Replace(',', '.');
    //        //return "insert into " + tabloAdı + "(ManufacturerCode, Name, Manufacturer, Quantity, Price, PriceCurrency, VehicleType, VehicleBrand, OrginalNo, Picture, MinOrder) values(\"" + ManufacturerCode + "\",\"" + Name + "\",\"" + Manufacturer + "\",\"" + Quantity + "\"," + x + ",\"" + PriceCurrency + "\",\"" + VehicleType + "\",\"" + VehicleBrand + "\",\"" + OrginalNo + "\",\"" + Picture + "\"," + MinOrder.ToString() + ")";
    //    }

    //}
    //public class Dinamik_kayıt_sınıfı
    //{
    //    public int id;
    //    public string stok_kodu;
    //    public string stok_adi;
    //    public string marka;
    //    public string uretici_kodu;
    //    public string onceki_kod;
    //    public string kull1s;
    //    public string kull2s;
    //    public string kull3s;
    //    public string kull4s;
    //    public string kull5s;
    //    public string kull6s;
    //    public string kull7s;
    //    public string kull8s;
    //    public string resim_url;
    //    public string oem_liste;
    //    public string esdegerListe;
    //    public double fiyat;
    //    public string varyok;
    //    public string barkod1;
    //    public string barkod2;
    //    public string barkod3;
    //    public int kampanyaOrani;
    //    public int paketMiktari;
    //    public int koliMiktari;
    //    public string olcuBirimi;

    //    public Dinamik_kayıt_sınıfı(dynamic data)
    //    {
    //        stok_kodu = data["stok_kodu"].Value;
    //        stok_adi = data["stok_adi"].Value;
    //        marka = data["marka"].Value;
    //        uretici_kodu = data["uretici_kodu"].Value;
    //        onceki_kod = data["onceki_kod"].Value;
    //        kull1s = data["kull1s"].Value;
    //        kull2s = data["kull2s"].Value;
    //        kull3s = data["kull3s"].Value;
    //        kull4s = data["kull4s"].Value;
    //        kull5s = data["kull5s"].Value;
    //        kull6s = data["kull6s"].Value;
    //        kull7s = data["kull7s"].Value;
    //        kull8s = data["kull8s"].Value;
    //        resim_url = data["resim_url"].Value;
    //        oem_liste = data["oem_liste"].Value;
    //        esdegerListe = data["esdegerListe"].Value;
    //        fiyat = data["fiyat"].Value;
    //        varyok = data["varyok"].Value;
    //        barkod1 = data["barkod1"].Value;
    //        barkod2 = data["barkod2"].Value;
    //        barkod3 = data["barkod3"].Value;
    //        kampanyaOrani = (int)data["kampanyaOrani"].Value;
    //        paketMiktari = (int)data["paketMiktari"].Value;
    //        koliMiktari = (int)data["koliMiktari"].Value;
    //        olcuBirimi = data["olcuBirimi"].Value;
    //    }
    //    public MySqlCommand insertCommand(string tabloAdı, MySqlConnection con)
    //    {
    //        MySqlCommand c = new MySqlCommand("insert into " + tabloAdı + "(stok_kodu, stok_adi, marka, uretici_kodu, onceki_kod, kull1s, kull2s, kull3s, kull4s, kull5s, kull6s, kull7s, kull8s, resim_url, oem_liste, esdegerListe, fiyat, varyok, barkod1, barkod2, barkod3, kampanyaOrani, paketMiktari, koliMiktari,  olcuBirimi) values(@sk,@sa,@m,@uk,@ok,@k1,@k2,@k3,@k4,@k5,@k6,@k7,@k8,@ru,@ol,@el,@f,@v,@b1,@b2,@b3,@ko,@pm,@km,@ob)", con);
    //        c.Parameters.AddWithValue("@sk", stok_kodu);
    //        c.Parameters.AddWithValue("@sa", stok_adi);
    //        c.Parameters.AddWithValue("@m", marka);
    //        c.Parameters.AddWithValue("@uk", uretici_kodu);
    //        c.Parameters.AddWithValue("@ok", onceki_kod);
    //        c.Parameters.AddWithValue("@k1", kull1s);
    //        c.Parameters.AddWithValue("@k2", kull2s);
    //        c.Parameters.AddWithValue("@k3", kull3s);
    //        c.Parameters.AddWithValue("@k4", kull4s);
    //        c.Parameters.AddWithValue("@k5", kull5s);
    //        c.Parameters.AddWithValue("@k6", kull6s);
    //        c.Parameters.AddWithValue("@k7", kull7s);
    //        c.Parameters.AddWithValue("@k8", kull8s);
    //        c.Parameters.AddWithValue("@ru", resim_url);
    //        c.Parameters.AddWithValue("@ol", oem_liste);
    //        c.Parameters.AddWithValue("@el", esdegerListe);
    //        c.Parameters.AddWithValue("@f", fiyat);
    //        c.Parameters.AddWithValue("@v", varyok);
    //        c.Parameters.AddWithValue("@b1", barkod1);
    //        c.Parameters.AddWithValue("@b2", barkod2);
    //        c.Parameters.AddWithValue("@b3", barkod3);
    //        c.Parameters.AddWithValue("@ko", kampanyaOrani);
    //        c.Parameters.AddWithValue("@pm", paketMiktari);
    //        c.Parameters.AddWithValue("@km", koliMiktari);
    //        c.Parameters.AddWithValue("@ob", olcuBirimi);
    //        // return "insert into " + tabloAdı + "(stok_kodu, stok_adi, marka, uretici_kodu, onceki_kod, kull1s, kull2s, kull3s, kull4s, kull5s, kull6s, kull7s, kull8s, resim_url, oem_liste, esdegerListe, fiyat, varyok, barkod1, barkod2, barkod3, kampanyaOrani, paketMiktari, koliMiktari,  olcuBirimi) values(\"" + stok_kodu + "\",\"" + stok_adi + "\",\"" + marka + "\",\"" + uretici_kodu + "\",\"" + onceki_kod + "\",\"" + kull1s + "\",\"" + kull2s + "\",\"" + kull3s + "\",\"" + kull4s + "\",\"" + kull5s + "\",\"" + kull6s + "\",\"" + kull7s + "\",\"" + kull8s + "\",\"" + resim_url + "\",\"" + oem_liste + "\",\"" + esdegerListe + "\"," + x + ",\"" + varyok + "\",\"" + barkod1 + "\",\"" + barkod2 + "\",\"" + barkod3 + "\"," + kampanyaOrani.ToString() + "," + paketMiktari.ToString() + "," + koliMiktari.ToString() + ",\"" + olcuBirimi + "\")";
    //        return c;
    //    }
    //}

    //public static class veriÇekme
    //{
    //    public static Thread m;
    //    public static Thread d;

    //    static string m_companyKey = "954FCD2D";
    //    static string m_functionName = "GetProductList_Atamer";
    //    static string m_userName = "atamer_motorasin";
    //    static string m_password = "dDKs3dfyQH";
    //    static string m_dataType = "xml";
    //    static int m_SonLimit = 120000;
    //    static int m_Paket = 5000;
    //    static int dinamikBeklemeSüresi = 5000;

    //    public static Label motoraşinBilgilendirme;
    //    public static Label dinamikBilgilendirme;

    //    public static void motoraşinVeriÇekmeyeBaşla()
    //    {
    //        if (m != null) if (m.IsAlive) return;
    //        m = new Thread(() => MotoraşinVerileriniÇek(motoraşinBilgilendirme));
    //        m.IsBackground = true;
    //        //motoraşinVeriÇekiliyor = true;
    //        m.Start();
    //    }
    //    public static void dinamikVeriÇekmeyeBaşla()
    //    {
    //        if (d != null) if (d.IsAlive) return;
    //        d = new Thread(() => DinamikVerileriniÇek(dinamikBilgilendirme));
    //        d.IsBackground = true;
    //        d.Start();
    //    }

    //    static IRestResponse veriÇekMotoraşin(int başlama, int bitiş, string function_name = "")
    //    {
    //        var jSonYapi = new { companyKey = m_companyKey, functionName = (function_name == "" ? m_functionName : function_name), userName = m_userName, password = m_password, dataType = m_dataType, parameters = new { pStart = başlama, pEnd = bitiş } };
    //        var json = JsonConvert.SerializeObject(jSonYapi);
    //        var client = new RestClient("http://share.eryaz.net/api/integration/getdata");
    //        var request = new RestRequest(Method.POST);
    //        request.AddHeader("cache-control", "no-cache");
    //        request.AddHeader("content-type", "application/json");
    //        request.AddHeader("Accept", "application/json");
    //        request.AddParameter("application/json", json, ParameterType.RequestBody);
    //        IRestResponse response = client.Execute(request);
    //        return response;
    //    }
    //    static IRestResponse markaListesiÇekDinamik()
    //    {
    //        var client = new RestClient("https://kokpit.dinamik.online:8181/operation/getBrandList?api_username=atamer&api_password=Ata20mer20*!*");
    //        var request = new RestRequest(Method.GET);
    //        IRestResponse response = client.Execute(request);
    //        return response;
    //    }
    //    static IRestResponse markaVeriÇekDinamik(string Marka)
    //    {
    //        var client = new RestClient("https://kokpit.dinamik.online:8181/operation/getStockList?api_username=atamer&api_password=Ata20mer20*!*&api_marka=" + Marka);
    //        var request = new RestRequest(Method.GET);
    //        IRestResponse response = client.Execute(request);
    //        return response;
    //    }

    //    //static public void MotoraşinVerileriniÇek(Label motoraşinBilgilendirme)
    //    //{
    //    //    int sonLimit = m_SonLimit;
    //    //    int paket = m_Paket;

    //    //    int parti = (sonLimit / paket) + (sonLimit % paket == 0 ? 0 : 1);
    //    //    IRestResponse x;

    //    //    x = veriÇekMotoraşin(1, 50);
    //    //    dynamic j = JsonConvert.DeserializeObject(x.Content);

    //    //    if (!(bool)j["Status"].Value) return;

    //    //    MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
    //    //    con.Open();

    //    //    bool tabloTemizlendi = false;
    //    //    for (int i = 0; i < parti; i++)
    //    //    {
    //    //        int baş = i * paket - 1;
    //    //        if (baş < 0) baş = 0;
    //    //        int bit = (((i + 1) * paket) > sonLimit ? sonLimit : ((i + 1) * paket)) + 1;

    //    //        motoraşinBilgilendirme.Text = (i + 1).ToString() + "/" + parti.ToString() + " veri çekiliyor..";

    //    //        x = veriÇekMotoraşin(baş, bit);

    //    //        if (!tabloTemizlendi)
    //    //        {
    //    //            veritabanı.tabloOluştur_idAuto(veritabanı.MotoraşinTablosu, veritabanı.MotoraşinTabloYapısı, veritabanı.MotoraşinTablosuIndexYapısı, con);
    //    //            veriYazma.yazmayaBaşlaMotoraşin();
    //    //            tabloTemizlendi = true;
    //    //        }

    //    //        dynamic jsonData = JsonConvert.DeserializeObject(x.Content);
    //    //        veriYazma.yazılacakEkleMotoraşin(jsonData);
    //    //    }
    //    //    con.Close();
    //    //    veriYazma.yazmayıBitirMotoraşin();
    //    //    motoraşinBilgilendirme.Text = "Motoraşin verileri çekildi. Yazma işleminin bitmesi bekleniyor..";
    //    //    veriYazma.m.Join();
    //    //    //motoraşinVeriÇekiliyor = false;
    //    //    motoraşinBilgilendirme.Text = "Motoraşin verileri çekildi ve veritabanına kaydedildi..";
    //    //}
    //    //static public void DinamikVerileriniÇek(Label dinamikBilgilendirme)
    //    //{
    //    //    dinamikBilgilendirme.Text = "Marka Listesi Çekiliyor..";
    //    //    dinamikBilgilendirme.Refresh();
    //    //    IRestResponse x = markaListesiÇekDinamik();
    //    //    DateTime t = DateTime.Now;
    //    //    if (!x.IsSuccessful)
    //    //    {
    //    //        MessageBox.Show("Dinamikten veri çekilemiyor.\nHata mesajı:\n" + x.ErrorMessage, "Hata !!", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //    //        dinamikBilgilendirme.Text = "Dinamik verileri çekilemedi.";
    //    //        return;
    //    //    }

    //    //    dinamikBilgilendirme.Text = "Markalar çekildi. Kontrol ediliyor...";
    //    //    dinamikBilgilendirme.Refresh();

    //    //    MySqlConnection con = new MySqlConnection(veritabanı.connectionString);

    //    //    con.Open();

    //    //    MySqlCommand cmd = new MySqlCommand();
    //    //    cmd.Connection = con;
    //    //    cmd.Parameters.Add("@marka", MySqlDbType.VarChar);
    //    //    cmd.CommandText = "INSERT INTO " + veritabanı.DinamikMarkalarTablosu + "(marka) SELECT * FROM (SELECT @marka) AS tmp WHERE NOT EXISTS(SELECT marka FROM " + veritabanı.DinamikMarkalarTablosu + " WHERE marka = @marka) LIMIT 1";

    //    //    dynamic jsonData = JsonConvert.DeserializeObject(x.Content);
    //    //    int adet = 0;
    //    //    string marka;
    //    //    string eklenenMarkalar = "";
    //    //    int d;
    //    //    foreach (var item in jsonData["brandlist"])
    //    //    {
    //    //        marka = item["brand"];
    //    //        cmd.Parameters["@marka"].Value = marka;
    //    //        d = cmd.ExecuteNonQuery();
    //    //        if (d > 0)
    //    //        {
    //    //            eklenenMarkalar += (eklenenMarkalar != "" ? ", " : "") + marka;
    //    //            adet++;
    //    //        }
    //    //    }

    //    //    if (adet > 0)
    //    //        if (MessageBox.Show("Dinamikten çekilecek markalar içerisine " + adet.ToString() + " adet yeni marka eklendi ve onların verileri de çekilecek.\nBu markaları görüntülemek ister misiniz ?", "Yeni markalar bulundu !!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
    //    //            MessageBox.Show(eklenenMarkalar);

    //    //    dinamikBilgilendirme.Text = "Marka liste tamam..";
    //    //    dinamikBilgilendirme.Refresh();



    //    //    veritabanı.tabloOluştur_idAuto(veritabanı.DinamikTablosu, veritabanı.DinamikTabloYapısı, veritabanı.DinamikTablosuIndexYapısı, con);

    //    //    string stokKodu = "";
    //    //    int başarılı = -1;
    //    //    int bekleme;

    //    //    cmd.CommandText = "select marka from " + veritabanı.DinamikMarkalarTablosu + " where cekilecek IS TRUE";
    //    //    MySqlDataReader dr = cmd.ExecuteReader();
    //    //    List<string> markalar = new List<string>();
    //    //    while (dr.Read())
    //    //    {
    //    //        markalar.Add((string)dr.GetValue(0));
    //    //    }
    //    //    dr.Close();

    //    //    cmd.CommandText = "UPDATE " + veritabanı.DinamikMarkalarTablosu + " SET cekilecek='0' WHERE marka=@marka";
    //    //    veriYazma.yazmayaBaşlaDinamik();

    //    //    bekleme = dinamikBeklemeSüresi - (int)DateTime.Now.Subtract(t).TotalMilliseconds;
    //    //    if (bekleme > 0) Thread.Sleep(bekleme);

    //    //    for (int i = 0; i < markalar.Count; i++)
    //    //    {
    //    //        marka = markalar[i];

    //    //        bekleme = dinamikBeklemeSüresi;
    //    //        dinamikBilgilendirme.Text = marka + " çekiliyor..";
    //    //        dinamikBilgilendirme.Refresh();
    //    //        stokKodu = "ERR";
    //    //        başarılı = -1;
    //    //        while (true)
    //    //        {
    //    //            IRestResponse data = veriÇekme.markaVeriÇekDinamik(marka);
    //    //            jsonData = JsonConvert.DeserializeObject(data.Content);
    //    //            jsonData = jsonData["stockList"];
    //    //            if (jsonData.First == null) { başarılı = 0; break; }
    //    //            stokKodu = jsonData.First["stok_kodu"].Value;
    //    //            if (stokKodu != "ERR") { başarılı = 1; break; }
    //    //            if (bekleme < 5000)
    //    //                bekleme = 5000;
    //    //            else if (bekleme < 10000)
    //    //                bekleme = 10000;
    //    //            else if (bekleme < 20000)
    //    //                bekleme = 20000;
    //    //            else if (bekleme < 30000)
    //    //                bekleme = 30000;
    //    //            else
    //    //            {
    //    //                başarılı = -1;
    //    //                break;
    //    //            }
    //    //            Thread.Sleep(bekleme);
    //    //        }
    //    //        if (başarılı < 1)
    //    //        {
    //    //            dinamikBilgilendirme.Text = marka + " BOŞ. Çekilecekler listesinde değeri 'False' yapılıyor.";
    //    //            cmd.Parameters["@marka"].Value = marka;
    //    //            cmd.ExecuteNonQuery();
    //    //            continue;
    //    //        }
    //    //        dinamikBilgilendirme.Text = marka + " OK.";
    //    //        dinamikBilgilendirme.Refresh();

    //    //        veriYazma.yazılacakEkleDinamik(jsonData);

    //    //        dinamikBilgilendirme.Text = "Sonraki veri çekme için bekleniyor..";
    //    //        dinamikBilgilendirme.Refresh();
    //    //        Thread.Sleep(bekleme);
    //    //    }
    //    //    con.Close();
    //    //    veriYazma.yazmayıBitirDinamik();
    //    //    dinamikBilgilendirme.Text = "Dinamik verileri çekildi. Yazma işleminin bitmesi bekleniyor..";
    //    //    veriYazma.d.Join();
    //    //    dinamikBilgilendirme.Text = "Dinamik verileri çekildi ve veritabanına kaydedildi..";
    //    //}
    //}
    //public static class veriYazma
    //{
    //    public static Thread m;
    //    public static Thread d;

    //    static bool işlemVarMotoraşin = false;
    //    static bool işlemVarDinamik = false;

    //    static List<dynamic> yazılacakPaketMotoraşin = new List<dynamic>();
    //    static List<dynamic> yazılacakPaketDinamik = new List<dynamic>();

    //    public static void yazılacakEkleMotoraşin(dynamic eklenecek)
    //    {
    //        lock (yazılacakPaketMotoraşin) { yazılacakPaketMotoraşin.Add(eklenecek); }
    //    }
    //    public static void yazılacakEkleDinamik(dynamic eklenecek)
    //    {
    //        lock (yazılacakPaketDinamik) { yazılacakPaketDinamik.Add(eklenecek); }
    //    }

    //    public static void yazmayaBaşlaMotoraşin()
    //    {
    //        if (işlemVarMotoraşin) return;
    //        m = new Thread(yazmaİşlemiMotoraşin);
    //        m.IsBackground = true;
    //        işlemVarMotoraşin = true;
    //        m.Start();
    //    }
    //    public static void yazmayaBaşlaDinamik()
    //    {
    //        if (işlemVarDinamik) return;
    //        d = new Thread(yazmaİşlemiDinamik);
    //        d.IsBackground = true;
    //        işlemVarDinamik = true;
    //        d.Start();
    //    }

    //    public static void yazmayıBitirMotoraşin()
    //    {
    //        işlemVarMotoraşin = false;
    //    }
    //    public static void yazmayıBitirDinamik()
    //    {
    //        işlemVarDinamik = false;
    //    }

    //    static void yazmaİşlemiMotoraşin()
    //    {
    //        MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
    //        con.Open();
    //        Motoraşin_kayıt_sınıfı m;
    //        while (true)
    //        {
    //            while (yazılacakPaketMotoraşin.Count > 0)
    //            {
    //                foreach (var item in yazılacakPaketMotoraşin[0]["Data"])
    //                {
    //                    m = new Motoraşin_kayıt_sınıfı(item);
    //                    m.insertCommand(veritabanı.MotoraşinTablosu, con).ExecuteNonQuery();
    //                }
    //                lock (yazılacakPaketMotoraşin) { yazılacakPaketMotoraşin.RemoveAt(0); }
    //            }
    //            if (!işlemVarMotoraşin) break;
    //            Thread.Sleep(1000);
    //        }
    //        con.Close();
    //    }
    //    static void yazmaİşlemiDinamik()
    //    {
    //        MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
    //        con.Open();
    //        Dinamik_kayıt_sınıfı d;
    //        while (true)
    //        {
    //            while (yazılacakPaketDinamik.Count > 0)
    //            {
    //                foreach (var item in yazılacakPaketDinamik[0])
    //                {
    //                    d = new Dinamik_kayıt_sınıfı(item);
    //                    d.insertCommand(veritabanı.DinamikTablosu, con).ExecuteNonQuery();
    //                }
    //                lock (yazılacakPaketDinamik) { yazılacakPaketDinamik.RemoveAt(0); }
    //            }
    //            if (!işlemVarDinamik) break;
    //            Thread.Sleep(1000);
    //        }
    //        con.Close();
    //    }
    //}

    //public class alanSınıfı
    //{
    //    bool _stokmiktarı;
    //    bool _fiyat;
    //    bool _fiyatcinsi;
    //    bool _resim;
    //    bool _label;

    //    bool _boşAlan;
    //    bool _sabitLabel;

    //    bool _kategoriBilgileriniAl;

    //    bool _fiyatıOlanKayıtlar;
    //    string _işleç;
    //    int _fiyatı;
    //    bool _stoktaOlanKayıtlar;
    //    bool _resmiOlanKayıtlar;

    //    public alanSınıfı(bool stokmiktarı, bool fiyat, bool fiyatcinsi, bool resim, bool label, bool boşAlan, bool sabitLabel, bool kategoriBilgileriniAl, bool fiyatıOlanKayıtlar, string işlec, int fiyatı, bool stoktaOlanKayıtlar, bool resmiOlanKayıtlar)
    //    {
    //        _stokmiktarı = stokmiktarı;
    //        _fiyat = fiyat;
    //        _fiyatcinsi = fiyatcinsi;
    //        _resim = resim;
    //        _label = label;
    //        _boşAlan = boşAlan;
    //        _sabitLabel = sabitLabel;
    //        _kategoriBilgileriniAl = kategoriBilgileriniAl;
    //        _fiyatıOlanKayıtlar = fiyatıOlanKayıtlar;
    //        _işleç = işlec;
    //        _fiyatı = fiyatı;
    //        _stoktaOlanKayıtlar = stoktaOlanKayıtlar;
    //        _resmiOlanKayıtlar = resmiOlanKayıtlar;
    //    }
    //    public alanSınıfı(alanSınıfı seçenekler)
    //    {
    //        _stokmiktarı = seçenekler.stokMiktarı;
    //        _fiyat = seçenekler._fiyat;
    //        _fiyatcinsi = seçenekler.fiyatCinsi;
    //        _resim = seçenekler._resim;
    //        _label = seçenekler._label;
    //        _boşAlan = seçenekler._boşAlan;
    //        _sabitLabel = seçenekler.sabitLabel;
    //        _kategoriBilgileriniAl = seçenekler._kategoriBilgileriniAl;
    //        _fiyatıOlanKayıtlar = seçenekler._fiyatıOlanKayıtlar;
    //        _işleç = seçenekler._işleç;
    //        _fiyatı = seçenekler._fiyatı;
    //        _stoktaOlanKayıtlar = seçenekler._stoktaOlanKayıtlar;
    //        _resmiOlanKayıtlar = seçenekler._resmiOlanKayıtlar;
    //    }

    //    public bool tümü { get { return !(_stokmiktarı || _fiyatcinsi || _fiyat || _resim || _label); } }
    //    public bool stokMiktarı { get { return _stokmiktarı; } }
    //    public bool fiyat { get { return _fiyat; } }
    //    public bool resim { get { return _resim; } }
    //    public bool label { get { return _label; } }

    //    public bool fiyatCinsi { get { return _fiyatcinsi; } }
    //    public bool boşAlanAlınsın { get { return !_boşAlan; } }
    //    public bool alanAlınsın(string kategoriAdı)
    //    {
    //        if (tümü || boşAlanAlınsın) return true;
    //        else
    //        {
    //            switch (kategoriAdı)
    //            {
    //                case "stockCode": return true;
    //                case "stockAmount": if (_stokmiktarı) return true; break;
    //                case "price1": if (_fiyat) return true; break;
    //                case "currencyAbbr": if (_fiyatcinsi) return true; break;
    //                case "category_path": if (_kategoriBilgileriniAl) return true; break;
    //                case "picture1Path": if (_resim) return true; break;
    //                case "label": if (_label) return true; break;
    //                default: return false;
    //            }
    //            return false;
    //            //if (kategoriAdı ==      "stockCode") return true;
    //            //else if (kategoriAdı == "stockAmount" && _stokmiktarı) return true;
    //            //else if (kategoriAdı == "price1" && _fiyat) return true;
    //            //else if (kategoriAdı == "currencyAbbr" && _fiyatcinsi) return true;
    //            //else if (kategoriAdı == "category_path" && _kategoriBilgileriniAl) return true;
    //            //else if (kategoriAdı == "picture1Path" && _resim) return true;
    //            //else if (kategoriAdı == "label" && _label) return true;
    //            //else return false;
    //        }
    //    }
    //    public bool sabitLabel { get { return _sabitLabel; } }
    //    public bool kategoriBilgileriAlınsın { get { return tümü || _kategoriBilgileriniAl; } }

    //    public bool fiyatıOlanKayıtlar { get { return _fiyatıOlanKayıtlar; } }
    //    public bool fiyatUygun(double fiyat, int paketMiktarı = 1)
    //    {
    //        switch (_işleç)
    //        {
    //            case ">": return fiyat * paketMiktarı > _fiyatı;
    //            case ">=": return fiyat * paketMiktarı >= _fiyatı;
    //            case "<": return fiyat * paketMiktarı < _fiyatı;
    //            case "<=": return fiyat * paketMiktarı <= _fiyatı;
    //            case "=": return fiyat * paketMiktarı == _fiyatı;
    //            case "!=": return fiyat * paketMiktarı != _fiyatı;
    //            default: return false;
    //        }
    //    }

    //    public bool stoktaOlanKayıtlar { get { return _stoktaOlanKayıtlar; } }
    //    public bool resmiOlanKayıtlar { get { return _resmiOlanKayıtlar; } }

    //}
    //public class değişecekAlan
    //{
    //    public string marka;
    //    public string[] eskideğerlerTam = new string[0];
    //    public string[] yenideğerlerTam = new string[0];
    //    public string[] eskideğerlerKısmi = new string[0];
    //    public string[] yenideğerlerKısmi = new string[0];

    //    public değişecekAlan(string marka_) { marka = marka_; }

    //    public string getir(string aranan)
    //    {
    //        int yer = -1;
    //        for (int i = 0; i < eskideğerlerTam.Length; i++) if (eskideğerlerTam[i] == aranan) { yer = i; break; }
    //        if (yer == -1)
    //        {
    //            for (int i = 0; i < eskideğerlerKısmi.Length; i++) aranan = aranan.Replace(eskideğerlerKısmi[i], yenideğerlerKısmi[i]);
    //            return aranan;
    //        }
    //        else return yenideğerlerTam[yer];
    //    }
    //    public void ekle(string eski, string yeni, bool kısmieşleşme = false)
    //    {
    //        if (kısmieşleşme)
    //        {
    //            eski = eski.Substring(1, eski.Length - 1);
    //            Array.Resize(ref eskideğerlerKısmi, eskideğerlerKısmi.Length + 1);
    //            Array.Resize(ref yenideğerlerKısmi, yenideğerlerKısmi.Length + 1);
    //            eskideğerlerKısmi[eskideğerlerKısmi.Length - 1] = eski;
    //            yenideğerlerKısmi[yenideğerlerKısmi.Length - 1] = yeni;
    //        }
    //        else
    //        {
    //            Array.Resize(ref eskideğerlerTam, eskideğerlerTam.Length + 1);
    //            Array.Resize(ref yenideğerlerTam, yenideğerlerTam.Length + 1);
    //            eskideğerlerTam[eskideğerlerTam.Length - 1] = eski;
    //            yenideğerlerTam[yenideğerlerTam.Length - 1] = yeni;
    //        }
    //    }

    //}
    //public class değişimClass
    //{
    //    public değişecekAlan[] alanlar = new değişecekAlan[0];
    //    public void ekle(string alan, string eski, string yeni)
    //    {
    //        foreach (değişecekAlan item in alanlar)
    //            if (item.marka == alan)
    //            {
    //                item.ekle(eski, yeni, eski.StartsWith("%"));
    //                return;
    //            }
    //        Array.Resize(ref alanlar, alanlar.Length + 1);
    //        alanlar[alanlar.Length - 1] = new değişecekAlan(alan);
    //        ekle(alan, eski, yeni);
    //    }
    //    public string getir(string alan, string değer)
    //    {
    //        foreach (değişecekAlan item in alanlar)
    //            if (item.marka == alan)
    //                return item.getir(değer);
    //        return değer;
    //    }
    //}
    //public class XML_Yazma_Sınıfı
    //{
    //    FileInfo yazılacakDosya;
    //    int dosyaSayaç = 0;
    //    bool dosyaTek;
    //    FileStream dosya;
    //    StreamWriter writer;
    //    Encoding kodlama;
    //    long fileSize;
    //    public XML_Yazma_Sınıfı(FileInfo _yazılacakDosya, bool _dosyaTek, long _fileSize, Encoding _kodlama)
    //    {
    //        yazılacakDosya = _yazılacakDosya;
    //        dosyaTek = _dosyaTek;
    //        kodlama = _kodlama;
    //        fileSize = _fileSize;
    //    }
    //    public void yazmaBaşladı()
    //    {
    //        FileInfo file = yazılacakDosya;
    //        dosyaSayaç++;
    //        if (!dosyaTek)
    //            file = new FileInfo(file.DirectoryName + "\\" + file.Name.Split('.')[0] + dosyaSayaç.ToString() + file.Extension);
    //        dosya = new FileStream(file.FullName, FileMode.Create);
    //        //Encoding kodlama = (rbKodlama8.Checked ? Encoding.UTF8 : (rbKodlama16.Checked ? Encoding.Unicode : (rbKodlama32.Checked ? Encoding.UTF32 : Encoding.UTF8)));
    //        writer = new StreamWriter(dosya, kodlama);
    //        writer.Write("<?xml version='1.0' encoding='" + kodlama.WebName + "'?>\n<root>\n");
    //    }
    //    public void yazmaBitti()
    //    {
    //        writer.Write("</root>");
    //        writer.Close();
    //        dosya.Close();
    //    }
    //    public void yaz(string yazı)
    //    {
    //        if (writer.BaseStream.Length > fileSize)
    //        {
    //            yazmaBitti();
    //            yazmaBaşladı();
    //            writer.Write(yazı);
    //        }
    //        else
    //        {
    //            writer.Write(yazı);
    //        }
    //    }
    //}
    //public class Miktar_Fiyat_Güncelleme_Sınıfı
    //{

    //}
    //public class sabitLabelSınıfı
    //{
    //    FileStream dosya;
    //    StreamWriter w;
    //    StreamReader r;
    //    List<string> satırlar = new List<string>();
    //    Thread sabitLabelYazma;
    //    string _dosyaAdı;

    //    public static string[] sabitLabel_stockCode;
    //    public static string[] sabitLabel_label;
    //    Thread sabitLabelOkuma;

    //    public bool çalışan_işlem_var()
    //    {
    //        return sabitLabelOkuma.IsAlive;
    //    }

    //    public void dosyalarıKapat()
    //    {
    //        w.Close();
    //        dosya.Close();
    //    }
    //    void sabitLabelOku()
    //    {
    //        string satır = "";
    //        string[] ss;
    //        while ((satır = r.ReadLine()) != null)
    //        {
    //            if (satır.StartsWith("//")) continue;
    //            ss = satır.Split('½');
    //            if (ss.Length != 2) continue;

    //            Array.Resize(ref sabitLabel_stockCode, sabitLabel_stockCode.Length + 1);
    //            sabitLabel_stockCode[sabitLabel_stockCode.Length - 1] = ss[0];
    //            Array.Resize(ref sabitLabel_label, sabitLabel_label.Length + 1);
    //            sabitLabel_label[sabitLabel_label.Length - 1] = ss[1];
    //        }
    //        r.Close();
    //        dosya.Close();
    //        sabitLabelYazma = new Thread(yaz);
    //        sabitLabelYazma.IsBackground = true;
    //        sabitLabelYazma.Start();
    //    }
    //    public sabitLabelSınıfı(string dosyaAdı)
    //    {
    //        _dosyaAdı = dosyaAdı;
    //        Array.Resize(ref sabitLabel_stockCode, 0);
    //        Array.Resize(ref sabitLabel_label, 0);
    //        FileInfo f = new FileInfo(dosyaAdı);
    //        if (!f.Exists)
    //        {
    //            dosya = new FileStream(dosyaAdı, FileMode.Create);
    //            dosya.Close();
    //        }
    //        else
    //        {
    //            dosya = new FileStream(dosyaAdı, FileMode.Open);
    //            r = new StreamReader(dosya, Encoding.UTF8);
    //            sabitLabelOkuma = new Thread(sabitLabelOku);
    //            sabitLabelOkuma.IsBackground = true;
    //            sabitLabelOkuma.Start();

    //        }

    //    }
    //    ~sabitLabelSınıfı()
    //    {
    //        if (sabitLabelYazma != null) if (sabitLabelYazma.IsAlive) sabitLabelYazma.Abort();
    //    }
    //    void yaz()
    //    {
    //        if (!dosya.CanWrite)
    //        {
    //            dosya = new FileStream(_dosyaAdı, FileMode.Append);
    //            w = new StreamWriter(dosya, Encoding.UTF8);
    //        }
    //        string s;
    //        while (true)
    //        {
    //            while (satırlar.Count > 0)
    //            {
    //                lock (satırlar)
    //                {
    //                    s = satırlar[0];
    //                    satırlar.RemoveAt(0);
    //                }
    //                w.WriteLine(s);
    //            }
    //            Thread.Sleep(1000);
    //        }
    //    }
    //    public void ekle(string satır)
    //    {
    //        lock (satırlar)
    //        {
    //            satırlar.Add(satır);
    //        }
    //    }
    //}
    //public class Item
    //{
    //    static alanSınıfı _seçenek;
    //    public static alanSınıfı seçenek { get { return _seçenek; } set { _seçenek = new alanSınıfı(value); } }

    //    static değişimClass değişim = new değişimClass();
    //    public static string[] stokKoduMotoraşin = new string[0]; //motoraşinden veriler çift geliyor.
    //                                                              ////static string[] dinamikDeğişim_alanları;


    //    //ToolStripProgressBar p1;
    //    //ToolStripStatusLabel l1;


    //    public Item(/*ToolStripProgressBar p1, ToolStripStatusLabel l1*/)
    //    {
    //        FileInfo f = new FileInfo(".\\Genel\\değişim.txt");
    //        if (f.Exists)
    //        {
    //            FileStream file = new FileStream(f.FullName, FileMode.Open);
    //            StreamReader okuyucu = new StreamReader(file, Encoding.UTF8);
    //            string satır = "";
    //            string[] ss;
    //            string alan, eski, yeni;
    //            while ((satır = okuyucu.ReadLine()) != null)
    //            {
    //                if (satır.StartsWith("//")) continue;
    //                ss = satır.Split(';');
    //                if (ss.Length != 2) continue;
    //                yeni = ss[1];
    //                ss = ss[0].Split('½');
    //                if (ss.Length != 2) continue;
    //                eski = ss[1];
    //                alan = ss[0];
    //                değişim.ekle(alan, eski, yeni);
    //            }
    //            okuyucu.Close();
    //            file.Close();

    //        }
    //    }

    //    public bool yazılmalıMı()
    //    {
    //        bool sonuç = true;
    //        if (seçenek.fiyatıOlanKayıtlar)
    //        {
    //            double fiyat = 0.0;
    //            try
    //            {
    //                if (price1.IndexOf('.') > -1) fiyat = double.Parse(price1, new NumberFormatInfo() { NumberDecimalSeparator = "." });
    //                else fiyat = double.Parse(price1);

    //            }
    //            catch { }

    //            sonuç = sonuç && seçenek.fiyatUygun(fiyat, paketMiktarı);
    //        }
    //        if (seçenek.stoktaOlanKayıtlar)
    //        {
    //            double miktar = 0.0;
    //            try
    //            {
    //                miktar = double.Parse(stockAmount);

    //            }
    //            catch { }
    //            sonuç = sonuç && miktar > 0;
    //        }
    //        if (seçenek.resmiOlanKayıtlar) sonuç = sonuç && picture1Path != "" && picture1Path != null;
    //        return sonuç;
    //    }
    //    string toXmlTag(string başlık, string içerik, bool düzeltilsin = true)
    //    {
    //        return "<" + başlık + ">" + (düzeltilsin ? düzelt(içerik) : içerik) + "</" + başlık + ">";
    //    }
    //    string düzelt(string içerik)
    //    {
    //        ///&lt;  	< 	less than
    //        ///&gt;     > 	greater than
    //        ///&amp; 	& 	ampersand 
    //        ///&apos; 	' 	apostrophe
    //        ///&quot; 	" 	quotation mark
    //        if (içerik == null) return null;
    //        içerik = içerik.Replace("&", "&amp;");
    //        içerik = içerik.Replace("<", "&lt;");
    //        içerik = içerik.Replace(">", "&gt;");
    //        içerik = içerik.Replace("'", "&apos;");
    //        içerik = içerik.Replace("\"", "&quot;");
    //        return içerik;
    //    }

    //    public string toXmlString(string başlık = "item")
    //    {
    //        return "<" + başlık + ">" + (seçenek.alanAlınsın("stockCode") ? toXmlTag(nameof(stockCode), stockCode) : "") + (seçenek.alanAlınsın("label") ? toXmlTag(nameof(label), label) : "") + (seçenek.alanAlınsın("status") ? toXmlTag(nameof(status), status) : "") + (seçenek.alanAlınsın("brand") ? toXmlTag(nameof(brand), brand) : "") + (seçenek.alanAlınsın("brandId") ? toXmlTag(nameof(brandId), brandId) : "") + (seçenek.alanAlınsın("brandDistCode") ? toXmlTag(nameof(brandDistCode), brandDistCode) : "") + (seçenek.alanAlınsın("barcode") ? toXmlTag(nameof(barcode), barcode) : "") + (seçenek.alanAlınsın("category_path") ? toXmlTag(nameof(category_path), category_path) : "") + (seçenek.alanAlınsın("buyingPrice") ? toXmlTag(nameof(buyingPrice), buyingPrice) : "") + (seçenek.alanAlınsın("price1") || seçenek.fiyat ? toXmlTag(nameof(price1), price1.Replace(',', '.')) : "") + (seçenek.alanAlınsın("price2") ? toXmlTag(nameof(price2), price2) : "") + (seçenek.alanAlınsın("price3") ? toXmlTag(nameof(price3), price3) : "") + (seçenek.alanAlınsın("price4") ? toXmlTag(nameof(price4), price4) : "") + (seçenek.alanAlınsın("price5") ? toXmlTag(nameof(price5), price5) : "") + (seçenek.alanAlınsın("tax") ? toXmlTag(nameof(tax), tax) : "") + (seçenek.alanAlınsın("currencyAbbr") || seçenek.fiyatCinsi ? toXmlTag(nameof(currencyAbbr), currencyAbbr) : "") + (seçenek.alanAlınsın("stockAmount") || seçenek.stokMiktarı ? toXmlTag(nameof(stockAmount), stockAmount) : "") + (seçenek.alanAlınsın("stockType") ? toXmlTag(nameof(stockType), stockType) : "") + (seçenek.alanAlınsın("warranty") ? toXmlTag(nameof(warranty), warranty) : "") + (seçenek.alanAlınsın("picture1Path") ? toXmlTag(nameof(picture1Path), picture1Path) : "") + (seçenek.alanAlınsın("picture2Path") ? toXmlTag(nameof(picture2Path), picture2Path) : "") + (seçenek.alanAlınsın("picture3Path") ? toXmlTag(nameof(picture3Path), picture3Path) : "") + (seçenek.alanAlınsın("picture4Path") ? toXmlTag(nameof(picture4Path), picture4Path) : "") + (seçenek.alanAlınsın("dm3") ? toXmlTag(nameof(dm3), dm3) : "") + (seçenek.alanAlınsın("details") ? toXmlTag(nameof(details), details) : "") + (seçenek.alanAlınsın("rebate") ? toXmlTag(nameof(rebate), rebate) : "") + (seçenek.alanAlınsın("rebateType") ? toXmlTag(nameof(rebateType), rebateType) : "") + (seçenek.alanAlınsın("shortdetails") ? toXmlTag(nameof(shortdetails), shortdetails) : "") + (seçenek.alanAlınsın("title") ? toXmlTag(nameof(title), title) : "") + (seçenek.alanAlınsın("keywords") ? toXmlTag(nameof(keywords), keywords) : "") + (seçenek.alanAlınsın("descriptions") ? toXmlTag(nameof(descriptions), descriptions) : "") + (seçenek.alanAlınsın("searchKeywords") ? toXmlTag(nameof(searchKeywords), searchKeywords) : "") + (seçenek.alanAlınsın("seoTagName") ? toXmlTag(nameof(seoTagName), seoTagName) : "") + (seçenek.alanAlınsın("metaTitle") ? toXmlTag(nameof(metaTitle), metaTitle) : "") + (seçenek.alanAlınsın("metaDescription") ? toXmlTag(nameof(metaDescription), metaDescription) : "") + (seçenek.alanAlınsın("metaKeywords") ? toXmlTag(nameof(metaKeywords), metaKeywords) : "") + (seçenek.alanAlınsın("productSpecialInfoTitle") ? toXmlTag(nameof(productSpecialInfoTitle), productSpecialInfoTitle) : "") + (seçenek.alanAlınsın("productSpecialInfoContent") ? toXmlTag(nameof(productSpecialInfoContent), productSpecialInfoContent) : "") + (seçenek.alanAlınsın("variants") ? toXmlTag(nameof(variants), variants) : "") + (seçenek.alanAlınsın("specs") ? toXmlTag("specs", toXmlTag("spec", toXmlTag(nameof(specGroup), specGroup) + toXmlTag(nameof(specName), specName) + toXmlTag(nameof(specValue), specValue), false), false) : "") + " </" + başlık + ">\n";
    //    }
    //    public string virgüllerİptal(string s)
    //    {
    //        string[] ss = s.Split(',');
    //        s = "";
    //        foreach (string item in ss)
    //        {
    //            if (item.Trim() == "") continue;
    //            s += (s == "" ? "" : " ") + item.Trim();
    //        }
    //        return s;
    //    }

    //    public string motoraşinden_içeAktar(dynamic v)
    //    {
    //        if (Array.IndexOf(stokKoduMotoraşin, v["ManufacturerCode"].Value) > -1) return "-1";
    //        Array.Resize(ref stokKoduMotoraşin, stokKoduMotoraşin.Length + 1);
    //        stokKoduMotoraşin[stokKoduMotoraşin.Length - 1] = v["ManufacturerCode"].Value;

    //        string dönecekLabelDeğeri = "";

    //        foreach (var x in v) x.Value = değişim.getir(x.Name, (string)x.Value);

    //        stockCode = v["ManufacturerCode"].Value;
    //        //Manufacturer,Name,VehicleBrand ilk harf ingilizce küçük harf
    //        //ManufacturerCode
    //        //searchKeywords = seçenek.tümü ? (v["Manufacturer"].Value + " " + v["ManufacturerCode"].Value + " " + küçükHarf(v["Name"].Value) + " " + v["OrginalNo"].Value + " " + v["VehicleBrand"].Value) : "";
    //        int yer;
    //        if (seçenek.tümü || seçenek.label)
    //        {
    //            if (seçenek.sabitLabel)
    //            {
    //                yer = Array.IndexOf(sabitLabelSınıfı.sabitLabel_stockCode, stockCode);
    //                label = yer > -1 ? sabitLabelSınıfı.sabitLabel_label[yer] : v["Manufacturer"].Value + " " + v["ManufacturerCode"].Value + " " + v["Name"].Value + " " + (v["OrginalNo"].Value == "" ? "" : v["OrginalNo"].Value + " ") + v["VehicleBrand"].Value;
    //                if (yer == -1)
    //                {
    //                    Array.Resize(ref sabitLabelSınıfı.sabitLabel_label, sabitLabelSınıfı.sabitLabel_label.Length + 1);
    //                    Array.Resize(ref sabitLabelSınıfı.sabitLabel_stockCode, sabitLabelSınıfı.sabitLabel_stockCode.Length + 1);
    //                    sabitLabelSınıfı.sabitLabel_label[sabitLabelSınıfı.sabitLabel_label.Length - 1] = label;
    //                    sabitLabelSınıfı.sabitLabel_stockCode[sabitLabelSınıfı.sabitLabel_stockCode.Length - 1] = stockCode;
    //                    dönecekLabelDeğeri = stockCode + "½" + label;
    //                }
    //            }
    //            else label = v["Manufacturer"].Value + " " + v["ManufacturerCode"].Value + " " + v["Name"].Value + " " + (v["OrginalNo"].Value == "" ? "" : v["OrginalNo"].Value + " ") + v["VehicleBrand"].Value;
    //        }

    //        status = seçenek.tümü ? "1" : "";
    //        brandDistCode = brand = seçenek.tümü ? v["Manufacturer"].Value : "";
    //        brandId = "";
    //        barcode = "";

    //        //mainCategoryDistCode = mainCategory = seçenek.kategoriBilgileriAlınsın ? v["VehicleType"].InnerText : "";
    //        //category = seçenek.kategoriBilgileriAlınsın ? v["VehicleBrand"].InnerText : "";
    //        //categoryDistCode = seçenek.kategoriBilgileriAlınsın ? category + Form1.anaKategoriID(mainCategory) : "";

    //        category_path = seçenek.kategoriBilgileriAlınsın ? (v["VehicleType"].Value + " > " + v["VehicleBrand"].Value) : "";
    //        price1 = seçenek.tümü || seçenek.fiyat ? v["Price"].Value.ToString() : "";
    //        buyingPrice = price2 = price3 = price4 = price5 = "";

    //        tax = seçenek.tümü ? "18" : "";
    //        currencyAbbr = seçenek.tümü || seçenek.fiyatCinsi ? v["PriceCurrency"].Value : "";

    //        double adet = 0;
    //        try
    //        {
    //            adet = double.Parse(v["Quantity"].Value, new NumberFormatInfo() { NumberDecimalSeparator = "." });
    //        }
    //        catch
    //        {
    //            if (v["Quantity"].Value == "VAR")
    //            {
    //                try
    //                {
    //                    double pktMik = double.Parse(v["MinOrder"].Value, new NumberFormatInfo() { NumberDecimalSeparator = "." });
    //                    if (pktMik < 5) adet = 5;
    //                    else adet = pktMik;
    //                }
    //                catch
    //                {
    //                    adet = 5;
    //                }
    //            }
    //        }

    //        try
    //        {
    //            //if (((string)v["minOrder"].Value).IndexOf('.') > -1)
    //            //    paketMiktarı = (int)double.Parse(v["minOrder"].Value, new NumberFormatInfo() { NumberDecimalSeparator = "." });
    //            //else
    //            paketMiktarı = Convert.ToInt16(v["MinOrder"].Value);
    //        }
    //        catch { paketMiktarı = 1; }

    //        //stockAmount = seçenek.tümü || seçenek.stokMiktarı ? adet.ToString() : "";

    //        stockType = seçenek.tümü ? "Adet" : "";
    //        warranty = seçenek.tümü ? "0" : "";
    //        picture1Path = seçenek.tümü || seçenek.resim ? v["Picture"].Value : "";
    //        picture2Path = picture3Path = picture4Path = "";
    //        dm3 = "";

    //        details = seçenek.tümü ? "<span style=\"font-family: Tahoma, Geneva, sans-serif; font-size: 14pt\">" + v["Manufacturer"].Value + " " + v["ManufacturerCode"].Value + " " + v["Name"].Value + " " + (v["OrginalNo"].Value == "" ? "" : v["OrginalNo"].Value + " ") + v["VehicleBrand"].Value + "</span>" : "";
    //        shortdetails = rebateType = rebate = "";
    //        metaKeywords = metaDescription = metaTitle = descriptions = keywords = title = seçenek.tümü ? v["Manufacturer"].Value + " " + v["ManufacturerCode"].Value + " " + v["Name"].Value + " " + (v["OrginalNo"].Value == "" ? "" : v["OrginalNo"].Value + " ") + v["VehicleBrand"].Value + "</span>" : "";
    //        searchKeywords = "";

    //        seoTagName = seçenek.tümü ? (v["VehicleBrand"].Value + ", " + v["VehicleType"].Value + ", " + v["Manufacturer"].Value + " ") : "";

    //        productSpecialInfoTitle = seçenek.tümü ? "Oem & Eşdeğer" : "";
    //        productSpecialInfoContent = seçenek.tümü ? ("<div><p><span style=\"font-size:medium;\">Oem Numarası: " + v["OrginalNo"].Value + " </span></p><p><span style=\"font-size:medium;\">Eşdeğer Parça: " + "</span> </p></div>") : "";
    //        variants = "";
    //        specGroup = specName = specValue = "";

    //        return dönecekLabelDeğeri;
    //    }
    //    public string dinamikden_içeAktar(dynamic v)
    //    {
    //        string dönecekLabelDeğeri = "";

    //        foreach (var x in v)
    //            x.Value = değişim.getir(x.Name, (string)x.Value);

    //        v["oem_liste"].Value = virgüllerİptal(v["oem_liste"].Value);
    //        v["esdegerListe"].Value = virgüllerİptal(v["esdegerListe"].Value);

    //        #region kull1 den kull1A ve kull1B değerlerini buluyor
    //        string kull1 = v["kull1s"].Value;
    //        kull1 = kull1.Trim();
    //        string kull1A = "";
    //        string kull1B = "";
    //        int yer = kull1.IndexOf("TRUCK");
    //        if (yer + 5 == kull1.Length && yer > -1)
    //        {
    //            kull1A = "AĞIR VASITA";
    //            kull1B = kull1.Substring(0, yer).Trim();
    //        }
    //        else
    //        {
    //            yer = kull1.IndexOf("TICARI");
    //            if (yer + 6 == kull1.Length && yer > -1)
    //            {
    //                kull1A = "HAFİF TİCARİ";
    //                kull1B = kull1.Substring(0, yer).Trim();
    //            }
    //            else
    //            {
    //                kull1A = "BİNEK";
    //                kull1B = kull1;
    //            }
    //        }
    //        #endregion


    //        stockCode = v["stok_kodu"].Value;
    //        //Manufacturer,Name,VehicleBrand ilk harf ingilizce küçük harf
    //        //ManufacturerCode
    //        searchKeywords = "";
    //        //seçenek.tümü ? (v["Manufacturer"].InnerText + " " + v["ManufacturerCode"].InnerText + " " + küçükHarf(v["Name"].InnerText) + " " + v["OrginalNo"].InnerText + " " + v["VehicleBrand"].InnerText) : "";
    //        if (seçenek.tümü || seçenek.label)
    //        {
    //            if (seçenek.sabitLabel)
    //            {
    //                yer = Array.IndexOf(sabitLabelSınıfı.sabitLabel_stockCode, stockCode);
    //                label = yer > -1 ? sabitLabelSınıfı.sabitLabel_label[yer] : (v["stok_kodu"].Value + " " + v["stok_adi"].Value + " " + (v["oem_liste"].Value == "" ? "" : v["oem_liste"].Value) + " " + (v["esdegerListe"].Value == "" ? "" : "(" + v["esdegerListe"].Value + " EŞDEĞERİ) ") + kull1B + " " + kull1A + " " + v["kull8s"].Value);
    //                if (yer == -1)
    //                {
    //                    Array.Resize(ref sabitLabelSınıfı.sabitLabel_label, sabitLabelSınıfı.sabitLabel_label.Length + 1);
    //                    Array.Resize(ref sabitLabelSınıfı.sabitLabel_stockCode, sabitLabelSınıfı.sabitLabel_stockCode.Length + 1);
    //                    sabitLabelSınıfı.sabitLabel_label[sabitLabelSınıfı.sabitLabel_label.Length - 1] = label;
    //                    sabitLabelSınıfı.sabitLabel_stockCode[sabitLabelSınıfı.sabitLabel_stockCode.Length - 1] = stockCode;
    //                    dönecekLabelDeğeri = stockCode + "½" + label;
    //                }
    //            }
    //            else label = v["stok_kodu"].Value + " " + v["stok_adi"].Value + " " + (v["oem_liste"].Value == "" ? "" : v["oem_liste"].Value) + " " + (v["esdegerListe"].Value == "" ? "" : "(" + v["esdegerListe"].Value + " EŞDEĞERİ) ") + kull1B + " " + kull1A + " " + v["kull8s"].Value;
    //        }
    //        status = seçenek.tümü ? "1" : "";
    //        brandDistCode = brand = seçenek.tümü ? v["marka"].Value : "";
    //        brandId = "";
    //        //tek barkod yeter
    //        string barkods = ((string)(((string)(v["barkod1"].Value + " " + v["barkod2"].Value)).Trim() + " " + v["barkod3"].Value)).Trim();
    //        barcode = seçenek.tümü ? barkods : "";

    //        //mainCategoryDistCode = mainCategory = seçenek.kategoriBilgileriAlınsın ? v["VehicleType"].InnerText : "";
    //        //category = seçenek.kategoriBilgileriAlınsın ? v["VehicleBrand"].InnerText : "";
    //        //categoryDistCode = seçenek.kategoriBilgileriAlınsın ? category + Form1.anaKategoriID(mainCategory) : "";

    //        category_path = seçenek.kategoriBilgileriAlınsın ? (kull1A + " > " + kull1B + " > " + v["kull7s"].Value + " > " + v["kull8s"].Value) : "";

    //        //bunlar çıktıda hiç olmayacak
    //        buyingPrice = price2 = price3 = price4 = price5 = ""; //seçenek.tümü ? "0.000" : "";

    //        #region fiyat hesaplama
    //        double fiyat = 0.0;
    //        v["fiyat"].Value = ((string)v["fiyat"].Value).Replace('.', ',');
    //        try
    //        {
    //            fiyat = double.Parse(v["fiyat"].Value);

    //        }
    //        catch { }
    //        if (fiyat > 0)
    //        {
    //            if (fiyat < 50) fiyat *= 1;
    //            else if (fiyat < 100) fiyat *= 0.9;
    //            else if (fiyat < 250) fiyat *= 0.85;
    //            else if (fiyat < 500) fiyat *= 0.8;
    //            else if (fiyat < 1000) fiyat *= 0.75;
    //            else if (fiyat < 5000) fiyat *= 0.7;
    //            else if (fiyat < 100000) fiyat *= 0.65;
    //            else fiyat *= 0.65;
    //        }
    //        #endregion

    //        price1 = seçenek.tümü || seçenek.fiyat ? fiyat.ToString("0.000") : "";

    //        tax = seçenek.tümü ? "18" : "";

    //        //currencyAbbr = seçenek.tümü || seçenek.fiyatCinsi ? v["PriceCurrency"].InnerText : "";
    //        currencyAbbr = seçenek.tümü ? "TL" : "";
    //        int pkMik = 0;
    //        try
    //        {
    //            pkMik = int.Parse(v["paketMiktari"].Value);
    //        }
    //        catch { }
    //        try
    //        {
    //            paketMiktarı = (int)double.Parse(v["paketMiktari"].Value);
    //        }
    //        catch
    //        {
    //            paketMiktarı = 1;
    //        }
    //        stockAmount = seçenek.tümü || seçenek.stokMiktarı ? (v["varyok"].Value == "VAR" ? pkMik.ToString() : "0") : "";

    //        string ölçüBirimi = v["olcuBirimi"].Value;
    //        switch (ölçüBirimi)
    //        {
    //            case "AD": ölçüBirimi = "Adet"; break;
    //            case "TK": ölçüBirimi = "Person"; break;
    //            case "MT": ölçüBirimi = "Metre"; break;
    //            case "KG": ölçüBirimi = "Kg"; break;
    //            case "PK": ölçüBirimi = "Person"; break;
    //            case "pk": ölçüBirimi = "Person"; break;
    //            default:
    //                ölçüBirimi = Interaction.InputBox("Ölçü birimi (dinamikten gelen) '" + ölçüBirimi + "' dir. İdeaSoft'a gidecek veride bunun ne olmasını istersiniz? (Normalde gelen veride 'AD' yazar. Biz onu 'Adet' olarak yazdırıyoruz)", "?????", "Adet");
    //                break;
    //        }
    //        stockType = seçenek.tümü ? ölçüBirimi : "";
    //        warranty = seçenek.tümü ? "0" : "";

    //        //picture1 olacak diğerleri yok
    //        //picture1Path = seçenek.tümü ? v["Picture"].InnerText : "";

    //        //if (seçenek.tümü || seçenek.resim) 
    //        picture1Path = picture2Path = picture3Path = picture4Path = "";
    //        dm3 = (paketMiktarı * 5).ToString();

    //        //label ın aynısı
    //        details = seçenek.tümü ? "<div><p><span style=\"font-size: 12pt;\">Stok Kodu: " + v["stok_kodu"].Value + "</span></p><p><span style=\"font-size: medium;\"> Stok Adı: " + v["stok_adi"].Value + "</span></p><p><span style=\"font-size: medium;\">Parça Markası: " + v["marka"].Value + "</span></p><p><span style=\"font-size: medium;\">Önceki Kodu: " + v["onceki_kod"].Value + "</span></p><p><span style=\"font-size: medium;\">Araç: " + kull1A + " / " + kull1B + "</span></p><p><span style=\"font-size: medium;\">Ürün Grubu:" + v["kull7s"].Value + "</span></p><p><span style=\"font-size: medium;\">Ürün Tipi: " + v["kull8s"].Value + "</span></p><p><span style=\"font-size: medium;\">Oem Numarası: " + v["oem_liste"].Value + "</span></p><p><span style=\"font-size: medium;\">Eşdeğer Parça: " + v["esdegerListe"].Value + "</span></p></div><div><br/></div>" : "";

    //        rebate = seçenek.tümü ? v["kampanyaOrani"].Value : "";
    //        rebateType = seçenek.tümü ? "1" : "";
    //        shortdetails = "";
    //        metaTitle = metaDescription = metaKeywords = title = keywords = descriptions = seçenek.tümü ? (v["stok_kodu"].Value + " " + v["stok_adi"].Value + " " + (v["oem_liste"].Value == "" ? "" : v["oem_liste"].Value) + " " + (v["esdegerListe"].Value == "" ? "" : "(" + v["esdegerListe"].Value + " EŞDEĞERİ) ") + kull1B + " " + kull1A + " " + v["kull8s"].Value) : "";
    //        //searchKeywords yukarıda yapıldı.
    //        seoTagName = seçenek.tümü ? (kull1B + ", " + kull1A + ", " + v["kull8s"].Value + ", " + v["marka"].Value + ", " + v["kull7s"].Value) : "";
    //        productSpecialInfoTitle = seçenek.tümü ? "Oem & Eşdeğer" : "";
    //        productSpecialInfoContent = seçenek.tümü ? ("<div><p><span style=\"font-size:medium;\">Oem Numarası: " + v["oem_liste"].Value + " </span></p><p><span style=\"font-size:medium;\">Eşdeğer Parça: " + v["esdegerListe"].Value + "</span> </p></div>") : "";
    //        variants = "";
    //        specGroup = seçenek.tümü ? "Filtreleme" : "";
    //        specName = seçenek.tümü ? v["kull7s"].Value : "";
    //        specValue = seçenek.tümü ? v["kull8s"].Value : "";

    //        return dönecekLabelDeğeri;
    //    }

    //    #region Alanlar

    //    public string stockCode;
    //    public string label;
    //    public string status;
    //    public string brand;
    //    public string brandId;
    //    public string brandDistCode;
    //    public string barcode;

    //    // bunların yerine category_path olacak
    //    //public string mainCategory;
    //    //public string mainCategoryDistCode;
    //    //public string category;
    //    //public string categoryDistCode;
    //    //public string subCategory;
    //    //public string subCategoryDistCode;
    //    public string category_path;

    //    public string buyingPrice;
    //    public string price1;
    //    public string price2;
    //    public string price3;
    //    public string price4;
    //    public string price5;
    //    public string tax;
    //    public string currencyAbbr;
    //    public string stockAmount;
    //    public int paketMiktarı;
    //    public string stockType;
    //    public string warranty;
    //    public string picture1Path;
    //    public string picture2Path;
    //    public string picture3Path;
    //    public string picture4Path;
    //    public string dm3;
    //    public string details;
    //    public string rebate;
    //    public string rebateType;
    //    //-------yeni------
    //    public string shortdetails;
    //    public string title;
    //    public string keywords;
    //    public string descriptions;
    //    public string seoTagName;
    //    public string metaTitle;
    //    public string metaDescription;
    //    public string metaKeywords;
    //    public string productSpecialInfoTitle;
    //    public string productSpecialInfoContent;

    //    // en alttaki specs yapısı şu şekilde olacak.
    //    //<specs>
    //    //<spec>
    //    //<specGroup><![CDATA[Filtreleme(sabit)]]></specGroup>
    //    //<specName><![CDATA["kull7s"]]></specName>
    //    //<specValue><![CDATA["kull8s"]]></specValue>
    //    //</spec>
    //    //</specs>

    //    //--------yeni end ------
    //    public string searchKeywords;
    //    public string variants;
    //    public string specGroup;
    //    public string specName;
    //    public string specValue;
    //    #endregion
    //}
}