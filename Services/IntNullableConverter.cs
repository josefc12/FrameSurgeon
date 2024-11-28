using Avalonia.Data;
using Avalonia.Data.Converters;
using ImageMagick;
using System;
using System.Diagnostics;
using System.Globalization;

namespace FrameSurgeon.Services;

public class IntNullableConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value; // No conversion needed for displaying
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Try to parse the input as a number, if successful return the value, otherwise return 0 or null
        if (int.TryParse(value?.ToString(), out int result))
        {
            return result;
        }
        return 0; // Or handle invalid input however you like
    }

}