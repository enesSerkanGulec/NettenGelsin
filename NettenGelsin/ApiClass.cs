using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.Net;

namespace NettenGelsin
{
    //public class resimTipi
    //{
    //    private const string Format = "{\"filename\": \"{0}\",\"extension\": \"jpg\",\"sortOrder\": 1,\"product\": {\"id\":{1}},\"attachment\":\"data:image/jpeg;base64,{2}\"}";
    //    public string sku;
    //    public string resim_base64;
    //    public resimTipi(string sku_, string resim_base64_) { sku = sku_; resim_base64 = resim_base64_; }
    //    public string GetData_to_BodyFormat(MySqlConnection con)
    //    {
    //        if (resim_base64 == "") return "";
    //        int id = -1;
    //        string dosyaAdı = "";

    //        MySqlCommand cmd = new MySqlCommand("select id from products where sku='" + sku + "'", con);
    //        MySqlDataReader dr = cmd.ExecuteReader();

    //        if (dr.Read()) id = dr.GetInt32(0);
    //        dr.Close();
    //        if (id == -1) return "";

    //        Regex illegalInFileName = new Regex(@"[\\/:*?\s""<>|]");

    //        dosyaAdı = illegalInFileName.Replace(sku, "_");

    //        string body = string.Format(Format, dosyaAdı, id, resim_base64);
    //        return body;
    //    }
    //}

    public static class Thread_Apiİşlemleri
    {
        public static int bekleme = 5;
        public static int tekrar = 10;
        public static int tekrarÇarpan = 3; //request aralığı-->bekleme eğer hata verirse tekrarÇarpan ile çarpılıp tekrar denenecek. kaç tekrar olacağını tekrar belirliyor.
        //static Thread t_kontrol;
        //static List<Thread> İşlem = new List<Thread>();
        //static void kontrol()
        //{
        //    int i;
        //    while (true)
        //    {
        //        i = 0;
        //        while (i < İşlem.Count)
        //        {
        //            if (!İşlem[i].IsAlive) lock (İşlem) { İşlem.RemoveAt(i); }
        //            else i++;
        //        }
        //        Thread.Sleep(33);
        //    }
        //}
        //public static void Başla()
        //{
        //    t_kontrol = new Thread(kontrol);
        //    t_kontrol.IsBackground = true;
        //    t_kontrol.Start();
        //}
        //public static void işEkle(Thread eklenecek_iş)
        //{
        //    İşlem.Add(eklenecek_iş);
        //}
        //public static void işlemler_Suspend()
        //{
        //    foreach (Thread iş in İşlem) if (iş.IsAlive) if (iş.ThreadState == ThreadState.Running) iş.Suspend();
        //}
        //public static void işlemler_Resume()
        //{
        //    foreach (Thread iş in İşlem) if (iş.IsAlive) if (iş.ThreadState == ThreadState.Suspended) iş.Resume();
        //}
    }

    public static class TokenKeySınıfı
    {
        public static DateTime yenilemeZamanı;
        public static string access_token = "";
        //public static int expires_in = 0;
        //public static string token_type = "";
        //public static string scope = "";
        public static string refresh_token = "";
        //public static DateTime yenilenme_zamani;
        //public static DateTime olusturma_zamani;
        //public Timer saat = new Timer(1000);
        //public static Thread kontrol;
        //public static ToolStripStatusLabel bilgilendirme;
        public static void Başla()
        {
            //yenilenme_zamani = olusturma_zamani = DateTime.Now;
            dosyadanOku();
            //kontrol = new Thread(new ThreadStart(kontrol_));
            //kontrol.Start();
        }
        //private static void kontrol_()
        //{
        //    if (bilgilendirme == null) bilgilendirme = new ToolStripStatusLabel();
        //    while (true)
        //    {
        //        if (aktif()) bilgilendirme.Text = "Durum: Aktif    Token key " + geçerlilikSüresi + " daha geçerli. (" + yenilenebilenSonGün + " tarihine kadar yenilenebilir)";
        //        else
        //        {

