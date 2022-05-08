using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace NettenGelsin
{
    public partial class resim : Form
    {
        public resim()
        {
            InitializeComponent();
        }

        List<resimTipi> tab1 = new List<resimTipi>();
        List<resimTipi> tab2 = new List<resimTipi>();
        List<resimTipi> tab3 = new List<resimTipi>();

        void progresssBarGöster(object max)
        {
            progressBar1.Maximum = Convert.ToInt32(max);
            progressBar1.Value = 0;
            panel3.Left = (this.Width - panel3.Width) / 2;
            panel3.Top = (this.Height - panel3.Height) / 2;
            label4.Text = "";
            panel3.Visible = true;
        }

        private void motorasinResimleriniİçeAktarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tab1.Count > 0 || tab2.Count > 0)
            {
                if (DialogResult.No == MessageBox.Show("Başka bir içe aktarma işlemi görünüyor. Bu işlemi tamamladıysanız 'Evet' diyerek yeni bir içe aktarma işlemine geçebilirsiniz. Devam edilsin mi? (Önceki işlem tamamlandı kabul edilecek.", "Başka bir işlem devam ediyor..!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)) return;
            }
            string marka = sender.Equals(motorasinResimleriniİçeAktarToolStripMenuItem) ? "motorasin" : (sender.Equals(dinamikVerileriİçinKontrolEtToolStripMenuItem) ? "dinamik" : "");
            if (marka == "") return;
            if (DialogResult.Cancel == MessageBox.Show(marka + " den gelen verilerde resmi olanlar içe aktarılacak. Devam edilsin mi?", "Resim içe aktarma", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)) return;

            resimTipi.k1s.Clear(); resimTipi.k2s.Clear();
            tabControl2.TabPages.Clear();
            tab1.Clear(); tab2.Clear(); tab3.Clear();
            List<resimTipi> resimler = new List<resimTipi>();

            resimTipi.marka = marka;
            resimTipi.işlemGörenler.Clear();

            MySqlCommand cmd = new MySqlCommand();
            MySqlDataReader dr;
            cmd.Connection = new MySqlConnection(veritabanı.connectionString);
            cmd.Connection.Open();

            //cmd = new MySqlCommand("set net_write_timeout=99999; set net_read_timeout=99999", con);
            //cmd.ExecuteNonQuery();

            cmd.CommandText = marka == "motorasin" ? "select Count(*) from motorasin where Picture<>''" : "select Count(*) from dinamik where resim_url<>''";
            var adet = cmd.ExecuteScalar();

            progresssBarGöster(adet);

            int kalan = progressBar1.Maximum;
            int alt = 0;
            int paket = 5000;
            while (kalan > 0)
            {
                cmd.CommandText = (marka == "motorasin" ? "select Concat(Manufacturer,' ',ManufacturerCode) as sku, Picture from motorasin where Picture<>''" : "select stok_kodu, resim_url from dinamik where resim_url<>''") + string.Format(" limit {0},{1}", alt, paket);
                alt += paket;
                kalan -= paket;
                dr = cmd.ExecuteReader();
                resimTipi eklenecek;
                while (true)
                {
                    try
                    {
                        if (!dr.Read()) break;
                    }
                    catch (Exception hata)
                    {
                        MessageBox.Show("Bir hata oluştu. İşlem iptal ediliyor.\nHata Mesajı:\n" + hata.Message);
                        panel3.Visible = false;

                        return;
                    }
                    progressBar1.Value++;
                    label4.Text = marka + " verileri aktarılıyor.. (" + progressBar1.Value.ToString() + "/" + adet.ToString() + ")";
                    progressBar1.Refresh(); label4.Refresh();
                    eklenecek = new resimTipi(dr.GetString(0), dr.GetString(1));
                    if (eklenecek.durum == durumTipi.Eklenecek || eklenecek.durum == durumTipi.Güncellenecek)
                        resimler.Add(eklenecek);
                    //if (resimler.Count>=5000)
                    //{
                    //    MessageBox.Show("İşlemlerin kolay yapılabilmesi için işlenebilecek resim sayısı 5000 olarak ayarlanmıştır. Daha fazla resim var ise bu işlem bittikten sonra aynı işlemi tekrar yapınız.");
                    //    break;
                    //}
                }
                dr.Close();
            }
            cmd.Connection.Close();
            label4.Text = "İşlem bitiyor.."; label4.Refresh();
            int x = 0;
            if (resimler.Count == 0)
            {
                MessageBox.Show("Aktarılacak yeni resim yok");
                panel3.Visible = false;
                return;
            }
            foreach (resimTipi item in resimler)
            {
                if (item.durum == durumTipi.Hata) tab3.Add(item);
                else if (item.durum == durumTipi.Güncellenecek) tab2.Add(item);
                else if (item.durum == durumTipi.Eklenecek) tab1.Add(item);
                else x++;
            }
            resimler.Clear();
            string mesaj = "";
            if (tab1.Count > 0)
            {
                mesaj = "Eklenecek sayısı: " + tab1.Count.ToString();
                pictureBox1.BackgroundImage = null;
                label1.Text = marka + " (" + tab1.Count.ToString() + ")";
                checkedListBox1.Items.Clear();
                foreach (resimTipi item in tab1) checkedListBox1.Items.Add(item.sku);
                tabControl2.TabPages.Add(tabPage1);
            }

            if (tab2.Count > 0)
            {
                mesaj += (mesaj != "" ? "\n" : "") + "Güncellenecek sayısı: " + tab2.Count.ToString();
                pictureBox2.BackgroundImage = null;
                pictureBox3.BackgroundImage = null;
                label2.Text = marka + " (" + tab2.Count.ToString() + ")";
                checkedListBox2.Items.Clear();
                foreach (resimTipi item in tab2) checkedListBox2.Items.Add(item.sku);
                tabControl2.TabPages.Add(tabPage2);
            }

            if (tab3.Count > 0)
            {
                mesaj += (mesaj != "" ? "\n" : "") + "Hata oluşan sayısı: " + tab3.Count.ToString();
                pictureBox4.BackgroundImage = null;
                label3.Text = tab3.Count.ToString() + " adet resim yüklenemedi";
                listBox1.Items.Clear();
                foreach (resimTipi item in tab3) listBox1.Items.Add(item.sku);
                tabControl2.TabPages.Add(tabPage3);
            }

            if (x > 0) mesaj += (mesaj != "" ? "\n" : "") + "İşlem yapılmayacak(aynısı mevcut) sayısı: " + x.ToString();
            panel3.Visible = false;

            MessageBox.Show(mesaj);
        }

        private void dosyadanİçeAktarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tab1.Count > 0 || tab2.Count > 0)
            {
                if (DialogResult.No == MessageBox.Show("Başka bir içe aktarma işlemi görünüyor. Bu işlemi tamamladıysanız 'Evet' diyerek yeni bir içe aktarma işlemine geçebilirsiniz. Devam edilsin mi? (Önceki işlem tamamlandı kabul edilecek.", "Başka bir işlem devam ediyor..!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)) return;
            }
            resimTipi.k1s.Clear(); resimTipi.k2s.Clear();
            tabControl2.TabPages.Clear();
            tab1.Clear(); tab2.Clear(); tab3.Clear();
            List<resimTipi> resimler = new List<resimTipi>();
            FolderBrowserDialog f = new FolderBrowserDialog();
            f.Description = "İçe aktarılacak resim dosyalarının olduğu klasörü seçiniz. ([Klasör ismi + ' ' + Dosya isimi] stokKodu olarak kabul edilecek)";
            if (f.ShowDialog() != DialogResult.OK) return;

            DirectoryInfo d = new DirectoryInfo(f.SelectedPath);
            FileInfo[] jpg_ler = d.GetFiles("*.jpg");
            FileInfo[] png_ler = d.GetFiles("*.png");
            if ((jpg_ler.Length + png_ler.Length) == 0)
            {
                MessageBox.Show("Seçilen klasörde hiç resim dosyası yok", "Resim dosyası bulunamadı.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string mesaj = "";
            string k1, k2;
            int devam = 1;

            if (DialogResult.Yes == MessageBox.Show("Stok kodu içinde dosya ismi için uygun olmayan karakter olup da herhangi bir karakter değişimi yaptınız mı?", "Resim içe aktarma", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
            {
                do
                {
                    mesaj = mesaj == "" ? "Dosya isminde olamayan fakat stok kodu içinde olan bir karakter var mı? (Örneğin /  gibi..)\nVarsa bu karakteri girip 'Tamam' düğmesine basınız." : "Dosya isminde olamayan fakat stok kodu içinde olan başka bir karakter var mı? (Örneğin /  gibi..)\nVarsa bu karakteri girip 'Tamam' düğmesine basınız.";
                    do
                    {
                        k1 = Microsoft.VisualBasic.Interaction.InputBox(mesaj, "Uyumsuz karakter sorgusu");
                        if (k1 == "") { devam = 0; break; }
                        k1 = k1.Trim();
                        if (k1.Length == 1)
                        {
                            if (resimTipi.k1s.IndexOf(k1) == -1) { devam = 1; break; }
                            else { devam = -1; MessageBox.Show("Bu karakteri daha önce girdiniz.", "Hatalı giriş..", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                        }
                        else MessageBox.Show("Girilen değer 1 karakter olmalı !", "Hatalı giriş..", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    } while (true);
                    if (devam == 1)
                    {
                        do
                        {
                            k2 = Microsoft.VisualBasic.Interaction.InputBox(k1 + " karakteri yerine hangi karakteri kullandınız ?", "Uyumsuz karakter sorgusu");
                            if (k2 == "") { k1 = ""; devam = 0; break; }
                            k2 = k2.Trim();
                            if (k2.Length == 1)
                            {
                                if (resimTipi.k2s.IndexOf(k2) == -1) break;
                                else MessageBox.Show("Bu karakteri daha önce girdiniz.", "Hatalı giriş..", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else MessageBox.Show("Girilen değer 1 karakter olmalı !", "Hatalı giriş..", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        } while (true);
                        if (devam == 1) { resimTipi.k1s.Add(k1); resimTipi.k2s.Add(k2); }
                    }
                } while (devam != 0);
            }

            string marka = Microsoft.VisualBasic.Interaction.InputBox("Seçili klasördeki " + (jpg_ler.Length + png_ler.Length).ToString() + " resim dosyası içe aktarılacak. Bu ürünlerin markası nedir?", "Marka belirleme..", d.Name);
            if (marka == "")
            {
                MessageBox.Show("İşlem iptal edildi.", "İşlem iptal edildi.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            resimTipi.marka = marka;
            resimTipi.işlemGörenler.Clear();

            progresssBarGöster(jpg_ler.Length);
            //progressBar1.Maximum = jpg_ler.Length;
            //progressBar1.Value = 0;
            //panel3.Left = (this.Width - panel3.Width) / 2;
            //panel3.Top = (this.Height - panel3.Height) / 2;
            //label4.Text = "";
            //panel3.Visible = true;

            for (int i = 0; i < jpg_ler.Length; i++)
            {
                progressBar1.Value = i + 1;
                label4.Text = "jpeg resimleri aktarılıyor. (" + (i + 1).ToString() + "/" + jpg_ler.Length.ToString() + ")";
                progressBar1.Refresh(); label4.Refresh();
                resimler.Add(new resimTipi(marka, jpg_ler[i]));
            }
            progressBar1.Maximum = jpg_ler.Length;
            progressBar1.Value = 0;
            label4.Text = "";
            for (int i = 0; i < png_ler.Length; i++)
            {
                progressBar1.Value = i + 1;
                label4.Text = "png resimleri aktarılıyor. (" + (i + 1).ToString() + "/" + png_ler.Length.ToString() + ")";
                progressBar1.Refresh(); label4.Refresh();
                resimler.Add(new resimTipi(marka, png_ler[i]));
            }
            label4.Text = "İşlem bitiyor.."; label4.Refresh();
            int x = 0;
            foreach (resimTipi item in resimler)
            {
                if (item.durum == durumTipi.Hata) tab3.Add(item);
                else if (item.durum == durumTipi.Güncellenecek) tab2.Add(item);
                else if (item.durum == durumTipi.Eklenecek) tab1.Add(item);
                else x++;
            }
            resimler.Clear();
            mesaj = "";
            if (tab1.Count > 0)
            {
                mesaj = "Eklenecek sayısı: " + tab1.Count.ToString();
                pictureBox1.BackgroundImage = null;
                label1.Text = marka + " (" + tab1.Count.ToString() + ")";
                checkedListBox1.Items.Clear();
                foreach (resimTipi item in tab1) checkedListBox1.Items.Add(item.sku);
                tabControl2.TabPages.Add(tabPage1);
            }

            if (tab2.Count > 0)
            {
                mesaj += (mesaj != "" ? "\n" : "") + "Güncellenecek sayısı: " + tab2.Count.ToString();
                pictureBox2.BackgroundImage = null;
                pictureBox3.BackgroundImage = null;
                label2.Text = marka + " (" + tab2.Count.ToString() + ")";
                checkedListBox2.Items.Clear();
                foreach (resimTipi item in tab2) checkedListBox2.Items.Add(item.sku);
                tabControl2.TabPages.Add(tabPage2);
            }

            if (tab3.Count > 0)
            {
                mesaj += (mesaj != "" ? "\n" : "") + "Hata oluşan sayısı: " + tab3.Count.ToString();
                pictureBox4.BackgroundImage = null;
                label3.Text = tab3.Count.ToString() + " adet resim yüklenemedi";
                listBox1.Items.Clear();
                foreach (resimTipi item in tab3) listBox1.Items.Add(item.sku);
                tabControl2.TabPages.Add(tabPage3);
            }

            if (x > 0) mesaj += (mesaj != "" ? "\n" : "") + "İşlem yapılmayacak(aynısı mevcut) sayısı: " + x.ToString();
            panel3.Visible = false;
            MessageBox.Show(mesaj);
        }

        private void resim_Load(object sender, EventArgs e)
        {
            if (resimTipi.con == null) resimTipi.con = new MySqlConnection(veritabanı.connectionString);
            if (resimTipi.con.State != ConnectionState.Open) resimTipi.con.Open();
            //con = new MySqlConnection(veritabanı.connectionString);
            //Task t = con.OpenAsync();
            //t.Wait();
        }

        private void resim_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (resimTipi.con.State == ConnectionState.Open) resimTipi.con.Close();

            //Task t = con.CloseAsync();
            //t.Wait();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++) checkedListBox1.SetItemChecked(i, sender.Equals(button1));

        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox2.Items.Count; i++) checkedListBox2.SetItemChecked(i, sender.Equals(button5));
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex == -1) return;
            Image resim = veritabanı.Base64ToImage(tab1[checkedListBox1.SelectedIndex].yüklenecek_Base64);
            pictureBox1.BackgroundImage = resim ?? pictureBox1.ErrorImage;
        }

        private void checkedListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkedListBox2.SelectedIndex == -1) return;
            Image resim = veritabanı.Base64ToImage(tab2[checkedListBox2.SelectedIndex].mevcutResim_Base64);
            pictureBox2.BackgroundImage = resim ?? pictureBox2.ErrorImage;
            resim = veritabanı.Base64ToImage(tab2[checkedListBox2.SelectedIndex].yüklenecek_Base64);
            pictureBox3.BackgroundImage = resim ?? pictureBox3.ErrorImage;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CheckedListBox l = sender.Equals(button3) ? checkedListBox1 : checkedListBox2;
            if (l.CheckedItems.Count == 0) return;
            //string marka = sender.Equals(button3) ? tab1[0].marka : tab2[0].marka;
            progressBar1.Maximum = l.CheckedItems.Count;
            progressBar1.Value = 0;
            panel3.Left = (this.Width - panel3.Width) / 2;
            panel3.Top = (this.Height - panel3.Height) / 2;
            label4.Text = "";
            panel3.Visible = true;
            int x;
            for (int i = 0; i < l.CheckedItems.Count; i++)
            {
                label4.Text = (i + 1).ToString() + "/" + l.CheckedItems.Count.ToString();
                x = l.Items.IndexOf(l.CheckedItems[i]);
                progressBar1.Value = i + 1;
                progressBar1.Refresh(); label4.Refresh();
                if (sender.Equals(button3)) tab1[x].işlemYap(); else tab2[x].işlemYap();
            }
            label4.Text = "İşlem sonlanıyor.."; label4.Refresh();
            while (l.CheckedItems.Count > 0)
            {
                x = l.Items.IndexOf(l.CheckedItems[0]);
                l.Items.RemoveAt(x);
                if (sender.Equals(button3)) tab1.RemoveAt(x); else tab2.RemoveAt(x);

            }
            panel3.Visible = false;
            if (resimTipi.marka == "ideasoft") return;
            string mesaj = resimTipi.marka + ((resimTipi.marka == "motorasin" || resimTipi.marka == "dinamik") ? " den" : "markasına ait") + " içeri aktarılan veya güncellenen resimler siteye hemen gönderilsin mi?\n(Bu işlemi 'İçe aktarılacaklar' ve 'Güncelleme onayı bekleyenler' işlemlerinin en sonuncusu yapıldıktan sonra onaylamanız gerekir.";
            if (DialogResult.Yes == MessageBox.Show(mesaj, "İdeaSoft üzerine resimler eklensin mi?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                panel3.Visible = true;
                Entegrasyon.resimleri_Ekle_Güncelle(resimTipi.işlemGörenler, progressBar1, label4);
                panel3.Visible = false;

            }
        }

        private void resim_Resize(object sender, EventArgs e)
        {
            panel3.Left = (this.Width - panel3.Width) / 2;
            panel3.Top = (this.Height - panel3.Height) / 2;
            panel3.Refresh();
        }

        private void skularIKopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string s = "";
            List<resimTipi> l = tabControl2.SelectedTab.Equals(tabPage1) ? tab1 : tab2;
            foreach (resimTipi item in l)
            {
                s += (s == "" ? "" : ",") + "'" + item.sku + "'";
            }
            Clipboard.SetText("(" + s + ")");
        }

        private void resimleriTestEtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("select count(*) from resimler", con);
            cmd.CommandTimeout = 1000;
            MySqlDataReader dr = cmd.ExecuteReader();
            int adet = 0;
            if (dr.Read()) adet = dr.GetInt32(0);
            dr.Close();
            if (adet == 0) return;
            int kaçarlı = 10;
            int paket = adet / kaçarlı + (adet % kaçarlı == 0 ? 0 : 1);
            List<string> silinecekler = new List<string>();
            List<string> güncellenecekler = new List<string>();
            progresssBarGöster(paket);
            label4.Text = "Resimler kontrol ediliyor.";
            for (int i = 0; i < paket; i++)
            {
                progressBar1.Value++;
                label4.Text = "(" + progressBar1.Value.ToString() + "/" + progressBar1.Maximum.ToString() + ") Resimler kontrol ediliyor.";
                label4.Refresh();
                cmd.CommandText = string.Format("select sku,url,resim_base64 from resimler limit {0},{1}", i * kaçarlı, kaçarlı);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    try
                    {
                        Resim_işlemleri.Image_base64_Test(dr.GetString(2));
                        güncellenecekler.Add("('" + dr.GetString(0) + "','" + Resim_işlemleri.Image_Url_Test(dr.GetString(1)) + "')");
                    }
                    catch
                    {
                        silinecekler.Add("'" + dr.GetString(0) + "'");
                    }
                }
                dr.Close();
            }
            if (silinecekler.Count > 0)
                File.WriteAllText("Resimler_silinecekeler" + DateTime.Now.ToLongTimeString() + ".txt", string.Join(",", silinecekler), Encoding.UTF8);
            if (güncellenecekler.Count > 0)
                File.WriteAllText("Resimler_güncellenecekler" + DateTime.Now.ToLongTimeString() + ".txt", string.Join(",", güncellenecekler), Encoding.UTF8);
            panel3.Visible = false;
        }

        private void kontrolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string sku = Microsoft.VisualBasic.Interaction.InputBox("stok Kodunu giriniz.").Trim();
            //if (sku == "") return;
            int paketMiktarı = 1;
            string base64_değişen;
            MySqlDataReader dr;
            MySqlConnection con = new MySqlConnection(veritabanı.connectionString);
            con.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            cmd.CommandTimeout = 6000;
            cmd.CommandText = string.Format("select count(*) from resimler where resim_uzunluk>{0}", Resim_işlemleri.max_byte);
            int adet = 0;
            dr = cmd.ExecuteReader();
            if (dr.Read()) adet = dr.GetInt32(0);
            dr.Close();
            if (adet > 0) adet = (adet / paketMiktarı) + (adet % paketMiktarı == 0 ? 0 : 1);
            int adet_i = 0;
            int toplam_silinen = 0;
            while (true)
            {
                cmd.CommandText = string.Format("select sku,resim_base64 from resimler where resim_uzunluk>{0} order by resim_uzunluk desc limit {1}", Resim_işlemleri.max_byte, paketMiktarı);
                List<string> sku = new List<string>();
                List<string> resim_base64 = new List<string>();
                
                    dr = cmd.ExecuteReader();
                    while (dr.Read()) { sku.Add(dr.GetString(0)); resim_base64.Add(dr.GetString(1)); }
                    dr.Close();
                    if (sku.Count > 0) progresssBarGöster(sku.Count);
                    else break;
                    adet_i++;
                    int silinen = 0;
                    while (sku.Count > 0)
                    {
                        progressBar1.Value++;
                        label4.Text = string.Format("{0}/{1}  ({2}/{3})", progressBar1.Value, paketMiktarı, adet_i, adet);
                        progressBar1.Refresh(); label4.Refresh();

                        if (resim_base64[0] != "")
                        {
                            try
                            {
                                base64_değişen = Resim_işlemleri.resimBase64_KontrolEdilmişHali(resim_base64[0]);
                                cmd.CommandText = string.Format("update resimler set resim_base64='{0}' where sku='{1}'", base64_değişen, sku[0]);
                            }
                            catch (Exception hata)
                            {
                                cmd.CommandText = string.Format("delete from resimler where sku='{0}'", sku[0]);
                                silinen++;

                            }
                            veritabanı.cmdExecute(cmd);
                        }
                        sku.RemoveAt(0); resim_base64.RemoveAt(0);
                    }
                    toplam_silinen += silinen;
            }
            con.Close();
        }
    }
}
