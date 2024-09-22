using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VParser.src
{
    public class FileDownloader
    {
        // Асинхронная функция для загрузки файла
        public static async Task DownloadFileAsyncJPG(string fileUrl, string downloadDirectory = null)
        {
            try
            {
                // Получаем директорию исполняемого файла, если не указана папка загрузки
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Если не передан путь к папке загрузки, используем папку "Downloads" в директории исполняемого файла
                if (string.IsNullOrEmpty(downloadDirectory))
                {
                    downloadDirectory = Path.Combine(exeDirectory, "Downloads");

                    // Если папка не существует, создаем её
                    if (!Directory.Exists(downloadDirectory))
                    {
                        Directory.CreateDirectory(downloadDirectory);
                        Console.WriteLine($"Папка {downloadDirectory} создана.");
                    }
                }

                // Имя файла из URL
                string fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);

                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                {
                    fileName += ".jpg"; // Замените на нужное расширение по умолчанию
                }

                // Полный путь к сохраненному файлу
                string filePath = Path.Combine(downloadDirectory, fileName);

                // Используем HttpClient для загрузки файла
                using (HttpClient client = new HttpClient())
                {
                    Console.WriteLine($"Начало загрузки файла с URL: {fileUrl}");
                    byte[] fileBytes = await client.GetByteArrayAsync(fileUrl);
                    await File.WriteAllBytesAsync(filePath, fileBytes);
                    Console.WriteLine($"Файл загружен и сохранен по пути: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
            }
        }

        public static async Task DownloadFileAsyncZcool(string fileUrl, string downloadDirectory = null)
        {
            try
            {
                // Получаем директорию исполняемого файла, если не указана папка загрузки
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Если не передан путь к папке загрузки, используем папку "Downloads" в директории исполняемого файла
                if (string.IsNullOrEmpty(downloadDirectory))
                {
                    downloadDirectory = Path.Combine(exeDirectory, "Downloads");

                    // Если папка не существует, создаем её
                    if (!Directory.Exists(downloadDirectory))
                    {
                        Directory.CreateDirectory(downloadDirectory);
                        Console.WriteLine($"Папка {downloadDirectory} создана.");
                    }
                }

                //string fileName = Guid.NewGuid().ToString();
                string fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);

                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                {
                    fileName += ".jpg"; // Замените на нужное расширение по умолчанию
                }

                // Полный путь к сохраненному файлу
                string filePath = Path.Combine(downloadDirectory, fileName);

                // Используем HttpClient для загрузки файла
                using (HttpClient client = new HttpClient())
                {
                    Console.WriteLine($"Начало загрузки файла с URL: {fileUrl}");
                    byte[] fileBytes = await client.GetByteArrayAsync(fileUrl);
                    await File.WriteAllBytesAsync(filePath, fileBytes);
                    Console.WriteLine($"Файл загружен и сохранен по пути: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
            }
        }

        public static async Task DownloadFileAsyncMP4(string fileUrl, string downloadDirectory = null)
        {
            try
            {
                // Получаем директорию исполняемого файла, если не указана папка загрузки
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Если не передан путь к папке загрузки, используем папку "Downloads" в директории исполняемого файла
                if (string.IsNullOrEmpty(downloadDirectory))
                {
                    downloadDirectory = Path.Combine(exeDirectory, "Downloads");

                    // Если папка не существует, создаем её
                    if (!Directory.Exists(downloadDirectory))
                    {
                        Directory.CreateDirectory(downloadDirectory);
                        Console.WriteLine($"Папка {downloadDirectory} создана.");
                    }
                }

                // Генерируем уникальное имя файла
                string fileExtension = ".mp4"; // Замените на нужное расширение
                string randomFileName = Guid.NewGuid().ToString() + fileExtension;

                // Полный путь к сохраненному файлу
                string filePath = Path.Combine(downloadDirectory, randomFileName);

                // Используем HttpClient для загрузки файла
                using (HttpClient client = new HttpClient())
                {
                    Console.WriteLine($"Начало загрузки файла с URL: {fileUrl}");
                    byte[] fileBytes = await client.GetByteArrayAsync(fileUrl);
                    await File.WriteAllBytesAsync(filePath, fileBytes);
                    Console.WriteLine($"Файл загружен и сохранен по пути: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
            }
        }
    }
}
