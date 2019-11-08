using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkNet;
using System.Linq;
using VkNet.Model;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;
using System.Text.RegularExpressions;

namespace VK_Unicorn
{
    class Worker
    {
        public static Worker Instance { get; private set; }

        // Ссылка на API ВКонтакте
        VkApi api;

        // Авторизированы ли ВКонтакте. Если нет, то будем пытаться авторизироваться заново
        bool isAuthorized;

        // Произошла какая-то фатальная ошибка. Ничего не делаем
        bool inFatalErrorState;

        // Количество ошибок. Если слишком много, то перестаём что-то делать
        int errorsCount;

        public Worker()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        async public void RunMainThread()
        {
            // Ждём чуть-чуть пока не появится главное окно программы
            await Task.Delay(TimeSpan.FromSeconds(0.1f));

            // Если настройки ещё не установлены, то показываем окно настроек сразу же после запуска программы
            if (!Database.Instance.IsSettingsValid())
            {
                MainForm.Instance.OpenSettingsWindow();
            }

            // Создаём класс VkApi для дальнейшей работы
            api = new VkApi();
            api.OnTokenExpires += (sender) =>
            {
                isAuthorized = false;

                Utils.Log("Токен авторизации стал недействительным. Будет необходимо авторизироваться заново", LogLevel.WARNING);
            };

            // Запускаем основной поток выполнения. Тут определяем что в данный момент нужно делать и делаем это
            while (true)
            {
                // Текущая задача, если есть
                Func<Task> currentTask = null;

                // Фатальная ошибка. Ничего не делаем вообще
                if (inFatalErrorState)
                {
                    currentTask = async () => { await WaitAlotAfterError(); };
                }

                // Если программа успешно настроена, то кэшируем эти настройки и используем их для выполнения текущей задачи
                if (Database.Instance.IsSettingsValid())
                {
                    Database.Instance.For<Database.Settings>(Database.INTERNAL_DB_MARKER, (settings) =>
                    {
                        // Готовим список задач, которые вообще можно делать. Задачи будут проверяться в порядке их объявления
                        var possibleTaskConditions = new List<Callback>()
                        {
                            // Временный таск для разработки. Мешает выполнению методов, требующих авторизацию
                            () =>
                            {
                                //currentTask = async () => { await WaitAndSlack(); };
                            },

                            // Проверяем, авторизированы ли мы вообще. Если нет, то авторизируемся
                            // Все задачи ниже требуют того, чтобы пользователь был залогинен
                            () =>
                            {
                                if (!isAuthorized)
                                {
                                    currentTask = async () => { await AuthorizationTask(settings); };
                                }
                            },

                            // Проверяем нужно ли получить основную информацию о каких-либо сообществах, которые добавил пользователь
                            () =>
                            {
                                var groupsToReceiveInfo = Database.Instance.Take<Database.GroupToReceiveInfo>(VkLimits.GROUPS_GETBYID_GROUP_IDS);
                                if (groupsToReceiveInfo.Count > 0)
                                {
                                    currentTask = async () => { await GetGroupsInfoTask(groupsToReceiveInfo); };
                                }
                            },

                            // Ищем сообщества в которые можно подать заявки. Если такие есть, то подаём заявку
                            () =>
                            {
                                Database.Instance.ForFirstInteractableWantToJoinGroup((group) =>
                                {
                                    currentTask = async () => { await JoinClosedGroupTask(group); };
                                });
                            },

                            // Ищём сообщество которое можно просканировать и сканируем его
                            () =>
                            {
                                Database.Instance.ForBestGroupToInteract((group) =>
                                {
                                    currentTask = async () => { await ScanGroupTask(group); };
                                });
                            },

                            // Нечего больше делать, просто ждём. Другие задачи могут появиться позже
                            () =>
                            {
                                currentTask = async () => { await WaitAndSlack(); };
                            },
                        };

                        // Менеджер обходит каждую задачу, проверяет нужно ли её делать и берёт её в работу, если нету других
                        foreach (var possibleTaskCondition in possibleTaskConditions)
                        {
                            // Уже нашли что делать? Прекращаем искать задачу
                            if (currentTask != null)
                            {
                                break;
                            }

                            // Проверяем, нужно ли взять эту задачу
                            possibleTaskCondition();
                        }
                    });
                }
                else
                {
                    // Устанавливаем статус ошибки, чтобы знать что программа ещё не работает из-за неправильных настроек
                    MainForm.Instance.SetStatus("не настроено", StatusType.ERROR);
                }

                // Выполняем текущую задачу, если она есть
                if (currentTask != null)
                {
                    await currentTask();
                }

                // Ждём минимальное время
                await WaitMinimumTimeout();
            }
        }

