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

namespace VParser.src.ZCOOLDownloader
{
    class ZCOOLSeleniumDownloader
    {
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
            string[] videoLinks = GeneralTools.ReadVideoLinks("zcool");
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
                GeneralSeleniumTools.ScrollDown(driver);
                System.Threading.Thread.Sleep(time);
                GeneralSeleniumTools.ScrollUp(driver);

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
                    GeneralSeleniumTools.ScrollDown(driver);
                    System.Threading.Thread.Sleep(time);
                    GeneralSeleniumTools.ScrollUp(driver);

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
    }
}
