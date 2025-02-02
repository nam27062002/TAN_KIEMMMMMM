using System;
using System.Collections.Generic;
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
    
    [Title("Text Mesh Pro")]
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI levelName;
    [SerializeField] private TextMeshProUGUI characterFocusName;
    [SerializeField] private TextMeshProUGUI roundIndex;
    
    [Title("Skill"), Space]
    [SerializeField] private List<Skill_UI> skillUI = new List<Skill_UI>();
    [SerializeField] private UI_Button endTurnButton;
    
    [Title("Character Index"), Space]
    [SerializeField] private Transform characterPool;
    [SerializeField] private GameObject avatarPrefab;
    
    [Title("Avatar"), Space]
    [SerializeField] private Image characterIcon;
    [SerializeField] private ProcessBar hpBar;
    [SerializeField] private ProcessBar mpBar;
    [SerializeField] private ActionPointUI actionPointUI;
    
    [Title("Objects"),Space]
    [SerializeField] private SerializableDictionary<UIInGameObjectType, GameObject> objects;

    public override void Open(UIBaseParameters parameters = null)
    {
        base.Open(parameters);
        HideAllUI();
    }
    
    private void HideAllUI()
    {
        foreach (var item in objects.Values)
        {
            item.SetActive(false);
        }
    }
}
