using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet;
using VkNet.Model;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;

namespace VK_Unicorn
{
    class Worker
    {
        public static Worker Instance { get; private set; }

        // Ссылка на API ВКонтакте
        VkApi api;

        // Авторизированы ли ВКонтакте
        bool isAuthorized;

        // Счётчик для отображения изменяющегося троеточия в процессе сканирования
        int dotsCounter = 1;

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

            // Если настройки ещё не установлены , то показываем окно настроек сразу же после запуска программы
            if (!Database.Instance.IsSettingsValid())
            {
                MainForm.Instance.OpenSettingsWindow();
            }

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
                            // Проверяем, залогинены ли мы вообще. Если нет, то добавляем задачу залогиниться
                            () =>
                            {
                                if (!isAuthorized)
                                {
                                    currentTask = async () => { await AuthorizationTask(settings); };
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
                await Task.Delay(TimeSpan.FromSeconds(1.5d));
            }
        }

        async Task AuthorizationTask(Database.Settings settings)
        {
            MainForm.Instance.SetStatus("авторизация...", StatusType.SUCCESS);

            Utils.Log("Авторизируемся в ВКонтакте", LogLevel.NOTIFY);

            var applicationId = 0ul;
            if (ulong.TryParse(settings.ApplicationId.Trim(), out applicationId))
            {
                api = new VkApi();
                api.OnTokenExpires += (sender) =>
                {
                    isAuthorized = false;

                    Utils.Log("Токен авторизации стал недействительным. Будет необходимо авторизироваться заново", LogLevel.WARNING);
                };

                try
                {
                    await api.AuthorizeAsync(new ApiAuthParams
                    {
                        ApplicationId = applicationId,
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
                    isAuthorized = false;

                    Utils.Log("не удалось авторизироваться. Причина: " + ex.Message, LogLevel.ERROR);

                    // Ждём некоторое время после неудачной авторизации
                    await WaitAlot();
                }
            }
            else
            {
                Utils.Log("не удалось преобразовать ID приложения в unsigned long число", LogLevel.ERROR);
            }
        }

        async Task JustWait()
        {
            MainForm.Instance.SetStatus("ожидание" + GetProgressDots(), StatusType.SUCCESS);

            await Task.Delay(TimeSpan.FromSeconds(10d));
        }

        async Task WaitAlot()
        {
            await Task.Delay(TimeSpan.FromSeconds(20d));
        }

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
