using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VParser.src
{
    internal class tools
    {
        public static void iniFileCreate(string iniFilePath)
        {
            using (File.Create(iniFilePath));
            INIManager manager = new INIManager(iniFilePath);
            manager.WritePrivateString("SETTINGS", "XiaohongshuDownloadDirectory", "");
            manager.WritePrivateString("SETTINGS", "MinutesToWaitSiteLoading", "30");

            Console.WriteLine("ini file was not found. File has been created.");
        }
        public static string[] ReadVideoLinks(string siteName)
        {
            string fileName;

            if (siteName == "Douyin")
                fileName = "Douyin.txt";
            else if (siteName == "zcool")
                fileName = "zcool.txt";
            else
                return new string[0];

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            
            // Если файл существует, возвращаем массив строк
            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath);
            }

            // Если файла нет, возвращаем пустой массив
            return new string[0];
        }
    }
}
