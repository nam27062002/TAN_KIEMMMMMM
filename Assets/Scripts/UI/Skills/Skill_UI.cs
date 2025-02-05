using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Skill_UI : MonoBehaviour
{
    public TextMeshProUGUI skillIndex;
    public Image skillImage;
    public Button skillButton;
    public GameObject lockObject;
    public Highlightable highlightable;
    private bool _isEnoughMana;
    private bool _isLocked;
    private int _skillIndex;
    private Type _type;
    public static Skill_UI Selected; 
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
        {
            GameplayManager.Instance.HandleSelectSkill(_skillIndex);
            Selected?.highlightable.Unhighlight();
            Selected = this;
            highlightable.Highlight();
        }
    }

    private bool CanTrigger()
    {
        return !GameplayManager.Instance.IsTutorialLevel &&
               _isEnoughMana &&
               !_isLocked &&
               _type == Type.Player;
    }
    
    public void SetSkill(int index, Sprite skillIcon, bool unlock, bool enoughMana, Type type)
    {
        _isLocked = !unlock;
        _isEnoughMana = enoughMana;
        _skillIndex = index - 1;
        _type = type;
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