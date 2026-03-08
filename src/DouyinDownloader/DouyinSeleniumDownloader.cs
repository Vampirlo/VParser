using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VParser.src.General;

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

namespace VParser.src.DouyinDownloader
{
    class DouyinSeleniumDownloader
    {
        /// <summary>
        /// Loads a list of Douyin video URLs, opens each one in a Chrome browser,
        /// extracts the direct video source URL from the <video> tag, and downloads the file.
        /// If the video source is unavailable, it retries several times and logs failed downloads.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task MultiplyDouyinVideoDownloadAsync(ChromeOptions options)
        {
            string[] videoLinks = GeneralTools.ReadVideoLinks("Douyin");

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
    }
}
