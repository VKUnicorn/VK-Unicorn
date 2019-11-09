using System;

namespace VK_Unicorn
{
    class Timeouts
    {
        /// <summary>
        /// Минимальный таймаут для взаимодействия с API ВКонтакте
        /// Торопиться некуда, лучше сканировать медленно, но зато без угрозы бана аккаунта
        /// Слишком частые запросы это плохо, о лимитах можно почитать тут https://vk.com/dev/api_requests
        /// в разделе "3. Ограничения и рекомендации". В целом рекомендуется обращаться не чаще трёх раз в секунду,
        /// но мы будем сканировать значительно реже, опять же чтобы снизить угрозу бана аккаунта или появления капчи
        /// Тем более мы сканируем некоторые элементы через execute запросы, которые позволяют упаковать множество
        /// обращений в одно в виде скрипта на языке VKScript
        /// </summary>
        public static TimeSpan AFTER_ANY_REQUEST_TO_API = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Таймаут для взаимодействия с сообществом после отправки заявки на вступление.
        /// Обычно за это время бот автоматически принимает заявку на вступление
        /// </summary>
        public static TimeSpan AFTER_GROUP_JOIN_REQUEST_SENT = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Таймаут после повторной проверки не приняли ли нашу заявку на вступление в сообщество
        /// </summary>
        public static TimeSpan AFTER_GROUP_JOIN_REQUEST_NOT_ACCEPTED = TimeSpan.FromHours(1);

        /// <summary>
        /// Таймаут после сканирования сообщества
        /// </summary>
        public static TimeSpan AFTER_GROUP_WAS_SCANNED = TimeSpan.FromMinutes(30);
    }
}