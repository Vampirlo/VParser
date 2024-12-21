using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;

namespace VParser.src
{
    internal class ImageMerger
    {
        public static void imageMergerWithOneHeight(string imagesPath)
        {
            if (!Directory.Exists(imagesPath))
            {
                Console.WriteLine("Указанная папка не существует.");
                return;
            }

            // Получаем список файлов PNG, отсортированных по числам
            string[] imageFiles = Directory.GetFiles(imagesPath, "*.png")
                .Where(f => int.TryParse(Path.GetFileNameWithoutExtension(f), out _)) // Оставляем только те файлы, у которых имя - число
                .OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f))) // Сортируем по числовому значению
                .ToArray();

            if (imageFiles.Length == 0)
            {
                Console.WriteLine("В папке нет изображений в формате PNG.");
                return;
            }

            try
            {
                // Загружаем первое изображение как базовое
                Image resultImage = Image.FromFile(imageFiles[0]);
                int width = resultImage.Width;
                int totalHeight = resultImage.Height;

                // Узнаём полную высоту результирующего изображения
                for (int i = 1; i < imageFiles.Length; i++)
                {
                    using (Image nextImage = Image.FromFile(imageFiles[i]))
                    {
                        if (nextImage.Width != width)
                        {
                            Console.WriteLine($"Ширина изображения {imageFiles[i]} не совпадает с шириной предыдущих. Пропускаем это изображение.");
                            continue;
                        }
                        totalHeight += nextImage.Height;
                    }
                }

                // Создаем результирующее изображение
                using (Bitmap combinedImage = new Bitmap(width, totalHeight))
                using (Graphics g = Graphics.FromImage(combinedImage))
                {
                    int currentHeight = 0;
                    foreach (string file in imageFiles)
                    {
                        using (Image img = Image.FromFile(file))
                        {
                            g.DrawImage(img, 0, currentHeight);
                            currentHeight += img.Height;
                        }
                    }

                    // Сохраняем итоговое изображение
                    string outputFilePath = Path.Combine(imagesPath, "combined_image.png");

                    if (File.Exists(outputFilePath)) File.Delete(outputFilePath);
                    combinedImage.Save(outputFilePath, ImageFormat.Png);
                    Console.WriteLine($"Изображение успешно сохранено в: {outputFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка: " + ex.Message);
            }
        }
    }
}
