using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;
using RestSharp;
using Newtonsoft.Json;
using System.Data;
using System.Windows.Forms;

namespace NettenGelsin
{
    public static class program
    {
        public static void Kapat()
        {
            log.Yaz("PROGRAM KAPANIYOR");
            log.kapanacak = true;
            //System.Threading.Thread.Sleep(80000);
            //log.con.Close();
            //Environment.Exit(Environment.ExitCode);
            //Application.Exit();
        }
    }

    public static class firmaVeriÇekmeMotorları
    {
        static string m_companyKey = "954FCD2D";
        static string m_functionName = "GetProductList_Atamer";
        static string m_userName = "atamer_motorasin";
        static string m_password = "dDKs3dfyQH";
        static string m_dataType = "xml";
        static int m_SonLimit = 120000;
        static int m_Paket = 1000;
        public static List<IRestResponse> motorasinDönenDeğerler = new List<IRestResponse>();
        //static int dinamikBeklemeSüresi = 5000;

        static IRestResponse veriÇekMotoraşin(int başlama, int bitiş, string function_name = "")
        {
            var jSonYapi = new { companyKey = m_companyKey, functionName = (function_name == "" ? m_functionName : function_name), userName = m_userName, password = m_password, dataType = m_dataType, parameters = new { pStart = başlama, pEnd = bitiş } };
            var json = JsonConvert.SerializeObject(jSonYapi);
            var client = new RestClient("http://share.eryaz.net/api/integration/getdata");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response;
        }
        static IRestResponse markaListesiÇekDinamik()
        {
            var client = new RestClient("https://kokpit.dinamik.online:8181/operation/getBrandList?api_username=atamer&api_password=Ata20mer20*!*");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            return response;
        }
        static IRestResponse markaVeriÇekDinamik(string Marka)
        {
            var client = new RestClient("https://kokpit.dinamik.online:8181/operation/getStockList?api_username=atamer&api_password=Ata20mer20*!*&api_marka=" + Marka);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            return response;
        }


        public static void MotorasinGündüzVeriÇekmeMotoru(object[] parametreler)
        {

        }

