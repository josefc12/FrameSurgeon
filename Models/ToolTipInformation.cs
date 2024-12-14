using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameSurgeon.Models
{
    public class ToolTipInformation
    {
        // BUTTONS
        public static string LoadButton { get; } = "Select images to be used for processing.";
        public static string ResetButton { get; } = "Refresh/reset associated values.";
        public static string OutputPathButton { get; } = "Select the output destination for the result(s).";
        public static string MakeButton { get; } = "Make the result and save it at the selected destination under the selected format.";
        public static string PreviewButton { get; } = "Preview the result.";
        public static string AppendButton { get; } = "Append new frames at the end of the list.";
        public static string OpenProjectButton { get; } = "Open saved project.";
        public static string SaveAsProjectButton { get; } = "Save As.";
        public static string SaveProjectButton { get; } = "Save.";

        // SETTINGS
        public static string HorSetting { get; } = "Amount of frames on the horizontal axis.";
        public static string VerSetting { get; } = "Amount of frames on the vertical axis.";
        public static string FpsSetting { get; } = "Amount of frames the GIF will play in a second.";
        public static string LoopingSetting { get; } = "Should the GIF be looping?";
        public static string TransparentSetting { get; } = "Keep existing transparency?";
        public static string SkipFramesSetting { get; } = "This setting doesn't apply to the Dismatle Flipbook process.";
        public static string AnnotateFramesSetting { get; } = "Each frame will be annotated with its index number. " + "\n" + "Use for debugging."  ;
        public static string WidthSetting { get; } = "Width of a single frame in pixels.";
        public static string HeightSetting { get; } = "Height of a single frame in pixels.";
        public static string UniformScalingSetting { get; } = "Edit both fields at the same time.";
        public static string FormatSetting { get; } = "Select the format for the result(s).";
        public static string OpenLastProjectSetting { get; } = "Current state of the program will be saved upon exit " + "\n" + "and loaded again upon launch, regardless of whether it's a saved project or not";
        public static string OpenFolderAfterMakeSetting { get; } = "Opens the output location after processing.";
        
            
    }
}
