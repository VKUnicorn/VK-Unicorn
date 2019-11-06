using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkNet;
using VkNet.Model;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;
using VkNet.Enums.SafetyEnums;
using System.Net;
using System.IO;

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
                    Database.Instance.ForSettings((settings) =>
                    {
                        // Готовим список задач, которые вообще можно делать. Задачи будут проверяться в порядке их объявления
                        var possibleTaskConditions = new List<Callback>()
                        {
                            // Временный таск для разработки. Мешает выполнению методов, требующих авторизацию
                            () =>
                            {
                                currentTask = async () => { await JustWait(); };
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
                                var groupsToReceiveInfo = Database.Instance.GetGroupsToReceiveInfo();
                                if (groupsToReceiveInfo.Count > 0)
                                {
                                    currentTask = async () => { await GetGroupsInfoTask(groupsToReceiveInfo); };
                                }
                            },

                            // Нечего больше делать, просто ждём. Другие задачи могут появиться позже
                            () =>
                            {
                                currentTask = async () => { await JustWait(); };
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

                // Ждём некоторое время. Торопиться некуда, лучше сканировать медленно, но зато без угрозы бана аккаунта
                // Слишком частые запросы это плохо, о лимитах можно почитать тут https://vk.com/dev/api_requests
                // в разделе "3. Ограничения и рекомендации". В целом рекомендуется обращаться не чаще трёх раз в секунду,
                // но мы будем сканировать значительно реже, опять же чтобы снизить угрозу бана аккаунта или появления капчи
                await Task.Delay(TimeSpan.FromSeconds(1d));
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
            catch (System.Exception ex)
            {
                Utils.Log("не удалось авторизироваться. Причина: " + ex.Message, LogLevel.ERROR);
                await WaitAlotAfterError();
            }
        }

        async Task GetGroupsInfoTask(IEnumerable<Database.GroupToAdd> groupsToReceiveInfo)
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
                            if (!Database.Instance.IsGroupAlreadyExists(groupInfo.Id))
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
                                Database.Instance.AddGroupOrReplace(newGroup);
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
                    Database.Instance.RemoveGroupsToReceiveInfo(groupsToReceiveInfo);
                }
            }
            catch (System.Exception ex)
            {
                Utils.Log("не удалось получить информацию о группах. Причина: " + ex.Message, LogLevel.ERROR);
                await WaitAlotAfterError();
            }
        }

        async Task JustWait()
        {
            MainForm.Instance.SetStatus("ожидание" + GetProgressDots(), StatusType.SUCCESS);

            await Task.Delay(TimeSpan.FromSeconds(10d));
        }

        async Task WaitAlotAfterError()
        {
            MainForm.Instance.SetStatus("ожидание после ошибки", StatusType.ERROR);

            await Task.Delay(TimeSpan.FromSeconds(20d));
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
    }
}