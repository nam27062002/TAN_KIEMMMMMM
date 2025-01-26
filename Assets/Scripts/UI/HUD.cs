using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : SingletonMonoBehavior<HUD>
{ 
    public List<Skill_UI> skillUI = new List<Skill_UI>();
    public Button endTurnButton;
    [Title("Text Mesh Pro")]
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI levelName;
    public TextMeshProUGUI characterFocusName;
    
    [Title("Character Index")]
    public Transform characterPool;
    public GameObject avatarPrefab;
    public List<AVT_SpdUI> avtSpdUI = new();
    
    [Space]
    public Image characterIcon;
    public ProcessBar hpBar;
    public ProcessBar mpBar;
    
    private CharacterParams _characterParams;

    protected override void Awake()
    {
        base.Awake();
        endTurnButton.onClick.AddListener(EndTurnButtonClicked);
        gameObject.SetActiveIfNeeded(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        endTurnButton.onClick.RemoveListener(EndTurnButtonClicked);
    }

    public void SetLevelName(string level)
    {
        levelName.text = level;
    }
    
    public void SetupCharacterFocus(List<Character> characters, int index)
    {
        if (avtSpdUI == null || avtSpdUI.Count == 0)
        {
            avtSpdUI = new List<AVT_SpdUI>();
            foreach (var go in characters.Select(_ => Instantiate(avatarPrefab, characterPool)))
            {
                avtSpdUI.Add(go.GetComponent<AVT_SpdUI>());
            }
        }

        for (var i = 0; i < avtSpdUI.Count; i++)
        {
            avtSpdUI[i].SetupUI(i == index, characters[i] is AICharacter, characters[i].characterConfig.characterIcon);
        }
    }
    
    public void SetCharacterFocus(CharacterParams characterParams)
    {
        Debug.Log("[Gameplay][HUD] SetCharacterFocus");
        _characterParams = characterParams;
        foreach (var skill in skillUI)
        {
            skill.gameObject.SetActive(false);
        }

        for (var i = 0; i < characterParams.Skills.Count; i++)
        {
            skillUI[i].SetSkill(index: i + 1, 
                skillIcon: characterParams.Skills[i].icon, 
                unlock: !characterParams.Character.characterInfo.LockSkill, 
                enoughMana: characterParams.Character.characterInfo.CurrentMP >= characterParams.Skills[i].mpCost);
        }

        if (GameplayManager.Instance.characterManager.IsMainCharacterSelected)
            characterFocusName.text = $"Lượt của {characterParams.Character.characterConfig.characterName}";
        endTurnButton.gameObject.SetActiveIfNeeded(GameplayManager.Instance.characterManager.CanShowEndTurn);
        characterName.text = characterParams.Character.characterConfig.characterName;
        characterIcon.sprite = characterParams.Character.characterConfig.characterIcon;
        characterParams.Character.characterInfo.OnHpChanged = OnHpChanged;
        characterParams.Character.characterInfo.OnMpChanged = OnMpChanged;
        characterParams.Character.characterInfo.OnHpChanged?.Invoke();
        characterParams.Character.characterInfo.OnMpChanged?.Invoke();
    }

    private void OnHpChanged()
    {
        var currentHp = _characterParams.Character.characterInfo.CurrentHP;
        var maxHp = _characterParams.Character.characterInfo.Attributes.health;
        hpBar.SetValue(currentHp * 1f/ maxHp, $"{currentHp} / {maxHp}");
    }

    private void OnMpChanged()
    {
        var currentMp = _characterParams.Character.characterInfo.CurrentMP;
        var maxMp = _characterParams.Character.characterInfo.Attributes.mana;
        mpBar.SetValue(currentMp * 1f/ maxMp, $"{currentMp} / {maxMp}");
    }

    private void EndTurnButtonClicked()
    {
        if (!GameplayManager.Instance.IsTutorialLevel) GameplayManager.Instance.HandleEndTurn();
    }

    public void HideHUD()
    {
        gameObject.SetActive(false);
    }

    public void ShowHUD()
    {
        gameObject.SetActive(true);
    }
}