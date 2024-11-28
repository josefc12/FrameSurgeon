using Avalonia.Data;
using Avalonia.Data.Converters;
using ImageMagick;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FrameSurgeon.Services;

public class IntNullableConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Return the value as a string, ensuring null is handled gracefully
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return null; // Return null for empty input
        }

        // Parse the string input into an integer, ignoring invalid characters
        string stringValue = Regex.Replace(value.ToString(), "[^0-9]", "");
        return int.TryParse(stringValue, out int intValue) ? intValue : (object?)null;
    }

}