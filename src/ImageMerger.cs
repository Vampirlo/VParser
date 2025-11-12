using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Program.cs example:
 using ImageMerger;

namespace VParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string mergePathName = "merge";
            string pathNameForMergedImages = "AlreadyMerged";
            string mergeFullPath = Path.Combine(exeDirectory, mergePathName); // мб её создавать ещё надо? 
            string fullPathOfAlreadyMergedImages = Path.Combine(mergeFullPath, pathNameForMergedImages);

            //директория для готовых разделённых изображений и 
            string randomFolderName = Path.GetRandomFileName();
            string SplitedImagesPath = Path.Combine(mergeFullPath, randomFolderName);
            Directory.CreateDirectory(SplitedImagesPath);

            randomFolderName = Path.GetRandomFileName();
            string ImageWithoutWhileLinesPath = Path.Combine(mergeFullPath, randomFolderName);
            Directory.CreateDirectory(ImageWithoutWhileLinesPath);

            // обычная вариация

            //imageMerger.ImageMergerWithOneHeight(imageMerger.GetSortedImageFilesByDate(mergeFullPath), fullPathOfAlreadyMergedImages);
            //imageMerger.MultiThreadedProcessImages(imageMerger.GetSortedImageFilesByDate(fullPathOfAlreadyMergedImages), mergeFullPath, SplitedImagesPath, imageMerger.SplitAllImageFromPath);
            //imageMerger.MultiThreadedProcessImages(imageMerger.GetSortedImageFilesByDate(SplitedImagesPath), ImageWithoutWhileLinesPath, imageMerger.GetImagesWithoutWhiteLines);
            //or
            imageMerger.MultiThreadedProcessImages(imageMerger.GetSortedImageFilesByDate(mergeFullPath), mergeFullPath, SplitedImagesPath, imageMerger.SplitAllImageFromPath);
            imageMerger.MultiThreadedProcessImages(imageMerger.GetSortedImageFilesByDate(SplitedImagesPath), ImageWithoutWhileLinesPath, imageMerger.GetImagesWithoutWhiteLines);
        }
    }
}
*/

namespace ImageMerger
{
    internal class imageMerger
    {
        public static string[] GetSortedImageFilesByDate(string imagesPath)
        {
            string[] imageFiles = Directory.GetFiles(imagesPath, "*.*") // Ищем все файлы
                .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)) // Фильтруем только .png и .jpg
                .OrderBy(f => File.GetLastWriteTime(f)) // Сортируем по дате последнего изменения
                .ToArray();

