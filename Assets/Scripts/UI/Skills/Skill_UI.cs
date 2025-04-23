using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class Skill_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI skillIndex;
    public TextMeshProUGUI mp;
    public Image skillImage;
    public Button skillButton;
    public GameObject lockObject;
    public Highlightable highlightable;
    public Highlightable tutoHighlightable;
    private bool _isEnoughMana;
    private bool _isLocked;
    private int _skillIndex;
    private Type _type;
    public static Skill_UI Selected;
    
    // Thêm biến static để theo dõi panel nào đang hiển thị
    private static Skill_UI _currentlyShowingPanel;
    
    // Thêm biến static để quản lý việc vô hiệu hóa tạm thời
    private static bool _isTemporarilyDisabled = false;
    private static Coroutine _disableCoroutine;

    [Title("Show Skill UI")] 
    public GameObject showSkillPanel;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescriptionText;
    private float delayToShowSkill = 1f;
    
    private bool _isHovering;
    private Tween _showTween;
    private Tween _hideTween;
    private SkillInfo _skillInfo;

    // Phương thức tĩnh để vô hiệu hóa UI Skill trong một khoảng thời gian
    public static void TemporarilyDisableSkills(float duration = 1f)
    {
        _isTemporarilyDisabled = true;
        
        // Sử dụng DOTween để reset sau một khoảng thời gian
        DOTween.Sequence()
            .AppendInterval(duration)
            .AppendCallback(() => _isTemporarilyDisabled = false);
    }
    
    private static IEnumerator EnableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isTemporarilyDisabled = false;
        _disableCoroutine = null;
    }

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
        
        // Cleanup static reference if this is the one showing panel
        if (_currentlyShowingPanel == this)
        {
            _currentlyShowingPanel = null;
        }
    }
    
    private void OnSkillButtonClicked()
    {
        if (CanTrigger())
        {
            // Ẩn tất cả các panel đang hiển thị
            HideAllPanels();
            
            _isHovering = false;
            HideSkillPanel();
            
            Selected?.highlightable.Unhighlight();
            highlightable.Highlight();
            GameplayManager.Instance.HandleSelectSkill(_skillIndex, this);
            Selected = this;
            
            TemporarilyDisableSkills(0.5f);
        }
    }

    // Phương thức tĩnh để ẩn tất cả các panel
    private static void HideAllPanels()
    {
        if (_currentlyShowingPanel != null)
        {
            _currentlyShowingPanel._isHovering = false;
            _currentlyShowingPanel.HideSkillPanel();
            _currentlyShowingPanel = null;
        }
    }

    private bool CanTrigger()
    {
        return !GameplayManager.Instance.IsTutorialLevel &&
               _isEnoughMana &&
               !_isLocked &&
               _type == Type.Player &&
               GameplayManager.Instance.CanInteract &&
               !_isTemporarilyDisabled;
    }
    
    public void SetSkill(SkillInfo skillInfo, bool unlock, bool enoughMana, Type type)
    {
        _skillInfo = skillInfo;
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
        if (skillInfo.damageDescription != "")
        {
            mp.text = $"{skillInfo.mpCost}MP | damage: {skillInfo.damageDescription}";
        }
        else
        {
            mp.text = $"{skillInfo.mpCost}MP";
        }
    }

    private bool CanShowInfo => _skillIndex != 0 && GameplayManager.Instance.CanInteract;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Kiểm tra trạng thái vô hiệu hóa tạm thời
        if (_isTemporarilyDisabled) return;
        
        if (!CanShowInfo) return;
        _isHovering = true;
        
        // Ẩn panel khác nếu đang hiển thị
        if (_currentlyShowingPanel != null && _currentlyShowingPanel != this)
        {
            _currentlyShowingPanel._isHovering = false;
            _currentlyShowingPanel.HideSkillPanel();
        }
        
        if (!_skillInfo.isDirectionalSkill)
        {
            GameplayManager.Instance.SelectedCharacter.ShowSkillTarget(_skillInfo);   
        }
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
        GameplayManager.Instance.SelectedCharacter.HideSkillTarget();
        if (_isTemporarilyDisabled) return;
        if (!CanShowInfo) return;
        _isHovering = false;
        HideSkillPanel();
    }

    private void ShowSkillPanel()
    {
        if (_showTween != null && _showTween.IsActive()) return;
        
        // Cập nhật panel hiện tại đang hiển thị
        _currentlyShowingPanel = this;
        
        showSkillPanel.SetActive(true);
        _showTween = showSkillPanel.transform.DOScale(Vector3.one, 0.3f)
            .From(Vector3.zero)
            .SetEase(Ease.OutBack);
    }

    private void HideSkillPanel()
    {
        if (_hideTween != null && _hideTween.IsActive()) return;
        
        // Xóa tham chiếu đến panel hiện tại nếu panel này là panel đang hiển thị
        if (_currentlyShowingPanel == this)
        {
            _currentlyShowingPanel = null;
        }
        
        _hideTween = showSkillPanel.transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack) 
            .OnComplete(() => showSkillPanel.SetActive(false));
    }
}