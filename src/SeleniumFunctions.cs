﻿using OpenQA.Selenium.Chrome;
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
        public static void WaitAndClickByXPath(IWebDriver driver, string xpath, int timeoutInSeconds = 30)
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

        public static List<string> GetAllJpgImageSrcsIncludingCustom(IWebDriver driver)
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
                Console.WriteLine("Ошибка при поиске изображений: " + ex.Message);
            }

            return jpgUrls;
        }

        public static IWebElement? WaitForElementOrNull(IWebDriver driver, string xpath, int timeoutInSeconds = 10)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv =>
                {
                    try
                    {
                        var element = drv.FindElement(By.XPath(xpath));
                        return element.Displayed ? element : null;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                });
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }
        }

        public static async Task VIPDownloader(string url, ChromeOptions options)
        {
        start:

            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            WaitAndClickByXPath(driver, "//div[contains(@class, 'uni-modal__btn') and contains(@class, 'uni-modal__btn_default') and normalize-space(text())='Cancel']");
            WaitAndClickByXPath(driver, "/html/body/uni-app/uni-page/uni-page-wrapper/uni-page-body/uni-view/uni-view[1]/uni-view/uni-button");
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            WaitAndClickByXPath(driver, "//div[contains(@class, 'uni-modal__btn') and contains(@class, 'uni-modal__btn_default') and normalize-space(text())='Cancel']");

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

            // Попробуем найти кнопку воспроизведения — если она есть, кликнем
            string playButtonXPath = "//*[@id=\"brannerViewId\"]/uni-swiper/div/div/div/uni-swiper-item[1]/uni-view[1]/uni-view[2]";

            if (ElementExists(driver, playButtonXPath))
            {
                Console.WriteLine("Есть кнопка ебаная");
                WaitAndClickByXPath(driver, playButtonXPath);
                WaitAndClickByXPath(driver, "/html/body/uni-app/uni-modal/div[2]/div[3]/div[2]");

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
                driver.Quit();
                goto start;
            }


            foreach (var singlejpgUrl in jpgUrls)
            {
                SaveTextToFile(singlejpgUrl);
            }

            driver.Close();
        }
        public static async Task GetImageFromXiaohongshuURLAsync(string url, int MinutesToWaitSiteLoading)
        {
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl(url);

            // Ожидание загрузки элемента
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromMinutes(MinutesToWaitSiteLoading));
            wait.Until(d => d.FindElement(By.ClassName("swiper-wrapper"))); // Ожидание загрузки элемента
            wait.Until(d => d.FindElement(By.ClassName("content-container")));

            // Получаем все изображения из элемента swiper-wrapper
            var swiperImages = driver.FindElements(By.CssSelector(".swiper-wrapper img.carousel-image"));
            var swiperSrcLinks = swiperImages.Select(img => img.GetAttribute("src")).ToList();

            // Получаем все изображения из элемента content-container
            var contentImages = driver.FindElements(By.CssSelector(".content-container img"));
            var contentSrcLinks = contentImages.Select(img => img.GetAttribute("src")).ToList();

            // Объединяем ссылки из обоих элементов
            var allSrcLinks = swiperSrcLinks.Concat(contentSrcLinks).ToList();

            // Путь для сохранения текстового файла
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "image_links.txt");

            // Сохраняем ссылки в текстовый файл
            await File.AppendAllLinesAsync(filePath, allSrcLinks);

            Console.WriteLine($"Ссылки на изображения сохранены в {filePath}"); // нужно будет прям тут и скачивать и дожидлаться скачки

            driver.Close();
        }

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
        static void ScrollDown(IWebDriver driver)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            while (true)
            {
                //var scrollHeightA = ((IJavaScriptExecutor)driver).ExecuteScript("return window.scrollY;");
                var scrollHeightA = (long)js.ExecuteScript("return window.scrollY;");

                js.ExecuteScript($"window.scrollBy(0, 100);");
                System.Threading.Thread.Sleep(10);

                //var scrollHeightB = ((IJavaScriptExecutor)driver).ExecuteScript("return window.scrollY;");
                var scrollHeightB = (long)js.ExecuteScript("return window.scrollY;");
                //var documentHeight = (long)js.ExecuteScript("return document.body.scrollHeight;");
                var documentHeight = Convert.ToInt64(js.ExecuteScript("return document.body.scrollHeight;"));

                if (scrollHeightA == scrollHeightB || scrollHeightB + 200 >= documentHeight)
                {
                    break;
                }
            }
        }

        static void ScrollUp(IWebDriver driver)
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
    }
}
