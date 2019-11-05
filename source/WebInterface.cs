using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace VK_Unicorn
{
    class WebInterface
    {
        public static WebInterface Instance { get; private set; }

        public WebInterface()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        Dictionary<string, string> embeddedFilesCache = new Dictionary<string, string>();
        string GetEmbeddedFileByName(string fileName)
        {
            // Ещё нету в кэше?
            if (!embeddedFilesCache.ContainsKey(fileName))
            {
                using (var stream = Utils.GetAssemblyStreamByName(fileName))
                using (var reader = new StreamReader(stream))
                {
                    // Добавляем в кэш
                    embeddedFilesCache.Add(fileName, reader.ReadToEnd());
                }
            }

            return embeddedFilesCache[fileName];
        }

        public byte[] GetEmbeddedFileByNameAsBytes(string fileName)
        {
            using (var stream = Utils.GetAssemblyStreamByName(fileName))
            {
                if (stream == null)
                {
                    return null;
                }

                var ba = new byte[stream.Length];
                stream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        public bool HandleGetRequest(string request, out byte[] data, out string responseContentType, Dictionary<string, string> parametersDictionary)
        {
            responseContentType = "text/html";

            // Обрабатываем известные запросы на получение файлов и API
            switch (request)
            {
                case "style.css":
                    data = System.Text.Encoding.UTF8.GetBytes(GetEmbeddedFileByName(request));
                    responseContentType = "text/css";
                    return true;

                case "main.js":
                case "hullabaloo.min.js":
                    data = System.Text.Encoding.UTF8.GetBytes(GetEmbeddedFileByName(request));
                    return true;

                case "favicon.ico":
                    data = GetEmbeddedFileByNameAsBytes("icon.ico");
                    responseContentType = "image/vnd.microsoft.icon";
                    return true;
            }

            // Неизвестный запрос?
            if (request != string.Empty)
            {
                data = null;
                return false;
            }

            var result = string.Empty;

            // Загружаем шаблон ответа
            result = GetEmbeddedFileByName("index.html");

            // Заменяем константы
            result = result
                .Replace("$APP_NAME$", Constants.APP_NAME)
                .Replace("$APP_VERSION$", Constants.APP_VERSION)
            ;

            /*
            var groupsContent = string.Empty;

            // Список пар: эффективность группы и заполненный шаблон группы.
            // используется потом для отображения групп в порядке эффективности
            var groupsList = new List<KeyValuePair<int, string>>();
            Database.Instance.ForEachGroup((group) =>
            {
                // Заполняем шаблон группы для отображения
                var groupTemplate = GetEmbeddedFileByName("group.template.html");

                var groupResultsCount = group.GetResultsCount();
                groupsList.Add(
                    new KeyValuePair<int, string>(groupResultsCount, groupTemplate
                        .Replace("$GROUP_ID$", group.Id.ToString())
                        .Replace("$GROUP_NAME$", group.Name)
                        .Replace("$GROUP_PHOTO_URL$", group.PhotoURL)
                        .Replace("$GROUP_RESULTS$", groupResultsCount.ToString())
                        .Replace("$GROUP_URL$", group.GetURL())
                    )
                );
            });

            // Сортируем группы по эффективости. Сначала идут группы с которых
            // было получено больше всего профилей
            groupsList.Sort((left, right) =>
            {
                return right.Key.CompareTo(left.Key);
            });

            foreach (var groupPair in groupsList)
            {
                groupsContent += groupPair.Value;
            }

            result = result.Replace("$CONTENT$", groupsContent);
            */

            // Отправляем результат в UTF8 кодировке
            data = System.Text.Encoding.UTF8.GetBytes(result);
            return true;
        }
    }
}