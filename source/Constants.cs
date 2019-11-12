using System;

namespace VK_Unicorn
{
    public class Constants
    {
        public const string APP_NAME = "VK Unicorn";
        public const string APP_VERSION = "v1.0.0";
        public const string PROJECT_WEB_PAGE = "https://github.com/VKUnicorn/VK-Unicorn";
        public const string VK_WEB_PAGE = "https://vk.com/";

        public const int WEB_PORT = 5051;
        public static readonly string RESULTS_WEB_PAGE = "http://localhost:" + WEB_PORT + "/";
        public const string SERVER_NAME = "VKUnicornWebServer";

        public const string DATABASE_FILENAME = "database.db";

        public const string DEFAULT_STOP_WORDS = "выберу;лайкам;репост;прогноз;ставк;http;.com;.ru;фотограф;свинг;подработ;проездом;архив;видео;порно;прон;вирт;бесплатно;куни";
        public const char STOP_WORDS_SEPARATOR = ';';

        // Id пола ВКонтакте который нам интересен
        public const VkNet.Enums.Sex TARGET_SEX_ID = VkNet.Enums.Sex.Female;

        // Максимальная глубина сканирования записей по времени
        public static readonly TimeSpan MAX_SCANNING_DEPTH_IN_TIME = TimeSpan.FromDays(60);

        // Максимальное количество лайков которые мы будем сканировать. Пользователь может случайно добавить
        // сообщество где каждую запись лайкают тысячи пользователей, поэтому есть необходимость в таком ограничении
        public static int MAX_LIKES_TO_SCAN = 1000;

        // Id города, когда он незивестен
        public static int UNKNOWN_CITY_ID = 0;
    }
}