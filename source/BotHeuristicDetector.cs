using System;
using VkNet.Model;

namespace VK_Unicorn
{
    class BotHeuristicDetector
    {
        public static bool Process(User user, out string reason)
        {
            try
            {
                var status = user.Status.ToLowerInvariant();
                var site = user.Site;

                // vk.cc сокращатель спам ссылок
                if (site.StartsWith("https://vk.cc") || site.StartsWith("http://vk.cc") || site.StartsWith("vk.cc"))
                {
                    reason = "сайт vk.cc";
                    return true;
                }

                // Ну это просто жесть... Написала все на своей стенке!
                if (status.Contains("написал") && status.Contains("стен"))
                {
                    reason = "спам на стене";
                    return true;
                }

                // ШОК, но я решила выложить это.. Все уже на стенке!!
                if (status.Contains("выложи") && status.Contains("стен"))
                {
                    reason = "спам на стене";
                    return true;
                }

                // Быстрее читай мою стену, чтобы узнать как провести со мной время!!!
                if (status.Contains("читай") && status.Contains("стен"))
                {
                    reason = "спам на стене";
                    return true;
                }

                // Переходи и найди меня на этом сайте
                if (status.Contains("переходи") && status.Contains("сайт"))
                {
                    reason = "ссылка на сайт";
                    return true;
                }

                // Ссылка в описании
                if (status.Contains("ссылк") && status.Contains("описани"))
                {
                    reason = "ссылка на сайт";
                    return true;
                }

                // Хочу подробностей
                if (status.Contains("хочу подробностей"))
                {
                    reason = "бот - автоответчик";
                    return true;
                }
            }
            catch (Exception ex)
            {
                Utils.Log("не удалось проверить на бота пользователя " + user.Id + ". Причина: " + ex.Message, LogLevel.ERROR);
            }

            reason = string.Empty;
            return false;
        }
    }
}
