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
            
            characterName.text = config.characterName;
            damage.text = info.GetCurrentDamage().ToString();
            spd.text = info.Attributes.spd.ToString();
            def.text = info.GetDef().ToString();
            chiDef.text = info.GetChiDef().ToString();
            avatar.sprite = config.characterNoBgIcon;
            
            var currentHp = info.CurrentHp;
            var maxHp = info.Attributes.health;
            hpBarUI.SetValue((float)currentHp / maxHp, $"{currentHp} / {maxHp}");
            
            var currentMp = info.CurrentMp;
            var maxMp = info.Attributes.mana;
            if (maxMp != 0)
                mpBarUI.SetValue((float)currentMp / maxMp, $"{currentMp} / {maxMp}");
            else
                mpBarUI.gameObject.SetActive(false);

            ShowSkillInfo();
        }
    }

    private void ShowSkillInfo()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        var skills = _showInfoCharacterParameters.Skills;
        int skillCount = skills.Count;
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
        base.Close();
        if (GameplayManager.Instance.IsTutorialLevel)
        {
            TutorialManager.Instance.OnTutorialClicked(13, 0.5f);
        }
    }
}