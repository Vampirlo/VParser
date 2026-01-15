using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeleniumExtras.WaitHelpers;
using System.IO;
using System.Diagnostics;
using System.Collections;
using OpenQA.Selenium.Firefox;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Reflection.Metadata;
using OpenQA.Selenium.Interactions;

/* selenium program.cs
 * 
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Net;
using VParser.src;

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
            int MinutesToWaitSiteLoading = 30;

            string douyinDownloadDirectory = Path.Combine(exeDirectory, "DouyinDowloads"); //затем считывать из ini, если пусто - задавать автоматически
            // перед нажатием на кнопку скачать - каунт файлов сейвить и сравнивать после нажатия на кнопку, и когда каунт не будет равен друг другу - продолжать скачивание
            var options = new ChromeOptions();

            //ini file check
            if (!File.Exists(iniFIlepath))
                VParser.src.tools.iniFileCreate(iniFIlepath);

            INIManager manager = new INIManager(iniFIlepath);

            XiaohongshuDownloadDirectory = manager.GetPrivateString("SETTINGS", "XiaohongshuDownloadDirectory");
            MinutesToWaitSiteLoading = int.Parse(manager.GetPrivateString("SETTINGS", "MinutesToWaitSiteLoading"));

            options.AddUserProfilePreference("download.default_directory", douyinDownloadDirectory);  // Указываем папку загрузки
            options.AddUserProfilePreference("download.prompt_for_download", false);  // Отключаем запрос на подтверждение загрузки
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);  // Отключаем предупреждения Safe Browsing

            //await VParser.src.SeleniumFunctions.MultiplyDouyinVideoDownloadAsync(options);
            await VParser.src.SeleniumFunctions.MultiplyZcoolDownloadAsync(options);
        }
    }
}

 */

namespace VParser.src
{
    internal class SeleniumFunctions
    {
        /// <summary>
        /// Waiting for the xpath element
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="xpath"></param>
        /// <param name="timeoutInSeconds">seconds to wait</param>
        /// <param name="click">element.Click() to appear</param>
        private static void WaitByXPath(IWebDriver driver, string xpath, int timeoutInSeconds = 30, bool click = false)
        {
            IWebElement element = null;
            var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout)
            {
                try
                {
                    element = driver.FindElement(By.XPath(xpath));

                    if (element.Displayed && element.Enabled)
                    {
                        if (click)
                            element.Click();
                        return;
                    }
                }
                catch (NoSuchElementException)
                {
                    // Элемент ещё не появился — продолжаем ждать
                }
                catch (StaleElementReferenceException)
                {
                    // DOM обновился — ждём следующую попытку
                }

                Thread.Sleep(500);
            }

            Console.WriteLine($"[WARNING] Элемент с XPath '{xpath}' не был найден или не стал кликабельным за {timeoutInSeconds} секунд.");
        }

