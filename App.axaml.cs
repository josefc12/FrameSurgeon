using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FrameSurgeon.ViewModels;
using FrameSurgeon.Views;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FrameSurgeon;

public partial class App : Application
{

    private readonly List<Window> _previewWindows = new List<Window>();
    public IReadOnlyList<Window> PreviewWindows => _previewWindows;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(this),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    public void RegisterPreviewWindow(Window window)
    {
        _previewWindows.Add(window);
        window.Closed += (s, e) => _previewWindows.Remove(window);
    }

}