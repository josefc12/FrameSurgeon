using FrameSurgeon.Enums;
using FrameSurgeon.Models;
using FrameSurgeon.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using static SkiaSharp.SKImageFilter;
using FrameSurgeon.Classes;
using System.Diagnostics;
using SkiaSharp;
using Avalonia.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace FrameSurgeon.Services
{
    public class Processor
    {
        public async Task<ProcessResult> Process(ExportMode mode, MainWindowViewModel context)
        {
            //Validate parameters
            ProcessResult validatorResult = Validator.IsMakeAllowed(ref context);

            if (validatorResult.Result == Result.Failure)
            {
                return validatorResult;
            }

            // Specific process
            ProcessResult result = mode switch
            {
                ExportMode.Flipbook => MakeFlipbook(ref context),
                ExportMode.DismantleFlipbook => DismantleFlipbook(ref context),
                ExportMode.Convert => MakeFlipbook(ref context),
                ExportMode.AnimatedGif => MakeFlipbook(ref context),
                _ => new ProcessResult(Result.Failure, "Mode didn't have a set processor function!")
            };
            return await Task.FromResult(result);
        }

        private ProcessResult MakeFlipbook(ref MainWindowViewModel context)
        {
            // TODO creating the actual flipbook
            string firstImagePath = context.LoadedFiles[0];
            string outputPath = context.OutputPath;
            string extension = context.SelectedExtension;
            int h = context.FlipbookResolutionHorizontal;
            int v = context.FlipbookResolutionVertical;

            try
            {
                var firstImage = SKBitmap.Decode(firstImagePath);

                // Create a new image/canvas to paste the pixels onto
                var newImage = new SKBitmap(h * firstImage.Width, v * firstImage.Height);
                var canvas = new SKCanvas(newImage);
                canvas.Clear(SKColors.Black);

                int step = 0;
                // Loop through rows and columns
                for (int row = 0; row < v; row++)
                {
                    for (int col = 0; col < h; col++)
                    {
                        int x = col * firstImage.Width;
                        int y = row * firstImage.Height;
                        // Area of the pixels to be extracted
                        var cropArea = new SKRectI(x, y, x + firstImage.Width, y + firstImage.Height);


                        if (step <= context.LoadedFiles.Count -1)
                        {
                            SKBitmap image = SKBitmap.Decode(context.LoadedFiles[step]);
                            canvas.DrawBitmap(image,new SKRect(0, 0, firstImage.Width, firstImage.Height), cropArea);
                            image.Dispose();
                            
                        }
                        step++;

                    }
                }

                // Find whether there's a dot at the end of the output path with some kind of an extention
                outputPath = RemoveExtension(outputPath);

                Extension extE = ValueConverter.GetExtensionAsEnumValue(extension);
                SKEncodedImageFormat extV = ValueConverter.GetSkEncodedImageFormat(extE);
                string extS = ValueConverter.GetDotExtension(extE);
                using (var outputStream = System.IO.File.OpenWrite(outputPath + extS))
                {
                    newImage.Encode(outputStream, extV, 100);
                }
                newImage.Dispose();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }


            return new ProcessResult(Result.Success, "Flipbook created!");
        }

        private ProcessResult DismantleFlipbook(ref MainWindowViewModel context)
        {
            //OPTIMIZE - these make duplicates. Can the output and writing be generalized?
            string inputPath = context.LoadedFiles[0];
            string outputPath = context.OutputPath;
            string extension = context.SelectedExtension;
            int h = context.FlipbookResolutionHorizontal;
            int v = context.FlipbookResolutionVertical;

            // Check if path exists and is a valid image file
            try
            {
                var image = SKBitmap.Decode(inputPath);

                int cropSizeHorizontal = image.Width / h;
                int cropSizeVertical = image.Height / v;

                int step = 0;
                // Loop through rows and columns
                for (int row = 0; row < v; row++)
                {
                    for (int col = 0; col < h; col++)
                    {
                        int x = col * cropSizeHorizontal;
                        int y = row * cropSizeVertical;
                        // Area of the pixels to be extracted
                        var cropArea = new SKRectI(x, y, x + cropSizeHorizontal, y + cropSizeVertical);
                        
                        // Create a new image/canvas to paste the pixels onto
                        var newImage = new SKBitmap(cropSizeHorizontal, cropSizeVertical);

                        using (var canvas = new SKCanvas(newImage))
                        {
                            canvas.Clear(SKColors.Black);
                            canvas.DrawBitmap(image, cropArea, new SKRect(0, 0, cropSizeHorizontal, cropSizeVertical));
                        }
                        
                        // Find whether there's a dot at the end of the output path with some kind of an extention
                        outputPath = RemoveExtension(outputPath);

                        Extension extE = ValueConverter.GetExtensionAsEnumValue(extension);
                        SKEncodedImageFormat extV = ValueConverter.GetSkEncodedImageFormat(extE);
                        string extS = ValueConverter.GetDotExtension(extE);
                        using (var outputStream = System.IO.File.OpenWrite(outputPath + step + extS))
                        {
                            newImage.Encode(outputStream, extV, 100);
                        }
                        newImage.Dispose();
                        step++;
                    }
                }

                image.Dispose();

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            
            return new ProcessResult(Result.Success, "Dismantling finished!");
        }
        private string RemoveExtension(string filePath)
        {
            // Loop backward through the file path string
            for (int i = filePath.Length - 1; i >= 0; i--)
            {
                if (filePath[i] == '.')
                {
                    // Return the substring that excludes the extension
                    return filePath.Substring(0, i);
                }
            }

            // Return the original string if no dot is found (i.e., no extension)
            return filePath;
        }
    }
}
