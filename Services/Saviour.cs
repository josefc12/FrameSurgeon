using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using FrameSurgeon.Models;

namespace FrameSurgeon.Services;

public static class Saviour
{
    
    // THIS CAN BE DRIED
    
    public static void SaveUserSettings(UserSettings userSettings)
    {
        
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        string appLocalFolder = System.IO.Path.Combine(localAppData, "FrameSurgeon");
        
        if (!System.IO.Directory.Exists(appLocalFolder))
        {
            System.IO.Directory.CreateDirectory(appLocalFolder);
        }
        
        string filePath = System.IO.Path.Combine(appLocalFolder, "UserSettings.json");
        
        string jsonFile = JsonSerializer.Serialize(userSettings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, jsonFile);
    }
    
    public static void SaveProjectStartupFile(ProjectSettings projectSettings)
    {
        
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        string appLocalFolder = System.IO.Path.Combine(localAppData, "FrameSurgeon");
        
        if (!System.IO.Directory.Exists(appLocalFolder))
        {
            System.IO.Directory.CreateDirectory(appLocalFolder);
        }
        
        string filePath = System.IO.Path.Combine(appLocalFolder, "StartupProjectSettings.json");
        
        string jsonFile = JsonSerializer.Serialize(projectSettings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, jsonFile);
    }
    
    public static void SaveAsProject(string path,ProjectSettings projectSettings)
    {
        
        string jsonFile = JsonSerializer.Serialize(projectSettings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, jsonFile);
    }
    
    public static UserSettings LoadUserSettings()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        string appLocalFolder = System.IO.Path.Combine(localAppData, "FrameSurgeon");
        string filePath = System.IO.Path.Combine(appLocalFolder, "UserSettings.json");

        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Read the JSON data from the file
            string jsonFile = File.ReadAllText(filePath);
            
            // Deserialize the JSON data to a UserSettings object
            UserSettings userSettings = JsonSerializer.Deserialize<UserSettings>(jsonFile);
            
            if (userSettings != null)
            {
                return userSettings;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
    
    public static ProjectSettings LoadProjectStartupFile()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        string appLocalFolder = System.IO.Path.Combine(localAppData, "FrameSurgeon");
        string filePath = System.IO.Path.Combine(appLocalFolder, "StartupProjectSettings.json");

        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Read the JSON data from the file
            string jsonFile = File.ReadAllText(filePath);
            
            // Deserialize the JSON data to a UserSettings object
            ProjectSettings projectSettings = JsonSerializer.Deserialize<ProjectSettings>(jsonFile);
            
            if (projectSettings != null)
            {
                return projectSettings;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    public static ProjectSettings LoadProjectFile(string path)
    {

        // Check if the file exists
        if (File.Exists(path))
        {
            // Read the JSON data from the file
            string jsonFile = File.ReadAllText(path);
            
            // Deserialize the JSON data to a UserSettings object
            ProjectSettings projectSettings = JsonSerializer.Deserialize<ProjectSettings>(jsonFile);
            
            if (projectSettings != null)
            {
                return projectSettings;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
}