        async Task AuthorizationTask(Database.Settings settings)
        {
            try
            {
                isAuthorized = false;

                MainForm.Instance.SetStatus("авторизация...", StatusType.GENERAL);

                Utils.Log("Авторизируемся в ВКонтакте", LogLevel.GENERAL);

                await api.AuthorizeAsync(new ApiAuthParams
                {
                    ApplicationId = (ulong)settings.ApplicationId,
                    Login = settings.Login.Trim(),
                    Password = settings.Password.Trim(),
                    Settings = Settings.Groups
                });

                isAuthorized = true;

                var apiTokenShort = api.Token.Length > 8 ? api.Token.Substring(0, 4) + "..." + api.Token.Substring(api.Token.Length - 4) : api.Token;
                Utils.Log("Авторизация прошла успешно. Токен авторизации: " + apiTokenShort, LogLevel.SUCCESS);

                MainForm.Instance.SetStatus("успешная авторизация", StatusType.SUCCESS);
            }
            catch (Exception ex)
            {
                Utils.Log("не удалось авторизироваться. Причина: " + ex.Message, LogLevel.ERROR);
                await WaitAlotAfterError();
            }
        }

        async Task GetGroupsInfoTask(IEnumerable<Database.GroupToReceiveInfo> groupsToReceiveInfo)
        {
            try
            {
                var groupIds = new List<string>();
                foreach (var group in groupsToReceiveInfo)
                {
                    groupIds.Add(group.DomainName);
                }

                if (groupIds.Count > 0)
                {
                    MainForm.Instance.SetStatus("получаем информацию о сообществах", StatusType.GENERAL);

                    Utils.Log("Получаем информацию о сообществах " + groupIds.GenerateSeparatedString(", "), LogLevel.GENERAL);

                    var groupsInfo = await api.Groups.GetByIdAsync(groupIds, null, null);
                    if (groupsInfo != null)
                    {
                        Utils.Log("Информация о сообществах успешно получена", LogLevel.SUCCESS);

                        foreach (var groupInfo in groupsInfo)
                        {
                            // Сообщество не активно? Удалено, не создано, заблокировано?
                            if (groupInfo.Deactivated != null)
                            {
                                if (groupInfo.Deactivated != Deactivated.Activated)
                                {
                                    Utils.Log("Не добавляем сообщество " + groupInfo.GetURL() + " потому что оно удалено", LogLevel.NOTIFY);
                                }
                            }

                            // Сообщество не было уже добавлено ранее?
                            if (!Database.Instance.IsAlreadyExists<Database.Group>(groupInfo.Id))
                            {
                                // Готовим новое сообщество
                                var newGroup = new Database.Group()
                                {
                                    Id = groupInfo.Id,
                                    Name = groupInfo.Name,
                                    ScreenName = groupInfo.ScreenName,
                                    IsClosed = groupInfo.IsClosed.HasValue ? groupInfo.IsClosed == VkNet.Enums.GroupPublicity.Closed : false,
                                    IsMember = groupInfo.IsMember.GetValueOrDefault(true),
                                    PhotoURL = groupInfo.Photo200.ToString(),
                                };

                                // Добавляем сообщество в базу данных
                                Database.Instance.InsertOrReplace(newGroup);
                            }
                            else
                            {
                                Utils.Log("Не добавляем сообщество " + groupInfo.ScreenName + " " + groupInfo.GetURL() + " потому что оно уже было добавлено", LogLevel.NOTIFY);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("ничего не получено в ответ");
                    }

                    // Мы получили информацию о всех нужных сообществах, удаляем их из очереди на обработку
                    foreach (var group in groupsToReceiveInfo)
                    {
                        Database.Instance.Delete(group);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.Log("не удалось получить информацию о сообществах. Причина: " + ex.Message, LogLevel.ERROR);
                await WaitAlotAfterError();
            }
        }

        async Task JoinClosedGroupTask(Database.Group group)
        {
            try
            {
                MainForm.Instance.SetStatus("присоединяемся к сообществу", StatusType.GENERAL);

                Utils.Log("Определяем, нужно ли присоединиться к сообществу " + group.Name, LogLevel.GENERAL);

                // Получаем информацию о сообществе. Может мы уже присоединились к нему? Поле member_status нельзя получить через обычный запрос
                var response = await api.CallAsync("groups.getById", new VkNet.Utils.VkParameters()
                {
                    { "group_id", group.ScreenName },
                    { "fields", "member_status" },
                });
                if (response != null)
                {
                    var groupInfoAsResponse = ((VkNet.Utils.VkResponseArray)response).FirstOrDefault();
                    if (groupInfoAsResponse != null)
                    {
                        var groupInfo = (VkNet.Model.Group)groupInfoAsResponse;
                        if (groupInfo != null)
                        {
                            Utils.Log("    статус участия в сообществе: " + groupInfo.MemberStatus, LogLevel.NOTIFY);

                            // Обновляем данные о закрытости и членстве в сообществе
                            group.IsClosed = groupInfo.IsClosed.HasValue ? groupInfo.IsClosed == VkNet.Enums.GroupPublicity.Closed : group.IsClosed;
                            group.IsMember = groupInfo.IsMember.GetValueOrDefault(group.IsMember);

                            // Всё ещё закрытое сообщество и не вступили?
                            if (group.IsWantToJoin())
                            {
                                switch (groupInfo.MemberStatus)
                                {
                                    case VkNet.Enums.MemberStatus.SendRequest:
                                        // За прошлые пять минут заявку всё ещё не приняли. Похоже заявки
                                        // принимает человек, а не бот, поэтому ждём значительно дольше
                                        // прежде чем проверять это сообщество снова
                                        group.SetInteractTimeout(Timeouts.AFTER_GROUP_JOIN_REQUEST_NOT_ACCEPTED);

                                        Utils.Log("Заявка на вступление в сообщество " + group.Name + " была уже отправлена, но ещё не принята. Ждём значительно дольше", LogLevel.NOTIFY);
                                        break;

                                    case VkNet.Enums.MemberStatus.Rejected:
                                        // Заявку на вступление отклонили? Удаляем сообщество из списка для оработки
                                        Utils.Log("Заявка на вступление в сообщество " + group.Name + " " + group.GetURL() + " была отклонена. Удаляем сообщество", LogLevel.WARNING);

                                        Database.Instance.Delete(group);
                                        break;

                                    default:
                                        Utils.Log("Отправляем заявку на вступление в " + group.Name, LogLevel.GENERAL);
                                        // Добавляем таймаут в пять минут для взаимодействия с сообществом
                                        // обычно за это время бот автоматически принимает заявку на вступление
                                        group.SetInteractTimeout(Timeouts.AFTER_GROUP_JOIN_REQUEST_SENT);

                                        // Отправляем заявку на вступление
                                        var result = await api.Groups.JoinAsync(group.Id);
                                        break;
                                }
                            }
                            else
                            {
                                Utils.Log("    присоединяться не нужно", LogLevel.NOTIFY);
                            }

                            // Обновляем сообщество в базе данных
                            Database.Instance.InsertOrReplace(group);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.Log("не удалось отправить заявку на вступление в сообщество " + group.Name + ". Причина: " + ex.Message, LogLevel.ERROR);
                await WaitAlotAfterError();
            }
        }

        async Task ScanGroupTask(Database.Group group)
        {
            try
            {
                MainForm.Instance.SetStatus("сканируем сообщество", StatusType.GENERAL);

                // Список интересующей нас активности пользователей
                var userActivitiesToProcess = new List<Database.UserActivity>();

                var scanOffset = 0ul;
                var needToLoadMorePosts = true;

                while (needToLoadMorePosts)
                {
                    needToLoadMorePosts = false;

                    Utils.Log("Сканируем сообщество " + group.Name + " с позиции " + scanOffset, LogLevel.GENERAL);

                    try
                    {
                        // Загружаем сообщения из сообщества
                        // Максимум 5000 запросов в сутки https://vk.com/dev/data_limits
                        // каждый запрос ценен и нужно получить как можно больше информации сразу,
                        // поэтому нет смысла получать меньше записей чем VkLimits.WALL_GET_COUNT
                        var postsLimit = 5ul; // VkLimits.WALL_GET_COUNT;
                        var wallGetObjects = await api.Wall.GetAsync(new WallGetParams()
                        {
                            OwnerId = group.GetNegativeId(),
                            Count = postsLimit,
                            Offset = scanOffset,
                        });
                        await WaitMinimumTimeout();

                        // Начинаем работу с записями
                        var posts = wallGetObjects.WallPosts;

                        // Нету больше записей. Сканирование сообщества завершено
                        if (posts.Count <= 0)
                        {
                            break;
                        }

                        // Запоминаем видели ли последнюю запись ранее
                        var isLastPostNotSeenBefore = false;

                        // Обходим все записи
                        foreach (var post in posts)
                        {
                            // Нужно ли вообще сканировать запись?
                            var needToScanPost = true;
                            var isPostNotSeenBefore = true;

                            // Ищем запись в нашей базе
                            Database.Instance.ForScannedPostInfo(group.Id, post.Id.GetValueOrDefault(), (scannedPost) =>
                            {
                                // Уже видели эту запись когда-то
                                isLastPostNotSeenBefore = false;
                                isPostNotSeenBefore = false;

                                // Нужно сканировать запись повторно?
                                needToScanPost = (post.Comments.Count > scannedPost.CommentsCount) || (post.Likes.Count > scannedPost.LikesCount);
                            });

                            if (needToScanPost)
                            {
                                // Кто был фактическим автором записи? Пользователь или сообщество? Ищем наиболее подходящий Id
                                var postAuthorId = post.SignerId.GetValueOrDefault(post.FromId.GetValueOrDefault());

                                // Запись была не анонимной? (Не от сообщества?)
                                if (postAuthorId > 0)
                                {
                                    // Не видели эту запись раньше?
                                    if (isPostNotSeenBefore)
                                    {
                                        // Добавляем автора записи для дальнейшей обработки
                                        userActivitiesToProcess.Add(new Database.UserActivity()
                                        {
                                            UserId = postAuthorId,
                                            Type = Database.UserActivity.ActivityType.POST,
                                            Content = post.Text,
                                            PostId = post.Id.GetValueOrDefault(),
                                            WhenAdded = post.Date.GetValueOrDefault(),
                                        });
                                    }
                                }

                                // Сканируем лайки
                                if (post.Likes.Count > 0)
                                {

                                }

                                // Сканируем комментарии
                                if (post.Comments.Count > 0)
                                {

                                    // Сканируем лайки к комментариям
                                }
                            }
                        }

                        // Было возвращено записей не меньше чем мы запросили?
                        // Это значит что можно загрузить ещё записи при необходимости
                        if ((ulong)posts.Count >= postsLimit)
                        {
                            // Последняя запись была новая для нас?
                            if (isLastPostNotSeenBefore)
                            {
                                // Последняя запись не слишком старая?
                                if ((DateTime.Now - posts.Last().Date) > Constants.MAX_SCANNING_DEPTH_IN_TIME)
                                {
                                    // Увеличиваем отступ с которого будем продолжать сканирование
                                    scanOffset += postsLimit;

                                    // Нужно загрузить ещё записи
                                    needToLoadMorePosts = true;

                                    // DEBUG Для отладки
                                    Utils.Log("хочу загрузить ещё", LogLevel.GENERAL);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.Log("не удалось получить записи. Причина: " + ex.Message, LogLevel.ERROR);
                        await WaitAlotAfterError();
                    }

                    // DEBUG Для отладки
                    needToLoadMorePosts = false;
                }

                // Составляем список тех пользователей, о которых нужно получить информацию
                var userIdsToReceiveInfo = new List<long>();

                // Сначала добавляем все возможные Id пользователей в общий список
                foreach (var userActivityToProcess in userActivitiesToProcess)
                {
                    userIdsToReceiveInfo.Add(userActivityToProcess.UserId);
                }

                // Удаляем дубликаты
                userIdsToReceiveInfo = userIdsToReceiveInfo.Distinct().ToList();

                // Удаляем тех пользователей, которых мы уже просканировали
                userIdsToReceiveInfo.RemoveAll(_ => Database.Instance.IsAlreadyExists<Database.ScannedUser>(_));

                // Загружаем информацию о нужных пользователях
                var users = new List<User>();
                while (userIdsToReceiveInfo.Count > 0)
                {
                    // Берём максимальное количество Id, которое мы можем просканировать за один запрос
                    var chunkOfUserIdsToReceiveInfo = userIdsToReceiveInfo.Take(VkLimits.USERS_GET_USER_IDS).ToList();

                    // Удаляем то количество Id, которые мы взяли для получения
                    userIdsToReceiveInfo.RemoveRange(0, chunkOfUserIdsToReceiveInfo.Count);

                    // Получаем информацию о этих пользователях
                    users.AddRange(await api.Users.GetAsync(chunkOfUserIdsToReceiveInfo));
                }

                // Обрабатываем интересующие нас активности: записи, лайки, комментарии и т.п.
                while (userActivitiesToProcess.Count > 0)
                {
                    // Обрабатываем первый найденный элемент
                    var userActivityToProcess = userActivitiesToProcess.First();

                    // DEBUG Выводим отладочную информацию о активности
                    Utils.Log("Активность: " + userActivityToProcess.Type, LogLevel.NOTIFY);
                    Utils.Log("    userId: " + Constants.VK_WEB_PAGE + "id" + userActivityToProcess.UserId, LogLevel.NOTIFY);
                    Utils.Log("    postId: " + userActivityToProcess.PostId, LogLevel.NOTIFY);
                    Utils.Log("    content: " + userActivityToProcess.Content, LogLevel.NOTIFY);
                    Utils.Log("    whenAdded: " + userActivityToProcess.WhenAdded, LogLevel.NOTIFY);

                    // Пользователь уже был добавлен ранее?
                    if (Database.Instance.IsAlreadyExists<Database.User>(userActivityToProcess.UserId))
                    {
                        // Сохраняем эту активность как новую активность пользователя
                    }
                    else
                    {
                        // Пользователь нам не известен, сканируем пользователя

                    }

                    // Удаляем обработанную активность из списка обработки
                    userActivitiesToProcess.Remove(userActivityToProcess);
                }

                // Помечаем сообщество как только что просканированное
                group.MarkAsJustScanned();

                // Устанавливаем время ожидания перед следующим сканированием
                //group.SetInteractTimeout(Timeouts.AFTER_GROUP_WAS_SCANNED);
            }
            catch (Exception ex)
            {
                Utils.Log("не удалось просканировать сообщество " + group.Name + ". Причина: " + ex.Message, LogLevel.ERROR);
                await WaitAlotAfterError();
            }

            // DEBUG Для отладки
            inFatalErrorState = true;
        }

        async Task WaitAndSlack()
        {
            MainForm.Instance.SetStatus("ожидание" + GetProgressDots(), StatusType.SUCCESS);

            await Task.Delay(TimeSpan.FromSeconds(10d));
        }

        async Task WaitAlotAfterError()
        {
            MainForm.Instance.SetStatus("ожидание после ошибки", StatusType.ERROR);

            await Task.Delay(TimeSpan.FromSeconds(20d));
        }

        async Task WaitMinimumTimeout()
        {
            await Task.Delay(Timeouts.MINIMUM_TIMEOUT);
        }

        // Счётчик для отображения изменяющегося троеточия в процессе сканирования
        int dotsCounter = 1;
        string GetProgressDots()
        {
            if (dotsCounter > 3)
            {
                dotsCounter = 1;
            }

            return new string('.', dotsCounter++);
        }

        /// <summary>
        /// Получаем ссылку на новое сообщество, получаем из неё DomainName
        /// Добавляем сообщество в таблицу GroupToReceiveInfo
        /// </summary>
        public void RegisterNewGroupToReceiveInfo(string groupWebUrl)
        {
            // Удаляем все символы перед доменным именем
            var domainName = Regex.Replace(groupWebUrl, @".+\/", "").Trim();

            // Это сообщество начинающееся с public?
            if (Regex.Match(domainName.ToLowerInvariant(), @"public\d+$").Success)
            {
                // Заменяем слово piblic на club т.к. API ВКонтакта больше не принимает public
                domainName = domainName.ToLowerInvariant().Replace("public", "club");
            }

            try
            {
                if (!string.IsNullOrEmpty(domainName))
                {
                    var result = Database.Instance.InsertOrReplace(new Database.GroupToReceiveInfo()
                    {
                        DomainName = domainName,
                    });

                    if (result)
                    {
                        Utils.Log("Сообщество " + domainName + " успешно добавлено в очередь на начальное сканирование", LogLevel.SUCCESS);
                    }
                    else
                    {
                        throw new Exception("скорее всего сообщество уже добавлено в очередь на начальное сканирование");
                    }
                }
                else
                {
                    throw new Exception("не удалось получить имя сообщества из ссылки");
                }
            }
            catch (Exception ex)
            {
                Utils.Log("не удалось добавить сообщество " + groupWebUrl + " на сканирование. Причина: " + ex.Message, LogLevel.ERROR);
            }
        }
    }
}