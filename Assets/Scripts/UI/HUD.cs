using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class HUD : SingletonMonoBehavior<HUD>
{ 
    public List<Skill_UI> skillUI = new List<Skill_UI>();
    public TextMeshProUGUI characterName;
    public Image characterIcon;
    public void SetCharacterFocus(CharacterParams characterParams)
    {
        foreach (var skill in skillUI)
        {
            skill.gameObject.SetActive(false);
        }

        for (var i = 0; i < characterParams.Skills.Count; i++)
        {
            skillUI[i].SetSkill(index: i + 1, 
                skillIcon: characterParams.Skills[i].icon, 
                unlock: !characterParams.Character.characterInfo.LockSkill, 
                enoughMana: true);
        }

        characterName.text = characterParams.Character.characterConfig.characterName;
        characterIcon.sprite = characterParams.Character.characterConfig.characterIcon;
    }
}