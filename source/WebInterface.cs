using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace VK_Unicorn
{
    class WebInterface
    {
        public static WebInterface Instance { get; private set; }

        public WebInterface()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        string GetEmbeddedFileByName(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public bool HandleRequest(string target, out byte[] data)
        {
            switch (target)
            {
                case "index":
                    var result = string.Empty;

                    // Загружаем шаблон ответа
                    result = GetEmbeddedFileByName(target + ".html");

                    // Заменяем константы
                    result = result.Replace("$APP_NAME$", Constants.APP_NAME);
                    result = result.Replace("$APP_VERSION$", Constants.APP_VERSION);

                    // Отправляем результат в UTF8 кодировке
                    data = System.Text.Encoding.UTF8.GetBytes(result);
                    return true;
            }

            // Неизвестный запрос
            data = null;
            return false;
        }
    }
}