using UnityEngine;

namespace SaveLoad
{
    public class SaveLoadManager : SingletonMonoBehavior<SaveLoadManager>
    {
        public static int currentLevel = 0;
        
        private const string CURRENT_LEVEL_KEY = "CurrentLevel"; 
        private const int DEFAULT_LEVEL = 0; 

        private void Start()
        {
            LoadLevel();
        }

        public static void LoadLevel()
        {
            currentLevel = PlayerPrefs.HasKey(CURRENT_LEVEL_KEY) ? PlayerPrefs.GetInt(CURRENT_LEVEL_KEY) : DEFAULT_LEVEL;
        }

        public void SetCurrentLevel(int level)
        {
            currentLevel = level;
            SaveLevel();
        }

        public static void SaveLevel()
        {
            PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, currentLevel);
            PlayerPrefs.Save();
        }

        public void ClearAll()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}