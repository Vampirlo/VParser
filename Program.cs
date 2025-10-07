using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Net;
using VParser.src;

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
            string mergeFullPath = Path.Combine(exeDirectory, mergePathName);
            string fullPathOfAlreadyMergedImages = Path.Combine(mergeFullPath, pathNameForMergedImages);

            //директория для готовых разделённых изображений и 
            string randomFolderName = Path.GetRandomFileName();
            string SplitedImagesPath = Path.Combine(mergeFullPath, randomFolderName);
            Directory.CreateDirectory(SplitedImagesPath);

            randomFolderName = Path.GetRandomFileName();
            string ImageWithoutWhileLinesPath = Path.Combine(mergeFullPath, randomFolderName);
            Directory.CreateDirectory(ImageWithoutWhileLinesPath);

            // обычная вариация

            imageMerger.ImageMergerWithOneHeight(imageMerger.GetSortedImageFilesByDate(mergeFullPath), fullPathOfAlreadyMergedImages);
            imageMerger.MultiThreadedProcessImages(imageMerger.GetSortedImageFilesByDate(fullPathOfAlreadyMergedImages), mergeFullPath, SplitedImagesPath, imageMerger.SplitAllImageFromPath);
            imageMerger.MultiThreadedProcessImages(imageMerger.GetSortedImageFilesByDate(SplitedImagesPath), ImageWithoutWhileLinesPath, imageMerger.GetImagesWithoutWhiteLines);
            //or
            //imageMerger.MultiThreadedProcessImages(imageMerger.GetSortedImageFilesByDate(mergeFullPath), mergeFullPath, SplitedImagesPath, imageMerger.SplitAllImageFromPath);
            //imageMerger.MultiThreadedProcessImages(imageMerger.GetSortedImageFilesByDate(SplitedImagesPath), ImageWithoutWhileLinesPath, imageMerger.GetImagesWithoutWhiteLines);
        }
    }
}