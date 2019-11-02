using SQLite;
using System;
using System.Linq;

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
            // Только профили, в которых указан твой город
            BY_CITY,
            // Все профили из закрытых групп, остальные по городу
            SMART,
            // Ищет все профили женского пола. Огромное количество спама и ботов
            ALL_FEMALES,
        }

        // Основная таблица с интересными нам профилями
        public class Profile
        {
            // Id профиля, уникальный для каждого
            [PrimaryKey, Unique]
            public string Id { get; set; }

            // Имя
            public string FirstName { get; set; }

            // Фамилия
            public string LastName { get; set; }

            // Id города откуда был добавлен профиль
            public int CityId { get; set; }

            // Когда этот профиль был добавлен в базу данных
            public DateTime WhenAdded { get; set; }

            // Настройка того скрыт ли профиль
            public HiddenStatus IsHidden { get; set; }
        }

        // Таблица с группами
        public class Group
        {
            // Id группы, уникальный для каждой
            [PrimaryKey, Unique]
            public string Id { get; set; }

            // Имя группы
            public string Name { get; set; }

            // Как давно было найдено чего-нибудь полезное в этой группе
            public DateTime LastActivity { get; set; }

            // Как давно был последний скан группы
            public DateTime LastScanned { get; set; }

            // Сколько времени заняло сканирование этого паблика в секундах
            public int ScanTimeInSeconds { get; set; }
        }

        // Таблица активности когда кто-то лайкает пост
        public class LikeActivity
        {
            // Id профиля, который что-то лайкнул
            public string Id { get; set; }

            // Ссылка на пост
            public int PostLink { get; set; }

            // Что было написано в посте, который лайкнули
            public string PostContent { get; set; }

            // Id группы в которой был пост
            public string GroupId { get; set; }

            // Когда этот профиль был лайкнут
            public DateTime WhenAdded { get; set; }
        }

        // Таблица активности когда кто-то пишет пост
        public class PostActivity
        {
            // Id профиля, который что-то написал
            public string Id { get; set; }

            // Ссылка на пост
            public int PostLink { get; set; }

            // Что было написано в посте
            public string PostContent { get; set; }

            // Id группы в которой был пост
            public string GroupId { get; set; }

            // Когда этот пост был написан
            public DateTime WhenAdded { get; set; }
        }

        // Таблица настроек приложения
        public class Settings
        {
            // Всегда "db"
            [PrimaryKey, Unique]
            public string Id { get; set; }

            // Id приложения
            public string ApplicationId { get; set; }

            // Логин
            public string Login { get; set; }

            // Пароль
            public string Password { get; set; }

            // Id города
            public int CityId { get; set; }

            // Метод поиска профилей
            public SearchMethod SearchMethod { get; set; }

            // Стоп слова
            public string StopWords { get; set; }
        }

        // Таблица для служебного использования
        class _System
        {
            // Всегда "db"
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

            Utils.Log("Обновляем таблицы базы данных, если это необходимо", LogLevel.NOTIFY);

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
            ForDatabaseLocked((connection) =>
            {
                // Создаём таблицы для служебного использования
                connection.CreateTable<_System>();
                connection.CreateTable<Settings>();

                // Создаём все остальные таблицы
                connection.CreateTable<Profile>();
                connection.CreateTable<Group>();
                connection.CreateTable<LikeActivity>();
                connection.CreateTable<PostActivity>();
            });
        }

        void UpdateTables()
        {
            ForDatabaseLocked((db) =>
            {
                var query = db.Table<_System>().Where(v => v.Id == "db").SingleOrDefault();
                if (query != null)
                {
                    Utils.Log("Загружена база данных версии " + query.SchemeVersion, LogLevel.NOTIFY);

                    var needToUpdate = query.SchemeVersion < SCHEME_VERSION;

                    if (needToUpdate)
                    {
                        Utils.Log("Обновляем таблицы", LogLevel.NOTIFY);

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
                    db.Insert(new _System { Id = "db", SchemeVersion = SCHEME_VERSION });
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
                    callback(db);
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
            database.BusyTimeout = TimeSpan.FromSeconds(5d);
            return database;
        }

        public int GetProfilesCount()
        {
            var result = 0;

            ForDatabaseUnlocked((db) =>
            {
                result = db.Table<Profile>().Count();
            });

            return result;
        }

        public int GetGroupsCount()
        {
            var result = 0;

            ForDatabaseUnlocked((db) =>
            {
                result = db.Table<Group>().Count();
            });

            return result;
        }

        public bool IsSettingsValid()
        {
            var result = false;

            ForSettings((settings) =>
            {
                result = !string.IsNullOrEmpty(settings.ApplicationId)
                      && !string.IsNullOrEmpty(settings.Login)
                      && !string.IsNullOrEmpty(settings.Password)
                      ;
            });

            return result;
        }

        public void ForSettings(Callback<Settings> callback)
        {
            ForDatabaseUnlocked((db) =>
            {
                var settings = db.Table<Settings>().Where(v => v.Id == "db").SingleOrDefault();
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
                settings.Id = "db";

                db.InsertOrReplace(settings);
            });
        }
    }
}