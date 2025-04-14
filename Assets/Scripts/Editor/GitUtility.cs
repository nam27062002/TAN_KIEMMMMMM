#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class GitUtility : EditorWindow
{
    [MenuItem("Git/Quick Push (add, commit, push)")]
    static void QuickPush()
    {
        // Lấy đường dẫn thư mục gốc của project
        string projectPath = Path.GetDirectoryName(Application.dataPath);

        // Hiển thị thông báo đang thực hiện
        EditorUtility.DisplayProgressBar("Git Quick Push", "Running git commands...", 0.1f);

        try
        {
            // 1. Git Add
            EditorUtility.DisplayProgressBar("Git Quick Push", "Running git add .", 0.3f);
            if (!RunGitCommand("add .", projectPath))
            {
                EditorUtility.DisplayDialog("Git Error", "Failed to run 'git add .'. Check console for details.", "OK");
                return;
            }

            // 2. Git Commit
            EditorUtility.DisplayProgressBar("Git Quick Push", "Running git commit...", 0.6f);
            // Sử dụng dấu nháy kép đúng cách cho commit message
            if (!RunGitCommand("commit -m \"update\"", projectPath))
            {
                // Kiểm tra xem có phải lỗi "nothing to commit" không
                string commitOutput = GetGitCommandOutput("status --porcelain", projectPath);
                if (string.IsNullOrWhiteSpace(commitOutput) || commitOutput.Contains("nothing to commit"))
                {
                    UnityEngine.Debug.LogWarning("Git Commit: Nothing to commit, working tree clean. Proceeding to push.");
                }
                else
                {
                    EditorUtility.DisplayDialog("Git Error", "Failed to run 'git commit'. Check console for details.", "OK");
                    return;
                }
            }

            // 3. Git Push
            EditorUtility.DisplayProgressBar("Git Quick Push", "Running git push...", 0.9f);
            if (!RunGitCommand("push", projectPath))
            {
                EditorUtility.DisplayDialog("Git Error", "Failed to run 'git push'. Check console for details.", "OK");
                return;
            }

            // Hoàn tất
            EditorUtility.DisplayDialog("Git Quick Push", "Successfully added, committed, and pushed changes!", "OK");
            UnityEngine.Debug.Log("Git Quick Push successful!");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Git Quick Push failed: {e.Message}");
            EditorUtility.DisplayDialog("Git Error", $"An error occurred: {e.Message}", "OK");
        }
        finally
        {    
            // Xóa thanh tiến trình
            EditorUtility.ClearProgressBar();
        }
    }

    static bool RunGitCommand(string args, string workingDirectory)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo("git", args)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process process = Process.Start(startInfo);
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (!string.IsNullOrEmpty(output)) UnityEngine.Debug.Log($"Git output ({args}):\n{output}");
        if (!string.IsNullOrEmpty(error)) UnityEngine.Debug.LogError($"Git error ({args}):\n{error}");

        return process.ExitCode == 0;
    }
    
    static string GetGitCommandOutput(string args, string workingDirectory)
    {
         ProcessStartInfo startInfo = new ProcessStartInfo("git", args)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process process = Process.Start(startInfo);
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }
}
#endif 