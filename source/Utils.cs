using System;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VK_Unicorn
{
    public enum LogLevel
    {
        NOTIFY,
        GENERAL,
        SUCCESS,
        WARNING,
        ERROR,
    }

    public enum StatusType
    {
        GENERAL,
        SUCCESS,
        ERROR,
    }

    public static class Utils
    {
        public static void Log(string text, LogLevel logLevel = LogLevel.GENERAL)
        {
            var color = Color.Black;
            var prefix = string.Empty;

            switch (logLevel)
            {
                case LogLevel.ERROR:
                    color = Color.Red;
                    prefix = "Ошибка: ";
                    break;

                case LogLevel.WARNING:
                    color = Color.DarkSalmon;
                    break;

                case LogLevel.NOTIFY:
                    color = Color.Gray;
                    break;

                case LogLevel.SUCCESS:
                    color = Color.DarkGreen;
                    break;
            }

            Log(prefix + text, color);
        }

        public static void Log(string text, Color? color = null)
        {
            text = "[" + DateTime.Now.ToLongTimeString() + "] " + text;

            MainForm.Instance.Invoke((MethodInvoker)delegate
            {
                var logTextBox = MainForm.Instance.GetLogTextBox();

                logTextBox.SuspendLayout();

                var previousSelectionColor = logTextBox.SelectionColor;
                if (color != null)
                {
                    logTextBox.SelectionColor = color.Value;
                }

                if (!string.IsNullOrWhiteSpace(logTextBox.Text))
                {
                    logTextBox.AppendText($"{Environment.NewLine}{text}");
                }
                else
                {
                    logTextBox.AppendText(text);
                }

                logTextBox.ScrollToCaret();
                logTextBox.SelectionColor = previousSelectionColor;
                logTextBox.ResumeLayout();
            });
        }

        /// <summary>
        /// Получаем "1,2,3" из листа "1" "2" 3", если разделитель ","
        /// </summary>
        public static string GenerateSeparatedString<T>(this IEnumerable<T> self, string separator)
        {
            var result = string.Empty;

            var index = 0;
            foreach (var item in self)
            {
                result += item.ToString();

                if (index != self.Count() - 1)
                {
                    result += separator;
                }

                ++index;
            }

            return result;
        }

        public static string GetURL(this VkNet.Model.Group self)
        {
            return Constants.VK_WEB_PAGE + self.ScreenName;
        }

        public static bool IsOneOf<T>(this T valueToFind, params T[] valuesToCheck)
        {
            foreach (var value in valuesToCheck)
            {
                if (valueToFind.Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsNoneOf<T>(this T valueToFind, params T[] valuesToCheck)
        {
            return !IsOneOf(valueToFind, valuesToCheck);
        }

        public static bool IsProfileIdNotGroupId(long id)
        {
            return id > 0;
        }

        static Dictionary<string, byte[]> embeddedFilesCache = new Dictionary<string, byte[]>();
        public static byte[] GetEmbeddedFileByName(string fileName)
        {
            // Ещё нету в кэше?
            if (!embeddedFilesCache.ContainsKey(fileName))
            {
                byte[] result = null;

                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream == null)
                        {
                            return null;
                        }

                        result = new byte[stream.Length];
                        stream.Read(result, 0, result.Length);
                    }
                }
                catch (Exception)
                {
                    // Файл не найден или какая-то ошибка
                    result = null;
                }

                // Добавляем в кэш
                embeddedFilesCache.Add(fileName, result);
            }

            return embeddedFilesCache[fileName];
        }

        public static string FixPhotoURL(string url)
        {
            // Заменяем ссылку на фото, если нужно
            // Это связано с тем, что многие скрипты для uBlock/Adblock блокируют
            // загрузку изображений ВКонтакта с другого домена, в итоге изображение блокируется
            // и дизайн сайта страдает от неправильно отображаемых элементов интерфейса
            if (url.StartsWith(Constants.VK_WEB_PAGE))
            {
                // Удаляем начальный адрес до имени файла
                url = Regex.Replace(url, @".+\/", string.Empty);

                // Удаляем параметры запроса. Например ?ava=1 и т.п.
                url = Regex.Replace(url, @"\?.+$", string.Empty);
            }

            return url;
        }

        public static DateTime GetNowAsUniversalTime()
        {
            return DateTime.Now.ToUniversalTime();
        }
    }

    public delegate void Callback();
    public delegate void Callback<T0>(T0 arg0);
    public delegate void Callback<T0, T1>(T0 arg0, T1 arg1);
    public delegate void Callback<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);
    public delegate void Callback<T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2, T3 arg3);
    public delegate void Callback<T0, T1, T2, T3, T4>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void Callback<T0, T1, T2, T3, T4, T5>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate void Callback<T0, T1, T2, T3, T4, T5, T6>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate void Callback<T0, T1, T2, T3, T4, T5, T6, T7>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate void Callback<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

    public delegate U CallbackWithReturn<U>();
    public delegate U CallbackWithReturn<U, T0>(T0 arg0);
    public delegate U CallbackWithReturn<U, T0, T1>(T0 arg0, T1 arg1);
    public delegate U CallbackWithReturn<U, T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2);
    public delegate U CallbackWithReturn<U, T0, T1, T2, T3>(T0 arg0, T1 arg1, T2 arg2, T3 arg3);
    public delegate U CallbackWithReturn<U, T0, T1, T2, T3, T4>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate U CallbackWithReturn<U, T0, T1, T2, T3, T4, T5>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate U CallbackWithReturn<U, T0, T1, T2, T3, T4, T5, T6>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate U CallbackWithReturn<U, T0, T1, T2, T3, T4, T5, T6, T7>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate U CallbackWithReturn<U, T0, T1, T2, T3, T4, T5, T6, T7, T8>(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
}