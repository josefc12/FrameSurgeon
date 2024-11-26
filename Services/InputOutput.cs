using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FrameSurgeon.Enums;
using FrameSurgeon.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll },
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
    }

}
