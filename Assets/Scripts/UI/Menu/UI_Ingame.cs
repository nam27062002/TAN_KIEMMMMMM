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
    
    [Title("Skill"), Space] [SerializeField]
    private List<Skill_UI> skillUI = new();

    [SerializeField] private UI_Button endTurnButton;
    [SerializeField] private Toggle toggle;
    
    [Title("Avatar"), Space] [SerializeField]
    private Image characterIcon;

    [SerializeField] private ProcessBar hpBar;
    [SerializeField] private ProcessBar mpBar;
    [SerializeField] private ActionPointUI actionPointUI;

    [Title("Buttons"), Space] [SerializeField]
    private Button settingsButton;
    [SerializeField] private Button skipButton;
    
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
        if (!GameplayManager.IsTutorialLevel) GameplayManager.HandleEndTurn("Click vào end turn");
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
            skillUI[i].SetSkill(skills[i],
                unlock: !characterParams.Character.Info.IsLockSkill,
                enoughMana: characterParams.Character.Info.CanCastSkill(skills[i]) && _characterParams.Character.CanUseSkill,
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
    }

    private void FixedUpdate()
    {
        UpdateEffect();
        SetLevelName();
        skipButton.gameObject.SetActiveIfNeeded(GameplayManager.LevelConfig.levelType == LevelType.Tutorial && SaveLoadManager.Instance.IsFinishedTutorial);
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
        if (UIManager.Instance.CurrentPopup is ConversationPopup conversationPopup)
        {
            conversationPopup.OnSkipButtonClicked();
        }

        if (TutorialManager.HasInstance)
        {
            TutorialManager.Instance?.DestroyCharacters();
            TutorialManager.Instance?.DestroyTutorial();
        }
        DOTween.KillAll();
        GameplayManager.Instance?.NextLevel();
    }

    private void SetLevelName()
    {
        levelName.text = GameplayManager.LevelConfig.levelName;
    }
}