using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VParser.src.xiaohongshuDownloader
{
    class XiaohongshuTools
    {
        private static readonly object logLock = new();
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
                    updatedFiles.Add($"https://sns-video-bd.xhscdn.com/{file}");
                    updatedFiles.Add($"https://sns-webpic.xhscdn.com/spectrum/{file}?imageView2/2/w/0/format/png");
                    updatedFiles.Add($"https://sns-img-hw.xhscdn.com/notes_pre_post/{file}?imageView2/2/w/0/format/png");
                    updatedFiles.Add($"https://sns-video-bd.xhscdn.com/pre_post/{file}");
                    updatedFiles.Add($"https://sns-webpic.xhscdn.com/notes_pre_post/{file}?imageView2/2/w/0/format/png");
                    updatedFiles.Add($"https://sns-webpic.xhscdn.com/{file}?imageView2/2/w/0/format/png");
                    updatedFiles.Add($"https://sns-webpic.xhscdn.com/notes_uhdr/{file}?imageView2/2/w/0/format/png");
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
            // Directory setup
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string XiaohongshuDownloadFolderPath = Path.Combine(exeDirectory, "XiaohongshuDownload");
            string XiaohongshuPostDownloadFolderPath = Path.Combine(XiaohongshuDownloadFolderPath, ExtractNameFromUrl(mainLink));

            Directory.CreateDirectory(XiaohongshuPostDownloadFolderPath);

            // Limit simultaneous downloads to 15
            using SemaphoreSlim semaphore = new SemaphoreSlim(15);

            var downloadTasks = new List<Task>();

            foreach (var url in urls)
            {
                await semaphore.WaitAsync();

                downloadTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        // Prepare file name
                        string fileName = Path.GetFileName(new Uri(url).AbsolutePath);

                        if (string.IsNullOrWhiteSpace(fileName) || !fileName.Contains('.'))
                        {
                            if (url.Contains("png") || url.Contains("format/png"))
                                fileName += ".png";
                            else if (url.Contains("mp4") || url.Contains("/stream/"))
                                fileName += ".mp4";
                            else
                                fileName += ".mp4"; // fallback
                        }

                        string filePath = Path.Combine(XiaohongshuPostDownloadFolderPath, fileName);

                        var handler = new HttpClientHandler()
                        {
                            AllowAutoRedirect = true
                        };

                        using var client = new HttpClient(handler)
                        {
                            Timeout = Timeout.InfiniteTimeSpan // allow long downloads
                        };

                        // Timeout only for connecting, not downloading
                        using var connectTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                        HttpResponseMessage? response = await TryDownloadAsync(url, client, connectTimeout, mainLink);

                        bool shouldTryJpg = url.Contains("png") && (response == null || response.StatusCode == HttpStatusCode.BadRequest);

                        if (shouldTryJpg)
                        {
                            if (response != null)
                                response.Dispose();

                            string jpgUrl = url[..^3] + "jpg";

                            lock (logLock)
                            {
                                File.AppendAllText("download_errors.log",
                                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | link: {mainLink} | {jpgUrl} | Trying to download jpg{Environment.NewLine}");
                            }

                            response = await TryDownloadAsync(url, client, connectTimeout, mainLink);

                            if (response == null)
                            {
                                return;
                            }
                        }

                        response.EnsureSuccessStatusCode();

                        // Stream download (no timeout)
                        await using var httpStream = await response.Content.ReadAsStreamAsync();
                        await using var fileStream = File.Create(filePath);

                        await httpStream.CopyToAsync(fileStream);
                    }
                    catch (Exception ex)
                    {
                        lock (logLock)
                        {
                            File.AppendAllText("download_errors.log", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | link: {mainLink} | {url} | {ex.Message}{Environment.NewLine}");
                        }
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

        public static async Task<HttpResponseMessage?> TryDownloadAsync(string urlToDownload, HttpClient client, CancellationTokenSource connectTimeout, string mainLink)
        {
            try
            {
                return await client.GetAsync(
                    urlToDownload,
                    HttpCompletionOption.ResponseHeadersRead,
                    connectTimeout.Token
                );
            }
            catch (OperationCanceledException)
            {
                lock (logLock)
                {
                    File.AppendAllText("download_errors.log",
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | link: {mainLink} | {urlToDownload} | connect timeout{Environment.NewLine}");
                }
                return null;
            }
        }
    }
}
