using System.Linq;
using Avalonia.Controls;
using System;
using FrameSurgeon.ViewModels;
using ReactiveUI;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Avalonia.Input;

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

    private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // Filter the text to allow only numeric characters
            string newText = new string(textBox.Text.Where(char.IsDigit).ToArray());

            // Update the TextBox only if there are changes
            if (textBox.Text != newText)
            {
                int caretIndex = textBox.SelectionStart; // Save caret position
                textBox.Text = newText;
            
                // Restore caret position to a valid location
                textBox.SelectionStart = Math.Min(caretIndex, newText.Length);
            }
        }
    }

}
