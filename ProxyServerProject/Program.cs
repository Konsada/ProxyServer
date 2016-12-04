using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServerProject
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPProxyServer proxy = new TCPProxyServer(4550);
        }
    }
}
