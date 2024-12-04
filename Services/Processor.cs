using FrameSurgeon.Enums;
using FrameSurgeon.Models;
using FrameSurgeon.ViewModels;
using System;
using System.Threading.Tasks;
using FrameSurgeon.Classes;
using ImageMagick;
using Avalonia.Controls;
using System.Diagnostics;
using ImageMagick.Drawing;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using Avalonia.Threading;
using System.Threading;

namespace FrameSurgeon.Services
{
    public class Processor
    {
        public ProcessResult Process(GlobalSettings globalSettings, FlipbookSettings flipbookSettings, GifSettings gifSettings, MainWindowViewModel context)
        {

            //Set progress bar divisions
            Dispatcher.UIThread.Post(() => {context.MaxProgress = 3;});

            //Validate parameters
            ProcessResult validatorResult = Validator.IsMakeAllowed(globalSettings, flipbookSettings);

            //First division complete
            Dispatcher.UIThread.Post(() => {context.CurrentProgress++;});

            if (validatorResult.Result == Result.Failure)
            {
                return validatorResult;
            }

            // Specific process
            ProcessResult result = globalSettings.ExportMode switch
            {
                ExportMode.Flipbook => MakeFlipbook(globalSettings, flipbookSettings, context),
                ExportMode.DismantleFlipbook => DismantleFlipbook(globalSettings, flipbookSettings, context),
                ExportMode.Convert => Convert(globalSettings, context),
                ExportMode.AnimatedGif => MakeAnimatedGif(globalSettings, gifSettings, context),
                _ => new ProcessResult(Result.Failure, "Mode didn't have a set processor function!")
            };
            return result;
        }

        private ProcessResult MakeFlipbook(GlobalSettings globalSettings, FlipbookSettings flipbookSettings, MainWindowViewModel context)
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

                //Max amount from 2nd progress bar division
                var maxProgress = flipbookSettings.vResolution * flipbookSettings.hResolution;

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

                        //Adding to second division
                        Dispatcher.UIThread.Post(() => { context.CurrentProgress += 1.0 / maxProgress; });
                    }
                }

                // Find whether there's a dot at the end of the output path with some kind of an extention
                InputOutput.OutputImage(globalSettings.SelectedExtension, globalSettings.OutputPath, canvas);

                //Adding to third 
                Dispatcher.UIThread.Post(() => { context.CurrentProgress++; });

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            

            return new ProcessResult(Result.Success, "Flipbook created!");
        }

        private ProcessResult DismantleFlipbook(GlobalSettings globalSettings, FlipbookSettings flipbookSettings, MainWindowViewModel context)
        {

            // Check if path exists and is a valid image file
            try
            {
                //First image in the loaded images
                MagickImage image = new MagickImage(globalSettings.LoadedFiles[0]);

                int cropSizeHorizontal = (int)image.Width / flipbookSettings.hResolution;
                int cropSizeVertical = (int)image.Height / flipbookSettings.vResolution;

                //Max amount from 2nd progress bar division
                var maxProgress = flipbookSettings.vResolution * flipbookSettings.hResolution;

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

                        InputOutput.OutputImage(globalSettings.SelectedExtension, globalSettings.OutputPath, image: canvas, null,step);
                        canvas.Dispose();
                        step++;

                        //Adding to second division
                        Dispatcher.UIThread.Post(() => { context.CurrentProgress += 2.0 / maxProgress; });

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

        private ProcessResult Convert(GlobalSettings globalSettings, MainWindowViewModel context)
        {
            try
            {

                //Max amount from 2nd progress bar division
                var maxProgress = globalSettings.LoadedFiles.Count();

                int step = 0;
                //For each loaded path
                foreach (string path in globalSettings.LoadedFiles)
                {
                    //Don't check anything, just load the image and save it as new under the selected format
                    
                    MagickImage image = new MagickImage(path);

                    //Re-scale if applicable
                    ResizeFrame(image, (uint)globalSettings.FrameSizeWidth, (uint)globalSettings.FrameSizeHeight);

                    //The image is disposed of in the OutputImage function
                    InputOutput.OutputImage(globalSettings.SelectedExtension, globalSettings.OutputPath, image: image, null,step);

                    step++;

                    //Adding to second division
                    Dispatcher.UIThread.Post(() => { context.CurrentProgress += 2.0 / maxProgress; });
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return new ProcessResult(Result.Success, "Conversion finished!");

        }

        private ProcessResult MakeAnimatedGif(GlobalSettings globalSettings, GifSettings gifSettings, MainWindowViewModel context)
        {

            //Max amount from 2nd progress bar division
            var maxProgress = globalSettings.LoadedFiles.Count();

            using (var collection = new MagickImageCollection())
            {
                
                // Add frames to the collection
                foreach (string path in globalSettings.LoadedFiles)
                {   
                        

                    // Create a new image for each frame
                    MagickImage image = new MagickImage(path);
                    ResizeFrame(image, (uint)globalSettings.FrameSizeWidth, (uint)globalSettings.FrameSizeHeight);

                    // Set frame delay (in 1/100ths of a second)
                    image.AnimationDelay = 100 / (uint)gifSettings.Fps ;

                    // Add the frame to the collection
                    collection.Add(image);

                    //Adding to second division
                    Dispatcher.UIThread.Post(() => { context.CurrentProgress += 1.0 / maxProgress; });

                }

                // Set the loop count (0 for infinite looping)
                if (gifSettings.Looping)
                {
                    collection[0].AnimationIterations = 0;
                }
                else
                {
                    collection[0].AnimationIterations = 1;
                }

                // Optimize the animation (reduces file size)
                collection.Optimize();

                // Write the animated GIF to file
                InputOutput.OutputImage(globalSettings.SelectedExtension, globalSettings.OutputPath,collection: collection);

                //Adding to third
                Dispatcher.UIThread.Post(() => { context.CurrentProgress++; });
            }

            return new ProcessResult(Result.Success, "Animated GIF created!");
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
