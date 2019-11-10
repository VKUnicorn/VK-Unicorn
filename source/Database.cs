using SQLite;
using System;
using System.Linq;
using System.Linq.Expressions;
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
            // Пользователь не скрыт
            NOT_HIDDEN,
            // Пользователь скрыт пока не появится ещё какая-то активность
            HIDDEN_UNTIL_ACTIVITY,
        }

        // Таблица с сообществами о которых ещё предстоит получить информацию и добавить в обычную
        // таблицу с сообществами, если там ещё нету такой же
        public class GroupToReceiveInfo
        {
            // Введённый пользователем id сообщества. Идентификатор или короткое имя сообщества
            // Например пользователь ввёл адрес https://vk.com/public1 или https://vk.com/club1
            // Это всё адреса одного и того же сообщества https://vk.com/apiclub, но по public1, как и
            // по club1 получить информацию по запросу wall.get нельзя. Чтобы не создавать путаницу
            // мы просто получим реальный id сообщества через запрос groups.getById и дальше уже
            // будем работать только с ним. Нам всё равно вызывать этот запрос для получения имени
            // сообщества и всех других данных
            [PrimaryKey, Unique]
            public string DomainName { get; set; }
        }

        // Таблица с сообществами
        public class Group
        {
            // Id сообщества. Например у сообщества "ВКонтакте API" этот Id равен единице https://vk.com/public1
            // Не тот, который ввёл пользователь, а тот, который получим с сервера потом сами
            [PrimaryKey, Unique]
            public long Id { get; set; }

            // Название сообщества
            public string Name { get; set; }

            // Короткий адрес сообщества. Сохраняем его для открытия ссылок
            public string ScreenName { get; set; }

            // Статус закрытости сообщества
            public bool IsClosed { get; set; }

            // Статус членства в сообществе. Актуально только для закрытых сообществ
            public bool IsMember { get; set; }

            // URL главной фотографии в максимальном размере
            public string PhotoURL { get; set; }

            // Как давно было найдено чего-нибудь полезное в этом сообществе
            public DateTime LastActivity { get; set; }

            // Как давно был последний успешный скан сообщества
            public DateTime LastScanned { get; set; }

            // Дата когда будет разрешено в следующий раз взаимодейстовать с сообществом
            // Например мы отправили заявку в сообщество и будем пытаться сканировать сообщество только через минут пять
            // А так же нету смысла сканировать сообщество прям сразу же после того как просканировали его, можно подождать минут 30
            public DateTime InteractTimeout { get; set; }

            /// <summary>
            /// Сколько пользователей было найдено с этого сообщества
            /// </summary>
            public int GetEfficiency()
            {
                var result = 0;
                Instance.ForDatabaseUnlocked((db) =>
                {
                    result = db.Table<User>().Where(_ => _.FromGroupId == Id).Count();
                });

                return result;
            }

            /// <summary>
            /// Возвращает ссылку на сообщество
            /// </summary>
            public string GetURL()
            {
                return Constants.VK_WEB_PAGE + ScreenName;
            }

            /// <summary>
            /// Это закрытое сообщество в которое ещё не вступили?
            /// </summary>
            public bool IsWantToJoin()
            {
                return IsClosed && !IsMember;
            }

            /// <summary>
            /// Прошло ли время после которого разрешено взаимодействовать с сообществом
            /// </summary>
            public bool CanInteract()
            {
                return DateTime.Now > InteractTimeout;
            }

            /// <summary>
            /// Устанавливаем таймаут на дальнейшее взаимодействие с сообществом
            /// </summary>
            public void SetInteractTimeout(TimeSpan timeSpan)
            {
                Instance.ModifyFields<Group>(Id, (group) =>
                {
                    group.InteractTimeout = DateTime.Now.Add(timeSpan);
                });
            }

            /// <summary>
            /// Помечаем сообщество как только что просканированное
            /// </summary>
            public void MarkAsJustScanned()
            {
                Instance.ModifyFields<Group>(Id, (group) =>
                {
                    group.LastScanned = DateTime.Now;
                });
            }

            /// <summary>
            /// Помечаем сообщество как только что активное
            /// </summary>
            public void MarkAsJustActive()
            {
                Instance.ModifyFields<Group>(Id, (group) =>
                {
                    group.LastActivity = DateTime.Now;
                });
            }

            /// <summary>
            /// Возвращает Id со знаком минус. Используется в API запросах
            /// </summary>
            public long GetNegativeIdForAPI()
            {
                return -Id;
            }
        }

        // Основная таблица с интересными нам пользователями
        public class User
        {
            // Id пользователя. Например у Павла Дурова этот Id равен единице - https://vk.com/id1
            [PrimaryKey, Unique]
            public long Id { get; set; }

            // Имя
            public string FirstName { get; set; }

            // Фамилия
            public string LastName { get; set; }

            // Дата рождения
            public DateTime BirthDate { get; set; }

            // Id города
            public long CityId { get; set; }

            // Что было написано в статусе
            public string Status { get; set; }

            // Мобильный телефон
            public string MobilePhone { get; set; }

            // Домашний телефон
            public string HomePhone { get; set; }

            // URL главной фотографии в максимальном размере
            public string PhotoURL { get; set; }

            // Когда была последняя активность этого пользователя
            public DateTime LastActivity { get; set; }

            // Когда этот пользователь был добавлен в базу данных
            public DateTime WhenAdded { get; set; }

            // С какого сообщества был добавлен пользователь
            public long FromGroupId { get; set; }

            // Скрыт ли этот пользователь
            public HiddenStatus IsHidden { get; set; }

            // Добавлен ли этот пользователь в избранное
            public bool IsFavorite { get; set; }

            /// <summary>
            /// Возвращает ссылку на пользователя
            /// </summary>
            public string GetURL()
            {
                return Constants.VK_WEB_PAGE + "id" + Id;
            }
        }

        // Таблица с действиями пользователя
        public class UserActivity
        {
            public enum ActivityType
            {
                POST,
                LIKE,
                COMMENT,
                COMMENT_LIKE,
            }

            // Тип активности
            public ActivityType Type { get; set; }

            // Id пользователя, который что-то сделал
            [PrimaryKey]
            public long UserId { get; set; }

            // Id сообщества в которой была запись
            public long GroupId { get; set; }

            // Id записи из сообщества
            public long PostId { get; set; }

            // Id комментария к записи. Может быть не определён
            public long CommentId { get; set; }

            // Когда была произведена эта активость
            public DateTime WhenHappened { get; set; }

            // Эта активность - лайк к чему-либо?
            public bool IsLikeToSomething()
            {
                return Type.IsOneOf(ActivityType.LIKE, ActivityType.COMMENT_LIKE);
            }

            // Возвращает клон активности
            public UserActivity ShallowCopy()
            {
                return (UserActivity)MemberwiseClone();
            }
        }

        // Таблица с id тех пользователей, которых мы уже просканировали.
        // Эта таблица нужна чтобы не сканировать сто раз одного и того же пользователя, а
        // следовательно будет отправляться значительно меньше запросов на серверы
        // ВКонтакте. После того как пользователь попадает в эту таблицу мы больше не
        // будем получать никакую информацию о нём в дальнейшем
        public class ScannedUser
        {
            // Id пользователя
            [PrimaryKey, Unique]
            public long UserId { get; set; }
        }

        // Таблица просканированных записей.
        // Запоминается так же количество лайков и комментариев к этой записи чтобы
        // потом повторно сканировать запись где что-то изменилось в большую сторону
        public class Post
        {
            // Id записи и сообщества вместе
            [PrimaryKey, Unique]
            public string Id { get; set; }

            // Создаём скомбинированный Id сообщества и записи
            public static string MakeId(long groupId, long postId)
            {
                return groupId + "_" + postId;
            }

            // Id сообщества, в котором была написана запись
            public long GroupId { get; set; }

            // Id записи в этом сообществе
            public long PostId { get; set; }

            // Счётчик лайков. Если он изменится в большую сторону, то будем
            // сканировать запись повторно
            public int LikesCount { get; set; }

            // Счётчик комментариев. Если он изменится в большую сторону, то будем
            // сканировать запись повторно
            public int CommentsCount { get; set; }

            // Что было в содержимом
            public string Content { get; set; }
        }

        // Маркер для служебного использования. Менять не нужно
        public const string INTERNAL_DB_MARKER = "db";

        // Таблица настроек приложения
        public class Settings
        {
            public enum SearchMethodType
            {
                /// <summary>
                /// Только те пользователи, у которых указан твой город
                /// </summary>
                BY_CITY,
                /// <summary>
                /// Все пользователи из закрытых сообществ, остальные по городу
                /// </summary>
                SMART,
                /// <summary>
                /// Все пользователи c полом Constants.TARGET_SEX_ID. Огромное количество спама и ботов
                /// </summary>
                ALL_OF_TARGET_SEX,
            }

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

            // Метод поиска пользователей
            public SearchMethodType SearchMethod { get; set; }

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

            try
            {
                // Уменьшаем размер базы на диске
                Vacuum();
            }
            catch (Exception ex)
            {
                Utils.Log("Не удалось уменьшить размер базы на диске. Причина: " + ex.Message, LogLevel.ERROR);
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
                db.CreateTables<User, ScannedUser, UserActivity>();
                db.CreateTables<GroupToReceiveInfo, Group>();
                db.CreateTable<Post>();
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
        /// Модифицирует только некоторые поля записи
        /// </summary>
        public void ModifyFields<T>(object primaryKey, Callback<T> callbackToModifyFields) where T : new()
        {
            ForDatabaseUnlocked((db) =>
            {
                var record = db.Find<T>(primaryKey);
                if (record != null)
                {
                    // Модифицируем нужные поля
                    callbackToModifyFields(record);

                    // Сохраняем запись
                    InsertOrReplace(record);
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
        /// Возвращает все записи, которые удовлетворяют критерию
        /// </summary>
        public List<T> GetAllRecords<T>(Expression<Func<T, bool>> where) where T : new()
        {
            var result = new List<T>();

            ForDatabaseUnlocked((db) =>
            {
                result = db.Table<T>()
                    .Where(where)
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

        /// <summary>
        /// Правильно ли заданы настройки программы?
        /// </summary>
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
            Utils.Log("    Полезных пользователей: " + GetCount<User>(), LogLevel.NOTIFY);
            Utils.Log("    Количество сообществ для сканирования: " + GetCount<Group>(), LogLevel.NOTIFY);
            Utils.Log("    Сохранено пользователей: " + GetCount<ScannedUser>(), LogLevel.NOTIFY);
            Utils.Log("    Сохранено записей: " + GetCount<Post>(), LogLevel.NOTIFY);
        }

        /// <summary>
        /// Вызывает callback для каждого сообщества с которым можно взаимодействовать
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
        /// Вызывает callback на сообщество, к которому мы присоеденились (если закрытое), с которым можно взамодействовать
        /// и с которым дольше всего не взаимодействовали. В приоритете идут закрытые сообщества
        /// </summary>
        public void ForBestGroupToInteract(Callback<Group> callback)
        {
            ForDatabaseUnlocked((db) =>
            {
                var allGroups = db.Table<Group>().ToList();
                allGroups.RemoveAll(_ =>
                    !_.CanInteract() || _.IsWantToJoin()
                );
                var targetGroup = allGroups
                    .OrderByDescending(_ => _.IsClosed)
                    .ThenBy(_ => _.LastScanned)
                    .FirstOrDefault();
                if (targetGroup != null)
                {
                    callback(targetGroup);
                }
            });
        }

        /// <summary>
        /// Вызывает callback на закрытое сообщество, в котором ещё нету членства и с котором можно взамодействовать
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
        /// Такая активность пользователя уже была сохранена?
        /// </summary>
        public bool IsUserActivityAlreadyExists(UserActivity activity)
        {
            var result = false;
            ForDatabaseUnlocked((db) =>
            {
                result = db.Table<UserActivity>()
                    .Where(_ =>
                           (_.Type == activity.Type)
                        && (_.UserId == activity.UserId)
                        && (_.GroupId == activity.GroupId)
                        && (_.PostId == activity.PostId)
                        && (_.CommentId == activity.CommentId)
                    )
                    .FirstOrDefault() != null;
            });

            return result;
        }

        /// <summary>
        /// Удаляет все активности пользователя
        /// </summary>
        public void DeleteAllUserActivities(long userId)
        {
            ForDatabaseLocked((db) =>
            {
                db.Execute("DELETE FROM " + typeof(UserActivity).Name + " WHERE UserId = ?", userId);
            });
        }
    }
}