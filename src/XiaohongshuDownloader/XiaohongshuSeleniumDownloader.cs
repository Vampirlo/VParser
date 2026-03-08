using Kameleo.LocalApiClient.Model;
using Kameleo.LocalApiClient;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VParser.src.General;

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

                    string hardcore = "C:\\vs_proj\\VParser\\bin\\Debug\\net8.0\\XiaohongshuDownloaderAllHTMLPages\\6SkPbyOU9U6.html";

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

namespace VParser.src.xiaohongshuDownloader
{
    class XiaohongshuSeleniumDownloader
    {
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
            options.AddArguments("--allow-running-insecure-content");
            options.AddArguments("--disable-web-security");
            options.AddArguments("--ignore-certificate-errors");
            if (mobileDriver == true)
            {
                options.EnableMobileEmulation("iPhone X");
            }

            IWebDriver driver = new ChromeDriver(options);

            if (!string.IsNullOrEmpty(domainURLforSetCookie) && !string.IsNullOrEmpty(cookiesFilePath))
            {
                GeneralSeleniumTools.SetCookies(driver, domainURLforSetCookie, cookiesFilePath);
            }

            string htmlSitesFolderName = "XiaohongshuDownloaderAllHTMLPages";
            string htmlSitesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, htmlSitesFolderName);
            Directory.CreateDirectory(htmlSitesFolderPath);

            string htmlFileName = XiaohongshuTools.ExtractNameFromUrl(url) + ".html";
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
                elapsed += 10;
            }
            driver.Quit();
            Console.WriteLine("HTML is Empty. Xiaohongshu most likely ended the session. Reauthorization is required.");
            Environment.Exit(0);
            return HTMLFilePath; // ну это просто смешно
        }


        public static async Task<string> KameleoXiaohongshuDownloaderHTML(string url, bool? mobileDriver = false, string? domainURLforSetCookie = null, string? cookiesFilePath = null)
        {
            string htmlSitesFolderName = "XiaohongshuDownloaderAllHTMLPages";
            string htmlSitesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, htmlSitesFolderName);
            Directory.CreateDirectory(htmlSitesFolderPath);
            string htmlFileName = XiaohongshuTools.ExtractNameFromUrl(url) + ".html";
            string HTMLFilePath = Path.Combine(htmlSitesFolderPath, htmlFileName);


            // start client
            var client = new KameleoLocalApiClient(new Uri("http://localhost:5050"));

            // fingerprint 
            var fingerprints = await client.Fingerprint.SearchFingerprintsAsync("mobile", "ios", "safari");
            var fingerprint = fingerprints[new Random().Next(fingerprints.Count)];

            // create profile
            var profile = await client.Profile.CreateProfileAsync(
            new CreateProfileRequest(fingerprint.Id)
            {
                Name = $"xhs-{Guid.NewGuid()}"
            }
            );

            // start profile
            await client.Profile.StartProfileAsync(
            profile.Id,
            new BrowserSettings(
                arguments: new List<string>
                {
                    "mute-audio"
                },
                additionalOptions: new List<Preference>
                {
                    new Preference("pageLoadStrategy", "eager")
                }
            )
            );

            // connect to selenium
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddAdditionalOption("kameleo:profileId", profile.Id.ToString());
            chromeOptions.AddArguments("--allow-running-insecure-content");
            chromeOptions.AddArguments("--disable-web-security");
            chromeOptions.AddArguments("--ignore-certificate-errors");

            var driver = new RemoteWebDriver(
            new Uri("http://localhost:5050/webdriver"),
            chromeOptions
            );

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);

            try
            {
                driver.Navigate().GoToUrl(url);

                //await Task.Delay(1500); // дать JS отработать
                await Task.Delay(60000);

                var html = driver.PageSource;
                File.WriteAllText(HTMLFilePath, html);
            }
            catch
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
                await client.Profile.StopProfileAsync(profile.Id);
                await client.Profile.DeleteProfileAsync(profile.Id);
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
                    await client.Profile.StopProfileAsync(profile.Id);
                    await client.Profile.DeleteProfileAsync(profile.Id);
                    return HTMLFilePath;
                }
                await Task.Delay(10);
                elapsed += 10;
            }
            driver.Quit();
            await client.Profile.StopProfileAsync(profile.Id);
            await client.Profile.DeleteProfileAsync(profile.Id);
            Console.WriteLine("HTML is Empty. Xiaohongshu most likely ended the session. Reauthorization is required.");
            Environment.Exit(0);
            return HTMLFilePath; // ну это просто смешно
        }

    }
}
