using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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

    [Title("Text Mesh Pro")] [SerializeField]
    private TextMeshProUGUI characterName;

    [SerializeField] private TextMeshProUGUI levelName; 
    [SerializeField] private TextMeshProUGUI characterFocusName;
    [SerializeField] private TextMeshProUGUI roundIndex;
    [SerializeField] private TextMeshProUGUI notification;
    
    [Title("Skill"), Space] [SerializeField]
    private List<Skill_UI> skillUI = new();

    [SerializeField] private UI_Button endTurnButton;
    [SerializeField] private Highlightable endTurnHighlight;
    [SerializeField] private Toggle toggle;
    
    [Title("Avatar"), Space] [SerializeField]
    private Image characterIcon;

    [SerializeField] private ProcessBar hpBar;
    [SerializeField] private ProcessBar mpBar;
    [SerializeField] private ActionPointUI actionPointUI;

    [Title("Buttons"), Space] [SerializeField]
    private Button settingsButton;
    [SerializeField] public Button skipButton;
    
    [Title("Objects"), Space] [SerializeField]
    private SerializableDictionary<UIInGameObjectType, GameObject> objects;

    private GameplayManager GameplayManager => GameplayManager.Instance;
    private ShowInfoCharacterParameters _characterParams;

    private readonly List<EffectUI> _effectUIs = new();

    [Title("Effects")] [SerializeField] private EffectUI effectUI;
    [SerializeField] private RectTransform effectsPanel;
    [SerializeField] private ContentSizeFitter sizeFitter;

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
        GameplayManager.OnNewRound += GameplayManagerOnOnNewRound;
        endTurnButton.button.onClick.AddListener(OnEndTurnButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsClick);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    protected override void UnregisterEvents()
    {
        base.UnregisterEvents();
        GameplayManager.OnLoadCharacterFinished -= OnLoadCharacterFinished;
        GameplayManager.OnUpdateCharacterInfo -= GameplayManagerOnOnUpdateCharacterInfo;
        GameplayManager.OnNewRound -= GameplayManagerOnOnNewRound;
        endTurnButton.button.onClick.RemoveListener(OnEndTurnButtonClicked);
        settingsButton.onClick.RemoveListener(OnSettingsClick);
        skipButton.onClick.RemoveListener(OnSkipButtonClicked);
        toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
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
    
    private void OnEndTurnButtonClicked()
    {
        if (!GameplayManager.IsTutorialLevel)
        {
            // Vô hiệu hóa UI Skill trong 1 giây
            Skill_UI.TemporarilyDisableSkills(1f);
            
            // Sau đó gọi hàm EndTurn
            GameplayManager.HandleEndTurn("Click vào end turn");
        }
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
    
    private void SetCharacterFocus(ShowInfoCharacterParameters characterParams)
    {
        if (characterParams.Character == null) return;
        SetObjectActiveWhenCharacterFocus();
        _characterParams = characterParams;

        // Skill
        foreach (var skill in skillUI)
        {
            skill.gameObject.SetActive(false);
        }

        var skills = characterParams.Skills[characterParams.skillTurnType];
        for (var i = 0; i < skills.Count; i++)
        {
            bool canCastSkill;
            
            // Kiểm tra nếu là Phạm Cử Chích thì sử dụng phương thức đặc biệt
            if (characterParams.Character is PhamCuChich phamCuChich)
            {
                canCastSkill = phamCuChich.CheckCanCastSkillWithToggle(skills[i]) && _characterParams.Character.CanUseSkill;
            }
            else
            {
                canCastSkill = characterParams.Character.Info.CanCastSkill(skills[i]) && _characterParams.Character.CanUseSkill;
            }
            
            skillUI[i].SetSkill(skills[i],
                unlock: !characterParams.Character.Info.IsLockSkill,
                enoughMana: canCastSkill,
                type: characterParams.Character.Type);
        }

        if (characterParams.Character.IsMainCharacter)
            characterFocusName.text = $"Lượt của {characterParams.Character.characterConfig.characterName}";
        endTurnButton.gameObject.SetActiveIfNeeded(characterParams.Character.CanEndTurn);
        characterName.text = characterParams.Character.characterConfig.characterName;
        characterIcon.sprite = characterParams.Character.characterConfig.characterIcon;
        characterParams.Character.Info.OnHpChanged += OnHpChanged;
        characterParams.Character.Info.OnMpChanged += OnMpChanged;
        OnHpChanged(null);
        OnMpChanged(null);
        actionPointUI.SetActionPoints(characterParams.Character.Info.ActionPointsList);
        SetRound();
        toggle.gameObject.SetActiveIfNeeded(characterParams.Character.characterConfig.hasToggle);
        toggle.isOn = characterParams.Character.Info.IsToggleOn;
        
        // Xử lý hiển thị số lượng độc trùng cho Đoàn Gia Linh
        if (characterParams.Character.characterType == CharacterType.DoanGiaLinh)
        {
            // Tìm TheAllPoisonScript trong passiveSkills
            TheAllPoisonScript poisonScript = characterParams.Character.passiveSkills
                .OfType<TheAllPoisonScript>()
                .FirstOrDefault();
                
            if (poisonScript != null)
            {
                notification.gameObject.SetActive(true);
                notification.text = $"Độc Trùng: {poisonScript.VenomousParasite}";
            }
        }
        else
        {
            // Clear thông báo cho các nhân vật khác
            notification.text = string.Empty;
            notification.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        UpdateEffect();
        SetLevelName();
        UpdateVenomousParasiteInfo();
        skipButton.gameObject.SetActiveIfNeeded(GameplayManager.LevelConfig.levelType == LevelType.Tutorial && GameplayManager.CanShowSkipTutorial
                                                // && SaveLoadManager.Instance.IsFinishedTutorial
                                                );
    }

    private void UpdateEffect()
    {
        if (_characterParams == null || _characterParams.Character == null) return;
        foreach (var item in _effectUIs)
        {
            item.DestroyEffect();
        }

        _effectUIs.Clear();
        foreach (var item in _characterParams.Character.Info.EffectInfo.Effects)
        {
            var go = Instantiate(effectUI.gameObject, effectsPanel);
            go.transform.SetAsFirstSibling();
            var cpn = go.GetComponent<EffectUI>();
            UIManager.Instance.effectIcons.TryGetValue(item.effectType, out var effectIcon);
            if (effectIcon == null) effectIcon = UIManager.Instance.defaultIcon;
            cpn.Initialize(effectIcon);
            _effectUIs.Add(cpn);
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        GameplayManager.SelectedCharacter.Info.IsToggleOn = isOn;
        
        // Refresh skill UI khi toggle thay đổi, đặc biệt quan trọng cho Phạm Cử Chích
        if (_characterParams?.Character is PhamCuChich)
        {
            // Cập nhật lại UI skill để phản ánh trạng thái mới của toggle
            GameplayManagerOnOnUpdateCharacterInfo(null, _characterParams);
        }
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

    private void OnHpChanged(object sender, int _ = 0)
    {
        var currentHp = _characterParams.Character.Info.CurrentHp;
        var maxHp = _characterParams.Character.Info.Attributes.health;
        hpBar.SetValue(currentHp * 1f / maxHp, $"{currentHp} / {maxHp}");
    }

    private void OnMpChanged(object sender, int _ = 0)
    {
        var currentMp = _characterParams.Character.Info.CurrentMp;
        var maxMp = _characterParams.Character.Info.Attributes.mana;
        if (maxMp == 0) return;
        if (currentMp < 0)
        {
            currentMp = 0;
        }
        mpBar.SetValue(currentMp * 1f / maxMp, $"{currentMp} / {maxMp}");
    }

    private void SetRound()
    {
        roundIndex.text = $"Vòng " + GameplayManager.Instance.CurrentRound;
    }

    private void OnSkipButtonClicked()
    {
        // if (UIManager.Instance.CurrentPopup is ConversationPopup conversationPopup)
        // {
        //     conversationPopup.OnSkipButtonClicked();
        // }
        UIManager.Instance?.CurrentPopup?.Close();
        if (TutorialManager.HasInstance)
        {
            TutorialManager.Instance?.DestroyCharacters();
            TutorialManager.Instance?.DestroyTutorial();
        }
        foreach (var item in skillUI)
        {
            item.highlightable.Unhighlight();
            item.tutoHighlightable.Unhighlight();
        }
        endTurnHighlight.Unhighlight();
        DOTween.KillAll();
        GameplayManager.MapManager?.DestroyMap();
        if (GameplayManager.mapPrefab != null) Destroy(GameplayManager.mapPrefab);
        GameplayManager.Instance?.NextLevel();
    }

    private void SetLevelName()
    {
        levelName.text = GameplayManager.LevelConfig.levelName;
    }

    private void UpdateVenomousParasiteInfo()
    {
        if (_characterParams == null || _characterParams.Character == null) return;
        
        if (_characterParams.Character.characterType == CharacterType.DoanGiaLinh)
        {
            TheAllPoisonScript poisonScript = _characterParams.Character.passiveSkills
                .OfType<TheAllPoisonScript>()
                .FirstOrDefault();
                
            if (poisonScript != null)
            {
                notification.gameObject.SetActive(true);
                notification.text = $"Độc Trùng: {poisonScript.VenomousParasite}";
            }
        }
    }
}