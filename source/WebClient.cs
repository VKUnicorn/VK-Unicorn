using System;
using System.IO;
using System.Net.Sockets;

namespace VK_Unicorn
{
    class WebClient
    {
        MemoryStream memoryStream = new MemoryStream();
        readonly NetworkStream networkStream;
        readonly StreamReader streamReader;

        public WebClient(Socket socket)
        {
            networkStream = new NetworkStream(socket, true);
            streamReader = new StreamReader(memoryStream);
        }

        public async void Do()
        {
            var buffer = new byte[4096];
            while (true)
            {
                var bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    return;
                }

                memoryStream.Seek(0, SeekOrigin.End);
                memoryStream.Write(buffer, 0, bytesRead);

                var done = ProcessHeader();
                if (done)
                {
                    break;
                }
            }
        }

        bool ProcessHeader()
        {
            while (true)
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                var line = streamReader.ReadLine();
                if (line == null)
                {
                    break;
                }

                if (line.ToUpperInvariant().StartsWith("GET "))
                {
                    // We got a request: GET /file HTTP/1.1
                    var target = line.Split(' ')[1].TrimStart('/');

                    // Default target is index
                    if (string.IsNullOrWhiteSpace(target))
                    {
                        target = "index";
                    }

                    Console.WriteLine("Получен запрос " + target);

                    // Send header + response
                    SendResponse(target);

                    return true;
                }
            }

            return false;
        }

        async void SendResponse(string target)
        {
            byte[] data;
            var responseCode = string.Empty;
            var contentType = string.Empty;

            try
            {
                if (!WebInterface.Instance.HandleRequest(target, out data))
                {
                    data = System.Text.Encoding.ASCII.GetBytes("<html><body><h1>404 File Not Found</h1></body></html>");
                    contentType = "text/html";
                    responseCode = "404 Not found";
                }
                else
                {
                    contentType = "text/html";
                    responseCode = "200 OK";
                }
            }
            catch (Exception exception)
            {
                data = System.Text.Encoding.ASCII.GetBytes("<html><body><h1>500 Internal server error</h1><pre>" + exception.ToString() + "</pre></body></html>");
                responseCode = "500 Internal server error";
            }

            var header = string.Format("HTTP/1.1 {0}\r\n"
                                       + "Server: {1}\r\n"
                                       + "Content-Length: {2}\r\n"
                                       + "Content-Type: {3}\r\n"
                                       + "Keep-Alive: Close\r\n"
                                       + "\r\n",
                                       responseCode, Constants.SERVER_NAME, data.Length, contentType);

            var headerBytes = System.Text.Encoding.ASCII.GetBytes(header);
            await networkStream.WriteAsync(headerBytes, 0, headerBytes.Length);
            await networkStream.WriteAsync(data, 0, data.Length);
            await networkStream.FlushAsync();

            networkStream.Dispose();
        }
    }
}