using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;
using RestSharp;
using Newtonsoft.Json;
using System.Data;
using System.Windows.Forms;
using MySql.Data;
using System.Security.Cryptography;
using System;

namespace NettenGelsin
{
    public class Anahtar
    {
        public string alanAdı;
        public char type;
        public Anahtar child;
        public string parameter(dynamic item)
        {
            if (type == 's') return "'" + (item[alanAdı] == null ? "" : ((string)item[alanAdı].Value).Replace("'", "\\'")) + "'";
            else if (type == 'i') return item[alanAdı] == null ? "NULL" : ((long)item[alanAdı].Value).ToString();
            else if (type == 'f') return item[alanAdı] == null ? "NULL" : ((double)item[alanAdı].Value).ToString().Replace(',', '.');
            else if (type == 'b') return item[alanAdı] == null ? "NULL" : ((bool)item[alanAdı].Value ? "true" : "false");
            else
            {
                if (item[alanAdı] == null)
                    return "NULL";
                else
                    return child.parameter(item[alanAdı]);
            }
        }
        public Anahtar(string alanTanımlaması)
        {
            alanTanımlaması = alanTanımlaması.Trim();
            alanAdı = alanTanımlaması.Trim().Split(' ')[0];
            if (alanTanımlaması.IndexOf("INT") > -1) type = 'i';
            else if (alanTanımlaması.IndexOf("TEXT") > -1 || alanTanımlaması.IndexOf("VARCHAR") > -1) type = 's';
            else if (alanTanımlaması.IndexOf("FLOAT") > -1) type = 'f';
            else if (alanTanımlaması.IndexOf("BOOLEAN") > -1) type = 'b';

            if (alanAdı.IndexOf('_') > -1)
            {
                string[] s = alanAdı.Split('_');
                alanAdı = s[0];
                child = new Anahtar(s[1] + " " + alanTanımlaması.Trim().Split(' ')[1]);
                type = '\0';
            }
        }
    }

    public class ClassNesne
    {
        public static Label lb;
        string getİsteği;
        string stringBuilderİfadesi
        {
            get
            {
                string s = "";
                for (int i = 0; i < anahtarlar.Count; i++) s += (i == 0 ? "" : ",") + (anahtarlar[i].child == null ? anahtarlar[i].alanAdı : anahtarlar[i].alanAdı + "_" + anahtarlar[i].child.alanAdı);
                return "INSERT INTO " + getİsteği + "(" + s + ") VALUES ";
            }
        }
        string tabloYapısı;
        string indexTanımlaması;
        List<Anahtar> anahtarlar = new List<Anahtar>();
        public Thread T_veriÇekme; //ideasoft tan
        //public Thread T_veriYazma; //ideasoft a

