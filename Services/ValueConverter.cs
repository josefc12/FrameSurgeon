using FrameSurgeon.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameSurgeon.Classes
{
    public class ValueConverter
    {
        public static ExportMode GetExportModeAsEnumValue(string exportMode)
        {
            ExportMode mode = exportMode switch
            {
                "Flip book" => ExportMode.FlipBook,
                "Individual Frames" => ExportMode.IndividualFrames,
                "Animated GIF" => ExportMode.AnimatedGif,
                _ => ExportMode.FlipBook
            };
            return mode;
        }

        public static string GetExportModeAsString(ExportMode exportMode)
        {
            string mode = exportMode switch
            {
                ExportMode.FlipBook => "Flip book",
                ExportMode.IndividualFrames => "Individual Frames",
                ExportMode.AnimatedGif => "Animated GIF",
                _ => exportMode.ToString()
            };
            return mode;
        }

        public static List<string> GetConvertedExportModes(IEnumerable<ExportMode> exportModes)
        {
            var convertedModes = new List<string>();

            foreach (ExportMode mode in exportModes)
            {
                string displayName = mode switch
                {
                    ExportMode.FlipBook => "Flip book",
                    ExportMode.IndividualFrames => "Individual Frames",
                    ExportMode.AnimatedGif => "Animated GIF",
                    _ => mode.ToString()
                };
                convertedModes.Add(displayName);
            }
            return convertedModes;
        }
    }
}
