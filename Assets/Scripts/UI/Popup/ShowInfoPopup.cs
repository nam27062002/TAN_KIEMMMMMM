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
            damage.text =  showInfoCharacterParameters.Character.Info.Attributes.atk.ToString();
            spd.text =  showInfoCharacterParameters.Character.Info.Attributes.spd.ToString();
            def.text =  showInfoCharacterParameters.Character.Info.Attributes.def.ToString();
            chiDef.text =  showInfoCharacterParameters.Character.Info.Attributes.chiDef.ToString();
            avatar.sprite = showInfoCharacterParameters.Character.characterConfig.characterIcon;
            
            var currentHp = showInfoCharacterParameters.Character.Info.CurrentHp;
            var maxHp = showInfoCharacterParameters.Character.Info.Attributes.health;
            hpBarUI.SetValue(currentHp * 1f/ maxHp, $"{currentHp} / {maxHp}");
        
            var currentMp = showInfoCharacterParameters.Character.Info.CurrentMp;
            var maxMp = showInfoCharacterParameters.Character.Info.Attributes.mana;
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