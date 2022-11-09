using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using RestSharp;

namespace NettenGelsin
{
    public class İşlem
    {

        public Label label;
        public ProgressBar progressBar;
        public ListBox listBoxFiyatArttırılacaklar;
        public ListBox listBoxSilinecekler;
        public NumericUpDown numericUpDown;
        public RadioButton radioButton;
        public bool debugMode;

        public static string mesaj;
        public static string motorasinHatlıResimler = "";
        public static string dinamikHatlıResimler = "";

        string şimdi { get { return DateTime.Now.ToShortTimeString(); } }
        bool cumartesi_mi(string firma)
        {
            DateTime t = DateTime.Now;
            if (firma == "motorasin")
            {
                return (t.DayOfWeek == DayOfWeek.Saturday && t.Hour >= 20) || (t.DayOfWeek == DayOfWeek.Sunday && t.Hour < 20);
            }
            else if (firma == "dinamik")
            {
                return (t.DayOfWeek == DayOfWeek.Saturday && t.Hour >= 21) || (t.DayOfWeek == DayOfWeek.Sunday && t.Hour < 21);
            }
            else return false;
        }

        public İşlem(Label label_, ProgressBar progressBar_, ListBox listBoxFiyatArttırılacaklar_, ListBox listBoxSilinecekler_, NumericUpDown numericUpDown_, RadioButton radioButton_, CheckBox checkBoxDebug)
        {
            label = label_;
            progressBar = progressBar_;
            radioButton = radioButton_;
            numericUpDown = numericUpDown_;
            listBoxFiyatArttırılacaklar = listBoxFiyatArttırılacaklar_;
            listBoxSilinecekler = listBoxSilinecekler_;
            debugMode = checkBoxDebug.Checked;
        }

