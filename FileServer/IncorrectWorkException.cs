using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileServer
{
    class IncorrectWorkException : Exception
    {
        public IncorrectWorkException(string message)
            : base(message)
        {
        }
    }
}
