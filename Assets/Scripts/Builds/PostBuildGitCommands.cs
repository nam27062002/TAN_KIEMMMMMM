// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEngine;
// using System.Diagnostics;
// using UnityEditor.Build.Reporting;
// using Debug = UnityEngine.Debug;
//
// public class PostBuildGitCommands : MonoBehaviour
// {
//     [MenuItem("Build/Build Web and Push to Git")]
//     public static void BuildAndPush()
//     {
//         Debug.Log("Starting build...");
//         if (BuildWebGL())
//         {
//             Debug.Log("Build completed successfully.");
//             string gitPath = "D:/My project/Unity/TANN KIEMM/BUILD_WEB";
//             Debug.Log("Executing Git commands...");
//             if (ExecuteGitCommands(gitPath))
//             {
//                 Debug.Log("Git commands executed successfully.");
//                 Debug.Log("Opening webpage...");
//                 OpenWebpage();
//             }
//             else
//             {
//                 Debug.LogError("Failed to execute Git commands.");
//             }
//         }
//         else
//         {
//             Debug.LogError("Build failed.");
//         }
//     }
//
//     private static bool BuildWebGL()
//     {
//         BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
//         buildPlayerOptions.scenes = new[]
//         {
//             "Assets/_Game/Scenes/Bootstrap.unity",
//             "Assets/_Game/Scenes/GameManager.unity",
//             "Assets/_Game/Scenes/UIManager.unity",
//             "Assets/_Game/Scenes/MainMenu.unity",
//             "Assets/_Game/Scenes/Loading.unity",
//             "Assets/_Game/Scenes/Game.unity",
//         };
//         buildPlayerOptions.locationPathName = "D:/My project/Unity/TANN KIEMM/BUILD_WEB";
//         buildPlayerOptions.target = BuildTarget.WebGL;
//         buildPlayerOptions.options = BuildOptions.None;
//
//         BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
//         return report.summary.result == BuildResult.Succeeded;
//     }
//
//     private static bool ExecuteGitCommands(string gitPath)
//     {
//         if (!IsGitRepository(gitPath))
//         {
//             Debug.LogError("The specified path is not a Git repository.");
//             return false;
//         }
//
//         ProcessStartInfo startInfo = new ProcessStartInfo();
//         startInfo.FileName = "cmd.exe";
//         startInfo.WorkingDirectory = gitPath;
//         startInfo.Arguments = "/c git add . && git commit -m \"update\" && git push";
//         startInfo.RedirectStandardOutput = true;
//         startInfo.RedirectStandardError = true;
//         startInfo.UseShellExecute = false;
//
//         Process process = new Process();
//         process.StartInfo = startInfo;
//         process.Start();
//         process.WaitForExit();
//
//         string output = process.StandardOutput.ReadToEnd();
//         string error = process.StandardError.ReadToEnd();
//
//         if (process.ExitCode != 0)
//         {
//             Debug.LogError("Git commands failed with exit code " + process.ExitCode + "\n" + error);
//             return false;
//         }
//
//         Debug.Log("Git commands executed successfully:\n" + output);
//         return true;
//     }
//
//     private static bool IsGitRepository(string path)
//     {
//         return System.IO.Directory.Exists(System.IO.Path.Combine(path, ".git"));
//     }
//
//     private static void OpenWebpage()
//     {
//         Application.OpenURL("https://nam27062002.github.io/tan_kiem_test/");
//     }
// }
// #endif