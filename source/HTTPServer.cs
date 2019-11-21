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
                Utils.Log("фатальный сбой при запуске веб сервера: " + e.Message, LogLevel.ERROR);

                MainForm.Instance.DisableStartWorkingButton();

                if (Worker.Instance != null)
                {
                    Worker.Instance.inFatalErrorState = true;
                }

                threadActive = false;
                return;
            }

            Utils.Log("Веб сервер подключен по адресу " + Constants.RESULTS_WEB_PAGE, LogLevel.SUCCESS);

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
                catch (HttpListenerException)
                {
                    // Игнорируем сетевые ошибки
                }
                catch (Exception e)
                {
                    Utils.Log("веб сервер не смог обработать запрос. Причина: " + e.Message + " " + e.ToString(), LogLevel.ERROR);
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

                    var showParams = request != "save_settings";
                    if (!showParams)
                    {
                        Utils.Log("    для безопасности параметры этого запроса не будут показаны в логе", LogLevel.NOTIFY);
                    }

                    // Параметры запроса
                    var parametersDictionary = new Dictionary<string, string>();
                    using (var streamReader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        var paramPairs = streamReader.ReadToEnd().Split('&');
                        foreach (var paramPair in paramPairs)
                        {
                            var splittedParam = paramPair.Split('=');
                            if (splittedParam.Length > 1)
                            {
                                var paramKey = splittedParam[0];
                                var paramValue = splittedParam[1];

                                if (showParams)
                                {
                                    Utils.Log("    параметр " + paramKey + "=" + WebUtility.UrlDecode(paramValue), LogLevel.NOTIFY);
                                }

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
            if (data != null)
            {
                context.Response.ContentLength64 = data.Length;
                using (var memoryStream = new MemoryStream(data))
                {
                    memoryStream.CopyTo(context.Response.OutputStream);
                    memoryStream.Flush();
                }

                context.Response.OutputStream.Flush();
            }
            context.Response.StatusCode = (int)statusCode;
            context.Response.OutputStream.Close();
        }
    }
}