        public void Yap(İşlemler_Tipi işlem)
        {
            int x;
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            MySqlCommand cmd = new MySqlCommand();
            MySqlDataReader dr;

            switch (işlem)
            {
                #region motoraşinVeriÇek
                case İşlemler_Tipi.motoraşinVeriÇek:
                    mesaj += şimdi + "\tMotoraşin verileri çekilmeye başladı.\n";
                    label.Text = "Motorasin verileri çekiliyor.";
                    label.Refresh();
                    firmalar.Motoraşin.veriÇekmeMotoru(new object[] { firmalar.Motoraşin.stringBuilderİfadesi, firmalar.Motoraşin.anahtarlar });
                    //label3.Text = "Motorasin verileri çekildi. (" + veritabanı.kayıtSayısı("motorasin_ham").ToString() + " kayıt)";
                    //label3.Refresh();
                    mesaj += şimdi + "\tMotorasin verileri çekildi. (" + veritabanı.kayıtSayısı("motorasin_ham").ToString() + " kayıt)\n";
                    break;
                #endregion
                #region motoraşinDeğişim
                case İşlemler_Tipi.motoraşinDeğişim:
                    con.Open();
                    label.Text = "Motorasin değişim uygulanıyor.";
                    label.Refresh();
                    firmalar.Motoraşin.degistirilmişDosyayıOlustur(con);
                    con.Close();
                    break;
                #endregion
                #region motoraşinÇokluKayıtlarıSil
                case İşlemler_Tipi.motoraşinÇokluKayıtlarıSil:
                    label.Text = "Motorasin çoklu kayıtlar siliniyor.";
                    label.Refresh();
                    veritabanı.çokluKayıtlarıEngelle("motorasin");
                    break;
                #endregion
                #region motoraşinOrtağaAktar
                case İşlemler_Tipi.motoraşinOrtağaAktar:
                    con.Open();
                    label.Text = "Motorasin Ortağa aktarılıyor.";
                    label.Refresh();
                    firmalar.Motoraşin.ortağaAktar(con);
                    con.Close();
                    break;
                #endregion
                #region motoraşinResimleriİçeAktar
                case İşlemler_Tipi.motoraşinResimleriİçeAktar:
                    if (cumartesi_mi("motorasin"))
                    {
                        con.Open();
                        label.Text = "Motorasin resimleri içe aktarılıyor.";
                        label.Refresh();
                        object[] s = firmalar.Motoraşin.resimleri_kontrol_et(progressBar, label, con);
                        con.Close();
                        if (s != null)
                        {
                            motorasinHatlıResimler = (string)s[2];
                            if ((int)s[0] > 0)
                                mesaj += şimdi + "\t" + string.Format("Motorasinden {0} yeni resimden {1} tanesi içe aktarıldı. Hatalı resimler için 'Kopyala menüsünü kullanabilirsiniz.'\n", (int)s[0], (int)s[1]);
                        }
                    }
                    break;
                #endregion

                #region dinamikVerileriÇek
                case İşlemler_Tipi.dinamikVerileriÇek:
                    label.Text = "Dinamik verileri çekiliyor.";
                    label.Refresh();
                    mesaj += şimdi + "\tDinamik verileri çekilmeye başladı.\n";
                    firmalar.Dinamik.veriÇekmeMotoru(new object[] { firmalar.Dinamik.stringBuilderİfadesi, firmalar.Dinamik.anahtarlar });
                    //label3.Text = "Dinamik verileri çekildi. (" + veritabanı.kayıtSayısı("dinamik_ham").ToString() + " kayıt)";
                    //label3.Refresh();
                    mesaj += şimdi + "\tDinamik verileri çekildi. (" + veritabanı.kayıtSayısı("dinamik_ham").ToString() + " kayıt)\n";
                    break;
                #endregion
                #region dinamikDeğişim
                case İşlemler_Tipi.dinamikDeğişim:
                    con.Open();
                    label.Text = "Dinamik değişim uygulanıyor.";
                    label.Refresh();
                    firmalar.Dinamik.degistirilmişDosyayıOlustur(con);
                    con.Close();
                    break;
                #endregion
                #region dinamikÇokluKayıtlarıSil
                case İşlemler_Tipi.dinamikÇokluKayıtlarıSil:
                    label.Text = "Dinamik çoklu kayıtlar siliniyor.";
                    label.Refresh();
                    veritabanı.çokluKayıtlarıEngelle("dinamik");
                    break;
                #endregion
                #region dinamikOrtağaAktar
                case İşlemler_Tipi.dinamikOrtağaAktar:
                    con.Open();
                    label.Text = "Dinamik Ortağa aktarılıyor.";
                    label.Refresh();
                    firmalar.Dinamik.ortağaAktar(con);
                    con.Close();
                    break;
                #endregion
                #region dinamikResimleriİçeAktar
                case İşlemler_Tipi.dinamikResimleriİçeAktar:
                    if (cumartesi_mi("dinamik"))
                    {
                        con.Open();
                        label.Text = "Dinamik resimleri içe aktarılıyor.";
                        label.Refresh();
                        object[] s = firmalar.Dinamik.resimleri_kontrol_et(progressBar, label, con);
                        con.Close();
                        if (s != null)
                        {
                            dinamikHatlıResimler = (string)s[2];
                            if ((int)s[0] > 0)
                                mesaj += şimdi + "\t" + string.Format("Dinamikten {0} yeni resimden {1} tanesi içe aktarıldı. Hatalı resimler için 'Kopyala menüsünü kullanabilirsiniz.'\n", (int)s[0], (int)s[1]);
                        }
                    }
                    break;
                #endregion

                #region ortakTekrarlıKayıtlarınBilgileriniDinamiktekiGibiYap
                case İşlemler_Tipi.ortakTekrarlıKayıtlarınBilgileriniDinamiktekiGibiYap:
                    con.Open();
                    label.Text = "Çift kayıtlardan label, metakeywords ve detail alanları dinamik firmasından gelenlerinki ile aynı yapılıyor";
                    label.Refresh();
                    //çift kayıtlardan label,metakeywords ve detail alanları dinamik firmasından gelenlerinki ile aynı yapılıyor
                    cmd = new MySqlCommand("UPDATE ortak t1 INNER JOIN ortak t2 ON t1.stok_kodu=t2.stok_kodu AND t1.id<>t2.id SET t1.label=t2.label, t1.metaKeywords=t2.metaKeywords, t1.detail=t2.detail, t1.barcode=t2.barcode WHERE t2.nereden = 'dinamik'", con);
                    cmd.CommandTimeout = 600;
                    veritabanı.cmdExecute(cmd);

                    cmd.CommandText = "DROP TABLE IF EXISTS ortak_;CREATE TABLE ortak_ LIKE ortak;INSERT INTO ortak_ SELECT * FROM ortak;";
                    veritabanı.cmdExecute(cmd);
                    con.Close();
                    break;
                #endregion
                #region ortaktan_Stok_Fiyat_PaketMiktarı_SıfırOlanlarıSil
                case İşlemler_Tipi.ortaktan_Stok_Fiyat_PaketMiktarı_SıfırOlanlarıSil:
                    con.Open();
                    label.Text = "Ortak stoğu olmayan veya fiyatı 0 olanlar siliniyor."; label.Refresh();
                    cmd.CommandText = "DELETE FROM ortak WHERE ortak.stok_amount<=0 OR ortak.price<=0 OR ortak.paket_miktari<=0;";
                    veritabanı.cmdExecute(cmd);
                    con.Close();
                    break;
                #endregion
                #region ortakTekrarlıKayıtlardanYüksekFiyatlılarıSil
                case İşlemler_Tipi.ortakTekrarlıKayıtlardanYüksekFiyatlılarıSil:
                    con.Open();
                    label.Text = "Çift kayıtlardan fiyatı yüksek olan siliniyor."; label.Refresh();
                    cmd.CommandText = string.Format("DELETE t1 FROM ortak t1 INNER JOIN ortak t2 WHERE t1.stok_kodu = t2.stok_kodu and TL_Fiyat_Ver(t1.currency_abbr, t1.price, {0}, {1}) > TL_Fiyat_Ver(t2.currency_abbr, t2.price, {0}, {1}); DELETE t1 FROM ortak t1 INNER JOIN ortak t2 WHERE t1.stok_kodu = t2.stok_kodu and t1.id < t2.id; ", Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.'));
                    veritabanı.cmdExecute(cmd);
                    con.Close();
                    break;
                #endregion

                #region ortakİçindeGeçenlerinFiyatlarınıArttır
                case İşlemler_Tipi.ortakİçindeGeçenlerinFiyatlarınıArttır:
                    string[] fiyatArttırılacaklar = new string[listBoxFiyatArttırılacaklar.Items.Count];
                    listBoxFiyatArttırılacaklar.Items.CopyTo(fiyatArttırılacaklar, 0);
                    veritabanı.icindeGecenlerinFiyatlariniArttir(fiyatArttırılacaklar, (double)numericUpDown.Value, radioButton.Checked);
                    break;
                #endregion
                #region ortakSilinecekleriSil
                case İşlemler_Tipi.ortakSilinecekleriSil:
                    string[] silinecekler = new string[listBoxSilinecekler.Items.Count];
                    listBoxSilinecekler.Items.CopyTo(silinecekler, 0);
                    veritabanı.icindeGecenleriSil(silinecekler);
                    break;
                #endregion
                #region ortakTablosunuBoyutununKüçült
                case İşlemler_Tipi.ortakTablosunuBoyutununKüçült:
                    con.Open();
                    string işlemGöreceklerlerin_Stok_kodu_Listesi = veritabanı.işlemGöreceklerListesi(150000, Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.'));
                    cmd = new MySqlCommand(string.Format("DELETE FROM ortak WHERE ortak.stok_kodu {0} ", (işlemGöreceklerlerin_Stok_kodu_Listesi.IndexOf(',') > -1 ? "NOT IN " : "!=") + işlemGöreceklerlerin_Stok_kodu_Listesi), con);
                    cmd.CommandTimeout = 600;
                    veritabanı.cmdExecute(cmd);
                    con.Close();
                    break;
                #endregion
                #region ortakTablosunaEldeOlanlarıEkle
                case İşlemler_Tipi.ortakTablosunaEldeOlanlarıEkle:
                    //burada elde olanlar Yoksa ekleniyor varsa güncelleniyor.
                    con.Open();
                    cmd.CommandText = "insert INTO ortak(stok_kodu, label, metaKeywords, brand, category_path, barcode, price, currency_abbr, paket_miktari, stok_amount, stock_type, discount, discountType, nereden, detail, discountedSortOrder) (select stok_kodu, label, metaKeywords, brand, category_path, barcode,price, currency_abbr, paket_miktari, stok_amount, stock_type, discount, discountType, nereden, detail, discountedSortOrder from elde_olanlar WHERE elde_olanlar.stok_kodu NOT IN (SELECT ortak.stok_kodu FROM ortak))";
                    veritabanı.cmdExecute(cmd);
                    con.Close();
                    break;
                #endregion

                #region ortakDeğişim
                case İşlemler_Tipi.ortakDeğişim:
                    con.Open();
                    label.Text = "Ortak değişim uygulanıyor.";
                    label.Refresh();
                    cmd = new MySqlCommand("call degistir('ortak');", con);
                    cmd.CommandTimeout = 600;
                    veritabanı.cmdExecute(cmd);
                    con.Close();
                    break;
                #endregion
                #region ortakBinekDüzeltme
                case İşlemler_Tipi.ortakBinekDüzeltme:
                    label.Text = "Binek düzeltme yapılıyor.";
                    label.Refresh();
                    veritabanı.binekDüzeltme();
                    break;
                #endregion
                #region ortakDiscountSortOrderAyarlanıyor
                case İşlemler_Tipi.ortakDiscountSortOrderAyarlanıyor:
                    List<string> skular = new List<string>();
                    label.Text = "İndirimli ürünler için discountSortOrder değerleri ayarlanıyor.";
                    label.Refresh();
                    con.Open();
                    cmd = new MySqlCommand(string.Format("SELECT stok_kodu FROM ortak WHERE ortak.discount>0 order by TL_Fiyat_Ver(ortak.currency_abbr, ortak.price, {0}, {1})*ortak.discount/100 DESC LIMIT 100", Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.')), con);
                    dr = cmd.ExecuteReader();
                    while (dr.Read()) skular.Add(dr.GetString(0));
                    dr.Close();
                    for (int i = 0; i < skular.Count; i++)
                    {
                        cmd.CommandText = string.Format("update ortak set discountedSortOrder={0} where stok_kodu='{1}'", i, skular[i]);
                        veritabanı.cmdExecute(cmd);
                    }
                    con.Close();
                    break;
                #endregion

                #region ortak_TablosunuOluştur
                case İşlemler_Tipi.ortak_TablosunuOluştur:
                    //burada  veritabanı.işlemGöreceklerListesi metodu sadece bu kayıtları silmek için kullanılıyor. metoda gerek olmadan burada o işlemler yapılacak.
                    Yap(İşlemler_Tipi.ortakTekrarlıKayıtlarınBilgileriniDinamiktekiGibiYap);
                    Yap(İşlemler_Tipi.ortaktan_Stok_Fiyat_PaketMiktarı_SıfırOlanlarıSil);
                    Yap(İşlemler_Tipi.ortakTekrarlıKayıtlardanYüksekFiyatlılarıSil);

                    Yap(İşlemler_Tipi.ortakİçindeGeçenlerinFiyatlarınıArttır);
                    Yap(İşlemler_Tipi.ortakSilinecekleriSil);
                    Yap(İşlemler_Tipi.ortakTablosunuBoyutununKüçült);
                    Yap(İşlemler_Tipi.ortakTablosunaEldeOlanlarıEkle);

                    Yap(İşlemler_Tipi.ortakDeğişim);
                    Yap(İşlemler_Tipi.ortakBinekDüzeltme);
                    Yap(İşlemler_Tipi.ortakDiscountSortOrderAyarlanıyor);
                    break;
                #endregion

                #region ideaSoftTablosuOluştur
                case İşlemler_Tipi.ideaSoftTablosuOluştur:
                    label.Text = "İdeasoft tablosu oluşturuluyor";
                    label.Refresh();
                    IdeaSoftVeritabanı.ideaSoftTablosunuOluştur();
                    break;
                #endregion
                #region ortaktaOlmayanKayıtlarSitedenSiliniyor
                case İşlemler_Tipi.ortaktaOlmayanKayıtlarSitedenSiliniyor:
                    label.Text = "Olmayan kayıtlar siliniyor.";
                    label.Refresh();
                    x = Entegrasyon.ürünleriSil("SELECT id FROM ideasoft WHERE stok_kodu NOT IN (SELECT stok_kodu FROM ortak);", progressBar, label);
                    mesaj += şimdi + "\t" + x.ToString() + " ürün silindi.\n";
                    break;
                #endregion
                #region sitedeOlmayanKayıtlarEkleniyor
                case İşlemler_Tipi.sitedeOlmayanKayıtlarEkleniyor:
                    label.Text = "Ürünlerin ekleme işlemi yapılıyor.";
                    label.Refresh();
                    x = Entegrasyon.ürünleriEkle(Entegrasyon.işlemTipi.eklenecek, progressBar, label);
                    mesaj += şimdi + "\t" + x.ToString() + " ürün eklendi.\n";
                    break;
                #endregion
                #region güncellemeYapılıyor
                case İşlemler_Tipi.güncellemeYapılıyor:
                    label.Text = "ürünlerin güncelleme işlemi yapılıyor.";
                    label.Refresh();
                    x = Entegrasyon.ürünleriEkle(Entegrasyon.işlemTipi.güncellenecek, progressBar, label);
                    mesaj += şimdi + "\t" + x.ToString() + " ürün güncellendi.\n";
                    break;
                #endregion
                #region yayındakiÜrünlerdenResmiOlmayanlarınResmiGönderiliyor
                case İşlemler_Tipi.yayındakiÜrünlerdenResmiOlmayanlarınResmiGönderiliyor:
                    label.Text = "Yayındaki ürünlerden resmi olmayanların resimleri gönderiliyor.";
                    progressBar.Value = 0;
                    con.Open();
                    int adet = 0;
                    cmd = new MySqlCommand("select count(resimler.sku) from products inner join resimler on products.sku=resimler.sku where products.id not in (select product_id from product_images)", con);
                    cmd.CommandTimeout = 1200;
                    dr = cmd.ExecuteReader();
                    if (dr.Read()) adet = dr.GetInt32(0);
                    dr.Close();
                    if (adet == 0) break;

                    int başlama = 0;
                    int miktar = 100;
                    label.Tag = label.Text;
                    List<string> bodys = new List<string>();
                    int bekleme = Thread_Apiİşlemleri.bekleme;
                    IRestResponse data;
                    x = 0;
                    do
                    {
                        progressBar.Value = 0; progressBar.Maximum = miktar;
                        label.Text = string.Concat("(", başlama / miktar + 1, "/", adet / miktar, ") ", (string)label.Tag); label.Refresh();
                        cmd.CommandText = string.Format("SELECT resimler.sku,products.id,resimler.resim_base64 FROM products inner join resimler on products.sku = resimler.sku WHERE products.id not in (select product_id from product_images) LIMIT 0,{0}", miktar);
                        dr = cmd.ExecuteReader();
                        bodys.Clear();
                        while (dr.Read())
                        {
                            progressBar.Value++; progressBar.Refresh();
                            bodys.Add(Resim_işlemleri.getData_bodyFormat(dr.GetString(0), dr.GetInt32(1), dr.GetString(2)));
                        }
                        dr.Close();
                        progressBar.Maximum = bodys.Count;
                        progressBar.Value = 0;
                        if (bodys.Count > 0)
                        {
                            foreach (string body in bodys)
                            {
                                progressBar.Value++; progressBar.Refresh();
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
                                    if (d["id"] != null) { IdeaSoftVeritabanı.resimOluştur(d, con); x++; }
                                }
                            }
                        }
                        başlama += miktar;
                    } while ((başlama + miktar) < adet);
                    mesaj += şimdi + "\t" + x.ToString() + " ürün resmi gönderildi.\n";
                    con.Close();
                    break;
                #endregion


                #region SADECE_GÜNCELLEME
                case İşlemler_Tipi.sadeceGüncelleme:
                    Yap(İşlemler_Tipi.motoraşinVeriÇek);
                    Yap(İşlemler_Tipi.motoraşinDeğişim);
                    Yap(İşlemler_Tipi.motoraşinÇokluKayıtlarıSil);
                    Yap(İşlemler_Tipi.motoraşinOrtağaAktar);

                    Yap(İşlemler_Tipi.dinamikVerileriÇek);
                    Yap(İşlemler_Tipi.dinamikDeğişim);
                    Yap(İşlemler_Tipi.dinamikÇokluKayıtlarıSil);
                    Yap(İşlemler_Tipi.dinamikOrtağaAktar);

                    Yap(İşlemler_Tipi.ortakTekrarlıKayıtlarınBilgileriniDinamiktekiGibiYap);
                    Yap(İşlemler_Tipi.ortakTekrarlıKayıtlardanYüksekFiyatlılarıSil);

                    Yap(İşlemler_Tipi.ortakİçindeGeçenlerinFiyatlarınıArttır);

                    Yap(İşlemler_Tipi.ortakDeğişim);
                    Yap(İşlemler_Tipi.ortakBinekDüzeltme);
                    Yap(İşlemler_Tipi.ortakDiscountSortOrderAyarlanıyor);

                    Yap(İşlemler_Tipi.ideaSoftTablosuOluştur);

                    label.Text = "Ürünlerin güncelleme işlemi yapılıyor.";
                    label.Refresh();
                    x = Entegrasyon.sadeceGüncelle(progressBar, label, debugMode);
                    mesaj += şimdi + "\t" + x.ToString() + " ürün güncellendi.\n";
                    break;
                #endregion
                default:
                    break;
            }
        }
    }
}
