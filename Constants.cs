namespace VK_Unicorn
{
    public class Constants
    {
        public const string APP_NAME = "VK Unicorn";
        public const string APP_VERSION = "v1.0";

        public const int WEB_PORT = 5051;
        public static string RESULTS_WEB_PAGE = "http://localhost:" + WEB_PORT + "/";
        public const string PROJECT_WEB_PAGE = "https://github.com/VKUnicorn/VK-Unicorn";
        public const string SERVER_NAME = "VKUnicornWebServer";

        public const string DATABASE_FILENAME = "database.db";

        public const string DEFAULT_STOP_WORDS = "Проездом;Уверенного;порно;прон;фотограф;живу одна;http;архив;куни без секса;лайкам;репост;выберу;видео;куни;вирт;бесплатно;прогноз;ставк;свинг;.com;.ru;подработ";
        public const char STOP_WORDS_SEPARATOR = ';';
    }
}