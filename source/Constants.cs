using System;

namespace VK_Unicorn
{
    public class Constants
    {
        public const string APP_NAME = "VK Unicorn";
        public const string APP_VERSION = "v1.0.0";
        public const string PROJECT_WEB_PAGE = "https://github.com/VKUnicorn/VK-Unicorn";
        public const string VK_WEB_PAGE = "https://vk.com/";
        public const string DONATION_ALERTS_WEB_PAGE = "https://www.donationalerts.com/r/VKUnicorn";

        public const int WEB_PORT = 5051;
        public static readonly string RESULTS_WEB_PAGE = "http://localhost:" + WEB_PORT + "/";
        public const string SERVER_NAME = "VKUnicornWebServer";

        public const string DATABASE_FILENAME = "database.db";

        public const string DEFAULT_STOP_WORDS = "выберу;лайкам;репост;прогноз;ставк;http;.com;.ru;фотограф;свинг;подработ;проездом;архив;видео;порно;прон;вирт;бесплатно";
        public const char STOP_WORDS_SEPARATOR = ';';

        /// <summary>
        /// Id пола ВКонтакте который нам интересен
        /// </summary>
        public const VkNet.Enums.Sex TARGET_SEX_ID = VkNet.Enums.Sex.Female;

        /// <summary>
        /// Максимальная глубина сканирования записей по времени
        /// </summary>
        public static readonly TimeSpan MAX_SCANNING_DEPTH_IN_TIME = TimeSpan.FromDays(60);

        /// <summary>
        /// Максимальное количество лайков которые мы будем сканировать. Пользователь может случайно добавить
        /// сообщество где каждую запись лайкают тысячи пользователей, поэтому есть необходимость в таком ограничении
        /// </summary>
        public static int MAX_LIKES_TO_SCAN = 1000;

        /// <summary>
        /// Максимальное количество комментариев которые мы будем сканировать.
        /// Просто нету необходимости сканировать большое количество комментариев, во многих группах они
        /// к тому же вообще закрыты из-за спама
        /// </summary>
        public static int MAX_COMMENTS_TO_SCAN = VkLimits.WALL_GET_COMMENTS_COUNT;

        /// <summary>
        /// Максимальное количество лайков к комментариев которые мы будем сканировать.
        /// </summary>
        public static int MAX_COMMENT_LIKES_TO_SCAN = VkLimits.LIKES_GET_LIST_COUNT;

        /// <summary>
        /// Id города, когда он неизвестен
        /// </summary>
        public static int UNKNOWN_CITY_ID = 0;
    }
}