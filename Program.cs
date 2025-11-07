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

namespace VParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string driverPath = Path.Combine(exeDirectory, "chromedriver.exe");
            string cookiesFileName = "xiaohongshu";
            string cookieFolderName = "cookies";
            string cookieFolderPath = Path.Combine(exeDirectory, cookieFolderName);
            string cookiesFileNameWithExtension = cookiesFileName + ".json";
            string cookieFilePath = Path.Combine(cookieFolderPath, cookiesFileNameWithExtension);
            Directory.CreateDirectory(cookieFolderPath);

            //VParser.src.SeleniumFunctions.GetCookies("https://www.xiaohongshu.com/", "xiaohongshu");


            //IWebDriver driver = new ChromeDriver();

            //VParser.src.SeleniumFunctions.SetCookies(driver, "https://www.xiaohongshu.com/", cookieFilePath);
            //await VParser.src.SeleniumFunctions.GetImageFromXiaohongshuURLAsync(driver, "http://xhslink.com/o/8sDMR7HKKej", 1);

            //driver.Close();

            string XiaHTMLPage = Path.Combine(exeDirectory, "page.html");

            var imageNames = VParser.src.tools.ExtractImageNames(XiaHTMLPage);
            var videoNames = VParser.src.tools.ExtractVideoNames(XiaHTMLPage);

            foreach (string imageName in imageNames)
            {
                Console.WriteLine(imageName);
            }
            foreach (string videoName in videoNames)
            {
                Console.WriteLine(videoName);
            }
        }
    }
}