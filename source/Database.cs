using SQLite;
using System;
using System.Linq;
using System.Collections.Generic;

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
        }

        public enum SearchMethod
        {
            // Только те профили, в которых указан твой город
            BY_CITY,
            // Все профили из закрытых групп, остальные по городу
            SMART,
            // Все профили c полом Constants.TARGET_SEX_ID. Огромное количество спама и ботов
            ALL_OF_TARGET_SEX,
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

            // URL главной фотографии в максимальном размере
            public string PhotoURL { get; set; }

            // Когда этот профиль был добавлен в базу данных
            public DateTime WhenAdded { get; set; }

            // С какой группы был добавлен профиль
            public long FromGroupId { get; set; }

            // Скрыт ли этот профиль пользователем
            public HiddenStatus IsHidden { get; set; }

            /// <summary>
            /// Возвращает ссылку на профиль
            /// </summary>
            public string GetURL()
            {
                return Constants.VK_WEB_PAGE + "id" + Id;
            }
        }

        // Таблица с группами о которых ещё предстоит получить информацию и добавить в обычную
        // таблицу с группами, если там ещё нету такой же
        public class GroupToReceiveInfo
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

            // URL главной фотографии в максимальном размере
            public string PhotoURL { get; set; }

            // Как давно было найдено чего-нибудь полезное в этой группе
            public DateTime LastActivity { get; set; }

            // Как давно был последний успешный скан группы
            public DateTime LastScanned { get; set; }

            // Дата когда будет разрешено в следующий раз взаимодейстовать с группой
            // например мы отправили заявку в группу и будем пытаться сканировать её только
            // через минут пять. А так же нету смысла сканировать группу прям сразу же после
            // того как просканировали её, можно подождать минут 30
            public DateTime InteractTimeout { get; set; }

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

            /// <summary>
            /// Это закрытая группа в которую ещё не вступили?
            /// </summary>
            public bool IsWantToJoin()
            {
                return IsClosed && !IsMember;
            }

            /// <summary>
            /// Прошло ли время после которого разрешено взаимодействовать с группой
            /// </summary>
            public bool CanInteract()
            {
                return DateTime.Now > InteractTimeout;
            }

            /// <summary>
            /// Устанавливаем таймаут на дальнейшее взаимодействие с группой
            /// </summary>
            public void SetInteractTimeout(TimeSpan timeSpan)
            {
                InteractTimeout = DateTime.Now.Add(timeSpan);

                Instance.InsertOrReplace(this);
            }

            /// <summary>
            /// Помечаем группу как только что просканированную
            /// </summary>
            public void MarkAsJustScanned()
            {
                LastScanned = DateTime.Now;

                Instance.InsertOrReplace(this);
            }

            /// <summary>
            /// Сканировали группу хотя бы раз?
            /// </summary>
            public bool WasScanned()
            {
                return LastActivity.Ticks > 0;
            }

            /// <summary>
            /// Возвращает Id со знаком минус. Используется в API запросах
            /// </summary>
            public long GetNegativeId()
            {
                return -Id;
            }
        }

        // Таблица активности когда кто-то лайкает запись
        public class LikeActivity
        {
            // Id профиля, который что-то лайкнул
            public long Id { get; set; }

            // Id группы в которой была запись
            public long GroupId { get; set; }

            // Id записи из группы
            public long PostId { get; set; }

            // Что было написано в записи, которую лайкнули
            public string PostContent { get; set; }

            // Когда этот профиль был лайкнут
            public DateTime WhenAdded { get; set; }
        }

        // Таблица активности когда кто-то пишет запись
        public class PostActivity
        {
            // Id профиля, который что-то написал
            public long ProfileId { get; set; }

            // Id группы в которой была запись
            public long GroupId { get; set; }

            // Id записи из группы
            public long PostId { get; set; }

            // Что было написано в записи
            public string PostContent { get; set; }

            // Когда эта запись была добавлена
            public DateTime WhenAdded { get; set; }
        }

        // Таблица активности когда кто-то пишет комментарий
        public class CommentActivity
        {
            // Id профиля, который что-то написал
            public long ProfileId { get; set; }

            // Id группы в которой была запись
            public long GroupId { get; set; }

            // Id записи из группы
            public long PostId { get; set; }

            // Id комментария к записи
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
        public class ScannedProfile
        {
            // Id профиля
            [PrimaryKey, Unique]
            public long Id { get; set; }
        }

        // Таблица с id тех записей, которые мы уже просканировали.
        // Запоминается так же количество лайков и комментариев к этой записи чтобы
        // потом повторно сканировать запись где что-то изменилось в большую сторону
        public class ScannedPost
        {
            // Id группы, в которой была написана запись
            public long GroupId { get; set; }

            // Id записи в этой группе
            public long PostId { get; set; }

            // Счётчик лайков. Если он изменится в большую сторону, то будем
            // сканировать запись повторно
            public int LikesCount { get; set; }

            // Счётчик комментариев. Если он изменится в большую сторону, то будем
            // сканировать запись повторно
            public int CommentsCount { get; set; }
        }

        // Маркер для служебного использования. Менять не нужно
        public const string INTERNAL_DB_MARKER = "db";

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
            catch (Exception ex)
            {
                Utils.Log("Не удалось инициализировать базу данных " + Constants.DATABASE_FILENAME + " для хранения базы данных. Причина: " + ex.Message, LogLevel.ERROR);
                throw;
            }

            Utils.Log("Обновляем таблицы базы данных, если это необходимо", LogLevel.GENERAL);

            try
            {
                UpdateTables();
            }
            catch (Exception ex)
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
                db.CreateTables<GroupToReceiveInfo, Group>();
                db.CreateTables<LikeActivity, PostActivity, CommentActivity>();
                db.CreateTables<ScannedProfile, ScannedPost>();
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
                    catch (Exception ex)
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

        /// <summary>
        /// Добавляет новую запись в таблицу или изменяет её, если запись уже существует
        /// </summary>
        public bool InsertOrReplace<T>(T target) where T : new()
        {
            var rowsModified = 0;
            ForDatabaseLocked((db) =>
            {
                rowsModified = db.InsertOrReplace(target);
            });

            return rowsModified > 0;
        }

        /// <summary>
        /// Удаляет запись
        /// </summary>
        public bool Delete(object objectToDelete)
        {
            var rowsModified = 0;
            ForDatabaseLocked((db) =>
            {
                rowsModified = db.Delete(objectToDelete);
            });

            return rowsModified > 0;
        }

        /// <summary>
        /// Удаляет запись типа T по PrimaryKey
        /// </summary>
        public bool Delete<T>(object primaryKey)
        {
            var rowsModified = 0;
            ForDatabaseLocked((db) =>
            {
                rowsModified = db.Delete<T>(primaryKey);
            });

            return rowsModified > 0;
        }

        /// <summary>
        /// Запись уже существует?
        /// </summary>
        public bool IsAlreadyExists<T>(object primaryKey) where T : new()
        {
            var result = false;
            ForDatabaseUnlocked((db) =>
            {
                result = db.Find<T>(primaryKey) != null;
            });

            return result;
        }

        /// <summary>
        /// Вызывает callback на запись, если она существует
        /// </summary>
        public void For<T>(object primaryKey, Callback<T> callback) where T : new()
        {
            ForDatabaseUnlocked((db) =>
            {
                var record = db.Find<T>(primaryKey);
                if (record != null)
                {
                    callback(record);
                }
            });
        }

        /// <summary>
        /// Вызывает callback для каждой записи
        /// </summary>
        public void ForEach<T>(Callback<T> callback) where T : new()
        {
            ForDatabaseUnlocked((db) =>
            {
                foreach (var record in db.Table<T>())
                {
                    callback(record);
                }
            });
        }

        /// <summary>
        /// Берёт несколько записей из таблицы и возвращает список этих записей
        /// </summary>
        public List<T> Take<T>(int amount) where T : new()
        {
            var result = new List<T>();
            ForDatabaseUnlocked((db) =>
            {
                result = db.Table<T>()
                    .Take(amount)
                    .ToList();
            });

            return result;
        }

        /// <summary>
        /// Уменьшает размер базы на диске, если были удалены какие-то поля или таблицы
        /// </summary>
        void Vacuum()
        {
            ForDatabaseUnlocked((db) =>
            {
                db.Execute("vacuum");
            });
        }

        public bool IsSettingsValid()
        {
            var result = false;
            For<Settings>(INTERNAL_DB_MARKER, (settings) =>
            {
                result = settings.ApplicationId > 0
                      && !string.IsNullOrEmpty(settings.Login)
                      && !string.IsNullOrEmpty(settings.Password);
            });

            return result;
        }

        public void ShowStatistics()
        {
            Utils.Log("Статистика:", LogLevel.SUCCESS);
            Utils.Log("    Всего найдено полезных профилей: " + GetCount<Profile>(), LogLevel.NOTIFY);
            Utils.Log("    Количество групп для сканирования: " + GetCount<Group>(), LogLevel.NOTIFY);
            Utils.Log("    Просканировано профилей: " + GetCount<ScannedProfile>(), LogLevel.NOTIFY);
            Utils.Log("    Просканировано записей: " + GetCount<ScannedPost>(), LogLevel.NOTIFY);
        }

        /// <summary>
        /// Вызывает callback для каждой группы с которой можно взаимодействовать
        /// </summary>
        public void ForEachInteractableGroup(Callback<Group> callback)
        {
            ForEach<Group>((group) =>
            {
            	if (group.CanInteract())
                {
                    callback(group);
                }
            });
        }

        /// <summary>
        /// Вызывает callback на группу, к которой мы присоеденились (если закрытая), с которой можно взамодействовать
        /// и с которой дольше всего не взаимодействовали
        /// </summary>
        public void ForBestGroupToInteract(Callback<Group> callback)
        {
            ForDatabaseUnlocked((db) =>
            {
                var allGroups = db.Table<Group>().ToList();
                allGroups.RemoveAll(_ =>
                    !_.CanInteract() || _.IsWantToJoin()
                );
                var targetGroup = allGroups.OrderBy(_ => _.LastScanned).FirstOrDefault();
                if (targetGroup != null)
                {
                    callback(targetGroup);
                }
            });
        }

        /// <summary>
        /// Вызывает callback на закрытую группу, в которой ещё нету членства и с которой можно взамодействовать
        /// </summary>
        public void ForFirstInteractableWantToJoinGroup(Callback<Group> callback)
        {
            ForDatabaseUnlocked((db) =>
            {
                var allGroups = db.Table<Group>().ToList();
                allGroups.RemoveAll(_ =>
                    !_.CanInteract() || !_.IsWantToJoin()
                );
                var targetGroup = allGroups.FirstOrDefault();
                if (targetGroup != null)
                {
                    callback(targetGroup);
                }
            });
        }

        /// <summary>
        /// Вызывает callback на запись из группы, если она уже просканирована
        /// </summary>
        public void ForScannedPostInfo(long groupId, long postId, Callback<ScannedPost> callback)
        {
            ForDatabaseUnlocked((db) =>
            {
                var result = db.Table<ScannedPost>()
                    .Where(_ => (_.GroupId == groupId) && (_.PostId == postId))
                    .FirstOrDefault();
                if (result != null)
                {
                    callback(result);
                }
            });
        }
    }
}