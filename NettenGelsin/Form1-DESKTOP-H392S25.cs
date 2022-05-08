using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using MySql.Data.MySqlClient;
using RestSharp;
using Microsoft.VisualBasic;

namespace NettenGelsin
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            veritabanı.Başla();
            TokenKeySınıfı.bilgilendirme = tokenBilgilendirme;
            TokenKeySınıfı.Başla();
            IdeaSoftVeritabanı.BAŞLA();
            firmalar.BAŞLA();
            markaToolStripMenuItem.DropDownItems.AddRange(IdeaSoftVeritabanı.brandListesi(new string[] { "AYD", "TEKNOROT" }).AsParallel().ToArray());
            log.Başla();
            log.Yaz("PROGRAM AÇILDI..");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            program.Kapat();
        }

        private void tokenBilgilendirme_TextChanged(object sender, EventArgs e)
        {
            Color renk;
            if (tokenBilgilendirme.Text.StartsWith("Durum: Aktif")) renk = Color.MediumSeaGreen;
            else if (tokenBilgilendirme.Text.StartsWith("Durum: Güncelleniyor")) renk = Color.Orange;
            else renk = Color.Red;
            toolStripStatusLabel1.BackColor = tokenBilgilendirme.ForeColor = renk;
        }

        Thread thread_ideaSoftTümVerileriÇekme;

        void ideaSoftTümVerileriÇekme()
        {
            DateTime başlama = DateTime.Now;

            IdeaSoftVeritabanı.product_image.verileriÇek();
            IdeaSoftVeritabanı.product_image.T_veriÇekme.Join();

            IdeaSoftVeritabanı.brand.verileriÇek();
            IdeaSoftVeritabanı.brand.T_veriÇekme.Join();

            IdeaSoftVeritabanı.currency.verileriÇek();
            IdeaSoftVeritabanı.currency.T_veriÇekme.Join();

            //button5.BackColor = Color.Yellow;
            //ideaSoftVeritabanı.price.verileriÇek();
            //ideaSoftVeritabanı.price.T_veriÇekme.Join();
            //button5.BackColor = Color.GreenYellow;

            IdeaSoftVeritabanı.details.verileriÇek();
            IdeaSoftVeritabanı.details.T_veriÇekme.Join();

            IdeaSoftVeritabanı.category.verileriÇek();
            IdeaSoftVeritabanı.category.T_veriÇekme.Join();

            IdeaSoftVeritabanı.product_to_category.verileriÇek();
            IdeaSoftVeritabanı.product_to_category.T_veriÇekme.Join();


            IdeaSoftVeritabanı.purchase_limitations.verileriÇek();
            IdeaSoftVeritabanı.purchase_limitations.T_veriÇekme.Join();

            IdeaSoftVeritabanı.purchase_limitation_items.verileriÇek();
            IdeaSoftVeritabanı.purchase_limitation_items.T_veriÇekme.Join();

            IdeaSoftVeritabanı.product.verileriÇek();
            IdeaSoftVeritabanı.product.T_veriÇekme.Join();

            //button13.BackColor = Color.Yellow;
            IdeaSoftVeritabanı.tags.verileriÇek();
            IdeaSoftVeritabanı.tags.T_veriÇekme.Join();
            //button13.BackColor = Color.GreenYellow;

            //button14.BackColor = Color.Yellow;
            IdeaSoftVeritabanı.product_to_tags.verileriÇek();
            IdeaSoftVeritabanı.product_to_tags.T_veriÇekme.Join();
            //button14.BackColor = Color.GreenYellow;

            TimeSpan geçen = DateTime.Now.Subtract(başlama);
            log.Yaz("ideasofttan veri çekme işlemleri bitti. İşlem süresi: " + (geçen.Days == 0 ? "" : geçen.Days.ToString() + " gün ") + (geçen.Hours == 0 ? "" : geçen.Hours.ToString() + " saat ") + (geçen.Minutes == 0 ? "" : geçen.Minutes.ToString() + " dakika ") + (geçen.Seconds == 0 ? "" : geçen.Seconds.ToString() + " saniye"));
        }

        private void button21_Click(object sender, EventArgs e)
        {
            ////string[] s = { "ABA KF000138", "ABA 8PK1614", "ABA 8PK1475", "ABA 7PK2060", "ABA 7PK1793", "ABA 7PK1515", "ABA 7PK1148", "ABA 6PK985" };
            //MySqlConnection con1 = new MySqlConnection(veritabanı.connectionString);
            //MySqlConnection con2 = new MySqlConnection(veritabanı.connectionString);
            //con1.Open(); con2.Open();
            //MySqlCommand cmd = new MySqlCommand("SELECT ortak.*,mulahaza.durum,products.id FROM (ortak left join products on ortak.stok_kodu=products.sku) inner JOIN mulahaza on mulahaza.sku=ortak.stok_kodu  WHERE ortak.stok_kodu='180190112'", con1);
            //MySqlDataReader dr = cmd.ExecuteReader();
            //while (dr.Read())
            //{
            //    object[] ürünBilgileri=new object[20];
            //    dr.GetValues(ürünBilgileri);
            //    Entegrasyon.ürünEkleGüncelle(ürünBilgileri, con2);
            //}
            //dr.Close();
            ////MySqlDataReader dr = cmd.ExecuteReader();
            ////string sku = "";
            ////int i = 1;
            ////listBox1.Items.Clear();
            ////while (i<=s.Length)
            ////{
            ////    sku = s[i - 1];
            ////        //dr.GetString(0);
            ////    listBox1.Items.Add(i.ToString() + ". " + sku + " ....");
            ////    listBox1.SelectedIndex = i - 1;
            ////    if (Entegrasyon.ürünSil(sku,con2)) listBox1.Items[i - 1] = i.ToString() + ". " + sku + " ..OK.."; 
            ////    else listBox1.Items[i - 1] = i.ToString() + ". " + sku + " ---ERROR---";
            ////    i++;
            ////}
            ////MessageBox.Show(Entegrasyon.datas.Length.ToString());
            //////dr.Close();

            //////object[] x = Entegrasyon.kategoriIDGetir("BİNEK 1.GRUP>DenemeÜst>Deneme>DenemeAlt", con);

            //////firmalar.Motoraşin.ortağaAktar(con);

            ////firmalar.Dinamik.verilerÇekildiktenSonraYapılacaklar(con);
            ////log.Yaz("Dinamik\tİşlem BİTTİ.");
            ////MessageBox.Show("Dinamik İşlem Bitti. çoklu kayıtlar siliniyor.");
            ////classFirma.multiRecordControlOrtak(con);
            ////log.Yaz("Motorasin Mulahaza");
            ////firmalar.Motoraşin.mulahazaEt(con);
            ////log.Yaz("Dinamik Mulahaza");
            ////firmalar.Dinamik.mulahazaEt(con);
            ////log.Yaz("İdeaSoft Mulahaza.");
            ////classFirma.mulahazaEt_ideaSoft(con);
            ////log.Yaz("Mulahaza BİTTİ.");
            ////MessageBox.Show("İşlem Bitti");
            //con1.Close(); con2.Close();
        }

        private void button24_Click(object sender, EventArgs e)
        {
            //MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            //con.Open();
            //int ideasoftKayıtSayısı = IdeaSoftVeritabanı.ideasoftKayıtSayısı(con);
            //int ideasofttanSilinecekler_OrtaktaOlmayanlar = IdeaSoftVeritabanı.ideasofttanSilinecekler_OrtaktaOlmayanlar(con);
            //int ideasofttanSilinecekler_StokMiktarıSıfırOlanlar = IdeaSoftVeritabanı.ideasofttanSilinecekler_StokMiktarıSıfırOlanlar(con);
            //int ideasofttanSilinecekler_FiyatDüşükOlanlar = IdeaSoftVeritabanı.ideasofttanSilinecekler_FiyatDüşükOlanlar(con);
            //int eklenecekler = IdeaSoftVeritabanı.ideasoftaEklenecekler_StokMiktarıSıfırDanBüyük_ve_FiyatıDüşükOlmayanlar(con);
            //int sonuç = ideasoftKayıtSayısı - ideasofttanSilinecekler_FiyatDüşükOlanlar - ideasofttanSilinecekler_OrtaktaOlmayanlar - ideasofttanSilinecekler_StokMiktarıSıfırOlanlar + eklenecekler;
            //label20.Text = string.Format("IDEASOFT KAYIT SAYISI : {0}\n\nSilinecekler :\nOrtakta olmayanlar  :{1}\nStoğu sıfır olanlar : {2}\nFiyatı Düşük Olanlar : {3}\nSİLİNECEKLER TOPLAMI :{4}\n\nEKLENECEK(Stokta olan ve Fiyatı düşük olmayan) : {5}\n\n\nSON DURUMDA KAYIT SAYISI :{6}", ideasoftKayıtSayısı, ideasofttanSilinecekler_OrtaktaOlmayanlar, ideasofttanSilinecekler_StokMiktarıSıfırOlanlar, ideasofttanSilinecekler_FiyatDüşükOlanlar, (ideasofttanSilinecekler_OrtaktaOlmayanlar + ideasofttanSilinecekler_StokMiktarıSıfırOlanlar + ideasofttanSilinecekler_FiyatDüşükOlanlar), eklenecekler, sonuç);
            //con.Close();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            //IdeaSoftVeritabanı.katsayı = (int)numericUpDown1.Value;
        }

        private void verilerÇekildiktenSonraTıklaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            log.Yaz("Silinecek ürünler tespit ediliyor.");
            classFirma.mulahazaEt();
            log.Yaz("Silinecek ürünler tesbit edildi.");
        }

        private void ideaSoftVerileriniÇekToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void ideasoftTablosunuOluşturToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IdeaSoftVeritabanı.ideaSoft_TablosunuOluştur();
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    if (Entegrasyon.Thread_silmeİşlemi != null) if (Entegrasyon.Thread_silmeİşlemi.IsAlive) return;
        //    float usd=0;
        //    float eur=0;
        //    string sql = "";
        //    if (radioButton1.Checked) sql = "select count(*) from mulahaza where durum='X'";
        //    else if (radioButton2.Checked) sql = "select count(*) from ideasoft where stok_amount=0";
        //    else if (radioButton3.Checked) sql = "";
        //    else if (radioButton5.Checked)
        //    {
        //        usd = Entegrasyon.kurNedir("USD");
        //        eur = Entegrasyon.kurNedir("EUR");
        //        sql = string.Format("select count(*) from ideasoft where brand not in ('AYD','TEKNOROT') and TL_Fiyat_Ver(currency_abbr,price,{0},{1})*1.18<25", usd.ToString().Replace(',','.'),eur.ToString().Replace(',', '.'));
        //    }
        //    else return;
        //    MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
        //    con.Open();
        //    MySqlCommand cmd = new MySqlCommand(sql, con);
        //    MySqlDataReader dr = cmd.ExecuteReader();
        //    int adet = 0;
        //    if (dr.Read()) adet = dr.GetInt32(0);
        //    dr.Close();
        //    con.Close();
        //    if (adet == 0)
        //    {
        //        MessageBox.Show("Şarta uygun hiç kayıt yok.Hiçbir silme işlemi yapılmayacak.", "Silme İşlemi");
        //        return;
        //    }
        //    if (DialogResult.No == MessageBox.Show("Şarta uyan " + adet.ToString() + " kayıt var. Bu kayıtlar silinecek. Devam edilsin mi?","Silme İşlemi",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)) return;

        //    if (radioButton1.Checked) sql = "select sku from mulahaza where durum='X'";
        //    else if (radioButton2.Checked) sql = "select stok_kodu from ideasoft where stok_amount=0";
        //    else if (radioButton3.Checked) sql = "";
        //    else if (radioButton5.Checked)
        //    {
        //        Thread_Apiİşlemleri.işEkle(Entegrasyon.Thread_silmeİşlemi = new Thread(() => Entegrasyon.ürünleriSil(new string[] { "AYD", "TEKNOROT" }, 25f)));
        //        Entegrasyon.Thread_silmeİşlemi.Start();
        //        return;
        //    }
        //    Thread_Apiİşlemleri.işEkle(Entegrasyon.Thread_silmeİşlemi = new Thread(() => Entegrasyon.ürünleriSil(sql,!radioButton3.Checked,true)));
        //    Entegrasyon.Thread_silmeİşlemi.Start();
        //}

        private void uSDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Entegrasyon.kurNedir("USD").ToString());
        }

        private void eURToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Entegrasyon.kurNedir("EUR").ToString());
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            float x;
            if (e.KeyChar == (char)Keys.Back) return;
            e.Handled = !float.TryParse(toolStripTextBox1.Text + e.KeyChar, out x);
        }

        public static bool brandMenusündenSeçildi = false;
        private void markaToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            brandMenusündenSeçildi = false;
        }

        private void verileriÇekToolStripMenuItem_Click(object sender, EventArgs e)
        {
            firmalar.Motoraşin.verileriÇek();
        }

        private void mulahazaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //firmalar.Motoraşin.mulahazaEt();
        }

        private void verileriÇekToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            firmalar.Dinamik.verileriÇek();
        }

        private void mulahazaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //firmalar.Dinamik.mulahazaEt();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //if (tEkleme != null)
            //    if (tEkleme.IsAlive)
            //    {
            //        MessageBox.Show("Devam eden bir ekleme işlemi var. Onun bitmesini bekleyiniz..", "Ekleme İşlemi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }
            //MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            //con.Open();
            //MySqlCommand cmd = new MySqlCommand(string.Format("SELECT COUNT(*) FROM mulahaza inner JOIN ortak on mulahaza.sku=ortak.stok_kodu where mulahaza.durum = 'E' AND ortak.stok_amount > 0 AND (TL_Fiyat_Ver(ortak.currency_abbr, ortak.price, {0}, {1}) * 1.18 >= 25 OR ortak.brand IN('AYD', 'TEKNOROT'))", Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.')), con);
            //MySqlDataReader dr = cmd.ExecuteReader();
            //int adet = 0;
            //if (dr.Read()) adet = dr.GetInt32(0);
            //dr.Close();
            //con.Close();
            //groupBox4.Enabled = adet > 0;
            //numericUpDown3.Maximum = adet;
            //numericUpDown3.Value = numericUpDown3.Maximum;
            //if (adet == 0) MessageBox.Show("Eklenebilecek hiç kayıt yok.", "Ekleme İşlemi");
            //else MessageBox.Show("Eklenebilecek " + adet.ToString() + " kayıt var.", "Ekleme İşlemi");

        }

        private void mulahazaToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //if (IdeaSoftVeritabanı.Thread_ideaSoft_oluşturma != null)
            //    if (IdeaSoftVeritabanı.Thread_ideaSoft_oluşturma.IsAlive)
            //    {
            //        MessageBox.Show("Çalışan bir İdeasoft tablosu oluşturma işlemi var.");
            //        return;
            //    }
            //log.Yaz("İdeasoft tablosu oluşturuluyor.");
            //IdeaSoftVeritabanı.ideaSoftTablosunuOluştur();
            //log.Yaz("Motorasin mulahaza ediliyor");
            //firmalar.Motoraşin.mulahazaEt();
            //log.Yaz("Dinamik mulahaza ediliyor");
            //firmalar.Dinamik.mulahazaEt();
            //log.Yaz("İdeasoft mulahaza ediliyor");
            //classFirma.mulahazaEt();
            //log.Yaz("İşlem BİTTİ.");

        }

        private void button5_Click(object sender, EventArgs e)
        {

            ////List<string> l = new List<string>();
            ////string[] satırlar = File.ReadAllLines("kategoriID_Liste.txt");
            ////foreach (string eleman in satırlar)
            ////{
            ////    IRestResponse data;
            ////    log.Yaz(eleman + " numaralı kategori siliniyor.",false,false);
            ////    int bekleme = Thread_Apiİşlemleri.bekleme;
            ////    while (true)
            ////    {
            ////        data = Entegrasyon.DELETE("categories", "77939");
            ////        if (data.IsSuccessful || bekleme > 10000) break;
            ////        log.Yaz(bekleme.ToString() + " msn. bekleniyor.",false,false);
            ////        Thread.Sleep(bekleme);
            ////        bekleme *= 3;
            ////    }
            ////    log.Yaz(eleman + (data.Content == "" ? " Silindi." : " SİLİNEMEDİ"),false,false);
            ////    richTextBox1.Refresh();
            ////}
            //MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            //con.Open();
            //firmalar.Motoraşin.verilerÇekildiktenSonraYapılacaklar(con);
            //firmalar.Dinamik.verilerÇekildiktenSonraYapılacaklar(con);
            //con.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //if (Entegrasyon.Thread_eklemeGüncellemeİşlemi != null)
            //    if (Entegrasyon.Thread_eklemeGüncellemeİşlemi.IsAlive)
            //    {
            //        MessageBox.Show("Devam eden bir ekleme/güncelleme işlemi var. Onun bitmesini bekleyiniz..", "Ekleme İşlemi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }
            //string işlemGöreceklerlerin_Stok_kodu_Listesi = veritabanı.işlemGöreceklerListesi((int)numericUpDown4.Value, Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.'));

            ////Entegrasyon.ürünleriSil("select stok_kodu from ortak where stok_kodu not in " + işlemGöreceklerlerin_Stok_kodu_Listesi, true, true);

            //Thread_Apiİşlemleri.işEkle(Entegrasyon.Thread_eklemeGüncellemeİşlemi = new Thread(() => Entegrasyon.ürünleriEkle(işlemGöreceklerlerin_Stok_kodu_Listesi)));
            //Entegrasyon.Thread_eklemeGüncellemeİşlemi.Start();
        }

        private void button7_MouseDown(object sender, MouseEventArgs e)
        {
        }

        private void button7_MouseUp(object sender, MouseEventArgs e)
        {
        }

        private void button8_Click(object sender, EventArgs e)
        {
        }

        Thread geceİşlem;

        Thread timer_işlemi;

        void deveran()
        {
            int geceSaat = 19;
            int geceDakika = 45;
            while (true)
            {

            }
        }

        //private void günlükİşleminiYapToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (geceİşlem != null) if (geceİşlem.IsAlive) { MessageBox.Show("Bu işlem zaten yapılıyor.."); return; }
        //    geceİşlem = new Thread(() => geceYapılacak(150000));
        //    geceİşlem.Start();
        //}

        string istatistikVer(string işlemGöreceklerListesi)
        {
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            con.Open();
            cmd.CommandTimeout = 300;
            MySqlDataReader dr;
            int adet1 = 0;
            int adet2 = 0;
            int adet3 = 0;
            int adet4 = 0;

            cmd.CommandText = "select count(sku) from mulahaza where mulahaza.durum='X'"; ;
            dr = cmd.ExecuteReader();
            if (dr.Read()) adet1 = dr.GetInt32(0);
            dr.Close();
            Thread.Sleep(50);

            cmd.CommandText = "select count(sku) from mulahaza where mulahaza.durum!='E' AND sku not in " + işlemGöreceklerListesi;
            dr = cmd.ExecuteReader();
            if (dr.Read()) adet2 = dr.GetInt32(0);
            dr.Close();
            Thread.Sleep(50);

            cmd.CommandText = "select count(sku) from mulahaza where mulahaza.durum='E' AND sku in " + işlemGöreceklerListesi;
            dr = cmd.ExecuteReader();
            if (dr.Read()) adet3 = dr.GetInt32(0);
            dr.Close();
            Thread.Sleep(50);

            cmd.CommandText = "select count(sku) from mulahaza where mulahaza.durum!='E' and mulahaza.durum!='Y' AND sku in " + işlemGöreceklerListesi;
            dr = cmd.ExecuteReader();
            if (dr.Read()) adet4 = dr.GetInt32(0);
            dr.Close();
            Thread.Sleep(50);

            con.Close();
            return (adet1 > 0 ? "Ortakta olmayıpta SİLİNECKELER : " + adet1.ToString() + "\n" : "") + (adet2 > 0 ? "Fiyat sıralamasına giremediğinden SİLİNECEKLER : " + adet2.ToString() + "\n" : "") + ("EKLENECEK kayıt sayısı : " + adet3.ToString() + "\n") + ("GÜNCELLENECEK kayıt sayısı : " + adet4.ToString() + "\n");
        }

        //void geceYapılacak(int kayıtSayısı)
        //{
        //    DateTime t;
        //    //ideasoft tablosu oluşturuluyor
        //    IdeaSoftVeritabanı.ideaSoftTablosunuOluştur();
        //    Thread.Sleep(10000);

        //    //saat 20:05 e kadar bekleniyor
        //    t = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 20, 05, 0);
        //    while (DateTime.Now < t) Thread.Sleep(60000);

        //    //motorasin verileri çekiliyor ve mülahaza ediliyor
        //    email.send("motorasin verileri çekiliyor..", "");
        //    firmalar.Motoraşin.veriÇekmeMotoru(new object[] { firmalar.Motoraşin.stringBuilderİfadesi, firmalar.Motoraşin.anahtarlar });
        //    email.send("motorasin verileri çekildi.", veritabanı.kayıtSayısı("motorasin").ToString() + " ürün çekildi.");
        //    Thread.Sleep(10000);
        //    veritabanı.resimKontrol("motorasin");
        //    Thread.Sleep(10000);



        //    //saat 21:05 e kadar bekleniyor.

        //    DateTime t1 = new DateTime(10, 10, 10, 21, 05, 0);
        //    DateTime t2 = new DateTime(10, 10, 10, 6, 0, 0);
        //    t = new DateTime(10, 10, 10, DateTime.Now.Hour, DateTime.Now.Minute, 0);
        //    while (t < t1 && t > t2)
        //    {
        //        Thread.Sleep(60000);
        //        t = new DateTime(10, 10, 10, DateTime.Now.Hour, DateTime.Now.Minute, 0);
        //    }

        //    //Dinamik verileri çekiliyor ve mülahaza ediliyor
        //    email.send("dinamik verileri çekiliyor..", "");
        //    firmalar.Dinamik.veriÇekmeMotoru(new object[] { firmalar.Dinamik.stringBuilderİfadesi, firmalar.Dinamik.anahtarlar });
        //    email.send("dinamik verileri çekildi.", veritabanı.kayıtSayısı("dinamik").ToString() + " ürün çekildi.");
        //    Thread.Sleep(10000);
        //    veritabanı.resimKontrol("dinamik");
        //    Thread.Sleep(10000);

        //    MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
        //    con.Open();
        //    MySqlCommand cmd = new MySqlCommand();
        //    cmd.CommandTimeout = 600;

        //    cmd = new MySqlCommand("call degistir('ortak');", con);
        //    cmd.CommandTimeout = 600;
        //    veritabanı.cmdExecute(cmd);
        //    con.Close();
        //    //veritabanı.degisim("ortak");
        //    //veritabanı.binekDüzeltme("motorasin");
        //    //veritabanı.binekDüzeltme("dinamik");

        //    //firmalar.Motoraşin.mulahazaEt();
        //    //Thread.Sleep(10000);
        //    //firmalar.Dinamik.mulahazaEt();
        //    //Thread.Sleep(10000);

        //    //mülahaza ediliyor(ideasoft)
        //    classFirma.mulahazaEt();
        //    Thread.Sleep(10000);

        //    if (!TokenKeySınıfı.aktif())
        //    {
        //        Thread.Sleep(60000);
        //        bool gönderildi = false;
        //        while (!TokenKeySınıfı.aktif())
        //        {
        //            if (!gönderildi)
        //            {
        //                log.Yaz(TokenKeySınıfı.refreshable() ? "Token Key tazelenemedi. Bir sıkıntı olabilir. Kontrol ediniz." : "Token key tazaleme süresi dolmuş. Tazelenemiyor.", "Hata");
        //                email.send("Token Key", TokenKeySınıfı.refreshable() ? "Token Key tazelenemedi. Bir sıkıntı olabilir. Kontrol ediniz." : "Token key tazaleme süresi dolmuş. Tazelenemiyor.");
        //                gönderildi = true;
        //            }
        //            Thread.Sleep(60000);
        //        }
        //        log.Yaz("Token Key yenilendi. İşlem devam edecek.");
        //    }

        //    //işlem görecekler belirleniyor

        //    string işlemGöreceklerlerin_Stok_kodu_Listesi = veritabanı.işlemGöreceklerListesi(kayıtSayısı, Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.'));

        //    string s_ = "";
        //    try
        //    {
        //        s_ = istatistikVer(işlemGöreceklerlerin_Stok_kodu_Listesi);
        //    }
        //    catch (Exception hata)
        //    {
        //        s_ = "istatistik bilgisi alınırken hata oluştu.\n\nHata mesajı:\n" + hata.Message;
        //    }
        //    email.send("nettengelsin veri ekleme/güncelleme yapılıyor.", s_);
        //    Thread işlem;

        //    //mulahazada durum X olanlar siliniyor
        //    Thread_Apiİşlemleri.işEkle(işlem = new Thread(() => Entegrasyon.ürünleriSil("select sku from mulahaza where mulahaza.durum='X'", true, true)));
        //    işlem.Start();
        //    işlem.Join();
        //    Thread.Sleep(10000);

        //    //fiyat kısıtlamasına takılanlar siliniyor.
        //    Thread_Apiİşlemleri.işEkle(işlem = new Thread(() => Entegrasyon.ürünleriSil("select sku from mulahaza where mulahaza.durum!='E' AND sku not in " + işlemGöreceklerlerin_Stok_kodu_Listesi, true, true)));
        //    işlem.Start();
        //    işlem.Join();
        //    Thread.Sleep(10000);

        //    //ürünlerin ekleme/güncelleme işlemi yapılıyor.
        //    Thread_Apiİşlemleri.işEkle(işlem = new Thread(() => Entegrasyon.ürünleriEkle(işlemGöreceklerlerin_Stok_kodu_Listesi)));
        //    işlem.Start();
        //    işlem.Join();

        //    //MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
        //    con.Open();
        //    cmd = new MySqlCommand("select mulahaza.sku,products.id,resimler.resim_base64 from (mulahaza inner join products on mulahaza.sku=products.sku) inner join resimler on mulahaza.sku=resimler.sku where mulahaza.sku " + (işlemGöreceklerlerin_Stok_kodu_Listesi.IndexOf(',') > -1 ? "in " : "=") + işlemGöreceklerlerin_Stok_kodu_Listesi + " and mulahaza.durum like '%I%'", con);
        //    MySqlDataReader dr = cmd.ExecuteReader();
        //    List<resimTipi_> resimEklenecekler = new List<resimTipi_>();
        //    while (true)
        //    {
        //        try
        //        {
        //            if (dr.Read())
        //            {
        //                resimEklenecekler.Add(new resimTipi_(dr.GetString(0), dr.GetInt32(1), dr.GetString(2)));
        //            }
        //            else break;
        //        }
        //        catch { break; }

        //    }
        //    dr.Close();
        //    email.send("Veri ekleme/güncelleme bitti.", "nettengelsin veri ekleme/güncelleme BİTTİ." + (resimEklenecekler.Count > 0 ? "Resmi olmayan " + resimEklenecekler.Count.ToString() + " ürünün resim ekleme işlemi başlıyor." : ""));
        //    if (resimEklenecekler.Count > 0)
        //    {
        //        Thread_Apiİşlemleri.işEkle(işlem = new Thread(() => Entegrasyon.resimleriEkle(resimEklenecekler)));
        //        işlem.Start();
        //        işlem.Join();
        //    }
        //}


        private void button3_Click(object sender, EventArgs e)
        {
            //if (textBox1.Text == "")
            //{
            //    Thread_Apiİşlemleri.işEkle(tEkleme = new Thread(() => Entegrasyon.ürünleriEkle((int)numericUpDown3.Value, new string[] { "AYD", "TEKNOROT" }, 25f)));
            //    tEkleme.Start();
            //    groupBox4.Enabled = false;
            //}
            //else
            //{
            //    //Entegrasyon.ürünEkleGüncelle()

            //}
        }

        private void button9_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        //private void verileriGönderideasoftuGüncelleToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    verileriGönderideasoftuGüncelleToolStripMenuItem.Enabled = false;
        //    //işlem görecekler belirleniyor
        //    email.send("nettengelsin veri ekleme/güncelleme yapılıyor.", "");
        //    string işlemGöreceklerlerin_Stok_kodu_Listesi = veritabanı.işlemGöreceklerListesi(150000, Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.'));

        //    Thread işlem;

        //    //mulahazada durum X olanlar siliniyor
        //    Thread_Apiİşlemleri.işEkle(işlem = new Thread(() => Entegrasyon.ürünleriSil("select sku from mulahaza where mulahaza.durum='X'", true, true)));
        //    işlem.Start();
        //    işlem.Join();
        //    Thread.Sleep(10000);

        //    //fiyat kısıtlamasına takılanlar siliniyor.
        //    Thread_Apiİşlemleri.işEkle(işlem = new Thread(() => Entegrasyon.ürünleriSil("select sku from mulahaza where mulahaza.durum!='E' AND sku not in " + işlemGöreceklerlerin_Stok_kodu_Listesi, true, true)));
        //    işlem.Start();
        //    işlem.Join();
        //    Thread.Sleep(10000);

        //    //ürünlerin ekleme/güncelleme işlemi yapılıyor.
        //    Thread_Apiİşlemleri.işEkle(işlem = new Thread(() => Entegrasyon.ürünleriEkle(işlemGöreceklerlerin_Stok_kodu_Listesi)));
        //    işlem.Start();
        //    işlem.Join();
        //    email.send("nettengelsin veri ekleme/güncelleme BİTTİ.", "");
        //    verileriGönderideasoftuGüncelleToolStripMenuItem.Enabled = true;
        //}

        //private void toolStripMenuItem4_Click(object sender, EventArgs e)
        //{
        //    string s = Interaction.InputBox("Stok kodunu giriniz.: (Virgülle ayırarak çoklu giriş yapabilirsiniz)", (sender.Equals(ekleGüncelleToolStripMenuItem) ? "Ekle/Güncelle işlemi" : "Silme işlemi"));
        //    if (s == "") return;
        //    if (sender.Equals(ekleGüncelleToolStripMenuItem))
        //    {
        //        string[] ss = s.Split(',');
        //        s = "";
        //        foreach (string item in ss) s += (s == "" ? "" : ",") + "'" + item + "'";
        //        Entegrasyon.ürünleriEkle(s);
        //    }
        //    else
        //        Entegrasyon.ürünleriSil(s);
        //}

        private void resimİşlemleriToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resim r = new resim();
            r.ShowDialog();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            IdeaSoftVeritabanı.tags.verileriÇek();
            IdeaSoftVeritabanı.tags.T_veriÇekme.Join();
            IdeaSoftVeritabanı.product_to_tags.verileriÇek();
            IdeaSoftVeritabanı.product_to_tags.T_veriÇekme.Join();
            MessageBox.Show("İşlem Bitti");
            ////Thread işlem;
            //MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            //con.Open();
            ////MySqlCommand cmd = new MySqlCommand("select mulahaza.sku from mulahaza inner join products on products.sku=mulahaza.sku where mulahaza.durum like '%I%'", con);
            ////MySqlDataReader dr = cmd.ExecuteReader();
            ////List<string> resimEklenecekler = new List<string>();
            ////while (dr.Read()) resimEklenecekler.Add(dr.GetString(0));
            ////dr.Close();
            ////Thread_Apiİşlemleri.işEkle(işlem = new Thread(() => Entegrasyon.resimleriEkle(resimEklenecekler)));
            ////işlem.Start();
            ////işlem.Join();
            ////veritabanı.resimKontrol("motorasin");
            ////veritabanı.resimKontrol("dinamik");

            //DateTime t = DateTime.Now;
            //firmalar.Motoraşin.verilerÇekildiktenSonraYapılacaklar(con);
            //firmalar.Dinamik.verilerÇekildiktenSonraYapılacaklar(con);

            //MySqlCommand cmd = new MySqlCommand("call degistir('ortak');",con);
            //veritabanı.cmdExecute(cmd);
            //veritabanı.degisim("ortak");
            ////veritabanı.binekDüzeltme("motorasin");
            ////veritabanı.binekDüzeltme("dinamik");

            //MessageBox.Show("Bitti. Geçen süre: "+DateTime.Now.Subtract(t).TotalMinutes.ToString()+" dakika.");
        }

        //private void çokKayıtToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    string s = richTextBox1.Text;
        //    List<string> ss = new List<string>(s.Split(','));
        //    ss = ss.Select(r => string.Concat("'", r, "'")).ToList();
        //    s = string.Join(",", ss);
        //    ss.Clear();
        //    Entegrasyon.ürünleriEkle("(" + s + ")");
        //    s = "";
        //}

        private void toolStripStatusLabel1_MouseDown(object sender, MouseEventArgs e)
        {
            Clipboard.SetText(TokenKeySınıfı.access_token);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            IdeaSoftVeritabanı.category.verileriÇek();
            IdeaSoftVeritabanı.product_to_category.verileriÇek();
            MessageBox.Show("İşlem bitti");

            //listBox1.Items.Clear(); listBox2.Items.Clear();
            //IRestResponse data;
            //int bekleme;
            //progressBar1.Maximum = silinecekIDler.Length;
            //progressBar1.Value = 0;



            //foreach (int item in silinecekIDler)
            //{
            //    progressBar1.Value++;
            //    bekleme = Thread_Apiİşlemleri.bekleme;
            //    while (true)
            //    {
            //        data = Entegrasyon.DELETE("categories", item.ToString());
            //        if (data.IsSuccessful || bekleme > 3000) break;
            //        Thread.Sleep(bekleme);
            //        bekleme *= 3;
            //    }
            //    if (data.IsSuccessful) listBox2.Items.Add(item);
            //    else listBox1.Items.Add(item);

            //} 
        }

        private void değerleriKopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s = "";
            foreach (var item in listBox1.Items)
            {
                s += item.ToString() + "\n";
            }
            if (s != "")
            {
                Clipboard.SetText(s);
                MessageBox.Show("Değerler kopyalandı");
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            foreach (var item in groupBox1.Controls)
            {
                if (!item.GetType().Name.Equals("CheckBox")) continue;
                ((CheckBox)item).Checked = sender.Equals(button7);
            }
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);

            if (cb1.Checked)
            {
                label3.Text = "İdeasoft tablosu oluşturuluyor";
                label3.Refresh();
                IdeaSoftVeritabanı.ideaSoftTablosunuOluştur();
            }

            if (cb2.Checked)
            {
                label3.Text = "Motorasin verileri çekiliyor.";
                label3.Refresh();
                firmalar.Motoraşin.veriÇekmeMotoru(new object[] { firmalar.Motoraşin.stringBuilderİfadesi, firmalar.Motoraşin.anahtarlar });
                label3.Text = "Motorasin verileri çekildi. (" + veritabanı.kayıtSayısı("motorasin_ham").ToString() + " kayıt)";
                label3.Refresh();
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
                label3.Text = "Motorasin resimleri kontrol ediliyor.";
                label3.Refresh();
                veritabanı.resimKontrol("motorasin");
            }



            if (cb7.Checked)
            {
                label3.Text = "Dinamik verileri çekiliyor.";
                label3.Refresh();
                firmalar.Dinamik.veriÇekmeMotoru(new object[] { firmalar.Dinamik.stringBuilderİfadesi, firmalar.Dinamik.anahtarlar });
                label3.Text = "Dinamik verileri çekildi. (" + veritabanı.kayıtSayısı("dinamik_ham").ToString() + " kayıt)";
                label3.Refresh();
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
                label3.Text = "Dinamik resimleri kontrol ediliyor.";
                label3.Refresh();
                veritabanı.resimKontrol("dinamik");
            }



            if (cb12.Checked)
            {
                con.Open();
                label3.Text = "Ortak stoğu olmayan veya fiyatı 0 olanlar siliniyor.";
                label3.Refresh();
                MySqlCommand cmd = new MySqlCommand("DELETE FROM ortak WHERE ortak.stok_amount=0 OR ortak.price=0;", con);
                cmd.CommandTimeout = 600;
                veritabanı.cmdExecute(cmd);
                con.Close();
            }
            if (cb13.Checked)
            {
                con.Open();
                label3.Text = "Ortak aynı stok kodlu ürünlerden fiyatı en az olanı tutup diğerlerini siliyor. (Teke düşürüyor)";
                label3.Refresh();
                MySqlCommand cmd = new MySqlCommand(string.Format("DELETE t1 FROM ortak t1 INNER JOIN ortak t2 WHERE t1.stok_kodu = t2.stok_kodu and TL_Fiyat_Ver(t1.currency_abbr, t1.price, {0}, {1}) > TL_Fiyat_Ver(t2.currency_abbr, t2.price, {0}, {1}); DELETE t1 FROM ortak t1 INNER JOIN ortak t2 WHERE t1.stok_kodu = t2.stok_kodu and t1.id < t2.id; ", Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.')), con);
                cmd.CommandTimeout = 600;
                veritabanı.cmdExecute(cmd);
                con.Close();
            }
            if (cb14.Checked)
            {

                //burada  veritabanı.işlemGöreceklerListesi metodu sadece bu kayıtları silmek için kullanılıyor. metoda gerek olmadan burada o işlemler yapılacak.
                con.Open();
                label3.Text = "İşlem görecekler tesbit edilip diğerleri ortaktan siliniyor.";
                label3.Refresh();
                string işlemGöreceklerlerin_Stok_kodu_Listesi = veritabanı.işlemGöreceklerListesi(150000, Entegrasyon.kurNedir("USD").ToString().Replace(',', '.'), Entegrasyon.kurNedir("EUR").ToString().Replace(',', '.'));
                MySqlCommand cmd = new MySqlCommand(string.Format("DELETE FROM ortak WHERE ortak.stok_kodu {0} ", (işlemGöreceklerlerin_Stok_kodu_Listesi.IndexOf(',') > -1 ? "NOT IN " : "!=") + işlemGöreceklerlerin_Stok_kodu_Listesi), con);
                cmd.CommandTimeout = 600;
                veritabanı.cmdExecute(cmd);
                con.Close();
            }
            if (cb15.Checked)
            {
                con.Open();
                label3.Text = "Ortak değişim uygulanıyor.";
                label3.Refresh();
                MySqlCommand cmd = new MySqlCommand("call degistir('ortak');", con);
                cmd.CommandTimeout = 600;
                veritabanı.cmdExecute(cmd);
                con.Close();
            }
            if (cb16.Checked)
            {
                label3.Text = "Binek düzeltme yapılıyor.";
                label3.Refresh();
                veritabanı.binekDüzeltme();
            }
            if (cb17.Checked)
            {
                //label3.Text = "Mulahaza ediliyor.";
                //label3.Refresh();

                ////firmalar.Motoraşin.mulahazaEt();
                ////firmalar.Dinamik.mulahazaEt();
                //classFirma.mulahazaEt();
            }

            if (cb17.Checked)
            {
                label3.Text = "Olmayan kayıtlar siliniyor.";
                label3.Refresh();
                Entegrasyon.ürünleriSil("SELECT stok_kodu FROM ideasoft WHERE stok_kodu NOT IN (SELECT stok_kodu FROM ortak);", true, true, progressBar2, label3);
            }
            if (cb18.Checked)
            {
                label3.Text = "Ürünlerin ekleme işlemi yapılıyor.";
                label3.Refresh();
                Entegrasyon.ürünleriEkle(Entegrasyon.işlemTipi.eklenecek, progressBar2, label3);
            }
            if (cb19.Checked)
            {
                label3.Text = "ürünlerin güncelleme işlemi yapılıyor.";
                label3.Refresh();
                Entegrasyon.ürünleriEkle(Entegrasyon.işlemTipi.güncellenecek, progressBar2, label3);
            }
            if (cb20.Checked)
            {
                label3.Text = "Yayında olan ürün resimleri kontrol edilip ekleme/güncelleme yapılıyor.";
                label3.Refresh();
                con.Open();
                MySqlCommand cmd = new MySqlCommand("select ortak.stok_kodu,ideasoft.id,resimler.id from ortak left join ideasoft on (ortak.stok_kodu=ideasoft.stok_kodu) left join resimler on ortak.stok_kodu=resimler.sku", con);
                cmd.CommandTimeout = 600;
                MySqlDataReader dr = cmd.ExecuteReader();

                //Entegrasyon.resimleri_Ekle_Güncelle metodunda ürünler yayında mı diye kontrol ediliyor. halbuki burada gönderdiğimiz hepsi yayında!? Ekstra iş yapıyor.
                List<resimTipi_> resimler = new List<resimTipi_>();
                string sku,id
                while (dr.Read())
                {

                }
                dr.Close();
                Entegrasyon.resimleri_Ekle_Güncelle(resimler, progressBar2, label3);
            }
            ////MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            //MySqlCommand cmd = new MySqlCommand();
            //con.Open();
            //cmd = new MySqlCommand("select mulahaza.sku,products.id,resimler.resim_base64 from (mulahaza inner join products on mulahaza.sku=products.sku) inner join resimler on mulahaza.sku=resimler.sku where mulahaza.sku " + (işlemGöreceklerlerin_Stok_kodu_Listesi.IndexOf(',') > -1 ? "in " : "=") + işlemGöreceklerlerin_Stok_kodu_Listesi + " and mulahaza.durum like '%I%'", con);
            //MySqlDataReader dr = cmd.ExecuteReader();
            //List<resimTipi_> resimEklenecekler = new List<resimTipi_>();
            //while (true)
            //{
            //    try
            //    {
            //        if (dr.Read())
            //        {
            //            resimEklenecekler.Add(new resimTipi_(dr.GetString(0), dr.GetInt32(1), dr.GetString(2)));
            //        }
            //        else break;
            //    }
            //    catch { break; }

            //}
            //dr.Close();
            //email.send("Veri ekleme/güncelleme bitti.", "nettengelsin veri ekleme/güncelleme BİTTİ." + (resimEklenecekler.Count > 0 ? "Resmi olmayan " + resimEklenecekler.Count.ToString() + " ürünün resim ekleme işlemi başlıyor." : ""));
            //if (resimEklenecekler.Count > 0)
            //{
            //    Thread_Apiİşlemleri.işEkle(işlem = new Thread(() => Entegrasyon.resimleriEkle(resimEklenecekler)));
            //    işlem.Start();
            //    işlem.Join();
            //}


            label3.Text = "";
            MessageBox.Show("İşlemler Bitti");
        }

        private void progressBar2_VisibleChanged(object sender, EventArgs e)
        {

        }

        private void günlükİşleminiYapToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void veriÇekmeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ideasoftVeritabanınıÇekToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (thread_ideaSoftTümVerileriÇekme != null) if (thread_ideaSoftTümVerileriÇekme.IsAlive) return;
            thread_ideaSoftTümVerileriÇekme = new Thread(ideaSoftTümVerileriÇekme);
            thread_ideaSoftTümVerileriÇekme.Start();
        }
    }
}
