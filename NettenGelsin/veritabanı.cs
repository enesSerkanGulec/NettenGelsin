using MySql.Data.MySqlClient;
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
    //class resimTip1
    //{
    //    public string sku;
    //    public string resim_path;
    //    public resimTip1(string sku_, string resim_path_) { sku = sku_; resim_path = resim_path_; }
    //    public virtual string toString(string resimBase64Data) { return "('" + sku + "','" + resim_path + "','" + resimBase64Data + "')"; }
    //}
    //class resimTip2 : resimTip1
    //{
    //    public string marka;
    //    public string sku_motorasin;
    //    public string resim;
    //    public resimTip2(string manufacturer_, string manufacturerCode_, string resim_path_, string resim_) : base(string.Concat(manufacturer_, " ", manufacturerCode_), resim_path_)
    //    {
    //        marka = manufacturer_;
    //        sku_motorasin = manufacturerCode_;
    //        resim_path = resim_path_;
    //        resim = resim_;
    //    }
    //    public override string toString(string resimBase64Data) { return "(" + id.ToString() + ",'" + marka + "','" + sku_motorasin + "','" + (resim == "" ? resimBase64Data : resim) + "')"; }
    //}
    //class resimTip3
    //{
    //    public string marka;
    //    public string sku_motorasin;
    //    public string resim_path;
    //    public resimTip3(string marka_, string sku_motorasin_, string resimPath_) { marka = marka_; sku_motorasin = sku_motorasin_; resim_path = resimPath_; }
    //    public string toString(string resimBase64) { return "('" + marka + "','" + sku_motorasin + "','" + resimBase64 + "')"; }
    //}
    //class resimTip4
    //{
    //    public int id;
    //    public string sku;
    //    public resimTip4(int id_, string sku_) { id = id_; sku = sku_; }
    //    public string toString() { return "(" + id.ToString() + ",'" + sku + "')"; }
    //}
    //class resimTip5
    //{
    //    public string marka;
    //    public string sku_dinamik;
    //    public string sku_motorasin { get { return sku_dinamik.Substring(marka.Length, sku_dinamik.Length - marka.Length).Trim(); } }
    //    public resimTip5(string marka_, string sku_dinamik_) { marka = marka_; sku_dinamik = sku_dinamik_; }
    //    public string toString() { return "('" + marka + "','" + sku_motorasin + "','" + sku_dinamik + "')"; }
    //}

    public enum durumTipi { Eklenecek = 1, Güncellenecek = 2, İşlemYok = 0, Hata = -1 }
    public enum işlemTipi { Dosyadan = 1, Firmalardan = 2, Ideasofttan = 3, Ürün_Ekleme = 4, ÜrünGüncelleme = 5 }

    public class resimTipi
    {
        public static MySqlConnection con;
        public static string marka;
        public static List<resimTipi> işlemGörenler = new List<resimTipi>();
        // k1s ve k2s dosya adında kullanılamayan fakat stok kodunda olan karakterler için gerekli
        public static List<string> k1s = new List<string>();
        public static List<string> k2s = new List<string>();

        public int resimId;
        public string mevcutResim_Base64 = "";
        //public FileInfo yüklenecekDosya;
        //public bool yüklemeHatası;
        public Exception hata;
        public string yüklenecek_Base64;
        //public string marka;
        public string sku;
        public string url = "";
        public int product_Id = -1;

        public durumTipi durum;
        public işlemTipi amaç;

        //public string getImage_base64_string()
        //{
        //    if (resimId)
        //}

        public string getData_bodyFormat
        {
            get
            {
                Regex illegalInFileName = new Regex(@"[\\/:*?\s""<>|]");
                return string.Format("{{\"filename\": \"{0}\",\"extension\": \"jpg\",\"sortOrder\": 1,\"product\": {{\"id\":{1}}},\"attachment\":\"data:image/jpeg;base64,{2}\"}}", illegalInFileName.Replace(sku, "_"), product_Id, yüklenecek_Base64);
            }
        }



        public resimTipi(string sku_, int product_id_, string resim_base64_) { sku = sku_; product_Id = product_id_; yüklenecek_Base64 = resim_base64_; }

        public resimTipi(string marka_, FileInfo dosya) //dosyadan yüklemelerde geçerli
        {
            //marka = marka_;
            //yüklenecekDosya = dosya;
            sku = string.Concat(marka_, " ", Path.GetFileNameWithoutExtension(dosya.Name));
            for (int i = 0; i < k1s.Count; i++) sku = sku.Replace(k2s[i][0], k1s[i][0]);
            try
            {
                yüklenecek_Base64 = veritabanı.Get_Resim_Base64_String(dosya.FullName);
            }
            catch (Exception hata_)
            {
                hata = hata_;
                //yüklemeHatası = true;
                durum = durumTipi.Hata;
                return;
            }

            MySqlDataReader dr;
            try
            {
                MySqlCommand cmd = new MySqlCommand(string.Format("select id, resim_base64 from resimler where sku='{0}'", sku), con);
                resimId = -1;
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    resimId = dr.GetInt32(0);
                    mevcutResim_Base64 = dr.GetString(1);
                }
                dr.Close();
            }
            catch (Exception hata_)
            {
                hata = hata_;
                durum = durumTipi.Hata;
                return;
            }
            if (resimId == -1) durum = durumTipi.Eklenecek;
            else if (mevcutResim_Base64 == "") durum = durumTipi.Eklenecek;
            else durum = mevcutResim_Base64 == yüklenecek_Base64 ? durumTipi.İşlemYok : durumTipi.Güncellenecek;

        }

        public resimTipi(string sku, string directoryName, string filename, string extension)
        //bu metod ile ideasofttaki resimler içe aktarılıyor. sku dinamik deki sku tarzında olacak (ileride sku ları birleştirme işleminden sonra kullanılabilir)
        {
            this.sku = sku;
            string adres = string.Concat("https://nettengelsin-1.myideasoft.com/myassets/products/", directoryName, "/", filename, ".", extension);
            MySqlDataReader dr;
            MySqlCommand cmd = new MySqlCommand(string.Format("select id, resim_base64 from resimler where sku='{0}'", sku), con);
            resimId = -1;
            try
            {
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    resimId = dr.GetInt32(0);
                    mevcutResim_Base64 = dr.GetString(1);
                }
                dr.Close();
            }
            catch (Exception hata_)
            {
                hata = hata_;
                durum = durumTipi.Hata;
                return;
            }

            if (resimId == -1) durum = durumTipi.Eklenecek;
            else if (mevcutResim_Base64 == "") durum = durumTipi.Eklenecek;
            else
            {
                try
                {
                    yüklenecek_Base64 = veritabanı.Get_Resim_Base64_String(adres);
                }
                catch (Exception hata_)
                {
                    durum = durumTipi.Hata;
                    hata = hata_;
                    return;
                }
                durum = yüklenecek_Base64 == mevcutResim_Base64 ? durumTipi.İşlemYok : durumTipi.Güncellenecek;
            }
        }

        public resimTipi(string sku, string url_)
        //firma verilerinden içe aktarmalarda geçerli
        //dinamik verilerinde sku aynen gelecek.
        //motorasinde sku=string.concat(Manufacturer," ",ManufacturerCode) şeklinde olacak
        {
            this.sku = sku;
            MySqlDataReader dr;
            MySqlCommand cmd = new MySqlCommand(string.Format("select id, url, resim_base64 from resimler where sku='{0}'", sku), con);
            resimId = -1;
            try
            {
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    resimId = dr.GetInt32(0);
                    url = dr.GetString(1);
                    mevcutResim_Base64 = dr.GetString(2);
                }
                dr.Close();
            }
            catch (Exception hata_)
            {
                hata = hata_;
                durum = durumTipi.Hata;
                return;
            }
            url = url_;
            if (resimId == -1) durum = durumTipi.Eklenecek;
            else if (mevcutResim_Base64 == "") durum = durumTipi.Eklenecek;
            else if (url == url_) durum = durumTipi.İşlemYok;
            else durum = durumTipi.Güncellenecek;

            if (durum == durumTipi.Eklenecek || durum == durumTipi.Güncellenecek)
            {
                try
                {
                    yüklenecek_Base64 = veritabanı.Get_Resim_Base64_String(url_);
                    //if (veritabanı.Base64ToImage(yüklenecek_Base64)==null)
                    //{
                    //    durum = durumTipi.Hata;
                    //    hata = new Exception("Resim_base64 stringi Image türüne çevrilemiyor.");
                    //    return;
                    //}

                }
                catch (Exception hata_)
                {
                    durum = durumTipi.Hata;
                    hata = hata_;
                    return;
                }
            }
        }

        public bool işlemYap()
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;

            if (durum == durumTipi.Eklenecek)
            {
                cmd.CommandText = string.Format("insert into resimler(sku,url,resim_base64) values('{0}','{1}','{2}')", sku, url, yüklenecek_Base64);
                if (veritabanı.cmdExecute(cmd)) { işlemGörenler.Add(this); return true; }
            }
            else if (durum == durumTipi.Güncellenecek)
            {
                cmd.CommandText = string.Format("update resimler set url='{0}', resim_base64='{1}' where id={2}", url, yüklenecek_Base64, resimId);
                if (veritabanı.cmdExecute(cmd)) { işlemGörenler.Add(this); return true; }
            }
            return false;
        }

        public static object[] yayında_olanları_getir(List<resimTipi> resimler)
        {
            //dönen değer iki boyutlu bir object array. ilki List<int> (silinecekler), ikincisi List<resimTipi_> yayında olan ürünlerin resimleri
            List<int> silinecekler = new List<int>();
            MySqlCommand cmd;
            MySqlDataReader dr;
            for (int i = 0; i < resimler.Count; i++)
            {
                if (resimler[i].durum == durumTipi.Hata || resimler[i].durum == durumTipi.İşlemYok)
                {
                    resimler.RemoveAt(i);
                    i--;
                }
                else
                {
                    cmd = new MySqlCommand(string.Format("select products.id,product_images.id from products left join product_images on products.id=product_images.product_id where products.sku='{0}'", resimler[i].sku), con);
                    dr = cmd.ExecuteReader();
                    try
                    {
                        if (dr.Read())
                        {
                            resimler[i].product_Id = dr.GetInt32(0);
                            if (!dr.IsDBNull(1))
                                silinecekler.Add(dr.GetInt32(1));
                        }
                        else { resimler.RemoveAt(i); i--; }
                    }
                    catch { resimler.RemoveAt(i); i--; }
                    finally { dr.Close(); }
                }
            }
            return new object[] { silinecekler, resimler };
        }

    }

    public static class veritabanı
    {
        public static int gönderilenPaketBoyutu = 32; //MB cinsinden

        // bu tablo isimleri değişmemeli. eğer değişirse sql fonskyon ve prosedure leri içinde de değiştirilmeleri gerekli. Bu bahsi geçen fonskyon ve prosedureler için proceduresAndFunctions değişkeni kontrol edilmeli
        public static string MotoraşinTablosu = "motorasin";
        public static string DinamikTablosu = "dinamik";
        public static string OrtakTablosu = "ortak";
        public static string SabitLabellerTablosu = "sabitLabeller";
        public static string DinamikMarkalarTablosu = "dinamikMarkalar";
        public static string DeğişimTablosu = "degisim";

        public static string Server = "127.0.0.1";
        public static string Port = "3308";
        public static string Uid = "nettengelsin";
        public static string Pwd = "netten2020gelsin";
        public static string Database = "nettenGelsin";

        public static int FiyatDeğişimYüzdesi = 5;

        public static string logsTabloYapısı = "zaman DATETIME NULL DEFAULT '0000-00-00 00:00:00', mesaj TEXT NULL DEFAULT NULL, tip TINYTEXT NULL DEFAULT NULL";
        public static string logsTablosuIndexYapısı = "	PRIMARY KEY (`id`) USING BTREE, INDEX `zaman` (`zaman`) USING BTREE";

        public static string MotoraşinTabloYapısı = "ManufacturerCode VARCHAR(50), Name TINYTEXT, Manufacturer VARCHAR(50), Quantity VARCHAR(10), Price FLOAT, PriceCurrency VARCHAR(50), VehicleType VARCHAR(50), VehicleBrand VARCHAR(50), OrginalNo VARCHAR(50), Picture TEXT, MinOrder FLOAT, Pictures TEXT, IsNetPrice TEXT, CampaignPrice FLOAT, CampaignCurrency VARCHAR(50)";
        public static string MotoraşinTablosuIndexYapısı = "PRIMARY KEY (`id`) USING BTREE, INDEX `ManufacturerCode` (`ManufacturerCode`) USING BTREE, INDEX `Manufacturer` (`Manufacturer`) USING BTREE, INDEX `Name` (`Name`(63)) USING BTREE, INDEX `VehicleType` (`VehicleType`) USING BTREE, INDEX `VehicleBrand` (`VehicleBrand`) USING BTREE, INDEX `Price` (`Price`) USING BTREE, INDEX `MinOrder` (`MinOrder`) USING BTREE, INDEX `Picture` (`Picture`(100)) USING BTREE, INDEX `IsNetPrice` (`IsNetPrice`(50)) USING BTREE, INDEX `CampaignCurrency` (`CampaignCurrency`) USING BTREE, INDEX `CampaignPrice` (`CampaignPrice`) USING BTREE";

        public static string DinamikTabloYapısı = "stok_kodu VARCHAR(75), stok_adi TEXT, marka VARCHAR(50), uretici_kodu VARCHAR(50), onceki_kod VARCHAR(50), kull1s VARCHAR(50), kull2s VARCHAR(50), kull3s VARCHAR(50), kull4s VARCHAR(50), kull5s VARCHAR(50), kull6s VARCHAR(50), kull7s VARCHAR(50), kull8s VARCHAR(150), resim_url TEXT, oem_liste TINYTEXT, esdegerListe TINYTEXT, fiyat FLOAT, varyok1 VARCHAR(5), varyok2 VARCHAR(5),varyok3 VARCHAR(5), barkod1 TINYTEXT, barkod2 TINYTEXT, barkod3 TINYTEXT, kampanyaOrani INT(2), paketMiktari INT(3), koliMiktari INT(3), olcuBirimi VARCHAR(10)";
        public static string DinamikTablosuIndexYapısı = "PRIMARY KEY (`id`) USING BTREE,INDEX `stok_kodu` (`stok_kodu`) USING BTREE,INDEX `kull1s` (`kull1s`) USING BTREE,INDEX `kull7s` (`kull7s`) USING BTREE,INDEX `kull8s` (`kull8s`) USING BTREE,INDEX `marka` (`marka`) USING BTREE,INDEX `stok_adi` (`stok_adi`(63)) USING BTREE,INDEX `fiyat` (`fiyat`) USING BTREE,INDEX `varyok1` (`varyok1`) USING BTREE, INDEX `varyok2` (`varyok2`) USING BTREE, INDEX `varyok3` (`varyok3`) USING BTREE, INDEX `kampanyaOrani` (`kampanyaOrani`) USING BTREE,INDEX `paketMiktari` (`paketMiktari`) USING BTREE,INDEX `resim_url` (`resim_url`(50)) USING BTREE";

        //public static string SabitLabelTabloYapısı = "stok_kodu VARCHAR(75), label TEXT";
        //public static string SabitLabelTablosuIndexYapısı = "PRIMARY KEY (`id`) USING BTREE, INDEX `stok_kodu` (`stok_kodu`) USING BTREE";

        public static string DinamikMarkalarTabloYapısı = "marka VARCHAR(50), cekilecek BOOL DEFAULT TRUE";
        public static string DinamikMarkalarTablosuIndexYapısı = "PRIMARY KEY (`id`) USING BTREE, INDEX `marka` (`marka`) USING BTREE";

        public static string DeğişimTabloYapısı = "tabloAdi VARCHAR(30), alanAdi VARCHAR(30), eskiDeger TEXT, yeniDeger TEXT, sart VARCHAR(1) DEFAULT '='";
        public static string DeğişimTablosuIndexYapısı = "PRIMARY KEY (`id`) USING BTREE, INDEX `eskiDeger` (`eskiDeger`(85)) USING BTREE, INDEX `tabloAdi` (`tabloAdi`) USING BTREE, INDEX `alanAdi` (`alanAdi`) USING BTREE, INDEX `sart` (`sart`) USING BTREE";

        public static string OrtakTabloYapısı = "stok_kodu VARCHAR(75), label TEXT, metaKeywords TEXT, brand VARCHAR(50), category_path TEXT, barcode TEXT, price FLOAT, currency_abbr VARCHAR(10), paket_miktari INT(3), stok_amount INT(4), stock_type VARCHAR(15), discount INT(4), discountType INT(1), nereden VARCHAR(30), detail TEXT, discountedSortOrder TINYINT(4) DEFAULT NULL";
        public static string OrtakTablosuIndexYapısı = "PRIMARY KEY (`id`) USING BTREE, INDEX `stok_kodu` (`stok_kodu`) USING BTREE, INDEX `paket_miktari` (`paket_miktari`) USING BTREE, INDEX `category_path` (`category_path`(85)) USING BTREE, INDEX `discountedSortOrder` (`discountedSortOrder`) USING BTREE";

        public static string OrtakTablosuINSERT_İfadesi = "INSERT INTO " + OrtakTablosu + "(stok_kodu, label, metaKeywords, brand, category_path, barcode, price, currency_abbr, paket_miktari, stok_amount, stock_type, discount, discountType, nereden, detail) ";

        //public static string IdeasoftTablosuOluşturma = "create table ideasoft (id INT(11) PRIMARY KEY, " + OrtakTabloYapısı + ") engine MyISAM select products.id, products.sku, products.name, getBrandName(products.brand_id), getPath(products.id), products.barcode, products.price1, getCurrencyAbbr(products.currency_id), getPaketMiktari(products.id), products.stockAmount, products.stockTypeLabel, getImageFileName(products.id) FROM products;";

        public static string ideasoftTablosuYapısı = "id INT(11), stok_kodu VARCHAR(75), label TEXT, brand VARCHAR(50), category_path TEXT, barcode TINYTEXT, price FLOAT, currency_abbr VARCHAR(10), paket_miktari INT(3), stok_amount INT(4), stock_type VARCHAR(15), discount INT(4), discountType INT(1), durum VARCHAR(1), discountedSortOrder TINYINT(4) DEFAULT NULL";
        public static string ideasoftTablosuIndexYapısı = "PRIMARY KEY (`id`) USING BTREE, INDEX `stok_kodu` (`stok_kodu`(75)) USING BTREE";

        public static string ideaSoftTablosuEklemeSQL = "INSERT INTO ideasoft (id, stok_kodu, label, brand, category_path, barcode, price, currency_abbr, paket_miktari, stok_amount, stock_type, discount, discountType, discountedSortOrder) SELECT products.id, products.sku, products.name, getBrandName(products.brand_id), getPath(products.id), products.barcode, products.price1, getCurrencyAbbr(products.currency_id), getPaketMiktari(products.id), products.stockAmount, products.stockTypeLabel, products.discount, products.discountType, products.discountedSortOrder FROM products  where distributor in ('dinamik','motorasin');";

        //public static string mulahazaTabloYapısı = "sku VARCHAR(50), durum VARCHAR(8), firma VARCHAR(30)";
        //public static string mulahazaTablosuIndexYapısı = "PRIMARY KEY (`id`) USING BTREE, INDEX `sku` (`sku`) USING BTREE, INDEX `durum` (`durum`) USING BTREE, INDEX `firma` (`firma`) USING BTREE";

        //public static string stokSifirSayaciTabloYapısı = "sku VARCHAR(50), kacGun INT(8)";
        //public static string stokSifirSayaciTablosuIndexYapısı = "PRIMARY KEY (`id`) USING BTREE, INDEX `sku` (`sku`) USING BTREE";

        public static string slugTabloYapısı = "sku TINYTEXT, slug TEXT";
        public static string slugTablosuIndexYapısı = "INDEX `sku` (`sku`(63)) USING BTREE";

        #region proceduresAndFunctions
        public static string proceduresAndFunctions = @"";

        #endregion

        public static string connectionString { get { return string.Format("Server={0};Port={1};Uid={2};Pwd={3};Database={4};", Server, Port, Uid, Pwd, Database); } }

        //public static void sabitLabelleri_DosyadanYükle(string dosya, Label labelBilgi)
        //{
        //    MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
        //    con.Open();
        //    if (labelBilgi == null) labelBilgi = new Label();
        //    tabloOluştur_idAuto(SabitLabellerTablosu, SabitLabelTabloYapısı, SabitLabelTablosuIndexYapısı, con);
        //    FileStream f = new FileStream(dosya, FileMode.Open);
        //    StreamReader r = new StreamReader(f, Encoding.UTF8);
        //    string satır = "";
        //    string[] s;
        //    MySqlCommand cmd = new MySqlCommand();
        //    cmd.Connection = con;
        //    cmd.Parameters.Add("@sk", MySqlDbType.VarChar);
        //    cmd.Parameters.Add("@l", MySqlDbType.VarChar);
        //    cmd.CommandText = "insert into " + SabitLabellerTablosu + "(stok_kodu,label) values(@sk,@l)";
        //    int i = 0;
        //    while ((satır = r.ReadLine()) != null)
        //    {
        //        s = satır.Trim().Split('½');
        //        if (s.Length != 2) continue;
        //        if (s[0] == null || s[1] == null) continue;
        //        s[0] = s[0].Trim();
        //        s[1] = s[1].Trim();
        //        if (s[0] == "" || s[1] == "") continue;
        //        cmd.Parameters["@sk"].Value = s[0];
        //        cmd.Parameters["@l"].Value = s[1];
        //        cmd.ExecuteNonQuery();
        //        labelBilgi.Text = "İçe aktarılan kayıt sayısı: " + (++i).ToString();
        //        labelBilgi.Refresh();
        //    }
        //    r.Close();
        //    f.Close();
        //    con.Close();
        //}

        public static bool cmdExecute(MySqlCommand cmd)
        {
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception hata)
            {
                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                {
                    Task t = cmd.Connection.OpenAsync();
                    t.Wait();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception hata1)
                    {
                        //email.send("Veri çekerken hata oluştu.", "Hata mesajı:\n" + hata1.Message + "\n\ncmd.CommandText:\n" + cmd.CommandText);
                        //MessageBox.Show("Hata oluştu!\nHata mesajı:\n" + hata1.Message + "\n\ncmd.commandText:\n" + cmd.CommandText);
                        log.Yaz("Komut çalıştırılınca hata oluştu." + "\nHata mesajı:\n" + hata1.Message + "\n\ncmd.CommandText:\n" + cmd.CommandText, "cmdExecute");
                        return false;
                    }

                }
                else
                {
                    email.send("Veri çekerken hata oluştu.", "Hata mesajı:\n" + hata.Message + "\n\ncmd.CommandText:\n" + cmd.CommandText);
                    log.Yaz("Komut çalıştırılınca hata oluştu." + "\nHata mesajı:\n" + hata.Message + "\n\ncmd.CommandText:\n" + cmd.CommandText, "cmdExecute");
                    //MessageBox.Show("Hata oluştu!\nHata mesajı:\n" + hata.Message + "\n\ncmd.commandText:\n" + cmd.CommandText);
                    return false;
                }
            }
        }

        public static bool veritabanıBağlantıBilgileriniDosyadanAl(string dosyaAdresi = ".\\Database\\databaseinfo.txt")
        {
            bool dönecek = false;
            FileInfo f = new FileInfo(dosyaAdresi);
            if (f.Exists)
            {
                string a, b, c, d, e;
                FileStream file = new FileStream(f.FullName, FileMode.Open);
                StreamReader reader = new StreamReader(file, Encoding.UTF8);
                try
                {
                    a = reader.ReadLine().Split(':')[1].Trim();
                    b = reader.ReadLine().Split(':')[1].Trim();
                    c = reader.ReadLine().Split(':')[1].Trim();
                    d = reader.ReadLine().Split(':')[1].Trim();
                    e = reader.ReadLine().Split(':')[1].Trim();
                    dönecek = true;
                }
                finally
                {
                    reader.Close();
                    file.Close();
                }
                if (dönecek)
                {
                    Server = a;
                    Port = b;
                    Uid = c;
                    Pwd = d;
                    Database = e;
                }
            }
            return dönecek;
        }
        public static bool veritabanıBağlantıBilgileriniDosyayaYaz(string dosyaAdresi = ".\\Database\\databaseinfo.txt")
        {
            bool dönecek = false;
            FileInfo f = new FileInfo(dosyaAdresi);
            DirectoryInfo dizin = new DirectoryInfo(f.DirectoryName);
            if (!dizin.Exists) dizin.Create();

            FileStream file = new FileStream(f.FullName, FileMode.Create);
            StreamWriter writer = new StreamWriter(file, Encoding.UTF8);
            try
            {
                writer.WriteLine("Server:" + Server);
                writer.WriteLine("Port:" + Port);
                writer.WriteLine("Uid:" + Uid);
                writer.WriteLine("Pwd:" + Pwd);
                writer.WriteLine("Database:" + Database);
                dönecek = true;
            }
            finally
            {
                writer.Close();
                file.Close();

            }
            return dönecek;
        }

        public static short veritabanıDurumu() //0 ise veritabanı yok, 1 ise veritabanına bağlanabiliyor, -1 ise bağlantı hatası
        {
            MySqlConnection con = new MySqlConnection("server=" + Server + ";Port=" + Port + ";Uid=" + Uid + ";Password=" + Pwd);

            try
            {
                con.Open();
            }
            catch (Exception hata)
            {
                return -1;
            }
            MySqlCommand cmd = new MySqlCommand("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME= '" + Database + "'", con);
            short s = 0;
            MySqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read()) s = 1;
            else s = 0;
            dr.Close();
            con.Close();
            return s;
        }
        public static bool veritabanıOluştur()
        {
            try
            {
                MySqlConnection con = new MySqlConnection("server=" + Server + ";Port=" + Port + ";Uid=" + Uid + ";Password=" + Pwd);
                MySqlCommand cmd = new MySqlCommand("CREATE DATABASE IF NOT EXISTS " + Database, con);
                cmdExecute(cmd);
                con.Close();
                return true;
            }
            catch (Exception hata)
            {
                return false;
            }
        }
        public static bool tabloVarmı(string tabloAdı, MySqlConnection con)
        {
            bool s = true;
            try
            {
                MySqlCommand cmd = new MySqlCommand("SHOW COLUMNS FROM " + tabloAdı, con);
                MySqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read()) s = true;
                else s = false;
                dr.Close();
            }
            catch (Exception hata)
            {
                s = false;
            }
            return s;
        }
        public static bool tabloOluştur_idAuto(string tabloAdı, string alanTanımlamaları, string indexTanımlamaları, MySqlConnection con)
        {
            indexTanımlamaları = indexTanımlamaları == "" ? "" : (", " + indexTanımlamaları);
            string s = string.Format(@"
DROP TABLE if EXISTS {0}; 
create table {0} (id INT(11) UNSIGNED AUTO_INCREMENT, {1}{2}) ENGINE=MyISAM;", tabloAdı, alanTanımlamaları, indexTanımlamaları);
            MySqlCommand cmd = new MySqlCommand(s, con);
            try
            {
                cmd.CommandTimeout = 600;
                cmdExecute(cmd);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool tabloOluştur_withoutId(string tabloAdı, string alanTanımlamaları, string indexTanımlamaları, MySqlConnection con)
        {
            indexTanımlamaları = indexTanımlamaları == "" ? "" : (", " + indexTanımlamaları);
            string s = string.Format(@"
DROP TABLE if EXISTS {0}; 
create table {0} ({1}{2}) ENGINE=MyISAM;", tabloAdı, alanTanımlamaları, indexTanımlamaları);
            MySqlCommand cmd = new MySqlCommand(s, con);
            try
            {
                cmd.CommandTimeout = 600;
                cmdExecute(cmd);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //public static bool tabloyuBoşalt(string tabloAdı, MySqlConnection con)
        //{
        //    MySqlCommand cmd = new MySqlCommand("Truncate table " + tabloAdı, con);
        //    try
        //    {
        //        cmd.ExecuteNonQuery();
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}
        //public static bool ortakTablosundanSil(string hangisini, MySqlConnection con)
        //{
        //    // hangisini şimdilik ya motorasin veya dinamik olabilir. istenirse başkaları da olbilir.
        //    MySqlCommand cmd = new MySqlCommand("delete from " + OrtakTablosu + " where nereden='" + hangisini + "'", con);
        //    try
        //    {
        //        cmd.CommandTimeout = 600;
        //        cmd.ExecuteNonQuery();
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}
        //public static bool kayıtEkle(string eklemeSorgusu, MySqlConnection con)
        //{
        //    bool x = true;
        //    MySqlCommand cmd = new MySqlCommand(eklemeSorgusu, con);
        //    try { cmd.ExecuteNonQuery(); } catch { x = false; }
        //    return x;
        //}
        public static void tabloKontrol(string tabloAdı, string tabloYapısı, string indexYapısı, MySqlConnection con)
        {
            if (!tabloVarmı(tabloAdı, con))
            {
                MessageBox.Show(tabloAdı + " tablosu olmadığı için yeni oluşturuluyor.", "Dikkat !!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (tabloOluştur_idAuto(tabloAdı, tabloYapısı, indexYapısı, con))
                    MessageBox.Show(tabloAdı + " tablosu oluşturuldu.");
                else
                {
                    MessageBox.Show(tabloAdı + " tablosu oluşturulamadı. Bu tablo olmadan program başlayamaz..", "Hata !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    program.Kapat();
                }
            }
        }

        //public static void fonksyonlarıOluştur(MySqlConnection con)
        //{
        //    MySqlCommand cmd = new MySqlCommand();
        //    cmd.Connection = con;
        //    cmd.CommandTimeout = 600;
        //    cmd.CommandText = veritabanı.proceduresAndFunctions;
        //    cmd.ExecuteNonQuery();
        //}

        public static string işlemGöreceklerListesi(int maksimum_kayıt_sayısı, string usdKur, string eurKur)
        {
            int adet = 0;
            MySqlConnection con = new MySqlConnection(connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM ortak WHERE brand IN (select marka from sabit_markalar where durum=1)", con);
            MySqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read()) adet = dr.GetInt32(0);
            dr.Close();
            //aşağıdaki select cümlesinin aslı:
            //cmd.CommandText = string.Format("(SELECT stok_kodu FROM ortak WHERE brand IN (select marka from sabit_markalar where durum=1) AND stok_amount>0) UNION (SELECT stok_kodu FROM ortak WHERE brand NOT IN (select marka from sabit_markalar where ABS(durum)=1) AND stok_amount>0 ORDER BY TL_Fiyat_Ver(currency_abbr,price,{0},{1})*1.18 DESC LIMIT {2})", usdKur, eurKur, maksimum_kayıt_sayısı - adet);

            cmd.CommandText = string.Format("(SELECT stok_kodu FROM ortak WHERE brand IN (select marka from sabit_markalar where durum=1) ORDER BY TL_Fiyat_Ver(currency_abbr,price,{0},{1})*1.18*paket_miktari DESC LIMIT {3}) UNION (SELECT stok_kodu FROM ortak WHERE brand NOT IN (select marka from sabit_markalar where ABS(durum)=1) ORDER BY TL_Fiyat_Ver(currency_abbr,price,{0},{1})*1.18*paket_miktari DESC LIMIT {2})", usdKur, eurKur, (maksimum_kayıt_sayısı - adet) < 0 ? 0 : maksimum_kayıt_sayısı - adet, maksimum_kayıt_sayısı);

            //yukarıdaki kodda currency_abbr,price,{1},{2})*1.18*paket_miktari şeklindeydi

            //declare adet int default 0;
            //SELECT COUNT(*) INTO adet FROM ortak WHERE ortak.brand IN { 0}
            //AND stok_amount> 0;
            //cmd = new MySqlCommand(string.Format(@"
            //declare adet int;
            //set adet=130000;
            //PREPARE sorgu FROM '()';
            //EXECUTE sorgu USING adet;
            //DEALLOCATE PREPARE sorgu;", tamamenAlınacaklarListesi, maksimum_kayıt_sayısı, usdKur, eurKur), con);

            dr = cmd.ExecuteReader();
            List<string> liste = new List<string>();
            while (dr.Read()) liste.Add("'" + dr.GetString(0) + "'");
            dr.Close();
            con.Close();
            if (liste.Count == 1) return liste[0];
            else return "(" + string.Join(",", liste.AsParallel().ToArray()) + ")";
        }

        public static void çokluKayıtlarıEngelle(string firma)
        {
            MySqlConnection con = new MySqlConnection(connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(string.Format("call cokluKayitlariTekeDusur('{0}')", firma), con);
            cmd.CommandTimeout = 600;
            veritabanı.cmdExecute(cmd);
        }

        public static void icindeGecenleriSil(string[] kelimeler)
        {
            string s = "";
            if (kelimeler.Length == 0) return;
            foreach (string kelime in kelimeler)
            {
                s += (s != "" ? " OR " : "") + string.Format("ortak.label LIKE '%{0}%'", kelime);
            }
            MySqlConnection con = new MySqlConnection(connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(string.Format("delete from ortak where {0}", s), con);
            cmd.CommandTimeout = 600;
            veritabanı.cmdExecute(cmd);
            con.Close();
        }
        public static void icindeGecenlerinFiyatlariniArttir(string[] kelimeler, double artis, bool artisMiktariYuzdeMi = false)
        {
            string s = "";
            if (kelimeler.Length == 0) return;
            foreach (string kelime in kelimeler)
            {
                s += (s != "" ? " OR " : "") + string.Format("ortak.label like '%{0}%'", kelime);
            }
            MySqlConnection con = new MySqlConnection(connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(string.Format("update ortak set ortak.price={0} where {1}",string.Format("artmisFiyat(ortak.currency_abbr, ortak.price, {0}, {1}, {2}, {3})", artis,artisMiktariYuzdeMi?1:0, Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.')), s), con);
            cmd.CommandTimeout = 600;
            veritabanı.cmdExecute(cmd);
            con.Close();
        }


        public static int kayıtSayısı(string tabloAdı)
        {
            MySqlConnection con = new MySqlConnection(connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select count(*) from " + tabloAdı, con);
            MySqlDataReader dr = cmd.ExecuteReader();
            int adet = 0;
            if (dr.Read()) adet = dr.GetInt32(0);
            dr.Close();
            con.Close();
            return adet;
        }

        public static Image Base64ToImage(string base64String)
        {
            if (base64String == "") return null;
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image;
            try
            {
                image = Image.FromStream(ms, true);
            }
            catch
            {
                image = null;
            }
            finally
            {
                ms.Close();
            }
            return image;
        }

        public static object[] Image_Url_Test(string path)
        {
            string base64string;
            try
            {
                base64string = Get_Resim_Base64_String(path);
                return new object[] { (Base64ToImage(base64string) == null ? true : false), base64string };
            }
            catch (Exception hata)
            {
                throw hata;
            }
        }

        public static string Get_Resim_Base64_String(string file_path)
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

        public static void resimKontrol(string firma)
        {
            return;
            //MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            //con.Open();
            //MySqlCommand cmd = new MySqlCommand("", con);
            //MySqlDataReader dr;
            //cmd.CommandTimeout = 600;
            //int limit = 32505856;
            //List<string> s = new List<string>();

            //if (firma == "motorasin")
            //{
            //    log.Yaz("Resimler (motorasin) kontrol ediliyor.", "Resim");
            //    //motorasin sku olan ve resim değeri olmayan ve gelen veride resim değeri gelmiş olanları update işlemi yapıyor.
            //    cmd.CommandText = "SELECT concat(motorasin.Manufacturer,' ',motorasin.ManufacturerCode) AS sku, motorasin.Picture FROM motorasin left JOIN resimler ON concat(motorasin.Manufacturer, ' ', motorasin.ManufacturerCode) = resimler.sku WHERE(resimler.sku IS NULL OR resimler.resim_base64 = '') AND motorasin.Picture <> ''";

            //    List<resimTip1> m = new List<resimTip1>();
            //    dr = cmd.ExecuteReader();
            //    while (dr.Read()) m.Add(new resimTip1(dr.GetString(0), dr.GetString(1)));
            //    dr.Close();

            //    string eklenecek = "";
            //    int uzunluk = 0;
            //    foreach (resimTip1 item in m)
            //    {
            //        eklenecek = item.toString(Get_Resim_Base64_String(item.resim_path));
            //        s.Add(eklenecek);
            //        uzunluk += eklenecek.Length + 4;
            //        if (uzunluk >= limit)
            //        {
            //            //önce resimler tablosunda sku alanını unique yap.
            //            // resimTip1 de url alanını da dahil et
            //            cmd.CommandText = string.Format("insert into resimler(sku,url,resim_base64) values {0} ON DUPLICATE KEY UPDATE resim_base64=VALUES(resim_base64)", string.Join(",", s));
            //            cmdExecute(cmd);
            //            uzunluk = 0;
            //            s.Clear();
            //        }

            //    }
            //    m.Clear();
            //    if (s.Count > 0)
            //    {
            //        cmd.CommandText = string.Format("insert into resimler(id,resim_base64) values {0} ON DUPLICATE KEY UPDATE resim_base64=VALUES(resim_base64)", string.Join(",", s));
            //        cmdExecute(cmd);
            //    }

            //    //resimlerin dinamik bölümünde kayıtlı olup fakat motorasin kısmı boş olanları bulup motorasin kısmını da dolduruyor.
            //    cmd.CommandText = "select resimler.id, motorasin.Manufacturer, motorasin.ManufacturerCode, motorasin.Picture, resimler.resim_base64 as resim from resimler inner join motorasin on resimler.sku_dinamik=CONCAT(motorasin.Manufacturer,' ',motorasin.ManufacturerCode) where resimler.sku_motorasin='' AND motorasin.ManufacturerCode<>''";
            //    List<resimTip2> n = new List<resimTip2>();
            //    dr = cmd.ExecuteReader();
            //    while (dr.Read()) n.Add(new resimTip2(dr.GetInt32(0), dr.GetString(1), dr.GetString(2), dr.GetString(3), dr.GetString(4)));
            //    dr.Close();
            //    uzunluk = 0;
            //    s = new List<string>();
            //    foreach (resimTip2 item in n)
            //    {
            //        eklenecek = item.toString(item.resim == "" ? Get_Resim_Base64_String(item.resim_path) : item.resim);
            //        s.Add(eklenecek);
            //        uzunluk += eklenecek.Length + 6;
            //        if (uzunluk >= limit)
            //        {
            //            cmd.CommandText = string.Format("insert into resimler(id,marka,sku_motorasin,resim_base64) values {0} ON DUPLICATE KEY UPDATE marka=VALUES(marka),sku_motorasin=VALUES(sku_motorasin),resim_base64=VALUES(resim_base64)", string.Join(",", s));
            //            cmdExecute(cmd);
            //            uzunluk = 0;
            //            s.Clear();
            //        }
            //    }
            //    n.Clear();
            //    if (s.Count > 0)
            //    {
            //        cmd.CommandText = string.Format("insert into resimler(id,marka,sku_motorasin,resim_base64) values {0} ON DUPLICATE KEY UPDATE marka=VALUES(marka),sku_motorasin=VALUES(sku_motorasin),resim_base64=VALUES(resim_base64)", string.Join(",", s));
            //        cmdExecute(cmd);
            //    }

            //    //motorasin tablosunda olup da resimler de olmayan kayıtlar ekleniyor.
            //    cmd.CommandText = "select motorasin.Manufacturer, motorasin.ManufacturerCode, motorasin.Picture from motorasin where concat(motorasin.Manufacturer,' ',motorasin.ManufacturerCode) not in (select concat(resimler.marka,' ',resimler.sku_motorasin) as sku from resimler) AND motorasin.ManufacturerCode<>''";
            //    List<resimTip3> k = new List<resimTip3>();
            //    dr = cmd.ExecuteReader();
            //    while (dr.Read()) k.Add(new resimTip3(dr.GetString(0), dr.GetString(1), dr.GetString(2)));
            //    dr.Close();
            //    uzunluk = 0;
            //    s = new List<string>();
            //    foreach (resimTip3 item in k)
            //    {
            //        eklenecek = item.toString(Get_Resim_Base64_String(item.resim_path));
            //        s.Add(eklenecek);
            //        uzunluk += eklenecek.Length + 5;
            //        if (uzunluk >= limit)
            //        {
            //            cmd.CommandText = string.Format("insert into resimler(marka,sku_motorasin,resim_base64) values {0}", string.Join(",", s));
            //            cmdExecute(cmd);
            //            uzunluk = 0;
            //            s.Clear();
            //        }
            //    }
            //    k.Clear();
            //    if (s.Count > 0)
            //    {
            //        cmd.CommandText = string.Format("insert into resimler(marka,sku_motorasin,resim_base64) values {0}", string.Join(",", s));
            //        cmdExecute(cmd);
            //    }
            //}
            //else if (firma == "dinamik")
            //{
            //    log.Yaz("Resimler (dinamik) kontrol ediliyor.", "Resim");
            //    //resimlerin dinamik bölümünde boş olup fakat motorasin kısmı ile eşleşen bulup dinamik kısmını da dolduruyor.
            //    cmd.CommandText = "select resimler.id, dinamik.stok_kodu from resimler inner join dinamik on dinamik.stok_kodu=CONCAT(resimler.marka,' ',resimler.sku_motorasin) where resimler.sku_dinamik='' AND dinamik.stok_kodu<>''";
            //    List<resimTip4> n = new List<resimTip4>();
            //    dr = cmd.ExecuteReader();
            //    while (dr.Read()) n.Add(new resimTip4(dr.GetInt32(0), dr.GetString(1)));
            //    dr.Close();
            //    foreach (resimTip4 item in n) s.Add(item.toString());
            //    n.Clear();
            //    if (s.Count > 0)
            //    {
            //        cmd.CommandText = string.Format("insert into resimler(id,sku_dinamik) values {0} ON DUPLICATE KEY UPDATE sku_dinamik=VALUES(sku_dinamik)", string.Join(",", s));
            //        cmdExecute(cmd);
            //    }

            //    //
            //    cmd.CommandText = "select dinamik.marka,dinamik.stok_kodu from dinamik where dinamik.stok_kodu not in (select resimler.sku_dinamik from resimler)";
            //    List<resimTip5> o = new List<resimTip5>();
            //    dr = cmd.ExecuteReader();
            //    while (dr.Read()) o.Add(new resimTip5(dr.GetString(0), dr.GetString(1)));
            //    dr.Close();
            //    s.Clear();
            //    foreach (resimTip5 item in o) s.Add(item.toString());
            //    n.Clear();
            //    if (s.Count > 0)
            //    {
            //        cmd.CommandText = string.Format("insert into resimler(marka,sku_motorasin,sku_dinamik) values {0}", string.Join(",", s));
            //        cmdExecute(cmd);
            //    }
            //}
            //con.Close();
        }

        public static void degisim(string firma)
        {
            MySqlConnection con = new MySqlConnection(connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(string.Format("select * from degisim where tabloAdi='{0}'", firma), con);
            List<string> sqls = new List<string>();
            MySqlDataReader dr = cmd.ExecuteReader();
            string ta = "";
            string aa = "";
            string ed = "";
            string yd = "";
            string sart = "";
            while (dr.Read())
            {
                ta = dr.GetString(1); aa = dr.GetString(2); ed = dr.GetString(3); yd = dr.GetString(4); sart = dr.GetString(5);
                if (sart == "r") sqls.Add(string.Concat("UPDATE ", ta, " set ", aa, "=REGEXP_REPLACE(", aa, ",'", ed, "','", yd, "')"));
                else if (sart == "=") sqls.Add(string.Concat("update ", ta, " set ", aa, "='", yd, "' where ", aa, "='", ed, "'"));
                else sqls.Add(string.Concat("UPDATE ", ta, " set ", aa, "=REGEXP_REPLACE(", aa, ",'", ed, "','", yd, "')")); //continue;
            }
            dr.Close();
            for (int i = 0; i < sqls.Count; i++)
            {
                cmd.CommandText = sqls[i];
                cmdExecute(cmd);
            }
        }

        public static void binekDüzeltme()
        {
            MySqlConnection con = new MySqlConnection(connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("CALL binekDuzeltme", con);
            cmd.CommandTimeout = 4800;
            cmdExecute(cmd);
            con.Close();
        }

        public static void Başla()
        {
            DirectoryInfo dizin = new DirectoryInfo(".\\Database");
            if (!dizin.Exists) dizin.Create();

            if (!veritabanıBağlantıBilgileriniDosyadanAl())
            {
                veritabanıBilgileri v = new veritabanıBilgileri();
                if (v.ShowDialog() == DialogResult.Cancel)
                {
                    MessageBox.Show("Veritabanı bağlantı bilgileri olmadan program başlayamaz..", "Hata !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    program.Kapat();
                }
                else
                {
                    Server = ((int)v.numericUpDown2.Value).ToString() + "." + ((int)v.numericUpDown3.Value).ToString() + "." + ((int)v.numericUpDown4.Value).ToString() + "." + ((int)v.numericUpDown5.Value).ToString();
                    Port = ((int)v.numericUpDown1.Value).ToString();
                    Uid = v.textBox1.Text;
                    Pwd = v.textBox2.Text;
                    Database = v.textBox3.Text;
                    if (!veritabanıBağlantıBilgileriniDosyayaYaz())
                    {
                        MessageBox.Show("Veritabanı bilgileri dosyaya yazılamadı.");
                    }
                }
            }

            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);

            try
            {
                con.Open();
                Thread.Sleep(10);
            }
            catch (Exception hata)
            {
                switch (veritabanıDurumu())
                {
                    case 1: MessageBox.Show(Database + " veritabanı oluşturuldu."); break;
                    case 0:
                        if (!veritabanıOluştur())
                        {
                            MessageBox.Show("Veritabanı bağlantısı sağlanamadı. Program başlatılamıyor.\nHata mesajı:\n" + hata.Message, "Hata !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            program.Kapat();
                        }
                        else
                        {
                            con = new MySqlConnection(veritabanı.connectionString);
                        }
                        break;
                    case -1:
                        MessageBox.Show("Veritabanı bağlantısı sağlanamadı. Program başlatılamıyor. Veritabanı bağlantı bilgileri hatalı.\nHata mesajı:\n" + hata.Message, "Hata !!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        program.Kapat();
                        break;
                }
            }
            tabloKontrol("dinamik_ham", DinamikTabloYapısı, DinamikTablosuIndexYapısı, con);
            tabloKontrol(OrtakTablosu, OrtakTabloYapısı, OrtakTablosuIndexYapısı, con);
            tabloKontrol(DeğişimTablosu, DeğişimTabloYapısı, DeğişimTablosuIndexYapısı, con);
            tabloKontrol("logs", logsTabloYapısı, logsTablosuIndexYapısı, con);
            if (!tabloVarmı("slug", con))
            {
                MessageBox.Show("slug tablosu önemli bir tablo. bu tablo olmadan program başlayamaz.", "Hata !", MessageBoxButtons.OK, MessageBoxIcon.Error);
                program.Kapat();

            }
            con.Close();
        }
    }
}