        public static void MotoraşinVeriÇekmeMotoru(object[] parametreler)
        {//parametreler 0-->stringBuilder ifadesi, 1-->anahtarlar, 2-->progressBar, 3-->label olacak
            motorasinDönenDeğerler.Clear();
            log.Yaz(firmalar.Motoraşin.firmaAdı + " Veri Çekme Başlıyor..");
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();

            string stringBuilderİfadesi = (string)parametreler[0]; //ham veri dosyası (motorasin_ham) ile geliyor
            List<firmaAnahtar> anahtarlar = (List<firmaAnahtar>)parametreler[1];
            //bool gece = (bool) parametreler[2]
            bool tablo_boşaltılacak = true;
            int sonLimit = m_SonLimit;
            int paket = m_Paket;

            int parti = (sonLimit / paket) + (sonLimit % paket == 0 ? 0 : 1);

            DateTime t_Start = DateTime.Now;

            IRestResponse data;
            bool devam = true;
            StringBuilder sCommandMotorasinHam = new StringBuilder(stringBuilderİfadesi);
            List<string> RowsMotorasinHam = new List<string>();
            int gönderilenVeriUzunluğuMotorasin = 0;
            //StringBuilder sCommandOrtak = new StringBuilder(veritabanı.OrtakTablosuStringBuilderİfadesi);
            //List<string> RowsOrtak = new List<string>();
            //int gönderilenVeriUzunluğuOrtak = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            int i;
            int deneme = 0;
            do
            {
                deneme++;
                int bekleme;
                i = -1;
                while (devam)
                {
                    i++;
                    int tekrar = 0;
                    int baş = i * paket - 1;
                    if (baş < 0) baş = 0;
                    int bit = (((i + 1) * paket) > sonLimit ? sonLimit : ((i + 1) * paket)) + 1;
                    bekleme = Thread_Apiİşlemleri.bekleme;
                    do
                    {
                        log.Yaz("motorasin " + (i + 1).ToString() + "/" + parti.ToString() + " veri çekiliyor..");
                        data = veriÇekMotoraşin(baş, bit);
                        Thread.Sleep(bekleme);
                        dynamic içerik = JsonConvert.DeserializeObject(data.Content);
                        if (data.Content == "" || data.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable|| !(bool)içerik.Status)
                        {
                            bekleme *= Thread_Apiİşlemleri.tekrarÇarpan;
                            tekrar++;
                        }
                        else
                        {
                            //dynamic j = JsonConvert.DeserializeObject(data.Content);
                            //if (!(bool)j["Status"].Value) return;
                            if (devam = (bool)içerik["Status"].Value)
                                break;
                            else
                            {
                                bekleme *= Thread_Apiİşlemleri.tekrarÇarpan;
                                tekrar++;
                            }
                        }
                    } while (tekrar < Thread_Apiİşlemleri.tekrar);
                    if (tekrar == Thread_Apiİşlemleri.tekrar)
                    {
                        email.send("motorasin verileri çekilirken hata", string.Format("motorasin verileri çekilirken hata oluştu. Hatalı paket no:{0}, Dönen statusCode:{1}, dönen Content:'{2}'", i + 1, data.StatusCode, data.Content));
                        MessageBox.Show(string.Format("motorasin verileri çekilirken hata oluştu. Hatalı paket no:{0}, Dönen statusCode:{1}, dönen Content:'{2}'", i + 1, data.StatusCode, data.Content));

                    }
                    motorasinDönenDeğerler.Add(data);
                    if (devam)
                    {
                        dynamic d = JsonConvert.DeserializeObject(data.Content);
                        d = d["Data"];
                        if (devam = (d.Count > 0))
                        {
                            foreach (var item in d)
                            {
                                RowsMotorasinHam.Add(classFirma.formatStringVer(item, anahtarlar, firmalar.Motoraşin.firmaAdı));
                                gönderilenVeriUzunluğuMotorasin += RowsMotorasinHam[RowsMotorasinHam.Count - 1].Length;
                                //RowsOrtak.Add(classFirma.ortakİçinFormatStringVer(item, anahtarlar, firmalar.Motoraşin.firmaAdı));
                                //gönderilenVeriUzunluğuOrtak += RowsOrtak[RowsOrtak.Count - 1].Length;
                                long x = (veritabanı.gönderilenPaketBoyutu - 1) * 1024 * 1024;
                                if (gönderilenVeriUzunluğuMotorasin > x) // || gönderilenVeriUzunluğuOrtak > x)
                                {
                                    if (tablo_boşaltılacak)
                                    {
                                        veritabanı.tabloOluştur_idAuto(firmalar.Motoraşin.hamVeriİçinDosyaAdı, firmalar.Motoraşin.tabloYapısı, firmalar.Motoraşin.indexYapısı, con);
                                        tablo_boşaltılacak = false;
                                    }

                                    sCommandMotorasinHam.Append(string.Join(",", RowsMotorasinHam));
                                    sCommandMotorasinHam.Append(";");
                                    cmd.CommandText = sCommandMotorasinHam.ToString();
                                    cmd.CommandType = CommandType.Text;
                                    veritabanı.cmdExecute(cmd);
                                    RowsMotorasinHam.Clear();
                                    gönderilenVeriUzunluğuMotorasin = 0;
                                    sCommandMotorasinHam = new StringBuilder(stringBuilderİfadesi);

                                    //sCommandOrtak.Append(string.Join(",", RowsOrtak));
                                    //sCommandOrtak.Append(";");
                                    //cmd.CommandText = sCommandOrtak.ToString();
                                    //cmd.CommandType = CommandType.Text;
                                    //cmd.ExecuteNonQuery();
                                    //RowsOrtak.Clear();
                                    //gönderilenVeriUzunluğuOrtak = 0;
                                    //sCommandOrtak = new StringBuilder(veritabanı.OrtakTablosuStringBuilderİfadesi);

                                }
                            }
                            if (RowsMotorasinHam.Count > 0)
                            {
                                if (tablo_boşaltılacak)
                                {
                                    veritabanı.tabloOluştur_idAuto(firmalar.Motoraşin.hamVeriİçinDosyaAdı, firmalar.Motoraşin.tabloYapısı, firmalar.Motoraşin.indexYapısı, con);
                                    tablo_boşaltılacak = false;
                                }

                                sCommandMotorasinHam.Append(string.Join(",", RowsMotorasinHam));
                                sCommandMotorasinHam.Append(";");
                                cmd.CommandText = sCommandMotorasinHam.ToString();
                                cmd.CommandType = CommandType.Text;
                                veritabanı.cmdExecute(cmd);
                                RowsMotorasinHam.Clear();
                                gönderilenVeriUzunluğuMotorasin = 0;
                                sCommandMotorasinHam = new StringBuilder(stringBuilderİfadesi);

                                //sCommandOrtak.Append(string.Join(",", RowsOrtak));
                                //sCommandOrtak.Append(";");
                                //cmd.CommandText = sCommandOrtak.ToString();
                                //cmd.CommandType = CommandType.Text;
                                //cmd.ExecuteNonQuery();
                                //RowsOrtak.Clear();
                                //gönderilenVeriUzunluğuOrtak = 0;
                                //sCommandOrtak = new StringBuilder(veritabanı.OrtakTablosuStringBuilderİfadesi);
                            }
                        }
                        //else page--;
                    }
                    //if (i >= paket) break;
                }
                devam = true;
            }
            while ((i + 1) * paket < 100000 && deneme < 5);

            //if (motorasinDönenDeğerler.Count<24)
            //{
            //    email.send("motorasin verileri hatalı çekildi.", "Program akışı sizin incelemeniz için kırıldı. Bekleniyorsunuz..");


            //}

            if (!tablo_boşaltılacak)
            {
                log.Yaz(firmalar.Motoraşin.firmaAdı + " verileri çekildi.");
                //firmalar.Motoraşin.verilerÇekildiktenSonraYapılacaklar(con);
            }
            con.Close();
            TimeSpan geçen = DateTime.Now.Subtract(t_Start);
            log.Yaz("Motoraşin işlemi bitti. Geçen süre: " + (geçen.Days == 0 ? "" : geçen.Days.ToString() + " gün ") + (geçen.Hours == 0 ? "" : geçen.Hours.ToString() + " saat ") + (geçen.Minutes == 0 ? "" : geçen.Minutes.ToString() + " dakika ") + (geçen.Seconds == 0 ? "" : geçen.Seconds.ToString() + " saniye") + " SON Paket=" + i.ToString());
        }
        public static void DinamikVeriÇekmeMotoru(object[] parametreler)
        {
            log.Yaz(firmalar.Dinamik.firmaAdı + " Veri çekme başlıyor..");
            DateTime t_Start = DateTime.Now;
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            string stringBuilderİfadesi = (string)parametreler[0];
            List<firmaAnahtar> anahtarlar = (List<firmaAnahtar>)parametreler[1];
            log.Yaz(firmalar.Dinamik.firmaAdı + " Marka listesi çekiliyor.");
            IRestResponse x = markaListesiÇekDinamik();
            DateTime t = DateTime.Now;
            if (!x.IsSuccessful)
            {
                log.Yaz("Dinamik verileri çekilemedi.(" + x.ErrorMessage + ")", "Hata");
                return;
            }
            log.Yaz("Dinamik markalar çekildi.");

            bool yazıldı = false;
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            //cmd.Parameters.Add("@marka", MySqlDbType.VarChar);
            cmd.CommandTimeout = 600;
            //cmd.CommandText = string.Format("INSERT INTO {0}(marka) SELECT * FROM (SELECT @marka) AS tmp WHERE NOT EXISTS(SELECT marka FROM {0} WHERE marka = @marka) LIMIT 1", veritabanı.DinamikMarkalarTablosu);

            dynamic jsonData = JsonConvert.DeserializeObject(x.Content);
            //int adet = 0;
            string marka;
            //string eklenenMarkalar = "";
            //int d;

            //foreach (var item in jsonData["brandlist"])
            //{
            //    //marka = item["brand"];
            //    //cmd.Parameters["@marka"].Value = marka;
            //    //d = cmd.ExecuteNonQuery();
            //    //if (d > 0)
            //    //{
            //    //    eklenenMarkalar += (eklenenMarkalar != "" ? ", " : "") + marka;
            //    //    adet++;
            //    //}
            //}

            //if (adet > 0)
            //{
            //    log.Yaz(adet.ToString() + " adet yeni marka tespit edildi. " + eklenenMarkalar,true,false);
            //    Thread.Sleep(3000);
            //}
            //cmd.Parameters.Clear();
            //#endregion
            //#endregion
            //#region çekilecek markalar getiriliyor
            //cmd.CommandText = string.Format("select marka from {0}", veritabanı.DinamikMarkalarTablosu);
            ////where cekilecek IS TRUE 
            //MySqlDataReader dr = cmd.ExecuteReader();
            //List<string> markalar = new List<string>();
            //while (dr.Read())
            //{
            //    markalar.Add((string)dr.GetValue(0));
            //}
            //dr.Close();


            veritabanı.tabloOluştur_idAuto(firmalar.Dinamik.hamVeriİçinDosyaAdı, firmalar.Dinamik.tabloYapısı, firmalar.Dinamik.indexYapısı, con);
            //veritabanı.ortakTablosundanSil(firmalar.Dinamik.firmaAdı, con);

            IRestResponse data;
            StringBuilder sCommandDinamik = new StringBuilder(stringBuilderİfadesi);
            List<string> RowsDinamik = new List<string>();
            int gönderilenVeriUzunluğuDinamik = 0;
            //StringBuilder sCommand1 = new StringBuilder(string.Format("UPDATE {0} SET cekilecek='0' WHERE marka IN (", veritabanı.DinamikMarkalarTablosu));
            //List<string> Rows1 = new List<string>();
            //StringBuilder sCommandOrtak = new StringBuilder(veritabanı.OrtakTablosuStringBuilderİfadesi);
            //List<string> RowsOrtak = new List<string>();
            //int gönderilenVeriUzunluğuOrtak = 0;

            string stokKodu = "";
            int başarılı = -1;
            int bekleme;

            foreach (var eleman in jsonData["brandlist"])
            {
                marka = eleman["brand"];
                bekleme = Thread_Apiİşlemleri.bekleme;
                log.Yaz(firmalar.Dinamik.firmaAdı + "  " + marka + " verileri çekiliyor.");
                stokKodu = "ERR";
                başarılı = -1;
                int tekrar = 0;
                while (true)
                {
                    Thread.Sleep(bekleme);
                    data = markaVeriÇekDinamik(marka);
                    if (jsonData != null)
                    {
                        jsonData = JsonConvert.DeserializeObject(data.Content);
                        jsonData = jsonData["stockList"];
                        if (jsonData.First == null) { başarılı = 0; break; }
                        stokKodu = jsonData.First["stok_kodu"].Value;
                        if (stokKodu != "ERR") { başarılı = 1; break; }
                        if (tekrar >= Thread_Apiİşlemleri.tekrar) break;
                    }
                    bekleme *= Thread_Apiİşlemleri.tekrarÇarpan;
                    tekrar++;
                    log.Yaz(marka + " çekilemedi.. Bekleme süresi arttırılıyor. Bekleme süresi:" + bekleme.ToString(), "Hata");
                }

                if (başarılı < 1) continue;
                //{
                //    log.Yaz(marka + " BOŞ. Çekilecekler listesinde değeri 'False' yapılıyor.", false, false);
                //    Rows1.Add("'" + marka + "'");
                //    continue;
                //}
                if (jsonData.Count > 0)
                {
                    foreach (var item in jsonData)
                    {
                        RowsDinamik.Add(classFirma.formatStringVer(item, anahtarlar, firmalar.Dinamik.firmaAdı));
                        gönderilenVeriUzunluğuDinamik += RowsDinamik[RowsDinamik.Count - 1].Length;
                        //RowsOrtak.Add(classFirma.ortakİçinFormatStringVer(item, anahtarlar, firmalar.Dinamik.firmaAdı));
                        //gönderilenVeriUzunluğuOrtak += RowsOrtak[RowsOrtak.Count - 1].Length;
                        long xx = (veritabanı.gönderilenPaketBoyutu - 1) * 1024 * 1024;
                        if (gönderilenVeriUzunluğuDinamik > xx) // || gönderilenVeriUzunluğuOrtak > xx)
                        {
                            yazıldı = true;
                            sCommandDinamik.Append(string.Join(",", RowsDinamik));
                            sCommandDinamik.Append(";");
                            cmd.CommandText = sCommandDinamik.ToString();
                            cmd.CommandType = CommandType.Text;
                            veritabanı.cmdExecute(cmd);
                            RowsDinamik.Clear();
                            gönderilenVeriUzunluğuDinamik = 0;
                            sCommandDinamik = new StringBuilder(stringBuilderİfadesi);

                            //sCommandOrtak.Append(string.Join(",", RowsOrtak));
                            //sCommandOrtak.Append(";");
                            //cmd.CommandText = sCommandOrtak.ToString();
                            //cmd.CommandType = CommandType.Text;
                            //cmd.ExecuteNonQuery();
                            //RowsOrtak.Clear();
                            //gönderilenVeriUzunluğuOrtak = 0;
                            //sCommandOrtak = new StringBuilder(veritabanı.OrtakTablosuStringBuilderİfadesi);
                        }
                    }
                    if (RowsDinamik.Count > 0)
                    {
                        yazıldı = true;
                        sCommandDinamik.Append(string.Join(",", RowsDinamik));
                        sCommandDinamik.Append(";");
                        cmd.CommandText = sCommandDinamik.ToString();
                        cmd.CommandType = CommandType.Text;

                        veritabanı.cmdExecute(cmd);
                        RowsDinamik.Clear();
                        gönderilenVeriUzunluğuDinamik = 0;
                        sCommandDinamik = new StringBuilder(stringBuilderİfadesi);

                        //sCommandOrtak.Append(string.Join(",", RowsOrtak));
                        //sCommandOrtak.Append(";");
                        //cmd.CommandText = sCommandOrtak.ToString();
                        //cmd.CommandType = CommandType.Text;
                        //cmd.ExecuteNonQuery();
                        //RowsOrtak.Clear();
                        //gönderilenVeriUzunluğuOrtak = 0;
                        //sCommandOrtak = new StringBuilder(veritabanı.OrtakTablosuStringBuilderİfadesi);
                    }
                }
            }
            //if (Rows1.Count > 0)
            //{
            //    sCommand1.Append(string.Join(",", Rows1));
            //    sCommand1.Append(");");
            //    cmd.CommandText = sCommand1.ToString();
            //    cmd.CommandType = CommandType.Text;
            //    cmd.ExecuteNonQuery();
            //}

            if (yazıldı)
            {
                log.Yaz(firmalar.Dinamik.firmaAdı + " verileri çekildi.");
                //firmalar.Dinamik.verilerÇekildiktenSonraYapılacaklar(con);
            }
            con.Close();
            TimeSpan geçen = DateTime.Now.Subtract(t_Start);
            log.Yaz("Dinamik işlemi bitti. Geçen süre: " + (geçen.Days == 0 ? "" : geçen.Days.ToString() + " gün ") + (geçen.Hours == 0 ? "" : geçen.Hours.ToString() + " saat ") + (geçen.Minutes == 0 ? "" : geçen.Minutes.ToString() + " dakika ") + (geçen.Seconds == 0 ? "" : geçen.Seconds.ToString() + " saniye"));
        }
    }