        //            if (refreshable())
        //            {
        //                bilgilendirme.Text = "Durum: Güncelleniyor    Token key kullanım süresi bitti. Tazeleniyor";
        //                Thread_Apiİşlemleri.işlemler_Suspend();
        //                Thread.Sleep(15000);
        //                bool sonuç;
        //                int bekleme = Thread_Apiİşlemleri.bekleme;
        //                int tekrar = 0;
        //                do
        //                {
        //                    tekrar++;
        //                    sonuç = RefreshToken();
        //                    if (!sonuç)
        //                    {
        //                        Thread.Sleep(bekleme);
        //                        bekleme *= Thread_Apiİşlemleri.tekrarÇarpan;
        //                        if (tekrar == Thread_Apiİşlemleri.tekrar)
        //                        {
        //                            MessageBox.Show("Token Key yenilenemedi.. Programın çalışması durdu..", "Hata !", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                            break;
        //                        }
        //                    }
        //                } while (!sonuç);
        //                if (DateTime.Now.Subtract(olusturma_zamani).Days == 59) email.send("TokenKey yenilenebilirlik için son gün.", "TokenKey yenilenebilme süresi iki aydır. Yarın yenileme işlemi yapılamayacak. Yarın yanileme gerektiği  zaman sizin yardımınıza başvuracağımız bilgisini içeren bir e-posta iletisi alacaksınız. (Bu mesaj hatırlatma amaçlıdır.)");
        //                if (sonuç) Thread_Apiİşlemleri.işlemler_Resume();
        //            }
        //            else
        //            {
        //                bilgilendirme.Text = "Durum: Pasif    Token key kullanım süresi bitti. Tazeleme işlemi için kullanılan anahtarında kullanım süresi bitti. Yeniden yetki verilmesi gerekli.";
        //                GetToken();
        //            }
        //        }
        //        Thread.Sleep(500);
        //    }
        //}
        public static bool dosyadanOku()
        {
            FileInfo f = new FileInfo("managementFiles/token.txt");
            if (f.Exists)
            {
                FileStream file = new FileStream(f.FullName, FileMode.Open);
                bool sonuc = false;
                try
                {
                    StreamReader r = new StreamReader(file, Encoding.UTF8);
                    access_token = r.ReadLine().Trim();
                    refresh_token = r.ReadLine().Trim();
                    yenilemeZamanı = Convert.ToDateTime(r.ReadLine().Trim());
                    sonuc = true;
                    r.Close();
                }
                finally
                {
                    file.Close();
                }
                return sonuc;
            }
            else return false;
        }
        public static bool dosyayaYaz()
        {
            FileStream file = new FileStream("managementFiles/token.txt", FileMode.Create);
            StreamWriter w = new StreamWriter(file, Encoding.UTF8);
            bool sonuc = false;
            try
            {
                w.WriteLine(access_token);
                w.WriteLine(refresh_token);
                w.WriteLine(yenilemeZamanı.ToString());
                sonuc = true;
                w.Close();
            }
            finally
            {
                file.Close();
            }
            return sonuc;
        }
        //public static bool refreshable()
        //{
        //    if (access_token == "") return false;
        //    DateTime son_gecerlilik_tarihi = olusturma_zamani.AddDays(60); // 2 ay refresh yapılabiliyor onun için
        //    return DateTime.Now <= son_gecerlilik_tarihi;
        //}
        //public static string yenilenebilenSonGün
        //{
        //    get
        //    {
        //        return olusturma_zamani.AddDays(60).ToShortDateString();
        //    }
        //}
        //public static string geçerlilikSüresi
        //{
        //    get
        //    {
        //        double fark = yenilenme_zamani.AddSeconds((double)expires_in).Subtract(DateTime.Now).TotalSeconds;
        //        int saat = (int)(fark / 3600.0);
        //        int dakika = (int)((fark - (saat * 3600)) / 60.0);
        //        int saniye = (int)(fark - (saat * 3600) - (dakika * 60));
        //        return (saat > 0 ? saat.ToString() + " saat " : "") + (dakika > 0 ? dakika.ToString() + " dakika " : "") + (saniye > 0 ? saniye.ToString() + " saniye" : "");
        //    }
        //}
        //public static bool aktif()
        //{
        //    if (access_token == "") return false;
        //    double fark = DateTime.Now.Subtract(yenilenme_zamani).TotalSeconds;
        //    return fark < (21600 - 10); // 21600 sn yani 6 saat eder.
        //}
        //public static bool GetToken()
        //{
        //    email.send("GetToken için yardım etmelisiniz.", "TokenKey yenilemek için kod girmelisiniz. Program arayüzünü kontrol ediniz.");
        //    string code = "http://" + Entegrasyon.siteAdresi + "/admin/user/auth?client_id=" + Entegrasyon.clientId + "&response_type=code&state=2b33fdd45jbevd6nam&redirect_uri=" + Entegrasyon.redirectUri;
        //    //client.Timeout = -1;
        //    code = Interaction.InputBox("Api işlemlerinin yapılabilmesi için Token değerine ihtiyaç var.\nAşağıda verilen adres bilgisini tarayıcının adres çubuğuna yapıştırınız. Gelen adres verisi içinden 'code=' den sonraki kod değerini yine aşağıdaki yere giriniz.(Kod değeri 30 sn. geçerlidir. !!)", "Code değeri gerekli !!!", code);
        //    code = code.Trim();
        //    if (code == "")
        //    {
        //        return false;
        //    }

        //    var client = new RestClient("http://" + Entegrasyon.siteAdresi + "/oauth/v2/token");
        //    client.Timeout = -1;
        //    var request = new RestRequest(Method.POST);
        //    request.AlwaysMultipartFormData = true;
        //    request.AddParameter("grant_type", "authorization_code");
        //    request.AddParameter("client_id", Entegrasyon.clientId);
        //    request.AddParameter("client_secret", Entegrasyon.clientSecret);
        //    request.AddParameter("redirect_uri", Entegrasyon.redirectUri);
        //    request.AddParameter("code", code);
        //    try
        //    {
        //        IRestResponse response = client.Execute(request);
        //        if (response.StatusCode.ToString() == "OK")
        //        {
        //            dynamic data = JsonConvert.DeserializeObject(response.Content);
        //            access_token = data["access_token"].Value;
        //            expires_in = (int)data["expires_in"].Value;
        //            token_type = data["token_type"].Value;
        //            scope = data["scope"].Value;
        //            refresh_token = data["refresh_token"].Value;
        //            yenilenme_zamani = olusturma_zamani = DateTime.Now;
        //            dosyayaYaz();
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch (Exception hata)
        //    {
        //        System.Windows.Forms.MessageBox.Show(hata.Message);
        //        return false;
        //    }
        //}
        public static bool RefreshToken()
        {
            if (DateTime.Now.Subtract(yenilemeZamanı).TotalSeconds < 300)
            {
                Form1.hata++;
                log.Yaz("Token key refresh hatası. " + "Hata adedi: " + Form1.hata.ToString() + " Zaman: " + DateTime.Now.ToString(), yenilemeZamanı.ToString());
                return true;
            }
            log.Yaz("TokenKey yenileniyor..", "TokenKey");
            var client = new RestClient("http://" + Entegrasyon.siteAdresi + "/oauth/v2/token");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AlwaysMultipartFormData = true;


            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("client_id", Entegrasyon.clientId);
            request.AddParameter("client_secret", Entegrasyon.clientSecret);
            request.AddParameter("refresh_token", refresh_token);
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);
            if (response.StatusCode.ToString() == "OK")
            {
                dynamic data = JsonConvert.DeserializeObject(response.Content);
                yenilemeZamanı = DateTime.Now;
                access_token = data["access_token"].Value;
                refresh_token = data["refresh_token"].Value;
                Form1.hata = 0;
                dosyayaYaz();
                log.Yaz("TokenKey yenilendi..", "TokenKey");
                return true;
            }
            else
            {
                log.Yaz("HATA !..   TokenKey YENİLENEMEDİ.", "TokenKey");
                return false;
            }
        }
        //public static bool RefreshToken()
        //{
        //    log.Yaz("TokenKey yenileniyor..", "TokenKey");
        //    var client = new RestClient("http://" + Entegrasyon.siteAdresi + "/oauth/v2/token");
        //    client.Timeout = -1;
        //    var request = new RestRequest(Method.POST);
        //    request.AlwaysMultipartFormData = true;


