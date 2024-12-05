using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Linq;
using System;
using System.Windows.Input;

namespace FrameSurgeon.Views;

public partial class NumberBoxComponent : UserControl
{

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<NumberBoxComponent, string>(nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public NumberBoxComponent()
    {
        InitializeComponent();
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