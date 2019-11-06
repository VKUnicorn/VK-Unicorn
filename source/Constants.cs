﻿namespace VK_Unicorn
{
    public class Constants
    {
        public const string APP_NAME = "VK Unicorn";
        public const string APP_VERSION = "v1.0.0";
        public const string PROJECT_WEB_PAGE = "https://github.com/VKUnicorn/VK-Unicorn";
        public const string VK_WEB_PAGE = "https://vk.com/";

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

        // Профили с возрастом ниже этого значения будут видны, но будут явно помечены как потенциально опасные по причине
        // УК РФ Статья 240.1. Получение сексуальных услуг несовершеннолетнего
        // http://www.consultant.ru/document/cons_doc_LAW_10699/dde581e459215d45b7512d19d96d3d5040893d4c/
        public const int MINIMUM_AGE = 18;
    }
}