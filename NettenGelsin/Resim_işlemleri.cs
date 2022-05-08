using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NettenGelsin
{
    public enum resim_durumu { resim_yükleme_hatası = -1, işlem_yok = 0, eklenecek = 1, güncellenecek = 2 }
    public enum resim_nereden { dosyadan = 1, firmadan = 2, ideasofttan = 3 }

    class resim_yayında_sınıfı
    {
        public int product_id;
        public int product_images_id;

        public resim_yayında_sınıfı(string sku, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand(string.Format("select products.id, product_images.id from products left join product_images where products.sku='{0}'", sku), con);
            MySqlDataReader dr = cmd.ExecuteReader();
            product_id = -1;
            product_images_id = -1;
            if (dr.Read())
            {
                product_id = dr.GetInt32(0);
                product_images_id = dr.IsDBNull(1) ? -1 : dr.GetInt32(1);
            }
            dr.Close();
        }
        public resim_yayında_sınıfı(int product_id, int product_images_id)
        {
            this.product_id = product_id; this.product_images_id = product_images_id;
        }
        public resim_yayında_sınıfı() { product_id = product_images_id = -1; }

    }
    //public enum yayın_durumu { yayında = 1, yayında_değil = -1, bilinmiyor = 0 }

    class Resim_işlemleri
    {
        public static int max_byte = 1024 * 256; //256 KB 
        public string sku;
        public Uri url;
        public int resimler_id;

        resim_yayında_sınıfı yayın_durumu;
        resim_durumu vaziyet_nedir;
        //resim_nereden nereden;

        public static string ideasoft_resim_adresi(string directoryName, string filename, string extension)
        {
            return string.Concat("https://nettengelsin-1.myideasoft.com/myassets/products/", directoryName, "/", filename, ".", extension);
        }
        public static string getData_bodyFormat(string sku, int product_id, string resim_base64)
        {
            Regex illegalInFileName = new Regex(@"[\\/:*?\s""<>|]");
            return string.Format("{{\"filename\": \"{0}\",\"extension\": \"jpg\",\"sortOrder\": 1,\"product\": {{\"id\":{1}}},\"attachment\":\"data:image/jpeg;base64,{2}\"}}", illegalInFileName.Replace(sku, "_"), product_id, resim_base64);
        }
        public static string Get_Resim_Base64_String(string file_path) //path den resim verisi çekmeye çalışıyor.
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (file_path == "") return "";
            byte[] data;
            using (WebClient client = new WebClient())
            {
                try
                {
                    data = client.DownloadData(file_path);
                    string s = Convert.ToBase64String(data);
                    return s;
                }
                catch (Exception hata)
                {
                    throw hata;
                }
            }
        }
        public static Image Base64ToImage(string base64String) //base64 stringini Image e çevirmeye çalışıyor.
        {
            if (base64String == "") return null;
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            Image image;
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {

                try
                {
                    image = Image.FromStream(ms, true);
                }
                catch (Exception hata)
                {
                    ms.Close();
                    throw hata;
                }
                finally { ms.Close(); }
            }

            return image;
        }
        public static string Image_Url_Test(string path) //path i verilen resmi çeşitli kontrollere tabi tutuyor.
                                                         // 1. öncelikle path den veri çekilebiliyor mu? (Get_Resim_Base64_String)
                                                         // 2. resim boyutu uygun mu değilse küçültmeye çalışıyor. (resimBase64_KontrolEdilmişHali)
                                                         // 3. base64 string i Image e çevrilip çevrilmediğini kontrol ediyor. (Image_base64_Test)
        {
            try
            {
                string base64string = resimBase64_KontrolEdilmişHali(Get_Resim_Base64_String(path));
                Image_base64_Test(base64string);
                return base64string;
            }
            catch (Exception hata)
            {
                throw hata;
            }
        }
        public static void Image_base64_Test(string base64_string) //base64 string i Image e çevrilip çevrilmediğini kontrol ediyor.
        {
            Image r;
            try
            {
                r = Base64ToImage(base64_string);
            }
            catch (Exception hata)
            {
                throw hata;
            }
        }

        public static string resimBase64_KontrolEdilmişHali(string base64string)//resim boyutunu kontrol ediyor. Büyükse küçültüyor.
        {
            if (Math.Floor((float)base64string.Length * 3.0 / 4.0) <= max_byte) return base64string;
            Image i;
            try
            {
                i = Base64ToImage(base64string);
            }
            catch (Exception hata)
            {
                throw hata;
            }

            byte[] currentByteImageArray = Convert.FromBase64String(base64string);
            double scale = 1f;

            //MemoryStream inputMemoryStream = new MemoryStream(byteImageIn);
            //Image fullsizeImage = Image.FromStream(inputMemoryStream);

            while (currentByteImageArray.Length > max_byte)
            {
                scale -= 0.05f;
                using (Bitmap fullSizeBitmap = new Bitmap(i, new Size((int)(i.Width * scale), (int)(i.Height * scale))))
                {
                    using (MemoryStream resultStream = new MemoryStream())
                    {

                        fullSizeBitmap.Save(resultStream, i.RawFormat);

                        currentByteImageArray = resultStream.ToArray();
                        resultStream.Close();
                        resultStream.Dispose();
                        
                    }
                    //Thread.Sleep(1000);
                    fullSizeBitmap.Dispose();
                }

            }
            return Convert.ToBase64String(currentByteImageArray);
        }

        public Resim_işlemleri(string sku, string path) //ekleneceklercde kullanılacak
        {
            this.sku = sku;
            url = new Uri(path);
            this.yayın_durumu = new resim_yayında_sınıfı();
            vaziyet_nedir = resim_durumu.eklenecek;
        }
        public Resim_işlemleri(string sku, string path, MySqlConnection con, resim_yayında_sınıfı yayın_durumu = null)
        {
            this.sku = sku;
            url = new Uri(path);
            this.yayın_durumu = yayın_durumu ?? new resim_yayında_sınıfı();
            try
            {
                string s = Image_Url_Test(url.AbsoluteUri);
                vaziyet_nedir = resim_durumu.işlem_yok;

                MySqlCommand cmd = new MySqlCommand(string.Format("select id,url from resimler where sku='{0}'", sku), con);
                MySqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    resimler_id = dr.GetInt32(0);
                    string adres = dr.GetString(1);
                    if (adres != (url.IsFile ? url.AbsolutePath : url.AbsoluteUri))
                        if (Get_Resim_Base64_String(adres) != s)
                            vaziyet_nedir = resim_durumu.güncellenecek;
                }
                else
                    vaziyet_nedir = resim_durumu.eklenecek;
                dr.Close();
            }
            catch
            {
                vaziyet_nedir = resim_durumu.resim_yükleme_hatası;
            }
        }

        public System.Drawing.Image[] resimleri_getir(MySqlConnection con)
        {
            //ilk resim mevcut olan, ikincisi ise yüklenecek olan
            //güncellemelerde iki resmi de ekranda göstermek gerektiğinde lazım olacak.

            if (vaziyet_nedir != resim_durumu.güncellenecek) return null;
            string r1 = "", r2 = "";
            MySqlCommand cmd = new MySqlCommand(string.Format("select resim_base64 from resimler where id={0}", resimler_id), con);
            MySqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                r1 = dr.GetString(0);
            }
            dr.Close();
            try
            {
                r2 = veritabanı.Get_Resim_Base64_String(url.AbsoluteUri);
                if (r1 == "" || r2 == "") return null;
                return new System.Drawing.Image[] { veritabanı.Base64ToImage(r1), veritabanı.Base64ToImage(r2) };
            }
            catch (Exception hata)
            {
                throw hata;
            }
        }

        public void işlemYap(MySqlConnection con)
        {
            if (vaziyet_nedir == resim_durumu.işlem_yok || vaziyet_nedir == resim_durumu.resim_yükleme_hatası) return;

            string resim_base64;
            try
            {
                resim_base64 = resimBase64_KontrolEdilmişHali(Get_Resim_Base64_String(url.AbsoluteUri));
            }
            catch (Exception hata)
            {
                throw hata;
            }

            if (resim_base64 == "") return;
            //burada resim boyutu çok büyükse küçültülerek alınacak.
            //resimler tablosuna ekleme(eğer mevcutsa güncelleme) yapılıyor
            MySqlCommand cmd = new MySqlCommand(string.Format("insert into resimler(sku,url,resim_base64) values('{0}','{1}','{2}') ON DUPLICATE KEY UPDATE url=VALUES(url), resim_base64=VALUES(resim_base64)", sku, url.AbsoluteUri, resim_base64), con);
            veritabanı.cmdExecute(cmd);

            if (yayın_durumu.product_id > -1) //resim sitede yayında ise 
            {
                if (yayın_durumu.product_images_id > -1) //ürünün resim verisi varsa önce onu siliyor.
                {
                    if (Entegrasyon.resimSil(yayın_durumu.product_images_id)) //eğer resim siteden silinebildi ise kendi veritabanından da siliyor.
                    {
                        cmd.CommandText = string.Format("delete from product_images where id={0}", yayın_durumu.product_images_id);
                        veritabanı.cmdExecute(cmd);
                    }
                    else return;
                }

                //yeni resim gönderiliyor.
                string body = getData_bodyFormat(sku, yayın_durumu.product_id, resim_base64);
                int bekleme = Thread_Apiİşlemleri.bekleme;
                IRestResponse data;
                while (true)
                {
                    data = Entegrasyon.POST("product_images", body);
                    if (data.IsSuccessful || bekleme > 3000) break;
                    Thread.Sleep(bekleme);
                    bekleme *= 3;
                }
                if (data.IsSuccessful)
                {
                    dynamic d = JsonConvert.DeserializeObject(data.Content);
                    if (d["id"] != null) IdeaSoftVeritabanı.resimOluştur(d, con);
                }
            }
        }

    }

}
