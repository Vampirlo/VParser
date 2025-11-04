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

namespace VParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //string cookiesFilePath = VParser.src.SeleniumFunctions.GetCookies("https://sync.beatoven.ai/home", "beatoven");

            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string driverPath = Path.Combine(exeDirectory, "chromedriver.exe");
            string cookiesFileName = "beatoven";
            string cookieFolderName = "cookies";
            string cookieFolderPath = Path.Combine(exeDirectory, cookieFolderName);
            string cookiesFileNameWithExtension = cookiesFileName + ".json";
            string cookieFilePath = Path.Combine(cookieFolderPath, cookiesFileNameWithExtension);
            Directory.CreateDirectory(cookieFolderPath);

            VParser.src.SeleniumFunctions.GetCookies("https://www.xiaohongshu.com/", "xiaohongshu");
        }
    }
}