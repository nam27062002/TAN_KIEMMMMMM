using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Skill_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

    [Title("Show Skill UI")] 
    public GameObject showSkillPanel;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescriptionText;
    public float delayToShowSkill = 0.5f;
    
    private bool _isHovering;
    private Tween _showTween;
    private Tween _hideTween;

    private void Awake()
    {
        skillButton.onClick.AddListener(OnSkillButtonClicked);
        showSkillPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        skillButton.onClick.RemoveListener(OnSkillButtonClicked);
        _showTween?.Kill(); 
        _hideTween?.Kill();
    }
    
    private void OnSkillButtonClicked()
    {
        if (CanTrigger())
        {
            Selected?.highlightable.Unhighlight();
            highlightable.Highlight();
            GameplayManager.Instance.HandleSelectSkill(_skillIndex, this);
            Selected = this;
        }
    }

    private bool CanTrigger()
    {
        return !GameplayManager.Instance.IsTutorialLevel &&
               _isEnoughMana &&
               !_isLocked &&
               _type == Type.Player;
    }
    
    public void SetSkill(SkillInfo skillInfo, bool unlock, bool enoughMana, Type type)
    {
        _isLocked = !unlock;
        _isEnoughMana = enoughMana;
        _skillIndex = (int)skillInfo.skillIndex;
        _type = type;
        skillIndex.text = (_skillIndex + 1).ToString();
        skillImage.sprite = skillInfo.icon;
        gameObject.SetActive(true);
        lockObject.SetActive(!unlock);
        if (!unlock) return;
        var color = skillImage.color;
        color.a = enoughMana ? 1f : 0.5f;
        skillImage.color = color;
        skillNameText.text = skillInfo.name;
        skillDescriptionText.text = skillInfo.description;
    }

    private bool CanShowInfo => _skillIndex != 0;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanShowInfo) return;
        _isHovering = true;
        DOTween.Sequence()
            .AppendInterval(delayToShowSkill) 
            .AppendCallback(() =>
            {
                if (_isHovering) 
                {
                    ShowSkillPanel();
                }
            });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!CanShowInfo) return;
        _isHovering = false;
        HideSkillPanel();
    }

    private void ShowSkillPanel()
    {
        if (_showTween != null && _showTween.IsActive()) return; 
        showSkillPanel.SetActive(true);
        _showTween = showSkillPanel.transform.DOScale(Vector3.one, 0.3f)
            .From(Vector3.zero)
            .SetEase(Ease.OutBack); 
    }

    private void HideSkillPanel()
    {
        if (_hideTween != null && _hideTween.IsActive()) return; 
        _hideTween = showSkillPanel.transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack) 
            .OnComplete(() => showSkillPanel.SetActive(false));
    }
}