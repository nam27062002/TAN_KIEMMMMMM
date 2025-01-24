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
        if (!GameplayManager.Instance.IsTutorialLevel) GameplayManager.Instance.HandleSelectSkill(_skillIndex);
    }
    
    public void SetSkill(int index, Sprite skillIcon, bool unlock, bool enoughMana)
    {
        _isEnoughMana = !unlock;
        _isEnoughMana = enoughMana;
        _skillIndex = index - 1;
        skillIndex.text = index.ToString();
        skillImage.sprite = skillIcon;
        gameObject.SetActive(true);
        lockObject.SetActive(!unlock);

        if (!unlock) return;
        var color = skillImage.color;
        color.a = enoughMana ? 1f : 0.5f;
        skillImage.color = color;

    }
}