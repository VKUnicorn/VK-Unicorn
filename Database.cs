using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace VK_Unicorn
{
    class Database
    {
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

        // Основная таблица с профилями
        public class Profile
        {
            // Id профиля, уникальный для каждого
            [PrimaryKey, Unique]
            public string Id { get; set; }

            // Имя в профиле
            public string Name { get; set; }

            // Id города откуда был добавлен профиль
            public int CityId { get; set; }

            // Когда этот профиль был добавлен в базу данных
            public DateTime WhenAdded { get; set; }

            // Настройка того скрыт ли профиль
            public HiddenStatus Hidden { get; set; }
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

        // Таблица для служебного использования
        public class _System
        {
            // Всегда "db"
            [PrimaryKey, Unique]
            public string Id { get; set; }

            // Версия базы данных
            public int SchemeVersion { get; set; }
        }

        public Database()
        {
            CreateDatabaseIfDontExists();
        }

        void CreateDatabaseIfDontExists()
        {
            try
            {
                CreateTables();
            }
            catch (System.Exception ex)
            {
                Utils.Log("Не удалось инициализировать базу данных " + Constants.DATABASE_FILENAME + " для хранения базы данных. Причина: " + ex.Message, LogLevel.ERROR);
                return;
            }

            Utils.Log("Обновляем таблицы базы данных, если это необходимо", LogLevel.NOTIFY);

            try
            {
                UpdateTables();
            }
            catch (System.Exception ex)
            {
                Utils.Log("Не удалось обновить таблицы в базе данных. Причина: " + ex.Message, LogLevel.ERROR);
                return;
            }

            Utils.Log("База данных готова к работе", LogLevel.SUCCESS);
        }

        void CreateTables()
        {
            ForDatabaseLocked((connection) =>
            {
                // Создаём таблицу для служебного использования
                connection.CreateTable<_System>();

                // Создаём все остальные таблицы
                connection.CreateTable<Profile>();
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
            using (var db = GetConnection())
            {
                db.RunInTransaction(() =>
                {
                    callback(db);
                });
            }
        }

        SQLiteConnection GetConnection()
        {
            var db = new SQLiteConnection(Constants.DATABASE_FILENAME);
            db.BusyTimeout = TimeSpan.FromSeconds(5d);
            return db;
        }

        public int GetProfilesCount()
        {
            var result = 0;

            ForDatabaseLocked((db) =>
            {
                result = db.Table<Profile>().Count();
            });

            return result;
        }
    }
}