using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VParser.src
{
    internal class tools
    {
        public static void iniFileCreate(string iniFilePath)
        {
            using (File.Create(iniFilePath));
            INIManager manager = new INIManager(iniFilePath);
            manager.WritePrivateString("SETTINGS", "XiaohongshuDownloadDirectory", "");
            manager.WritePrivateString("SETTINGS", "MinutesToWaitSiteLoading", "30");

            Console.WriteLine("ini file was not found. File has been created.");
        }
        public static string[] ReadVideoLinks(string siteName)
        {
            string fileName;

            if (siteName == "Douyin")
                fileName = "Douyin.txt";
            else if (siteName == "zcool")
                fileName = "zcool.txt";
            else
                return new string[0];

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            
            // Если файл существует, возвращаем массив строк
            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath);
            }

            // Если файла нет, возвращаем пустой массив
            return new string[0];
        }
        public static void CleanUrlsInFile(string path)
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

        public static void RemoveDuplicateLines(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[ERROR] Файл не найден: {filePath}");
                return;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                var distinctLines = lines.Distinct();
                File.WriteAllLines(filePath, distinctLines);
                Console.WriteLine($"[INFO] Повторяющиеся строки удалены. Файл обновлён: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка при обработке файла: {ex.Message}");
            }
        }

        public static async Task DownloadFilesFromUrls(string srcUrlsFile, string downloadFolder)
        {
            var urls = await File.ReadAllLinesAsync(srcUrlsFile);
            int maxParallelism = Environment.ProcessorCount;

            using var semaphore = new SemaphoreSlim(maxParallelism);
            using var httpClient = new HttpClient();

            var tasks = new List<Task>();

            foreach (var url in urls)
            {
                await semaphore.WaitAsync(); // ограничиваем параллелизм

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var fileName = Path.Combine(downloadFolder, Path.GetFileName(new Uri(url).LocalPath));
                        var content = await httpClient.GetByteArrayAsync(url);
                        await File.WriteAllBytesAsync(fileName, content);

                        Console.WriteLine($"\"download\"{fileName}");
                        Console.WriteLine($"Поток: {Thread.CurrentThread.ManagedThreadId}, Время: {DateTime.Now}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при загрузке {url}: {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }
    }
}
