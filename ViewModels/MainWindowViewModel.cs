using System;
using System.ComponentModel;
using framesurgeon_csharp.Enums;
using System.Collections.Generic;
using System.Linq;


namespace framesurgeon_csharp.ViewModels;

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
                IsFlipbookModeSelected = _selectedExportMode == ExportMode.Flipbook;
            }
        }
    }

    private ExportMode GetExportModeAsEnumValue(string exportMode)
    {
        ExportMode mode = exportMode switch
        {
            "Flipbook" => ExportMode.Flipbook ,
            "Individual Frames" => ExportMode.IndividualFrames ,
            "Animated GIF" => ExportMode.AnimatedGif
        };
        return mode;
    }
    
    private string GetExportModeAsString(ExportMode exportMode)
    {
        string mode = exportMode switch
        {
            ExportMode.Flipbook => "Flipbook",
            ExportMode.IndividualFrames => "Individual Frames",
            ExportMode.AnimatedGif => "Animated GIF" 
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
                ExportMode.Flipbook => "Flipbook",
                ExportMode.IndividualFrames => "Individual Frames",
                ExportMode.AnimatedGif => "Animated GIF",
                _ => mode.ToString()
            };
            convertedModes.Add(displayName);
        }
        return convertedModes;
    }
    
    private bool _isFlipbookModeSelected = true;
    public bool IsFlipbookModeSelected
    {
        get => _isFlipbookModeSelected;
        set
        {
            if (_isFlipbookModeSelected != value)
            {
                _isFlipbookModeSelected = value;
                OnPropertyChanged(nameof(IsFlipbookModeSelected));
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
}
