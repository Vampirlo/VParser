using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMerger
{
    internal class imageMerger
    {
        static string[] GetSortedImageFilesByDate(string imagesPath)
        {
            string[] imageFiles = Directory.GetFiles(imagesPath, "*.*") // Ищем все файлы
                .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)) // Фильтруем только .png и .jpg
                .OrderBy(f => File.GetLastWriteTime(f)) // Сортируем по дате последнего изменения
                .ToArray();

            return imageFiles;
        }

        public static void ImageMergerWithOneHeight(string imagesPath)
        {
            if (!Directory.Exists(imagesPath))
            {
                Console.WriteLine("Указанная папка не существует.");
                return;
            }

            // Получаем список файлов PNG, отсортированных по дате последнего изменения
            string[] imageFiles = GetSortedImageFilesByDate(imagesPath);
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

        public static void SplitImageByWhiteLines(string imagePath, string outputDirectory)
        {
            using (Bitmap bitmap = new Bitmap(imagePath))
            {
                int lastSplitY = 0;
                bool isPreviousLineWhite = false;

                for (int y = 0; y < bitmap.Height; y++)
                {
                    bool isCurrentLineWhite = IsLineWhite(bitmap, y);

                    if (isCurrentLineWhite && !isPreviousLineWhite)
                    {
                        // Сохранить участок от lastSplitY до текущей линии
                        if (y > lastSplitY)
                        {
                            SaveSegment(bitmap, lastSplitY, y, outputDirectory);
                        }

                        lastSplitY = y;
                    }

                    isPreviousLineWhite = isCurrentLineWhite;
                }

                // Сохранить последний участок, если он есть
                if (lastSplitY < bitmap.Height)
                {
                    SaveSegment(bitmap, lastSplitY, bitmap.Height, outputDirectory);
                }
            }
        }

        static bool IsLineWhite(Bitmap bitmap, int y)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                if (pixel.R < 220 || pixel.G < 220 || pixel.B < 220) // Не полностью белый
                {
                    return false;
                }
            }
            return true;
        }

        static void SaveSegment(Bitmap bitmap, int startY, int endY, string outputDirectory)
        {
            int height = endY - startY;

            if (height <= 0)
                return;

            Rectangle section = new Rectangle(0, startY, bitmap.Width, height);
            using (Bitmap segment = bitmap.Clone(section, bitmap.PixelFormat))
            {
                string fileName = Path.Combine(outputDirectory, $"segment_{startY}_{endY}.png");
                segment.Save(fileName, ImageFormat.Png);
                Console.WriteLine($"Saved segment: {fileName}");
            }
        }

        public static void SplitAllImageFromPath(string mergeFullPath, string SplitedImagesPath)
        {
            string[] imageFiles = GetSortedImageFilesByDate(mergeFullPath);
            for (int i = 0; i < imageFiles.Length; i++)
            {
                string imgFullPathToSplit = Path.Combine(mergeFullPath, imageFiles[i]);
                SplitImageByWhiteLines(imgFullPathToSplit, SplitedImagesPath);
            }
        }

        public static void GetImagesWithoutWhiteLines(string imagesPath, string outputPath)
        {
            // Получаем список файлов изображений
            string[] imageFiles = GetSortedImageFilesByDate(imagesPath);

            foreach (string imageFile in imageFiles)
            {
                using (Bitmap bitmap = new Bitmap(imageFile))
                {
                    // Определяем верхнюю границу (где заканчиваются белые полосы сверху)
                    int top = 0;
                    while (top < bitmap.Height && IsLineWhite(bitmap, top))
                    {
                        top++;
                    }

                    // Определяем нижнюю границу (где заканчиваются белые полосы снизу)
                    int bottom = bitmap.Height - 1;
                    while (bottom > top && IsLineWhite(bitmap, bottom))
                    {
                        bottom--;
                    }

                    // Если нужно обрезать (т.е. есть белые полосы сверху или снизу)
                    if (top > 0 || bottom < bitmap.Height - 1)
                    {
                        int newHeight = bottom - top + 1;
                        using (Bitmap croppedBitmap = bitmap.Clone(new Rectangle(0, top, bitmap.Width, newHeight), bitmap.PixelFormat))
                        {
                            // Генерируем путь для сохранения обрезанного изображения в выходной папке
                            string outputFileName = Path.Combine(outputPath, Path.GetFileName(imageFile));

                            // Сохраняем обрезанное изображение
                            croppedBitmap.Save(outputFileName, ImageFormat.Png);

                            Console.WriteLine($"Изображение {imageFile} было обрезано и сохранено как {outputFileName}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Изображение {imageFile} не содержит белых полос и осталось без изменений.");
                    }
                }
            }
        }
    }
}
