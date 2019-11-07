using System;

namespace VK_Unicorn
{
    class Timeouts
    {
        /// <summary>
        /// Таймаут для взаимодействия с группой после отправки заявки на вступление в группу
        /// обычно за это время бот автоматически принимает заявку на вступление
        /// </summary>
        public static TimeSpan AFTER_GROUP_JOIN_REQUEST_SENT = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Таймаут после повторной проверки не приняли ли нашу заявку на вступление в группу
        /// </summary>
        public static TimeSpan AFTER_GROUP_JOIN_REQUEST_NOT_ACCEPTED = TimeSpan.FromHours(1);

        /// <summary>
        /// Таймаут после сканирования группы
        /// </summary>
        public static TimeSpan AFTER_GROUP_WAS_SCANNED = TimeSpan.FromMinutes(30);
    }
}