        string formatStringVer(dynamic item)
        {
            string s = "";
            for (int i = 0; i < anahtarlar.Count; i++) s += (i == 0 ? "" : ",") + anahtarlar[i].parameter(item);
            return "(" + s + ")";
        }
        public ClassNesne(string getİsteği_, string tabloYapısı_, string indexTanımlaması_)
        {
            getİsteği = getİsteği_;
            tabloYapısı = tabloYapısı_;
            indexTanımlaması = indexTanımlaması_;
            string[] s = tabloYapısı.Split(',');
            for (int i = 0; i < s.Length; i++) anahtarlar.Add(new Anahtar(s[i]));
        }
        public void _verileriÇek()
        {
            lb = lb ?? new Label();
            lb.Tag = lb.Text;
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            DateTime t_Start = DateTime.Now;
            int page = 0;
            IRestResponse data;
            bool devam = true;
            StringBuilder sCommand = new StringBuilder(stringBuilderİfadesi);
            List<string> Rows = new List<string>();
            int gönderilenVeriUzunluğu = 0;
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            int bekleme;
            while (devam)
            {
                page++;
                int tekrar = 0;
                bekleme = Thread_Apiİşlemleri.bekleme;
                do
                {
                    lb.Text = (string)lb.Tag + " " + page.ToString() + ". veri paketi çekiliyor";
                    log.Yaz(getİsteği + " limit=100 page=" + page.ToString() + "     veri çekiliyor..(Bekleme: " + bekleme.ToString() + ")");
                    Thread.Sleep(bekleme);
                    data = Entegrasyon.GET(getİsteği, "limit=100&page=" + page.ToString());
                    if (devam = data.IsSuccessful) break;
                    else
                    {
                        bekleme *= Thread_Apiİşlemleri.tekrarÇarpan;
                        tekrar++;
                    }
                } while (tekrar < Thread_Apiİşlemleri.tekrar);
                if (devam)
                {
                    dynamic d = JsonConvert.DeserializeObject(data.Content);
                    if (devam = (d.Count > 0))
                    {
                        foreach (var item in d)
                        {
                            Rows.Add(formatStringVer(item));
                            gönderilenVeriUzunluğu += Rows[Rows.Count - 1].Length;
                        }
                        if (gönderilenVeriUzunluğu > ((veritabanı.gönderilenPaketBoyutu - 1) * 1024 * 1024))
                        {
                            sCommand.Append(string.Join(",", Rows));
                            sCommand.Append(";");
                            cmd.CommandText = sCommand.ToString();
                            cmd.CommandType = CommandType.Text;
                            cmd.ExecuteNonQuery();
                            Rows.Clear();
                            sCommand = new StringBuilder(stringBuilderİfadesi);
                        }
                    }
                    else page--;
                }
            }
            if (Rows.Count > 0)
            {
                sCommand.Append(string.Join(",", Rows));
                sCommand.Append(";");
                cmd.CommandText = sCommand.ToString();
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            con.Close();
            TimeSpan geçen = DateTime.Now.Subtract(t_Start);
            log.Yaz(getİsteği + " İşlem Bitti. Geçen süre: " + (geçen.Days == 0 ? "" : geçen.Days.ToString() + " gün ") + (geçen.Hours == 0 ? "" : geçen.Hours.ToString() + " saat ") + (geçen.Minutes == 0 ? "" : geçen.Minutes.ToString() + " dakika ") + (geçen.Seconds == 0 ? "" : geçen.Seconds.ToString() + " saniye") + " SON PAGE=" + page.ToString());
        }
        public bool verileriÇek() //ideaSoft tan
        {
            if (T_veriÇekme != null) if (T_veriÇekme.IsAlive) return false;
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            veritabanı.tabloOluştur_withoutId(getİsteği, tabloYapısı, indexTanımlaması, con);
            con.Close();
            //Thread_Apiİşlemleri.işEkle(T_veriÇekme = new Thread(() => _verileriÇek()));
            T_veriÇekme = new Thread(() => _verileriÇek());
            T_veriÇekme.Start();
            return true;
        }
    }

    public static class IdeaSoftVeritabanı
    {
        public static Thread Thread_ideaSoft_oluşturma;

        public static ClassNesne brand;
        public static ClassNesne currency;
        public static ClassNesne price;
        public static ClassNesne details;
        public static ClassNesne category;
        public static ClassNesne product_to_category;
        public static ClassNesne product_image;
        public static ClassNesne purchase_limitations;
        public static ClassNesne purchase_limitation_items;
        public static ClassNesne tags;
        public static ClassNesne product_to_tags;
        public static ClassNesne product;


        private static void MarkaTıklama(object sender, System.EventArgs e)
        {
            if (((ToolStripItem)sender).Image == null) ((ToolStripItem)sender).Image = NettenGelsin.Properties.Resources.Ok_icon;
            else ((ToolStripItem)sender).Image = null;
        }

        public static List<ToolStripItem> brandListesi(string[] eklenecekler)
        {
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select distinct(brand) from ideasoft", con);
            MySqlDataReader dr = cmd.ExecuteReader();
            List<string> s = new List<string>();
            while (dr.Read()) s.Add(dr.GetString(0));
            dr.Close();
            con.Close();
            foreach (string item in eklenecekler)
                if (s.IndexOf(item) == -1) s.Add(item);
            s.Sort();
            List<ToolStripItem> r = new List<ToolStripItem>();
            foreach (string item in s)
            {
                r.Add(new ToolStripMenuItem(item, Array.IndexOf(eklenecekler, item) == -1 ? null : NettenGelsin.Properties.Resources.Ok_icon, MarkaTıklama));
            }
            return r;

        }

