using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using FrameSurgeon.Enums;


namespace FrameSurgeon.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{

    private ExportMode _selectedExportMode;
    
    public string SelectedExportMode
    {
        get => GetExportModeAsString(_selectedExportMode);
        set
        {
            if (_selectedExportMode != GetExportModeAsEnumValue(value))
            {
                _selectedExportMode = GetExportModeAsEnumValue(value);
                OnPropertyChanged(nameof(SelectedExportMode));
                IsFlipBookModeSelected = _selectedExportMode == ExportMode.FlipBook;
            }
        }
    }

    private ExportMode GetExportModeAsEnumValue(string exportMode)
    {
        ExportMode mode = exportMode switch
        {
            "Flip book" => ExportMode.FlipBook ,
            "Individual Frames" => ExportMode.IndividualFrames ,
            "Animated GIF" => ExportMode.AnimatedGif
        };
        return mode;
    }
    
    private string GetExportModeAsString(ExportMode exportMode)
    {
        string mode = exportMode switch
        {
            ExportMode.FlipBook => "Flip book",
            ExportMode.IndividualFrames => "Individual Frames",
            ExportMode.AnimatedGif => "Animated GIF",
            _ => exportMode.ToString()
        };
        return mode;
    }

    public List<string> ConvertedExportModes => GetConvertedExportModes(Enum.GetValues(typeof(ExportMode)).Cast<ExportMode>());

    private List<string> GetConvertedExportModes(IEnumerable<ExportMode> exportModes)
    {
        var convertedModes = new List<string>();
        
        foreach (ExportMode mode in exportModes)
        {
            string displayName = mode switch
            {
                ExportMode.FlipBook => "Flip book",
                ExportMode.IndividualFrames => "Individual Frames",
                ExportMode.AnimatedGif => "Animated GIF",
                _ => mode.ToString()
            };
            convertedModes.Add(displayName);
        }
        return convertedModes;
    }
    
    private bool _isFlipBookModeSelected = true;
    public bool IsFlipBookModeSelected
    {
        get => _isFlipBookModeSelected;
        set
        {
            if (_isFlipBookModeSelected != value)
            {
                _isFlipBookModeSelected = value;
                OnPropertyChanged(nameof(IsFlipBookModeSelected));
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
}
