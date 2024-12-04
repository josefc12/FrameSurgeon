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

        // SETTINGS
        public static string HorSetting { get; } = "Amount of frames on the horizontal axis.";
        public static string VerSetting { get; } = "Amount of frames on the vertical axis.";
        public static string FpsSetting { get; } = "Amount of frames the GIF will play in a second.";
        public static string LoopingSetting { get; } = "Should the GIF be looping?";
        public static string TransparentSetting { get; } = "Keep existing transparency?";
        public static string WidthSetting { get; } = "Width of a single frame in pixels.";
        public static string HeightSetting { get; } = "Height of a single frame in pixels.";
        public static string UniformScalingSetting { get; } = "Edit both fields at the same time.";
        public static string FormatSetting { get; } = "Select the format for the result(s).";
    }
}
