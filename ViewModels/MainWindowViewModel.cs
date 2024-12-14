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
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Runtime;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.IO;
using Microsoft.VisualBasic;
using Avalonia;
using Avalonia.Media;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using DynamicData;

namespace FrameSurgeon.ViewModels;

public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ReactiveCommand<bool, Unit> LoadNewImages { get; }
    public ReactiveCommand<string, Unit> RemoveFrame { get; }
    public ReactiveCommand<Unit, Unit> ResetFlipbookResolution { get; }
    public ReactiveCommand<Unit, Unit> ResetFrameSize { get; }
    public ReactiveCommand<Unit, Unit> ResetGifFps { get; }
    public ReactiveCommand<Unit, Unit> SetNewOutputPath { get; }
    public ReactiveCommand<Unit, Unit> ProcessMake { get; }
    public ReactiveCommand<Unit, Unit> OpenSettings { get; }
    public ReactiveCommand<Unit, Unit> PreviewResult { get; }
    public ReactiveCommand<Unit, Unit> OpenProject { get; }
    public ReactiveCommand<Unit, Unit> SaveAsProject { get; }
    public ReactiveCommand<Unit, Unit> SaveProject { get; }

    public Interaction<DialogViewModel, MainWindowViewModel> ShowDialog { get;} = new Interaction<DialogViewModel, MainWindowViewModel>();
    public Interaction<MainWindowViewModel, MainWindowViewModel> ShowSettings { get; } = new Interaction<MainWindowViewModel, MainWindowViewModel>();

    private readonly App _app;
    private ObservableCollection<string> _loadedFiles = new ObservableCollection<string>();
    private ObservableCollection<FrameRow> _loadedFilesNames = new ObservableCollection<FrameRow>();
    private ExportMode _selectedExportMode;
    private List<string> _convertedExtensions = ValueConverter.GetConvertedExtensions(ExportMode.Flipbook);
    private bool _isFlipBookModeSelected = true;
    private bool _isAnimatedGifModeSelected = false;
    private bool _frameNotLoaded = true;
    private bool _transparencyEnabled = false;
    private bool _skipFramesEnabled = false;
    private bool _annotateFramesEnabled = false;
    private bool _uniformScalingEnabled = true;
    private bool _openLastProjectEnabled = false;
    private bool _openFolderAfterMakeEnabled = false;
    private bool _gifLooping = true;
    private bool _isProcessing = false;
    private int? _flipbookResolutionHorizontal;
    private int? _flipbookResolutionVertical;
    private int? _frameSizeWidth;
    private int? _frameSizeHeight;
    private int? _gifFps = 30;
    private int? _maxProgress = 100;
    private double? _currentProgress = 0;
    private string _outputPath;
    private string _savePath;

    public bool isAdding { get; } = false;

    public ToolTipInformation ToolTips { get; } = new ToolTipInformation();

    public string SelectedExtension { get; set;}
    public List<string> ConvertedExportModes => ValueConverter.GetConvertedExportModes(Enum.GetValues(typeof(ExportMode)).Cast<ExportMode>());
    public List<string> ConvertedExtensions 
    {
        get => _convertedExtensions;
        set 
        {
            if (_convertedExtensions.SequenceEqual(value) != true)
            {
                _convertedExtensions = value;
                OnPropertyChanged(nameof(ConvertedExtensions));
                SelectedExtension = _convertedExtensions[0];
                OnPropertyChanged(nameof(SelectedExtension));
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
                OnPropertyChanged(nameof(UniformScalingEnabled));
            }
        }
    }

    public bool OpenLastProjectEnabled
    {
        get => _openLastProjectEnabled;
        set
        {
            if (_openLastProjectEnabled != value)
            {
                _openLastProjectEnabled = value;
                OnPropertyChanged(nameof(OpenLastProjectEnabled));
            }
        }
    }

    public bool OpenFolderAfterMakeEnabled
    {
        get => _openFolderAfterMakeEnabled;
        set
        {
            if (_openFolderAfterMakeEnabled != value)
            {
                _openFolderAfterMakeEnabled = value;
                OnPropertyChanged(nameof(OpenFolderAfterMakeEnabled));
            }
        }
    }

    public bool GifLooping
    {
        get => _gifLooping;
        set
        {
            if (_gifLooping != value)
            {
                _gifLooping = value;
                OnPropertyChanged(nameof(GifLooping));
            }
        }
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            if (_isProcessing != value)
            {
                _isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
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
    public bool SkipFramesEnabled
    {
        get => _skipFramesEnabled;
        set
        {
            if (_skipFramesEnabled != value)
            {
                _skipFramesEnabled = value;
                OnPropertyChanged(nameof(SkipFramesEnabled));

                SetFlipbookResolution(SkipFramesEnabled);

            }
        }
    }
    public bool AnnotateFramesEnabled
    {
        get => _annotateFramesEnabled;
        set
        {
            if (_annotateFramesEnabled != value)
            {
                _annotateFramesEnabled = value;
                OnPropertyChanged(nameof(AnnotateFramesEnabled));
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
    public ObservableCollection<FrameRow> LoadedFilesNames
    {
        get => _loadedFilesNames;
        set
        {
            if (_loadedFilesNames != value)
            {
                _loadedFilesNames = value;
                OnPropertyChanged(nameof(LoadedFilesNames));
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
    public int? GifFps
    {
        get => _gifFps;
        set
        {
            if (_gifFps != value && int.TryParse(value.ToString(), out int x))
            {
                _gifFps = value;
                OnPropertyChanged(nameof(GifFps));
            }
        }
    }

    public int? MaxProgress
    {
        get => _maxProgress;
        set
        {
            if (_maxProgress != value)
            {
                _maxProgress = value;
                OnPropertyChanged(nameof(MaxProgress));
            }
        }
    }
    public double? CurrentProgress
    {
        get => _currentProgress;
        set
        {
            if (_currentProgress != value)
            {
                _currentProgress = value;
                OnPropertyChanged(nameof(CurrentProgress));
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
    public string SavePath
    {
        get => _savePath;
        set
        {
            if (_savePath != value)
            {
                _savePath = value;
                OnPropertyChanged(nameof(SavePath));
            }
        }
    }

    public MainWindowViewModel(App app)
    {
        
        //LoadUser Settings
        var userSettings = Saviour.LoadUserSettings();
        OpenFolderAfterMakeEnabled = userSettings.OpenFolderAfterMakeEnabled;
        OpenLastProjectEnabled = userSettings.OpenLastProjectEnabled;
        
        //Load last project settings:
        if (OpenLastProjectEnabled)
        {
            var lastProjectSettings = Saviour.LoadProjectStartupFile();
            
            LoadSettings(lastProjectSettings);
        }
        _app = app;
        SelectedExtension = _convertedExtensions[0];
        
        LoadNewImages = ReactiveCommand.Create<bool>(RunLoadNewImages);
        RemoveFrame = ReactiveCommand.Create<string>(RunRemoveFrame);
        ResetFlipbookResolution = ReactiveCommand.Create(RunResetFlipbookResolution);
        ResetFrameSize = ReactiveCommand.Create(RunResetFrameSize);
        ResetGifFps = ReactiveCommand.Create(RunResetGifFps);
        SetNewOutputPath = ReactiveCommand.Create(RunSetNewOutputPath);
        ProcessMake = ReactiveCommand.CreateFromTask(RunProcessMake);
        PreviewResult = ReactiveCommand.CreateFromTask(RunPreviewResult);
        OpenProject = ReactiveCommand.CreateFromTask(RunOpenProject);
        SaveAsProject = ReactiveCommand.CreateFromTask(RunSaveAsProject);
        SaveProject = ReactiveCommand.CreateFromTask(RunSaveProject);
        OpenSettings = ReactiveCommand.Create(RunOpenSettings);
       

    }
    private async void RunOpenSettings()
    {
        var progressTask = await ShowSettings.Handle(this);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // COMMAND FUNCTIONS
    private async void RunLoadNewImages(bool isAdding)
    {
        
        // Load paths
        try
        {
            var filePaths = await InputOutput.DoOpenFilePickerAsync();
            if (!filePaths.Any()) return;
            if (!isAdding)
            {
                LoadedFiles.Clear();
                LoadedFilesNames.Clear();
                
            }
            foreach (var filePath in filePaths)
            {
                LoadedFiles.Add(filePath);
            }

            foreach (var filePath in filePaths)
            {
                LoadedFilesNames.Add(new FrameRow(Path.GetFileName(filePath), filePath));
            }

            if (filePaths.Count() > 0)
            {
                FrameNotLoaded = false;
            }

            foreach (var aFile in LoadedFiles)
            {
                
            }
            foreach (var aFile in LoadedFilesNames)
            {
                
            }

        }
        catch (Exception e)
        {
            throw new Exception($"{e.Message}");
        }

        // Calculate new dimensions if Flipbook mode is selected.
        SetFlipbookResolution(SkipFramesEnabled);
        SetFrameSize();

    }
    private void RunRemoveFrame(string itemPath)
    {

        var itemToRemove = LoadedFilesNames.FirstOrDefault(f => f.Name == Path.GetFileName(itemPath));
        if (itemToRemove != null)
        {
            LoadedFiles.Remove(itemToRemove.AbsolutePath);
            LoadedFilesNames.Remove(itemToRemove);
        }

        if (LoadedFiles.Count == 0)
        {
            FrameNotLoaded = true;
        }

        // Recalculate the dimensions of the flipbook
        SetFlipbookResolution(SkipFramesEnabled);
        SetFrameSize();

    }
    private void RunResetFlipbookResolution()
    {
        SetFlipbookResolution(SkipFramesEnabled);
    }
    private void RunResetFrameSize()
    {
        SetFrameSize();
    }
    private void RunResetGifFps()
    {
        GifFps = 30;
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
    private async Task RunProcessMake()
    {

        ProcessResult? result = null;

        try
        {
            await Task.Run(() =>
            {

                Dispatcher.UIThread.Post(() => IsProcessing = true);
                
                Processor processor = new Processor(this);
                result = processor.Process();

                Dispatcher.UIThread.Post(() => IsProcessing = false);

            });

            Dispatcher.UIThread.Post(() => { CurrentProgress = 0; });

            if (result != null && result.Result == Result.Failure)
            {
                // Show error message
                var warning = new DialogViewModel(DialogType.Warning, result.Message);
                await ShowDialog.Handle(warning);
            }
            else if (result != null)
            {
                // Show success message
                var success = new DialogViewModel(DialogType.Success, result.Message);
                await ShowDialog.Handle(success);
            }
            else
            {
                var failure = new DialogViewModel(DialogType.Error, "Fatal error");
                await ShowDialog.Handle(failure);

            }

            if (OpenFolderAfterMakeEnabled)
            {
                // Opens the destination folder if the setting's enabled.
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows: Use Explorer
                    Process.Start("explorer.exe", Path.GetDirectoryName(OutputPath));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS: Use Finder
                    Process.Start("open", Path.GetDirectoryName(OutputPath));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Linux: Use xdg-open (common on many distros)
                    Process.Start("xdg-open", Path.GetDirectoryName(OutputPath));
                }
                else
                {
                    throw new PlatformNotSupportedException("Unsupported operating system.");
                }
            }
        }
        catch (Exception ex)
        {

        }
        finally
        {
            result?.Image?.Dispose();
            result?.Collection?.Dispose();
        }

    }

    private async Task RunPreviewResult()
    {


        Processor processor = new Processor(this);
        
        var previewResult = processor.Preview();

        if (previewResult != null && previewResult.Result == Result.Failure)
        {
            // Show error message
            var warning = new DialogViewModel(DialogType.Warning, previewResult.Message);
            await ShowDialog.Handle(warning);
        }
        else if (previewResult != null)
        {
            // Display the resulted preview

            if (previewResult.Image != null)
            {
                await DisplayImage(previewResult.Image);
            }
            else if (previewResult.Collection != null)
            {
                try
                {
                    await DisplayAnimatedGif(previewResult.Collection);

                }
                catch (Exception ex)
                {
                    var failure = new DialogViewModel(DialogType.Error, ex.Message);
                    await ShowDialog.Handle(failure);
                }

            }
            else
            {
                var failure = new DialogViewModel(DialogType.Error, "Fatal error");
                await ShowDialog.Handle(failure);
            }

        }
        else
        {
            var failure = new DialogViewModel(DialogType.Error, "Fatal error");
            await ShowDialog.Handle(failure);

        }

    }
    
    private async Task RunOpenProject()
    {
        // Load paths
        try
        {
            ProjectSettings? loadedSettings = await InputOutput.DoOpenProjectAsync();
            if (loadedSettings != null)
            {
                
                LoadSettings(loadedSettings);
                Console.WriteLine(SavePath);
            }

        }
        catch (Exception e)
        {
            throw new Exception($"{e.Message}");
        }
    }
    
    private async Task RunSaveAsProject()
    {
        // Load paths
        try
        {
            var savePath = await InputOutput.DoOpenSaveFileAsync();
            if (savePath == "") return;
            
            Console.WriteLine(SavePath);
            SavePath = savePath;
            ProjectSettings projectSettings = new ProjectSettings(this);
            Saviour.SaveAsProject(SavePath, projectSettings);

        }
        catch (Exception e)
        {
            throw new Exception($"{e.Message}");
        }
    }

    private async Task RunSaveProject()
    {
        if (SavePath == "")
        {
            await RunSaveAsProject();
        }
        else
        {
            Saviour.SaveAsProject(SavePath, new ProjectSettings(this));
        }
    }

    private void SetFlipbookResolution(bool isHalved)
    {

        int frameCount = LoadedFiles.Count;
        if (isHalved)
        {
            frameCount = frameCount / 2;
        }

        FlipbookResolution fResolution = Calculator.CalculateFlipbookDimensions(frameCount);
        FlipbookResolutionHorizontal = fResolution.HorizontalAmount;
        FlipbookResolutionVertical = fResolution.VerticalAmount;
    }

    private void SetFrameSize()
    {
        if (LoadedFiles.Count() <= 0)
        { return; }
        //First "leading" frame

        // Check if the file still exists
        if (!System.IO.File.Exists(LoadedFiles[0]))
        {
            return;
        }

        MagickImage firstImage = new MagickImage(LoadedFiles[0]);

        FrameSizeWidth = (int)firstImage.Width;
        FrameSizeHeight = (int?)firstImage.Height;

        firstImage.Dispose();

    }

    private async Task DisplayAnimatedGif(MagickImageCollection collection)
    {
        var frames = collection.Select(frame =>
        {
            using (var memoryStream = new MemoryStream())
            {
                frame.Write(memoryStream, MagickFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return new Avalonia.Media.Imaging.Bitmap(memoryStream);
            }
        }).ToList();

        if (frames.Count == 0)
        {
            var failure = new DialogViewModel(DialogType.Error, "No frames found in GIF.");
            await ShowDialog.Handle(failure);
            return;
        }

        var imageControl = new Avalonia.Controls.Image
        {
            Stretch = Avalonia.Media.Stretch.Uniform
        };

        var window = new Window
        {
            Title = "Animated GIF Preview",
            Width = frames[0].PixelSize.Width + 200,
            Height = frames[0].PixelSize.Height + 40,
            Content = new Border
            {
                Padding = new Thickness(20),
                Background = Brushes.White,
                Child = imageControl
            }
        };

        window.Closed += (s, e) =>
        {
            collection.Dispose();
            foreach (var frame in frames)
            {
                frame.Dispose();
            }

            _app.PurgePreviewWindow(window);
        };

        _app.RegisterPreviewWindow(window);

        window.Show();
        
        // Animate the GIF frames
        _ = Task.Run(async () =>
        {
            while (true)
            {
                foreach (var frame in frames)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        imageControl.Source = frame;
                    });

                    // Use frame delay from the GIF metadata
                    var delay = collection[frames.IndexOf(frame)].AnimationDelay * 10; // Convert to milliseconds
                    await Task.Delay((int)(delay > 0 ? delay : 100)); // Default to 100ms if no delay is specified
                }
            }
        });
        
    }
    private Task DisplayImage(MagickImage image)
    {
        var memoryStream = new MemoryStream();
        image.Write(memoryStream, MagickFormat.Png);
        memoryStream.Seek(0, SeekOrigin.Begin);

        var avaloniaBitmap = new Bitmap(memoryStream);

        var window = new Window
        {
            Title = "Image Preview",
            Width = avaloniaBitmap.PixelSize.Width +200,
            Height = avaloniaBitmap.PixelSize.Height +40,
            Content = new Border
            {
                Padding = new Thickness(20),
                Background = Brushes.White,
                Child = new Image
                {
                    Source = avaloniaBitmap,
                    Stretch = Stretch.Uniform
                }
            }
        };

        window.Closed += (s, e) =>
        {
            avaloniaBitmap.Dispose();
            memoryStream.Dispose();
            _app.PurgePreviewWindow(window);
        };

        _app.RegisterPreviewWindow(window);

        window.Show();

        return Task.CompletedTask;
    }

    private void LoadSettings(ProjectSettings settings)
    {
        // This could be further optimized.
        LoadedFiles = settings.LoadedFiles;
        LoadedFilesNames = settings.LoadedFilesNames;
        SelectedExportMode = settings.SelectedExportMode;
        ConvertedExtensions = settings.ConvertedExtensions;
        IsFlipBookModeSelected = settings.IsFlipBookModeSelected;
        IsAnimatedGifModeSelected = settings.IsAnimatedGifModeSelected;
        FrameNotLoaded = settings.FrameNotLoaded;
        TransparencyEnabled = settings.TransparencyEnabled;
        SkipFramesEnabled = settings.SkipFramesEnabled;
        AnnotateFramesEnabled = settings.AnnotateFramesEnabled;
        UniformScalingEnabled = settings.UniformScalingEnabled;
        OpenLastProjectEnabled = settings.OpenLastProjectEnabled;
        OpenFolderAfterMakeEnabled = settings.OpenFolderAfterMakeEnabled;
        GifLooping = settings.GifLooping;
        IsProcessing = settings.IsProcessing;
        FlipbookResolutionHorizontal = settings.FlipbookResolutionHorizontal;
        FlipbookResolutionVertical = settings.FlipbookResolutionVertical;
        FrameSizeWidth = settings.FrameSizeWidth;
        FrameSizeHeight = settings.FrameSizeHeight;
        GifFps = settings.GifFps;
        MaxProgress = settings.MaxProgress;
        CurrentProgress = settings.CurrentProgress;
        OutputPath = settings.OutputPath;
        SavePath = settings.SavePath;  
    }
}
