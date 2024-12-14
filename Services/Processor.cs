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
using System.Collections.Generic;
using ImageMagick.Drawing;
using Avalonia.Controls;

namespace FrameSurgeon.Services
{
    public class Processor
    {

        /* Progress bar updating is fairly primitive. Good enough I guess. */

        private MainWindowViewModel _context { get; set; }

        public Processor(MainWindowViewModel context)
        {
            _context = context;
        }

        public ProcessResult Process()
        {

            // Set progress bar divisions
            Dispatcher.UIThread.Post(() => { _context.MaxProgress = 3; });

            // Validate parameters
            ProcessResult validatorResult = Validator.IsMakeAllowed(_context);
            // If validation failed, return
            if (validatorResult.Result == Result.Failure)
            {
                return validatorResult;
            }

            // First division complete
            Dispatcher.UIThread.Post(() => { _context.CurrentProgress++; });

            // Run specific process bases on information in the global settings
            ProcessResult result = ValueConverter.GetExportModeAsEnumValue(_context.SelectedExportMode) switch
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

            if (_context.LoadedFiles.Count() <= 0)
            {
                return new ProcessResult(Result.Failure, "No images loaded!");
            }

            var result = ValueConverter.GetExportModeAsEnumValue(_context.SelectedExportMode) switch
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
                    result.Image?.Dispose();
                    return new ProcessResult(Result.Failure, result.Message);
                }

                var canvas = result.Image;

                // Save/Export/Write the new image
                try
                {
                    InputOutput.OutputImage(_context.SelectedExtension, _context.OutputPath, canvas);
                }
                finally
                {
                    canvas?.Dispose();
                    result.Image?.Dispose();
                }

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
                if (!System.IO.File.Exists(_context.LoadedFiles[0]))
                {
                    return new ProcessResult(Result.Failure, $"Image {Path.GetFileName(_context.LoadedFiles[0])} couldn't be found!");
                }

                //First image in the loaded images
                using (var image = new MagickImage(_context.LoadedFiles[0]))
                {

                    int cropSizeHorizontal = (int)image.Width / _context.FlipbookResolutionHorizontal ?? 0;
                    int cropSizeVertical = (int)image.Height / _context.FlipbookResolutionVertical ?? 0;

                    //Max amount from 2nd progress bar division
                    var maxProgress = _context.FlipbookResolutionVertical * _context.FlipbookResolutionHorizontal;

                    int step = 0;
                    // Loop through rows and columns
                    for (int row = 0; row < _context.FlipbookResolutionVertical; row++)
                    {
                        for (int col = 0; col < _context.FlipbookResolutionHorizontal; col++)
                        {
                            int x = col * cropSizeHorizontal;
                            int y = row * cropSizeVertical;
                            // Area of the pixels to be extracted
                            var cropArea = new MagickGeometry(x, y, (uint)cropSizeHorizontal, (uint)cropSizeVertical);

                            using (var croppedImage = (MagickImage)image.Clone())
                            {
                                croppedImage.Crop(cropArea);

                                //Re-scale if applicable
                                ResizeFrame(croppedImage, (uint)(_context.FrameSizeWidth ?? 0), (uint)(_context.FrameSizeHeight ?? 0));

                                // Create a new image/canvas to paste the pixels onto
                                MagickColor color = _context.TransparencyEnabled == true ? MagickColors.Transparent : MagickColors.Black;

                                //Create canvas
                                using (var canvas = new MagickImage(color, croppedImage.Width, croppedImage.Height))
                                {
                                    canvas.Composite(croppedImage, 0, 0, CompositeOperator.Over);

                                    //Annotate if applicable
                                    if (_context.AnnotateFramesEnabled)
                                    {
                                        ApplyAnnotation(canvas, step.ToString());
                                    }

                                    InputOutput.OutputImage(_context.SelectedExtension, _context.OutputPath, image: canvas, null, step);
                                }

                            }

                            step++;

                            //Adding to second division
                            Dispatcher.UIThread.Post(() => { _context.CurrentProgress += 2.0 / maxProgress; });

                        }
                    }
                }
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
                var maxProgress = _context.LoadedFiles.Count();
                int step = 0;

                List<string> finalFrames = _context.LoadedFiles.ToList();
                if (_context.SkipFramesEnabled)
                {
                    finalFrames = ApplyFrameReduction(_context.LoadedFiles.ToList());
                }

