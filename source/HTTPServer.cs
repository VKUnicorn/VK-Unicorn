using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace VK_Unicorn
{
    class HTTPServer
    {
        // https://ru.wikipedia.org/wiki/Список_MIME-типов
        static readonly Dictionary<string, string> mimeTypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"", "text/html"},
            {".css", "text/css"},
            {".ico", "image/x-icon"},
            {".gif", "image/gif"},
            {".png", "image/png"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".js", "application/x-javascript"},
        };

        Thread thread;
        volatile bool threadActive;

        HttpListener listener;
        int port;

        public HTTPServer(int port)
        {
            this.port = port;

            thread = new Thread(Listen);
            thread.IsBackground = true;
            thread.Start();
        }

        void Listen()
        {
            threadActive = true;

            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(string.Format("http://localhost:{0}/", port));
                listener.Prefixes.Add(string.Format("http://127.0.0.1:{0}/", port));
                listener.Start();
            }
            catch (Exception e)
            {
                Utils.Log("ошибка веб сервера " + e.Message, LogLevel.ERROR);
                threadActive = false;
                return;
            }

            // Ожидаем запросы
            while (threadActive)
            {
                try
                {
                    var context = listener.GetContext();
                    if (!threadActive)
                    {
                        break;
                    }

                    ProcessContext(context);
                }
                catch (Exception e)
                {
                    Utils.Log("веб сервер не смог обработать запрос. Причина: " + e.Message, LogLevel.ERROR);
                    threadActive = false;
                }
            }
        }

        void ProcessContext(HttpListenerContext context)
        {
            var request = Path.GetFileName(context.Request.Url.AbsolutePath);

            var statusCode = HttpStatusCode.InternalServerError;
            context.Response.ContentType = mimeTypes[Path.GetExtension(request)];
            var data = System.Text.Encoding.UTF8.GetBytes(string.Empty);

            switch (context.Request.HttpMethod.ToUpperInvariant())
            {
                case "POST":
                    Utils.Log("Получен API запрос " + request, LogLevel.NOTIFY);

                    // Параметры запроса
                    var parametersDictionary = new Dictionary<string, string>();
                    using (var streamReader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        while (true)
                        {
                            var param = streamReader.ReadLine();
                            if (param == null)
                            {
                                break;
                            }

                            var splittedParam = param.Split('=');
                            if (splittedParam.Length > 1)
                            {
                                var paramKey = splittedParam[0];
                                var paramValue = splittedParam[1];

                                Utils.Log("    параметр " + paramKey + "=" + paramValue, LogLevel.NOTIFY);

                                parametersDictionary.Add(paramKey, paramValue);
                            }
                        }
                    }

                    if (!WebInterface.Instance.HandlePostRequest(request, out data, out statusCode, parametersDictionary))
                    {
                        statusCode = HttpStatusCode.BadRequest;
                    }
                    break;

                case "GET":
                    if (!WebInterface.Instance.HandleGetRequest(request, out data, context.Request.QueryString))
                    {
                        statusCode = HttpStatusCode.NotFound;
                    }
                    break;
            }

            // Отправляем ответ
            context.Response.ContentLength64 = data.Length;
            using (var memoryStream = new MemoryStream(data))
            {
                memoryStream.CopyTo(context.Response.OutputStream);
                memoryStream.Flush();
            }
            context.Response.OutputStream.Flush();
            context.Response.StatusCode = (int)statusCode;
            context.Response.OutputStream.Close();
        }
    }
}