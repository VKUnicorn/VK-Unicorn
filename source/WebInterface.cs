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

        enum ContentType
        {
            PROFILES,
            GROUPS,
        }

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
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();

                    // Добавляем в кэш
                    embeddedFilesCache.Add(fileName, result);
                }
            }

            return embeddedFilesCache[fileName];
        }

        public bool HandleRequest(string request, out byte[] data)
        {
            var result = string.Empty;

            // Загружаем шаблон ответа
            result = GetEmbeddedFileByName("index.html");

            // Определяем что хотел увидеть пользователь
            var contentType = ContentType.PROFILES;
            if (request == "groups")
            {
                contentType = ContentType.GROUPS;
            }

            // Пользователь хочет увидеть профили, но у него их нету и нету групп?
            // Тогда отправляем саразу в раздел групп, для настройки
            if (contentType == ContentType.PROFILES)
            {
                if (Database.Instance.IsNeedToSetupGroups())
                {
                    contentType = ContentType.GROUPS;
                }
            }

            // Заменяем константы
            result = result
                .Replace("$APP_NAME$", Constants.APP_NAME)
                .Replace("$APP_VERSION$", Constants.APP_VERSION)
            ;

            switch (contentType)
            {
                case ContentType.PROFILES:

                    break;

                case ContentType.GROUPS:
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
                    break;

                default:
                    data = null;
                    return false;
            }

            // Отправляем результат в UTF8 кодировке
            data = System.Text.Encoding.UTF8.GetBytes(result);
            return true;
        }
    }
}