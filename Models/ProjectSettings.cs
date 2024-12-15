using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using FrameSurgeon.Classes;
using FrameSurgeon.Enums;
using FrameSurgeon.ViewModels;

namespace FrameSurgeon.Models;

public class ProjectSettings
{
    public ObservableCollection<string> LoadedFiles {get;set;}
    public ObservableCollection<FrameRow> LoadedFilesNames {get;set;}
    public string SelectedExportMode {get;set;}
    public List<string> ConvertedExtensions {get;set;}
    public bool IsFlipBookModeSelected {get;set;}
    public bool IsAnimatedGifModeSelected {get;set;} 
    public bool FrameNotLoaded {get;set;}
    public bool TransparencyEnabled {get;set;}
    public bool SkipFramesEnabled {get;set;}
    public bool AnnotateFramesEnabled {get;set;}
    public bool UniformScalingEnabled {get;set;}
    public bool OpenLastProjectEnabled {get;set;}
    public bool OpenFolderAfterMakeEnabled {get;set;}
    public bool GifLooping {get;set;}
    public bool IsProcessing {get;set;}
    public int? FlipbookResolutionHorizontal {get;set;}
    public int? FlipbookResolutionVertical {get;set;}
    public int? FrameSizeWidth {get;set;}
    public int? FrameSizeHeight {get;set;}
    public int? GifFps {get;set;}
    public int? MaxProgress {get;set;}
    public double? CurrentProgress {get;set;}
    public string OutputPath {get;set;}
    public string SavePath {get;set;}
    public string Title {get;set;}

    public ProjectSettings()
    {
    }

    public ProjectSettings(MainWindowViewModel context)
    {
        LoadedFiles = context.LoadedFiles;
        LoadedFilesNames = context.LoadedFilesNames;
        SelectedExportMode = context.SelectedExportMode;
        ConvertedExtensions = context.ConvertedExtensions;
        IsFlipBookModeSelected = context.IsFlipBookModeSelected;
        IsAnimatedGifModeSelected = context.IsAnimatedGifModeSelected;
        FrameNotLoaded = context.FrameNotLoaded;
        TransparencyEnabled = context.TransparencyEnabled;
        SkipFramesEnabled = context.SkipFramesEnabled;
        AnnotateFramesEnabled = context.AnnotateFramesEnabled;
        UniformScalingEnabled = context.UniformScalingEnabled;
        OpenLastProjectEnabled = context.OpenLastProjectEnabled;
        OpenFolderAfterMakeEnabled = context.OpenFolderAfterMakeEnabled;
        GifLooping = context.GifLooping;
        IsProcessing = context.IsProcessing;
        FlipbookResolutionHorizontal = context.FlipbookResolutionHorizontal;
        FlipbookResolutionVertical = context.FlipbookResolutionVertical;
        FrameSizeWidth = context.FrameSizeWidth;
        FrameSizeHeight = context.FrameSizeHeight;
        GifFps = context.GifFps;
        MaxProgress = context.MaxProgress;
        CurrentProgress = context.CurrentProgress;
        OutputPath = context.OutputPath;
        SavePath = context.SavePath;
        Title = Path.GetFileName(SavePath);
    }
    
}