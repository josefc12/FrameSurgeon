using System.Linq;
using Avalonia.Controls;
using System;
using FrameSurgeon.ViewModels;
using ReactiveUI;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;

namespace FrameSurgeon.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(action =>
                action(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));
    }

    private async Task DoShowDialogAsync(InteractionContext<DialogViewModel,
                                        MainWindowViewModel> interaction)
    {
        var dialog = new DialogWindow();
        dialog.DataContext = interaction.Input;

        var result = await dialog.ShowDialog<MainWindowViewModel>(this);
        interaction.SetOutput(result);
    }
}
