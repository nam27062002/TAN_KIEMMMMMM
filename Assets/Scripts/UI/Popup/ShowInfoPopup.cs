using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public VerticalLayoutGroup verticalLayoutGroup;
    public RectTransform container;
    public SkillInfo_UI skillInfoPrefab; 
    public float skillInfoHeight;
    public int space;
    public ScrollRect skillScrollRect;

    private ShowInfoCharacterParameters _showInfoCharacterParameters;

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
        var maxHp = info.Attributes.health;
        hpBarUI.SetValue((float)currentHp / maxHp, $"{currentHp} / {maxHp}");
        var currentMp = info.CurrentMp;
        var maxMp = info.Attributes.mana;
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
        var skills = _showInfoCharacterParameters.Skills;
        int skillCount = skills.Count - 1; // không tính đánh thường
        float newHeight = skillInfoHeight * skillCount + space * (skillCount - 1);
        container.sizeDelta = new Vector2(container.sizeDelta.x, newHeight);
        verticalLayoutGroup.spacing = space;
        foreach (var skill in skills)
        {
            if (skill.skillIndex == 0) continue;
            var go = Instantiate(skillInfoPrefab.gameObject, container);
            var skillInfoUI = go.GetComponent<SkillInfo_UI>();
            skillInfoUI.Setup(skill);
        }
    }

    public override void Close()
    {
        ClearSkillInfo();
        ResetScroll();
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
}
