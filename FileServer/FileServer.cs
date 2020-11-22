using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Net;

namespace FileServer
{
    public class FileServer
    {
        private TcpListener listener;
        private Socket socket;
        private string[] args;
        private string dir;

        public FileServer(string[] args)
        {
            this.args = args;
            if (this.args.Length != 0) {
                dir = args[0];
            }
            else
            {
                dir = @"C:\Users\Dima\Pictures";
            }
        }

        public List<FileInfo> GetFileList(string dir = @"C:\Users\Dima\Pictures")
        {
            if (Directory.Exists(dir))
            {
                DirectoryInfo d = new DirectoryInfo(dir);
                System.IO.FileInfo[] files = d.GetFiles();
                var fileInfos = new List<FileInfo>();
                foreach (var file in files)
                {
                    fileInfos.Add(new FileInfo
                    {
                        FileName = file.Name,
                        FileSize = (int)file.Length
                    });
                }
                return fileInfos;
            }
            else
            {
                Console.WriteLine("Incorrect name or not found directory");
                throw new IncorrectWorkException("Incorrect name or not found directory");
            }
        }
        public string Serialize(List<FileInfo> files)
        {
            return JsonSerializer.Serialize(files);
        }

        public void Run()
        {
            InitConnection();
            ClientRequestHandling();
        }

        public void SendFileToClient(string fileName)
        {
            var file = $"{this.dir}\\{fileName}";
            Byte[] data = new Byte[1024];
            FileStream streamFile = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            while (true)
            {
                byte[] chunk = new byte[1024];

                int index = 0;
                while (index < chunk.Length)
                {
                    int bytesRead = streamFile.Read(chunk, index, chunk.Length - index);

                    if (bytesRead == 0)
                    {
                        break;
                    }
                    if (bytesRead < 1024)
                    {
                        byte[] temp = new byte[bytesRead];

                        for (var i = 0; i < bytesRead; i++)
                            temp[i] = chunk[i];

                        chunk = temp;
                    }

                    index += bytesRead;
                }
                if (index != 0)
                {
                    socket.Send(chunk);
                }
                if (index != chunk.Length)
                {
                    socket.Send(Encoding.ASCII.GetBytes("<DONE>"));
                    return;
                }
            }

        }

        public void SendFileListToClient()
        {
            var filelist = args.Length == 0 ? GetFileList() : GetFileList(args[0]);
            var jsonString = Serialize(filelist);
            byte[] msg = Encoding.ASCII.GetBytes(jsonString);
            socket.Send(msg);
        }

        public void ClientRequestHandling()
        {
            while (true)
            {
                if (socket is null || !socket.IsBound || !socket.Connected)
                {
                    socket = listener.AcceptSocket();
                }

                int bufferSize = 1024;
                var header = new byte[bufferSize];

                try
                {
                    socket.Receive(header);

                    var headerStr = Encoding.ASCII.GetString(header); //incorrect work

                    if (headerStr.StartsWith("INIT"))
                    {
                        CommandHandling(Command.INIT, "Hello");
                    }

                    else if (headerStr.StartsWith("LIST"))
                    {
                        CommandHandling(Command.LIST, "");
                    }

                    else if (headerStr.StartsWith("QUIT"))
                    {
                        CommandHandling(Command.QUIT, "");
                    }

                    else if (headerStr.StartsWith("FILE"))
                    {
                        var str_slice = headerStr.Substring(0, headerStr.IndexOf('\r'));
                        var tokens = str_slice.Split(' ');
                        var fn = "";
                        for (int i = 1; i < tokens.Length; i++) {
                            fn = fn + tokens[i];
                        }
                        CommandHandling(Command.FILE, fn);
                    }

                    else
                    {
                        CommandHandling(Command.INCORRECT, headerStr);
                    }
                }

                catch
                {
                    Console.WriteLine("Соединение принудительно закрыто клиентом");

                }
            }
        }

        public void CommandHandling(Command command, string parameter)
        {
            switch (command)
            {
                case Command.LIST:
                    SendFileListToClient();
                    break;
                case Command.FILE:
                    SendFileToClient(parameter);
                    break;
                case Command.QUIT:
                    Quit();
                    break;
                case Command.INIT:
                    Greetings(parameter);
                    break;
                case Command.INCORRECT:
                    IncorrectRequest(parameter);
                    break;
            }
        }

        public void Greetings(string str)
        {
            byte[] msg = Encoding.ASCII.GetBytes($"{str}\r\n");
            int c = socket.Send(msg);
        }

        public void Quit()
        {
            byte[] msg = Encoding.ASCII.GetBytes("Socket will be closed\r\n");
            socket.Send(msg);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        public void IncorrectRequest(string request)
        {
            byte[] msg = Encoding.ASCII.GetBytes($"{request}\r\n");
            socket.Send(msg);
        }

        public void InitConnection()
        {
            int port = 8089;
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
        }
    }
}
