using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/*
         YAPPY example

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

namespace VParser.src.Yappy
{
    class YappyDownloader
    {
        /// <summary>
        /// Generates a url to a media file from a link to a Yappy post
        /// </summary>
        /// <param name="filePath"></param>
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

        /// <summary>
        /// Extract UUID from url for Yappy (maybe can be merged with ExtractNameFromUrl())
        /// </summary>
        /// <param name="url"></param>
        /// <returns>URL UUID</returns>
        private static string ExtractVideoId(string url)
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

        /// <summary>
        /// Download files from URLs with GUID(UUID) name generation for files
        /// </summary>
        /// <param name="srcUrlsFile"></param>
        /// <param name="downloadFolder"></param>
        /// <param name="useUniqueNames"></param>
        /// <returns></returns>
        public static async Task DownloadFilesFromUrls(string srcUrlsFile, string downloadFolder, bool useUniqueNames)
        {
            var urls = await File.ReadAllLinesAsync(srcUrlsFile);
            int maxParallelism = Environment.ProcessorCount;

            using var semaphore = new SemaphoreSlim(maxParallelism);
            using var httpClient = new HttpClient();

            var tasks = new List<Task>();

            foreach (var url in urls)
            {
                await semaphore.WaitAsync();

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
    }
}
