using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServerProject
{
    class Handler
    {
        private Socket m_server;
        private string m_host;
        private const int m_ReadSize = 2048;

        public Handler(Socket serverSocket)
        {
            m_server = serverSocket;
        }
        /// <summary>
        /// starts handling the HTTP connection
        /// </summary>
        public void Handle()
        {
            Dictionary<string, string> m_headers;
            string requestHeader = getHTTPHeader(m_server);
            if (!requestHeader.Contains("GET") || requestHeader.Contains("Visual Studio") || requestHeader.Contains("192.168.1."))
                return;
            m_headers = getHeaders(requestHeader);

            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress[] addressList = Dns.GetHostAddresses(m_headers["Host"]);
            IPEndPoint remoteEP = new IPEndPoint(addressList[0], 80);
            clientSocket.Connect(remoteEP);

            SendReqeust(clientSocket, requestHeader);

            requestHeader = getHTTPHeader(clientSocket);

            SendReqeust(m_server, requestHeader);

            int read = 0;
            byte[] buf = new byte[m_ReadSize];
            read = clientSocket.Receive(buf);
            while (read > 0)
            {
                m_server.Send(buf, read, SocketFlags.None);
                read = clientSocket.Receive(buf);
            }

            m_server.Shutdown(SocketShutdown.Both);
            m_server.Close();
        }
        private static string getHTTPHeader(Socket socket)
        {
            string request = "";
            byte[] rbytes = new byte[m_ReadSize];
            int bytesRcvd = 0;

            while ((bytesRcvd = socket.Receive(rbytes)) > 0 && !request.Contains("\r\n\r\n"))
            {
                request += Encoding.ASCII.GetString(rbytes, 0, rbytes.Length);
            }
            string[] header = new string[2];
            header = request.Split(new string[] { "\r\n\r\n" }, 2, StringSplitOptions.None);
            return header[0];
        }
        private static Dictionary<string, string> getHeaders(string HTTPHeader) // must be sent header
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string[] lines = new string[256];
            lines = HTTPHeader.Split(new string[] { "\r\n" }, 256, StringSplitOptions.None);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] line = new string[2];
                line = lines[i].Split(new string[] { ": " }, 2, StringSplitOptions.None);
                headers.Add(line[0], line[1]);
            }

            return headers;
        }
        private static byte[] getBodyScrapsMethod(byte[] buf)
        {
            int bodyStartIndex = -1;
            for (int i = 0; i < buf.Length && bodyStartIndex < 0; i++)
            {
                if (buf[i] == '\r')
                    if (buf[i + 1] == '\n')
                        if (buf[i + 2] == '\r')
                            if (buf[i + 3] == '\n')
                                bodyStartIndex = i + 4;
            }
            while (++bodyStartIndex < buf.Length && buf[bodyStartIndex] == 0) ;

            byte[] bodySegment = new byte[buf.Length - bodyStartIndex];

            // might want to use Buffer.BlockCopy
            Array.Copy(buf, bodyStartIndex, bodySegment, 0, buf.Length - bodyStartIndex);
            return bodySegment;
        }
        public void SendReqeust(Socket socket, string header)
        {
            byte[] sbytes = Encoding.ASCII.GetBytes(header);
            socket.Send(sbytes, sbytes.Length, SocketFlags.None);
        }
    }
}
