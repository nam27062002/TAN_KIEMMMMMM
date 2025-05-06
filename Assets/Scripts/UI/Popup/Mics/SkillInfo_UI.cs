using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillInfo_UI : MonoBehaviour
{
    public Image skillImage;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI skillDescription;
    public Button leftButton;
    public Button rightButton;
    public Sprite normal;
    public Sprite highlight;
    public List<Image> circles = new();

    public GameObject skillImageParent;

    [SerializeField] private int currentTurnTypeIndex;
    private Dictionary<SkillTurnType, List<SkillInfo>> _skills = new();
    private int _skillIndex;
    private bool _isPassiveSkill;

    private SkillInfo _passiveSkill1;
    private SkillInfo _passiveSkill2;
    private bool _hasPassiveSkill1;
    private bool _hasPassiveSkill2;
    private int _currentPassiveIndex = 0;
    public ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRectTransform;
    [SerializeField] private RectTransform viewportRectTransform;
    
    // Biến để theo dõi trạng thái của scroll
    private bool _isAtTop = true;
    private bool _isAtBottom = false;

    private void Awake()
    {
        leftButton.onClick.AddListener(OnLeftButtonClick);
        rightButton.onClick.AddListener(OnRightButtonClick);

        if (skillImageParent == null)
        {
            skillImageParent = skillImage.transform.parent.gameObject;
        }
        
        // Lấy reference đến contentRect và viewportRect nếu chưa có
        if (contentRectTransform == null && scrollRect != null)
        {
            contentRectTransform = scrollRect.content;
        }
        
        if (viewportRectTransform == null && scrollRect != null)
        {
            viewportRectTransform = scrollRect.viewport as RectTransform;
        }
    }

    private void OnDestroy()
    {
        leftButton.onClick.RemoveListener(OnLeftButtonClick);
        rightButton.onClick.RemoveListener(OnRightButtonClick);
    }
    
    // Đặt lại vị trí scroll về đầu - đổi thành public để ShowInfoPopup có thể gọi
    public void ResetScrollPosition()
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1.0f;
            UpdateScrollState();
            _isAtTop = true;
            _isAtBottom = false;
        }
    }
    
    // Kiểm tra và cập nhật trạng thái của ScrollRect
    private void UpdateScrollState()
    {
        if (scrollRect == null || contentRectTransform == null || viewportRectTransform == null) 
            return;
        
        // Tính toán xem content có cần scroll không
        bool needsScrolling = contentRectTransform.rect.height > viewportRectTransform.rect.height;
        
        // Chỉ bật scroll nếu cần thiết
        scrollRect.enabled = needsScrolling;
        
        if (needsScrolling)
        {
            // Giới hạn vị trí scroll
            ClampScrollPosition();
        }
    }
    
    // Giới hạn vị trí cuộn để tránh khoảng trống
    private void ClampScrollPosition()
    {
        if (scrollRect == null) return;
        
        // Đảm bảo không cuộn quá cao hoặc quá thấp
        float normalizedPosition = scrollRect.verticalNormalizedPosition;
        normalizedPosition = Mathf.Clamp01(normalizedPosition);
        
        // Chỉ set lại nếu thực sự cần thiết để tránh event loops
        if (Mathf.Abs(scrollRect.verticalNormalizedPosition - normalizedPosition) > 0.001f)
        {
            scrollRect.verticalNormalizedPosition = normalizedPosition;
        }
        
        // Cập nhật trạng thái vị trí scroll
        _isAtTop = normalizedPosition >= 0.99f;
        _isAtBottom = normalizedPosition <= 0.01f;
    }
    
    // Phương thức công khai để ShowInfoPopup có thể gọi để kiểm tra giới hạn scroll
    public void CheckScrollBounds()
    {
        ClampScrollPosition();
    }
    
    // Kiểm tra xem nên cho phép ScrollRect cha xử lý sự kiện không
    public bool ShouldAllowParentScrolling(Vector2 scrollDelta)
    {
        // Nếu đang ở đỉnh và muốn scroll lên trên nữa hoặc ở đáy và muốn scroll xuống nữa
        return (_isAtTop && scrollDelta.y > 0) || (_isAtBottom && scrollDelta.y < 0);
    }
    
    // Được gọi khi kích thước của content thay đổi
    public void OnContentSizeChanged()
    {
        UpdateScrollState();
    }

    public void SetupPassives(SkillInfo passiveSkill1, SkillInfo passiveSkill2)
    {
        _isPassiveSkill = true;
        _passiveSkill1 = passiveSkill1;
        _passiveSkill2 = passiveSkill2;

        _hasPassiveSkill1 = passiveSkill1 != null && !string.IsNullOrEmpty(passiveSkill1.description);
        _hasPassiveSkill2 = passiveSkill2 != null && !string.IsNullOrEmpty(passiveSkill2.description);

        _currentPassiveIndex = _hasPassiveSkill1 ? 0 : 1;

        for (int i = 0; i < circles.Count; i++)
        {
            if (i == 0)
                circles[i].gameObject.SetActive(_hasPassiveSkill1);
            else if (i == 1)
                circles[i].gameObject.SetActive(_hasPassiveSkill2);
            else
                circles[i].gameObject.SetActive(false);
        }

        UpdatePassiveDisplay();
        ResetScrollPosition();
    }

    private void UpdatePassiveDisplay()
    {
        SkillInfo currentPassive = _currentPassiveIndex == 0 ? _passiveSkill1 : _passiveSkill2;

        if (currentPassive != null)
        {
            skillName.text = currentPassive.name;
            skillDescription.text = currentPassive.description;

            if (currentPassive.icon != null)
            {
                skillImageParent.SetActive(true);
                skillImage.sprite = currentPassive.icon;
            }
            else
            {
                skillImageParent.SetActive(false);
            }
        }
        else
        {
            skillName.text = "";
            skillDescription.text = "";
            skillImageParent.SetActive(false);
        }

        for (int i = 0; i < 2; i++)
        {
            if (circles[i].gameObject.activeSelf)
            {
                circles[i].sprite = (i == _currentPassiveIndex) ? highlight : normal;
            }
        }

        bool canGoLeft = _currentPassiveIndex > 0 && _hasPassiveSkill1;
        bool canGoRight = _currentPassiveIndex < 1 && _hasPassiveSkill2;

        leftButton.gameObject.SetActive(canGoLeft);
        rightButton.gameObject.SetActive(canGoRight);
        
        // Cập nhật trạng thái scroll sau khi thay đổi nội dung
        Canvas.ForceUpdateCanvases();
        UpdateScrollState();
    }

    public void SetupNormal(Dictionary<SkillTurnType, List<SkillInfo>> skillInfos, int skillIndex, int turnTypeIndex)
    {
        _isPassiveSkill = false;
        _skills = skillInfos;
        _skillIndex = skillIndex;

        List<int> availableTurnIndices = new List<int>();
        for (int i = 0; i < circles.Count; i++)
        {
            bool hasSkill = _skills.ContainsKey((SkillTurnType)i)
                            && _skills[(SkillTurnType)i].Exists(
                                s => s.skillIndex != 0 && !string.IsNullOrEmpty(s.description)
                            );
            if (hasSkill)
            {
                availableTurnIndices.Add(i);
            }
        }

        if (availableTurnIndices.Contains(turnTypeIndex))
        {
            currentTurnTypeIndex = availableTurnIndices.IndexOf(turnTypeIndex);
        }
        else if (availableTurnIndices.Count > 0)
        {
            currentTurnTypeIndex = 0;
        }
        else
        {
            currentTurnTypeIndex = -1;
        }

        UpdateNormalUI(availableTurnIndices);
        ResetScrollPosition();
    }

    private void UpdateNormalUI(List<int> availableTurnIndices)
    {
        for (int i = 0; i < circles.Count; i++)
        {
            circles[i].gameObject.SetActive(availableTurnIndices.Contains(i));
        }

        if (currentTurnTypeIndex < 0 || availableTurnIndices.Count == 0)
        {
            skillImageParent.SetActive(false);
            skillName.text = "";
            skillDescription.text = "";
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
            return;
        }

        int turnType = availableTurnIndices[currentTurnTypeIndex];
        var skillInfo = _skills[(SkillTurnType)turnType][_skillIndex];

        skillName.text = skillInfo.name;
        skillDescription.text = skillInfo.description;

        if (skillInfo.icon != null)
        {
            skillImageParent.SetActive(true);
            skillImage.sprite = skillInfo.icon;
        }
        else
        {
            skillImageParent.SetActive(false);
        }

        for (int i = 0; i < circles.Count; i++)
        {
            if (i == turnType)
            {
                circles[i].sprite = highlight;
            }
            else if (circles[i].gameObject.activeSelf)
            {
                circles[i].sprite = normal;
            }
        }

        leftButton.gameObject.SetActive(currentTurnTypeIndex > 0);
        rightButton.gameObject.SetActive(currentTurnTypeIndex < availableTurnIndices.Count - 1);
        
        // Cập nhật trạng thái scroll sau khi thay đổi nội dung
        Canvas.ForceUpdateCanvases();
        UpdateScrollState();
    }

    private void OnLeftButtonClick()
    {
        if (_isPassiveSkill)
        {
            if (_currentPassiveIndex > 0 && _hasPassiveSkill1)
            {
                _currentPassiveIndex--;
                UpdatePassiveDisplay();
                ResetScrollPosition();
            }
        }
        else
        {
            List<int> availableTurnIndices = new List<int>();
            for (int i = 0; i < circles.Count; i++)
            {
                bool hasSkill = _skills.ContainsKey((SkillTurnType)i)
                                && _skills[(SkillTurnType)i].Exists(
                                    s => s.skillIndex != 0 && !string.IsNullOrEmpty(s.description)
                                );
                if (hasSkill)
                {
                    availableTurnIndices.Add(i);
                }
            }

            currentTurnTypeIndex--;
            if (currentTurnTypeIndex < 0)
            {
                currentTurnTypeIndex = 0;
            }

            UpdateNormalUI(availableTurnIndices);
            ResetScrollPosition();
        }
    }

    private void OnRightButtonClick()
    {
        if (_isPassiveSkill)
        {
            if (_currentPassiveIndex < 1 && _hasPassiveSkill2)
            {
                _currentPassiveIndex++;
                UpdatePassiveDisplay();
                ResetScrollPosition();
            }
        }
        else
        {
            List<int> availableTurnIndices = new List<int>();
            for (int i = 0; i < circles.Count; i++)
            {
                bool hasSkill = _skills.ContainsKey((SkillTurnType)i)
                                && _skills[(SkillTurnType)i].Exists(
                                    s => s.skillIndex != 0 && !string.IsNullOrEmpty(s.description)
                                );
                if (hasSkill)
                {
                    availableTurnIndices.Add(i);
                }
            }

            currentTurnTypeIndex++;
            if (currentTurnTypeIndex >= availableTurnIndices.Count)
            {
                currentTurnTypeIndex = availableTurnIndices.Count - 1;
            }

            UpdateNormalUI(availableTurnIndices);
            ResetScrollPosition();
        }
    }
    
    private void OnEnable()
    {
        ResetScrollPosition();
    }
    
    private void OnDisable()
    {
        ResetScrollPosition();
    }
    
    private void Update()
    {
        // Kiểm tra và giới hạn vị trí scroll mỗi khung hình nếu scroll đang hoạt động
        if (scrollRect != null && scrollRect.enabled)
        {
            ClampScrollPosition();
        }
    }
}