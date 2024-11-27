using Avalonia.Controls;
using FrameSurgeon.Classes;
using FrameSurgeon.Enums;
using FrameSurgeon.Models;
using FrameSurgeon.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Reflection.Metadata.Ecma335;

namespace FrameSurgeon.Services
{
    public static class Validator
    {
        //TODO
        //MAYBE INSTEAD OF PASSING THE WHOLE MAINWINDOWVIEWMODEL PASS ONLY SOME DATA.
        public static ProcessResult IsMakeAllowed(ref MainWindowViewModel context)
        {

            foreach (var step in Enum.GetValues<Step>())
            {
                ProcessResult result = step switch
                {
                    Step.ImagesLoaded => CheckImagesLoaded(context.SelectedExportMode, context.LoadedFiles),
                    Step.ParametersCorrect => CheckParameters(ref context),
                    Step.OutputPathSet => context.OutputPath == "" || context.OutputPath == null ? new ProcessResult(Result.Failure, "Please set the output path.") : new ProcessResult(Result.Success),
                };

                if (result.Result == Result.Failure)
                {
                    return result;
                }
            }

            return new ProcessResult(Result.Success, "Finihsed!");

        }

        private static ProcessResult CheckImagesLoaded(string selectedExportMode ,ObservableCollection<string> imagesList)
        {
            ExportMode exportMode = ValueConverter.GetExportModeAsEnumValue(selectedExportMode);

            ProcessResult result = exportMode switch
            {
                ExportMode.Flipbook => imagesList.Count <= 1 ? new ProcessResult(Result.Failure, "Please add more than 1 image.") : new ProcessResult(Result.Success),
                ExportMode.DismantleFlipbook => imagesList.Count != 1 ? new ProcessResult(Result.Failure, "The list must consist of exactly 1 image.") : new ProcessResult(Result.Success),
                ExportMode.Convert => imagesList.Count <= 0 ? new ProcessResult(Result.Failure, "Please add iamge(s).") : new ProcessResult(Result.Success),
                ExportMode.AnimatedGif => imagesList.Count <= 1 ? new ProcessResult(Result.Failure, "Please add more than 1 image.") : new ProcessResult(Result.Success),
            };

            return result;

        }



        private static ProcessResult CheckParameters(ref MainWindowViewModel context)
        {

            if (GlobalSettingsValid(ref context) == false)
            {
                return new ProcessResult(Result.Failure, "Global settings are invalid.");
            }

            ProcessResult parametersValid = ValueConverter.GetExportModeAsEnumValue(context.SelectedExportMode) switch
            {
                ExportMode.Flipbook => CheckFlipbookParameters(ref context),
                ExportMode.DismantleFlipbook => CheckDismantleFlipbookParameters(ref context),
                ExportMode.Convert => CheckConversionParameters(ref context),
                ExportMode.AnimatedGif => CheckAnimatedGifParameters(ref context)
            };

            return parametersValid;
        }

        private static ProcessResult CheckFlipbookParameters(ref MainWindowViewModel context)
        {

            if (context.FlipbookResolutionHorizontal == 0 || context.FlipbookResolutionVertical == 0)
            {
                return new ProcessResult(Result.Failure, "Please make sure flipbook dimensions aren't set to 0.");
            }

            return new ProcessResult(Result.Success, "Flipbook can be created safely.");

        }

        private static ProcessResult CheckDismantleFlipbookParameters(ref MainWindowViewModel context)
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

        private static ProcessResult CheckConversionParameters(ref MainWindowViewModel context)
        {
            return new ProcessResult(Result.Success, "Images can be converted safely.");
        }

        private static ProcessResult CheckAnimatedGifParameters(ref MainWindowViewModel context)
        {

            //TODO
            return new ProcessResult(Result.Success, "GIF can be created safely.");

        }

        private static bool GlobalSettingsValid(ref MainWindowViewModel context)
        {
            //TODO
            return true;
        }
    }
}
