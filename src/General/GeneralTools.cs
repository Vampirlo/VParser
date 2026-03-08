using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VParser.src.General
{
    class GeneralTools
    {
        /// <summary>
        /// save text to file for VIPDownloader()
        /// </summary>
        /// <param name="text"></param>
        public static void SaveTextToFile(string text)
        {

            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string srcUrlsFileName = "URLS.txt";
            string srcUrlsFile = Path.Combine(exeDirectory, srcUrlsFileName);

            try
            {
                // Убедимся, что директория существует
                string? dir = Path.GetDirectoryName(srcUrlsFile);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Добавляем текст в конец файла с переносом строки
                File.AppendAllText(srcUrlsFile, text + Environment.NewLine);
                Console.WriteLine("Добавлено в файл: " + srcUrlsFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при записи в файл: " + ex.Message);
            }
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

            using var semaphore = new SemaphoreSlim(maxParallelism); // нужно (иметь возможность) делать меньше
            using var httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(50)
            };

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
        /// currently not in use. Maybe I need a more universal way so that I don't have to set all the parameters here.
        /// </summary>
        /// <param name="iniFilePath"></param>
        public static void iniFileCreate(string iniFilePath)
        {
            using (File.Create(iniFilePath)) ;
            INIManager manager = new INIManager(iniFilePath);
            manager.WritePrivateString("SETTINGS", "XiaohongshuDownloadDirectory", "");
            manager.WritePrivateString("SETTINGS", "MinutesToWaitSiteLoading", "30");

            manager.WritePrivateString("PROXY", "UseProxy", "false");
            manager.WritePrivateString("PROXY", "ProxySettings", "");

            Console.WriteLine("ini file was not found. File has been created.");
        }
    }
}
