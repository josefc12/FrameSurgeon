using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FrameSurgeon.Views;

public partial class DialogWindow : Window
{
    public DialogWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }
}