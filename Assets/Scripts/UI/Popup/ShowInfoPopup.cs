using TMPro;

public abstract class ShowInfoPopup : PopupBase
{
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI damage;
    public TextMeshProUGUI spd;
    public TextMeshProUGUI def;
    public TextMeshProUGUI chiDef;
    public ProcessBar hpBarUI;
    public ProcessBar mpBarUI;
    
    public void OpenPopup()
    {
        // base.OpenPopup();
        var characterParams = GameplayManager.Instance.ShowInfoCharacterParams;
        characterName.text = characterParams.Character.characterConfig.characterName;
        damage.text =  characterParams.Character.characterInfo.Attributes.atk.ToString();
        spd.text =  characterParams.Character.characterInfo.Attributes.spd.ToString();
        def.text =  characterParams.Character.characterInfo.Attributes.def.ToString();
        chiDef.text =  characterParams.Character.characterInfo.Attributes.chiDef.ToString();
        
        var currentHp = characterParams.Character.characterInfo.CurrentHP;
        var maxHp = characterParams.Character.characterInfo.Attributes.health;
        hpBarUI.SetValue(currentHp * 1f/ maxHp, $"{currentHp} / {maxHp}");
        
        var currentMp = characterParams.Character.characterInfo.CurrentMP;
        var maxMp = characterParams.Character.characterInfo.Attributes.mana;
        mpBarUI.SetValue(currentMp * 1f/ maxMp, $"{currentMp} / {maxMp}");
    }
}