        public static int katsayı = 5;

        public static string slugGetir(string sku, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand("select `slug` from slug where `sku`='" + sku + "'", con);
            string x = "";
            MySqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read()) x = dr.GetString(0);
            dr.Close();
            return x;
        }

        public static int sortOrderGetir(MySqlConnection con) //şimdilik parentId null olanlar içerisinden getiriyor.
        {
            MySqlCommand cmd = new MySqlCommand("select (select COUNT(*) from categories  where categories.parent_id IS NULL)+1", con);
            MySqlDataReader dr = cmd.ExecuteReader();
            int x = -1;
            if (dr.Read()) x = dr.GetInt32(0);
            dr.Close();
            return x;
        }

        public static int brandIdGetir(string brandName, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand(string.Format("select id from brands where name='{0}'", brandName), con);
            MySqlDataReader dr = cmd.ExecuteReader();
            int id = -1;
            if (dr.Read()) id = dr.GetInt32(0);
            dr.Close();
            return id;
        }
        public static int brandOluştur(dynamic data, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand("insert into brands values(@id,@name,@slug,@distributorCode,@distributor,@imageFile,@metaKeywords,@metaDescription,@pageTitle)", con);
            cmd.Parameters.AddWithValue("@id", (int)data["id"].Value);
            cmd.Parameters.AddWithValue("@name", (string)data["name"].Value);
            cmd.Parameters.AddWithValue("@slug", (string)data["slug"].Value);
            cmd.Parameters.AddWithValue("@distributorCode", (string)data["distributorCode"].Value);

            if (object.ReferenceEquals(null, data["distributor"]) || data["distributor"] == null) cmd.Parameters.AddWithValue("@distributor", DBNull.Value);
            else cmd.Parameters.AddWithValue("@distributor", (int)data["distributor"].Value);

            if (object.ReferenceEquals(null, data["imageFile"]) || data["imageFile"] == null) cmd.Parameters.AddWithValue("@imageFile", DBNull.Value);
            else cmd.Parameters.AddWithValue("@imageFile", (int)data["imageFile"].Value);

            if (object.ReferenceEquals(null, data["metaKeywords"]) || data["metaKeywords"] == null) cmd.Parameters.AddWithValue("@metaKeywords", DBNull.Value);
            else cmd.Parameters.AddWithValue("@metaKeywords", (int)data["metaKeywords"].Value);

            if (object.ReferenceEquals(null, data["metaDescription"]) || data["metaDescription"] == null) cmd.Parameters.AddWithValue("@metaDescription", DBNull.Value);
            else cmd.Parameters.AddWithValue("@metaDescription", (int)data["metaDescription"].Value);

            if (object.ReferenceEquals(null, data["pageTitle"]) || data["pageTitle"] == null) cmd.Parameters.AddWithValue("@pageTitle", DBNull.Value);
            else cmd.Parameters.AddWithValue("@pageTitle", (int)data["pageTitle"].Value);

            cmd.ExecuteNonQuery();
            return (int)data["id"].Value;
        }
        public static int kategoryIDGetir(string name, int parent_id, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            cmd.CommandText = "select id from categories where name=@name and parent_id" + (parent_id == -1 ? " is null" : "=@pID");
            cmd.Parameters.Add("@name", MySqlDbType.VarChar);
            cmd.Parameters.Add("@pID", MySqlDbType.Int64);
            MySqlDataReader dr;
            cmd.Parameters["@name"].Value = name;
            if (parent_id == -1) cmd.Parameters["@pID"].Value = DBNull.Value; else cmd.Parameters["@pID"].Value = parent_id;
            dr = cmd.ExecuteReader();
            parent_id = -1;
            if (dr.Read()) parent_id = dr.GetInt32(0);
            dr.Close();
            return parent_id;
        }

