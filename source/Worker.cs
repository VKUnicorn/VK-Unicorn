using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VkNet;
using VkNet.Utils;
using VkNet.Model;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;
using Newtonsoft.Json;

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
                            // Временная задача-заглушка для разработки. Мешает выполнению методов, требующих авторизацию
                            () =>
                            {
                                currentTask = async () => { await WaitAndSlack(); };
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

                            // Проверяем нужно ли получить основную информацию о каких-либо сообществах, которые мы добавили
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
                                    currentTask = async () => { await ScanGroupTask(settings, group); };
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

                // Ждём минимальное время в любом случае
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
                                    Utils.Log("Не добавляем сообщество " + groupInfo.GetURL() + " потому что оно удалено или заблокировано", LogLevel.NOTIFY);
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

                // Удаляем все сообщества из очереди на обработку потому что там есть какая-то ошибка
                foreach (var group in groupsToReceiveInfo)
                {
                    Database.Instance.Delete(group);
                }

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
                var response = await api.CallAsync("groups.getById", new VkParameters()
                {
                    { "group_id", group.ScreenName },
                    { "fields", "member_status" },
                });
                if (response != null)
                {
                    var groupInfoAsResponse = ((VkResponseArray)response).FirstOrDefault();
                    if (groupInfoAsResponse != null)
                    {
                        var groupInfo = (VkNet.Model.Group)groupInfoAsResponse;
                        Utils.Log("Статус участия в сообществе: " + groupInfo.MemberStatus, LogLevel.NOTIFY);

                        // Обновляем данные о закрытости и членстве в сообществе
                        group.IsClosed = groupInfo.IsClosed.HasValue ? groupInfo.IsClosed == VkNet.Enums.GroupPublicity.Closed : group.IsClosed;
                        group.IsMember = groupInfo.IsMember.GetValueOrDefault(group.IsMember);

                        // Обновляем новую информацию о сообществе в базе данных
                        Database.Instance.InsertOrReplace(group);

                        // Всё ещё закрытое сообщество и не вступили?
                        if (group.IsWantToJoin())
                        {
                            switch (groupInfo.MemberStatus)
                            {
                                case VkNet.Enums.MemberStatus.SendRequest:
                                    // За прошлые пять минут заявку всё ещё не приняли. Похоже заявки принимает человек, а не бот
                                    Utils.Log("Заявка на вступление в сообщество " + group.Name + " была уже отправлена, но ещё не принята. Ждём значительно дольше", LogLevel.NOTIFY);

                                    // Ждём значительно дольше прежде чем проверять это сообщество снова
                                    group.SetInteractTimeout(Timeouts.AFTER_GROUP_JOIN_REQUEST_NOT_ACCEPTED);
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
                            Utils.Log("Присоединяться не нужно", LogLevel.NOTIFY);
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

        async Task ScanGroupTask(Database.Settings settings, Database.Group group)
        {
            try
            {
                MainForm.Instance.SetStatus("сканируем сообщество", StatusType.GENERAL);

                // Список интересующей нас активности пользователей
                var userActivitiesToProcess = new List<Database.UserActivity>();

                // Список активностей, к которым нужно получить лайки через execute запросы
                var activitiesToReceiveLikesByExecute = new List<Database.UserActivity>();

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
                        var postsLimit = VkLimits.WALL_GET_COUNT;
                        var wallGetObjects = await api.Wall.GetAsync(new WallGetParams()
                        {
                            OwnerId = group.GetNegativeIdForAPI(),
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
                        var isLastPostNotSeenBefore = true;

                        // Обходим все записи
                        foreach (var post in posts)
                        {
                            // Ограничиваем количество лайков которые "видны" нам в дальнейшем
                            post.Likes.Count = Math.Min(post.Likes.Count, Constants.MAX_LIKES_TO_SCAN);
                            // Ограничиваем количество комментариев которые "видны" нам в дальнейшем
                            post.Comments.Count = Math.Min(post.Comments.Count, Constants.MAX_COMMENTS_TO_SCAN);

                            // Нужно ли вообще сканировать запись?
                            var needToScanPost = true;
                            var isPostNotSeenBefore = true;
                            var savedPostContent = string.Empty;
                            var savedPostAttachments = string.Empty;

                            // Ищем запись в нашей базе
                            Database.Instance.For<Database.Post>(Database.Post.MakeId(group.Id, post.Id.GetValueOrDefault()), (scannedPost) =>
                            {
                                // Уже видели эту запись когда-то
                                isLastPostNotSeenBefore = false;
                                isPostNotSeenBefore = false;

                                // Сохраняем содержимое и вложения записи, чтобы потом их не перезатереть.
                                // Нас интересует первоначальный вариант, до любых редактирований или удалений
                                savedPostContent = scannedPost.Content;
                                savedPostAttachments = scannedPost.Attachments;

                                // Нужно сканировать запись повторно?
                                needToScanPost = (post.Comments.Count > scannedPost.CommentsCount) || (post.Likes.Count > scannedPost.LikesCount);
                            });

                            if (needToScanPost)
                            {
                                // Кто был фактическим автором записи? Пользователь или сообщество? Ищем наиболее подходящий Id
                                var postAuthorId = post.SignerId.GetValueOrDefault(post.FromId.GetValueOrDefault());

                                // Запись была не анонимной? (Не от сообщества?)
                                if (Utils.IsProfileIdNotGroupId(postAuthorId))
                                {
                                    // Не видели эту запись раньше?
                                    if (isPostNotSeenBefore)
                                    {
                                        // Добавляем автора записи для дальнейшей обработки
                                        userActivitiesToProcess.Add(new Database.UserActivity()
                                        {
                                            UserId = postAuthorId,
                                            Type = Database.UserActivity.ActivityType.POST,
                                            PostId = post.Id.GetValueOrDefault(),
                                            GroupId = group.Id,
                                            WhenHappened = post.Date.GetValueOrDefault(),
                                        });
                                    }
                                }

                                // Сканируем лайки
                                if (post.Likes.Count > 0)
                                {
                                    // Загружаем информацию о лайках сразу или подготавливаем для execute запросов
                                    var likesToLoad = post.Likes.Count;
                                    if (likesToLoad <= VkLimits.LIKES_GET_LIST_COUNT)
                                    {
                                        // Мало лайков, сохраняем активность для execute запроса
                                        activitiesToReceiveLikesByExecute.Add(new Database.UserActivity()
                                        {
                                            Type = Database.UserActivity.ActivityType.LIKE,
                                            // UserId пока нам неизвестен и будет получен в результате execute запроса
                                            GroupId = group.Id,
                                            PostId = post.Id.GetValueOrDefault(),
                                            // Мы не можем узнать время лайка, поэтому будем считать что пользователь
                                            // проявил эту активность во время последнего сканирования, если запись уже
                                            // была просканирована ранее. В другом случае считаем что лайк был поставлен
                                            // в то же время, что и написана запись
                                            WhenHappened = isPostNotSeenBefore ? post.Date.GetValueOrDefault() : Utils.GetNowAsUniversalTime(),
                                        });
                                    }
                                    else
                                    {
                                        Utils.Log("Сканируем лайки сразу " + likesToLoad, LogLevel.GENERAL);

                                        // Очень много лайков, загружаем порциями через обычные запросы
                                        var offset = 0u;
                                        while (likesToLoad > 0)
                                        {
                                            try
                                            {
                                                // Отправляем запрос к API ВКонтакте
                                                var likesUserIds = await api.Likes.GetListAsync(new LikesGetListParams()
                                                {
                                                    Type = LikeObjectType.Post,
                                                    OwnerId = group.GetNegativeIdForAPI(),
                                                    ItemId = post.Id.GetValueOrDefault(),
                                                    Offset = offset,
                                                    Count = VkLimits.LIKES_GET_LIST_COUNT,
                                                });
                                                await WaitMinimumTimeout();

                                                foreach (var likeUserId in likesUserIds)
                                                {
                                                    // Добавляем автора лайка для дальнейшей обработки
                                                    userActivitiesToProcess.Add(new Database.UserActivity()
                                                    {
                                                        Type = Database.UserActivity.ActivityType.LIKE,
                                                        UserId = likeUserId,
                                                        GroupId = group.Id,
                                                        PostId = post.Id.GetValueOrDefault(),
                                                        // Мы не можем узнать время лайка, поэтому будем считать что пользователь
                                                        // проявил эту активность во время последнего сканирования, если запись уже
                                                        // была просканирована ранее. В другом случае считаем что лайк был поставлен
                                                        // в то же время, что и написана запись
                                                        WhenHappened = isPostNotSeenBefore ? post.Date.GetValueOrDefault() : Utils.GetNowAsUniversalTime(),
                                                    });
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Utils.Log("не удалось получить информацию о большом количестве лайков к записи " + post.Id.GetValueOrDefault() + ". Причина: " + ex.Message, LogLevel.ERROR);
                                                await WaitAlotAfterError();
                                            }

                                            // Нужно загрузить ещё больше информации о лайках?
                                            likesToLoad -= VkLimits.LIKES_GET_LIST_COUNT;
                                            if (likesToLoad > 0)
                                            {
                                                offset += VkLimits.LIKES_GET_LIST_COUNT;
                                            }
                                        }
                                    }
                                }

                                // Сканируем комментарии
                                if (post.Comments.Count > 0)
                                {
                                    try
                                    {
                                        // Отправляем запрос к API ВКонтакте
                                        var commentsResult = await api.Wall.GetCommentsAsync(new WallGetCommentsParams()
                                        {
                                            OwnerId = group.GetNegativeIdForAPI(),
                                            PostId = post.Id.GetValueOrDefault(),
                                            NeedLikes = true,
                                            Count = VkLimits.WALL_GET_COMMENTS_COUNT,
                                        });
                                        await WaitMinimumTimeout();

                                        foreach (var comment in commentsResult.Items)
                                        {
                                            // Ограничиваем количество лайков которые "видны" нам в дальнейшем
                                            comment.Likes.Count = Math.Min(comment.Likes.Count, Constants.MAX_COMMENT_LIKES_TO_SCAN);

                                            // Нужно ли вообще сканировать комментарий?
                                            var needToScanComment = true;
                                            var isCommentNotSeenBefore = true;
                                            var savedCommentContent = string.Empty;
                                            var savedCommentAttachments = string.Empty;

                                            // Ищем комментарий в нашей базе
                                            Database.Instance.For<Database.Comment>(Database.Comment.MakeId(group.Id, post.Id.GetValueOrDefault(), comment.Id), (scannedComment) =>
                                            {
                                                // Уже видели этот комментарий когда-то
                                                isCommentNotSeenBefore = false;

                                                // Сохраняем содержимое и вложения записи, чтобы потом их не перезатереть.
                                                // Нас интересует первоначальный вариант, до любых редактирований или удалений
                                                savedCommentContent = scannedComment.Content;
                                                savedCommentAttachments = scannedComment.Attachments;

                                                // Нужно сканировать комментарий повторно?
                                                needToScanComment = (comment.Likes.Count > scannedComment.LikesCount);
                                            });

                                            // Комментарий нужно было просканировать. Сохраняем новую информацию о нём или обновляем старую
                                            if (needToScanComment)
                                            {
                                                // Добавляем автора комментария для дальнейшей обработки
                                                var commentAuthorId = comment.FromId.GetValueOrDefault();
                                                if (Utils.IsProfileIdNotGroupId(commentAuthorId))
                                                {
                                                    // Не видели этот комментарий раньше?
                                                    if (isCommentNotSeenBefore)
                                                    {
                                                        userActivitiesToProcess.Add(new Database.UserActivity()
                                                        {
                                                            Type = Database.UserActivity.ActivityType.COMMENT,
                                                            UserId = commentAuthorId,
                                                            GroupId = group.Id,
                                                            PostId = post.Id.GetValueOrDefault(),
                                                            CommentId = comment.Id,
                                                            WhenHappened = comment.Date.GetValueOrDefault(),
                                                        });
                                                    }
                                                }

                                                // В комментарии есть фото вложения? Сохраняем их
                                                var commentPhotoAttachments = new List<string>();
                                                if (comment.Attachments != null)
                                                {
                                                    foreach (var attachment in comment.Attachments)
                                                    {
                                                        if (attachment.Instance is VkNet.Model.Attachments.Photo)
                                                        {
                                                            var attachmentAsPhoto = attachment.Instance as VkNet.Model.Attachments.Photo;
                                                            if (attachmentAsPhoto.Sizes != null)
                                                            {
                                                                var lastSizePhoto = attachmentAsPhoto.Sizes.Last();
                                                                if (lastSizePhoto != null)
                                                                {
                                                                    commentPhotoAttachments.Add(lastSizePhoto.Url.ToString());
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                // Комментарий нужно было просканировать. Сохраняем новую информацию о нём или обновляем старую
                                                Database.Instance.InsertOrReplace(new Database.Comment()
                                                {
                                                    Id = Database.Comment.MakeId(group.Id, post.Id.GetValueOrDefault(), comment.Id),
                                                    GroupId = group.Id,
                                                    PostId = post.Id.GetValueOrDefault(),
                                                    СommentId = comment.Id,
                                                    LikesCount = comment.Likes.Count,
                                                    // Не сохраняем какое-то новое содержимое или вложения т.к. комментарий могли отредактировать
                                                    // в худшую сторону - удалить что-то или зацензурить
                                                    Content = savedCommentContent != string.Empty ? savedCommentContent : comment.Text,
                                                    Attachments = savedCommentAttachments != string.Empty ? savedCommentAttachments : JsonConvert.SerializeObject(commentPhotoAttachments),
                                                });

                                                // Сканируем лайки к комментарию
                                                activitiesToReceiveLikesByExecute.Add(new Database.UserActivity()
                                                {
                                                    Type = Database.UserActivity.ActivityType.COMMENT_LIKE,
                                                    // UserId пока нам неизвестен и будет получен в результате execute запроса
                                                    GroupId = group.Id,
                                                    PostId = post.Id.GetValueOrDefault(),
                                                    CommentId = comment.Id,
                                                    // Мы не можем узнать время лайка, поэтому будем считать что пользователь
                                                    // проявил эту активность во время последнего сканирования, если комментарий уже
                                                    // был просканирован ранее. В другом случае считаем что лайк был поставлен
                                                    // в то же время, что и написан комментарий
                                                    WhenHappened = isCommentNotSeenBefore ? comment.Date.GetValueOrDefault() : Utils.GetNowAsUniversalTime(),
                                                });
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Utils.Log("не удалось получить информацию о комментариях к записи " + post.Id.GetValueOrDefault() + ". Причина: " + ex.Message, LogLevel.ERROR);
                                        await WaitAlotAfterError();
                                    }
                                }

                                // В записи есть фото вложения? Сохраняем их
                                var photoAttachments = new List<string>();
                                if (post.Attachments != null)
                                {
                                    foreach (var attachment in post.Attachments)
                                    {
                                        if (attachment.Instance is VkNet.Model.Attachments.Photo)
                                        {
                                            var attachmentAsPhoto = attachment.Instance as VkNet.Model.Attachments.Photo;
                                            if (attachmentAsPhoto.Sizes != null)
                                            {
                                                var lastSizePhoto = attachmentAsPhoto.Sizes.Last();
                                                if (lastSizePhoto != null)
                                                {
                                                    photoAttachments.Add(lastSizePhoto.Url.ToString());
                                                }
                                            }
                                        }
                                    }
                                }

                                // Запись нужно было просканировать. Сохраняем новую информацию о ней или обновляем старую
                                Database.Instance.InsertOrReplace(new Database.Post()
                                {
                                    Id = Database.Post.MakeId(group.Id, post.Id.GetValueOrDefault()),
                                    GroupId = group.Id,
                                    PostId = post.Id.GetValueOrDefault(),
                                    LikesCount = post.Likes.Count,
                                    CommentsCount = post.Comments.Count,
                                    // Не сохраняем какое-то новое содержимое или вложения т.к. запись могли отредактировать
                                    // в худшую сторону - удалить что-то или зацензурить
                                    Content = savedPostContent != string.Empty ? savedPostContent : post.Text,
                                    Attachments = savedPostAttachments != string.Empty ? savedPostAttachments : JsonConvert.SerializeObject(photoAttachments),
                                });
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
                                if ((Utils.GetNowAsUniversalTime() - posts.Last().Date.GetValueOrDefault()) < Constants.MAX_SCANNING_DEPTH_IN_TIME)
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
                }

                // Получаем лайки к активностям через упакованные execute запросы
                while (activitiesToReceiveLikesByExecute.Count > 0)
                {
                    Utils.Log("Получаем информацию о лайках. Осталось: " + activitiesToReceiveLikesByExecute.Count, LogLevel.GENERAL);

                    // Берём из очереди максимальное количество активностей, которое мы можем просканировать за один запрос
                    var chunkOfActivities = activitiesToReceiveLikesByExecute.Take(VkLimits.EXECUTE_VK_API_METHODS_COUNT).ToList();

                    // Удаляем из очереди то количество активностей, которое мы взяли для сканирования
                    activitiesToReceiveLikesByExecute.RemoveRange(0, chunkOfActivities.Count);

                    try
                    {
                        // Заполняем список вызовов к API
                        var listOfAPICalls = new List<string>();
                        foreach (var activity in chunkOfActivities)
                        {
                            var isCommentLike = activity.Type == Database.UserActivity.ActivityType.COMMENT_LIKE;
                            var type = isCommentLike ? "comment" : "post";
                            var itemId = isCommentLike ? activity.CommentId : activity.PostId;

                            listOfAPICalls.Add("{\"item_id\":" + itemId + "}+API.likes.getList({\"type\":\"" + type + "\",\"owner_id\":" + group.GetNegativeIdForAPI() + ",\"item_id\":" + itemId + "})");
                        }

                        // Вызываем execute запрос
                        var response = await api.CallAsync("execute", new VkParameters()
                        {
                            { "code", "return[" + listOfAPICalls.GenerateSeparatedString(",") + "];" },
                        });
                        await WaitMinimumTimeout();

                        // Обрабатываем ответ
                        if (response != null)
                        {
                            var likesAsResponseList = ((VkResponseArray)response).ToList();
                            foreach (var likesAsResponse in likesAsResponseList)
                            {
                                var itemId = (long)likesAsResponse["item_id"];
                                var userIds = likesAsResponse.ToVkCollectionOf<long>(_ => _);

                                // Добавляем активности от этих пользователей
                                foreach (var userId in userIds)
                                {
                                    // Ищем активность по этому itemId
                                    // Сначала ищем как комментарий
                                    var activity = chunkOfActivities.Find(_ => _.CommentId == itemId);

                                    // Если не нашли, то как запись
                                    if (activity == null)
                                    {
                                        activity = chunkOfActivities.Find(_ => _.PostId == itemId);
                                    }

                                    if (activity != null)
                                    {
                                        // Активность найдена, клонируем её и заполняем userId поле
                                        var activityClone = activity.ShallowCopy();
                                        activityClone.UserId = userId;

                                        // Добавляем её к дальнейшей обработке
                                        userActivitiesToProcess.Add(activityClone);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.Log("не удалось получить информацию о некоторых лайках. Причина: " + ex.Message, LogLevel.ERROR);
                        await WaitAlotAfterError();
                    }
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
                var usersInfo = new List<User>();
                while (userIdsToReceiveInfo.Count > 0)
                {
                    Utils.Log("Получаем информацию о пользователях. Осталось: " + userIdsToReceiveInfo.Count, LogLevel.GENERAL);

                    // Берём максимальное количество Id, которое мы можем просканировать за один запрос
                    var chunkOfUserIdsToReceiveInfo = userIdsToReceiveInfo.Take(VkLimits.USERS_GET_USER_IDS).ToList();

                    // Удаляем то количество Id, которое мы взяли для сканирования
                    userIdsToReceiveInfo.RemoveRange(0, chunkOfUserIdsToReceiveInfo.Count);

                    // Получаем информацию о этих пользователях
                    try
                    {
                        // Отправляем запрос в API ВКонтакте
                        // Не используем api.Users.GetAsync потому что из-за бага эта функция не может обработать контакты пользователя
                        var response = await api.CallAsync("users.get", new VkParameters()
                        {
                            { "user_ids", chunkOfUserIdsToReceiveInfo.GenerateSeparatedString(",") },
                            { "fields", "sex,city,photo_max_orig,site,bdate,status,contacts" },
                        });
                        await WaitMinimumTimeout();

                        // Обрабатываем ответ. Чиним баг с контактами пользователя
                        if (response != null)
                        {
                            var usersAsResponseList = ((VkResponseArray)response).ToList();
                            foreach (var userAsResponse in usersAsResponseList)
                            {
                                var user = (User)userAsResponse;

                                // Заполняем контакты пользователя
                                user.Contacts = new Contacts()
                                {
                                    MobilePhone = userAsResponse.ContainsKey("mobile_phone") ? userAsResponse["mobile_phone"] : string.Empty,
                                    HomePhone = userAsResponse.ContainsKey("home_phone") ? userAsResponse["home_phone"] : string.Empty,
                                };

                                usersInfo.Add(user);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.Log("не удалось получить информацию о некоторых пользователях. Причина: " + ex.Message, LogLevel.ERROR);
                        await WaitAlotAfterError();
                    }
                }

                Utils.Log("Обработка результатов. Это может занять какое-то время", LogLevel.GENERAL);

                // Вызывает callback для полученной информации о пользователе, если она есть
                Callback<long, Callback<User>> ForReceivedInfoAboutUser = (userId, callback) =>
                {
                    var userInfo = usersInfo.Find(_ => _.Id == userId);
                    if (userInfo != null)
                    {
                        callback(userInfo);
                    }
                };

                // Сортируем активности. Лайки нас интересуют в последнюю очередь т.к. если удалённый
                // или деактивированный пользователь не написал никаких записей и комментариев, то его
                // лайки не нужно сохранять в базу
                userActivitiesToProcess.Sort((left, right) =>
                {
                    return left.IsLikeToSomething().CompareTo(right.IsLikeToSomething());
                });

                // Счётчик сколько нашли новых полезных пользователей и ботов
                var newUsersCount = 0;
                var botsCount = 0;

                // Обрабатываем интересующие нас активности: записи, лайки, комментарии и т.п.
                while (userActivitiesToProcess.Count > 0)
                {
                    // Берём первую же активность из списка для обработки
                    var userActivityToProcess = userActivitiesToProcess.First();

                    // Нужно ли будет сохранить данные об активности?
                    var needToSaveActivity = false;

                    // Пользователь уже был добавлен ранее?
                    if (Database.Instance.IsAlreadyExists<Database.User>(userActivityToProcess.UserId))
                    {
                        needToSaveActivity = true;
                    }
                    else
                    {
                        ForReceivedInfoAboutUser(userActivityToProcess.UserId, (userInfo) =>
                        {
                            // Локальная функция на удаление всей активности этого пользователя
                            Callback DeleteAllActivitiesToProcessFromThisUser = () =>
                            {
                                userActivitiesToProcess.RemoveAll(_ => _.UserId == userActivityToProcess.UserId);
                            };

                            // Проверяем не деактивирован ли пользователь
                            var isDeactivated = false;
                            if (userInfo.Deactivated != null)
                            {
                                // Пользователь деактивирован или удалён?
                                if (userInfo.Deactivated != Deactivated.Activated)
                                {
                                    isDeactivated = true;

                                    // Не сохраняем лайки от деактивированных или удалённых пользователей т.к.
                                    // в них не содержится никакой полезной информации
                                    if (userActivityToProcess.IsLikeToSomething())
                                    {
                                        // Можно смело удалять всю активность от этого пользователя т.к. там
                                        // остались одни лайки
                                        DeleteAllActivitiesToProcessFromThisUser();
                                        return;
                                    }
                                }
                            }

                            // Проверяем пол пользователя
                            if (userInfo.Sex != Constants.TARGET_SEX_ID)
                            {
                                // Неправильный пол. Удаляем всю активность этого пользователя из
                                // очереди на проверку и завершаем сканирование активности
                                DeleteAllActivitiesToProcessFromThisUser();
                                return;
                            }

                            // Определяем Id города
                            var cityId = userInfo.City != null ? userInfo.City.Id.GetValueOrDefault(0) : 0;
                            var cityName = userInfo.City != null ? userInfo.City.Title : string.Empty;

                            // Проверяем город учитывая настройки пользователя
                            switch (settings.SearchMethod)
                            {
                                case Database.Settings.SearchMethodType.BY_CITY:
                                    if (cityId != settings.CityId)
                                    {
                                        DeleteAllActivitiesToProcessFromThisUser();
                                        return;
                                    }
                                    break;

                                case Database.Settings.SearchMethodType.SMART:
                                    if (!group.IsClosed)
                                    {
                                        if (cityId != settings.CityId)
                                        {
                                            DeleteAllActivitiesToProcessFromThisUser();
                                            return;
                                        }
                                    }
                                    break;
                            }

                            // Это анкета бота? Эвристический анализ
                            if (false)
                            {
                                // Похоже что это бот. Удаляем всю активность бота из очереди на проверку
                                DeleteAllActivitiesToProcessFromThisUser();

                                // Увеличиваем счётчик найденных ботов
                                ++botsCount;

                                // Завершаем сканирование активности
                                return;
                            }

                            // Определяем дату рождения
                            var birthDate = new DateTime();
                            var birthDateSet = false;

                            try
                            {
                                // Проверяем, указан ли год вообще.
                                // Для этого считаем количество точек в дате. Если "1.1.1990" то год есть, а если "1.1", то нету
                                if (userInfo.BirthDate.Count(_ => _ == '.') == 2)
                                {
                                    // Пробуем сконвертировать дату рождения
                                    if (DateTime.TryParseExact(userInfo.BirthDate, "d.M.yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out birthDate))
                                    {
                                        birthDateSet = true;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // Не удалось преобразовать дату рождения, игнорируем ошибку
                            }

                            // Определяем мобильный телефон
                            var mobilePhone = userInfo.Contacts != null ? userInfo.Contacts.MobilePhone : string.Empty;
                            var homePhone = userInfo.Contacts != null ? userInfo.Contacts.HomePhone : string.Empty;

                            // Всё нормально, все условия и тесты пройдены, сохраняем пользователя
                            Database.Instance.InsertOrReplace(new Database.User()
                            {
                                Id = userActivityToProcess.UserId,
                                FirstName = userInfo.FirstName,
                                LastName = userInfo.LastName,
                                BirthDate = birthDateSet ? birthDate : default(DateTime),
                                CityId = cityId,
                                CityName = cityName,
                                Status = userInfo.Status,
                                MobilePhone = mobilePhone,
                                HomePhone = homePhone,
                                Site = userInfo.Site,
                                PhotoURL = userInfo.PhotoMaxOrig.ToString(),
                                LastActivity = userActivityToProcess.WhenHappened,
                                WhenAdded = DateTime.Now,
                                FromGroupId = group.Id,
                                IsDeactivated = isDeactivated,
                            });

                            // Увеличиваем счётчик полезных пользователей
                            ++newUsersCount;

                            // Помечаем сообщество как только что активное
                            group.MarkAsJustActive();

                            // Нужно так же сохранить это активность пользователя
                            needToSaveActivity = true;
                        });
                    }

                    // Нужно ли сохранить данные о активности?
                    if (needToSaveActivity)
                    {
                        // Эта активность не была ещё добавлена ранее?
                        if (!Database.Instance.IsUserActivityAlreadyExists(userActivityToProcess))
                        {
                            // Сохраняем эту активность как новую активность пользователя
                            Database.Instance.InsertOrReplace(userActivityToProcess);

                            // Обновляем поля у пользователя
                            Database.Instance.ModifyFields<Database.User>(userActivityToProcess.UserId, (user) =>
                            {
                                // Найденная активность была раньше той, которую мы уже сохранили?
                                if (userActivityToProcess.WhenHappened > user.LastActivity)
                                {
                                    // Показываем пользователя снова, если он был скрыт нами
                                    if (user.IsHidden == Database.HiddenStatus.HIDDEN_UNTIL_ACTIVITY)
                                    {
                                        user.IsHidden = Database.HiddenStatus.NOT_HIDDEN;
                                    }

                                    // Обновляем дату последней активности
                                    user.LastActivity = userActivityToProcess.WhenHappened;
                                }
                            });
                        }
                    }

                    // Удаляем обработанную активность из списка обработки
                    userActivitiesToProcess.Remove(userActivityToProcess);
                }

                // Помечаем всех пользователей о которых мы получили информацию как просканированных
                foreach (var userInfo in usersInfo)
                {
                    Database.Instance.InsertOrReplace(new Database.ScannedUser()
                    {
                        UserId = userInfo.Id
                    });
                }

                // Помечаем сообщество как только что просканированное
                group.MarkAsJustScanned();

                // Устанавливаем время ожидания перед следующим сканированием сообщества
                group.SetInteractTimeout(Timeouts.AFTER_GROUP_WAS_SCANNED);

                // Сообщество просканировано
                Utils.Log("Сообщество " + group.Name + " успешно просканировано", LogLevel.SUCCESS);
                Utils.Log("Новых пользователей: " + newUsersCount, LogLevel.SUCCESS);
                Utils.Log("Отсеяно ботов: " + botsCount, LogLevel.SUCCESS);
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
            await Task.Delay(Timeouts.AFTER_ANY_REQUEST_TO_API);
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
            var domainName = Regex.Replace(groupWebUrl, @".+\/", string.Empty).Trim();

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