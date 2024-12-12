using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FrameSurgeon.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }
}