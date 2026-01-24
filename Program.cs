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

                    string hardcore = "C:\\vs_proj\\VParser\\bin\\Debug\\net8.0\\XiaohongshuDownloaderAllHTMLPages\\7q9iM530MQC.html";

                    List<string> imageNames = tools.XiaohongshuExtractImageNames(htmlFile);
                    List<string> videoNames = tools.XiaohongshuExtractVideoNames(htmlFile);

                    List<string> AllFilesNames = imageNames.Concat(videoNames).ToList();

                    List<string> AllFilesURL = tools.XiaohongshuGetURLToAllFiles(AllFilesNames);

                    foreach (var item in AllFilesURL)
                    {
                        Console.WriteLine(item);
                    }

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