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

    [SerializeField] private int currentIndex;
    private Dictionary<SkillTurnType, List<SkillInfo>> _skills = new();
    private int _skillIndex;

    // Danh sách các SkillTurnType có skill thực sự (để ẩn/hiện chính xác)
    private List<int> availableTurnIndices = new();

    private void Awake()
    {
        leftButton.onClick.AddListener(OnLeftButtonClick);
        rightButton.onClick.AddListener(OnRightButtonClick);
    }

    private void OnDestroy()
    {
        leftButton.onClick.RemoveListener(OnLeftButtonClick);
        rightButton.onClick.RemoveListener(OnRightButtonClick);
    }

    public void Setup(Dictionary<SkillTurnType, List<SkillInfo>> skillInfos, int skillIndex, int index)
    {
        _skills = skillInfos;
        _skillIndex = skillIndex;

        // Xác định những skill turn type nào thực sự có skill 
        availableTurnIndices.Clear();
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

        // Nếu chưa có turn type nào khả dụng, đặt currentIndex = 0 để tránh lỗi
        if (availableTurnIndices.Count == 0)
        {
            currentIndex = 0;
        }
        else
        {
            // Nếu index truyền vào không hợp lệ, hoặc lớn hơn danh sách,
            // ta gán mặc định là 0 (có thể thay đổi nếu muốn)
            if (index < 0 || index >= availableTurnIndices.Count)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex = index;
            }
        }

        SetupUI();
    }

    private void SetupUI()
    {
        // Ẩn/hiện các vòng tròn tuỳ theo turn type có skill hay không
        for (int i = 0; i < circles.Count; i++)
        {
            bool isActive = availableTurnIndices.Contains(i);
            circles[i].gameObject.SetActive(isActive);
        }

        // Nếu không có turn nào khả dụng thì không cần hiển thị skill
        if (availableTurnIndices.Count == 0)
        {
            skillImage.sprite = null;
            skillName.text = "";
            skillDescription.text = "";
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
            return;
        }

        // Lấy turn type hiện tại dựa trên currentIndex trong danh sách availableTurnIndices
        int turnType = availableTurnIndices[currentIndex];
        var skillInfo = _skills[(SkillTurnType)turnType][_skillIndex];

        // Cập nhật UI hiển thị skill
        skillImage.sprite = skillInfo.icon;
        skillName.text = skillInfo.name;
        skillDescription.text = skillInfo.description;

        // Đánh dấu highlight vòng tròn của turn hiện tại
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

        // Ẩn nút trái/phải nếu không còn turn trước/sau
        leftButton.gameObject.SetActive(currentIndex > 0);
        rightButton.gameObject.SetActive(currentIndex < availableTurnIndices.Count - 1);
    }

    private void OnLeftButtonClick()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }
        SetupUI();
    }

    private void OnRightButtonClick()
    {
        currentIndex++;
        if (currentIndex > availableTurnIndices.Count - 1)
        {
            currentIndex = availableTurnIndices.Count - 1;
        }
        SetupUI();
    }
}