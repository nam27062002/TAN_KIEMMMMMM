using TMPro;

public class ShowInfoPopup : PopupBase
{
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI damage;
    public TextMeshProUGUI spd;
    public TextMeshProUGUI def;
    public TextMeshProUGUI chiDef;
    public ProcessBar hpBarUI;
    public ProcessBar mpBarUI;

    private Character Character => CharacterManager.Instance.SelectedCharacter;
    public override void OpenPopup()
    {
        base.OpenPopup();
        // characterName.text = Character.Config.characterName;
        // damage.text = Character.Info.CharacterAttributes.atk.ToString();
        // spd.text = Character.Info.Speed.ToString();
        // def.text = Character.Info.CharacterAttributes.def.ToString();
        // chiDef.text = Character.Info.CharacterAttributes.chiDef.ToString();
        // hpBarUI.SetProcess(Character.Info.Health * 1f/ Character.Info.CharacterAttributes.hp, Character.Info.Health,Character.Info.CharacterAttributes.hp);
        // mpBarUI.SetProcess(Character.Info.Mp * 1f/ Character.Info.CharacterAttributes.mp, Character.Info.Mp,Character.Info.CharacterAttributes.mp);
    }
}