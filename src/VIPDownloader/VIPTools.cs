using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VParser.src.VIPDownloader
{
    class VIPTools
    {
        /// <summary>
        /// processing of received links to images and videos from the website vip.com
        /// Example ????????????????????????????
        /// </summary>
        /// <param name="path"></param>
        public static void VIPCleanUrlsInFile(string path)
        {
            var cleanedLines = new List<string>();
            foreach (var line in File.ReadAllLines(path))
            {
                string trimmed = line.Trim();

                // Пропускаем h2a-ссылки, кроме mp4
                if (trimmed.Contains("h2a") && !trimmed.EndsWith(".mp4"))
                    continue;

                // Обработка jpg: удаляем суффикс до двух последних подчёркиваний
                if (trimmed.EndsWith(".jpg"))
                {
                    // Находим последние два подчёркивания перед .jpg
                    var match = Regex.Match(trimmed, @"_(?:[^_]+_){1}[^_]+(?=\.jpg)");
                    if (match.Success)
                    {
                        // Удаляем этот суффикс
                        trimmed = trimmed.Replace(match.Value, "");
                    }
                }

                cleanedLines.Add(trimmed);
            }

            File.WriteAllLines(path, cleanedLines);
        }
    }
}
