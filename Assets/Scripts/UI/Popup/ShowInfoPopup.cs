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
    
    public override void Open(UIBaseParameters parameters = null)
    {
        base.Open(parameters);
        if (parameters is ShowInfoCharacterParameters showInfoCharacterParameters)
        {
            characterName.text = showInfoCharacterParameters.Character.characterConfig.characterName;
            damage.text =  showInfoCharacterParameters.Character.characterInfo.Attributes.atk.ToString();
            spd.text =  showInfoCharacterParameters.Character.characterInfo.Attributes.spd.ToString();
            def.text =  showInfoCharacterParameters.Character.characterInfo.Attributes.def.ToString();
            chiDef.text =  showInfoCharacterParameters.Character.characterInfo.Attributes.chiDef.ToString();
            avatar.sprite = showInfoCharacterParameters.Character.characterConfig.characterIcon;
            
            var currentHp = showInfoCharacterParameters.Character.characterInfo.CurrentHp;
            var maxHp = showInfoCharacterParameters.Character.characterInfo.Attributes.health;
            hpBarUI.SetValue(currentHp * 1f/ maxHp, $"{currentHp} / {maxHp}");
        
            var currentMp = showInfoCharacterParameters.Character.characterInfo.CurrentMp;
            var maxMp = showInfoCharacterParameters.Character.characterInfo.Attributes.mana;
            mpBarUI.SetValue(currentMp * 1f/ maxMp, $"{currentMp} / {maxMp}");
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