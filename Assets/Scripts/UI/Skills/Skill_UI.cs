using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Skill_UI : MonoBehaviour
{
    public TextMeshProUGUI skillIndex;
    public Image skillImage;
    public Button skillButton;
    public GameObject lockObject;

    private bool _isEnoughMana;
    private bool _isLocked;
    private int _skillIndex;
    
    
    private void Awake()
    {
        skillButton.onClick.AddListener(OnSkillButtonClicked);
    }

    private void OnDestroy()
    {
        skillButton.onClick.RemoveListener(OnSkillButtonClicked);
    }

    private void OnSkillButtonClicked()
    {
        if (CanTrigger()) 
            GameplayManager.Instance.HandleSelectSkill(_skillIndex);
    }

    private bool CanTrigger()
    {
        return !GameplayManager.Instance.IsTutorialLevel &&
               _isEnoughMana &&
               !_isLocked;
    }
    
    public void SetSkill(int index, Sprite skillIcon, bool unlock, bool enoughMana)
    {
        _isLocked = !unlock;
        _isEnoughMana = enoughMana;
        _skillIndex = index - 1;
        skillIndex.text = index.ToString();
        skillImage.sprite = skillIcon;
        gameObject.SetActive(true);
        lockObject.SetActive(!unlock);
        Debug.Log($"[Gameplay][Skill_UI] SetSkill {_skillIndex}: {CanTrigger()}");
        if (!unlock) return;
        var color = skillImage.color;
        color.a = enoughMana ? 1f : 0.5f;
        skillImage.color = color;

    }
}