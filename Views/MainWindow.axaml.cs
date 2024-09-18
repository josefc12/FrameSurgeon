using System.Linq;
using Avalonia.Controls;
using System;

namespace framesurgeon_csharp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        frames.ItemsSource = new string[] {"cat", "camel", "cow", "chameleon", "mouse", "lion", "zebra" }.OrderBy(x => x);
        Console.WriteLine("Fuck microsoft");
    }
}
