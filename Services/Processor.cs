using FrameSurgeon.Enums;
using FrameSurgeon.Models;
using FrameSurgeon.ViewModels;
using System;
using System.Threading.Tasks;
using FrameSurgeon.Classes;
using ImageMagick;

namespace FrameSurgeon.Services
{
    public class Processor
    {
        public async Task<ProcessResult> Process(GlobalSettings globalSettings, FlipbookSettings flipbookSettings)
        {
            //Validate parameters
            ProcessResult validatorResult = Validator.IsMakeAllowed(globalSettings, flipbookSettings);

            if (validatorResult.Result == Result.Failure)
            {
                return validatorResult;
            }

            // Specific process
            ProcessResult result = globalSettings.ExportMode switch
            {
                ExportMode.Flipbook => MakeFlipbook(globalSettings, flipbookSettings),
                ExportMode.DismantleFlipbook => DismantleFlipbook(globalSettings, flipbookSettings),
                ExportMode.Convert => MakeFlipbook(globalSettings, flipbookSettings),
                ExportMode.AnimatedGif => MakeFlipbook(globalSettings, flipbookSettings),
                _ => new ProcessResult(Result.Failure, "Mode didn't have a set processor function!")
            };
            return await Task.FromResult(result);
        }

        private ProcessResult MakeFlipbook(GlobalSettings globalSettings, FlipbookSettings flipbookSettings)
        {

            try
            {
                //New image
                MagickImage firstImage = new MagickImage(globalSettings.LoadedFiles[0]);

                //Re-scale if applicable
                ResizeFrame(firstImage, (uint)globalSettings.FrameSizeWidth, (uint)globalSettings.FrameSizeHeight);

                //Image size
                uint newWidth = (uint)(flipbookSettings.hResolution * firstImage.Width);
                uint newHeight = (uint)(flipbookSettings.vResolution * firstImage.Height);

                MagickColor color = globalSettings.TransparencyEnabled == true ? MagickColors.Transparent : MagickColors.Black;

                //Create canvas
                MagickImage canvas = new MagickImage(color, newWidth, newHeight);

                int step = 0;
                // Loop through rows and columns
                for (int row = 0; row < flipbookSettings.vResolution; row++)
                {
                    for (int col = 0; col < flipbookSettings.hResolution; col++)
                    {
                        int x = col * (int)firstImage.Width;
                        int y = row * (int)firstImage.Height;

                        if (step <= globalSettings.LoadedFiles.Count -1)
                        {
                            // Area of the pixels to be extracted
                            MagickImage image = new MagickImage(globalSettings.LoadedFiles[step]);

                            //Re-scale if applicable
                            ResizeFrame(image, (uint)globalSettings.FrameSizeWidth, (uint)globalSettings.FrameSizeHeight);

                            canvas.Composite(image, x, y, CompositeOperator.Over);
                            image.Dispose();
                            
                        }
                        step++;

                    }
                }

                // Find whether there's a dot at the end of the output path with some kind of an extention
                InputOutput.OutputImage(globalSettings.SelectedExtension, globalSettings.OutputPath, canvas);

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            

            return new ProcessResult(Result.Success, "Flipbook created!");
        }

        private ProcessResult DismantleFlipbook(GlobalSettings globalSettings, FlipbookSettings flipbookSettings)
        {

            // Check if path exists and is a valid image file
            try
            {
                //First image in the loaded images
                MagickImage image = new MagickImage(globalSettings.LoadedFiles[0]);

                int cropSizeHorizontal = (int)image.Width / flipbookSettings.hResolution;
                int cropSizeVertical = (int)image.Height / flipbookSettings.vResolution;

                int step = 0;
                // Loop through rows and columns
                for (int row = 0; row < flipbookSettings.vResolution; row++)
                {
                    for (int col = 0; col < flipbookSettings.hResolution; col++)
                    {
                        int x = col * cropSizeHorizontal;
                        int y = row * cropSizeVertical;
                        // Area of the pixels to be extracted
                        var cropArea = new MagickGeometry(x, y, (uint)cropSizeHorizontal, (uint)cropSizeVertical);

                        MagickImage croppedImage = (MagickImage)image.Clone();
                        croppedImage.Crop(cropArea);

                        //Re-scale if applicable
                        ResizeFrame(croppedImage, (uint)globalSettings.FrameSizeWidth, (uint)globalSettings.FrameSizeHeight);

                        // Create a new image/canvas to paste the pixels onto
                        MagickColor color = globalSettings.TransparencyEnabled == true ? MagickColors.Transparent : MagickColors.Black;

                        //Create canvas
                        MagickImage canvas = new MagickImage(color, croppedImage.Width, croppedImage.Height);


                        canvas.Composite(croppedImage, 0, 0, CompositeOperator.Over);

                        InputOutput.OutputImage(globalSettings.SelectedExtension, globalSettings.OutputPath, canvas, step);
                        canvas.Dispose();
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

        private void ResizeFrame(MagickImage image, uint width, uint height)
        {
            if ((int)image.Width != width || (int)image.Height != height)
            {
                image.Resize(width, height);
            }
        }
        
    }
}
