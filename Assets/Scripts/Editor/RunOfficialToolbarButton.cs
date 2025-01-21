using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

namespace Editor
{
    [InitializeOnLoad]
    public static class RunOfficialToolbarButton
    {
        private static readonly string[] SceneOptions =
            { "Game", "GameManager", "Bootstrap", "MainMenu", "UIManager" };

        private static int _selectedSceneIndex;

        static RunOfficialToolbarButton()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            // Add a label for context
            GUILayout.Label("Scenes:", EditorStyles.label, GUILayout.Width(50));

            // Scene selector
            _selectedSceneIndex = EditorGUILayout.Popup(
                _selectedSceneIndex,
                SceneOptions,
                EditorStyles.toolbarPopup,
                GUILayout.Width(120));

            // Open Scene Button
            if (GUILayout.Button(new GUIContent("🔄 Open Scene", "Open the selected scene"), EditorStyles.toolbarButton))
            {
                OpenSelectedScene();
            }

            // Run Official Button
            if (GUILayout.Button(new GUIContent("▶ Run Official", "Run the official Bootstrap scene"), EditorStyles.toolbarButton))
            {
                RunOfficial();
            }

            GUILayout.EndHorizontal();
        }

        private static void OpenScene(string scenePath, bool closeLevelEditor)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (closeLevelEditor && LevelEditorWindow.Instance != null)
            {
                LevelEditorWindow.Instance.Close();
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        private static void OpenSelectedScene()
        {
            var sceneName = SceneOptions[_selectedSceneIndex];
            var scenePath = $"Assets/_Game/Scenes/{sceneName}.unity";
            OpenScene(scenePath, false);
        }

        private static void RunOfficial()
        {
            OpenScene("Assets/_Game/Scenes/Bootstrap.unity", true);
            EditorApplication.isPlaying = true;
        }
    }

    public class LevelEditorWindow : EditorWindow
    {
        private static LevelEditorWindow _instance;

        public static LevelEditorWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (LevelEditorWindow)GetWindow(typeof(LevelEditorWindow));
                }

                return _instance;
            }
        }
    }
}
