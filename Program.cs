using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Net;
using VParser.src;

/*
 Settings.ini

[SETTINGS]
XiaohongshuDownloadDirectory=
MinutesToWaitSiteLoading=30
[PROXY]
UseProxy=true
ProxySettings=login:password@ip:port
 */

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
            string yappyLinks = (Path.Combine(exeDirectory, "links yappy.txt"));
            int MinutesToWaitSiteLoading = 30;


            string DownloadDirectory = Path.Combine(exeDirectory, "Downloads"); //затем считывать из ini, если пусто - задавать автоматически

            VParser.src.tools.ConvertYappyLinksToRutubeCDN(yappyLinks);
            await VParser.src.tools.DownloadFilesFromUrls(yappyLinks, DownloadDirectory, useUniqueNames: true);
        }
    }
}