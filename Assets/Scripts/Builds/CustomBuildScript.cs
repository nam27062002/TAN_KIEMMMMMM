#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

public class CustomBuildScript
{
    [MenuItem("Build/Windows")]
    public static void BuildGame()
    {
        // Increment version before building
        IncrementVersion();
        
        // Get current version after incrementing
        string version = PlayerSettings.bundleVersion;
        
        // Build folder path
        string buildFolderName = $"BUILD_{version}";
        string rootPath = Path.GetDirectoryName(Application.dataPath);
        string buildPath = Path.Combine(rootPath, buildFolderName);
        
        // Create build folder if it doesn't exist
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        
        // Set up build settings
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetAllEnabledScenes(),
            locationPathName = Path.Combine(buildPath, PlayerSettings.productName + ".exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        
        // Display progress bar
        EditorUtility.DisplayProgressBar("Building Game", "Building game, please wait...", 0.3f);
        
        try
        {
            // Execute build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            // Check build result
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build successful: {summary.totalSize / 1024 / 1024} MB");
                
                // Clean up unnecessary folders before compressing
                EditorUtility.DisplayProgressBar("Cleaning Build", "Removing unnecessary folders...", 0.6f);
                CleanupBuildFolder(buildPath, PlayerSettings.productName);
                
                // Compress BUILD_{version} folder to ZIP
                EditorUtility.DisplayProgressBar("Zipping Build", "Compressing build to ZIP file...", 0.7f);
                
                string zipPath = Path.Combine(rootPath, $"{buildFolderName}.zip");
                
                // Delete old ZIP file if it exists
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                
                // Compress folder
                ZipFile.CreateFromDirectory(buildPath, zipPath);
                
                Debug.Log($"Successfully compressed: {zipPath}");
                EditorUtility.DisplayProgressBar("Build Completed", "Build and compression completed!", 1.0f);
                
                // Open folder containing ZIP file
                EditorUtility.RevealInFinder(zipPath);
            }
            else
            {
                Debug.LogError($"Build failed: {summary.totalErrors} errors");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during build process: {e.Message}");
        }
        finally
        {
            // Clear progress bar
            EditorUtility.ClearProgressBar();
        }
    }
    
    // Remove unnecessary folders from the build
    private static void CleanupBuildFolder(string buildPath, string productName)
    {
        try
        {
            // Identify and delete backup folder
            string backupFolder = Path.Combine(buildPath, $"{productName}_BackUpThisFolder_ButDontShipItWithYourGame");
            if (Directory.Exists(backupFolder))
            {
                Directory.Delete(backupFolder, true);
                Debug.Log($"Deleted backup folder: {backupFolder}");
            }
            
            // Identify and delete Burst debug information folder
            string burstDebugFolder = Path.Combine(buildPath, $"{productName}_BurstDebugInformation_DoNotShip");
            if (Directory.Exists(burstDebugFolder))
            {
                Directory.Delete(burstDebugFolder, true);
                Debug.Log($"Deleted Burst debug folder: {burstDebugFolder}");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error during cleanup: {e.Message}");
        }
    }
    
    // Get all enabled scenes in build settings
    private static string[] GetAllEnabledScenes()
    {
        return (from scene in EditorBuildSettings.scenes
                where scene.enabled
                select scene.path).ToArray();
    }
    
    // Manually increment version like in AutoIncrementVersion
    private static void IncrementVersion()
    {
        string version = PlayerSettings.bundleVersion;
        string[] parts = version.Split('.');
        if (parts.Length >= 2)
        {
            try
            {
                int lastPart = int.Parse(parts[^1]);
                lastPart++;
                parts[^1] = lastPart.ToString();
                string newVersion = string.Join(".", parts);
                
                PlayerSettings.bundleVersion = newVersion;
                Debug.Log($"Version incremented from {version} to {newVersion}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Cannot increment version: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("Version string has incorrect format. Please set version in 'X.Y' format (e.g.: 0.1).");
        }
    }
}
#endif 