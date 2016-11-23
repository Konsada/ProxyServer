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
        TCPProxyServer()
        {
            TcpListener listener = null;
            try
            {
                int port = 4550;
                IPAddress localAddr = IPAddress.(Dns.GetHostEntry("localhost").AddressList[1]);

                listener = new TcpListener(localAddr, port);

                listener.Start();

                byte[] bytes = new byte[1024];
                string data = null;

                while (true)
                {
                    Console.WriteLine("Waiting for connection...");

                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Connected...");

                    int remotePort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;

                    // call handler
                }
            }
            catch
            {

            }
        }
    }
}
