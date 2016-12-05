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
        private Dictionary<string, string> m_headers;
        private const int m_ReadSize = 8192; //(1024*8)

        public Handler(Socket serverSocket)
        {
            m_server = serverSocket;
        }
        /// <summary>
        /// starts handling the HTTP connection
        /// </summary>
        public void Handle()
        {
            // State 0: Handle Reqeust from Client
            byte[] rbuf = rcvBytes(m_server);
            byte[] body = getBodyScrapsMethod(rbuf);

            string requestHeader = getHTTPHeader(Encoding.ASCII.GetString(rbuf));

            if (!string.IsNullOrEmpty(requestHeader))
            {
                Console.WriteLine(requestHeader);
            }
            if (!requestHeader.Contains("GET") || requestHeader.Contains("Visual Studio") || requestHeader.Contains("VisualStudio") || requestHeader.Contains("Microsoft"))
                return;
            Console.WriteLine(requestHeader);

            m_headers = getHeaders(requestHeader);
            // State 1: Rebuilding Request Information and Create Connection to Destination Server
            requestHeader = requestHeader.Replace("http://" + m_headers["Host"], "");

            requestHeader = requestHeader.Replace("Proxy-Connection", "Connection");
            if (m_headers.ContainsKey("Proxy-Connection") && !m_headers.ContainsKey("Connection"))
            {
                m_headers.Add("Connection", m_headers["Proxy-Connection"]);
                m_headers.Remove("Proxy-Connection");
            }
            
            Socket remoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress[] addressList = Dns.GetHostAddresses(m_headers["Host"]);
            IPEndPoint remoteEP = new IPEndPoint(addressList[0], 80);

            // State 2: Sending New Request Information to Destination Server and Relay Response to Client
            remoteSocket.Connect(remoteEP);
            SendHeader(remoteSocket, requestHeader);

            byte[] sbuf = rcvBytes(remoteSocket); 
            byte[] sBody = getBodyScrapsMethod(sbuf);
            string responseHeader = getHTTPHeader(Encoding.ASCII.GetString(sbuf));
            string respBody = Encoding.ASCII.GetString(sBody);
            int respBodyLen = respBody.Length;
            Console.WriteLine("Received --> \n-----\n" + requestHeader);
            // send response header to client form remote
            SendHeader(m_server, responseHeader);
            // Send body of response to client from remote
            int read = 0;
            byte[] buf = new byte[m_ReadSize];
            if (sBody.Length > 0)
            {
                int sbodySent = 0;
                while(sbodySent < sBody.Length)
                {
                    sbodySent += m_server.Send(sBody, sbodySent, m_ReadSize, SocketFlags.None); // crashes here, might be sending too many zeros?
                }
            }
            while ((read = remoteSocket.Receive(buf)) != 0)
            {
                m_server.Send(buf, read, SocketFlags.None);
            }

            m_server.Shutdown(SocketShutdown.Both);
            m_server.Close();
        }
        /// <summary>
        /// Receives bytes from socket
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private static byte[] rcvBytes(Socket socket)
        {
            byte[] rbytes = new byte[m_ReadSize];
            int bytesRcvd = 0;
            int offset = 0;
            //bool headerRead = false;
            while ((bytesRcvd = socket.Receive(rbytes, offset, m_ReadSize, SocketFlags.None)) > 0)
            {
                offset += bytesRcvd;
                string temp = Encoding.ASCII.GetString(rbytes);
                if (temp.Contains("\r\n\r\n"))
                    break;
                Array.Resize(ref rbytes, rbytes.Length * 2);
            }
            return rbytes;
        }
        private static string getHTTPHeader(string s)
        {
            string[] header = new string[2];
            header = s.Split(new string[] { "\r\n\r\n" }, 2, StringSplitOptions.None);
            return header[0] + "\r\n\r\n";
        }
        /// <summary>
        /// Parses HTTP header into dictionary of headers
        /// </summary>
        /// <param name="HTTPHeader"></param>
        /// <returns></returns>
        private static Dictionary<string, string> getHeaders(string HTTPHeader) // must be sent header
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string[] lines = new string[256];
            lines = HTTPHeader.Split(new string[] { "\r\n" }, 256, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] line = new string[2];
                line = lines[i].Split(new string[] { ": " }, 2, StringSplitOptions.RemoveEmptyEntries);
                headers.Add(line[0], line[1].Trim());
            }
            return headers;
        }
        /// <summary>
        /// Utility function used to read body if initial rcv read past header
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        private static byte[] getBodyScrapsMethod(byte[] buf)
        {
            int bodyStartIndex = -1;
            int bodyEndIndex = 0;
            for (int i = 0; i < buf.Length && bodyStartIndex < 0; i++)
            {
                if (buf[i] == '\r')
                    if (buf[i + 1] == '\n')
                        if (buf[i + 2] == '\r')
                            if (buf[i + 3] == '\n')
                                bodyStartIndex = i + 4;
            }
            while (++bodyStartIndex < buf.Length && buf[bodyStartIndex] == 0); // passes leading zeros
            for (bodyEndIndex = bodyStartIndex; bodyEndIndex < buf.Length; bodyEndIndex++) ; // gets to last byte of data

            byte[] bodySegment = new byte[bodyEndIndex - bodyStartIndex];

            // might want to use Buffer.BlockCopy
            
            Array.Copy(buf, bodyStartIndex, bodySegment, 0, bodySegment.Length);
            Encoding.ASCII.GetString(buf);
            Encoding.ASCII.GetString(bodySegment);
            return bodySegment;
        }
        /// <summary>
        /// Sends out bytes to client socket
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="header"></param>
        public void SendHeader(Socket socket, string header)
        {
            Console.WriteLine("Sending --> \n-----\n" + header);
            byte[] sbytes = Encoding.ASCII.GetBytes(header);
            socket.Send(sbytes, sbytes.Length, SocketFlags.None);
        }
        private static string getRequestURI(string requestLine)
        {
            string[] uri = new string[3];
            uri = requestLine.Split(new string[] { " " }, 3, StringSplitOptions.None);
            return uri[1];
        }

        /*TODOs
         * 1. PUT and HEAD methodology
         * 2. Concat body segment of first response read to rest of message to browser client
         * 3. Get CNN.com working
         * 4. Maybe a Dictionary of RemoteEPs in the TCPProxyServer.cs 
         */
    }
}
