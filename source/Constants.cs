namespace VK_Unicorn
{
    public class Constants
    {
        public const string APP_NAME = "VK Unicorn";
        public const string APP_VERSION = "v1.0.0";
        public const string PROJECT_WEB_PAGE = "https://github.com/VKUnicorn/VK-Unicorn";

        public const int WEB_PORT = 5051;
        public static string RESULTS_WEB_PAGE = "http://localhost:" + WEB_PORT + "/";
        public const string SERVER_NAME = "VKUnicornWebServer";

        public const string DATABASE_FILENAME = "database.db";

        public const string DEFAULT_STOP_WORDS = "выберу;лайкам;репост;прогноз;ставк;http;.com;.ru;фотограф;свинг;подработ;проездом;архив;видео;порно;прон;вирт;бесплатно;куни";
        public const char STOP_WORDS_SEPARATOR = ';';

        // Id пола ВКонтакте который нам интересен
        // 0 - мужской
        // 1 - женский
        public const int TARGET_SEX_ID = 1;
    }
}