    public class firmaAnahtar
    {
        public string alanAdı;
        public char type;
        //public anahtar child;
        public string parameter(dynamic item)
        {
            if (type == 's') return "'" + (item[alanAdı] == null ? "" : ((string)item[alanAdı].Value).Replace("'", "\\'")) + "'";
            else if (type == 'i') return item[alanAdı] == null ? "NULL" : ((long)item[alanAdı].Value).ToString();
            else if (type == 'f') return item[alanAdı] == null ? "NULL" : ((double)item[alanAdı].Value).ToString().Replace(',', '.');
            else if (type == 'b') return item[alanAdı] == null ? "NULL" : ((bool)item[alanAdı].Value ? "true" : "false");
            else
                return "'hata'";
        }
        public firmaAnahtar(string alanTanımlaması)
        {
            alanTanımlaması = alanTanımlaması.Trim();
            string[] s = alanTanımlaması.Trim().Split(' ');
            alanAdı = s[0];
            string x = s[s.Length - 1];
            if (x.IndexOf("INT") > -1) type = 'i';
            else if (x.IndexOf("TEXT") > -1 || x.IndexOf("VARCHAR") > -1) type = 's';
            else if (x.IndexOf("FLOAT") > -1) type = 'f';
            else if (x.IndexOf("BOOLEAN") > -1) type = 'b';

            //if (alanAdı.IndexOf('_') > -1)
            //{
            //    s = alanAdı.Split('_');
            //    alanAdı = s[0];
            //    child = new anahtar(s[1] + " " + x);
            //    type = '\0';
            //}
        }
    }

