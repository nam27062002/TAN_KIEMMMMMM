using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class ShowInfoPopup : PopupBase
{
    [Title("Show Info Popup"), Space]
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI damage;
    public TextMeshProUGUI spd;
    public TextMeshProUGUI def;
    public TextMeshProUGUI chiDef;
    public ProcessBar hpBarUI;
    public ProcessBar mpBarUI;
    public Image avatar;

    [Title("Skill Info"), Space]
    public GameObject skillScrollObject;
    public VerticalLayoutGroup verticalLayoutGroup;
    public RectTransform container;
    public SkillInfo_UI skillInfoPrefab;
    public float skillInfoHeight;
    public int space;
    public ScrollRect skillScrollRect;
    public Image skillPanelImage;
    public Image skillTitleName;

    [Title("Story Info"), Space]
    public GameObject storyScrollObject;
    public RectTransform storyContainer;
    public TextMeshProUGUI storyText;
    public Image storyPanelImage;
    public Image storyTitleName;

    private ShowInfoCharacterParameters _showInfoCharacterParameters;
    [SerializeField] private ScrollType scrollType = ScrollType.Skill;
    public Button skillButton;
    public Button storyButton;

    public enum ScrollType
    {
        Skill,
        Story
    }

    [SerializeField, HideInInspector] private ContentSizeFitter storyContentSizeFitter;

    public override void Open(UIBaseParameters parameters = null)
    {
        base.Open(parameters);
        if (parameters is ShowInfoCharacterParameters showInfoCharacterParameters)
        {
            _showInfoCharacterParameters = showInfoCharacterParameters;
            var character = showInfoCharacterParameters.Character;
            var info = character.Info;
            var config = character.characterConfig;
            SetupCharacterInfo(config, info);
            SetupAvatar(config);
            SetupBars(info);
            ShowSkillInfo();
            ResetScroll();
            SetupStoryContent();
            SetScrollUI();
            RegisterButtonEvents();
        }
    }

    private void SetupCharacterInfo(CharacterConfig config, CharacterInfo info)
    {
        characterName.text = config.characterName;
        damage.text = info.GetCurrentDamage().ToString();
        spd.text = info.Attributes.spd.ToString();
        def.text = info.GetDef().ToString();
        chiDef.text = info.GetChiDef().ToString();
    }

    private void SetScrollUI()
    {
        skillScrollObject.SetActiveIfNeeded(scrollType == ScrollType.Skill);
        storyScrollObject.SetActiveIfNeeded(scrollType == ScrollType.Story);
        skillPanelImage.enabled = scrollType == ScrollType.Skill;
        storyPanelImage.enabled = scrollType == ScrollType.Story;
        skillTitleName.color = scrollType == ScrollType.Skill ? Color.black : Color.white;
        storyTitleName.color = scrollType == ScrollType.Story ? Color.black : Color.white;
    }

    private void SetupAvatar(CharacterConfig config)
    {
        avatar.sprite = config.characterNoBgIcon;
        avatar.SetNativeSize();
        RectTransform avatarRect = avatar.GetComponent<RectTransform>();
        RectTransform parentRect = avatar.transform.parent.GetComponent<RectTransform>();
        Vector2 nativeSize = avatarRect.rect.size;
        float scaleFactor = Mathf.Min(parentRect.rect.width / nativeSize.x, parentRect.rect.height / nativeSize.y);
        avatarRect.sizeDelta = nativeSize * scaleFactor;
        avatarRect.anchorMin = new Vector2(0.5f, 0.5f);
        avatarRect.anchorMax = new Vector2(0.5f, 0.5f);
        avatarRect.pivot = new Vector2(0.5f, 0.5f);
        avatarRect.anchoredPosition = Vector2.zero;
    }

    private void SetupBars(CharacterInfo info)
    {
        var currentHp = info.CurrentHp;
        if (currentHp < 0)
        {
            currentHp = 0;
        }
        var maxHp = info.Attributes.health;
        hpBarUI.SetValue((float)currentHp / maxHp, $"{currentHp} / {maxHp}");
        var currentMp = info.CurrentMp;
        var maxMp = info.Attributes.mana;
        if (currentMp < 0)
        {
            currentMp = 0;
        }
        if (maxMp != 0)
            mpBarUI.SetValue((float)currentMp / maxMp, $"{currentMp} / {maxMp}");
        else
            mpBarUI.gameObject.SetActive(false);
    }

    private void ShowSkillInfo()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        var passiveSkill1 = _showInfoCharacterParameters.Character.skillConfig.passtiveSkill1;
        var passiveSkill2 = _showInfoCharacterParameters.Character.skillConfig.passtiveSkill2;

        bool hasPassiveSkill1 = passiveSkill1 != null && !string.IsNullOrEmpty(passiveSkill1.description);
        bool hasPassiveSkill2 = passiveSkill2 != null && !string.IsNullOrEmpty(passiveSkill2.description);

        List<GameObject> skillObjects = new List<GameObject>();

        if (hasPassiveSkill1 || hasPassiveSkill2)
        {
            var passiveGo = Instantiate(skillInfoPrefab.gameObject, container);
            var passiveSkillUI = passiveGo.GetComponent<SkillInfo_UI>();
            passiveSkillUI.SetupPassives(passiveSkill1, passiveSkill2);
            skillObjects.Add(passiveGo);
        }

        var skills = _showInfoCharacterParameters.Skills[_showInfoCharacterParameters.skillTurnType];
        int count = 1;
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i].skillIndex == 0) { continue; }

            var go = Instantiate(skillInfoPrefab.gameObject, container);
            var skillInfoUI = go.GetComponent<SkillInfo_UI>();
            skillInfoUI.SetupNormal(
                _showInfoCharacterParameters.Skills,
                count,
                (int)_showInfoCharacterParameters.skillTurnType
            );
            skillObjects.Add(go);
            count++;
        }

        int totalSkillCount = skillObjects.Count;
        float newHeight = skillInfoHeight * totalSkillCount + space * (totalSkillCount - 1);
        container.sizeDelta = new Vector2(container.sizeDelta.x, newHeight);
        verticalLayoutGroup.spacing = space;
    }

    private void SetupStoryContent()
    {
        storyText.text = _showInfoCharacterParameters.Character.characterConfig.story;
        AdjustStoryContainerSize();
    }

    private void AdjustStoryContainerSize()
    {
        Canvas.ForceUpdateCanvases();
        float textHeight = storyText.GetPreferredValues(storyText.text, storyContainer.rect.width, 0).y;
        storyContainer.sizeDelta = new Vector2(storyContainer.sizeDelta.x, textHeight);

        if (storyContentSizeFitter != null)
        {
            storyContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    private void RegisterButtonEvents()
    {
        skillButton.onClick.RemoveAllListeners();
        storyButton.onClick.RemoveAllListeners();

        skillButton.onClick.AddListener(() =>
        {
            scrollType = ScrollType.Skill;
            SetScrollUI();
            ResetScroll();
        });

        storyButton.onClick.AddListener(() =>
        {
            scrollType = ScrollType.Story;
            SetScrollUI();
            ResetScroll();
        });
    }

    public override void Close()
    {
        scrollType = ScrollType.Skill;
        ClearSkillInfo();
        ResetScroll();
        skillButton.onClick.RemoveAllListeners();
        storyButton.onClick.RemoveAllListeners();
        base.Close();
        if (GameplayManager.Instance.IsTutorialLevel)
        {
            TutorialManager.Instance.OnTutorialClicked(13, 0.5f);
        }
    }

    private void ClearSkillInfo()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    private void ResetScroll()
    {
        if (skillScrollRect != null)
        {
            skillScrollRect.verticalNormalizedPosition = 1;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (storyContainer != null && storyContentSizeFitter == null)
        {
            storyContentSizeFitter = storyContainer.GetComponent<ContentSizeFitter>();
            if (storyContentSizeFitter == null)
            {
                storyContentSizeFitter = storyContainer.gameObject.AddComponent<ContentSizeFitter>();
                storyContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                storyContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }
    }
#endif
}