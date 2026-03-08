using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VParser.src.General;

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

namespace VParser.src.VIPDownloader
{
    class VIPSeleniumDownloader
    {
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

            GeneralSeleniumTools.WaitByXPath(driver, "//div[contains(@class, 'uni-modal__btn') and contains(@class, 'uni-modal__btn_default') and normalize-space(text())='Cancel']", click: true);
            GeneralSeleniumTools.WaitByXPath(driver, "/html/body/uni-app/uni-page/uni-page-wrapper/uni-page-body/uni-view/uni-view[1]/uni-view/uni-button", click: true);
            GeneralSeleniumTools.WaitByXPath(driver, "//div[contains(@class, 'uni-modal__btn') and contains(@class, 'uni-modal__btn_default') and normalize-space(text())='Cancel']", click: true);

            // Попробуем найти кнопку воспроизведения — если она есть, кликнем
            string playButtonXPath = "//*[@id=\"brannerViewId\"]/uni-swiper/div/div/div/uni-swiper-item[1]/uni-view[1]/uni-view[2]";

            if (GeneralSeleniumTools.ElementExists(driver, playButtonXPath))
            {
                Console.WriteLine("Есть кнопка воспроизведения");
                GeneralSeleniumTools.WaitByXPath(driver, playButtonXPath, click: true);
                GeneralSeleniumTools.WaitByXPath(driver, "/html/body/uni-app/uni-modal/div[2]/div[3]/div[2]", click: true);

                IWebElement videoElement = driver.FindElement(By.XPath("//*[@id=\"myVideo\"]/div/video"));
                string videoSrc = videoElement.GetAttribute("src");

                GeneralTools.SaveTextToFile(videoSrc);
            }
            else
            {
                Console.WriteLine("Кнопка воспроизведения не найдена, продолжаем без неё.");
            }

            List<string> jpgUrls = GeneralSeleniumTools.GetAllJpgImageSrcsIncludingCustom(driver);

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
                GeneralTools.SaveTextToFile(singlejpgUrl);
            }

            driver.Close();
        }
    }
}
