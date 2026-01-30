using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeleniumExtras.WaitHelpers;
using System.IO;
using System.Diagnostics;
using System.Collections;
using OpenQA.Selenium.Firefox;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Reflection.Metadata;
using OpenQA.Selenium.Interactions;
using Kameleo.LocalApiClient;
using Kameleo.LocalApiClient.Model;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.DevTools;
using Emulation = OpenQA.Selenium.DevTools.V143.Emulation;
using System.Text.Json.Nodes;

namespace VParser.src
{
    class StealthDriver
    {
        public static async Task<string> VStealthDriverXiaohongshuDownloaderHTML(string url, bool? mobileDriver = false, string? domainURLforSetCookie = null, string? cookiesFilePath = null)
        {
            var options = new ChromeOptions();

            // Launch flags
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " + "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            options.AddArgument("--lang=en-US");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            // WebRTC (IP leak)
            options.AddArgument("--force-webrtc-ip-handling-policy=disable_non_proxied_udp");
            options.AddArgument("--webrtc-ip-handling-policy=disable_non_proxied_udp");
            options.AddUserProfilePreference("profile.default_content_setting_values.geolocation", 1);

            options.AddArgument("--proxy-server=http=localhost:8080");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--allow-running-insecure-content");
            options.AddArgument("--ignore-ssl-errors=yes");
            options.AddArgument("--disable-quic");
            options.AddArgument("--allow-insecure-localhost");
            options.AddArgument(" --user-data-dir=D:\\prog\\profile");
            options.AcceptInsecureCertificates = true;

            IWebDriver driver = new ChromeDriver(options);

            var devTools = driver as IDevTools;
            var session = devTools.GetDevToolsSession();

            // Timezone
            await session.SendCommand(
                "Emulation.setTimezoneOverride",
                new JsonObject
                {
                    ["timezoneId"] = "Europe/Amsterdam"
                }
            );
            // Geolocation
            await session.SendCommand(
                "Emulation.setGeolocationOverride",
                new JsonObject
                {
                    ["latitude"] = 52.37,
                    ["longitude"] = 4.90,
                    ["accuracy"] = 20
                }
            );



            driver.Navigate().GoToUrl(url);

            Console.ReadLine();

            driver.Quit();

            return "";
        }
    }
}
