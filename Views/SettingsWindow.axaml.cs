using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FrameSurgeon.Models;
using FrameSurgeon.Services;
using FrameSurgeon.ViewModels;

namespace FrameSurgeon.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var viewModel = this.DataContext as MainWindowViewModel;
        //Save settings
        if (viewModel != null)
        {
            UserSettings userSettings = new UserSettings(viewModel);
            
            Saviour.SaveUserSettings(userSettings);
            
        }
        
        this.Close();
    }
}