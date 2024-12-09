using FrameSurgeon.Enums;
using FrameSurgeon.Models;
using FrameSurgeon.ViewModels;
using System;
using FrameSurgeon.Classes;
using ImageMagick;
using System.Linq;
using Avalonia.Threading;
using System.IO;
using System.Diagnostics;

namespace FrameSurgeon.Services
{
    public class Processor
    {

        /* Progress bar updating is fairly primitive. Good enough I guess. */

        private GlobalSettings _globalSettings { get; set; }
        private FlipbookSettings _flipbookSettings { get; set; }
        private GifSettings _gifSettings { get; set; }
        private MainWindowViewModel _context { get; set; }

        public  Processor(GlobalSettings globalSettings, FlipbookSettings flipbookSettings, GifSettings gifSettings, MainWindowViewModel context)
        {
            _globalSettings = globalSettings;
            _flipbookSettings = flipbookSettings;
            _gifSettings = gifSettings;
            _context = context;
        }

        public ProcessResult Process()
        {

            // Set progress bar divisions
            Dispatcher.UIThread.Post(() => { _context.MaxProgress = 3;});

            // Validate parameters
            ProcessResult validatorResult = Validator.IsMakeAllowed(_globalSettings, _flipbookSettings);
            // If validation failed, return
            if (validatorResult.Result == Result.Failure)
            {
                return validatorResult;
            }

            // First division complete
            Dispatcher.UIThread.Post(() => { _context.CurrentProgress++;});

            // Run specific process bases on information in the global settings
            ProcessResult result = _globalSettings.ExportMode switch
            {
                ExportMode.Flipbook => MakeFlipbook(),
                ExportMode.DismantleFlipbook => DismantleFlipbook(),
                ExportMode.Convert => Convert(),
                ExportMode.AnimatedGif => MakeAnimatedGif(),
                _ => new ProcessResult(Result.Failure, "Mode didn't have a set processor function!")
            };
            return result;
        }

        public ProcessResult Preview()
        {

            if (_globalSettings.LoadedFiles.Count() <= 0)
            {
               return new ProcessResult(Result.Failure, "No images loaded!");
            }

            var result = _globalSettings.ExportMode switch
            {
                ExportMode.Flipbook => ProcessFlipbook(true),
                ExportMode.DismantleFlipbook => new ProcessResult(Result.Failure, "This mode can't be previewed."),
                ExportMode.Convert => new ProcessResult(Result.Failure, "This mode can't be previewed."),
                ExportMode.AnimatedGif => ProcessAnimatedGif(true),
                _ => new ProcessResult(Result.Failure, "Mode didn't have a set preview function!")
            };

            return result;
        }

