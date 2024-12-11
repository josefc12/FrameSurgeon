using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Linq;
using System;

namespace FrameSurgeon.Views;

public partial class DockPanelLeftComponent : UserControl
{
    public DockPanelLeftComponent()
    {
        InitializeComponent();
    }

    private void DataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dGrid)
        {

            dGrid.SelectedItem = null;
        }
    }
}