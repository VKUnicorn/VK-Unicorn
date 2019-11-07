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

                // Если программа успешно настроена, то кэшируем эти настройки и используем их для выполнения текущей задачи
                if (Database.Instance.IsSettingsValid())
                {
                    Database.Instance.For<Database.Settings>(Database.INTERNAL_DB_MARKER, (settings) =>
                    {
                        // Готовим список задач, которые вообще можно делать. Задачи будут проверяться в порядке их объявления
                        var possibleTaskConditions = new List<Callback>()
                        {
                            // Ищём группу которую можно просканировать и сканируем её, если есть
                            () =>
                            {
                                Database.Instance.ForBestGroupToInteract((group) =>
                                {
                                    currentTask = async () => { await ScanGroupTask(group); };
                                });
                            },

                            // Временный таск для разработки. Мешает выполнению методов, требующих авторизацию
                            () =>
                            {
                                currentTask = async () => { await WaitAndSlack(); };
                            },

                            // Проверяем, залогинены ли мы вообще. Если нет, то добавляем задачу залогиниться
                            // Все задачи ниже требуют того, чтобы пользователь был залогинен
                            () =>
                            {
                                if (!isAuthorized)
                                {
                                    currentTask = async () => { await AuthorizationTask(settings); };
                                }
                            },

                            // Проверяем нужно ли получить основную информацию о каких-либо группах, которые добавил пользователь
                            () =>
                            {
                                var groupsToReceiveInfo = Database.Instance.Take<Database.GroupToReceiveInfo>(VkLimits.GROUPS_GETBYID_GROUP_IDS);
                                if (groupsToReceiveInfo.Count > 0)
                                {
                                    currentTask = async () => { await GetGroupsInfoTask(groupsToReceiveInfo); };
                                }
                            },

                            // Ищем группы в которые можно подать заявки. Если такие есть, то подаём заявку
                            () =>
                            {
                                Database.Instance.ForFirstInteractableWantToJoinGroup((group) =>
                                {
                                    currentTask = async () => { await JoinClosedGroupTask(group); };
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
                    MainForm.Instance.SetStatus("получаем информацию о группах", StatusType.GENERAL);

                    Utils.Log("Получаем информацию о группах " + groupIds.GenerateSeparatedString(", "), LogLevel.GENERAL);

                    var groupsInfo = await api.Groups.GetByIdAsync(groupIds, null, null);
                    if (groupsInfo != null)
                    {
                        Utils.Log("Информация о группах успешно получена", LogLevel.SUCCESS);

                        foreach (var groupInfo in groupsInfo)
                        {
                            // Группа не активна? Удалена, не создана, заблокирована?
                            if (groupInfo.Deactivated != null)
                            {
                                if (groupInfo.Deactivated != Deactivated.Activated)
                                {
                                    Utils.Log("Не добавляем группу " + groupInfo.GetURL() + " потому что она удалена", LogLevel.NOTIFY);
                                }
                            }

                            // Группа не была уже добавлена ранее?
                            if (!Database.Instance.IsAlreadyExists<Database.Group>(groupInfo.Id))
                            {
                                // Готовим новую группу
                                var newGroup = new Database.Group()
                                {
                                    Id = groupInfo.Id,
                                    Name = groupInfo.Name,
                                    ScreenName = groupInfo.ScreenName,
                                    IsClosed = groupInfo.IsClosed.HasValue ? groupInfo.IsClosed == VkNet.Enums.GroupPublicity.Closed : false,
                                    IsMember = groupInfo.IsMember.HasValue ? groupInfo.IsMember.Value : true,
                                    PhotoURL = groupInfo.Photo200.ToString(),
                                };

                                // Добавляем группу в базу данных
                                Database.Instance.InsertOrReplace(newGroup);
                            }
                            else
                            {
                                Utils.Log("Не добавляем группу " + groupInfo.ScreenName + " " + groupInfo.GetURL() + " потому что она уже была добавлена", LogLevel.NOTIFY);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("ничего не получено в ответ");
                    }

                    // Мы получили информацию о всех нужных группах, удаляем их из очереди на обработку
                    foreach (var group in groupsToReceiveInfo)
                    {
                        Database.Instance.Delete(group);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.Log("не удалось получить информацию о группах. Причина: " + ex.Message, LogLevel.ERROR);
                await WaitAlotAfterError();
            }
        }

        async Task JoinClosedGroupTask(Database.Group group)
        {
            try
            {
                MainForm.Instance.SetStatus("присоединяемся к группе", StatusType.GENERAL);

                Utils.Log("Определяем, нужно ли присоединиться к группе " + group.Name, LogLevel.GENERAL);

                // Получаем информацию о группе. Может мы уже присоединились к ней? Поле member_status нельзя получить через обычный запрос
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
                            Utils.Log("    статус участия в группе: " + groupInfo.MemberStatus, LogLevel.NOTIFY);

                            // Обновляем данные о закрытости и членстве в группе
                            group.IsClosed = groupInfo.IsClosed.HasValue ? groupInfo.IsClosed == VkNet.Enums.GroupPublicity.Closed : group.IsClosed;
                            group.IsMember = groupInfo.IsMember.HasValue ? groupInfo.IsMember.Value : group.IsMember;

                            // Всё ещё закрытая группа и не вступили?
                            if (group.IsWantToJoin())
                            {
                                switch (groupInfo.MemberStatus)
                                {
                                    case VkNet.Enums.MemberStatus.SendRequest:
                                        // За прошлые пять минут заявку всё ещё не приняли. Похоже заявки
                                        // принимает человек, а не бот, поэтому ждём значительно дольше
                                        // прежде чем проверять эту группу снова
                                        group.SetInteractTimeout(Timeouts.AFTER_GROUP_JOIN_REQUEST_NOT_ACCEPTED);

                                        Utils.Log("Заявка на вступление в " + group.Name + " была уже отправлена, но ещё не принята. Ждём значительно дольше", LogLevel.WARNING);
                                        break;

                                    case VkNet.Enums.MemberStatus.Rejected:
                                        // Заявку на вступление отклонили? Удаляем группу из списка для оработки
                                        Utils.Log("Заявка на вступление в группу " + group.Name + " " + group.GetURL() + " была отклонена. Удаляем группу", LogLevel.WARNING);

                                        Database.Instance.Delete(group);
                                        break;

                                    default:
                                        Utils.Log("Отправляем заявку на вступление в " + group.Name, LogLevel.GENERAL);
                                        // Добавляем таймаут в пять минут для взаимодействия с группой
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

                            // Обновляем группу в базе данных
                            Database.Instance.InsertOrReplace(group);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.Log("не удалось отправить заявку на вступление в группу " + group.Name + ". Причина: " + ex.Message, LogLevel.ERROR);
                await WaitAlotAfterError();
            }
        }

        class ProfileActivity
        {
            public enum Type
            {
                POST,
                LIKE,
                COMMENT,
                COMMENT_LIKE,
            }

            public long ProfileId;
            public Type type;

            public string content;

            public long PostId;
            public long? CommentId;
        }
        async Task ScanGroupTask(Database.Group group)
        {
            try
            {
                MainForm.Instance.SetStatus("сканируем группу", StatusType.GENERAL);

                // Список интересующей нас активности профилей
                var profileActivities = new List<ProfileActivity>();

                var scanOffset = 0ul;
                var needToLoadMorePosts = true;

                while (needToLoadMorePosts)
                {
                    needToLoadMorePosts = false;

                    Utils.Log("Сканируем группу " + group.Name + " с позиции " + scanOffset, LogLevel.GENERAL);

                    try
                    {
                        // Загружаем сообщения из группы
                        // Максимум 5000 запросов в сутки https://vk.com/dev/data_limits
                        // поэтому нет смысла получать меньше записей чем лимит VkLimits.WALL_GET_COUNT
                        // т.к. каждый запрос ценен и нужно получить как можно больше информации сразу
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

                        // Нету больше записей. Сканирование группы завершено
                        if (posts.Count <= 0)
                        {
                            break;
                        }

                        var isLastPostNotSeenBefore = false;

                        // Обходим все записи
                        foreach (var post in posts)
                        {
                            // Нужно ли вообще сканировать запись?
                            var needToScanPost = true;

                            // Ищем запись в нашей базе
                            Database.Instance.ForScannedPostInfo(group.Id, post.Id.Value, (scannedPost) =>
                            {
                                // Уже видели эту запись когда-то
                                isLastPostNotSeenBefore = false;

                                // Нужно сканировать запись повторно?
                                needToScanPost = (post.Comments.Count > scannedPost.CommentsCount) || (post.Likes.Count > scannedPost.LikesCount);
                            });

                            if (needToScanPost)
                            {
                                // Добавляем автора записи для дальнейшей обработки
                                profileActivities.Add(new ProfileActivity()
                                {
                                    ProfileId = post.FromId.Value,
                                    type = ProfileActivity.Type.POST,
                                    content = post.Text,
                                    PostId = post.Id.Value,
                                });

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
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.Log("не удалось получить записи. Причина: " + ex.Message, LogLevel.ERROR);
                        await WaitAlotAfterError();
                    }

                    // Для дебага
                    needToLoadMorePosts = false;
                }

                // Обрабатываем интересующие нас записи, лайки, комментарии и т.п.

                // Помечаем группу как только что просканированную
                group.MarkAsJustScanned();

                // Устанавливаем время ожидания перед следующим сканированием
                group.SetInteractTimeout(Timeouts.AFTER_GROUP_WAS_SCANNED);
            }
            catch (Exception ex)
            {
                Utils.Log("не удалось просканировать группу " + group.Name + ". Причина: " + ex.Message, LogLevel.ERROR);
                await WaitAlotAfterError();
            }
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
            // Ждём некоторое время. Торопиться некуда, лучше сканировать медленно, но зато без угрозы бана аккаунта
            // Слишком частые запросы это плохо, о лимитах можно почитать тут https://vk.com/dev/api_requests
            // в разделе "3. Ограничения и рекомендации". В целом рекомендуется обращаться не чаще трёх раз в секунду,
            // но мы будем сканировать значительно реже, опять же чтобы снизить угрозу бана аккаунта или появления капчи
            await Task.Delay(TimeSpan.FromSeconds(1.5d));
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
        /// Получаем ссылку на новую группу, получаем из неё DomainName
        /// Добавляем группу в таблицу GroupToAdd
        /// </summary>
        public void RegisterNewGroupToAdd(string groupWebUrl)
        {
            // Удаляем все символы перед доменным именем
            var domainName = Regex.Replace(groupWebUrl, @".+\/", "").Trim();

            // Это группа начинающаяся с public?
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
                        Utils.Log("Группа " + domainName + " успешно добавлена в очередь на начальное сканирование", LogLevel.SUCCESS);
                    }
                    else
                    {
                        throw new Exception("скорее всего группа уже добавлена в очередь на начальное сканирование");
                    }
                }
                else
                {
                    throw new Exception("не удалось получить имя группы из ссылки");
                }
            }
            catch (Exception ex)
            {
                Utils.Log("не удалось добавить группу " + groupWebUrl + " на сканирование. Причина: " + ex.Message, LogLevel.ERROR);
            }
        }
    }
}