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


            /*метод, в который передаётся файл с ссылками. 
           1. Необходимо извлечь ссылки из файла в какой-либо массив и вызывать метод get image links from url
            и записывать их в какой-либо файл. Далее удалить повторяющиеся, если они будут
           2. Сделать метод get image links from url 
           3. Сделать метод MultiplyDownloadFileAsync
            
            //string imageUrl = "https://qimg.xiaohongshu.com/material_space/820e2648-b9d6-44d1-8235-ce967d3d6638";
            //await VParser.src.FileDownloader.DownloadFileAsync(imageUrl, downloadDirectory);



            //string url = @"https://www.xiaohongshu.com/goods-detail/66c169acad99590001076262?instation_link=xhsdiscover%3A%2F%2Fgoods_detail%2F66c169acad99590001076262%3Ftrade_ext%3DeyJjaGFubmVsSW5mbyI6bnVsbCwiZHNUb2tlbkluZm8iOm51bGwsInNoYXJlTGluayI6Imh0dHBzOi8vd3d3LnhpYW9ob25nc2h1LmNvbS9nb29kcy1kZXRhaWwvNjZjMTY5YWNhZDk5NTkwMDAxMDc2MjYyP2FwcHVpZD02NjdjMTgzZTAwMDAwMDAwMDcwMDQ2ZDIiLCJsaXZlSW5mbyI6bnVsbCwic2hvcEluZm8iOm51bGwsImdvb2RzTm90ZUluZm8iOm51bGwsImNoYXRJbmZvIjpudWxsLCJzZWFyY2hJbmZvIjpudWxsfQ%3D%3D%26rn%3Dtrue&xhsshare=CopyLink&appuid=667c183e00000000070046d2&apptime=1726469973";
            //await VParser.src.SeleniumFunctions.GetImageFromXiaohongshuURLAsync(url, MinutesToWaitSiteLoading);
            */

            //await VParser.src.SeleniumFunctions.MultiplyDouyinVideoDownloadAsync(options);
            await VParser.src.SeleniumFunctions.MultiplyZcoolDownloadAsync(options);
        }
    }
}
