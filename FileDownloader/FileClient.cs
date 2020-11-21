using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileDownloader
{
    public class FileClient
    {
        private TcpClient client;
        public bool Run()
        {
            if (!InitConnection())
            {
                return false;
            }
            return true;
        }

        public bool InitConnection()
        {
            try
            {
                client = new TcpClient();
                client.Connect("127.0.0.1", 8089);
                MessageBox.Show("Соединение с сервером установлено");
            }
            catch
            {
                MessageBox.Show("Не удалось установить соединение с сервером");
                client = null;
                return false;
            }
            return true;
        }

        public void CheckServer(bool show_msg)
        {
            if (client == null || !client.Connected)
            {
                Run();
            }
            else
            {
                NetworkStream stream = client.GetStream();
                Byte[] data = Encoding.ASCII.GetBytes("INIT\r\n");
                stream.Write(data, 0, data.Length);

                data = new Byte[256];
                Int32 bytes = stream.Read(data, 0, data.Length);
                var msg = Encoding.ASCII.GetString(data, 0, bytes);
                Trace.Write(msg);

                if (show_msg)
                {
                    MessageBox.Show("Соединение с сервером установлено");
                }
                else
                {
                    ;
                }
            }

            
        }

        public void Quit()
        {

        }

        public void GetFileList()
        {

        }

        public void GetFile()
        {

        }
    }
}