        public static int resimOluştur(dynamic data, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand("insert into product_images(id,filename,extension,directoryName,sortOrder,product_id) values(@id,@filename,@extension,@directoryName,@sortOrder,@product_id)", con);
            cmd.Parameters.AddWithValue("@id", (int)data["id"].Value);
            cmd.Parameters.AddWithValue("@filename", (string)data["filename"].Value);
            cmd.Parameters.AddWithValue("@extension", (string)data["extension"].Value);
            cmd.Parameters.AddWithValue("@sortOrder", (int)data["sortOrder"].Value);
            cmd.Parameters.AddWithValue("@directoryName", (string)data["directoryName"].Value);
            if (object.ReferenceEquals(null, data["product"]) || data["product"] == null) cmd.Parameters.AddWithValue("@product_id", DBNull.Value);
            else cmd.Parameters.AddWithValue("@product_id", (int)data["product"]["id"].Value);
            cmd.ExecuteNonQuery();
            return (int)data["id"].Value;
        }
        public static bool resimleriSil(List<int> product_images_id_ler, MySqlConnection con)
        {
            if (product_images_id_ler.Count == 0) return true;
            string idler = "";
            if (product_images_id_ler.Count > 0) idler = " in (" + string.Join(",", product_images_id_ler) + ")";
            else idler = "=" + idler[0];
            MySqlCommand cmd = new MySqlCommand(string.Format("delete from product_images where id{0}", idler), con);
            try { cmd.ExecuteNonQuery(); return true; }
            catch { return false; }
        }
        

