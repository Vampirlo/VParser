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

            manager.WritePrivateString("PROXY", "UseProxy", "false");
            manager.WritePrivateString("PROXY", "ProxySettings", "");

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
        public static async Task DownloadFilesFromUrls(string srcUrlsFile, string downloadFolder, bool useUniqueNames)
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
                        string fileName;
                        if (useUniqueNames)
                        {
                            var extension = Path.GetExtension(new Uri(url).LocalPath);
                            var uniqueName = $"{Guid.NewGuid()}{extension}";
                            fileName = Path.Combine(downloadFolder, uniqueName);
                        }
                        else
                        {
                            fileName = Path.Combine(downloadFolder, Path.GetFileName(new Uri(url).LocalPath));
                        }

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
        /*
         YAPPY

namespace VParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string iniFileName = "Settings.ini";
            string iniFIlepath = Path.Combine(exeDirectory, iniFileName);
            string XiaohongshuDownloadDirectory = string.Empty;
            string yappyLinks = (Path.Combine(exeDirectory, "links yappy.txt"));
            int MinutesToWaitSiteLoading = 30;


            string DownloadDirectory = Path.Combine(exeDirectory, "Downloads"); //затем считывать из ini, если пусто - задавать автоматически

            VParser.src.tools.ConvertYappyLinksToRutubeCDN(yappyLinks);
            await VParser.src.tools.DownloadFilesFromUrls(yappyLinks, DownloadDirectory, useUniqueNames: true);
        }
    }
}


         */
        public static void ConvertYappyLinksToRutubeCDN(string filePath)
        {
            // Читаем все строки из файла
            string[] lines = File.ReadAllLines(filePath);

            // Обрабатываем каждую строку
            for (int i = 0; i < lines.Length; i++)
            {
                string originalUrl = lines[i].Trim();
                if (string.IsNullOrEmpty(originalUrl))
                    continue;

                // Пытаемся извлечь UUID из ссылки
                string videoId = ExtractVideoId(originalUrl);
                if (videoId == null)
                    continue; // Пропускаем, если не удалось распарсить

                // Формируем новую ссылку в формате Rutube CDN
                string newUrl = $"https://cdn-st.rutubelist.ru/media/{videoId.Substring(0, 2)}/{videoId.Substring(2, 2)}/{videoId.Substring(4)}/hd.mp4";
                lines[i] = newUrl;
            }

            // Перезаписываем файл
            File.WriteAllLines(filePath, lines);
        }

        static string ExtractVideoId(string url)
        {
            // Паттерн для извлечения UUID из URL (например, "0c0db06331e84206832de99bd1f3e6b9")
            var regex = new Regex(@"([a-f0-9]{32})", RegexOptions.IgnoreCase);
            Match match = regex.Match(url);

            if (match.Success && match.Groups[1].Value.Length == 32)
            {
                return match.Groups[1].Value;
            }

            return null; // Не удалось извлечь ID
        }
    }
}
