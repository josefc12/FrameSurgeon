using FrameSurgeon.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace FrameSurgeon.Classes
{
    public class ValueConverter
    {
        public static ExportMode GetExportModeAsEnumValue(string exportMode)
        {
            ExportMode mode = exportMode switch
            {
                "Flipbook" => ExportMode.Flipbook,
                "Dismantle Flipbook" => ExportMode.DismantleFlipbook,
                "Convert" => ExportMode.Convert,
                "Animated GIF" => ExportMode.AnimatedGif,
                _ => ExportMode.Flipbook
            };
            return mode;
        }

        public static string GetExportModeAsString(ExportMode exportMode)
        {
            string mode = exportMode switch
            {
                ExportMode.Flipbook => "Flipbook",
                ExportMode.DismantleFlipbook => "Dismantle Flipbook",
                ExportMode.Convert => "Convert",
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
                    ExportMode.Flipbook => "Flipbook",
                    ExportMode.DismantleFlipbook => "Dismantle Flipbook",
                    ExportMode.Convert => "Convert",
                    ExportMode.AnimatedGif => "Animated GIF",
                    _ => mode.ToString()
                };
                convertedModes.Add(displayName);
            }
            return convertedModes;
        }

        public static Extension GetExtensionAsEnumValue(string extension)
        {
            Extension ext = extension switch
            {
                "TGA" => Extension.TGA,
                "JPEG" => Extension.JPEG,
                "PNG" => Extension.PNG,
                "GIF" => Extension.GIF,
                _ => Extension.TGA
            };
            return ext;
        }

        public static string GetExtensionAsString(Extension extension)
        {
            string ext = extension switch
            {
                Extension.TGA => "TGA",
                Extension.JPEG => "JPEG",
                Extension.PNG => "PNG",
                Extension.GIF => "GIF",
                _ => extension.ToString()
            };
            return ext;
        }

        public static List<string> GetConvertedExtensions(ExportMode exportMode)
        {
            var convertedExtensions = new List<string>();

            if (exportMode == ExportMode.AnimatedGif)
            {
                convertedExtensions.Add("GIF");
                return convertedExtensions;
            }
            //Else
            foreach (Extension ext in Enum.GetValues(typeof(Extension)))
            {
                if (ext != Extension.GIF)
                {
                    string displayName = ext switch
                    {
                        Extension.TGA => "TGA",
                        Extension.JPEG => "JPEG",
                        Extension.PNG => "PNG",
                        _ => ext.ToString()
                    };
                    convertedExtensions.Add(displayName);
                }
                
            }
            return convertedExtensions;
        }

        public static string GetDotExtension(Extension extension)
        {
            string ext = extension switch
            {
                Extension.TGA => ".tga",
                Extension.JPEG => ".jpg",
                Extension.PNG => ".png",
                Extension.GIF => ".gif",
                _ => extension.ToString()
            };
            return ext;
        }
    }
}
