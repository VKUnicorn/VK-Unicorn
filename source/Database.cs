using SQLite;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VK_Unicorn
{
    class Database
    {
        // Версия базы данных. Увеличивается если база как-то кардинально меняется
        // Не нужно увеличивать если в таблицу просто добавляется новая колонка
        const int SCHEME_VERSION = 1;

        public enum HiddenStatus
        {
            // Профиль не скрыт
            NOT_HIDDEN,
            // Профиль скрыт пока не появится ещё какая-то активность
            HIDDEN_UNTIL_ACTIVITY,
            // Профиль скрыт навсегда. Будет удалён при чистке базы
            HIDDEN_PERMANENTLY,
        }

        public enum SearchMethod
        {
            // Только те профили, в которых указан твой город
            BY_CITY,
            // Все профили из закрытых групп, остальные по городу
            SMART,
            // Все профили женского пола. Огромное количество спама и ботов
            ALL_FEMALES,
        }

        // Основная таблица с интересными нам профилями
        public class Profile
        {
            // Id профиля. Например у Павла Дурова этот Id равен единице - https://vk.com/id1
            [PrimaryKey, Unique]
            public long Id { get; set; }

            // Имя
            public string FirstName { get; set; }

            // Фамилия
            public string LastName { get; set; }

            // Дата рождения
            public DateTime BirthDate { get; set; }

            // Id города откуда был добавлен профиль
            public long CityId { get; set; }

            // Когда этот профиль был добавлен в базу данных
            public DateTime WhenAdded { get; set; }

            // С какой группы был добавлен профиль
            public long FromGroupId { get; set; }

            // Скрыт ли этот профиль пользователем
            public HiddenStatus IsHidden { get; set; }

            // URL главной фотографии в максимальном размере
            public string PhotoURL { get; set; }
        }

        // Таблица с группами о которых ещё предстоит получить информацию и добавить в обычную
        // таблицу с группами, если там ещё нету такой же
        public class GroupToAdd
        {
            // Введённый пользователем id группы. Идентификатор или короткое имя сообщества
            // Например пользователь ввёл адрес https://vk.com/public1 или https://vk.com/club1
            // Это всё адреса одной и той же группы https://vk.com/apiclub, но по public1, как и
            // по club1 получить информацию по запросу wall.get нельзя. Чтобы не создавать путаницу
            // мы просто получим реальный id сообщества через запрос groups.getById и дальше уже
            // будем работать только с ним. Нам всё равно вызывать этот запрос для получения имени
            // сообщества и всех других данных
            [PrimaryKey, Unique]
            public string DomainName { get; set; }
        }

        // Таблица с группами
        public class Group
        {
            // Id группы. Например у группы "ВКонтакте API" этот Id равен единице https://vk.com/public1
            // Не тот, который ввёл пользователь, а тот, который получим с сервера потом сами
            [PrimaryKey, Unique]
            public long Id { get; set; }

            // Название группы
            public string Name { get; set; }

            // Короткий адрес группы. Сохраняем его для открытия ссылок
            public string ScreenName { get; set; }

            // Статус закрытости группы
            public bool IsClosed { get; set; }

            // Статус членства в группе. Актуально только для закрытых групп
            public bool IsMember { get; set; }

            // Как давно было найдено чего-нибудь полезное в этой группе
            public DateTime LastActivity { get; set; }

            // Как давно был последний успешный скан группы
            public DateTime LastScanned { get; set; }

            // Сколько времени заняло сканирование этого паблика в секундах
            public int ScanTimeInSeconds { get; set; }

            // URL главной фотографии в максимальном размере
            public string PhotoURL { get; set; }

            /// <summary>
            /// Сколько анкет было найдено с этой группы
            /// </summary>
            public int GetEfficiency()
            {
                var result = 0;

                Instance.ForDatabaseUnlocked((db) =>
                {
                    result = db.Table<Profile>().Where(_ => _.FromGroupId == Id).Count();
                });

                return result;
            }

            /// <summary>
            /// Возвращает ссылку на группу
            /// </summary>
            public string GetURL()
            {
                return Constants.VK_WEB_PAGE + ScreenName;
            }
        }

        // Таблица активности когда кто-то лайкает пост
        public class LikeActivity
        {
            // Id профиля, который что-то лайкнул
            public long Id { get; set; }

            // Id группы в которой был пост
            public long GroupId { get; set; }

            // Id поста из группы
            public long PostId { get; set; }

            // Что было написано в посте, который лайкнули
            public string PostContent { get; set; }

            // Когда этот профиль был лайкнут
            public DateTime WhenAdded { get; set; }
        }

        // Таблица активности когда кто-то пишет пост
        public class PostActivity
        {
            // Id профиля, который что-то написал
            public long ProfileId { get; set; }

            // Id группы в которой был пост
            public long GroupId { get; set; }

            // Id поста из группы
            public long PostId { get; set; }

            // Что было написано в посте
            public string PostContent { get; set; }

            // Когда этот пост был добавлен
            public DateTime WhenAdded { get; set; }
        }

        // Таблица активности когда кто-то пишет комментарий
        public class CommentActivity
        {
            // Id профиля, который что-то написал
            public long ProfileId { get; set; }

            // Id группы в которой был пост
            public long GroupId { get; set; }

            // Id поста из группы
            public long PostId { get; set; }

            // Id комментария к посту
            public long CommentId { get; set; }

            // Что было написано в комментарии
            public string CommentContent { get; set; }

            // Когда этот комментарий был добавлен
            public DateTime WhenAdded { get; set; }
        }

        // Таблица с id тех профилей, которые мы уже просканировали.
        // Эта таблица нужна чтобы не сканировать сто раз одни и тех же профили, а
        // следовательно будет отправляться значительно меньше запросов на серверы
        // ВКонтакте. После того как профиль попадает в эту таблицу мы больше не
        // будем получать никакую информацию о нём в дальнейшем
        public class ScannedProfiles
        {
            // Id профиля
            [PrimaryKey, Unique]
            public long Id { get; set; }
        }

        // Таблица с id тех постов, которые мы уже просканировали.
        // Запоминается так же количество лайков и комментариев к этому посту чтобы
        // потом повторно сканировать посты где что-то изменилось в большую сторону
        public class ScannedPosts
        {
            // Id группы, в которой был написан пост
            public long GroupId { get; set; }

            // Id поста в этой группе
            public long PostId { get; set; }

            // Счётчик лайков. Если он изменится в большую сторону, то будем
            // сканировать пост повторно
            public int LikesCount { get; set; }

            // Счётчик комментариев. Если он изменится в большую сторону, то будем
            // сканировать пост повторно
            public int CommentsCount { get; set; }
        }

        // Маркер для служебного использования. Менять не нужно
        const string INTERNAL_DB_MARKER = "db";

        // Таблица настроек приложения
        public class Settings
        {
            // Всегда равен INTERNAL_DB_MARKER
            [PrimaryKey, Unique]
            public string Id { get; set; }

            // Id приложения
            public long ApplicationId { get; set; }

            // Логин
            public string Login { get; set; }

            // Пароль
            public string Password { get; set; }

            // Id города
            public long CityId { get; set; }

            // Метод поиска профилей
            public SearchMethod SearchMethod { get; set; }

            // Стоп слова
            public string StopWords { get; set; }
        }

        // Таблица для служебного использования
        class _System
        {
            // Всегда такой же как INTERNAL_DB_MARKER
            [PrimaryKey, Unique]
            public string Id { get; set; }

            // Версия базы данных
            public int SchemeVersion { get; set; }
        }

        public static Database Instance { get; private set; }

        SQLiteConnection database;

        public Database()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            try
            {
                CreateTables();
            }
            catch (System.Exception ex)
            {
                Utils.Log("Не удалось инициализировать базу данных " + Constants.DATABASE_FILENAME + " для хранения базы данных. Причина: " + ex.Message, LogLevel.ERROR);
                throw;
            }

            Utils.Log("Обновляем таблицы базы данных, если это необходимо", LogLevel.GENERAL);

            try
            {
                UpdateTables();
            }
            catch (System.Exception ex)
            {
                Utils.Log("Не удалось обновить таблицы в базе данных. Причина: " + ex.Message, LogLevel.ERROR);
                throw;
            }

            Utils.Log("База данных готова к работе", LogLevel.SUCCESS);
        }

        void CreateTables()
        {
            ForDatabaseLocked((db) =>
            {
                // Создаём таблицы для служебного использования
                db.CreateTables<_System, Settings>();

                // Создаём все остальные таблицы
                db.CreateTable<Profile>();
                db.CreateTables<GroupToAdd, Group>();
                db.CreateTables<LikeActivity, PostActivity, CommentActivity>();
                db.CreateTables<ScannedProfiles, ScannedPosts>();
            });
        }

        void UpdateTables()
        {
            ForDatabaseLocked((db) =>
            {
                var query = db.Table<_System>().Where(_ => _.Id == INTERNAL_DB_MARKER).SingleOrDefault();
                if (query != null)
                {
                    Utils.Log("Загружена база данных версии " + query.SchemeVersion, LogLevel.GENERAL);

                    var needToUpdate = query.SchemeVersion < SCHEME_VERSION;

                    if (needToUpdate)
                    {
                        Utils.Log("Обновляем таблицы", LogLevel.GENERAL);

                        // Тут код миграции ДБ, если будет необходимо в дальнейшем.
                        // Миграция по добавлению столбцов в таблицы происходит автоматически
                    }
                    else
                    {
                        Utils.Log("Обновление таблиц не требуется", LogLevel.NOTIFY);
                    }
                }
                else
                {
                    db.Insert(new _System { Id = INTERNAL_DB_MARKER, SchemeVersion = SCHEME_VERSION });
                }
            });
        }

        void ForDatabaseLocked(Callback<SQLiteConnection> callback)
        {
            var db = GetConnection();
            if (db != null)
            {
                db.RunInTransaction(() =>
                {
                    try
                    {
                        callback(db);
                    }
                    catch (System.Exception ex)
                    {
                        Utils.Log("во время блокирующего обращения к базе была поймана ошибка: " + ex.Message, LogLevel.ERROR);
                    }
                });
            }
        }

        void ForDatabaseUnlocked(Callback<SQLiteConnection> callback)
        {
            var db = GetConnection();
            if (db != null)
            {
                callback(db);
            }
        }

        SQLiteConnection GetConnection()
        {
            if (database != null)
            {
                return database;
            }

            database = new SQLiteConnection(Constants.DATABASE_FILENAME);
            database.BusyTimeout = TimeSpan.FromSeconds(10d);
            return database;
        }

        /// <summary>
        /// Возвращает количество записей в таблице
        /// </summary>
        int GetCount<T>() where T : new()
        {
            var result = 0;

            ForDatabaseUnlocked((db) =>
            {
                result = db.Table<T>().Count();
            });

            return result;
        }

        public bool IsSettingsValid()
        {
            var result = false;

            ForSettings((settings) =>
            {
                result = settings.ApplicationId > 0
                      && !string.IsNullOrEmpty(settings.Login)
                      && !string.IsNullOrEmpty(settings.Password);
            });

            return result;
        }

        public void ForSettings(Callback<Settings> callback)
        {
            ForDatabaseUnlocked((db) =>
            {
                var settings = db.Table<Settings>().Where(_ => _.Id == INTERNAL_DB_MARKER).SingleOrDefault();
                if (settings != null)
                {
                    callback(settings);
                }
            });
        }

        public void SaveSettings(Settings settings)
        {
            ForDatabaseUnlocked((db) =>
            {
                settings.Id = INTERNAL_DB_MARKER;

                db.InsertOrReplace(settings);
            });
        }

        public void ShowStatistics()
        {
            Utils.Log("Статистика:", LogLevel.SUCCESS);
            Utils.Log("    Всего найдено полезных профилей: " + GetCount<Profile>(), LogLevel.NOTIFY);
            Utils.Log("    Количество групп для сканирования: " + GetCount<Group>(), LogLevel.NOTIFY);
            Utils.Log("    Просканировано профилей: " + GetCount<ScannedProfiles>(), LogLevel.NOTIFY);
            Utils.Log("    Просканировано постов: " + GetCount<ScannedPosts>(), LogLevel.NOTIFY);
        }

        /// <summary>
        /// Получаем ссылку на новую группу, получаем из неё DomainName
        /// Добавляем группу в таблицу GroupToAdd, если там такой нету
        /// </summary>
        public void RegisterNewGroupToAdd(string groupWebUrl)
        {
            try
            {
                // Удаляем все символы перед доменным именем
                var domainName = Regex.Replace(groupWebUrl, @".+\/", "");

                if (!string.IsNullOrEmpty(domainName))
                {
                    var rowsModified = 0;
                    ForDatabaseLocked((db) =>
                    {
                        rowsModified = db.Insert(new GroupToAdd()
                        {
                            DomainName = domainName,
                        });
                    });

                    if (rowsModified > 0)
                    {
                        Utils.Log("Группа " + domainName + " успешно добавлена в очередь на начальное сканирование", LogLevel.SUCCESS);
                    }
                    else
                    {
                        throw new Exception("скорее всего группа уже добавлена в эту очередь ранее");
                    }
                }
                else
                {
                    throw new Exception("не удалось получить имя группы из ссылки");
                }
            }
            catch (System.Exception ex)
            {
                Utils.Log("не удалось добавить группу " + groupWebUrl + " в очередь на начальное сканирование. Причина: " + ex.Message, LogLevel.ERROR);
            }
        }

        /// <summary>
        /// Возвращает список групп для которых необходимо получить основную информацию
        /// </summary>
        public List<GroupToAdd> GetGroupsToReceiveInfo()
        {
            var result = new List<GroupToAdd>();

            ForDatabaseUnlocked((db) =>
            {
                result = db.Table<GroupToAdd>().Take(VkLimits.GROUPS_GETBYID_GROUP_IDS).ToList();
            });

            return result;
        }

        /// <summary>
        /// Удаляет группы из очереди на получение основной информации
        /// </summary>
        public void RemoveGroupsToReceiveInfo(IEnumerable<GroupToAdd> groupsToAdd)
        {
            ForDatabaseLocked((db) =>
            {
                foreach (var groupToAdd in groupsToAdd)
                {
                    db.Delete<GroupToAdd>(groupToAdd.DomainName);
                }
            });
        }

        /// <summary>
        /// Добавляет новую группу или изменяет её, если группа уже существует
        /// </summary>
        public bool AddGroupOrReplace(Group group)
        {
            var rowsModified = 0;
            ForDatabaseLocked((db) =>
            {
                rowsModified = db.InsertOrReplace(group);
            });

            if (rowsModified > 0)
            {
                Utils.Log("Группа " + group.Name + " (" + group.Id + ") была успешно сохранена", LogLevel.SUCCESS);
            }

            return rowsModified > 0;
        }

        /// <summary>
        /// Группа уже добавлена в список?
        /// </summary>
        public bool IsGroupAlreadyExists(long groupId)
        {
            var result = false;

            ForDatabaseUnlocked((db) =>
            {
                result = db.Table<Group>().Where(_ => _.Id == groupId).Count() > 0;
            });

            return result;
        }

        /// <summary>
        /// Пустое количество групп и профилей?
        /// </summary>
        public bool IsNeedToSetupGroups()
        {
            if (GetCount<Group>() > 0)
            {
                return false;
            }

            if (GetCount<Profile>() > 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Вызывает callback для каждой группы
        /// </summary>
        public void ForEachGroup(Callback<Group> callback)
        {
            ForDatabaseUnlocked((db) =>
            {
                foreach (var group in db.Table<Group>())
                {
                    callback(group);
                }
            });
        }
    }
}