            return imageFiles;
        }

        public static void ImageMergerWithOneHeight(string[] imageFiles, string fullPathOfAlreadyMergedImages)
        {
            if (!Directory.Exists(fullPathOfAlreadyMergedImages))
            {
                Directory.CreateDirectory(fullPathOfAlreadyMergedImages);
            }

            // Получаем список файлов PNG, отсортированных по дате последнего изменения udp: кто эту хуйню тут написал вообще?
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
                    string randomFileName = Guid.NewGuid().ToString("N") + ".png";

                    string outputFilePath = Path.Combine(fullPathOfAlreadyMergedImages, randomFileName); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

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
                    bool isCurrentLineWhite = IsLineWhite(bitmap, y, false); // вот эту фолс или тру что здесь нахуй, надо как-то учитывать и брать из интерфейса и атвоматизировать не заранее написанный цвет 220, а самому вводить в поле

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

        static bool IsLineWhite(Bitmap bitmap, int y, bool approximately)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                if (approximately)
                {
                    if (pixel.R < 220 || pixel.G < 220 || pixel.B < 220) // Не полностью белый
                    {
                        return false;
                    }
                }
                else
                {
                    if (pixel.R < 250 || pixel.G < 250 || pixel.B < 250) // Не полностью белый
                    {
                        return false;
                    }
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
                string fileName = (Path.Combine(outputDirectory, $"_segment_{startY}_{endY}_{Guid.NewGuid().ToString("N")}.png"));
                try
                {
                    segment.Save(fileName, ImageFormat.Png);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении файла {fileName}: {ex.Message}");
                    throw;
                }
                Console.WriteLine($"Saved segment: {fileName}");
            }
        }

        public static void SplitAllImageFromPath(string[] imageFiles, string mergeFullPath, string SplitedImagesPath)
        {
            for (int i = 0; i < imageFiles.Length; i++)
            {
                //string imgFullPathToSplit = Path.Combine(mergeFullPath, imageFiles[i]);
                SplitImageByWhiteLines(imageFiles[i], SplitedImagesPath);
            }
        }

        public static void GetImagesWithoutWhiteLines(string[] imageFiles, string outputPath)
        {
            foreach (string imageFile in imageFiles)
            {
                using (Bitmap bitmap = new Bitmap(imageFile))
                {
                    // Определяем верхнюю границу (где заканчиваются белые полосы сверху)
                    int top = 0;
                    while (top < bitmap.Height && IsLineWhite(bitmap, top, true))
                    {
                        top++;
                    }

                    // Определяем нижнюю границу (где заканчиваются белые полосы снизу)
                    int bottom = bitmap.Height - 1;
                    while (bottom > top && IsLineWhite(bitmap, bottom, true))
                    {
                        bottom--;
                    }

                    // Если нужно обрезать (т.е. есть белые полосы сверху или снизу)
                    if (top > 0 || bottom < bitmap.Height - 1)
                    {
                        int newHeight = bottom - top + 1;

                        // Проверка на допустимость высоты
                        if (newHeight > 0)
                        {
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
                            Console.WriteLine($"Изображение {imageFile} имеет некорректную высоту после обрезки и будет пропущено.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Изображение {imageFile} не содержит белых полос и осталось без изменений.");
                    }
                }
            }
        }

        public static List<string[]> SplitArrayIntoChunks(string[] imageFiles, int numChunks)
        {
            var chunks = new List<string[]>();
            int chunkSize = imageFiles.Length / numChunks; // Размер чанка
            int remainder = imageFiles.Length % numChunks; // Остаток

            int startIndex = 0;
            for (int i = 0; i < numChunks; i++)
            {
                // Если текущий чанк должен получить остаток (лишние элементы)
                int currentChunkSize = chunkSize + (i < remainder ? 1 : 0);
                var chunk = new string[currentChunkSize];
                Array.Copy(imageFiles, startIndex, chunk, 0, currentChunkSize);
                chunks.Add(chunk);
                startIndex += currentChunkSize;
            }

            return chunks;
        }
        public static void MultiThreadedProcessImages(string[] imageFiles, string mergeFullPath, string splitedImagesPath, Action<string[], string, string> processingMethod)
        {
            int maxThreads = Environment.ProcessorCount;  // Получаем максимальное количество потоков
            var chunks = SplitArrayIntoChunks(imageFiles, maxThreads);  // Разбиваем массив на части

            var tasks = new List<Task>();

            foreach (var chunk in chunks)
            {
                // Создаем задачу для обработки текущего чанка изображений
                var task = Task.Run(() =>
                {
                    // обработка
                    processingMethod(chunk, mergeFullPath, splitedImagesPath);
                });

                tasks.Add(task);
            }

            // Ждем завершения всех задач
            Task.WhenAll(tasks).Wait();
        }

        public static void MultiThreadedProcessImages(string[] imageFiles, string outputPath, Action<string[], string> processingMethod)
        {
            int maxThreads = Environment.ProcessorCount;
            var chunks = SplitArrayIntoChunks(imageFiles, maxThreads);

            var tasks = new List<Task>();

            foreach (var chunk in chunks)
            {
                var task = Task.Run(() =>
                {
                    processingMethod(chunk, outputPath);
                });
                tasks.Add(task);
            }

            Task.WhenAll(tasks).Wait();
        }
    }
}
