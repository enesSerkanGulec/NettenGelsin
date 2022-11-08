using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using System.Xml;
using System.IO;
using System.Globalization;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;

namespace NettenGelsin
{
    class logType
    {
        public DateTime zaman;
        public string ifade;
        public string tip;
        public logType(string ifade, string tip = "Bilgi")
        {
            this.zaman = DateTime.Now;
            this.ifade = ifade;
            this.tip = tip;
        }
        public string toString()
        {
            return "('" + zaman.ToString("yyyy-MM-dd HH:mm:ss") + "','" + ifade.Replace("'", "\\'") + "','" + tip + "')";
        }
    }
    public delegate void tetikleme();
    public static class log
    {
        public static MySqlConnection con;
        static MySqlCommand cmd = new MySqlCommand();
        
        static List<logType> yazılacaklar = new List<logType>();
        static Thread anaThread = new Thread(() => kontrol());
        static Thread yazmaişlemi;
        public static bool kapanacak = false;
        private static void kontrol()
        {
            while (true)
            {
                if (yazılacaklar.Count > 0 && !(yazmaişlemi != null && yazmaişlemi.IsAlive))
                {
                    yazmaişlemi = new Thread(() => Yaz_());
                    yazmaişlemi.Start();
                    yazmaişlemi.Join();
                    Thread.Sleep(10);
                }
                if (kapanacak) break;
                Thread.Sleep(5000);
            }
        }
        public static void Yaz(string ifade, string tip = "")
        {
            yazılacaklar.Add(new logType(ifade, tip));
            if (anaThread.ThreadState == System.Threading.ThreadState.Unstarted) anaThread.Start();
        }
        static void Yaz_()
        {
            int max = yazılacaklar.Count;
            string s = "";
            try
            {
                lock (yazılacaklar)
                {
                    for (int i = 0; i < max; i++) s += (s == "" ? "" : ",") + yazılacaklar[i].toString();
                    cmd.CommandText = "insert into logs(zaman,mesaj,tip) values " + s;
                    veritabanı.cmdExecute(cmd);
                    yazılacaklar.RemoveRange(0, max);
                }
            }
            catch(Exception hata)
            {
                cmd.CommandText = "Yazma işleminde hata oluştu. Hata Mesajı: " + hata.Message;
                veritabanı.cmdExecute(cmd);
                yazılacaklar.RemoveRange(0, max);
            }
        }
        public static void Başla()
        {
            con = new MySqlConnection(veritabanı.connectionString);
            cmd.Connection = con;
            cmd.CommandTimeout = 300;
            con.Open();
            cmd.CommandText = "DELETE FROM logs WHERE DATEDIFF(CURRENT_DATE(),logs.zaman)>5";
            veritabanı.cmdExecute(cmd);
        }
    }

    public static class email
    {
        static string senderAdres = "enesserkangulec@gmail.com";
        static string senderPwd = "Enes.072";
        public static string[] receivers = { "enesgulec@gmail.com", "engin@ortaklarbilgisayar.com" };

        public static void send(string konu, string gövdeMetni)
        {
            var fromAddress = new MailAddress(senderAdres, "__nettengelsin__");
            string fromPassword = senderPwd;
            string subject = konu;
            string body = gövdeMetni;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            foreach (string receiver in receivers)
            {
                var toAddress = new MailAddress(receiver, receiver.Substring(0, receiver.IndexOf('@')));
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception hata)
                    {
                        log.Yaz("e-posta gönderilemedi. (gönderilen:" + toAddress + ", Mesaj metni:[" + subject + "] " + body + ") Hata Mesajı:" + hata.Message);
                    }
                }
            }
        }
    }


 }