using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ProxyServerProject
{
    class TCPProxyServer
    {
        public TCPProxyServer(int port)
        {

            try
            {
                port = 4550;
                Socket alwaysListening = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress addr = IPAddress.Loopback;
                IPEndPoint localEndpoint = new IPEndPoint(addr, port);
                alwaysListening.Bind(localEndpoint);
                alwaysListening.Listen(100);
                ConcurrentQueue<Thread> clients = new ConcurrentQueue<Thread>();

                byte[] bytes = new byte[1024];

                while (true)
                {
                    Socket s = alwaysListening.Accept();

                    Thread c = new Thread(new Handler(s).Handle);
                    c.Start();
                    //clients.Enqueue(c);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
