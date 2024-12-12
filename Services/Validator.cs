using Avalonia.Controls;
using FrameSurgeon.Classes;
using FrameSurgeon.Enums;
using FrameSurgeon.Models;
using FrameSurgeon.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace FrameSurgeon.Services
{
    public static class Validator
    {
        public static ProcessResult IsMakeAllowed(MainWindowViewModel context)
        {
            // Run a specific set of checks and functions for each of the Steps
            foreach (var step in Enum.GetValues<Step>())
            {
                ProcessResult result = step switch
                {
                    Step.ImagesLoaded => CheckImagesLoaded(ValueConverter.GetExportModeAsEnumValue(context.SelectedExportMode), context.LoadedFiles.ToList()),
                    Step.ParametersCorrect => CheckParameters(context),
                    Step.OutputPathSet => context.OutputPath == "" || context.OutputPath == null ? new ProcessResult(Result.Failure, "Please set the output path.") : new ProcessResult(Result.Success),
                };

                if (result.Result == Result.Failure)
                {
                    return result;
                }

            }

            return new ProcessResult(Result.Success, "Finihsed!");

        }

        private static ProcessResult CheckImagesLoaded(ExportMode selectedExportMode ,List<string> imagesList)
        {
            ProcessResult result = selectedExportMode switch
            {
                ExportMode.Flipbook => imagesList.Count <= 1 ? new ProcessResult(Result.Failure, "Please add more than 1 image.") : new ProcessResult(Result.Success),
                ExportMode.DismantleFlipbook => imagesList.Count != 1 ? new ProcessResult(Result.Failure, "The list must consist of exactly 1 image.") : new ProcessResult(Result.Success),
                ExportMode.Convert => imagesList.Count <= 0 ? new ProcessResult(Result.Failure, "Please add image(s).") : new ProcessResult(Result.Success),
                ExportMode.AnimatedGif => imagesList.Count <= 1 ? new ProcessResult(Result.Failure, "Please add more than 1 image.") : new ProcessResult(Result.Success),
            };

            return result;

        }



        private static ProcessResult CheckParameters(MainWindowViewModel context)
        {

            if (GlobalSettingsValid(context) == false)
            {
                return new ProcessResult(Result.Failure, "Global settings are invalid.");
            }

            ProcessResult parametersValid = ValueConverter.GetExportModeAsEnumValue(context.SelectedExportMode) switch
            {
                ExportMode.Flipbook => CheckFlipbookParameters(context),
                ExportMode.DismantleFlipbook => CheckDismantleFlipbookParameters(context),
                ExportMode.Convert => CheckConversionParameters(),
                ExportMode.AnimatedGif => CheckAnimatedGifParameters() //TODO create pass settings object
            };

            return parametersValid;
        }

        private static ProcessResult CheckFlipbookParameters(MainWindowViewModel context)
        {

            if (context.FlipbookResolutionHorizontal == 0 || context.FlipbookResolutionVertical == 0)
            {
                return new ProcessResult(Result.Failure, "Please make sure flipbook dimensions aren't set to 0.");
            }

            return new ProcessResult(Result.Success, "Flipbook can be created safely.");

        }

        private static ProcessResult CheckDismantleFlipbookParameters(MainWindowViewModel context)
        {

            if (context.FlipbookResolutionHorizontal >= 2 || context.FlipbookResolutionVertical >= 2)
            {
                if (context.FlipbookResolutionHorizontal == 0 || context.FlipbookResolutionVertical == 0)
                {
                    return new ProcessResult(Result.Failure, "Please make sure flipbook dimensions aren't set to 0.");
                }
                return new ProcessResult(Result.Success, "Flipbook can be dismantled safely.");

            }
            return new ProcessResult(Result.Failure, "Please make sure that at least one flipbook dimension is set to more than 2.");
           
        }

        private static ProcessResult CheckConversionParameters()
        {
            return new ProcessResult(Result.Success, "Images can be converted safely.");
        }

        private static ProcessResult CheckAnimatedGifParameters()
        {

            //TODO
            return new ProcessResult(Result.Success, "GIF can be created safely.");

        }

        private static bool GlobalSettingsValid(MainWindowViewModel context)
        {
            //TODO
            return true;
        }
    }
}