        public static int kategoriOluştur(dynamic data, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand("insert into categories values(@id,@name,@slug,@sortOrder,@distributorCode,@distributor,@imageFile,@metaKeywords,@metaDescription,@pageTitle,@parent_id)", con);
            cmd.Parameters.AddWithValue("@id", (int)data["id"].Value);
            cmd.Parameters.AddWithValue("@name", (string)data["name"].Value);
            cmd.Parameters.AddWithValue("@slug", (string)data["slug"].Value);
            cmd.Parameters.AddWithValue("@sortOrder", (int)data["sortOrder"].Value);
            cmd.Parameters.AddWithValue("@distributorCode", (string)data["distributorCode"].Value);
            cmd.Parameters.AddWithValue("@distributor", (string)data["distributor"].Value);
            cmd.Parameters.AddWithValue("@imageFile", (string)data["imageFile"].Value);
            cmd.Parameters.AddWithValue("@metaKeywords", (string)data["metaKeywords"].Value);
            cmd.Parameters.AddWithValue("@metaDescription", (string)data["metaDescription"].Value);
            cmd.Parameters.AddWithValue("@pageTitle", (string)data["pageTitle"].Value);
            if (object.ReferenceEquals(null, data["parent"]) || data["parent"] == null) cmd.Parameters.AddWithValue("@parent_id", DBNull.Value);
            else cmd.Parameters.AddWithValue("@parent_id", (int)data["parent"]["id"].Value);
            cmd.ExecuteNonQuery();
            return (int)data["id"].Value;
        }
        public static int currencyIDGetir(string currencyAbbr, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand(string.Format("select id from currencies where abbr='{0}'", currencyAbbr), con);
            MySqlDataReader dr = cmd.ExecuteReader();
            int id = -1;
            if (dr.Read()) id = dr.GetInt32(0);
            dr.Close();
            return id;
        }
        public static int satınAlmaLimitiIDGetir(string limit, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand(string.Format("select id from purchase_limitations where minimumLimit={0}", limit), con);
            MySqlDataReader dr = cmd.ExecuteReader();
            int id = -1;
            if (dr.Read()) id = dr.GetInt32(0);
            dr.Close();
            return id;
        }
        public static int satınAlmaLimitiOluştur(dynamic data, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand("insert into purchase_limitations values(@id,@name,@minimumLimit,@maximumLimit,@type,@status)", con);
            cmd.Parameters.AddWithValue("@id", (int)data["id"].Value);
            cmd.Parameters.AddWithValue("@name", (string)data["name"].Value);
            cmd.Parameters.AddWithValue("@minimumLimit", (int)data["minimumLimit"].Value);
            cmd.Parameters.AddWithValue("@maximumLimit", (int)data["maximumLimit"].Value);
            cmd.Parameters.AddWithValue("@type", (string)data["type"].Value);
            cmd.Parameters.AddWithValue("@status", (bool)data["status"].Value);
            cmd.ExecuteNonQuery();
            return (int)data["id"].Value;
        }
        public static int ürünOluştur(dynamic data, bool yeni, MySqlConnection con)
        {
            string s = yeni ? "insert into products values(@id,@name,@slug,@fullName,@sku,@barcode,@price1,@distributor,@stockAmount,@buyingPrice,@stockTypeLabel,@discount,@discountType,@metaKeywords,@metaDescription,@pageTitle,@searchKeywords,@brand_id,@currency_id,@parent_id,@discountedSortOrder)" : "update products set name=@name,slug=@slug,fullName=@fullName,sku=@sku,barcode=@barcode,price1=@price1,distributor=@distributor,stockAmount=@stockAmount,buyingPrice=@buyingPrice,stockTypeLabel=@stockTypeLabel,discount=@discount,discountType=@discountType,metaKeywords=@metaKeywords,metaDescription=@metaDescription,pageTitle=@pageTitle,searchKeywords=@searchKeywords,brand_id=@brand_id,currency_id=@currency_id,parent_id=@parent_id,discountedSortOrder=@discountedSortOrder where id=@id";
            MySqlCommand cmd = new MySqlCommand(s, con);
            cmd.Parameters.AddWithValue("@id", (int)data["id"].Value);
            cmd.Parameters.AddWithValue("@name", (string)data["name"].Value);
            cmd.Parameters.AddWithValue("@slug", (string)data["slug"].Value);
            cmd.Parameters.AddWithValue("@fullName", (string)data["fullName"].Value);
            cmd.Parameters.AddWithValue("@sku", (string)data["sku"].Value);
            cmd.Parameters.AddWithValue("@barcode", (string)data["barcode"].Value);
            cmd.Parameters.AddWithValue("@price1", (float)data["price1"].Value);
            cmd.Parameters.AddWithValue("@distributor", (string)data["distributor"].Value);
            cmd.Parameters.AddWithValue("@stockAmount", (float)data["stockAmount"].Value);
            cmd.Parameters.AddWithValue("@buyingPrice", (float)data["buyingPrice"].Value);
            cmd.Parameters.AddWithValue("@stockTypeLabel", (string)data["stockTypeLabel"].Value);
            cmd.Parameters.AddWithValue("@discount", (int)data["discount"].Value);
            cmd.Parameters.AddWithValue("@discountType", (int)data["discountType"].Value);
            cmd.Parameters.AddWithValue("@metaKeywords", (string)data["metaKeywords"].Value);
            cmd.Parameters.AddWithValue("@metaDescription", (string)data["metaDescription"].Value);
            cmd.Parameters.AddWithValue("@pageTitle", (string)data["pageTitle"].Value);
            cmd.Parameters.AddWithValue("@searchKeywords", (string)data["searchKeywords"].Value);
            cmd.Parameters.AddWithValue("@brand_id", (int)data["brand"]["id"].Value);
            cmd.Parameters.AddWithValue("@currency_id", (int)data["currency"]["id"].Value);
            if (object.ReferenceEquals(null, data["parent"]) || data["parent"] == null) cmd.Parameters.AddWithValue("@parent_id", DBNull.Value);
            else cmd.Parameters.AddWithValue("@parent_id", (int)data["parent"]["id"].Value);
            if (data["discountedSortOrder"].Value==null) cmd.Parameters.AddWithValue("@discountedSortOrder", DBNull.Value);
            else cmd.Parameters.AddWithValue("@discountedSortOrder", (int)data["discountedSortOrder"].Value);
            
            cmd.ExecuteNonQuery();
            return (int)data["id"].Value;
        }

        public static void ürünleriSil(string product_ids)
        {
            if (product_ids == "") return;
            List<string> rowEkleme = new List<string>();
            //List<string> idler = new List<string>();
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT sku, slug FROM products WHERE id" + product_ids, con);
            MySqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                //idler.Add(dr.GetInt32(0).ToString());
                rowEkleme.Add("('" + dr.GetString(0) + "','" + dr.GetString(1) + "')");
            }
            dr.Close();
            cmd.CommandText = string.Format("DELETE FROM purchase_limitation_items WHERE product_id{0}", product_ids);
            cmd.ExecuteNonQuery();
            cmd.CommandText = string.Format("DELETE FROM product_to_categories WHERE product_id{0}", product_ids);
            cmd.ExecuteNonQuery();
            cmd.CommandText = string.Format("DELETE FROM product_details WHERE product_id{0}", product_ids);
            cmd.ExecuteNonQuery();

