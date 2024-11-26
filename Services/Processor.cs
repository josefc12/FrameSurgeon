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
                Image image = Image.FromFile(inputPath);
                Size imageSize = image.Size;

                int cropSizeHorizontal = imageSize.Width / h;
                int cropSizeVertical = imageSize.Height / v;

                int step = 0;
                // Loop through rows and columns
                for (int row = 0; row < v; row++)
                {
                    for (int col = 0; col < h; col++)
                    {
                        // Area of the pixels to be extracted
                        Rectangle cropArea = new Rectangle(col * cropSizeHorizontal, row * cropSizeVertical, cropSizeHorizontal, cropSizeVertical);

                        // Create a new image/canvas to paste the pixels onto
                        Bitmap newImage = new Bitmap(cropSizeHorizontal, cropSizeVertical);
                        newImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                        using (Graphics g = Graphics.FromImage(newImage))
                        {
                            g.Clear(Color.Pink);
                            g.DrawImage(image, 0, 0, cropArea, GraphicsUnit.Pixel);
                        }


                        // Find whether there's a dot at the end of the output path with some kind of an extention
                        outputPath = RemoveExtension(outputPath);

                        Extension extE = ValueConverter.GetExtensionAsEnumValue(extension);
                        string extS = ValueConverter.GetDotExtension(extE);
                        newImage.Save(outputPath + step + extS);
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
