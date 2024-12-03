using Avalonia.Controls;
using FrameSurgeon.Classes;
using FrameSurgeon.Enums;
using FrameSurgeon.Models;
using FrameSurgeon.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Metadata.Ecma335;

namespace FrameSurgeon.Services
{
    public static class Validator
    {
        public static ProcessResult IsMakeAllowed(GlobalSettings globalSettings, FlipbookSettings flipbookSettings)
        {

            foreach (var step in Enum.GetValues<Step>())
            {
                ProcessResult result = step switch
                {
                    Step.ImagesLoaded => CheckImagesLoaded(globalSettings.ExportMode, globalSettings.LoadedFiles),
                    Step.ParametersCorrect => CheckParameters(globalSettings, flipbookSettings),
                    Step.OutputPathSet => globalSettings.OutputPath == "" || globalSettings.OutputPath == null ? new ProcessResult(Result.Failure, "Please set the output path.") : new ProcessResult(Result.Success),
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



        private static ProcessResult CheckParameters(GlobalSettings globalSettings, FlipbookSettings flipbookSettings)
        {

            if (GlobalSettingsValid(globalSettings) == false)
            {
                return new ProcessResult(Result.Failure, "Global settings are invalid.");
            }

            ProcessResult parametersValid = globalSettings.ExportMode switch
            {
                ExportMode.Flipbook => CheckFlipbookParameters(flipbookSettings),
                ExportMode.DismantleFlipbook => CheckDismantleFlipbookParameters(flipbookSettings),
                ExportMode.Convert => CheckConversionParameters(),
                ExportMode.AnimatedGif => CheckAnimatedGifParameters() //TODO create pass settings object
            };

            return parametersValid;
        }

        private static ProcessResult CheckFlipbookParameters(FlipbookSettings flipbookSettings)
        {

            if (flipbookSettings.hResolution == 0 || flipbookSettings.vResolution == 0)
            {
                return new ProcessResult(Result.Failure, "Please make sure flipbook dimensions aren't set to 0.");
            }

            return new ProcessResult(Result.Success, "Flipbook can be created safely.");

        }

        private static ProcessResult CheckDismantleFlipbookParameters(FlipbookSettings flipbookSettings)
        {

            if (flipbookSettings.hResolution >= 2 || flipbookSettings.vResolution >= 2)
            {
                if (flipbookSettings.hResolution == 0 || flipbookSettings.vResolution == 0)
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

        private static bool GlobalSettingsValid(GlobalSettings globalSettings)
        {
            //TODO
            return true;
        }
    }
}