    public class classFirma
    {
        public System.Windows.Forms.Label l;
        public string firmaAdı;
        public string hamVeriİçinDosyaAdı { get { return firmaAdı + "_ham"; } }
        public string tabloYapısı;
        public string indexYapısı;
        public string stringBuilderİfadesi
        {
            get
            {
                string s = "";
                for (int i = 0; i < anahtarlar.Count; i++) s += (i == 0 ? "" : ",") + anahtarlar[i].alanAdı;
                return "INSERT INTO " + hamVeriİçinDosyaAdı + "(" + s + ") VALUES ";
            }
        }
        public List<firmaAnahtar> anahtarlar = new List<firmaAnahtar>();
        public Thread T_veriÇekme; //ideasoft tan
        public Thread T_veriYazma; //ideasoft a
        public delegate void Temsilci(object[] parameters);//ilk stringBuilderİfadesi, ikincisi anahtarlar
        public Temsilci veriÇekmeMotoru;
        public bool veriÇekmeBaşarılı = false;

        public static string formatStringVer(dynamic item, List<firmaAnahtar> anahtarlar, string hangiFirma)
        {
            string s = "";
            for (int i = 0; i < anahtarlar.Count; i++) s += (i == 0 ? "" : ",") + anahtarlar[i].parameter(item);
            return "(" + s + ")";
        }

