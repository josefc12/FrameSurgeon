using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;
using FrameSurgeon.Enums;
using System.Diagnostics;
using System.Reactive;
using FrameSurgeon.Classes;
using FrameSurgeon.Services;
using FrameSurgeon.Models;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Data.Converters;
using Avalonia.Data;
using System.Globalization;
using ImageMagick;

namespace FrameSurgeon.ViewModels;

public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ReactiveCommand<Unit, Unit> LoadNewImages { get; }
    public ReactiveCommand<string, Unit> RemoveFrame { get; }
    public ReactiveCommand<Unit, Unit> ResetFlipbookResolution { get; }
    public ReactiveCommand<Unit, Unit> ResetFrameSize { get; }
    public ReactiveCommand<Unit, Unit> SetNewOutputPath { get; }
    public ReactiveCommand<Unit, Unit> ProcessMake { get; }
    public ReactiveCommand<Unit, Unit> OpenWaringDialog { get; }

    public Interaction<DialogViewModel, MainWindowViewModel> ShowDialog { get;} = new Interaction<DialogViewModel, MainWindowViewModel>();

    private ObservableCollection<string> _loadedFiles = new ObservableCollection<string>();
    private ExportMode _selectedExportMode;
    private Extension _selectedExtension;
    private List<string> _convertedExtensions = ValueConverter.GetConvertedExtensions(ExportMode.Flipbook);
    private bool _isFlipBookModeSelected = true;
    private bool _isAnimatedGifModeSelected = true;
    private bool _frameNotLoaded = true;
    private bool _transparencyEnabled = false;
    private bool _uniformScalingEnabled = true;
    private int? _flipbookResolutionHorizontal;
    private int? _flipbookResolutionVertical;
    private int? _frameSizeWidth;
    private int? _frameSizeHeight;
    private string _outputPath;
    public List<string> ConvertedExportModes => ValueConverter.GetConvertedExportModes(Enum.GetValues(typeof(ExportMode)).Cast<ExportMode>());
    public List<string> ConvertedExtensions 
    {
        get => _convertedExtensions;
        set 
        {
            if (!_convertedExtensions.SequenceEqual(value))
            {
                _convertedExtensions = value;
                OnPropertyChanged(nameof(ConvertedExtensions));

                // Ensure SelectedExtension is valid and set to the first item in the updated list
                if (_convertedExtensions.Any())
                {
                    SelectedExtension = _convertedExtensions[0];
                }
            }

        } 
    }

    public string SelectedExportMode
    {
        get => ValueConverter.GetExportModeAsString(_selectedExportMode);
        set
        {
            if (_selectedExportMode != ValueConverter.GetExportModeAsEnumValue(value))
            {
                _selectedExportMode = ValueConverter.GetExportModeAsEnumValue(value);
                OnPropertyChanged(nameof(SelectedExportMode));
                IsFlipBookModeSelected = _selectedExportMode == ExportMode.Flipbook || _selectedExportMode == ExportMode.DismantleFlipbook;
                IsAnimatedGifModeSelected = _selectedExportMode == ExportMode.AnimatedGif;
                //Set appropriate export extentions
                ConvertedExtensions = ValueConverter.GetConvertedExtensions(_selectedExportMode);
            }
        }
    }

    public string SelectedExtension
    {
        get => ValueConverter.GetExtensionAsString(_selectedExtension);
        set
        {
            if (_selectedExtension != ValueConverter.GetExtensionAsEnumValue(value))
            {
                _selectedExtension = ValueConverter.GetExtensionAsEnumValue(value);
                OnPropertyChanged(nameof(SelectedExtension));
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

                // Recalculate the dimensions of the flipbook
                if (_isFlipBookModeSelected)
                {
                    Calculator.CalculateFlipbookDimensions(LoadedFiles.Count);
                }

                OnPropertyChanged(nameof(IsFlipBookModeSelected));
            }
        }
    }
    public bool IsAnimatedGifModeSelected
    {
        get => _isAnimatedGifModeSelected;
        set
        {
            if (_isAnimatedGifModeSelected != value)
            {
                _isAnimatedGifModeSelected = value;

                // Recalculate the dimensions of the flipbook
                if (_isAnimatedGifModeSelected)
                {
                    Calculator.CalculateFlipbookDimensions(LoadedFiles.Count);
                }

                OnPropertyChanged(nameof(IsAnimatedGifModeSelected));


            }
        }
    }
    public bool UniformScalingEnabled
    {
        get => _uniformScalingEnabled;
        set
        {
            if (_uniformScalingEnabled != value)
            {
                _uniformScalingEnabled = value;

                // Recalculate the dimensions of the flipbook
                if (_uniformScalingEnabled)
                {
                    Calculator.CalculateFlipbookDimensions(LoadedFiles.Count);
                }

                OnPropertyChanged(nameof(UniformScalingEnabled));
            }
        }
    }
    public bool FrameNotLoaded
    {
        get => _frameNotLoaded;
        set
        {
            if (_frameNotLoaded != value)
            {
                _frameNotLoaded = value;
                OnPropertyChanged(nameof(FrameNotLoaded));
            }
        }
    }
    public bool TransparencyEnabled
    {
        get => _transparencyEnabled;
        set
        {
            if (_transparencyEnabled != value)
            {
                _transparencyEnabled = value;
                OnPropertyChanged(nameof(TransparencyEnabled));
            }
        }
    }
    public ObservableCollection<string> LoadedFiles
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

    public int? FlipbookResolutionHorizontal
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

    public int? FlipbookResolutionVertical
    {
        get => _flipbookResolutionVertical;
        set
        {
            if (_flipbookResolutionVertical != value && int.TryParse(value.ToString(), out int x))
            {
                _flipbookResolutionVertical = value;
                OnPropertyChanged(nameof(FlipbookResolutionVertical));
            }
        }
    }
    public int? FrameSizeWidth
    {
        get => _frameSizeWidth;
        set
        {
            if (_frameSizeWidth != value && int.TryParse(value.ToString(), out int x))
            {
                _frameSizeWidth = value;
                OnPropertyChanged(nameof(FrameSizeWidth));
                if (_uniformScalingEnabled)
                {
                    FrameSizeHeight = value;

                }
            }
        }
    }
    public int? FrameSizeHeight
    {
        get => _frameSizeHeight;
        set
        {
            if (_frameSizeHeight != value && int.TryParse(value.ToString(), out int x))
            {
                _frameSizeHeight = value;
                OnPropertyChanged(nameof(FrameSizeHeight));
                if (_uniformScalingEnabled)
                {
                    FrameSizeWidth = value;

                }
            }
        }
    }
    public string OutputPath
    {
        get => _outputPath;
        set
        {
            if (_outputPath != value)
            {
                _outputPath = value;
                OnPropertyChanged(nameof(OutputPath));
            }
        }
    }

    public MainWindowViewModel()
    {
        LoadNewImages = ReactiveCommand.Create(RunLoadNewImages);
        RemoveFrame = ReactiveCommand.Create<string>(RunRemoveFrame);
        ResetFlipbookResolution = ReactiveCommand.Create(RunResetFlipbookResolution);
        ResetFrameSize = ReactiveCommand.Create(RunResetFrameSize);
        SetNewOutputPath = ReactiveCommand.Create(RunSetNewOutputPath);
        ProcessMake = ReactiveCommand.Create(RunProcessMake);

        OpenWaringDialog = ReactiveCommand.Create(RunOpenWarningDialog);
       

    }
    private async void RunOpenWarningDialog()
    {
        //TODO
        Debug.WriteLine("haha");
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
            LoadedFiles.Clear();
            foreach (var filePath in filePaths)
            {
                LoadedFiles.Add(filePath);
            }

            if (LoadedFiles.Count > 0)
            {
                FrameNotLoaded = false;
            }

        }
        catch (Exception e)
        {
            throw new Exception($"{e.Message}");
        }

        // Calculate new dimensions if Flipbook mode is selected.
        SetFlipbookResolution();
        SetFrameSize();

    }
    private void RunRemoveFrame(string itemPath)
    {
        LoadedFiles.Remove(itemPath);

        if (LoadedFiles.Count == 0)
        {
            FrameNotLoaded = true;
        }
        
        // Recalculate the dimensions of the flipbook
        SetFlipbookResolution();
        SetFrameSize();
    }
    private void RunResetFlipbookResolution()
    {
        SetFlipbookResolution();
    }
    private void RunResetFrameSize()
    {
        SetFrameSize();
    }
    private async void RunSetNewOutputPath()
    {
        // Load output path
        try
        {
            var file = await InputOutput.DoOpenOutputPickerAsync();
            if (file is null) return;
            OutputPath = file.Path.AbsolutePath;
        }
        catch (Exception e)
        {
            throw new Exception($"{e.Message}");
        }
    }
    private async void RunProcessMake()
    {
        GlobalSettings globalSettings = new GlobalSettings()
        {
            ExportMode = ValueConverter.GetExportModeAsEnumValue(SelectedExportMode),
            LoadedFiles = LoadedFiles.ToList(),
            OutputPath = OutputPath,
            SelectedExtension = SelectedExtension,
            TransparencyEnabled = TransparencyEnabled,
            FrameSizeHeight = FrameSizeHeight ?? 0,
            FrameSizeWidth = FrameSizeWidth ?? 0,
        };

        FlipbookSettings flipbookSettings = new FlipbookSettings()
        {
            hResolution = FlipbookResolutionHorizontal ?? 0,
            vResolution = FlipbookResolutionVertical ?? 0,
        };

        Processor processor = new Processor();
        ProcessResult result = await processor.Process(globalSettings, flipbookSettings);
        if (result.Result == Result.Failure)
        {
            // Show error message
            var warning = new DialogViewModel(DialogType.Warning, result.Message);
            await ShowDialog.Handle(warning);

        }
        else
        {
            // Show success message
            var success = new DialogViewModel(DialogType.Success, result.Message);
            await ShowDialog.Handle(success);
        }
       
    }

    private void SetFlipbookResolution()
    {
        FlipbookResolution fResolution = Calculator.CalculateFlipbookDimensions(LoadedFiles.Count);
        FlipbookResolutionHorizontal = fResolution.HorizontalAmount;
        FlipbookResolutionVertical = fResolution.VerticalAmount;
    }

    private void SetFrameSize()
    {
        if (LoadedFiles.Count() <= 0)
        { return; }
        //First "leading" frame
        MagickImage firstImage = new MagickImage(LoadedFiles[0]);

        FrameSizeWidth = (int)firstImage.Width;
        FrameSizeHeight = (int?)firstImage.Height;

        firstImage.Dispose();

    }

}
