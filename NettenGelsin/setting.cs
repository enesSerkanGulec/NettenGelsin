using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace NettenGelsin
{
    public class setting
    {
        static DirectoryInfo dizin = new DirectoryInfo(".\\setting");
        static FileInfo dosya = new FileInfo(dizin.FullName+"\\setting.txt");

        public static void Başla()
        {
           if (!dizin.Exists) dizin.Create();
           if (!dosya.Exists) dosya.Create();
        }

        public static void dosyayaYaz(bool yüzdeMi,decimal artış, string[] silinecekler,string[] fiyatArtacaklar,decimal eşikDeğeri,int g1,int g2)
        {
            //string[] clist = listBoxPart.Items.OfType<string>().ToArray();
            //string.Join("", test);

            if (!dizin.Exists) dizin.Create();
            if (!dosya.Exists) dosya.Create();
             
            try
            {
                FileStream file = new FileStream(dosya.FullName, FileMode.Create);
                StreamWriter writer = new StreamWriter(file, Encoding.UTF8);
                writer.WriteLine(string.Format("{0};{1};{2};{3};{4}",yüzdeMi?1:0,artış,eşikDeğeri,g1,g2));
                writer.WriteLine(string.Join("½",silinecekler));
                writer.WriteLine(string.Join("½", fiyatArtacaklar));
                writer.Close();
                file.Close();
            }
            catch
            {
                
            }
        }

        public static void dosyadanOku(RadioButton yüzde, RadioButton TL, NumericUpDown değer, ListBox silinecekler, ListBox fiyatArttırılacaklar,NumericUpDown eşikDeğeri,RadioButton r1A,RadioButton r1B,RadioButton r2A,RadioButton r2B,RadioButton r2C)
        {
            if (dosya.Exists)
            {
                string[] a, b, c;
                string x, y, z;
                FileStream file = new FileStream(dosya.FullName, FileMode.Open);
                StreamReader reader = new StreamReader(file, Encoding.UTF8);
                try
                {
                    x = reader.ReadLine();
                    y = reader.ReadLine();
                    z = reader.ReadLine();
                    if (x is null || y is null || z is null) return;

                    a = x.Split(';');
                    b = y.Split('½');
                    c = z.Split('½');

                    if (a[0] == "1") yüzde.Checked = true;
                    else TL.Checked = true;
                    decimal s = 100;
                    decimal.TryParse(a[1], out s);
                    değer.Value = s;

                    s = 10000;
                    decimal.TryParse(a[2], out s);
                    eşikDeğeri.Value = s;

                    int g1 = 2;
                    int g2 = 1;
                    int.TryParse(a[3], out g1);
                    int.TryParse(a[4], out g2);
                    if (g1 == 1) r1A.Checked = true;
                    else r1B.Checked = true;
                    if (g2 == 2) r2B.Checked = true;
                    else if (g2 == 3) r2C.Checked = true;
                    else r2A.Checked = true;

                    silinecekler.Items.Clear();
                    foreach (string kelime in b) if (kelime!="") silinecekler.Items.Add(kelime);

                    fiyatArttırılacaklar.Items.Clear();
                    foreach (string kelime in c) if (kelime != "") fiyatArttırılacaklar.Items.Add(kelime);
                }
                finally
                {
                    reader.Close();
                    file.Close();
                }
            }
        }
    }
}