        //public static void multiRecordControlOrtak(MySqlConnection con)
        //{
        //    MySqlCommand cmd = new MySqlCommand("CALL multiRecordControlOrtak", con);
        //    cmd.CommandTimeout = 4800;
        //    veritabanı.cmdExecute(cmd);
        //}

        //public static void mulahazaEt()
        //{
        //    MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
        //    con.Open();
        //    MySqlCommand cmd = new MySqlCommand(string.Format("CALL mulahaza({0},{1})", Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.')), con);
        //    cmd.CommandTimeout = 4800;
        //    veritabanı.cmdExecute(cmd);
        //    con.Close();
        //}

        //public static string ortakİçinFormatStringVer(dynamic item, List<firmaAnahtar> anahtarlar, string hangiFirma)
        //{
        //    if (hangiFirma == firmalar.Motoraşin.firmaAdı)
        //    {
        //        return string.Format("({12}, CONCAT(degisim({0},{3},{14}),' ',{12},' ',degisim({0},{2},{13}),' ', {20},' ',degisim({0},{8},{19})), degisim({0},{3},{14}), CONCAT(degisim({0},{7},{18}),'>',degisim({0},{8},{19})),' ',{16}, {17}, {22}, stockAmountMotorasin({15},{22}), 'Piece', 0, 1, {21}, '{23}')", "'" + firmalar.Motoraşin.firmaAdı + "'", "'ManufacturerCode'", "'Name'", "'Manufacturer'", "", "", "", "'VehicleType'", "'VehicleBrand'", "'OrginalNo'", "", "", anahtarlar[0].parameter(item), anahtarlar[1].parameter(item), anahtarlar[2].parameter(item), anahtarlar[3].parameter(item), anahtarlar[4].parameter(item), anahtarlar[5].parameter(item), anahtarlar[6].parameter(item), anahtarlar[7].parameter(item), anahtarlar[8].parameter(item), anahtarlar[9].parameter(item), anahtarlar[10].parameter(item), firmalar.Motoraşin.firmaAdı);
        //    }
        //    else if (hangiFirma == firmalar.Dinamik.firmaAdı)
        //    {
        //        return string.Format("({26}, CONCAT({26},' ',degisim({0},{2},{27}),' ', {40},' ', IF({41}='','',CONCAT('(',{41},' EŞDEĞERİ)')),' ', kull1B(degisim({0},{6},{31})),' ',kull1A(degisim({0},{6},{31})),' ', degisim({0},{13},{38})), degisim({0},{3},{28}), CONCAT(kull1A(degisim({0},{6},{31})), '>', kull1B(degisim({0},{6},{31})), '>', degisim({0},{12},{37}), '>', degisim({0},{13},{38})), barkodlar({44},{45},{46}),  fiyatDinamik({42}), 'TL', {48}, stockAmountDinamik({43},{48}), stockTypeDinamik({50}), {48}, 1, '', '{51}')", "'" + firmalar.Dinamik.firmaAdı + "'", "'stok_kodu'", "'stok_adi'", "'marka'", "", "", "'kull1s'", "", "", "", "", "", "'kull7s'", "'kull8s'", "", "", "", "", "", "", "", "", "", "", "", "", anahtarlar[0].parameter(item), anahtarlar[1].parameter(item), anahtarlar[2].parameter(item), "", "", anahtarlar[5].parameter(item), "", "", "", "", "", anahtarlar[11].parameter(item), anahtarlar[12].parameter(item), "", anahtarlar[14].parameter(item), anahtarlar[15].parameter(item), anahtarlar[16].parameter(item), anahtarlar[17].parameter(item), anahtarlar[18].parameter(item), anahtarlar[19].parameter(item), anahtarlar[20].parameter(item), "", anahtarlar[22].parameter(item), "", anahtarlar[24].parameter(item), firmalar.Dinamik.firmaAdı);
        //    }
        //    else return "";
        //}

        public classFirma(string firmaAdı, string tabloYapısı, string indexYapısı, Temsilci veriÇekmeMotoru)
        {
            l = new System.Windows.Forms.Label();
            this.firmaAdı = firmaAdı;
            this.tabloYapısı = tabloYapısı;
            this.indexYapısı = indexYapısı;
            string[] s = tabloYapısı.Split(',');
            for (int i = 0; i < s.Length; i++) anahtarlar.Add(new firmaAnahtar(s[i]));
            this.veriÇekmeMotoru = veriÇekmeMotoru;
        }

