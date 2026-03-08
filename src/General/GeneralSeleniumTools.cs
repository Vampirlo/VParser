using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VParser.src.General
{
    class GeneralSeleniumTools
    {
        /// <summary>
        /// Loads cookies from a JSON file and adds them to the specified website in the given WebDriver instance.
        /// After setting cookies, the page is reloaded to apply them.
        /// </summary>
        /// <param name="driver">The WebDriver instance used to navigate and set cookies.</param>
        /// <param name="URL">The URL of the website where cookies will be applied.</param>
        /// <param name="cookiesFilePath">Path to the JSON file containing cookie data.</param>
        public static void SetCookies(IWebDriver driver, string URL, string cookiesFilePath)
        {
            driver.Navigate().GoToUrl(URL);

            var json = File.ReadAllText(cookiesFilePath);
            var cookieList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

            foreach (var c in cookieList)
            {
                var cookie = new Cookie(
                    c["Name"].ToString(),
                    c["Value"].ToString(),
                    c["Domain"].ToString(),
                    c["Path"].ToString(),
                    c["Expiry"] != null ? (DateTime?)DateTime.Parse(c["Expiry"].ToString()) : null,
                    (bool)c["Secure"],
                    (bool)c["IsHttpOnly"],
                    null
                );

                try
                {
                    driver.Manage().Cookies.AddCookie(cookie);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при добавлении cookie {c["Name"]}: {ex.Message}");
                }
            }

            driver.Navigate().GoToUrl(URL);
        }

        /// <summary>
        /// The cookie name is passed without the json extension.
        /// The file will be saved at exe/cookies/cookiesFileName.json
        /// </summary>
        /// <param name="URL">example https://www.xiaohongshu.com/</param>
        /// <param name="cookiesFileName">example xiaohongshu</param>
        /// <returns>Path to cookie file</returns>
        public static string GetCookies(string URL, string cookiesFileName)
        {
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string cookieFolderName = "cookies";
            string cookieFolderPath = Path.Combine(exeDirectory, cookieFolderName);
            string cookiesFileNameWithExtension = cookiesFileName + ".json";
            string cookieFilePath = Path.Combine(cookieFolderPath, cookiesFileNameWithExtension);
            Directory.CreateDirectory(cookieFolderPath);

            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl(URL);

            System.Threading.Thread.Sleep(300000);

            // Get all cookies
            var cookies = driver.Manage().Cookies.AllCookies;

            // Save to JSON file
            var cookieList = new List<Dictionary<string, object>>();
            foreach (var c in cookies)
            {
                cookieList.Add(new Dictionary<string, object>
                {
                    {"Name", c.Name},
                    {"Value", c.Value},
                    {"Domain", c.Domain},
                    {"Path", c.Path},
                    {"Expiry", c.Expiry},
                    {"Secure", c.Secure},
                    {"IsHttpOnly", c.IsHttpOnly}
                });
            }

            string json = JsonConvert.SerializeObject(cookieList, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(cookieFilePath, json);

            driver.Quit();
            return cookieFilePath;
        }

        /// <summary>
        /// Waiting for the xpath element
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="xpath"></param>
        /// <param name="timeoutInSeconds">seconds to wait</param>
        /// <param name="click">element.Click() to appear</param>
        public static void WaitByXPath(IWebDriver driver, string xpath, int timeoutInSeconds = 30, bool click = false)
        {
            IWebElement element = null;
            var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout)
            {
                try
                {
                    element = driver.FindElement(By.XPath(xpath));

                    if (element.Displayed && element.Enabled)
                    {
                        if (click)
                            element.Click();
                        return;
                    }
                }
                catch (NoSuchElementException)
                {
                    // Элемент ещё не появился — продолжаем ждать
                }
                catch (StaleElementReferenceException)
                {
                    // DOM обновился — ждём следующую попытку
                }

                Thread.Sleep(500);
            }

            Console.WriteLine($"[WARNING] Элемент с XPath '{xpath}' не был найден или не стал кликабельным за {timeoutInSeconds} секунд.");
        }

        /// <summary>
        /// Idle verification of the existence of an xpath object
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static bool ElementExists(IWebDriver driver, string xpath)
        {
            try
            {
                var element = driver.FindElement(By.XPath(xpath));
                return element.Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        /// <summary>
        /// Get all jpg URL from page
        /// можно модернизировать и для всех типов изображений
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        public static List<string> GetAllJpgImageSrcsIncludingCustom(IWebDriver driver)
        {
            var jpgUrls = new List<string>();

            try
            {
                // Найдем все элементы с атрибутом src или data-src
                var elementsWithSrc = driver.FindElements(By.XPath("//*[@src or @data-src]"));

                foreach (var el in elementsWithSrc)
                {
                    string? src = el.GetAttribute("src");
                    string? dataSrc = el.GetAttribute("data-src");

                    // Проверяем оба варианта и добавляем, если jpg
                    if (!string.IsNullOrEmpty(src) && src.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                        jpgUrls.Add(src);
                    else if (!string.IsNullOrEmpty(dataSrc) && dataSrc.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                        jpgUrls.Add(dataSrc);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when searching for images: " + ex.Message);
            }

            return jpgUrls;
        }

        public static void ScrollDown(IWebDriver driver)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            while (true)
            {
                var scrollHeightA = (long)js.ExecuteScript("return window.scrollY;");

                js.ExecuteScript($"window.scrollBy(0, 100);");
                System.Threading.Thread.Sleep(10);

                var scrollHeightB = (long)js.ExecuteScript("return window.scrollY;");
                var documentHeight = Convert.ToInt64(js.ExecuteScript("return document.body.scrollHeight;"));

                if (scrollHeightA == scrollHeightB || scrollHeightB + 200 >= documentHeight)
                {
                    break;
                }
            }
        }

        public static void ScrollUp(IWebDriver driver)
        {

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            while (true)
            {
                var scrollHeightA = (long)js.ExecuteScript("return window.scrollY;");

                js.ExecuteScript($"window.scrollBy(0, -200);");
                System.Threading.Thread.Sleep(10);

                var scrollHeightB = (long)js.ExecuteScript("return window.scrollY;");
                var documentHeight = Convert.ToInt64(js.ExecuteScript("return document.body.scrollHeight;"));

                if (scrollHeightA == scrollHeightB || scrollHeightB + 200 >= documentHeight)
                {
                    break;
                }
            }
        }
    }
}
