using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;

namespace VK_Unicorn
{
    class WebClient
    {
        MemoryStream memoryStream = new MemoryStream();
        readonly NetworkStream networkStream;
        readonly StreamReader streamReader;

        enum RequestType
        {
            GET,
            POST,
        }

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
                var firstLine = streamReader.ReadLine();
                if (firstLine == null)
                {
                    break;
                }

                var parametersDictionary = new Dictionary<string, string>();

                // Читаем параметры запросы
                var needToSkip = true; // Пропускаем все запросы до первого пустого
                while (true)
                {
                    var param = streamReader.ReadLine();
                    if (param == null)
                    {
                        break;
                    }

                    if (param != string.Empty)
                    {
                        if (needToSkip)
                        {
                            continue;
                        }

                        var splittedParam = param.Split('=');
                        if (splittedParam.Length > 1)
                        {
                            var paramKey = splittedParam[0];
                            var paramValue = splittedParam[1];

                            Utils.Log("    параметр " + paramKey + " = " + paramValue, LogLevel.NOTIFY);

                            parametersDictionary.Add(paramKey, paramValue);
                        }
                    }
                    else
                    {
                        needToSkip = false;
                    }
                }

                if (firstLine.ToUpperInvariant().StartsWith("GET "))
                {
                    // GET запросы в лог не добавляем

                    // Пришёл запрос вида: GET /file HTTP/1.1
                    var request = firstLine.Split(' ')[1].TrimStart('/');

                    // Отправляем заголовок и ответ
                    HandleRequest(request, RequestType.GET, parametersDictionary);

                    return true;
                }
                else if (firstLine.ToUpperInvariant().StartsWith("POST "))
                {
                    Utils.Log("Получен запрос " + firstLine, LogLevel.NOTIFY);

                    // Пришёл запрос вида: POST /file HTTP/1.1
                    var request = firstLine.Split(' ')[1].TrimStart('/');

                    // Отправляем заголовок и ответ
                    HandleRequest(request, RequestType.POST, parametersDictionary);

                    return true;
                }
            }

            return false;
        }

        async void HandleRequest(string request, RequestType requestType, Dictionary<string, string> parametersDictionary)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(string.Empty);
            var responseContentType = "text/html";
            var responseCode = "200 OK";

            try
            {
                switch (requestType)
                {
                    case RequestType.GET:
                        if (!WebInterface.Instance.HandleGetRequest(request, out data, out responseContentType, parametersDictionary))
                        {
                            // Запрос не обработан, возвращаем ошибку что такой ресурс не найден
                            data = System.Text.Encoding.ASCII.GetBytes("<html><body><h1>404 File Not Found</h1></body></html>");
                            responseContentType = "text/html";
                            responseCode = "404 Not found";
                        }
                        break;

                    case RequestType.POST:
                        if (!WebInterface.Instance.HandlePostRequest(request, out data, out responseContentType, out responseCode, parametersDictionary))
                        {
                            responseCode = "400 Bad Request";
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                data = System.Text.Encoding.UTF8.GetBytes("<html><body><h1>500 Internal server error</h1><pre>" + ex.ToString() + "</pre></body></html>");
                responseCode = "500 Internal server error";
            }

            var header = string.Format("HTTP/1.1 {0}\r\n"
                                       + "Server: {1}\r\n"
                                       + "Content-Length: {2}\r\n"
                                       + "Content-Type: {3}\r\n"
                                       + "Keep-Alive: Close\r\n"
                                       + "\r\n",
                                       responseCode, Constants.SERVER_NAME, data.Length, responseContentType);

            var headerBytes = System.Text.Encoding.ASCII.GetBytes(header);
            await networkStream.WriteAsync(headerBytes, 0, headerBytes.Length);
            await networkStream.WriteAsync(data, 0, data.Length);
            await networkStream.FlushAsync();

            networkStream.Dispose();
        }
    }
}