        //    request.AddParameter("grant_type", "refresh_token");
        //    request.AddParameter("client_id", Entegrasyon.clientId);
        //    request.AddParameter("client_secret", Entegrasyon.clientSecret);
        //    request.AddParameter("refresh_token", refresh_token);
        //    ServicePointManager.Expect100Continue = true;
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //    IRestResponse response = client.Execute(request);
        //    if (response.StatusCode.ToString() == "OK")
        //    {
        //        dynamic data = JsonConvert.DeserializeObject(response.Content);
        //        access_token = data["access_token"].Value;
        //        refresh_token = data["refresh_token"].Value;
        //        dosyayaYaz();
        //        log.Yaz("TokenKey yenilendi..", "TokenKey");
        //        return true;
        //    }
        //    else
        //    {
        //        log.Yaz("HATA !..   TokenKey YENİLENEMEDİ.", "TokenKey");
        //        return false;
        //    }
        //}
    }

    public class Entegrasyon
    {
        public static Thread Thread_silmeİşlemi_X;
        public static Thread Thread_silmeİşlemi_İşlemGörmeyecekler;
        public static Thread Thread_eklemeGüncellemeİşlemi;

        public static string clientId = "10_12ty6lpdqgms8wsokg8oo8004kc0oc04g80444wkkw80kocooo";
        public static string clientSecret = "32i34rc58s8w0cgwgk4s8k0gckoc8g4cw8kc44g0gk0ss8s8sg";
        public static string redirectUri = "www.otobkm.com";
        public static string siteAdresi = "www.nettengelsin.com";
        public static string firmaAdı = "nettengelsin-1";
        public static string Content_Type = "application/json; charset=utf-8";
        public static string Content_Encoding = "gzip";

        public static string Api_endPoint = "http://" + firmaAdı + ".myideasoft.com/api/";
        public static string Host = firmaAdı + ".myideasoft.com";
        public static string Content_Length(string Body) { return Body.Length.ToString(); }

        public Entegrasyon()
        {
            DirectoryInfo d = new DirectoryInfo("managementFiles");
            if (!d.Exists) d.Create();
            FileInfo f = new FileInfo("managementFiles/EntegrasyonData.txt");
            if (f.Exists)
            {
                FileStream file = new FileStream(f.FullName, FileMode.Open);
                StreamReader r = new StreamReader(file, Encoding.UTF8);
                try
                {
                    string s;
                    if ((s = r.ReadLine().Trim()) != null) clientId = s;
                    if ((s = r.ReadLine().Trim()) != null) clientSecret = s;
                    if ((s = r.ReadLine().Trim()) != null) redirectUri = s;
                    if ((s = r.ReadLine().Trim()) != null) siteAdresi = s;
                    if ((s = r.ReadLine().Trim()) != null) firmaAdı = s;
                    if ((s = r.ReadLine().Trim()) != null) Content_Type = s;
                    if ((s = r.ReadLine().Trim()) != null) Content_Encoding = s;
                }
                finally
                {
                    r.Close();
                    file.Close();
                }
            }
            else
            {
                FileStream file = new FileStream(f.FullName, FileMode.CreateNew);
                StreamWriter w = new StreamWriter(file, Encoding.UTF8);
                w.WriteLine(clientId);
                w.WriteLine(clientSecret);
                w.WriteLine(redirectUri);
                w.WriteLine(siteAdresi);
                w.WriteLine(firmaAdı);
                w.WriteLine(Content_Type);
                w.WriteLine(Content_Encoding);
                w.Close();
                file.Close();

            }

            if (!TokenKeySınıfı.dosyadanOku()) TokenKeySınıfı.dosyayaYaz();
            //if (!TokenKeySınıfı.aktif())
            //    if (TokenKeySınıfı.refreshable())
            //    {
            //        TokenKeySınıfı.RefreshToken();
            //    }
            //    else TokenKeySınıfı.GetToken();
            //TokenKeySınıfı.kontrol.Start();
        }

        public static IRestResponse POST(string istek, string body)
        {
            var client = new RestClient(Entegrasyon.Api_endPoint + istek);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Host", Entegrasyon.Host);
            request.AddHeader("Content-Type", Entegrasyon.Content_Type);
            request.AddHeader("Content-Encoding", Entegrasyon.Content_Encoding);
            request.AddHeader("Content-Length", Entegrasyon.Content_Length(body));
            request.AddHeader("Authorization", "Bearer " + TokenKeySınıfı.access_token);
            request.AddParameter("application/json; charset=utf-8", body, ParameterType.RequestBody);

            IRestResponse response;
            while (true)
            {
                response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) TokenKeySınıfı.RefreshToken();
                else break;
            }
            return response;
        }
        public static IRestResponse PUT(string istek, string id, string body)
        {
            var client = new RestClient(Entegrasyon.Api_endPoint + istek + "/" + id);
            client.Timeout = -1;
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Host", Entegrasyon.Host);
            request.AddHeader("Content-Type", Entegrasyon.Content_Type);
            request.AddHeader("Content-Encoding", Entegrasyon.Content_Encoding);
            request.AddHeader("Content-Length", Entegrasyon.Content_Length(body));
            request.AddHeader("Authorization", "Bearer " + TokenKeySınıfı.access_token);
            request.AddParameter("application/json; charset=utf-8", body, ParameterType.RequestBody);
            IRestResponse response;
            while (true)
            {
                response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) TokenKeySınıfı.RefreshToken();
                else break;
            }
            return response;
        }
        public static IRestResponse GET(string istek, string parametre = "")
        {
            var client = new RestClient(Entegrasyon.Api_endPoint + istek + (parametre != "" ? "?" + parametre : ""));
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Host", Entegrasyon.Host);
            request.AddHeader("Content-Type", Entegrasyon.Content_Type);
            request.AddHeader("Authorization", "Bearer " + TokenKeySınıfı.access_token);
            IRestResponse response;
            while (true)
            {
                response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) TokenKeySınıfı.RefreshToken();
                else break;
            }
            return response;
        }
        public static IRestResponse DELETE(string istek, string id)
        {
            var client = new RestClient(Entegrasyon.Api_endPoint + istek + "/" + id);

            var request = new RestRequest(Method.DELETE);
            request.AddHeader("Host", Entegrasyon.Host);
            request.AddHeader("Content-Type", Entegrasyon.Content_Type);
            request.AddHeader("Authorization", "Bearer " + TokenKeySınıfı.access_token);
            IRestResponse response;
            while (true)
            {
                response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) TokenKeySınıfı.RefreshToken();
                else break;
            }
            return response;
        }


        public static float kurNedir(string currencyAbbr)
        {
            IRestResponse data;
            int bekleme = Thread_Apiİşlemleri.bekleme;
            while (true)
            {
                data = Entegrasyon.GET("currencies", "abbr=" + currencyAbbr);
                if (data.IsSuccessful || bekleme > 3000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }
            if (data.IsSuccessful)
            {
                dynamic d = JsonConvert.DeserializeObject(data.Content);
                if (d[0]["id"] == null) return -1;
                else return d[0]["sellingPrice"];
            }
            else return -1;

        }
        // ------------------------- İdeasoft APİ ile İdeaSoft Sunucusunda işlem Görüyor -------------------------------------

        //incelendi
        public static object[] ÜrünKategoriBağı(int id, string ürünID, string categoryId)
        {
            string body;
            if (id == -1) body = "{\"sortOrder\":null,\"product\":{\"id\":" + ürünID + "},\"category\":{\"id\":" + categoryId + "}}";
            else body = "{\"id\":" + id.ToString() + ",\"sortOrder\":null,\"product\":{\"id\":" + ürünID + "},\"category\":{\"id\":" + categoryId + "}}";
            int bekleme = Thread_Apiİşlemleri.bekleme;
            IRestResponse data;
            while (true)
            {
                data = id == -1 ? (Entegrasyon.POST("product_to_categories", body)) : (Entegrasyon.PUT("product_to_categories", id.ToString(), body));
                //ürünün product_to_category tablosundaki id değeri hatalı olma ihtimali var. sonuç notFound dönüyorsa öyledir.
                //o zaman get isteğini product parametresi ile çağırıp güncel değeri aldırmak gerekli.
                if (data.IsSuccessful || bekleme > 3000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }
            if (data.IsSuccessful)
            {
                dynamic d = JsonConvert.DeserializeObject(data.Content);
                if (d["id"] == null) return new object[] { false };
                else return new object[] { true, d };
            }
            else return new object[] { false };
        }

        //incelendi
        public static object[] SatınAlmaLimitiKalemi(int id, string ürünID, string satınAlmaLimitiID)
        {
            string body;
            if (id == -1) body = "{\"limitation\":{\"id\":" + satınAlmaLimitiID + "},\"product\":{\"id\":" + ürünID + "}}";
            else body = "{\"id\":" + id.ToString() + ",\"limitation\":{\"id\":" + satınAlmaLimitiID + "},\"product\":{\"id\":" + ürünID + "}}";
            int bekleme = Thread_Apiİşlemleri.bekleme;
            IRestResponse data;
            while (true)
            {
                data = id == -1 ? (Entegrasyon.POST("purchase_limitation_items", body)) : (Entegrasyon.PUT("purchase_limitation_items", id.ToString(), body));
                if (data.IsSuccessful || bekleme > 3000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }
            if (data.IsSuccessful)
            {
                dynamic d = JsonConvert.DeserializeObject(data.Content);
                if (d["id"] == null) return new object[] { false };
                else return new object[] { true, d };
            }
            else return new object[] { false };
        }

        //incelendi
        public static object[] ÜrünDetay(int id, string sku, string ürünID, string detail)
        {
            string body;
            if (id == -1) body = "{\"sku\":\"" + sku + "\",\"details\":\"" + detail + "\",\"product\":{\"id\":" + ürünID + "}}";
            else body = "{\"id\":" + id.ToString() + ",\"sku\":\"" + sku + "\",\"details\":\"" + detail + "\",\"product\":{\"id\":" + ürünID + "}}";
            int bekleme = Thread_Apiİşlemleri.bekleme;
            IRestResponse data;
            while (true)
            {
                data = id == -1 ? (Entegrasyon.POST("product_details", body)) : (Entegrasyon.PUT("product_details", id.ToString(), body));
                if (data.IsSuccessful || bekleme > 3000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }
            if (data.IsSuccessful)
            {
                dynamic d = JsonConvert.DeserializeObject(data.Content);
                if (d["id"] == null) return new object[] { false };
                else return new object[] { true, d };
            }
            else return new object[] { false };
        }

        //incelendi
        public static object[] KategoriOluştur(string name, int parentId, MySqlConnection con)
        {
            string body = "{\"name\":\"" + name + "\",\"sortOrder\":" + (parentId == -1 ? IdeaSoftVeritabanı.sortOrderGetir(con).ToString() : "999") + ",\"status\":1,\"displayShowcaseContent\":0,\"showcaseContentDisplayType\": 1,\"percent\":1.0,\"parent\":" + (parentId == -1 ? "null" : ("{\"id\":" + parentId.ToString() + "}")) + "}";
            int bekleme = Thread_Apiİşlemleri.bekleme;
            IRestResponse data;
            while (true)
            {
                data = POST("categories", body);
                if (data.IsSuccessful || bekleme > 3000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }
            if (data.IsSuccessful)
            {
                dynamic d = JsonConvert.DeserializeObject(data.Content);
                if (d["id"] == null) return new object[] { false };
                else return new object[] { true, d };
            }
            else return new object[] { false };
        }

        public static object[] ResimOluştur(resimTipi resim, MySqlConnection con)
        {
            string body = resim.getData_bodyFormat;
            if (body == "") return new object[] { false };
            int bekleme = Thread_Apiİşlemleri.bekleme;
            IRestResponse data;
            while (true)
            {
                data = POST("product_images", body);
                if (data.IsSuccessful || bekleme > 3000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }
            if (data.IsSuccessful)
            {
                dynamic d = JsonConvert.DeserializeObject(data.Content);
                if (d["id"] == null) return new object[] { false };
                else return new object[] { true, d };
            }
            else return new object[] { false, data };
        }

        //marka(brand) incelendi
        public static object[] MarkaOluştur(string name)
        {
            string body = "{\"name\":\"" + name + "\",\"sortOrder\":999,\"status\":1}";
            IRestResponse data;
            int bekleme = Thread_Apiİşlemleri.bekleme;
            while (true)
            {
                data = POST("brands", body);
                if (data.IsSuccessful || bekleme > 3000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }
            if (data.IsSuccessful)
            {
                dynamic d = JsonConvert.DeserializeObject(data.Content);
                if (d["id"] == null) return new object[] { false };
                else return new object[] { true, d };
            }
            else return new object[] { false };
        }

        //satın Alma Limiti incelendi
        public static object[] SatınAlmaLimitiOluştur(string limit)
        {
            string body = "{\"name\":\"" + limit + " adet" + "\",\"minimumLimit\":" + limit + ",\"maximumLimit\":1000,\"type\":\"product\",\"status\":true}";
            int bekleme = Thread_Apiİşlemleri.bekleme;
            IRestResponse data;
            while (true)
            {
                data = POST("purchase_limitations", body);
                if (data.IsSuccessful || bekleme > 3000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }

            if (data.IsSuccessful)
            {
                dynamic d = JsonConvert.DeserializeObject(data.Content);
                if (d["id"] == null) return new object[] { false };
                else return new object[] { true, d };
            }
            else return new object[] { false };
        }

        public static bool resimSil(int resimId)
        {
            int bekleme = Thread_Apiİşlemleri.bekleme;
            IRestResponse data;
            while (true)
            {
                data = DELETE("product_images", resimId.ToString());
                if (data.IsSuccessful || bekleme > 3000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }
            if (data.Content == "") return true;
            else return false;
        }

        public static bool ÜrünKategoriBağıSil(string ürünId, MySqlConnection con)
        {
            object[] x = IdeaSoftVeritabanı.productToCategoryIDGetir(ürünId, con);
            int id = (int)x[0];
            if (id == -1) return true;
            int bekleme = Thread_Apiİşlemleri.bekleme;
            IRestResponse data;
            while (true)
            {
                data = DELETE("product_to_categories", id.ToString());
                if (data.IsSuccessful || bekleme > 3000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }
            if (data.Content == "") return true;
            else return false;
        }
        public static bool SatınAlmaLimitiKalemiSil(int purchase_limitation_items_ID)
        {
            //object[] x = IdeaSoftVeritabanı.purchase_limitation_items_ID_ve_limitID_Getir(purchase_limitation_items_ID, con);
            //int id = (int)x[0];
            //if (id == -1) return true;
            int bekleme = Thread_Apiİşlemleri.bekleme;
            IRestResponse data;
            while (true)
            {
                data = DELETE("purchase_limitation_items", purchase_limitation_items_ID.ToString());
                if (data.IsSuccessful || bekleme > 3000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }
            return data.StatusCode == System.Net.HttpStatusCode.NoContent;
        }
        public static bool ÜrünDetaySil(string ürünId, MySqlConnection con)
        {
            object[] x = IdeaSoftVeritabanı.ürünDetailsIDGetir(ürünId, con);
            int id = (int)x[0];
            if (id == -1) return true;
            int bekleme = Thread_Apiİşlemleri.bekleme;
            IRestResponse data;
            while (true)
            {
                data = DELETE("product_details", id.ToString());
                if (data.IsSuccessful || bekleme > 3000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }
            if (data.Content == "") return true;
            else return false;
        }

        // ----------------- Yerel İdeasoft veritabanını kontrol ediyor. Varsa oradan true,ID getiriyor. 
        // ----------------- Yoksa hem ideSofta hem de yerel İdeasoft veritabanına ekleme yapıp true,Id değerini getiriyor. 
        // ----------------- Başarısız ise False değerini döndürüyor.

        //incelendi
        public static bool SatınAlmaLimitiKalemiAyarla(string ürünID, int satınAlmaLimiti, MySqlConnection con)
        {
            int satınAlmaLimitiID = -1;
            if (satınAlmaLimiti > 1)
            {
                satınAlmaLimitiID = IdeaSoftVeritabanı.satınAlmaLimitiIDGetir(satınAlmaLimiti.ToString(), con);
                if (satınAlmaLimitiID == -1)
                {
                    //satınAlmaLimiti yerel ideasoft veritabanında yoksa ideasoft api ile eklemeye çalışıyor.
                    object[] y = SatınAlmaLimitiOluştur(satınAlmaLimiti.ToString());
                    if ((bool)y[0])
                    {
                        satınAlmaLimitiID = IdeaSoftVeritabanı.satınAlmaLimitiOluştur(y[1], con);
                        //satınAlmaLimiti ideasoft(yerel) veritabanında oluşturuldu. ve ID değeri getirildi.
                    }
                    else return false;
                }
            }
            //satınAlmaLimiti var ve ID değerini aldık.
            if (satınAlmaLimitiID == -1 && satınAlmaLimiti > 1) return false;

            object[] x = IdeaSoftVeritabanı.purchase_limitation_items_ID_ve_limitID_Getir(ürünID, con);
            int id = (int)x[0];
            if (id != -1 && (int)x[1] == satınAlmaLimitiID) return true;
            if (id == -1 && satınAlmaLimiti == 1) return true;

            if (satınAlmaLimiti == 1) //silinecek
            {
                if (SatınAlmaLimitiKalemiSil((int)x[0]))
                {
                    MySqlCommand cmd = new MySqlCommand(string.Format("delete from purchase_limitation_items where id={0}", (int)x[0]), con);
                    cmd.ExecuteNonQuery();
                    return true;
                }
                else return false;
            }
            else //Ekleme/güncelleme yapılacak. SatınAlmaLimitiKalemi metodu varsa güncelliyor, yoksa ekliyor
            {
                object[] sonuç = SatınAlmaLimitiKalemi(id, ürünID, satınAlmaLimitiID.ToString());
                if ((bool)sonuç[0])
                {
                    IdeaSoftVeritabanı.purchase_limitation_itemsOluştur(sonuç[1], id == -1, con);
                    return true;
                }
                else return false;
            }
        }

        //incelendi
        public static bool ÜrünKategoriBağıAyarla(string ürünID, string categoryID, MySqlConnection con)
        {
            object[] x = IdeaSoftVeritabanı.productToCategoryIDGetir(ürünID, con);
            int id = (int)x[0];
            if (id != -1 && (string)x[1] == categoryID) return true;

            object[] sonuç = ÜrünKategoriBağı(id, ürünID, categoryID);
            if ((bool)sonuç[0])
            {
                IdeaSoftVeritabanı.productToCategoryOluştur(sonuç[1], id == -1, con);
                return true;
            }
            else return false;
        }

        //incelendi
        public static bool ÜrünDetayAyarla(string sku, string ürünID, string detail, MySqlConnection con)
        {
            object[] x = IdeaSoftVeritabanı.ürünDetailsIDGetir(ürünID, con);
            int id = (int)x[0];
            if (id != -1 && (string)x[1] == detail) return true;

            object[] sonuç = ÜrünDetay(id, sku, ürünID, detail);
            if ((bool)sonuç[0])
            {
                IdeaSoftVeritabanı.ürünDetailsOluştur(sonuç[1], id == -1, con);
                return true;
            }
            else return false;
        }

        //incelendi
        public static object[] KategoriIDGetir(string category_path, MySqlConnection con)
        {
            string[] kategoriler = category_path.Split('>');
            int parent_id = -1;
            for (int i = 0; i < kategoriler.Length; i++)
            {
                int id = IdeaSoftVeritabanı.kategoryIDGetir(kategoriler[i], parent_id, con);
                if (id == -1)
                {
                    object[] sonuç = KategoriOluştur(kategoriler[i], parent_id, con);
                    if ((bool)sonuç[0]) parent_id = IdeaSoftVeritabanı.kategoriOluştur(sonuç[1], con);
                    else return new object[] { false };
                }
                else parent_id = id;
            }
            return new object[] { true, parent_id };
        }

        //incelendi
        public static object[] MarkaIDGetir(string marka, MySqlConnection con)
        {
            int id = IdeaSoftVeritabanı.brandIdGetir(marka, con);
            if (id == -1)
            {
                object[] sonuç = MarkaOluştur(marka);
                if ((bool)sonuç[0]) id = IdeaSoftVeritabanı.brandOluştur(sonuç[1], con);
                else return new object[] { false };
            }
            return new object[] { true, id };
        }
        //public static object[] SatınAlmaLimitiIDGetir(string limit, MySqlConnection con)
        //{
        //    int id = IdeaSoftVeritabanı.satınAlmaLimitiIDGetir(limit, con);
        //    if (id == -1)
        //    {
        //        int bekleme = Thread_Apiİşlemleri.bekleme;
        //        object[] sonuç;
        //        while (true)
        //        {
        //            Thread.Sleep(bekleme);
        //            sonuç = SatınAlmaLimitiOluştur(limit);
        //            if ((bool)sonuç[0] || bekleme > 10000) break;
        //            bekleme *= 3;
        //        }
        //        if ((bool)sonuç[0]) id = IdeaSoftVeritabanı.satınAlmaLimitiOluştur(sonuç[1], con);
        //        else return new object[] { false };
        //    }
        //    return new object[] { true, id };
        //}
        public static IRestResponse[] datas = new IRestResponse[0];

        public static bool ÜrünSil(int product_id, MySqlConnection con)
        {
            int bekleme = Thread_Apiİşlemleri.bekleme;
            IRestResponse data;
            while (true)
            {
                data = DELETE("products", product_id.ToString());
                if (data.IsSuccessful || bekleme > 10000) break;
                Thread.Sleep(bekleme);
                bekleme *= 3;
            }
            if (data.Content == "") return true;
            else return false;
        }

        public static object[] HATA(string hataMesajı)
        {
            log.Yaz(hataMesajı, "Hata");
            return new object[] { false };
        }

        public static string uzunlukAyarla(string ifade, int uzunluk = 255)
        {
            if (ifade.Length <= 255) return ifade;
            ifade = ifade.Substring(0, 256).Trim() + " ";
            for (int i = ifade.Length - 1; i > 0; i--)
                if (ifade.Substring(i - 1, 2) == ", ") return ifade.Substring(0, i - 1);
            return ifade.Substring(0, 255).Trim();
        }

        public static object ürünEkleGüncelle(object[] ürünbilgileri, MySqlConnection con)
        {
            string durum = (string)ürünbilgileri[17];
            if (durum == "X" || durum == "Y") return HATA("[durum Y veya durum X] " + (string)ürünbilgileri[2]);
            object[] kategoriID;
            object[] markaID;
            int currencyID = IdeaSoftVeritabanı.currencyIDGetir((string)ürünbilgileri[8], con);
            if (currencyID == -1) return HATA("[currencyId -1 döndü.] " + (string)ürünbilgileri[2]);
            string body;
            string ürünID = durum == "E" ? "" : ((int)ürünbilgileri[18]).ToString();
            kategoriID = KategoriIDGetir((string)ürünbilgileri[5], con);
            if (!(bool)kategoriID[0]) HATA("Kategori oluşturulamadı.(" + (string)ürünbilgileri[5] + ")] " + (string)ürünbilgileri[2]);
            markaID = MarkaIDGetir((string)ürünbilgileri[4], con);
            if (!(bool)markaID[0]) return HATA("[" + (durum == "E" ? "ÜRÜN EKLENEMEDİ" : "ÜRÜN GÜNCELLENEMEDİ") + " Sebebi: Marka oluşturulamadı.(" + (string)ürünbilgileri[4] + ")]" + (string)ürünbilgileri[2]);
            string slug = (string)ürünbilgileri[19];
            if (slug != "") slug = " \"slug\":\"" + slug + "\",";
            string name = (string)ürünbilgileri[2];
            if (!(durum == "M" || durum == "C" || durum == "B"))
            {
                body = string.Format("{{\"name\": \"{0}\"," + slug + " \"fullName\": \"{0}\", \"sku\": \"{1}\", \"barcode\": \"{2}\", \"price1\": {3}, \"warranty\": 2, \"tax\": 18, \"stockAmount\": {4}, \"stockTypeLabel\": \"{5}\", \"discount\": {6}, \"discountType\": 1, \"moneyOrderDiscount\": 3, \"status\": 1, \"taxIncluded\": 0, \"distributor\": \"{7}\", \"customShippingDisabled\": 1, \"metaKeywords\": \"{8}\", \"metaDescription\": \"{0}\", \"searchKeywords\": \"{8}\", \"installmentThreshold\": \"-\", \"discountedSortOrder\":{11}, \"brand\": {{ \"id\": {9}}}, \"currency\": {{ \"id\": {10}}}}}", (name.Length <= 255 ? name : name.Substring(0, 255)), (string)ürünbilgileri[1], ((string)ürünbilgileri[6]).Replace("\t", ""), ((float)ürünbilgileri[7]).ToString().Replace(',', '.'), ürünbilgileri[10], (string)ürünbilgileri[11], (int)ürünbilgileri[12], (string)ürünbilgileri[14], uzunlukAyarla((string)(ürünbilgileri[3].GetType().Equals(typeof(DBNull)) ? "" : ürünbilgileri[3])), (int)markaID[1], currencyID, ürünbilgileri[16].GetType().Equals(typeof(DBNull)) ? "null" : ürünbilgileri[16].ToString());
                body = body.Replace("\t", " ").Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");

                IRestResponse data;
                int bekleme = Thread_Apiİşlemleri.bekleme;
                while (true)
                {
                    data = durum == "E" ? Entegrasyon.POST("products", body) : Entegrasyon.PUT("products", ürünID, body);
                    if (data.IsSuccessful || bekleme > 3000) break;
                    Thread.Sleep(bekleme);
                    bekleme *= 3;
                }
                if (data.IsSuccessful)
                {
                    dynamic d = JsonConvert.DeserializeObject(data.Content);
                    ürünID = IdeaSoftVeritabanı.ürünOluştur(d, durum == "E", con).ToString();
                }
            }
            if (ürünID != "")
            {
                if (durum == "E" || durum.Contains("M"))
                    if (!SatınAlmaLimitiKalemiAyarla(ürünID, (int)ürünbilgileri[9], con))
                        HATA("[" + (durum == "E" ? "ÜRÜN EKLENEMEDİ" : "ÜRÜN GÜNCELLENEMEDİ") + " Sebebi: Satın Alma Limiti Kalemi oluşturulamadı.(ÜrünID: " + ürünID + ", Paket Miktarı: " + ((int)ürünbilgileri[9]).ToString() + ")] " + (string)ürünbilgileri[2]);
                if ((durum == "E" || durum.Contains("C")) && (bool)kategoriID[0])
                    if (!ÜrünKategoriBağıAyarla(ürünID, ((int)kategoriID[1]).ToString(), con))
                        HATA("[" + (durum == "E" ? "ÜRÜN EKLENEMEDİ" : "ÜRÜN GÜNCELLENEMEDİ") + " Sebebi: Ürün-kategori bağı oluşturulamadı.(ÜrünID: " + ürünID + ", Kategori: " + (string)ürünbilgileri[5] + ")] " + (string)ürünbilgileri[2]);
                if (durum == "E" || durum.Contains("B") || durum.Contains("C") || durum.Contains("D"))
                    if (!ÜrünDetayAyarla((string)ürünbilgileri[1], ürünID, (string)ürünbilgileri[15], con))
                        HATA("[" + (durum == "E" ? "ÜRÜN EKLENEMEDİ" : "ÜRÜN GÜNCELLENEMEDİ") + " Sebebi: Ürün detail oluşturulamadı.(ÜrünID: " + ürünID + ", Detail: " + (string)ürünbilgileri[15] + ")] " + (string)ürünbilgileri[2]);
                return new object[] { true };
            }
            return "";
            //else return HATA("[" + (durum == "E" ? "ÜRÜN OLUŞTURULAMADI." : "ÜRÜN GÜNCELLENEMEDİ.") + " (Ürün sku: " + (string)ürünbilgileri[1] + ") " + (string)ürünbilgileri[2]);
        }

        public static int ürünleriSil(string sql, ProgressBar pb, Label lb)
        {
            string text = lb.Text;
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(sql, con);
            MySqlDataReader dr = cmd.ExecuteReader();
            bool sonuç;
            List<int> product_id = new List<int>();

            while (dr.Read()) product_id.Add(dr.GetInt32(0));
            dr.Close();

            if (product_id.Count >= 10000)
            {
                string s = string.Format("Silinecek ürün sayısı {0} adettir. ", product_id.Count);
                email.send(s + "Program sizin cevabınız bekliyor.", "Onay gerekli");
                if (DialogResult.No == MessageBox.Show(s + "İşleme devam etmek istiyor musunuz ? ", "İşleme devam için onayınız gerekli", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                {
                    MessageBox.Show("İşlemi iptal ettiniz..");
                    return -1;
                }
            }

            log.Yaz("Silme işlemi Başlıyor. " + product_id.Count.ToString() + " ürün silinecek");

            pb.Maximum = product_id.Count;
            pb.Value = 0;
            pb.Visible = true;

            List<string> silinenIDler = new List<string>();
            for (int i = 0; i < product_id.Count; i++)
            {
                pb.Value++; pb.Refresh(); lb.Text = string.Concat("(", pb.Value, '/', pb.Maximum, ") ", text); lb.Refresh();
                sonuç = Entegrasyon.ÜrünSil(product_id[i], con);
                if (sonuç) silinenIDler.Add("'" + product_id[i].ToString() + "'");
                else log.Yaz("(Product_ID: " + product_id[i] + ")" + "SİLİNEMEDİ.", "Hata");
            }
            con.Close();
            if (silinenIDler.Count > 0) IdeaSoftVeritabanı.ürünleriSil((silinenIDler.Count > 0 ? " in (" : "=") + string.Join(",", silinenIDler) + (silinenIDler.Count > 0 ? ")" : ""));
            log.Yaz("Silme işlemi BİTTİ..");
            pb.Visible = false;
            return pb.Maximum;
        }

        public enum işlemTipi { eklenecek = 'E', güncellenecek = 'G', sadeceGüncelleme = 'S' };

        //public static int olmayanlarıStokMiktarınıSıfırla(ProgressBar pb = null, Label lb = null)
        //{

        //}

        public static int sadeceGüncelle(ProgressBar pb = null, Label lb = null, bool debug = false)
        {
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();

            string debugDosya = string.Format(@"
DROP TABLE if EXISTS debugDosya; 
create table debugdosya 
(  
	(SELECT 
		t.id,
		t.stok_kodu,
		t.label,
		t.metaKeywords,
		t.brand,
		t.category_path,
		t.barcode,
		t.price,
		t.currency_abbr,
		t.paket_miktari,
		t.stok_amount,
		t.stock_type,
		t.discount,
		t.discountType,
		t.nereden,
		t.detail,
		t.discountedSortOrder,
		t.d,
		t.p_id,
		t.s
	FROM 
	(
		SELECT 
			ortak.*, 
			durumFilitre(durumNedir(ortak.stok_kodu, {0}, {1}, {2})) AS d, 
			ideasoft.id as p_id, 
			if(slug.slug is null,'',slug.slug) AS s,
			ideasoft.currency_abbr AS eskiBirim,
			ideasoft.price AS eskiFiyat,
			ortak.currency_abbr AS yeniBirim,
			ortak.price AS yeniFiyat
		from 
			(ortak inner join ideasoft on ortak.stok_kodu=ideasoft.stok_kodu) left join slug on ortak.stok_kodu = slug.sku) t 
        WHERE 
		(t.d REGEXP '[SPM]') OR ((t.d REGEXP '[A]') AND (fiyatDegisimYuzdesi(t.eskiBirim,t.eskiFiyat,t.yeniBirim,t.yeniFiyat,{0},{1})>5))
    )
	UNION
	(SELECT 
	    ideasoft.id,
	    ideasoft.stok_kodu,
	    ideasoft.label,
	    products.metaKeywords,
	    ideasoft.brand,
	    ideasoft.category_path,
	    ideasoft.barcode,
	    ideasoft.price,
	    ideasoft.currency_abbr,
	    ideasoft.paket_miktari,
	    0,#ideasoft.stok_amount,
	    ideasoft.stock_type,
	    ideasoft.discount,
	    ideasoft.discountType,
	    products.distributor,
	    product_details.details,
	    ideasoft.discountedSortOrder,
	    'P', #durum
	    ideasoft.id,
	    products.slug
    FROM (ideasoft INNER JOIN (products INNER JOIN product_details ON products.id=product_details.product_id) ON ideasoft.id=products.id) 
	WHERE ideasoft.stok_kodu NOT IN (SELECT ortak.stok_kodu FROM ortak) 
	)
);", Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.'), veritabanı.FiyatDeğişimYüzdesi);
           
            MySqlCommand cmd1 = new MySqlCommand(debugDosya, con);
            cmd1.CommandTimeout = 600;
            veritabanı.cmdExecute(cmd1);


            string cmdText = "SELECT * from debugDosya";

            MySqlCommand cmd = new MySqlCommand(cmdText, con);
            cmd.CommandTimeout = 600;
            MySqlDataReader dr = cmd.ExecuteReader();

            int i = 0;
            List<object[]> ürünler = new List<object[]>();
            object[] ürünBilgileri;
            while (dr.Read())
            {
                //ilk 0.-16. eleman ortak tablosundan 17. durum, 18. ürünID ve 19. de slug değeri (indis 0-19 arası olmalı)
                ürünBilgileri = new object[20];
                dr.GetValues(ürünBilgileri);
                ürünler.Add(ürünBilgileri);
            }
            dr.Close();

            //bool başarılı;
            lb = lb ?? new Label();
            pb = pb ?? new ProgressBar();
            pb.Maximum = ürünler.Count;
            pb.Value = 0;
            pb.Visible = true;
            string text = lb.Text;
            while (ürünler.Count > 0)
            {
                i++;
                pb.Value++; pb.Refresh(); lb.Text = string.Concat("(", pb.Value, '/', pb.Maximum, ") ", text); lb.Refresh();
                Entegrasyon.ürünEkleGüncelle(ürünler[0], con);
                ürünler.RemoveAt(0);
            }
            con.Close();

            pb.Visible = false;
            return pb.Maximum;
        }

        public static int ürünleriEkle(işlemTipi işlem = işlemTipi.güncellenecek, ProgressBar pb = null, Label lb = null)
        {
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();

            string cmdText = string.Format(işlem == işlemTipi.eklenecek ? ("SELECT ortak.*, 'E', '', if(slug.slug is null,'',slug.slug) from (ortak left join slug on ortak.stok_kodu=slug.sku) where stok_kodu not in (select stok_kodu from ideasoft)") : ("SELECT t.* FROM (SELECT ortak.*, durumNedir(ortak.stok_kodu, {0}, {1}, {2}) AS d, ideasoft.id as p_id, if (slug.slug is null,'',slug.slug) AS s FROM ((ortak inner join ideasoft on ortak.stok_kodu=ideasoft.stok_kodu) left join slug on ortak.stok_kodu = slug.sku)) AS t WHERE t.d <> 'E' AND t.d <> 'Y' AND t.d <> 'I'"), Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.'), veritabanı.FiyatDeğişimYüzdesi);

            MySqlCommand cmd = new MySqlCommand(cmdText, con);
            cmd.CommandTimeout = 600;
            MySqlDataReader dr = cmd.ExecuteReader();

            int i = 0;
            List<object[]> ürünler = new List<object[]>();
            object[] ürünBilgileri;
            while (dr.Read())
            {
                //ilk 0.-16. eleman ortak tablosundan 17. durum, 18. ürünID ve 19. de slug değeri (indis 0-19 arası olmalı)
                ürünBilgileri = new object[20];
                dr.GetValues(ürünBilgileri);
                ürünler.Add(ürünBilgileri);
            }
            dr.Close();

            //bool başarılı;
            lb = lb ?? new Label();
            pb = pb ?? new ProgressBar();
            pb.Maximum = ürünler.Count;
            pb.Value = 0;
            pb.Visible = true;
            string text = lb.Text;
            while (ürünler.Count > 0)
            {
                i++;
                pb.Value++; pb.Refresh(); lb.Text = string.Concat("(", pb.Value, '/', pb.Maximum, ") ", text); lb.Refresh();
                if ((string)ürünler[0][17] == "Y") { ürünler.RemoveAt(0); continue; }
                Entegrasyon.ürünEkleGüncelle(ürünler[0], con);
                ürünler.RemoveAt(0);
            }
            con.Close();

            pb.Visible = false;
            return pb.Maximum;
        }

        public static void resimleri_Ekle_Güncelle(List<resimTipi> resimler, ProgressBar p, Label l)
        {
            if (resimler.Count == 0) return;
            object[] işlenecekler = resimTipi.yayında_olanları_getir(resimler);
            List<int> silinecek_resim_idler = (List<int>)işlenecekler[0];
            resimler = (List<resimTipi>)işlenecekler[1];
            p.Maximum = silinecek_resim_idler.Count;

            p.Value = 0;
            for (int i = 0; i < silinecek_resim_idler.Count; i++)
            {
                p.Value++;
                l.Text = "Güncelleme için siliniyor.." + p.Value.ToString() + "/" + p.Maximum.ToString();
                p.Refresh(); l.Refresh();
                if (!resimSil(silinecek_resim_idler[i])) { silinecek_resim_idler.RemoveAt(i); i--; }
            }
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            IdeaSoftVeritabanı.resimleriSil(silinecek_resim_idler, con);

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            cmd.CommandTimeout = 600;


            p.Maximum = resimler.Count;
            p.Value = 0;
            object[] sonuç;
            int x;
            int başarılı = 0;
            for (int i = 0; i < resimler.Count; i++)
            {
                p.Value++;
                l.Text = "Ekleniyor.." + p.Value.ToString() + "/" + p.Maximum.ToString();
                p.Refresh(); l.Refresh();
                sonuç = ResimOluştur(resimler[i], con);
                if ((bool)sonuç[0])
                {
                    başarılı++;
                    x = IdeaSoftVeritabanı.resimOluştur(sonuç[1], con);
                    log.Yaz("Sku: " + resimler[i].sku + " için resim eklendi. id=" + x.ToString(), "Resim Ekleme");
                }
                else log.Yaz("Sku: " + resimler[i].sku + " için resim eklenemedi. Hata mesajı: " + ((IRestResponse)sonuç[1]).ErrorMessage, "Resim Ekleme Hatası");
            }
        }

        //public static void resimleriEkle(List<resimTipi> eklenecekler)
        //{
        //    MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
        //    con.Open();
        //    object[] sonuç;
        //    int x;
        //    int başarılı = 0;
        //    for (int i = 0; i < eklenecekler.Count; i++)
        //    {
        //        sonuç = ResimOluştur(eklenecekler[i], con);
        //        if ((bool)sonuç[0])
        //        {
        //            başarılı++;
        //            x = IdeaSoftVeritabanı.resimOluştur(sonuç[1], con);
        //            log.Yaz("Sku: " + eklenecekler[i].sku + " için resim eklendi. id=" + x.ToString(), "Resim Ekleme");
        //        }
        //        else log.Yaz("Sku: " + eklenecekler[i].sku + " için resim eklenemedi. Hata mesajı: " + ((IRestResponse)sonuç[1]).ErrorMessage, "Resim Ekleme Hatası");
        //    }
        //    email.send("Resim ekleme bitti", "Resim ekleme işlemi bitti. " + eklenecekler.Count.ToString() + " tane resimden " + başarılı.ToString() + " tanesi başarılı bir şekilde gönderildi. Başarısız olanlar ve nedeni için log tablosuna bakınız.");
        //}
    }
}