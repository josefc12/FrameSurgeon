using FrameSurgeon.ViewModels;

namespace FrameSurgeon.Models;

public class UserSettings
{
    public bool OpenFolderAfterMakeEnabled { get; set; }
    public bool OpenLastProjectEnabled { get; set; }

    
    public UserSettings() { }
    public UserSettings(MainWindowViewModel context)
    {
        OpenFolderAfterMakeEnabled = context.OpenFolderAfterMakeEnabled;
        OpenLastProjectEnabled = context.OpenLastProjectEnabled;
    }
    
}