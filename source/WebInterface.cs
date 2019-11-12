using System;
using System.Net;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Specialized;

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

        public bool HandlePostRequest(string request, out byte[] data, out HttpStatusCode statusCode, Dictionary<string, string> parametersDictionary)
        {
            data = Encoding.UTF8.GetBytes(string.Empty);
            statusCode = HttpStatusCode.OK;

            // Обработан ли запрос на API?
            var handled = false;
            // Результат для JSON ответа
            var resultObjects = new List<Dictionary<string, object>>();

            try
            {
                // Запросили API
                switch (request)
                {
                    // Удаление сообщества
                    case "delete_group":
                        {
                            Database.Instance.Delete<Database.Group>(long.Parse(parametersDictionary["id"]));
                            handled = true;
                        }
                        break;

                    // Добавление сообщества
                    case "add_group":
                        {
                            // Разделяем строку на список строк
                            var groupsToAdd = WebUtility.UrlDecode(parametersDictionary["url"]).Split(
                                new[] { "\r\n", "\r", "\n" },
                                StringSplitOptions.None
                            );

                            // Добавляем каждое сообщество
                            foreach (var group in groupsToAdd)
                            {
                                // Пропускаем пустые строки
                                if (group.Trim() == string.Empty)
                                {
                                    continue;
                                }

                                Worker.Instance.RegisterNewGroupToReceiveInfo(group);
                            }

                            handled = true;
                        }
                        break;

                    // Скрытие пользователя
                    case "hide_user":
                        {
                            var userId = long.Parse(parametersDictionary["id"]);

                            Database.Instance.ModifyFields<Database.User>(userId, (user) =>
                            {
                                user.IsHidden = Database.HiddenStatus.HIDDEN_UNTIL_ACTIVITY;
                            });

                            handled = true;
                        }
                        break;

                    // Удаление пользователя
                    case "delete_user":
                        {
                            var userId = long.Parse(parametersDictionary["id"]);

                            // Удаляем самого пользователя
                            Database.Instance.Delete<Database.User>(userId);

                            // Удаляем всю связанную с ним активность
                            Database.Instance.Delete<Database.UserActivity>(userId);

                            handled = true;
                        }
                        break;

                    // Изменение статуса "в избранном" для пользователя
                    case "favorite_user":
                        {
                            var userId = long.Parse(parametersDictionary["id"]);
                            var favorite = bool.Parse(parametersDictionary["favorite"]);

                            // Изменяем статус избранного
                            Database.Instance.ModifyFields<Database.User>(userId, (user) =>
                            {
                                user.IsFavorite = favorite;
                            });

                            handled = true;
                        }
                        break;
                }

                // Запрос обработан?
                if (handled)
                {
                    // Отправляем JSON ответ
                    data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(resultObjects));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Utils.Log("не удалось обработать запрос. Причина: " + ex.Message, LogLevel.ERROR);
                data = Encoding.UTF8.GetBytes(ex.Message);
                statusCode = HttpStatusCode.InternalServerError;
                return true;
            }

            return false;
        }

        public bool HandleGetRequest(string request, out byte[] data, NameValueCollection query)
        {
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
                    // Обработан ли запрос на API?
                    var handled = false;
                    // Результат для JSON ответа
                    var resultObjects = new List<Dictionary<string, object>>();

                    // Запросили API
                    switch (request)
                    {
                        // Получение списка сообществ
                        case "groups":
                            Database.Instance.ForEach<Database.Group>((group) =>
                            {
                                // Заменяем ссылку на фото, если нужно
                                group.PhotoURL = Utils.FixPhotoURL(group.PhotoURL);

                                resultObjects.Add(new Dictionary<string, object>()
                                {
                                    { "data", group },
                                    { "Efficiency", group.GetEfficiency() },
                                    { "URL", group.GetURL() },
                                });
                            });

                            // Сортируем сообщества по статусу закрытости, а потом по эффективости.
                            // Сначала идут сообщества с которых было получено больше всего пользователей
                            resultObjects = resultObjects
                                .OrderByDescending(_ => (_["data"] as Database.Group).IsClosed
                                                     && (_["data"] as Database.Group).IsMember)
                                .ThenByDescending(_ => (int)_["Efficiency"])
                                .ThenByDescending(_ => (_["data"] as Database.Group).LastActivity)
                                .ThenByDescending(_ => (_["data"] as Database.Group).LastScanned)
                                .ToList()
                            ;

                            handled = true;
                            break;

                        // Получение списка пользователей
                        case "users":
                            var onlyFavorites = query.Get("favorites") == "true";

                            // Получаем настройки для того чтобы проверять город в дальнейшем
                            Database.Instance.For<Database.Settings>(Database.INTERNAL_DB_MARKER, (settings) =>
                            {
                                // Составляем список стоп слов
                                var stopWords = settings.StopWords.Split(';');

                                // Функция для проверки было ли найдено какое-то стоп слово в строке
                                CallbackWithReturn<bool, string> IsAnyOfStopWordsFound = (target) =>
                                {
                                    foreach (var stopWord in stopWords)
                                    {
                                        if (target.Contains(stopWord))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                };

                                // Обходим всех пользователей
                                Database.Instance.ForEach<Database.User>((user) =>
                                {
                                    // Не показывать пользователей не в избранном, если загружаем только избранных
                                    if (onlyFavorites)
                                    {
                                        if (!user.IsFavorite)
                                        {
                                            return;
                                        }
                                    }

                                    if (!onlyFavorites)
                                    {
                                        // Не показывать скрытых по каким-либо причинам пользователей
                                        if (user.IsHidden != Database.HiddenStatus.NOT_HIDDEN)
                                        {
                                            return;
                                        }

                                        // Не показываем старых пользователей
                                        if (DateTime.Now - user.LastActivity > Constants.MAX_SCANNING_DEPTH_IN_TIME)
                                        {
                                            return;
                                        }
                                    }

                                    // Заменяем ссылку на фото, если нужно
                                    user.PhotoURL = Utils.FixPhotoURL(user.PhotoURL);

                                    // Получаем список активностей пользователя
                                    var userActivites = Database.Instance.GetAllWhere<Database.UserActivity>(_ => _.UserId == user.Id);

                                    // Получаем список недавних записей пользователя
                                    var recentPostActivities = userActivites
                                        // Ищем только посты
                                        .Where(_ => _.Type.IsOneOf(Database.UserActivity.ActivityType.POST, Database.UserActivity.ActivityType.COMMENT))
                                        // Сортируем по давности
                                        .OrderByDescending(_ => _.WhenHappened)
                                        // Берём несколько
                                        .Take(5)
                                        // Трансформируем их в новый класс, который поддерживает хранение поля с содержимым
                                        .Select(_ => new Database.UserActivityWithContent(_))
                                    ;

                                    // Получаем список недавних лайков пользователя
                                    var recentLikeActivities = userActivites
                                        // Ищем только посты
                                        .Where(_ => _.IsLikeToSomething())
                                        // Сортируем по давности
                                        .OrderByDescending(_ => _.WhenHappened)
                                        // Берём несколько
                                        .Take(3)
                                        // Трансформируем их в новый класс, который поддерживает хранение поля с содержимым
                                        .Select(_ => new Database.UserActivityWithContent(_))
                                    ;

                                    // Проверяем не было ли в записях и комментариях слов из стоп листа
                                    var isStopWordsFound = userActivites
                                        // Ищем только записи или комментарии
                                        .Where(_ => _.IsPostOrComment())
                                        // Трансформируем их в новый класс, который поддерживает хранение поля с содержимым
                                        .Select(_ => new Database.UserActivityWithContent(_))
                                        // Проверяем на вхождение стоп слов
                                        .FirstOrDefault(_ => IsAnyOfStopWordsFound(_.Post.Content))
                                        // Была ли найдена хотя бы одна такая активность?
                                        != null
                                    ;

                                    // Добавляем пользователя в ответ
                                    resultObjects.Add(new Dictionary<string, object>()
                                    {
                                        { "data", user },
                                        { "URL", user.GetURL() },
                                        { "Likes", userActivites.Count(_ => _.Type == Database.UserActivity.ActivityType.LIKE) },
                                        { "CommentLikes", userActivites.Count(_ => _.Type == Database.UserActivity.ActivityType.COMMENT_LIKE) },
                                        { "Posts", userActivites.Count(_ => _.Type == Database.UserActivity.ActivityType.POST) },
                                        { "Comments", userActivites.Count(_ => _.Type == Database.UserActivity.ActivityType.COMMENT) },
                                        { "RecentPosts", recentPostActivities },
                                        { "RecentLikes", recentLikeActivities },
                                        { "IsDifferentCity", (user.CityId != Constants.UNKNOWN_CITY_ID) && (user.CityId != settings.CityId) },
                                        { "IsStopWordsFound", isStopWordsFound },
                                    });
                                });
                            });

                            // Сортируем пользователей по их последней активноси
                            resultObjects = resultObjects
                                .OrderByDescending(_ => (_["data"] as Database.User).LastActivity)
                                .ToList()
                            ;

                            handled = true;
                            break;
                    }

                    // Запрос обработан?
                    if (handled)
                    {
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