        public void verileriÇek() //ideaSoft tan
        {
            if (T_veriÇekme != null) if (T_veriÇekme.IsAlive) return;
            //Thread_Apiİşlemleri.işEkle(T_veriÇekme = new Thread(() => veriÇekmeMotoru(new object[] { stringBuilderİfadesi, anahtarlar })));
            T_veriÇekme = new Thread(() => veriÇekmeMotoru(new object[] { stringBuilderİfadesi, anahtarlar }));
            T_veriÇekme.Start();
        }

        public void verilerÇekildiktenSonraYapılacaklar(MySqlConnection con)
        {
            log.Yaz(firmaAdı + " degisim tablosundaki değişimler uygulanıyor ve yeni değiştirilmiş tablo oluşturuluyor.");
            degistirilmişDosyayıOlustur(con);
            log.Yaz(firmaAdı + " ortak tablosuna aktarılıyor.");
            ortağaAktar(con);
            log.Yaz(firmaAdı + " Ortak Tablosundaki çoklu kayıtlar siliniyor.");
            //multiRecordControlOrtak(con);
            //MySqlCommand cmd = new MySqlCommand("call degistir('ortak')", con);
            //veritabanı.cmdExecute(cmd);
            //veritabanı.degisim("ortak");
            //log.Yaz(firmaAdı + " binekDuzeltme Uygulanıyor.");
            //binekDüzeltme(con);

            //log.Yaz(firmaAdı + "\tstokSifirKontrol Uygulanıyor.", true, false);
            //stokSıfırKontrol(con);

        }

        public void degistirilmişDosyayıOlustur(MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            cmd.CommandTimeout = 4800;
            cmd.CommandText = string.Format("DROP TABLE if EXISTS {0};", firmaAdı);
            veritabanı.cmdExecute(cmd);
            cmd.CommandText = string.Format("CREATE TABLE {0}({1}) SELECT * FROM {2};", firmaAdı, indexYapısı, hamVeriİçinDosyaAdı);
            veritabanı.cmdExecute(cmd);
            cmd.CommandText = string.Format("CALL degistir('{0}');", firmaAdı);
            veritabanı.cmdExecute(cmd);
            //veritabanı.degisim(firmaAdı);
        }

        public void ortağaAktar(MySqlConnection con)
        {
            string stokkodu, label, metakeywords, brand, categorypath, barcode, price, currencyabbr, paketmiktarı, stokamount, stoktype, discount, discounttype, nereden, detail, tabloadı;
            string nettengelsin = "'netten gelsin', ', ', 'nettengelsin', ', ', 'nettengelsin.com', ', ', 'oto yedek parça', ', ', 'yedek parça', ', ', 'parça'";
            string ortakAktarımSQL = string.Format("DELETE FROM {0} WHERE {0}.nereden = '{1}';", veritabanı.OrtakTablosu, firmaAdı) + veritabanı.OrtakTablosuINSERT_İfadesi;

            if (firmaAdı == firmalar.Motoraşin.firmaAdı)
            {
                float usdKur = Entegrasyon.kurNedir("USD");
                float eurKur = Entegrasyon.kurNedir("EUR");

                stokkodu = "CONCAT(Manufacturer,' ',ManufacturerCode)";
                label = "trim(substring(detayDegisim(CONCAT(Manufacturer, ' ', ManufacturerCode, ' ', Name, ' ', OrginalNo, ' ', VehicleBrand)),1,255))";
                metakeywords = string.Format("detayDegisim(CONCAT(Manufacturer, ', ', ManufacturerCode, ', ', CONCAT(Manufacturer,' ',ManufacturerCode), ', ', REPLACE(ManufacturerCode,' ','') , ', ', REPLACE(CONCAT(Manufacturer,' ',ManufacturerCode),' ',''), ', ', Name, ', ', if(OrginalNo='','',CONCAT(OrginalNo,', ')), VehicleBrand, ', ',{0}))", nettengelsin);
                brand = "Manufacturer";
                categorypath = "CONCAT(VehicleType, '>', VehicleBrand)";
                barcode = "''";
                price = string.Format("if(CampaignPrice=0 AND IsNetPrice='NET',motorasinHesaplananFiyat(Price,PriceCurrency,PriceCurrency,{0},{1}),Price)", usdKur.ToString().Replace(',', '.'), eurKur.ToString().Replace(',', '.'));
                currencyabbr = "if(PriceCurrency='','TL',PriceCurrency)";
                paketmiktarı = "MinOrder";
                stokamount = "stockAmountMotorasin(Quantity, MinOrder)";
                stoktype = "'Piece'";
                discount = string.Format("if(CampaignPrice>0,if(Price-motorasinHesaplananFiyat(CampaignPrice,CampaignCurrency,PriceCurrency,{0},{1})>0,(1-motorasinHesaplananFiyat(CampaignPrice,CampaignCurrency,PriceCurrency,{0},{1})/Price)*100,0),0)", usdKur.ToString().Replace(',', '.'), eurKur.ToString().Replace(',', '.'));
                discounttype = "1";
                nereden = "'motorasin'";
                detail = "detailVer('motorasin',ManufacturerCode,Name,Manufacturer,VehicleType,VehicleBrand,OrginalNo,'','','')";
                tabloadı = "motorasin";
            }

            // motorasinHesaplananFiyat(CampaignPrice,CampaignCurrency,PriceCurrency,{1},{2})
            else if (firmaAdı == firmalar.Dinamik.firmaAdı)
            {
                stokkodu = "stok_kodu";
                label = "trim(substring(detayDegisim(CONCAT(stok_kodu, ' ', stok_adi, ' ', oem_liste, if(esdegerListe='', '', CONCAT(' (', esdegerListe, ' EŞDEĞERİ)')), ' ', kull1B(kull1s), ' ', kull1A(kull1s), ' ', kull8s)),1,255))";
                metakeywords = string.Format("detayDegisim(CONCAT(marka, ', ', Replace(SUBSTRING(stok_kodu,INSTR(stok_kodu,' ')),' ',''), ', ', stok_kodu, ', ',replace(stok_kodu,' ',''), ', ', if(replace(oem_liste,',','')='','',CONCAT(replace(Oem_liste,',',''),', ')), if(replace(esdegerListe,',','')='', '', CONCAT(replace(esdegerListe,',',''),', ')), kull1B(kull1s), ', ', kull1A(kull1s), ', ', kull8s, ', ', barkodlar(barkod1, barkod2, barkod3), ', ',{0}))", nettengelsin);
                brand = "marka";
                categorypath = " CONCAT(kull1A(kull1s), '>', kull1B(kull1s),if(kull7s='','',CONCAT('>',kull7s)),if(kull8s='','',CONCAT('>',kull8s)))";
                barcode = "barkodlar(barkod1, barkod2, barkod3)";
                price = "fiyatDinamik(fiyat)";
                currencyabbr = "'TL'";
                paketmiktarı = "paketMiktari";
                stokamount = "stockAmountDinamik(varyok1, varyok2, varyok3, paketMiktari)";
                stoktype = "stockTypeDinamik(olcuBirimi)";
                discount = "kampanyaOrani";
                discounttype = "1";
                nereden = "'dinamik'";
                detail = "detailVer('dinamik', stok_kodu, stok_adi, marka, onceki_kod, kull1s, kull7s, kull8s, oem_liste, esdegerListe)";
                tabloadı = "dinamik";

                //kategori path eski değeri CONCAT(kull1A(kull1s), '>', kull1B(kull1s), '>', kull7s, if(kull8s<>'','>',''), kull8s) şeklindeydi
            }
            else return;

            ortakAktarımSQL += string.Format(@"SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10},{11}, {12}, {13}, {14} FROM {15};", stokkodu, label, metakeywords, brand, categorypath, barcode, price, currencyabbr, paketmiktarı, stokamount, stoktype, discount, discounttype, nereden, detail, tabloadı);

            MySqlCommand cmd = new MySqlCommand(ortakAktarımSQL, con);
            cmd.CommandTimeout = 4800;
            veritabanı.cmdExecute(cmd);
        }



