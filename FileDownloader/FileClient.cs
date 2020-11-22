using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Text.Json;
using System.Windows.Controls;
using System.IO;

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
                    //MessageBox.Show("Соединение с сервером установлено");
                }
            }
        }

        public void Quit()
        {
            if (client != null && client.Connected)
            {
                NetworkStream stream = client.GetStream();
                Byte[] data = Encoding.ASCII.GetBytes("QUIT\r\n");
                stream.Write(data, 0, data.Length);

                data = new Byte[256];
                Int32 bytes = stream.Read(data, 0, data.Length);
                var msg = Encoding.ASCII.GetString(data, 0, bytes);
                Trace.Write(msg);
                MessageBox.Show("Соединение с сервером закрыто");
            }
        }

        public List<FileInfo> GetFileList()
        {
            NetworkStream stream = client.GetStream();
            Byte[] data = Encoding.ASCII.GetBytes("LIST\r\n");
            stream.Write(data, 0, data.Length);

            string jsonString = "";
            while (true)
            {
                data = new Byte[1024];
                Int32 bytes = stream.Read(data, 0, data.Length);
                jsonString = jsonString + Encoding.ASCII.GetString(data, 0, bytes);
                Trace.Write(jsonString);
                if (bytes < 1024)
                {
                    break;
                }
            }

            var fileList = JsonSerializer.Deserialize<List<FileInfo>>(jsonString);
            return fileList;
        }

        public void GetFile(string fn, string dir)
        {
            CheckServer(false);
            MessageBox.Show("Началась загрузка файла");

            NetworkStream stream = client.GetStream();
            Byte[] data = Encoding.ASCII.GetBytes($"FILE {fn}\r\n");
            stream.Write(data, 0, data.Length);

            FileStream fs = new FileStream($"{dir}\\{fn}", FileMode.OpenOrCreate);

            while (true)
            {
                data = new Byte[1024];
                Int32 bytes = stream.Read(data, 0, data.Length);

                if (bytes < 1024)
                {
                    var data1 = new byte[data.Length - 6];
                    Buffer.BlockCopy(data, 0, data1, 0, data1.Length - 6);
                    fs.Write(data1, 0, bytes);
                    break;
                }
                    fs.Write(data, 0, bytes);
            }
            fs.Close();
            MessageBox.Show("Загрузка файла завершена");
        }
    }
}