            //burada slug değerini güncelleme yapsa nasıl olur?
            //cmd.CommandText = "DELETE FROM slug WHERE sku" + (stokKodları.IndexOf(',') > -1 ? " in (" + stokKodları + ")" : " = " + stokKodları);
            //cmd.ExecuteNonQuery();
            //if (slug_değeri_korunsun_mu)
            //{
            cmd.CommandText = string.Format("INSERT INTO slug(`sku`,`slug`) VALUES {0} ON DUPLICATE KEY UPDATE `slug`=VALUES(`slug`);", string.Join(",", rowEkleme));
            cmd.ExecuteNonQuery();
            //}
            cmd.CommandText = "DELETE FROM products WHERE id" + product_ids;
            cmd.ExecuteNonQuery();
            //if (mulahazadandaSilinsin)
            //{
            //    cmd.CommandText = "DELETE FROM mulahaza where sku" + (stokKodları.IndexOf(',') > -1 ? " in (" + stokKodları + ")" : "=" + stokKodları);
            //    cmd.ExecuteNonQuery();
            //}
            con.Close();
        }

        //public static void ürünSil(string stokKodu,MySqlConnection con, bool slug_değeri_korunsun_mu,bool mulahazadan_da_Sil=false)
        //{
        //    MySqlCommand cmd = new MySqlCommand(string.Format("call urunSil('{0}',{1});", stokKodu, (slug_değeri_korunsun_mu ? 1 : 0)), con);
        //    cmd.ExecuteNonQuery();
        //}

        public static object[] ürünDetailsIDGetir(string ürünID, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand("select id,details from product_details where product_id=" + ürünID, con);
            MySqlDataReader dr = cmd.ExecuteReader();
            int id = -1;
            string details = "";
            if (dr.Read())
            {
                id = dr.GetInt32(0);
                details = dr.GetString(1);
            }
            dr.Close();
            return new object[] { id, details };
        }
        public static int ürünDetailsOluştur(dynamic data, bool yeni, MySqlConnection con)
        {
            string s = yeni ? "insert into product_details values(@id,@sku,@details,@product_id)" : "update product_details set sku=@sku, details=@details, product_id=@product_id where id=@id";
            MySqlCommand cmd = new MySqlCommand(s, con);
            cmd.Parameters.AddWithValue("@id", (int)data["id"].Value);
            cmd.Parameters.AddWithValue("@sku", (string)data["sku"].Value);
            cmd.Parameters.AddWithValue("@details", (string)data["details"].Value);
            cmd.Parameters.AddWithValue("@product_id", (int)data["product"]["id"].Value);
            cmd.ExecuteNonQuery();
            return (int)data["id"].Value;
        }


        public static object[] purchase_limitation_items_ID_ve_limitID_Getir(string ürünID, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand("select purchase_limitation_items.id, purchase_limitation_items.limitation_id from purchase_limitation_items where purchase_limitation_items.product_id=" + ürünID, con);
            MySqlDataReader dr = cmd.ExecuteReader();
            int id = -1;
            int limitID = -1;
            if (dr.Read())
            {
                id = dr.GetInt32(0);
                limitID = dr.GetInt32(1);
            }
            dr.Close();
            return new object[] { id, limitID };
        }
        public static int purchase_limitation_itemsOluştur(dynamic data, bool yeni, MySqlConnection con)
        {
            string s = yeni ? "insert into purchase_limitation_items values(@id,@limitation_id,@product_id)" : "update purchase_limitation_items set limitation_id=@limitation_id, product_id=@product_id where id=@id";
            MySqlCommand cmd = new MySqlCommand(s, con);
            cmd.Parameters.AddWithValue("@id", (int)data["id"].Value);
            cmd.Parameters.AddWithValue("@limitation_id", (int)data["limitation"]["id"].Value);
            cmd.Parameters.AddWithValue("@product_id", (int)data["product"]["id"].Value);
            cmd.ExecuteNonQuery();
            return (int)data["id"].Value;
        }

