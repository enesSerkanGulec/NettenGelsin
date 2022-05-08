using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using MySql.Data.MySqlClient;
using RestSharp;
using Newtonsoft.Json;
using System.Data;

namespace NettenGelsin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static int hata = 0;

        string motorasinHatlıResimler = "";
        string dinamikHatlıResimler = "";
        Thread thread_ideaSoftTümVerileriÇekme;

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            setting.Başla();
            setting.dosyadanOku(radioButton1, radioButton2, numericUpDown2, listBox1, listBox2, numericUpDown1, rbGüncelleme, rbNormal, rbRaporOnay, rbSadeceGüncelleme, rbSilmeyeDevam);
            veritabanı.Başla();
            TokenKeySınıfı.Başla();
            IdeaSoftVeritabanı.BAŞLA();
            firmalar.BAŞLA();
            log.Başla();
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            elde_olanları_getir(con);
            con.Close();
            log.Yaz("PROGRAM AÇILDI..");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            program.Kapat();
        }

        void ideaSoftTümVerileriÇekme(int hangisi)
        {
            DateTime başlama = DateTime.Now;
            ClassNesne.lb = label1;
            if (hangisi == 0 || hangisi == 1)
            {
                label1.Text = "product_image";
                IdeaSoftVeritabanı.product_image.verileriÇek();
                IdeaSoftVeritabanı.product_image.T_veriÇekme.Join();
            }

            if (hangisi == 0 || hangisi == 2)
            {
                label1.Text = "brand";
                IdeaSoftVeritabanı.brand.verileriÇek();
                IdeaSoftVeritabanı.brand.T_veriÇekme.Join();
            }

            if (hangisi == 0 || hangisi == 3)
            {
                label1.Text = "currency";
                IdeaSoftVeritabanı.currency.verileriÇek();
                IdeaSoftVeritabanı.currency.T_veriÇekme.Join();
            }

            if (hangisi == 0 || hangisi == 4)
            {
                label1.Text = "details";
                IdeaSoftVeritabanı.details.verileriÇek();
                IdeaSoftVeritabanı.details.T_veriÇekme.Join();
            }

            if (hangisi == 0 || hangisi == 5)
            {
                label1.Text = "category";
                IdeaSoftVeritabanı.category.verileriÇek();
                IdeaSoftVeritabanı.category.T_veriÇekme.Join();
            }

            if (hangisi == 0 || hangisi == 6)
            {
                label1.Text = "product_to_category";
                IdeaSoftVeritabanı.product_to_category.verileriÇek();
                IdeaSoftVeritabanı.product_to_category.T_veriÇekme.Join();
            }

            if (hangisi == 0 || hangisi == 7)
            {
                label1.Text = "purchase_limitations";
                IdeaSoftVeritabanı.purchase_limitations.verileriÇek();
                IdeaSoftVeritabanı.purchase_limitations.T_veriÇekme.Join();
            }

            if (hangisi == 0 || hangisi == 8)
            {
                label1.Text = "purchase_limitation_items";
                IdeaSoftVeritabanı.purchase_limitation_items.verileriÇek();
                IdeaSoftVeritabanı.purchase_limitation_items.T_veriÇekme.Join();
            }

            if (hangisi == 0 || hangisi == 9)
            {
                label1.Text = "product";
                IdeaSoftVeritabanı.product.verileriÇek();
                IdeaSoftVeritabanı.product.T_veriÇekme.Join();
            }

            if (hangisi == 0 || hangisi == 10)
            {
                label1.Text = "tags";
                IdeaSoftVeritabanı.tags.verileriÇek();
                IdeaSoftVeritabanı.tags.T_veriÇekme.Join();
            }

            if (hangisi == 0 || hangisi == 11)
            {
                label1.Text = "product_to_tags";
                IdeaSoftVeritabanı.product_to_tags.verileriÇek();
                IdeaSoftVeritabanı.product_to_tags.T_veriÇekme.Join();
            }

            TimeSpan geçen = DateTime.Now.Subtract(başlama);
            string s = "ideasofttan veri çekme işlemleri bitti. İşlem süresi: " + (geçen.Days == 0 ? "" : geçen.Days.ToString() + " gün ") + (geçen.Hours == 0 ? "" : geçen.Hours.ToString() + " saat ") + (geçen.Minutes == 0 ? "" : geçen.Minutes.ToString() + " dakika ") + (geçen.Seconds == 0 ? "" : geçen.Seconds.ToString() + " saniye");
            log.Yaz(s);
            label1.Text = s;

        }

        private void uSDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Entegrasyon.kurNedir("USD").ToString());
        }

        private void eURToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Entegrasyon.kurNedir("EUR").ToString());
        }

        private void resimİşlemleriToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resim r = new resim();
            r.ShowDialog();
        }

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
        private void button9_Click_1(object sender, EventArgs e)
        {
            string mesaj = "";

            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);

            if (cb2.Checked)
            {
                mesaj += şimdi + "\tMotoraşin verileri çekilmeye başladı.\n";
                label3.Text = "Motorasin verileri çekiliyor.";
                label3.Refresh();
                firmalar.Motoraşin.veriÇekmeMotoru(new object[] { firmalar.Motoraşin.stringBuilderİfadesi, firmalar.Motoraşin.anahtarlar });
                //label3.Text = "Motorasin verileri çekildi. (" + veritabanı.kayıtSayısı("motorasin_ham").ToString() + " kayıt)";
                //label3.Refresh();
                mesaj += şimdi + "\tMotorasin verileri çekildi. (" + veritabanı.kayıtSayısı("motorasin_ham").ToString() + " kayıt)\n";
            }
            if (cb3.Checked)
            {
                con.Open();
                label3.Text = "Motorasin değişim uygulanıyor.";
                label3.Refresh();
                firmalar.Motoraşin.degistirilmişDosyayıOlustur(con);
                con.Close();
            }
            if (cb4.Checked)
            {
                label3.Text = "Motorasin çoklu kayıtlar siliniyor.";
                label3.Refresh();
                veritabanı.çokluKayıtlarıEngelle("motorasin");
            }
            if (cb5.Checked)
            {
                con.Open();
                label3.Text = "Motorasin Ortağa aktarılıyor.";
                label3.Refresh();
                firmalar.Motoraşin.ortağaAktar(con);
                con.Close();
            }
            if (cb6.Checked)
            {
                if (cumartesi_mi("motorasin"))
                {
                    con.Open();
                    label3.Text = "Motorasin resimleri içe aktarılıyor.";
                    label3.Refresh();
                    object[] s = firmalar.Motoraşin.resimleri_kontrol_et(progressBar2, label3, con);
                    con.Close();
                    if (s != null)
                    {
                        motorasinHatlıResimler = (string)s[2];
                        if ((int)s[0] > 0)
                            mesaj += şimdi + "\t" + string.Format("Motorasinden {0} yeni resimden {1} tanesi içe aktarıldı. Hatalı resimler için 'Kopyala menüsünü kullanabilirsiniz.'\n", (int)s[0], (int)s[1]);
                    }
                }
            }



            if (cb7.Checked)
            {
                label3.Text = "Dinamik verileri çekiliyor.";
                label3.Refresh();
                mesaj += şimdi + "\tDinamik verileri çekilmeye başladı.\n";
                firmalar.Dinamik.veriÇekmeMotoru(new object[] { firmalar.Dinamik.stringBuilderİfadesi, firmalar.Dinamik.anahtarlar });
                //label3.Text = "Dinamik verileri çekildi. (" + veritabanı.kayıtSayısı("dinamik_ham").ToString() + " kayıt)";
                //label3.Refresh();
                mesaj += şimdi + "\tDinamik verileri çekildi. (" + veritabanı.kayıtSayısı("dinamik_ham").ToString() + " kayıt)\n";
            }
            if (cb8.Checked)
            {
                con.Open();
                label3.Text = "Dinamik değişim uygulanıyor.";
                label3.Refresh();
                firmalar.Dinamik.degistirilmişDosyayıOlustur(con);
                con.Close();

            }
            if (cb9.Checked)
            {
                label3.Text = "Dinamik çoklu kayıtlar siliniyor.";
                label3.Refresh();
                veritabanı.çokluKayıtlarıEngelle("dinamik");
            }
            if (cb10.Checked)
            {
                con.Open();
                label3.Text = "Dinamik Ortağa aktarılıyor.";
                label3.Refresh();
                firmalar.Dinamik.ortağaAktar(con);
                con.Close();
            }
            if (cb11.Checked)
            {
                if (cumartesi_mi("dinamik"))
                {
                    con.Open();
                    label3.Text = "Dinamik resimleri içe aktarılıyor.";
                    label3.Refresh();
                    object[] s = firmalar.Dinamik.resimleri_kontrol_et(progressBar2, label3, con);
                    con.Close();
                    if (s != null)
                    {
                        dinamikHatlıResimler = (string)s[2];
                        if ((int)s[0] > 0)
                            mesaj += şimdi + "\t" + string.Format("Dinamikten {0} yeni resimden {1} tanesi içe aktarıldı. Hatalı resimler için 'Kopyala menüsünü kullanabilirsiniz.'\n", (int)s[0], (int)s[1]);
                    }
                }
            }


            if (cb12.Checked)
            {
                con.Open();
                label3.Text = "Çift kayıtlardan label, metakeywords ve detail alanları dinamik firmasından gelenlerinki ile aynı yapılıyor";
                label3.Refresh();
                //çift kayıtlardan label,metakeywords ve detail alanları dinamik firmasından gelenlerinki ile aynı yapılıyor
                MySqlCommand cmd = new MySqlCommand("UPDATE ortak t1 INNER JOIN ortak t2 ON t1.stok_kodu=t2.stok_kodu AND t1.id<>t2.id SET t1.label=t2.label, t1.metaKeywords=t2.metaKeywords, t1.detail=t2.detail, t1.barcode=t2.barcode WHERE t2.nereden = 'dinamik'", con);
                cmd.CommandTimeout = 600;
                veritabanı.cmdExecute(cmd);

                cmd.CommandText = "DROP TABLE IF EXISTS ortak_;CREATE TABLE ortak_ LIKE ortak;INSERT INTO ortak_ SELECT * FROM ortak;";
                veritabanı.cmdExecute(cmd);

                label3.Text = "Ortak stoğu olmayan veya fiyatı 0 olanlar siliniyor."; label3.Refresh();
                cmd.CommandText = "DELETE FROM ortak WHERE ortak.stok_amount<=0 OR ortak.price<=0 OR ortak.paket_miktari<=0;";
                veritabanı.cmdExecute(cmd);

                label3.Text = "Çift kayıtlardan fiyatı yüksek olan siliniyor."; label3.Refresh();
                cmd.CommandText = string.Format("DELETE t1 FROM ortak t1 INNER JOIN ortak t2 WHERE t1.stok_kodu = t2.stok_kodu and TL_Fiyat_Ver(t1.currency_abbr, t1.price, {0}, {1}) > TL_Fiyat_Ver(t2.currency_abbr, t2.price, {0}, {1}); DELETE t1 FROM ortak t1 INNER JOIN ortak t2 WHERE t1.stok_kodu = t2.stok_kodu and t1.id < t2.id; ", Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.'));
                veritabanı.cmdExecute(cmd);


                con.Close();
            }

            if (cb13.Checked)
            {

                //burada  veritabanı.işlemGöreceklerListesi metodu sadece bu kayıtları silmek için kullanılıyor. metoda gerek olmadan burada o işlemler yapılacak.
                con.Open();
                label3.Text = "İşlem görecekler tesbit edilip diğerleri ortaktan siliniyor.";
                label3.Refresh();

                string[] fiyatArttırılacaklar = new string[listBox2.Items.Count];
                listBox2.Items.CopyTo(fiyatArttırılacaklar, 0);
                veritabanı.icindeGecenlerinFiyatlariniArttir(fiyatArttırılacaklar, (double)numericUpDown2.Value, radioButton1.Checked);

                string[] silinecekler = new string[listBox1.Items.Count];
                listBox1.Items.CopyTo(silinecekler, 0);
                veritabanı.icindeGecenleriSil(silinecekler);


                string işlemGöreceklerlerin_Stok_kodu_Listesi = veritabanı.işlemGöreceklerListesi(150000, Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.'));
                MySqlCommand cmd = new MySqlCommand(string.Format("DELETE FROM ortak WHERE ortak.stok_kodu {0} ", (işlemGöreceklerlerin_Stok_kodu_Listesi.IndexOf(',') > -1 ? "NOT IN " : "!=") + işlemGöreceklerlerin_Stok_kodu_Listesi), con);
                cmd.CommandTimeout = 600;
                veritabanı.cmdExecute(cmd);

                //burada elde olanlar Yoksa ekleniyor varsa güncelleniyor.
                cmd.CommandText = "insert INTO ortak(stok_kodu, label, metaKeywords, brand, category_path, barcode, price, currency_abbr, paket_miktari, stok_amount, stock_type, discount, discountType, nereden, detail, discountedSortOrder) (select stok_kodu, label, metaKeywords, brand, category_path, barcode,price, currency_abbr, paket_miktari, stok_amount, stock_type, discount, discountType, nereden, detail, discountedSortOrder from elde_olanlar WHERE elde_olanlar.stok_kodu NOT IN (SELECT ortak.stok_kodu FROM ortak))";
                veritabanı.cmdExecute(cmd);
 
                con.Close();
            }
            if (cb14.Checked)
            {
                con.Open();
                label3.Text = "Ortak değişim uygulanıyor.";
                label3.Refresh();
                MySqlCommand cmd = new MySqlCommand("call degistir('ortak');", con);
                cmd.CommandTimeout = 600;
                veritabanı.cmdExecute(cmd);
                con.Close();
            }
            if (cb15.Checked)
            {
                label3.Text = "Binek düzeltme yapılıyor.";
                label3.Refresh();
                veritabanı.binekDüzeltme();
            }

            if (cb16.Checked)
            {
                List<string> skular = new List<string>();
                label3.Text = "İndirimli ürünler için discountSortOrder değerleri ayarlanıyor.";
                label3.Refresh();
                con.Open();
                MySqlCommand cmd = new MySqlCommand(string.Format("SELECT stok_kodu FROM ortak WHERE ortak.discount>0 order by TL_Fiyat_Ver(ortak.currency_abbr, ortak.price, {0}, {1})*ortak.discount/100 DESC LIMIT 100", Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.')), con);
                MySqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read()) skular.Add(dr.GetString(0));
                dr.Close();
                for (int i = 0; i < skular.Count; i++)
                {
                    cmd.CommandText = string.Format("update ortak set discountedSortOrder={0} where stok_kodu='{1}'", i, skular[i]);
                    veritabanı.cmdExecute(cmd);
                }
                con.Close();
            }

            if (cb1.Checked)
            {
                label3.Text = "İdeasoft tablosu oluşturuluyor";
                label3.Refresh();
                IdeaSoftVeritabanı.ideaSoftTablosunuOluştur();
            }

            if (rbGüncelleme.Checked)
            {

            }

            if (rbNormal.Checked)
            {
                if (rbRaporOnay.Checked)
                {

                }
                else if (rbSadeceGüncelleme.Checked)
                {

                }
                else //Sorun yok silmeye devam
                {

                }
            }
            //if (cb17.Checked)
            {
                label3.Text = "Olmayan kayıtlar siliniyor.";
                label3.Refresh();
                int x = Entegrasyon.ürünleriSil("SELECT id FROM ideasoft WHERE stok_kodu NOT IN (SELECT stok_kodu FROM ortak);", progressBar2, label3);
                mesaj += şimdi + "\t" + x.ToString() + " ürün silindi.\n";
            }
            //if (cb18.Checked)
            {
                label3.Text = "Ürünlerin ekleme işlemi yapılıyor.";
                label3.Refresh();
                int x = Entegrasyon.ürünleriEkle(Entegrasyon.işlemTipi.eklenecek, progressBar2, label3);
                mesaj += şimdi + "\t" + x.ToString() + " ürün eklendi.\n";
            }
            //if (cb19.Checked)
            {
                label3.Text = "ürünlerin güncelleme işlemi yapılıyor.";
                label3.Refresh();
                int x = Entegrasyon.ürünleriEkle(Entegrasyon.işlemTipi.güncellenecek, progressBar2, label3);
                mesaj += şimdi + "\t" + x.ToString() + " ürün güncellendi.\n";
            }
            if (cb20.Checked)
            {
                label3.Text = "Yayındaki ürünlerden resmi olmayanların resimleri gönderiliyor.";
                progressBar2.Value = 0;
                con.Open();
                int adet = 0;
                MySqlCommand cmd = new MySqlCommand("select count(resimler.sku) from products inner join resimler on products.sku=resimler.sku where products.id not in (select product_id from product_images)", con);
                cmd.CommandTimeout = 1200;
                MySqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read()) adet = dr.GetInt32(0);
                dr.Close();
                if (adet == 0) return;

                int başlama = 0;
                int miktar = 100;
                label3.Tag = label3.Text;
                List<string> bodys = new List<string>();
                int bekleme = Thread_Apiİşlemleri.bekleme;
                IRestResponse data;
                int x = 0;
                do
                {
                    progressBar2.Value = 0; progressBar2.Maximum = miktar;
                    label3.Text = string.Concat("(", başlama / miktar + 1, "/", adet / miktar, ") ", (string)label3.Tag); label3.Refresh();
                    cmd.CommandText = string.Format("SELECT resimler.sku,products.id,resimler.resim_base64 FROM products inner join resimler on products.sku = resimler.sku WHERE products.id not in (select product_id from product_images) LIMIT 0,{0}", miktar);
                    dr = cmd.ExecuteReader();
                    bodys.Clear();
                    while (dr.Read())
                    {
                        progressBar2.Value++; progressBar2.Refresh();
                        bodys.Add(Resim_işlemleri.getData_bodyFormat(dr.GetString(0), dr.GetInt32(1), dr.GetString(2)));
                    }
                    dr.Close();
                    progressBar2.Maximum = bodys.Count;
                    progressBar2.Value = 0;
                    if (bodys.Count > 0)
                    {
                        foreach (string body in bodys)
                        {
                            progressBar2.Value++; progressBar2.Refresh();
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
            }
            label3.Text = "İŞLEMLER BİTTİ.";
            //MessageBox.Show("İşlemler Bitti");
            if (mesaj != "")
            {
                mesaj = "GÜNCELLEME İŞLEMİ RAPORU\n\n" + mesaj;
                email.send("Günlük işlem", mesaj);
            }
        }

        private void ideasoftVeritabanınıÇekToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (thread_ideaSoftTümVerileriÇekme != null) if (thread_ideaSoftTümVerileriÇekme.IsAlive) return;
            int hangisi = Convert.ToInt32(((ToolStripMenuItem)sender).Tag);
            thread_ideaSoftTümVerileriÇekme = new Thread(() => ideaSoftTümVerileriÇekme(hangisi));
            thread_ideaSoftTümVerileriÇekme.Start();
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            foreach (var item in ((GroupBox)((Button)sender).Parent).Controls)
                if (item.GetType().Name == "CheckBox") ((CheckBox)item).Checked = ((Button)sender).Text == "Hepsi";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            foreach (var item1 in this.Controls)
            {
                if (item1.GetType().Name != "GroupBox") continue;
                foreach (var item2 in ((GroupBox)item1).Controls)
                    if (item2.GetType().Name == "CheckBox") ((CheckBox)item2).Checked = ((Button)sender).Text == "Hepsi";
            }
        }

        private void kopyalaToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            hatalıMotorasinResimleriniToolStripMenuItem.Enabled = motorasinHatlıResimler != "";
            hatalıDinamikResimleriniToolStripMenuItem.Enabled = dinamikHatlıResimler != "";
        }

        private void hatalıDinamikResimleriniToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(dinamikHatlıResimler);
        }

        private void hatalıMotorasinResimleriniToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(motorasinHatlıResimler);
        }

        private void cb17_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            groupBox1.Enabled = rbNormal.Checked;
            settingYaz();
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {

        }

        public void settingYaz()
        {
            string[] silinecekler = new string[listBox1.Items.Count];
            listBox1.Items.CopyTo(silinecekler, 0);

            string[] fiyatArtacaklar = new string[listBox2.Items.Count];
            listBox2.Items.CopyTo(fiyatArtacaklar, 0);

            lock (this)
            {
                setting.dosyayaYaz(radioButton1.Checked, numericUpDown2.Value, silinecekler, fiyatArtacaklar, numericUpDown1.Value, rbGüncelleme.Checked ? 1 : 2, rbRaporOnay.Checked ? 1 : (rbSadeceGüncelleme.Checked ? 2 : 3));
            }

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            settingYaz();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string deger = Microsoft.VisualBasic.Interaction.InputBox((tabControl1.SelectedTab.Equals(tabPage1) ? "Silineceklere" : "Fiyat arttırılacaklara") + " eklenecek değer:", "Ekleme");
            if (deger.Trim() == "") return;
            if (tabControl1.SelectedTab.Equals(tabPage1))
            {
                if (listBox2.Items.IndexOf(deger) > -1)
                    MessageBox.Show("Bu değer 'Fiyat arttırılacaklar' da zaten var.", "Ekleme yapılamıyor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (listBox1.Items.IndexOf(deger) > -1)
                    MessageBox.Show("Bu değer 'Silinecekler' de zaten var.", "Ekleme yapılamıyor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    listBox1.Items.Insert(0, deger);

                    settingYaz();
                }
            }
            else
            {
                if (listBox2.Items.IndexOf(deger) > -1)
                    MessageBox.Show("Bu değer 'Fiyat arttırılacaklar' da zaten var.", "Ekleme yapılamıyor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (listBox1.Items.IndexOf(deger) > -1)
                    MessageBox.Show("Bu değer 'Silinecekler' de zaten var.", "Ekleme yapılamıyor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    listBox2.Items.Insert(0, deger);

                    settingYaz();
                }
            }
        }

        private void radioButton1_CheckedChanged_1(object sender, EventArgs e)
        {
            settingYaz();
        }

        private void tabControl1_TabIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button10.Enabled = button11.Enabled = tabControl1.SelectedIndex < 2;
        }

        private void button11_Click_1(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0 && listBox1.SelectedIndex == -1 || tabControl1.SelectedIndex == 1 && listBox2.SelectedIndex == -1) return;
            if (MessageBox.Show("Seçili değer silinecek. Onaylıyor musunuz?", "Silme Onayı", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                if (tabControl1.SelectedIndex == 0) listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                else listBox2.Items.RemoveAt(listBox2.SelectedIndex);
                settingYaz();
            }

        }

        private void rbSilmeyeDevam_CheckedChanged(object sender, EventArgs e)
        {
            settingYaz();
        }

        void elde_olanları_getir(MySqlConnection con)
        {
            //"select stok_kodu, label, price, currency_abbr, stok_amount from elde_olanlar where stok_kodu='{0}'"
            MySqlCommand cmd = new MySqlCommand("select stok_kodu, label, price as Fiyat, currency_abbr as 'P.birim', stok_amount as 'S.Miktarı' from elde_olanlar order by id desc", con);
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;
            dataGridView1.Columns[0].ReadOnly = dataGridView1.Columns[1].ReadOnly = dataGridView1.Columns[3].ReadOnly = true;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string deger="";
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            while (true)
            {
                deger = Microsoft.VisualBasic.Interaction.InputBox("Elde olan ürünün stok kodu:", "Ekleme",deger);
                if (deger.Trim() == "") return;
                MySqlCommand cmd = new MySqlCommand(string.Format("select stok_kodu from elde_olanlar where stok_kodu='{0}'", deger), con);
                MySqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    dr.Close();
                    MessageBox.Show("Bu stok kodu 'elde_olanlar' da zaten var", "Eklenemiyor..", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    dr.Close();
                    cmd.CommandText = string.Format("select price,currency_abbr,stok_amount from ortak_ where stok_kodu='{0}'", deger);
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        float price = dr.GetFloat(0);
                        float price_;
                        string pBirimi = dr.GetString(1);
                        int stokMiktarı = dr.GetInt32(2);
                        int stokMiktarı_;
                        dr.Close();
                        while (true)
                        {
                            string s = Microsoft.VisualBasic.Interaction.InputBox(string.Format("{0} stok kodlu ürünün fiyatı {1} {2}. Yeni fiyatı giriniz..", deger, price, pBirimi), "Fiyat belirleme", price.ToString());
                            s = s.Trim();
                            if (s == "")
                            {
                                MessageBox.Show("Ekleme işlemi iptal edildi.", "İptal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                con.Close();
                                return;
                            }
                            else if (!float.TryParse(s, out price_))
                            {
                                MessageBox.Show("Girilen değer hatalı.", "Tekrar deneyin.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else break;
                        }
                        while (true)
                        {
                            string s = Microsoft.VisualBasic.Interaction.InputBox(string.Format("{0} stok kodlu ürünün stok miktarı {1}. Yeni stok miktarını giriniz..", deger, stokMiktarı), "Stok miktarı belirleme", stokMiktarı.ToString());
                            s = s.Trim();
                            if (s == "")
                            {
                                MessageBox.Show("Ekleme işlemi iptal edildi.", "İptal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                con.Close();
                                return;
                            }
                            else if (!int.TryParse(s, out stokMiktarı_))
                            {
                                MessageBox.Show("Girilen değer hatalı.", "Tekrar deneyin.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else break;
                        }

                        cmd.CommandText = string.Format("INSERT INTO elde_olanlar(stok_kodu,label,metaKeywords,brand,category_path,barcode,price,currency_abbr,paket_miktari,stok_amount,stock_type,discount,discountType,nereden,detail,discountedSortOrder) SELECT stok_kodu,label,metaKeywords,brand,category_path,barcode,{0},currency_abbr,paket_miktari,{1},stock_type,discount,discountType,nereden,detail,discountedSortOrder FROM ortak_ WHERE stok_kodu='{2}'", price_.ToString().Replace(',', '.'), stokMiktarı_, deger);
                        cmd.ExecuteNonQuery();

                        elde_olanları_getir(con);
                        //for (int i = 0; i < dataGridView1.Rows.Count; i++) dataGridView1.Rows[i].Selected = (string)dataGridView1.Rows[i].Cells[0].Value == deger;
                        dataGridView1.Rows[0].Selected = true;
                        MessageBox.Show("Ürün 'elde_olanlar' a eklendi", "Başarılı..", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    }
                    else
                    {
                        dr.Close();
                        MessageBox.Show("Bu stok kodu 'ortak_' tablosunda bulunamadığından eklenemedi.", "Eklenemiyor..", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            con.Close();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0) return;
            string stokKodu = (string)dataGridView1.SelectedRows[0].Cells[0].Value;
            if (DialogResult.OK == MessageBox.Show(string.Format("{0} stok kodlu ürün 'elde_olanlardan silinecek. Onaylıyor musunuz?", stokKodu), "Silme onayı", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
            {
                MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
                con.Open();
                MySqlCommand cmd = new MySqlCommand(string.Format("delete from elde_olanlar where stok_kodu='{0}'", stokKodu), con);
                cmd.ExecuteNonQuery();
                elde_olanları_getir(con);
                con.Close();
                MessageBox.Show("Ürün 'elde_olanlar' dan silindi", "Başarılı..", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            //con.Open();
            //string sku = (string)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
            //MySqlCommand cmd = new MySqlCommand(string.Format("select stok_kodu, label, price, currency_abbr, stok_amount from elde_olanlar where stok_kodu='{0}'", sku), con);
            //eldeolanduzenleme f = new eldeolanduzenleme();
            //MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            //DataTable dt = new DataTable();
            //da.Fill(dt);
            //f.dataGridView1.DataSource = dt;
            //f.dataGridView1.Columns[0].ReadOnly = true;
            //f.dataGridView1.Columns[1].ReadOnly = true;
            //f.dataGridView1.Columns[3].ReadOnly = true;
            //if (f.ShowDialog() == DialogResult.OK)
            //{
            //    cmd.CommandText = string.Format("update elde_olanlar set price={0}, stok_amount={1} where stok_kodu='{2}'", f.dataGridView1.Rows[0].Cells[2].Value.ToString().Replace(',', '.'), (int)f.dataGridView1.Rows[0].Cells[4].Value, sku);
            //    cmd.ExecuteNonQuery();
            //    elde_olanları_getir(con);
            //    for (int i = 0; i < dataGridView1.Rows.Count; i++)
            //        if ((string)dataGridView1.Rows[i].Cells[0].Value == sku)
            //        {
            //            dataGridView1.Rows[i].Selected = true;
            //            break;
            //        }
            //}
            //con.Close();
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                //dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = price;
                MessageBox.Show("Girilen değer hatalı ! Sayısal değer girin. Ondalıklı sayılar için '.' (nokta) yerine ',' (virgül) kullanın.");
            }
            else if (e.ColumnIndex == 4) MessageBox.Show("Girilen değer hatalı ! Tamsayı değer girin.");
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            string sku = (string)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
            MySqlCommand cmd = new MySqlCommand(string.Format("update elde_olanlar set price={0}, stok_amount={1} where stok_kodu='{2}'", dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString().Replace(',', '.'), (int)dataGridView1.Rows[e.RowIndex].Cells[4].Value, sku), con);
            cmd.ExecuteNonQuery();
            //elde_olanları_getir(con);
            //for (int i = 0; i < dataGridView1.Rows.Count; i++)
            //    if ((string)dataGridView1.Rows[i].Cells[0].Value == sku)
            //    {
            //        dataGridView1.Rows[i].Selected = true;
            //        break;
            //    }
            con.Close();
        }
    }
}