                //For each loaded path
                foreach (string path in finalFrames)
                {

                    // Check if the fil still exists
                    if (!System.IO.File.Exists(path))
                    {
                        return new ProcessResult(Result.Failure, $"Image {Path.GetFileName(path)} couldn't be found!");
                    }

                    //Don't check anything, just load the image and save it as new under the selected format
                    using (var image = new MagickImage(path))
                    {

                        //Re-scale if applicable
                        ResizeFrame(image, (uint)(_context.FrameSizeWidth ?? 0), (uint)(_context.FrameSizeHeight ?? 0));

                        //Annotate if applicable
                        if (_context.AnnotateFramesEnabled)
                        {
                            ApplyAnnotation(image, step.ToString());
                        }

                        //The image is disposed of in the OutputImage function
                        InputOutput.OutputImage(_context.SelectedExtension, _context.OutputPath, image: image, null, step);
                        step++;
                    }
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
            try
            {
                // Create the new image
                var result = ProcessAnimatedGif(false);

                if (result.Result == Result.Failure)
                {
                    return new ProcessResult(Result.Failure, result.Message);
                }

                using (var collection = result.Collection)
                {
                    // Write the animated GIF to file
                    InputOutput.OutputImage(_context.SelectedExtension, _context.OutputPath, collection: collection);
                }

                //Adding to third
                Dispatcher.UIThread.Post(() => { _context.CurrentProgress++; });

                return new ProcessResult(Result.Success, "Animated GIF created!");
            }
            catch (Exception e)
            {
                // Provide more context and preserve original exception details
                throw new Exception("An error occurred while creating the animated GIF.", e);
            }
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

            if (!System.IO.File.Exists(_context.LoadedFiles[0]))
            {
                return new ProcessResult(Result.Failure, $"Image {Path.GetFileName(_context.LoadedFiles[0])} couldn't be found!");
            }

            List<string> finalFrames = _context.LoadedFiles.ToList();
            if (_context.SkipFramesEnabled)
            {
                finalFrames = ApplyFrameReduction(_context.LoadedFiles.ToList());
            }

            //New image
            MagickImage firstImage = new MagickImage(_context.LoadedFiles[0]);

            //Re-scale if applicable
            ResizeFrame(firstImage, (uint)(_context.FrameSizeWidth ?? 0), (uint)(_context.FrameSizeHeight ?? 0));

            //Image size
            uint newWidth = (uint)((_context.FlipbookResolutionHorizontal ?? 0) * firstImage.Width);
            uint newHeight = (uint)((_context.FlipbookResolutionVertical ?? 0) * firstImage.Height);

            //Apply transparency, if enabled by the user
            MagickColor color = _context.TransparencyEnabled == true ? MagickColors.Transparent : MagickColors.Black;

            //Create canvas
            MagickImage canvas = new MagickImage(color, newWidth, newHeight);

            //Max amount from 2nd progress bar division
            var maxProgress = _context.FlipbookResolutionVertical * _context.FlipbookResolutionHorizontal;

            int step = 0;
            // Loop through rows and columns
            for (int row = 0; row < _context.FlipbookResolutionVertical; row++)
            {
                for (int col = 0; col < _context.FlipbookResolutionHorizontal; col++)
                {

                    int x = col * (int)firstImage.Width;
                    int y = row * (int)firstImage.Height;

                    if (step <= finalFrames.Count - 1)
                    {

                        // Check if the file still exists
                        if (!System.IO.File.Exists(finalFrames[step]))
                        {
                            firstImage.Dispose();
                            canvas.Dispose();
                            return new ProcessResult(Result.Failure, $"Image {Path.GetFileName(finalFrames[step])} couldn't be found!");
                        }

                        // Area of the pixels to be extracted
                        MagickImage image = new MagickImage(finalFrames[step]);

                        //Re-scale if applicable
                        ResizeFrame(image, (uint)(_context.FrameSizeWidth ?? 0), (uint)(_context.FrameSizeHeight ?? 0));

                        if (_context.AnnotateFramesEnabled)
                        {
                            ApplyAnnotation(image, step.ToString());
                        }

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
            firstImage.Dispose();
            return new ProcessResult(Result.Success, null, canvas, null);
        }

        private ProcessResult ProcessAnimatedGif(bool isPreview)
        {
            int maxProgress = 0;
            if (!isPreview)
            {
                //Max amount from 2nd progress bar division
                maxProgress = _context.LoadedFiles.Count();
            }
            var collection = new MagickImageCollection();

            List<string> finalFrames = _context.LoadedFiles.ToList();
            if (_context.SkipFramesEnabled)
            {
                finalFrames = ApplyFrameReduction(_context.LoadedFiles.ToList());
            }

            // Add frames to the collection
            foreach (string path in finalFrames)
            {
                // Check if the fil still exists
                if (!System.IO.File.Exists(path))
                {
                    collection.Dispose();
                    return new ProcessResult(Result.Failure, $"Image {Path.GetFileName(path)} couldn't be found!");
                }
                // Create a new image for each frame
                MagickImage image = new MagickImage(path);
                ResizeFrame(image, (uint)(_context.FrameSizeWidth ?? 0), (uint)(_context.FrameSizeHeight ?? 0));

                if (_context.AnnotateFramesEnabled)
                {
                    ApplyAnnotation(image, finalFrames.IndexOf(path).ToString());
                }

                // Set frame delay (in 1/100ths of a second)
                image.AnimationDelay = 100 / (uint)(_context.GifFps ?? 30);

                // Add the frame to the collection
                collection.Add(image);

                if (!isPreview)
                {
                    //Adding to second division
                    Dispatcher.UIThread.Post(() => { _context.CurrentProgress += 1.0 / maxProgress; });
                }

            }

            // Set the loop count (0 for infinite looping)
            if (_context.GifLooping)
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

        private List<string> ApplyFrameReduction(List<string> loadedFrames)
        {
            List<string> reducedFrames = new List<string>();
            foreach (var b in loadedFrames)
            {
                if (loadedFrames.IndexOf(b) % 2 == 0)
                {
                    reducedFrames.Add(b);
                }
            }

            return reducedFrames;

        }

        private void ApplyAnnotation(MagickImage frame, string annotation)
        {
            // Create a drawables object for drawing text
            var drawables = new Drawables()
                .Font("Noto Sans",FontStyleType.Normal,FontWeight.Bold,FontStretch.Normal)    
                .FontPointSize(24)           
                .FillColor(MagickColors.White)
                .StrokeColor(MagickColors.Black)
                .StrokeWidth(0.3)
                .TextAlignment(TextAlignment.Center)
                .Text(frame.Width /2 , frame.Height -8 , annotation); 

            // Apply the drawables to the image
            drawables.Draw(frame);
        }


    }
}