        public static object[] productToCategoryIDGetir(string ürünID, MySqlConnection con)
        {
            MySqlCommand cmd = new MySqlCommand("select id,category_id from product_to_categories where product_id=" + ürünID, con);
            MySqlDataReader dr = cmd.ExecuteReader();
            int id = -1;
            int categoryId = -1;
            if (dr.Read())
            {
                id = dr.GetInt32(0);
                categoryId = dr.GetInt32(1);
            }
            dr.Close();
            return new object[] { id, categoryId.ToString() };
        }
        public static int productToCategoryOluştur(dynamic data, bool yeni, MySqlConnection con)
        {
            string s = yeni ? "insert into product_to_categories values(@id,@sortOrder,@product_id,@category_id)" : "update product_to_categories set sortOrder=@sortOrder, product_id=@product_id, category_id=@category_id where id=@id";
            MySqlCommand cmd = new MySqlCommand(s, con);
            cmd.Parameters.AddWithValue("@id", (int)data["id"].Value);
            if (object.ReferenceEquals(null, data["sortOrder"]) || data["sortOrder"] == null) cmd.Parameters.AddWithValue("@sortOrder", DBNull.Value);
            else cmd.Parameters.AddWithValue("@sortOrder", (int)data["sortOrder"].Value);
            cmd.Parameters.AddWithValue("@product_id", (int)data["product"]["id"].Value);
            cmd.Parameters.AddWithValue("@category_id", (int)data["category"]["id"].Value);
            cmd.ExecuteNonQuery();
            return (int)data["id"].Value;
        }

