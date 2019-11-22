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
                var status = user.Status != null ? user.Status.ToLowerInvariant() : string.Empty;
                var site = user.Site != null ? user.Site : string.Empty;

                // Эмодзи "мешок с деньгами"
                if (status.Contains(char.ConvertFromUtf32(0x1F4B0)))
                {
                    reason = "эмодзи \"мешок с деньгами\"";
                    return true;
                }

                // Эмодзи "рука вниз"
                if (status.Contains(char.ConvertFromUtf32(0x1F447)))
                {
                    reason = "эмодзи \"рука вниз\"";
                    return true;
                }

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
                if ((status.Contains("читай") || status.Contains("читат")) && status.Contains("стен"))
                {
                    reason = "спам на стене";
                    return true;
                }

                // Только НЕ крайний пост на стене
                if (status.Contains("пост") && status.Contains("стен"))
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

                // Ссылка в контактах
                if (status.Contains("ссылк") && status.Contains("контакт"))
                {
                    reason = "ссылка на сайт";
                    return true;
                }

                // Появился сайт
                if (status.Contains("появился") && status.Contains("сайт"))
                {
                    reason = "ссылка на сайт";
                    return true;
                }

                // Пиши мне и ты узнаешь как зарабатывать от 100$ в месяц без вложений
                if (status.Contains("без вложений"))
                {
                    reason = "ссылка на сайт";
                    return true;
                }

                // Зарабатывай\заработай
                if (status.Contains("зарабатыва") || status.Contains("заработ"))
                {
                    reason = "ссылка на сайт";
                    return true;
                }

                // Ниже ссылочка, там оголяюсь, регистрируйтесь и скидывайте логины в лс, что бы я знала откуда вы и не попала на знакомых
                if (status.Contains("регистр"))
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

                // Я очень редкий исчезающий вид женщин - у меня свои ногти, волосы, брови, ресницы, губы и все остальное!)))
                if (status.Contains("исчезающий вид женщин"))
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
