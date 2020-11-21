using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileServer = new FileServer(args);
            fileServer.Run();

            //todo создание списка файлов и размера - DONE
            //todo получение запроса от клиента - DONE
            //todo отправка списка файов - DONE
            //todo получение запроса на файл - DONE
            //todo отдача файла - DONE
            Console.ReadKey();
        }
    }
}
