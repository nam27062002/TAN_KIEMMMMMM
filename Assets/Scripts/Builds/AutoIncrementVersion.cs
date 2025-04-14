// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEditor.Build;
// using UnityEditor.Build.Reporting;
// using System;
// using UnityEngine;

// public class AutoIncrementVersion : IPreprocessBuildWithReport
// {
//     public int callbackOrder => 0;

//     public void OnPreprocessBuild(BuildReport report)
//     {
//         string version = PlayerSettings.bundleVersion;
//         string[] parts = version.Split('.');
//         if (parts.Length >= 2)
//         {
//             try
//             {
//                 int lastPart = int.Parse(parts[^1]);
//                 lastPart++;
//                 parts[^1] = lastPart.ToString();
//                 string newVersion = string.Join(".", parts);
                
//                 PlayerSettings.bundleVersion = newVersion;
//                 Debug.Log($"Đã tăng phiên bản từ {version} lên {newVersion}");
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"Không thể tăng phiên bản: {e.Message}");
//             }
//         }
//         else
//         {
//             Debug.LogError("Chuỗi phiên bản không đúng định dạng. Vui lòng đặt phiên bản theo dạng 'X.Y' (ví dụ: 0.1).");
//         }
//     }
// }
// #endif