        //public void stokSıfırKontrol(MySqlConnection con)
        //{
        //    MySqlCommand cmd = new MySqlCommand(string.Format("CALL stokSifirKontrol('{0}')", firmaAdı), con);
        //    cmd.CommandTimeout = 4800;
        //    veritabanı.cmdExecute(cmd);
        //}

        //public void mulahazaEt()
        //{
        //    log.Yaz("Kur bilgileri getiriliyor.", "Kur");
        //    float usdKur = Entegrasyon.kurNedir("USD");
        //    float eurKur = Entegrasyon.kurNedir("EUR");
        //    log.Yaz("USD: " + usdKur.ToString() + "   EUR: " + eurKur.ToString(), "Kur");
        //    log.Yaz(firmaAdı + " mulahaza edilecek.", "Mülahaza");
        //    MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
        //    con.Open();
        //    MySqlCommand cmd = new MySqlCommand(string.Format("CALL mulahaza('{0}',{1},{2})", firmaAdı, usdKur.ToString().Replace(',', '.'), eurKur.ToString().Replace(',', '.')), con);
        //    cmd.CommandTimeout = 4800;
        //    veritabanı.cmdExecute(cmd);
        //    log.Yaz(firmaAdı + " mulahaza edildi.", "Mülahaza");
        //    con.Close();
        //}

        //public string detailVer(string sku, MySqlConnection con)
        //{
        //    MySqlCommand cmd = new MySqlCommand(string.Format("select detailVer('{0}','{1}')", firmaAdı, sku), con);
        //    string detay = "";
        //    MySqlDataReader dr = cmd.ExecuteReader();
        //    if (dr.Read()) detay = dr.GetString(0);
        //    dr.Close();
        //    return detay;
        //}

        public object[] resimleri_kontrol_et(ProgressBar p, Label l, MySqlConnection con)
        {
            string sqlVeriçekme = "";
            string sqlAdet = "";
            int adet = 0;
            if (firmaAdı == "dinamik")
            {
                sqlVeriçekme = "select stok_kodu,resim_url from dinamik where stok_kodu not in (select sku from resimler) AND resim_url!=''";
                sqlAdet = "select count(*) from dinamik where stok_kodu not in (select sku from resimler) AND resim_url!=''";
            }
            else if (firmaAdı == "motorasin")
            {
                sqlVeriçekme = "select Concat(Manufacturer,' ',ManufacturerCode),Picture from motorasin where Concat(Manufacturer,' ',ManufacturerCode) not in (select sku from resimler) AND Picture!=''";
                sqlAdet = "select count(*) from motorasin where Concat(Manufacturer,' ',ManufacturerCode) not in (select sku from resimler) AND Picture!=''";
            }
            if (sqlAdet == "") return null;

