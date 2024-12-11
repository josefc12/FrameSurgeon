using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using FrameSurgeon.Enums;
using FrameSurgeon.ViewModels;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FrameSurgeon.Classes
{
    public class InputOutput : ViewModelBase
    {
        public static async Task<IEnumerable<string>> DoOpenFilePickerAsync()
        {

            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow?.StorageProvider is not { } provider)
                throw new NullReferenceException("Missing StorageProvider instance.");

            var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open image frames",
                FileTypeFilter = new[] {
                    new FilePickerFileType("TARGA Image")
                    {
                        Patterns = new[] { "*.tga" },
                        AppleUniformTypeIdentifiers = new[] { "com.truevision.tga-image" }, // Optional, for macOS
                    },
                    FilePickerFileTypes.ImageAll
                    
                },
                AllowMultiple = true
            });

            return files?.Select(file => file.Path.LocalPath) ?? Enumerable.Empty<string>();
        }

        public static async Task<IStorageFile?> DoOpenOutputPickerAsync()
        {

            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow?.StorageProvider is not { } provider)
                throw new NullReferenceException("Missing StorageProvider instance.");

            return await provider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = "Save File"
            });
        }

        public static void OutputImage(string extension, string path, MagickImage? image = null, MagickImageCollection? collection = null, int? loopPosition = null)
        {
            // Find whether there's a dot at the end of the output path with some kind of an extention
            path = RemoveExtension(path);

            Extension extE = ValueConverter.GetExtensionAsEnumValue(extension);
            string extS = ValueConverter.GetDotExtension(extE);

            string finalPath = loopPosition == null ? path + extS : path + loopPosition + extS;

            if (image != null)
            {
                image.Format = extE switch
                {
                    Extension.TGA => MagickFormat.Tga,
                    Extension.JPEG => MagickFormat.Jpeg,
                    Extension.PNG => MagickFormat.Png,
                };

                image.Write(finalPath);

            }
            else if (collection != null)
            {
                collection.Write(finalPath);
            }
            
        }

        private static string RemoveExtension(string filePath)
        {
            // Loop backward through the file path string
            for (int i = filePath.Length - 1; i >= 0; i--)
            {
                if (filePath[i] == '.')
                {
                    // Return the substring that excludes the extension
                    return filePath.Substring(0, i);
                }
            }

            // Return the original string if no dot is found (i.e., no extension)
            return filePath;
        }

    }

}
