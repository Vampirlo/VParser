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

            VParser.src.tools.CleanUrlsInFile(srcUrlsFile);
            VParser.src.tools.RemoveDuplicateLines(srcUrlsFile);

            await VParser.src.tools.DownloadFilesFromUrls(srcUrlsFile, downloadFolder);
        }
    }
}