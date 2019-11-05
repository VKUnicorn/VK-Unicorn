using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

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

        public bool HandleGetRequest(string request, out byte[] data, out string responseContentType, Dictionary<string, string> parametersDictionary)
        {
            responseContentType = Utils.GetMIMETypeByFilename(request);

            // Обрабатываем известные запросы на получение файлов и API
            if (request != string.Empty)
            {
                // Простая проверка на то запросили файл или что-то другое
                if (request.Contains('.'))
                {
                    // Пытаемся выслать файл, если он есть
                    data = Utils.GetEmbeddedFileByName(request);
                    if (data != null)
                    {
                        return true;
                    }
                }
                else
                {
                    // Запросили API
                    switch (request)
                    {
                        case "groups":
                            var resultObjects = new List<Dictionary<string, object>>();
                            Database.Instance.ForEachGroup((group) =>
                            {
                                resultObjects.Add(new Dictionary<string, object>()
                                {
                                    { "data", group },
                                    { "Efficiency", group.GetEfficiency() },
                                    { "URL", group.GetURL() },
                                });
                            });

                            // Сортируем группы по эффективости. Сначала идут группы с которых
                            // было получено больше всего профилей
                            resultObjects.Sort((left, right) =>
                            {
                                return ((int)right["Efficiency"]).CompareTo((int)left["Efficiency"]);
                            });

                            // Отправляем JSON ответ
                            data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(resultObjects));
                            return true;
                    }
                }

                // Неизвестный запрос?
                data = null;
                return false;
            }

            // Отгружаем index.html во всех остальных случаях
            var result = string.Empty;

            // Загружаем шаблон ответа
            result = Encoding.UTF8.GetString(Utils.GetEmbeddedFileByName("index.html"));

            // Заменяем константы
            result = result
                .Replace("$APP_NAME$", Constants.APP_NAME)
                .Replace("$APP_VERSION$", Constants.APP_VERSION)
            ;

            // Отправляем результат в UTF8 кодировке
            data = Encoding.UTF8.GetBytes(result);
            return true;
        }
    }
}