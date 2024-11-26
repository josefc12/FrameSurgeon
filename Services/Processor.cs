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

namespace FrameSurgeon.Services
{
    public class Processor
    {
        public async Task<ProcessResult> Process(ExportMode mode, MainWindowViewModel context)
        {
            // General pre-process

            // Specific process
            ProcessResult result = mode switch
            {
                ExportMode.Flipbook => MakeFlipbook(),
                ExportMode.DismantleFlipbook => DismantleFlipbook(context.LoadedFiles[0] , context.OutputPath, context.SelectedExtension, context.FlipbookResolutionHorizontal, context.FlipbookResolutionVertical),
                ExportMode.IndividualFrames => MakeFlipbook(),
                ExportMode.AnimatedGif => MakeFlipbook(),
                _ => new ProcessResult(Result.Failure, "Mode didn't have a set processor function!")
            };
            return await Task.FromResult(result);
            //General after-process
        }

        private ProcessResult MakeFlipbook()
        {
            return new ProcessResult(Result.Success);
        }

        private ProcessResult DismantleFlipbook(string inputPath, string outputPath, string extension , int h, int v)
        {
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
                            canvas.Clear(SKColors.Pink);
                            canvas.DrawBitmap(image, cropArea, new SKRect(0, 0, cropSizeHorizontal, cropSizeVertical));
                        }
                        
                        // Find whether there's a dot at the end of the output path with some kind of an extention
                        outputPath = RemoveExtension(outputPath);

                        Extension extE = ValueConverter.GetExtensionAsEnumValue(extension);
                        SKEncodedImageFormat extS = ValueConverter.GetSkEncodedImageFormat(extE);
                        using (var outputStream = System.IO.File.OpenWrite(outputPath + step))
                        {
                            newImage.Encode(outputStream, extS, 100);
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
            


            return new ProcessResult(Result.Success);
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
