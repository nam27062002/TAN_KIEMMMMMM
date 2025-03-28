#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
public class PostBuildGitCommands : MonoBehaviour
{
    [MenuItem("Build/Build Web and Push to Git")]
    public static void BuildAndPush()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[]
        {
            "Assets/_Game/Scenes/Bootstrap.unity",
            "Assets/_Game/Scenes/Game.unity",
            "Assets/_Game/Scenes/GameManager.unity",
            "Assets/_Game/Scenes/MainMenu.unity",
            "Assets/_Game/Scenes/Loading.unity",
        }; 
        
        buildPlayerOptions.locationPathName = $"{Application.dataPath}/BUILD_WEB";
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        ExecuteGitCommands();
    }

    private static void ExecuteGitCommands()
    {
        string gitPath = $"{Application.dataPath}/BUILD_WEB";

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "cmd.exe";
        startInfo.WorkingDirectory = gitPath; 
        startInfo.Arguments = "/c git add . && git commit -m \"update\" && git push";

        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }
}
#endif
