using System.Linq;
using Avalonia.Controls;
using System;
using FrameSurgeon.ViewModels;
using ReactiveUI;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Avalonia.Input;
using System.Reactive;
using System.Diagnostics;
using FrameSurgeon.Models;
using FrameSurgeon.Services;

namespace FrameSurgeon.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(
            action =>action(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));

        this.WhenActivated(
            action => action(ViewModel!.ShowSettings.RegisterHandler(DoShowSettingsAsync)));

        this.Closing += MainWindow_Closing;
    }

    private async Task DoShowDialogAsync(InteractionContext<DialogViewModel,
                                        MainWindowViewModel> interaction)
    {
        var dialog = new DialogWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<MainWindowViewModel>(this);
        interaction.SetOutput(result);
    }

    private async Task DoShowSettingsAsync(InteractionContext<MainWindowViewModel,
                                        MainWindowViewModel> interaction)
    {
        var settings = new SettingsWindow();
        settings.DataContext = interaction.Input;

        var result = await settings.ShowDialog<MainWindowViewModel>(this);
        interaction.SetOutput(result);
    }

    private void OnTextInput(object sender, TextInputEventArgs e)
    {
        if (!char.IsDigit(e.Text[0]) && e.Text != "\b" && e.Text != "\u007F") // '\b' for backspace, '\u007F' for delete
        {
            e.Handled = true; // Block non-numeric input
        }
    }
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Allow only numeric keys (0-9), backspace, delete, tab, and enter keys.
        if (!char.IsDigit((char)e.Key) && e.Key != Key.Back && e.Key != Key.Delete && e.Key != Key.Tab && e.Key != Key.Enter)
        {
            e.Handled = true; // Prevent non-numeric key presses (letters and special chars)
        }
    }
    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Close all preview windows before the main window closes
        if (App.Current is App app)
        {
            //Save startup file if option enabled
            var context = this.DataContext as MainWindowViewModel;

            if (context.OpenLastProjectEnabled)
            {
                var startupFileSettings = new ProjectSettings(context);
                Saviour.SaveProjectStartupFile(startupFileSettings);
            }
            
            foreach (var previewWindow in app.PreviewWindows.ToList())
            {
                previewWindow.Close();
            }
        }
    }

}