            MySqlCommand cmd = new MySqlCommand(sqlAdet, con);
            MySqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read()) adet = dr.GetInt32(0);
            dr.Close();
            if (adet == 0) return null;

            p = p ?? new ProgressBar();
            l = l ?? new Label();
            l.Tag = l.Text;
            p.Maximum = adet;
            p.Value = 0;

            List<Resim_işlemleri> resimler = new List<Resim_işlemleri>();

            cmd.CommandText = sqlVeriçekme;
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                if ((p.Value + 1) <= p.Maximum)
                {
                    p.Value++; p.Refresh();
                    l.Text = string.Concat("(", p.Value, "/", p.Maximum, ") ", (string)l.Tag); l.Refresh();
                }
                resimler.Add(new Resim_işlemleri(dr.GetString(0), dr.GetString(1)));

            }
            dr.Close();

            MySqlConnection con1 = new MySqlConnection(veritabanı.connectionString);
            con1.Open();
            p.Value = 0; p.Refresh();
            string hatalılar = "";
            int x = 0;
            foreach (Resim_işlemleri resim in resimler)
            {
                if ((p.Value + 1) <= p.Maximum)
                {
                    p.Value++; p.Refresh();
                    l.Text = string.Concat("(", p.Value, "/", p.Maximum, ") ", (string)l.Tag); l.Refresh();
                }
                try
                {
                    resim.işlemYap(con1);
                    x++;
                }
                catch
                { 
                    hatalılar += resim.url.AbsoluteUri + "\n";
                }
            }
            con1.Close();
            return new object[] { resimler.Count, x, hatalılar };
        }
    }

    public static class firmalar
    {
        public static classFirma Motoraşin;
        public static classFirma Dinamik;
        public static void BAŞLA()
        {
            Motoraşin = new classFirma(veritabanı.MotoraşinTablosu, veritabanı.MotoraşinTabloYapısı, veritabanı.MotoraşinTablosuIndexYapısı, firmaVeriÇekmeMotorları.MotoraşinVeriÇekmeMotoru);
            Dinamik = new classFirma(veritabanı.DinamikTablosu, veritabanı.DinamikTabloYapısı, veritabanı.DinamikTablosuIndexYapısı, firmaVeriÇekmeMotorları.DinamikVeriÇekmeMotoru);
        }

    }


    //public delegate bool yapılacakİş(object[] args);

    //public class yapılacakİşSınıfı
    //{
    //    public yapılacakİş işlem;
    //    public yapılacakİş tetiklenecekDiğerİşlem;
    //    public object[] argsİşlem;
    //    public object[] argsTetiklenecekDiğerİşlem;
    //    public yapılacakİşSınıfı(yapılacakİş işlem, object[] args1, yapılacakİş tetiklenecekDiğerİşlem, object[] args2)
    //    {
    //        this.işlem = işlem;
    //        argsİşlem = args1;
    //        this.tetiklenecekDiğerİşlem = tetiklenecekDiğerİşlem;
    //        argsTetiklenecekDiğerİşlem = args2;
    //    }

    //}

    //public class zamanlamaSınıfı
    //{
    //    public static List<zamanlamaSınıfı> nesneleri = new List<zamanlamaSınıfı>();
    //    public static bool clockAktif = true;
    //    public static void ClockPalse(object state)
    //    {
    //        foreach (zamanlamaSınıfı nesne in nesneleri) nesne.kontrol();
    //    }

    //    public static System.Threading.Timer clock = new System.Threading.Timer(ClockPalse, clockAktif, 5000, 60000);

    //    public bool işYapıldı = false;
    //    public bool enabled = true;

    //    DateTime başlama;
    //    DateTime bitiş;
    //    DateTime denemeZamanı;
    //    void kontrol()
    //    {
    //        if (!enabled || işYapıldı) return;
    //        if (DateTime.Now < denemeZamanı) return;
    //        if (DateTime.Now > bitiş)
    //        {
    //            denemeZamanı = başlama = başlama.AddDays(1);
    //            bitiş = bitiş.AddDays(1);
    //            enabled = true;
    //            return;
    //        }
    //        enabled = false;
    //        bool x = false;
    //        Thread t = new Thread(() => işYapıldı = işlem.işlem(işlem.argsİşlem));
    //        t.Start();
    //        t.Join();
    //        if (işYapıldı)
    //        {
    //            if (işlem.tetiklenecekDiğerİşlem != null)
    //            {
    //                Thread tt = new Thread(() => işlem.tetiklenecekDiğerİşlem(işlem.argsTetiklenecekDiğerİşlem));
    //                tt.Start();
    //            }
    //            return;
    //        }
    //        enabled = true;
    //        denemeZamanı = denemeZamanı.AddMinutes(5);
    //    }
    //    yapılacakİşSınıfı işlem;

    //    public zamanlamaSınıfı(DateTime başlama_, DateTime bitiş_, yapılacakİşSınıfı iş)
    //    {
    //        denemeZamanı = başlama = başlama_;
    //        bitiş = bitiş_;
    //        işlem = iş;
    //        zamanlamaSınıfı.nesneleri.Add(this);
    //    }

    //    ~zamanlamaSınıfı()
    //    {
    //        zamanlamaSınıfı.nesneleri.Remove(this);
    //    }
    //}
}
