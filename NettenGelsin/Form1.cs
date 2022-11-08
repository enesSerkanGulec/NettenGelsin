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

            İşlem işlem = new İşlem(label3, progressBar2, listBox2, listBox1, numericUpDown2, radioButton1, con);

            if (cb2.Checked) işlem.Yap(İşlemler_Tipi.motoraşinVeriÇek);
            if (cb3.Checked) işlem.Yap(İşlemler_Tipi.motoraşinDeğişim);
            if (cb4.Checked) işlem.Yap(İşlemler_Tipi.motoraşinÇokluKayıtlarıSil);
            if (cb5.Checked) işlem.Yap(İşlemler_Tipi.motoraşinOrtağaAktar);
            if (cb6.Checked) işlem.Yap(İşlemler_Tipi.motoraşinResimleriİçeAktar);

            if (cb7.Checked) işlem.Yap(İşlemler_Tipi.dinamikVerileriÇek);
            if (cb8.Checked) işlem.Yap(İşlemler_Tipi.dinamikDeğişim);
            if (cb9.Checked) işlem.Yap(İşlemler_Tipi.dinamikÇokluKayıtlarıSil);
            if (cb10.Checked) işlem.Yap(İşlemler_Tipi.dinamikOrtağaAktar);
            if (cb11.Checked) işlem.Yap(İşlemler_Tipi.dinamikResimleriİçeAktar);

            if (cb12.Checked) işlem.Yap(İşlemler_Tipi.ortakTekrarlıKayıtlarınBilgileriniDinamiktekiGibiYap);
            if (cb13.Checked) işlem.Yap(İşlemler_Tipi.ortak_TablosunuOluştur);
            if (cb14.Checked) işlem.Yap(İşlemler_Tipi.ortakDeğişim);
            if (cb15.Checked) işlem.Yap(İşlemler_Tipi.ortakBinekDüzeltme);

            if (cb1.Checked) işlem.Yap(İşlemler_Tipi.ideaSoftTablosuOluştur);

            if (rbGüncelleme.Checked) //Sadece Güncelleme yapılıyor.
            {
                label3.Text = "Sadece güncelleme yapılıyor.";
                label3.Refresh();
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
                işlem.Yap(İşlemler_Tipi.ortaktaOlmayanKayıtlarSitedenSiliniyor);
            }
            //if (cb18.Checked)
            {
                işlem.Yap(İşlemler_Tipi.sitedeOlmayanKayıtlarEkleniyor);
            }
            //if (cb19.Checked)
            {
                işlem.Yap(İşlemler_Tipi.güncellemeYapılıyor);
            }
            if (cb20.Checked) işlem.Yap(İşlemler_Tipi.yayındakiÜrünlerdenResmiOlmayanlarınResmiGönderiliyor);
            
            label3.Text = "İŞLEMLER BİTTİ.";
            //MessageBox.Show("İşlemler Bitti");
            if (İşlem.mesaj != "")
            {
                İşlem.mesaj = "GÜNCELLEME İŞLEMİ RAPORU\n\n" + İşlem.mesaj;
                email.send("Günlük işlem", İşlem.mesaj);
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
            string deger = "";
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            while (true)
            {
                deger = Microsoft.VisualBasic.Interaction.InputBox("Elde olan ürünün stok kodu:", "Ekleme", deger);
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