        private ProcessResult MakeFlipbook()
        {

            try
            {
                // Create the new image
                var result = ProcessFlipbook(false);

                if (result.Result == Result.Failure)
                {
                    return new ProcessResult(Result.Failure, result.Message);
                }

                var canvas = result.Image;

                // Save/Export/Write the new image
                InputOutput.OutputImage(_globalSettings.SelectedExtension, _globalSettings.OutputPath, canvas);

                //Adding to third 
                Dispatcher.UIThread.Post(() => { _context.CurrentProgress++; });

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            

            return new ProcessResult(Result.Success, "Flipbook created!");
        }

        private ProcessResult DismantleFlipbook()
        {

            // Check if path exists and is a valid image file
            try
            {
                
                // Check if the file still exists
                if (!System.IO.File.Exists(_globalSettings.LoadedFiles[0]))
                {
                    return new ProcessResult(Result.Failure, $"Image {Path.GetFileName(_globalSettings.LoadedFiles[0])} couldn't be found!");
                }

                //First image in the loaded images
                MagickImage image = new MagickImage(_globalSettings.LoadedFiles[0]);

                int cropSizeHorizontal = (int)image.Width / _flipbookSettings.hResolution;
                int cropSizeVertical = (int)image.Height / _flipbookSettings.vResolution;

                //Max amount from 2nd progress bar division
                var maxProgress = _flipbookSettings.vResolution * _flipbookSettings.hResolution;

                int step = 0;
                // Loop through rows and columns
                for (int row = 0; row < _flipbookSettings.vResolution; row++)
                {
                    for (int col = 0; col < _flipbookSettings.hResolution; col++)
                    {
                        int x = col * cropSizeHorizontal;
                        int y = row * cropSizeVertical;
                        // Area of the pixels to be extracted
                        var cropArea = new MagickGeometry(x, y, (uint)cropSizeHorizontal, (uint)cropSizeVertical);

                        MagickImage croppedImage = (MagickImage)image.Clone();
                        croppedImage.Crop(cropArea);

                        //Re-scale if applicable
                        ResizeFrame(croppedImage, (uint)_globalSettings.FrameSizeWidth, (uint)_globalSettings.FrameSizeHeight);

                        // Create a new image/canvas to paste the pixels onto
                        MagickColor color = _globalSettings.TransparencyEnabled == true ? MagickColors.Transparent : MagickColors.Black;

                        //Create canvas
                        MagickImage canvas = new MagickImage(color, croppedImage.Width, croppedImage.Height);


                        canvas.Composite(croppedImage, 0, 0, CompositeOperator.Over);

                        InputOutput.OutputImage(_globalSettings.SelectedExtension, _globalSettings.OutputPath, image: canvas, null, step);
                        
                        canvas.Dispose();
                        step++;

                        //Adding to second division
                        Dispatcher.UIThread.Post(() => { _context.CurrentProgress += 2.0 / maxProgress; });

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

        private ProcessResult Convert()
        {
            try
            {

                //Max amount from 2nd progress bar division
                var maxProgress = _globalSettings.LoadedFiles.Count();

                int step = 0;
                //For each loaded path
                foreach (string path in _globalSettings.LoadedFiles)
                {

                    // Check if the fil still exists
                    if (!System.IO.File.Exists(path))
                    {
                        return new ProcessResult(Result.Failure, $"Image {Path.GetFileName(path)} couldn't be found!");
                    }

                    //Don't check anything, just load the image and save it as new under the selected format
                    MagickImage image = new MagickImage(path);

                    //Re-scale if applicable
                    ResizeFrame(image, (uint)_globalSettings.FrameSizeWidth, (uint)_globalSettings.FrameSizeHeight);

                    //The image is disposed of in the OutputImage function
                    InputOutput.OutputImage(_globalSettings.SelectedExtension, _globalSettings.OutputPath, image: image, null, step);

                    step++;

                    //Adding to second division
                    Dispatcher.UIThread.Post(() => { _context.CurrentProgress += 2.0 / maxProgress; });
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return new ProcessResult(Result.Success, "Conversion finished!");

        }

        private ProcessResult MakeAnimatedGif()
        {

            // Create the new image
            var result = ProcessAnimatedGif(false);

            if (result.Result == Result.Failure)
            {
                return new ProcessResult(Result.Failure, result.Message);
            }

            var collection = result.Collection;

            // Write the animated GIF to file
            InputOutput.OutputImage(_globalSettings.SelectedExtension, _globalSettings.OutputPath, collection: collection);
              
            //Adding to third
            Dispatcher.UIThread.Post(() => { _context.CurrentProgress++; });

            return new ProcessResult(Result.Success, "Animated GIF created!");
        }

        private void ResizeFrame(MagickImage image, uint width, uint height)
        {
            if ((int)image.Width != width || (int)image.Height != height)
            {
                image.Resize(width, height);
            }
        }

        private ProcessResult ProcessFlipbook(bool isPreview)
        {

            if (!System.IO.File.Exists(_globalSettings.LoadedFiles[0]))
            {
                return new ProcessResult(Result.Failure, $"Image {Path.GetFileName(_globalSettings.LoadedFiles[0])} couldn't be found!");
            }

            //New image
            MagickImage firstImage = new MagickImage(_globalSettings.LoadedFiles[0]);

            //Re-scale if applicable
            ResizeFrame(firstImage, (uint)_globalSettings.FrameSizeWidth, (uint)_globalSettings.FrameSizeHeight);

            //Image size
            uint newWidth = (uint)(_flipbookSettings.hResolution * firstImage.Width);
            uint newHeight = (uint)(_flipbookSettings.vResolution * firstImage.Height);

            //Apply transparency, if enabled by the user
            MagickColor color = _globalSettings.TransparencyEnabled == true ? MagickColors.Transparent : MagickColors.Black;

            //Create canvas
            MagickImage canvas = new MagickImage(color, newWidth, newHeight);

            //Max amount from 2nd progress bar division
            var maxProgress = _flipbookSettings.vResolution * _flipbookSettings.hResolution;

            int step = 0;
            // Loop through rows and columns
            for (int row = 0; row < _flipbookSettings.vResolution; row++)
            {
                for (int col = 0; col < _flipbookSettings.hResolution; col++)
                {
                    int x = col * (int)firstImage.Width;
                    int y = row * (int)firstImage.Height;

                    if (step <= _globalSettings.LoadedFiles.Count - 1)
                    {

                        // Check if the file still exists
                        if (!System.IO.File.Exists(_globalSettings.LoadedFiles[step]))
                        {
                            return new ProcessResult(Result.Failure, $"Image {Path.GetFileName(_globalSettings.LoadedFiles[step])} couldn't be found!");
                        }

                        // Area of the pixels to be extracted
                        MagickImage image = new MagickImage(_globalSettings.LoadedFiles[step]);

                        //Re-scale if applicable
                        ResizeFrame(image, (uint)_globalSettings.FrameSizeWidth, (uint)_globalSettings.FrameSizeHeight);

                        canvas.Composite(image, x, y, CompositeOperator.Over);
                        image.Dispose();

                    }
                    step++;

                    if (!isPreview)
                    {
                        //Adding to second division
                        Dispatcher.UIThread.Post(() => { _context.CurrentProgress += 1.0 / maxProgress; });
                    }
                    
                }
            }
            return new ProcessResult(Result.Success, null, canvas, null);
        }

        private ProcessResult ProcessAnimatedGif(bool isPreview)
        {
            int maxProgress=0;
            if (!isPreview)
            {
                //Max amount from 2nd progress bar division
                maxProgress = _globalSettings.LoadedFiles.Count();
            }
            var collection = new MagickImageCollection();

            // Add frames to the collection
            foreach (string path in _globalSettings.LoadedFiles)
            {
                // Check if the fil still exists
                if (!System.IO.File.Exists(path))
                {
                    return new ProcessResult(Result.Failure, $"Image {Path.GetFileName(path)} couldn't be found!");
                }
                // Create a new image for each frame
                MagickImage image = new MagickImage(path);
                ResizeFrame(image, (uint)_globalSettings.FrameSizeWidth, (uint)_globalSettings.FrameSizeHeight);

                // Set frame delay (in 1/100ths of a second)
                image.AnimationDelay = 100 / (uint)_gifSettings.Fps;

                // Add the frame to the collection
                collection.Add(image);

                if (!isPreview)
                {
                    //Adding to second division
                    Dispatcher.UIThread.Post(() => { _context.CurrentProgress += 1.0 / maxProgress; });
                }

            }

            // Set the loop count (0 for infinite looping)
            if (_gifSettings.Looping)
            {
                collection[0].AnimationIterations = 0;
            }
            else
            {
                collection[0].AnimationIterations = 1;
            }

            // Optimize the animation (reduces file size)
            //collection.Optimize();

            return new ProcessResult(Result.Success, null, null, collection);
            

        }

    }
}
