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
        /// <summary>
        /// currently not in use. Maybe I need a more universal way so that I don't have to set all the parameters here.
        /// </summary>
        /// <param name="iniFilePath"></param>
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

        /// <summary>
        /// Read links from file (support Douyin and zcool)
        /// </summary>
        /// <param name="siteName"> can be  Douyin or zcool</param>
        /// <returns></returns>
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

        /// <summary>
        /// Remove Duplicate Lines from file
        /// </summary>
        /// <param name="filePath"></param>
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

        /// <summary>
        /// Download files from URLs
        /// </summary>
        /// <param name="srcUrlsFile">file with urls to download</param>
        /// <param name="downloadFolder">folder for downloading files</param>
        /// <returns></returns>
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

        /////////////////////////////////////////XIAOHONGSHU//////////////////////////////////////////////////////////////////////////////////////

        // Метод для извлечения имен изображений
        /// <summary>
        /// Extracts all image names from an HTML file
        /// </summary>
        /// <param name="htmlPath"></param>
        /// <returns>image names</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static List<string> XiaohongshuExtractImageNames(string htmlPath)
        {
            if (!File.Exists(htmlPath))
                throw new FileNotFoundException("HTML file not found.", htmlPath);

            string html = File.ReadAllText(htmlPath);

            var regex = new Regex(@"1040[a-z0-9]+", RegexOptions.IgnoreCase);

            var result = new HashSet<string>();
            foreach (Match m in regex.Matches(html))
            {
                result.Add(m.Value);
            }

            return new List<string>(result);
        }

        // Метод для извлечения имен видео файлов @"u002F([a-z0-9_]+?)\.mp4" брать с u002F
        //                              без u002F @"(?<=u002F)([a-z0-9_]+?)\.mp4"

        /// <summary>
        /// Extracts all video names from an HTML file
        /// </summary>
        /// <param name="htmlPath"></param>
        /// <returns>video names</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static List<string> XiaohongshuExtractVideoNames(string htmlPath)
        {
            if (!File.Exists(htmlPath))
                throw new FileNotFoundException("HTML file not found.", htmlPath);

            string html = File.ReadAllText(htmlPath);

            var regex = new Regex(@"(?<=u002F)([a-z0-9_]+?)\.mp4", RegexOptions.IgnoreCase);

            var result = new HashSet<string>();
            foreach (Match m in regex.Matches(html))
            {
                result.Add(m.Value);
            }

            return new List<string>(result);
        }

        /// <summary>
        /// Substitutes the necessary domain names and media file parameters to get a link to the best quality file
        /// </summary>
        /// <param name="files">file names to processing. Can start with '1040' for images and '0' for videos</param>
        /// <returns></returns>
        public static List<string> XiaohongshuGetURLToAllFiles(List<string> files)
        {
            var updatedFiles = new List<string>();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];

                if (file.StartsWith("1040"))
                {
                    updatedFiles.Add($"https://sns-img-hw.xhscdn.com/{file}?imageView2/2/w/0/format/png");
                    updatedFiles.Add($"https://sns-video-bd.xhscdn.com/spectrum/{file}");
                    updatedFiles.Add($"https://sns-webpic.xhscdn.com/spectrum/{file}?imageView2/2/w/0/format/png");
                    updatedFiles.Add($"https://sns-img-hw.xhscdn.com/notes_pre_post/{file}?imageView2/2/w/0/format/png");

                }
                else if (file.StartsWith("0"))
                {
                    updatedFiles.Add($"https://sns-video-al.xhscdn.com/stream/1/10/19/{file}");
                }
            }
            return updatedFiles;
        }

        /// <summary>
        /// Downloads media files from links and saves them to a folder whose name is taken from the name of the link to the media files
        /// </summary>
        /// <param name="urls">A list of links to files.</param>
        /// <param name="mainLink">Link to the post</param>
        /// <returns>The path to the folder with saved media files</returns>
        public static async Task<string> XiaohongshuFileDownloader(List<string> urls, string mainLink)
        {
            //Directory
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string XiaohongshuDownloadFolderName = "XiaohongshuDownload";
            string XiaohongshuDownloadFolderPath = Path.Combine(exeDirectory, XiaohongshuDownloadFolderName);
            string XiaohongshuPostDownloadFolderName = ExtractNameFromUrl(mainLink);
            string XiaohongshuPostDownloadFolderPath = Path.Combine(XiaohongshuDownloadFolderPath, XiaohongshuPostDownloadFolderName);

            Directory.CreateDirectory(XiaohongshuPostDownloadFolderPath);

            using HttpClient client = new HttpClient();

            // Limit simultaneous downloads to 5
            using SemaphoreSlim semaphore = new SemaphoreSlim(5);

            // Download files async
            var downloadTasks = new List<Task>();

            foreach (var url in urls)
            {
                await semaphore.WaitAsync();

                downloadTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        // Extract filename
                        string fileName = Path.GetFileName(new Uri(url).AbsolutePath);

                        // Add extension if missing
                        if (!fileName.Contains('.'))
                        {
                            if (url.Contains("format/png") || url.EndsWith("png"))
                                fileName += ".png";
                            else if (url.EndsWith(".mp4") || url.Contains("/stream/"))
                                fileName += ".mp4";
                            else
                                fileName += ".bin"; // fallback
                        }

                        string filePath = Path.Combine(XiaohongshuPostDownloadFolderPath, fileName);

                        var bytes = await client.GetByteArrayAsync(url);
                        await File.WriteAllBytesAsync(filePath, bytes);

                        Console.WriteLine($"✅ Downloaded: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to download {url}: {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(downloadTasks);

            return XiaohongshuPostDownloadFolderPath;
        }

        /// <summary>
        /// http://xhslink.com/o/8sDMR7HKKej -> 8sDMR7HKKej
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static string ExtractNameFromUrl(string url)
        {
            // Пример: http://xhslink.com/o/8sDMR7HKKej -> 8sDMR7HKKej
            var match = Regex.Match(url, @"\/([A-Za-z0-9_-]+)$");
            return match.Success ? match.Groups[1].Value : Guid.NewGuid().ToString();
        }
    }
}
