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

    [Title("Character Index"), Space] [SerializeField]
    private Transform characterPool;

    [SerializeField] private GameObject avatarPrefab;

    [Title("Avatar"), Space] [SerializeField]
    private Image characterIcon;

    [SerializeField] private ProcessBar hpBar;
    [SerializeField] private ProcessBar mpBar;
    [SerializeField] private ActionPointUI actionPointUI;

    [Title("Buttons"), Space] [SerializeField]
    private Button settingsButton;

    [Title("Objects"), Space] [SerializeField]
    private SerializableDictionary<UIInGameObjectType, GameObject> objects;

    private GameplayManager GameplayManager => GameplayManager.Instance;
    private ShowInfoCharacterParameters _characterParams;
    private List<AVT_SpdUI> _avtSpdUI = new();

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
        GameplayManager.OnSetMainCharacterFinished += GameplayManagerOnOnSetMainCharacterFinished;
        GameplayManager.OnNewRound += GameplayManagerOnOnNewRound;
        GameplayManager.OnRetry += OnRetry;
        endTurnButton.button.onClick.AddListener(OnEndTurnButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsClick);
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    protected override void UnregisterEvents()
    {
        base.UnregisterEvents();
        GameplayManager.OnLoadCharacterFinished -= OnLoadCharacterFinished;
        GameplayManager.OnUpdateCharacterInfo -= GameplayManagerOnOnUpdateCharacterInfo;
        GameplayManager.OnSetMainCharacterFinished -= GameplayManagerOnOnSetMainCharacterFinished;
        GameplayManager.OnNewRound -= GameplayManagerOnOnNewRound;
        GameplayManager.OnRetry -= OnRetry;
        endTurnButton.button.onClick.RemoveListener(OnEndTurnButtonClicked);
        settingsButton.onClick.RemoveListener(OnSettingsClick);
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

    private void OnRetry(object sender, EventArgs e)
    {
        if (_avtSpdUI == null) return;
        foreach (var item in _avtSpdUI.ToList())
        {
            if (item != null)
            {
                item.transform.DOKill();
                var rect = item.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.DOKill();
                }

                item.DestroyObject();
            }

            _avtSpdUI.Remove(item);
        }

        _avtSpdUI.Clear();
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
        var validCharacters = GameplayManager.Characters.Where(c => c != null).ToList();

        if (_avtSpdUI == null || 
            !_avtSpdUI.Any() || 
            _avtSpdUI.Count != validCharacters.Count)
        {
            foreach (var item in _avtSpdUI.ToList())
            {
                if (item != null)
                {
                    item.DestroyObject();
                }
            }
            _avtSpdUI = new List<AVT_SpdUI>();
            
            foreach (var character in validCharacters)
            {
                if (character == null) continue;

                var go = Instantiate(avatarPrefab, characterPool);
                if (go == null) continue;

                var rt = go.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0, 0.5f);
                    rt.anchorMax = new Vector2(0, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                }

                var avtSpd = go.GetComponent<AVT_SpdUI>();
                if (avtSpd != null)
                {
                    _avtSpdUI.Add(avtSpd);
                }
                else
                {
                    Debug.LogError("AVT_SpdUI component missing on instantiated object");
                    Destroy(go);
                }
            }
        }
    
        var count = _avtSpdUI.Count;
        var centerIndex = (count % 2 == 0) ? count / 2 - 1 : count / 2;
        var startIndex = GameplayManager.CurrentPlayerIndex - centerIndex;
        if (startIndex < 0)
        {
            startIndex += count;
        }

        var spacing = 80f;
        var fixedY = 0f;
        if (_avtSpdUI.Count > 0)
        {
            fixedY = _avtSpdUI[0].GetComponent<RectTransform>().anchoredPosition.y;
        }
        var poolRect = characterPool.GetComponent<RectTransform>();
        var poolWidth = poolRect.rect.width;
        var totalGroupWidth = (count - 1) * spacing;
        var offset = (poolWidth - totalGroupWidth) / 2;

        for (var i = 0; i < count; i++)
        {
            var index = (startIndex + i) % count;
            var avatarRect = _avtSpdUI[index].GetComponent<RectTransform>();
            var targetPos = new Vector2(offset + i * spacing, fixedY);
            // Gán trực tiếp vị trí thay vì tween
            avatarRect.anchoredPosition = targetPos;
        
            var isFocused = (index == GameplayManager.CurrentPlayerIndex);
            var targetScale = isFocused ? 1f : 0.7f;
            // Gán trực tiếp scale thay vì tween
            _avtSpdUI[index].transform.localScale = new Vector3(targetScale, targetScale, 1f);

            _avtSpdUI[index].SetupUI(isFocused,
                GameplayManager.Characters[index].Type,
                GameplayManager.Characters[index].characterConfig.slideBarIcon);
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

        for (var i = 0; i < characterParams.Skills.Count; i++)
        {
            skillUI[i].SetSkill(characterParams.Skills[i],
                unlock: !characterParams.Character.Info.IsLockSkill,
                enoughMana: characterParams.Character.Info.CanCastSkill(characterParams.Skills[i]),
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
            UIManager.Instance.effectIcons.TryGetValue(item.EffectType, out var effectIcon);
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

    public void OnCharacterDeath(int index)
    {
        if (index < 0 || index >= _avtSpdUI.Count)
            return;

        var avatarUI = _avtSpdUI[index];
        if (avatarUI != null)
        {
            avatarUI.transform.DOKill();
            var rect = avatarUI.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOKill();
            }
            _avtSpdUI.Remove(avatarUI);
            avatarUI.DestroyObject();
        }

        SetupCharacterFocus();
        // Debug.Log($"NT - OnCharacterDeath: {index}");
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
        mpBar.SetValue(currentMp * 1f / maxMp, $"{currentMp} / {maxMp}");
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