using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint endp = new IPEndPoint(IPAddress.Any, 54188);
            UdpClient udp = new UdpClient(endp.Port);
            MessageBox.Show("OK");
            for(; ; )
            {
                byte[] buffer = udp.Receive(ref endp);
                string s = Encoding.UTF8.GetString(buffer);
                TextToSpeech(s);
            }
        }

        private static void TextToSpeech(string text)
        {
            GC.Collect();

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.text-to-speech.cn/getSpeek.php");
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                string request = $"language=中文（普通话，简体）&voice=zh-CN-YunxiNeural&text={text}&role=0&style=0&styledegree=1&rate=0&pitch=0&kbitrate=audio-16khz-32kbitrate-mono-mp3&silence=&user_id=";
                Stream reqs = req.GetRequestStream();
                byte[] buffer = Encoding.UTF8.GetBytes(request);
                reqs.Write(buffer, 0, buffer.Length);
                reqs.Close();
                var resp = req.GetResponse();
                var stream = resp.GetResponseStream();
                string s = new StreamReader(stream).ReadToEnd();

                string url = s;
                url = url.Substring(0, url.IndexOf(".mp3") + 4);
                url = url.Substring(url.LastIndexOf("https://"));

                WebClient wc = new WebClient();
                wc.DownloadFile(url, "temp.mp3");
                
                wc.Dispose();
            }
            catch (Exception)
            {
                return;
            }

            var player = new WindowsMediaPlayer();
            player.URL = "temp.mp3";
            player.controls.play();

            while (true)
            {
                try
                {
                    if (player.playState == WMPPlayState.wmppsStopped) break;
                    Task.Delay(100);
                }
                catch (COMException)
                {

                }
            }

            player.close();
        }
    }
}