        /// <summary>
        /// Idle verification of the existence of an xpath object
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static bool ElementExists(IWebDriver driver, string xpath)
        {
            try
            {
                var element = driver.FindElement(By.XPath(xpath));
                return element.Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        /// <summary>
        /// save text ro file for VIPDownloader()
        /// </summary>
        /// <param name="text"></param>
        private static void SaveTextToFile(string text)
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
        /// Get all jpg URL from page
        /// можно модернизировать и для всех типов изображений
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        private static List<string> GetAllJpgImageSrcsIncludingCustom(IWebDriver driver)
        {
            var jpgUrls = new List<string>();

            try
            {
                // Найдем все элементы с атрибутом src или data-src
                var elementsWithSrc = driver.FindElements(By.XPath("//*[@src or @data-src]"));

                foreach (var el in elementsWithSrc)
                {
                    string? src = el.GetAttribute("src");
                    string? dataSrc = el.GetAttribute("data-src");

                    // Проверяем оба варианта и добавляем, если jpg
                    if (!string.IsNullOrEmpty(src) && src.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                        jpgUrls.Add(src);
                    else if (!string.IsNullOrEmpty(dataSrc) && dataSrc.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                        jpgUrls.Add(dataSrc);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when searching for images: " + ex.Message);
            }

            return jpgUrls;
        }

        /*
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Net;
using VParser.src;

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
            int MinutesToWaitSiteLoading = 30;
            string srcUrlsFileName = "URLS.txt";
            string srcUrlsFile = Path.Combine(exeDirectory, srcUrlsFileName);
            string downloadFolderName = "Downloads";
            string downloadFolder = Path.Combine(exeDirectory, downloadFolderName);
            Directory.CreateDirectory(downloadFolder);

            string VIPLinksFileName = "VIP.txt";
            string VIPLinksFile = Path.Combine(exeDirectory, VIPLinksFileName);

            string douyinDownloadDirectory = Path.Combine(exeDirectory, "DouyinDowloads"); //затем считывать из ini, если пусто - задавать автоматически
            // перед нажатием на кнопку скачать - каунт файлов сейвить и сравнивать после нажатия на кнопку, и когда каунт не будет равен друг другу - продолжать скачивание
            var options = new ChromeOptions();

            //ini file check
            if (!File.Exists(iniFIlepath))
                VParser.src.tools.iniFileCreate(iniFIlepath);

            INIManager manager = new INIManager(iniFIlepath);

            XiaohongshuDownloadDirectory = manager.GetPrivateString("SETTINGS", "XiaohongshuDownloadDirectory");
            MinutesToWaitSiteLoading = int.Parse(manager.GetPrivateString("SETTINGS", "MinutesToWaitSiteLoading"));

            options.AddUserProfilePreference("download.default_directory", douyinDownloadDirectory);  // Указываем папку загрузки
            options.AddUserProfilePreference("download.prompt_for_download", false);  // Отключаем запрос на подтверждение загрузки
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);  // Отключаем предупреждения Safe Browsing


            if (!File.Exists(VIPLinksFile))
            {
                Console.WriteLine($"Приложение по пути '{VIPLinksFile}' не найдено. Завершение работы.");
                Environment.Exit(1);
            }

            string[] VIPLinksFromFile = File.ReadAllLines(VIPLinksFile);

            foreach (var _url in VIPLinksFromFile)
            {
                if (!string.IsNullOrWhiteSpace(_url))
                {
                    await VParser.src.SeleniumFunctions.VIPDownloader(_url, options);
                }
            }

            VParser.src.tools.VIPCleanUrlsInFile(srcUrlsFile);
            VParser.src.tools.RemoveDuplicateLines(srcUrlsFile);

            await VParser.src.tools.DownloadFilesFromUrls(srcUrlsFile, downloadFolder);
        }
    }
}
         */

        /// <summary>
        /// Download all images and videos from vip.com page
        /// </summary>
        /// <param name="url"></param>
        /// <param name="options"></param>
        public static void VIPDownloader(string url, ChromeOptions options)
        {
        start:

            int pizdec = 0;

            IWebDriver driver = new ChromeDriver(options);

            driver.Navigate().GoToUrl(url);

        tryagain:

            WaitByXPath(driver, "//div[contains(@class, 'uni-modal__btn') and contains(@class, 'uni-modal__btn_default') and normalize-space(text())='Cancel']", click: true);
            WaitByXPath(driver, "/html/body/uni-app/uni-page/uni-page-wrapper/uni-page-body/uni-view/uni-view[1]/uni-view/uni-button", click: true);
            WaitByXPath(driver, "//div[contains(@class, 'uni-modal__btn') and contains(@class, 'uni-modal__btn_default') and normalize-space(text())='Cancel']", click: true);

            // Попробуем найти кнопку воспроизведения — если она есть, кликнем
            string playButtonXPath = "//*[@id=\"brannerViewId\"]/uni-swiper/div/div/div/uni-swiper-item[1]/uni-view[1]/uni-view[2]";

            if (ElementExists(driver, playButtonXPath))
            {
                Console.WriteLine("Есть кнопка воспроизведения");
                WaitByXPath(driver, playButtonXPath, click: true);
                WaitByXPath(driver, "/html/body/uni-app/uni-modal/div[2]/div[3]/div[2]", click: true);

                IWebElement videoElement = driver.FindElement(By.XPath("//*[@id=\"myVideo\"]/div/video"));
                string videoSrc = videoElement.GetAttribute("src");

                SaveTextToFile(videoSrc);
            }
            else
            {
                Console.WriteLine("Кнопка воспроизведения не найдена, продолжаем без неё.");
            }

            List<string> jpgUrls = GetAllJpgImageSrcsIncludingCustom(driver);

            if (jpgUrls.Count < 20)
            {
                pizdec++;
                if (pizdec > 2)
                {
                    driver.Quit();
                    goto start;
                }
                goto tryagain;
            }


            foreach (var singlejpgUrl in jpgUrls)
            {
                SaveTextToFile(singlejpgUrl);
            }

            driver.Close();
        }

        /*
         * For XiaohongshuDownloaderHTML
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Net;
using VParser.src;

using ImageMerger;
using OpenQA.Selenium.Interactions;
using System.Xml;
using Newtonsoft.Json;
using System.Net.Http;
using System;

namespace VParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("How to use:");
                Console.WriteLine("1) To get cookies   : GetCookies    <Domain URL for get cookie>");
                Console.WriteLine("2) To download files: DownloadFiles <Domain URL for set cookie> <URL to download>");
                return;
            }

            string command = args[0];
            string parameter = args[1];
            string? urlToDownload = null; 

            if (args.Length >= 3)
                urlToDownload = args[2];

            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string driverPath = Path.Combine(exeDirectory, "chromedriver.exe");
            string cookiesFileName = "xiaohongshu";
            string cookieFolderName = "cookies";
            string cookieFolderPath = Path.Combine(exeDirectory, cookieFolderName);
            string cookiesFileNameWithExtension = cookiesFileName + ".json";
            string cookieFilePath = Path.Combine(cookieFolderPath, cookiesFileNameWithExtension);
            Directory.CreateDirectory(cookieFolderPath);

            switch (command.ToLower())
            {
                case "getcookies":
                    SeleniumFunctions.GetCookies(parameter, "xiaohongshu");
                    break;

                case "downloadfiles":
                    string htmlFile = await SeleniumFunctions.XiaohongshuDownloaderHTML(urlToDownload, true);

                    string hardcore = "C:\\vs_proj\\VParser\\bin\\Debug\\net8.0\\XiaohongshuDownloaderAllHTMLPages\\2HNLSYbqPW5.html";

                    List<string> imageNames = tools.XiaohongshuExtractImageNames(htmlFile);
                    List<string> videoNames = tools.XiaohongshuExtractVideoNames(htmlFile);

                    List<string> AllFilesNames = imageNames.Concat(videoNames).ToList();

                    List<string> AllFilesURL = tools.XiaohongshuGetURLToAllFiles(AllFilesNames);

                    string FinalDirectotyWithDownloadedFiles = await tools.XiaohongshuFileDownloader(AllFilesURL, urlToDownload);

                    Console.WriteLine(FinalDirectotyWithDownloadedFiles);



                    //await Task.Delay(5000);
                    //Console.WriteLine("C:\\vs_proj\\VParser\\bin\\Debug\\net8.0\\XiaohongshuDownload\\50BNdsgIygg");
                    break;

                default:
                    Console.WriteLine("Unknown command. Use GetCookies or DownloadFiles.");
                    break;
            }
        }
    }
}
        args: 
        DownloadFiles https://www.xiaohongshu.com http://xhslink.com/o/2HNLSYbqPW5
        15.01.2026
         */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="mobileDriver"></param>
        /// <param name="domainURLforSetCookie"></param>
        /// <param name="cookiesFilePath"></param>
        /// <returns>HTML file path</returns>
        public static async Task<string> XiaohongshuDownloaderHTML(string url, bool? mobileDriver = false, string? domainURLforSetCookie = null, string? cookiesFilePath = null)
        {
            var options = new ChromeOptions();
            if (mobileDriver == true)
            {
                options.EnableMobileEmulation("iPhone X");
            }

            IWebDriver driver = new ChromeDriver(options);

            if (!string.IsNullOrEmpty(domainURLforSetCookie) && !string.IsNullOrEmpty(cookiesFilePath))
            {
                SetCookies(driver, domainURLforSetCookie, cookiesFilePath);
            }

            string htmlSitesFolderName = "XiaohongshuDownloaderAllHTMLPages";
            string htmlSitesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, htmlSitesFolderName);
            Directory.CreateDirectory(htmlSitesFolderPath);

            string htmlFileName = tools.ExtractNameFromUrl(url) + ".html";
            string HTMLFilePath = Path.Combine(htmlSitesFolderPath, htmlFileName);

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);

            try
            {
                driver.Navigate().GoToUrl(url);
            }
            catch (WebDriverTimeoutException)
            {
                string pageSource = driver.PageSource;
                File.WriteAllText(HTMLFilePath, pageSource);
                if (pageSource.Contains("1040") || pageSource.Contains(".mp4"))
                {
                    File.WriteAllText(HTMLFilePath, pageSource);
                    driver.Quit();
                    return HTMLFilePath;
                }
                Console.WriteLine("HTML is Empty.");
                driver.Quit();
                Environment.Exit(0);
            }

            // Get HTML
            int maxWaitMilliseconds = 1000;
            int elapsed = 0;

            while (elapsed < maxWaitMilliseconds)
            {
                string pageSource = driver.PageSource;
                File.WriteAllText(HTMLFilePath, pageSource);
                if (pageSource.Contains("1040") || pageSource.Contains(".mp4"))
                {
                    File.WriteAllText(HTMLFilePath, pageSource);
                    driver.Quit();
                    return HTMLFilePath;
                }
                await Task.Delay(10);
                elapsed+= 10;
            }
            driver.Quit();
            Console.WriteLine("HTML is Empty. Xiaohongshu most likely ended the session. Reauthorization is required.");
            Environment.Exit(0);
            return HTMLFilePath; // ну это просто смешно
        }

        /// <summary>
        /// Loads a list of Douyin video URLs, opens each one in a Chrome browser,
        /// extracts the direct video source URL from the <video> tag, and downloads the file.
        /// If the video source is unavailable, it retries several times and logs failed downloads.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task MultiplyDouyinVideoDownloadAsync(ChromeOptions options)
        {
            string[] videoLinks = VParser.src.tools.ReadVideoLinks("Douyin");

            IWebDriver driver = new ChromeDriver(options);
            WebDriverWait waitLong = new WebDriverWait(driver, TimeSpan.FromMinutes(100));
            WebDriverWait waitShort = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            int tryCount;

            foreach (string vidoUrl in videoLinks)
            {
                driver.Navigate().GoToUrl(vidoUrl);
                System.Threading.Thread.Sleep(5000);
                tryCount = 0;

                try
                {
                    var closeButton = waitShort.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.ClassName("douyin-login__close")));
                    closeButton.Click();
                }
                catch (WebDriverTimeoutException)
                {
                    // Если кнопка не появляется, продолжаем без нажатия
                }
                // Находим элемент video
                var videoElement = waitLong.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.TagName("video")));
                // Находим все элементы source внутри video
                var sourceElements = videoElement.FindElements(By.TagName("source"));
                
                // Проверяем, есть ли элементы source
                if (sourceElements.Count > 0)
                {
                    // Получаем URL последнего элемента source
                    string videoUrlToDownload = sourceElements[sourceElements.Count - 1].GetAttribute("src");
                    if (!string.IsNullOrEmpty(videoUrlToDownload))
                    {
                        // код для скачивания видео
                        Console.WriteLine($"Скачивание видео с URL: {videoUrlToDownload}");
                        await VParser.src.FileDownloader.DownloadFileAsyncMP4(videoUrlToDownload);
                    }
                }
                else
                {
                    Console.WriteLine("Нет доступных элементов source для видео.");

                    while (true)
                    {
                        System.Threading.Thread.Sleep(2000);

                        string currentUrl = driver.Url;

                        if (!currentUrl.Contains(vidoUrl))
                        {
                            driver.Navigate().GoToUrl(vidoUrl);
                            System.Threading.Thread.Sleep(5000);

                            try
                            {
                                var closeButton = waitShort.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.ClassName("douyin-login__close")));
                                closeButton.Click();
                            }
                            catch (WebDriverTimeoutException)
                            {
                                // Если кнопка не появляется, продолжаем без нажатия
                            }
                        }

                        videoElement = waitLong.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.TagName("video")));
                        sourceElements = videoElement.FindElements(By.TagName("source"));

                        currentUrl = driver.Url;
                        if (sourceElements.Count > 0 || currentUrl.Contains(vidoUrl))
                        {
                            string videoUrlToDownload = sourceElements[sourceElements.Count - 1].GetAttribute("src");

                            if (!string.IsNullOrEmpty(videoUrlToDownload))
                            {
                                // код для скачивания видео
                                Console.WriteLine($"Скачивание видео с URL: {videoUrlToDownload}");
                                await VParser.src.FileDownloader.DownloadFileAsyncMP4(videoUrlToDownload);
                            }

                            break;
                        }

                        tryCount++;
                        if (tryCount > 5)
                        {
                            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                            string unsuccessfulDownloads = "UnsuccessfulDownloads.txt";
                            string unsuccessfulDownloadsPath = Path.Combine(exeDirectory, unsuccessfulDownloads);
                            File.AppendAllText(unsuccessfulDownloadsPath, vidoUrl);

                            break;
                        }
                    }
                }

            }

            driver.Quit();  // Закрываем браузер после завершения загрузок
        }

        /// <summary>
        /// Loads a list of Zcool project URLs, opens each page in a Chrome browser,
        /// extracts all full-resolution image URLs from specific content sections,
        /// filters out placeholders and duplicates, and downloads every valid image.
        /// The method scrolls the page to ensure lazy-loaded images are retrieved.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task MultiplyZcoolDownloadAsync(ChromeOptions options)
        {
            string[] videoLinks = VParser.src.tools.ReadVideoLinks("zcool");
            List<string> imageUrls = new List<string>();
            int time;

            IWebDriver driver = new ChromeDriver(options);
            WebDriverWait waitLong = new WebDriverWait(driver, TimeSpan.FromMinutes(100));
            WebDriverWait waitShort = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            foreach (string vidoUrl in videoLinks)
            {
                // очищать список src с линками
                imageUrls.Clear();
                time = 1000;

                driver.Navigate().GoToUrl(vidoUrl);

                waitLong.Until(drv => drv.FindElements(By.CssSelector("div[style*='margin-bottom:32px;margin-top:32px']")).Count > 0);

                // Прокрутка вниз и вверх
                ScrollDown(driver);
                System.Threading.Thread.Sleep(time);
                ScrollUp(driver); 

                // Находим все div элементы с заданными стилями
                var divs = driver.FindElements(By.CssSelector("div[style*='margin-bottom:32px;margin-top:32px']"));

                foreach (var div in divs)
                {
                    // Находим все изображения <img> внутри данного div
                    var imgElements = div.FindElements(By.TagName("img"));

                    foreach (var img in imgElements)
                    {
                        // Извлекаем значение атрибута src
                        string src = img.GetAttribute("src");
                        if (!string.IsNullOrEmpty(src))
                        {
                            imageUrls.Add(src);
                        }
                    }
                }
                // Обрезаем каждый URL до символа "?" и удаляем дубликаты
                imageUrls = imageUrls
                    .Select(url =>
                    {
                        int questionMarkIndex = url.IndexOf("?");
                        return questionMarkIndex > 0 ? url.Substring(0, questionMarkIndex) : url;
                    })
                    .Distinct() // Удаляем дубликаты
                    .ToList();

                // Проверяем, содержит ли какой-либо элемент подстроку "bg-placeholder.jpg"
                bool containsPlaceholder = imageUrls.Any(url => url.Contains("bg-placeholder.jpg"));

                while (containsPlaceholder)
                {
                    imageUrls.Clear();

                    // Прокрутка вниз и вверх
                    ScrollDown(driver);
                    System.Threading.Thread.Sleep(time);
                    ScrollUp(driver);

                    foreach (var div in divs)
                    {
                        // Находим все изображения <img> внутри данного div
                        var imgElements = div.FindElements(By.TagName("img"));

                        foreach (var img in imgElements)
                        {
                            // Извлекаем значение атрибута src
                            string src = img.GetAttribute("src");
                            if (!string.IsNullOrEmpty(src))
                            {
                                imageUrls.Add(src);
                            }
                        }
                    }
                    // Обрезаем каждый URL до символа "?" и удаляем дубликаты
                    imageUrls = imageUrls
                        .Select(url =>
                        {
                            int questionMarkIndex = url.IndexOf("?");
                            return questionMarkIndex > 0 ? url.Substring(0, questionMarkIndex) : url;
                        })
                        .Distinct() // Удаляем дубликаты
                        .ToList();

                    containsPlaceholder = imageUrls.Any(url => url.Contains("bg-placeholder.jpg"));
                }

                //скачивать
                foreach (var url in imageUrls)
                {
                    await VParser.src.FileDownloader.DownloadFileAsyncZcool(url);
                }
            }

            driver.Quit();
        }
        private static void ScrollDown(IWebDriver driver)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            while (true)
            {
                var scrollHeightA = (long)js.ExecuteScript("return window.scrollY;");

                js.ExecuteScript($"window.scrollBy(0, 100);");
                System.Threading.Thread.Sleep(10);

                var scrollHeightB = (long)js.ExecuteScript("return window.scrollY;");
                var documentHeight = Convert.ToInt64(js.ExecuteScript("return document.body.scrollHeight;"));

                if (scrollHeightA == scrollHeightB || scrollHeightB + 200 >= documentHeight)
                {
                    break;
                }
            }
        }

        private static void ScrollUp(IWebDriver driver)
        {

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            while (true)
            {
                var scrollHeightA = (long)js.ExecuteScript("return window.scrollY;");

                js.ExecuteScript($"window.scrollBy(0, -200);");
                System.Threading.Thread.Sleep(10);

                var scrollHeightB = (long)js.ExecuteScript("return window.scrollY;");
                var documentHeight = Convert.ToInt64(js.ExecuteScript("return document.body.scrollHeight;"));

                if (scrollHeightA == scrollHeightB || scrollHeightB + 200 >= documentHeight)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// The cookie name is passed without the json extension.
        /// The file will be saved at exe/cookies/cookiesFileName.json
        /// </summary>
        /// <param name="URL">example https://www.xiaohongshu.com/</param>
        /// <param name="cookiesFileName">example xiaohongshu</param>
        /// <returns>Path to cookie file</returns>
        public static string GetCookies(string URL, string cookiesFileName)
        {
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string cookieFolderName = "cookies";
            string cookieFolderPath = Path.Combine(exeDirectory, cookieFolderName);
            string cookiesFileNameWithExtension = cookiesFileName + ".json";
            string cookieFilePath = Path.Combine(cookieFolderPath, cookiesFileNameWithExtension);
            Directory.CreateDirectory(cookieFolderPath);

            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl(URL);

            System.Threading.Thread.Sleep(300000);

            // Get all cookies
            var cookies = driver.Manage().Cookies.AllCookies;

            // Save to JSON file
            var cookieList = new List<Dictionary<string, object>>();
            foreach (var c in cookies)
            {
                cookieList.Add(new Dictionary<string, object>
                {
                    {"Name", c.Name},
                    {"Value", c.Value},
                    {"Domain", c.Domain},
                    {"Path", c.Path},
                    {"Expiry", c.Expiry},
                    {"Secure", c.Secure},
                    {"IsHttpOnly", c.IsHttpOnly}
                });
            }

            string json = JsonConvert.SerializeObject(cookieList, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(cookieFilePath, json);

            driver.Quit();
            return cookieFilePath;
        }

        /// <summary>
        /// Loads cookies from a JSON file and adds them to the specified website in the given WebDriver instance.
        /// After setting cookies, the page is reloaded to apply them.
        /// </summary>
        /// <param name="driver">The WebDriver instance used to navigate and set cookies.</param>
        /// <param name="URL">The URL of the website where cookies will be applied.</param>
        /// <param name="cookiesFilePath">Path to the JSON file containing cookie data.</param>
        public static void SetCookies(IWebDriver driver, string URL, string cookiesFilePath)
        {
            driver.Navigate().GoToUrl(URL);

            var json = File.ReadAllText(cookiesFilePath);
            var cookieList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

            foreach (var c in cookieList)
            {
                var cookie = new Cookie(
                    c["Name"].ToString(),
                    c["Value"].ToString(),
                    c["Domain"].ToString(),
                    c["Path"].ToString(),
                    c["Expiry"] != null ? (DateTime?)DateTime.Parse(c["Expiry"].ToString()) : null,
                    (bool)c["Secure"],
                    (bool)c["IsHttpOnly"],
                    null
                );

                try
                {
                    driver.Manage().Cookies.AddCookie(cookie);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при добавлении cookie {c["Name"]}: {ex.Message}");
                }
            }

            driver.Navigate().GoToUrl(URL);
        }
    }
}
