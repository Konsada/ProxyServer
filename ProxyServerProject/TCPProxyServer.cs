using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

                byte[] bytes = new byte[1024];
                
                while (true)
                {
                    Console.WriteLine("Waiting for connection...");

                    Socket s = alwaysListening.Accept();
                    
                        Handler h = new Handler(s);
                        System.Threading.Thread newRequest = new System.Threading.Thread(h.Handle);
                        newRequest.Start();
                }
            }
            catch(SocketException ex)
            {

            }
        }
    }
}
