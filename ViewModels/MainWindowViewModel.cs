using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using System.Text;
using FrameSurgeon.Enums;
using System.Windows.Input;
using System.Diagnostics;
using System.Reactive;
using FrameSurgeon.Classes;
using System.Runtime.CompilerServices;
using Avalonia.Interactivity;
using System.IO;
using FrameSurgeon.Services;
using FrameSurgeon.Models;

namespace FrameSurgeon.ViewModels;

public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ReactiveCommand<Unit, Unit> LoadNewImages { get; }

    private List<string> _loadedFiles = new List<string>();
    private ExportMode _selectedExportMode;
    private bool _isFlipBookModeSelected = true;
    private int _flipbookResolutionHorizontal;
    private int _flipbookResolutionVertical;
    public List<string> ConvertedExportModes => ValueConverter.GetConvertedExportModes(Enum.GetValues(typeof(ExportMode)).Cast<ExportMode>());

    public string SelectedExportMode
    {
        get => ValueConverter.GetExportModeAsString(_selectedExportMode);
        set
        {
            if (_selectedExportMode != ValueConverter.GetExportModeAsEnumValue(value))
            {
                _selectedExportMode = ValueConverter.GetExportModeAsEnumValue(value);
                OnPropertyChanged(nameof(SelectedExportMode));
                IsFlipBookModeSelected = _selectedExportMode == ExportMode.FlipBook;
            }
        }
    }
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
    public List<string> LoadedFiles
    {
        get => _loadedFiles;
        set
        {
            if (_loadedFiles != value)
            {
                _loadedFiles = value;
                OnPropertyChanged(nameof(LoadedFiles));
            }
        }
    }

    public int FlipbookResolutionHorizontal
    {
        get => _flipbookResolutionHorizontal;
        set
        {
            if (_flipbookResolutionHorizontal != value)
            {
                _flipbookResolutionHorizontal = value;
                OnPropertyChanged(nameof(FlipbookResolutionHorizontal));
            }
        }
    }

    public int FlipbookResolutionVertical
    {
        get => _flipbookResolutionVertical;
        set
        {
            if (_flipbookResolutionVertical != value)
            {
                _flipbookResolutionVertical = value;
                OnPropertyChanged(nameof(FlipbookResolutionVertical));
            }
        }
    }


    public MainWindowViewModel()
    {
        LoadedFiles.Add("No frames loaded.");
        LoadNewImages = ReactiveCommand.Create(RunLoadNewImages);
    }
   
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // COMMAND FUNCTIONS
    private async void RunLoadNewImages()
    {
        // Load paths
        try
        {
            var filePaths = await InputOutput.DoOpenFilePickerAsync();
            if (!filePaths.Any()) return;
            LoadedFiles = filePaths.ToList();
        }
        catch (Exception e)
        {
            throw new Exception($"{e.Message}");
        }

        // Calculate new dimensions if Flipbook mode is selected.
        FlipbookResolution fResolution = Calculator.CalculateFlipbookDimensions(LoadedFiles.Count);
        FlipbookResolutionHorizontal = fResolution.HorizontalAmount;
        FlipbookResolutionVertical = fResolution.VerticalAmount;

    }

}
