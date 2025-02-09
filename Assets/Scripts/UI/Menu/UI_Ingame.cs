using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Ingame : MenuBase
{
    [Serializable]
    private enum UIInGameObjectType
    {
        None,
        SettingPanel,
        CharacterSelected,
        CharacterIndex,
    }
    
    [Title("Text Mesh Pro")]
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI levelName;
    [SerializeField] private TextMeshProUGUI characterFocusName;
    [SerializeField] private TextMeshProUGUI roundIndex;
    
    [Title("Skill"), Space]
    [SerializeField] private List<Skill_UI> skillUI = new List<Skill_UI>();
    [SerializeField] private UI_Button endTurnButton;
    
    [Title("Character Index"), Space]
    [SerializeField] private Transform characterPool;
    [SerializeField] private GameObject avatarPrefab;
    
    [Title("Avatar"), Space]
    [SerializeField] private Image characterIcon;
    [SerializeField] private ProcessBar hpBar;
    [SerializeField] private ProcessBar mpBar;
    [SerializeField] private ActionPointUI actionPointUI;
    
    [Title("Buttons"), Space]
    [SerializeField] private Button settingsButton;
    
    [Title("Objects"),Space]
    [SerializeField] private SerializableDictionary<UIInGameObjectType, GameObject> objects;

    private GameplayManager GameplayManager => GameplayManager.Instance;
    private ShowInfoCharacterParameters _characterParams;
    private List<AVT_SpdUI> _avtSpdUI = new();
    
    public override void Open(UIBaseParameters parameters = null)
    {
        base.Open(parameters);
        HideAllUI();
        SetLevelName();
    }

    protected override void RegisterEvents()
    {
        base.RegisterEvents();
        GameplayManager.OnLoadCharacterFinished += OnLoadCharacterFinished;
        GameplayManager.OnUpdateCharacterInfo += GameplayManagerOnOnUpdateCharacterInfo;
        GameplayManager.OnSetMainCharacterFinished += GameplayManagerOnOnSetMainCharacterFinished;
        GameplayManager.OnNewRound += GameplayManagerOnOnNewRound;
        endTurnButton.button.onClick.AddListener(OnEndTurnButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsClick);
    }
    
    protected override void UnregisterEvents()
    {
        base.UnregisterEvents();
        GameplayManager.OnLoadCharacterFinished -= OnLoadCharacterFinished;
        GameplayManager.OnUpdateCharacterInfo -= GameplayManagerOnOnUpdateCharacterInfo;
        GameplayManager.OnSetMainCharacterFinished -= GameplayManagerOnOnSetMainCharacterFinished;
        GameplayManager.OnNewRound -= GameplayManagerOnOnNewRound;
        endTurnButton.button.onClick.RemoveListener(OnEndTurnButtonClicked);
        settingsButton.onClick.RemoveListener(OnSettingsClick);
    }

    
    #region Events

    private void OnLoadCharacterFinished(object sender, EventArgs e)
    {
        var activeObject = new HashSet<UIInGameObjectType>
        {
            UIInGameObjectType.SettingPanel
        };
        foreach (var item in activeObject)
        {
            objects[item].SetActiveIfNeeded(true);
        }
    }
    
    private void OnSettingsClick()
    {
        UIMng.OpenPopup(PopupType.PauseGame);
        AlkawaDebug.Log(ELogCategory.UI, "Clicked Settings");
    }
    
    private void GameplayManagerOnOnUpdateCharacterInfo(object sender, ShowInfoCharacterParameters characterParams)
    {
        SetCharacterFocus(characterParams);
    }
    
    private void GameplayManagerOnOnSetMainCharacterFinished(object sender, EventArgs e)
    {
        SetupCharacterFocus();
    }

    private void OnEndTurnButtonClicked()
    {
        if (!GameplayManager.IsTutorialLevel) GameplayManager.HandleEndTurn();
    }
    
    private void GameplayManagerOnOnNewRound(object sender, EventArgs e)
    {
        SetRound();
    }
    
    #endregion

    private void SetObjectActiveWhenCharacterFocus()
    {
        var activeObject = new HashSet<UIInGameObjectType>
        {
            UIInGameObjectType.CharacterSelected,
            UIInGameObjectType.CharacterIndex
        };
         
        foreach (var item in activeObject)
        {
            objects[item].SetActiveIfNeeded(true);
        }
    }
    
     private void SetupCharacterFocus()
     {
         if (_avtSpdUI == null || _avtSpdUI.Count == 0)
         {
             _avtSpdUI = new List<AVT_SpdUI>();
             foreach (var go in GameplayManager.Characters.Select(_ => Instantiate(avatarPrefab, characterPool)))
             {
                 _avtSpdUI.Add(go.GetComponent<AVT_SpdUI>());
             }
         }

         for (var i = 0; i < _avtSpdUI.Count; i++)
         {
             _avtSpdUI[i].SetupUI(i == GameplayManager.CurrentPlayerIndex, GameplayManager.Characters[i].Type, GameplayManager.Characters[i].characterConfig.characterIcon);
         }
     }
    
    private void SetCharacterFocus(ShowInfoCharacterParameters characterParams)
     {
         SetObjectActiveWhenCharacterFocus();
         _characterParams = characterParams;
         
         // Skill
         foreach (var skill in skillUI)
         {
             skill.gameObject.SetActive(false);
         }
         
         for (var i = 0; i < characterParams.Skills.Count; i++)
         {
             skillUI[i].SetSkill(index: i + 1, 
                 skillIcon: characterParams.Skills[i].icon, 
                 unlock: !characterParams.Character.CharacterInfo.IsLockSkill, 
                 enoughMana: characterParams.Character.CharacterInfo.CanCastSkill(characterParams.Skills[i]),
                 type: characterParams.Character.Type);
         }

         if (characterParams.Character.IsMainCharacter)
            characterFocusName.text = $"Lượt của {characterParams.Character.characterConfig.characterName}";
         endTurnButton.gameObject.SetActiveIfNeeded(characterParams.Character.CanEndTurn);
         characterName.text = characterParams.Character.characterConfig.characterName;
         characterIcon.sprite = characterParams.Character.characterConfig.characterIcon;
         characterParams.Character.CharacterInfo.OnHpChanged += OnHpChanged;
         characterParams.Character.CharacterInfo.OnMpChanged += OnMpChanged;
         OnHpChanged(null);
         OnMpChanged(null);
         actionPointUI.SetActionPoints(characterParams.Character.CharacterInfo.GetActionPoints());
         SetRound();
     }
    
    public void HideAllUI()
    {
        foreach (var item in objects.Values)
        {
            item.SetActive(false);
        }
    }

    public void ShowAllUI()
    {
        foreach (var item in objects.Values)
        {
            item.SetActive(true);
        }
    }
    
     public void OnCharacterDeath(int index)
     {
         _avtSpdUI[index].DestroyObject();
         _avtSpdUI.RemoveAt(index);   
     }
    
    private void OnHpChanged(object sender, int _ = 0)
    {
        var currentHp = _characterParams.Character.CharacterInfo.CurrentHp;
        var maxHp = _characterParams.Character.CharacterInfo.Attributes.health;
        hpBar.SetValue(currentHp * 1f/ maxHp, $"{currentHp} / {maxHp}");
    }
    
    private void OnMpChanged(object sender, int _ = 0)
    {
        var currentMp = _characterParams.Character.CharacterInfo.CurrentMp;
        var maxMp = _characterParams.Character.CharacterInfo.Attributes.mana;
        mpBar.SetValue(currentMp * 1f/ maxMp, $"{currentMp} / {maxMp}");
    }
    
     private void SetRound()
     {
         roundIndex.text = $"Vòng " + GameplayManager.Instance.CurrentRound;
     }
     
     private void SetLevelName()
     {
         levelName.text = GameplayManager.LevelConfig.levelName;
     }
}