        public static void ideaSoftTablosunuOluştur()
        {
            int kayıtSayısı = 0;
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select count(*) from products where distributor in ('dinamik','motorasin')", con);
            cmd.CommandTimeout = 600; //600 sn.
            MySqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read()) kayıtSayısı = dr.GetInt32(0);
            dr.Close();
            if (kayıtSayısı == 0)
            {
                log.Yaz("product tablosu boş. İşlem yapılamadı.\n'İdeasoft tüm verileri çek' diyerek verileri çektikten sonra tekrar deneyin.", "Hata");
            }
            else
            {
                DateTime t = DateTime.Now;
                log.Yaz("ideasoft tablosu oluşturuluyor");
                //veritabanı.tabloOluştur_withoutId("ideasoft", veritabanı.ideasoftTablosuYapısı, veritabanı.ideasoftTablosuIndexYapısı, con);
                cmd.CommandText = "delete from ideasoft;" + veritabanı.ideaSoftTablosuEklemeSQL;
                cmd.ExecuteNonQuery();
                TimeSpan geçen = DateTime.Now.Subtract(t);
                log.Yaz("ideasoft tablosu oluşturuldu. Geçen süre: " + (geçen.Days == 0 ? "" : geçen.Days.ToString() + " gün ") + (geçen.Hours == 0 ? "" : geçen.Hours.ToString() + " saat ") + (geçen.Minutes == 0 ? "" : geçen.Minutes.ToString() + " dakika ") + (geçen.Seconds == 0 ? "" : geçen.Seconds.ToString() + " saniye"));
            }
            con.Close();
        }
        public static void ideaSoft_TablosunuOluştur()
        {
            if (Thread_ideaSoft_oluşturma != null) if (Thread_ideaSoft_oluşturma.IsAlive) return;

            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            bool varmı = veritabanı.tabloVarmı("products", con);
            con.Close();
            if (varmı)
            {
                Thread_ideaSoft_oluşturma = new Thread(() => ideaSoftTablosunuOluştur());
                Thread_ideaSoft_oluşturma.Start();
            }
            else log.Yaz("'products' tablosu olmadığından ideasoft tablosu oluşturulamıyor. !", "Hata");
        }
        public static void BAŞLA()
        {
            brand = new ClassNesne("brands", "id INT(11), name TINYTEXT, slug TINYTEXT,  distributorCode TINYTEXT, distributor TINYTEXT, imageFile TINYTEXT, metaKeywords TEXT, metaDescription TEXT, pageTitle TINYTEXT", "PRIMARY KEY (`id`) USING BTREE");
            currency = new ClassNesne("currencies", "id INT(11), label VARCHAR(50), abbr VARCHAR(5)", "PRIMARY KEY(`id`) USING BTREE");
            price = new ClassNesne("product_prices", "id INT(11), value FLOAT, type INT(1), product_id INT(11)", "PRIMARY KEY(`id`) USING BTREE, INDEX `product_id` (`product_id`) USING BTREE");
            details = new ClassNesne("product_details", "id INT(11), sku TINYTEXT, details TEXT, product_id INT(11)", "PRIMARY KEY (`id`) USING BTREE, INDEX `product_id` (`product_id`) USING BTREE");
            category = new ClassNesne("categories", "id INT(11), name TINYTEXT, slug TINYTEXT, sortOrder INT(3),  distributorCode TINYTEXT, distributor VARCHAR(128), imageFile TINYTEXT, metaKeywords TEXT, metaDescription TEXT, pageTitle TINYTEXT, parent_id INT(11)", "PRIMARY KEY (`id`) USING BTREE, INDEX `name` (`name`(63)) USING BTREE, INDEX `parent_id` (`parent_id`) USING BTREE");
            product_to_category = new ClassNesne("product_to_categories", "id INT(11), sortOrder INT(4), product_id INT(11), category_id INT(11)", "PRIMARY KEY (`id`) USING BTREE, INDEX `product_id` (`product_id`) USING BTREE");
            product_image = new ClassNesne("product_images", "id INT(11), filename TINYTEXT, extension VARCHAR(4), directoryName VARCHAR(10), sortOrder INT(1), product_id INT(11), attachment MEDIUMTEXT", "PRIMARY KEY (`id`) USING BTREE, INDEX `product_id` (`product_id`) USING BTREE");
            purchase_limitations = new ClassNesne("purchase_limitations", "id INT(11), name TINYTEXT, minimumLimit INT(4), maximumLimit INT(4), type VARCHAR(10), status BOOLEAN", "PRIMARY KEY (`id`) USING BTREE");
            purchase_limitation_items = new ClassNesne("purchase_limitation_items", "id INT(11), limitation_id INT(11), product_id INT(11)", "PRIMARY KEY (`id`) USING BTREE, INDEX `product_id` (`product_id`) USING BTREE");
            tags = new ClassNesne("tags", "id INT(11), name TINYTEXT, count INT(3), metaKeywords TEXT, metaDescription TEXT, pageTitle TEXT", "PRIMARY KEY(`id`) USING BTREE");
            product_to_tags = new ClassNesne("product_to_tags", "id INT(11), product_id INT(11), tag_id INT(11)", "PRIMARY KEY (`id`) USING BTREE, INDEX `product_id` (`product_id`) USING BTREE");
            product = new ClassNesne("products", "id INT(11), name TEXT, slug TEXT, fullName TEXT, sku TINYTEXT, barcode TINYTEXT, price1 FLOAT, distributor VARCHAR(50), stockAmount FLOAT, buyingPrice FLOAT, stockTypeLabel VARCHAR(8), discount INT(4), discountType INT(1), metaKeywords TEXT, metaDescription TEXT, pageTitle TINYTEXT, searchKeywords TINYTEXT, brand_id INT(11), currency_id INT(11), parent_id INT(11), discountedSortOrder TINYINT(4) DEFAULT NULL", "PRIMARY KEY (`id`) USING BTREE, INDEX `sku` (`sku`(85)) USING BTREE");
        }
    }

    public class Nesne_SatınAlmaLimiti
    {
        public int id;
        public string name;
        public double minimumLimit;
        public double maximumLimit;
        public string type;
        public bool status;
        public Nesne_SatınAlmaLimiti(dynamic data)
        {
            id = (int)data["id"].Value;
            name = (string)data["name"].Value;
            minimumLimit = (double)data["minimumLimit"].Value;
            maximumLimit = (double)data["maximumLimit"].Value;
            type = (string)data["type"].Value;
            status = (bool)data["status"].Value;
        }
    }

}
