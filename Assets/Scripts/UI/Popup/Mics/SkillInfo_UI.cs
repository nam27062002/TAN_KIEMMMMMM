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

    // Thêm tham chiếu đến parent của icon để có thể ẩn/hiện
    public GameObject skillImageParent;

    [SerializeField] private int currentTurnTypeIndex;
    private Dictionary<SkillTurnType, List<SkillInfo>> _skills = new();
    private int _skillIndex;
    private bool _isPassiveSkill;

    // Thêm biến cho passive skills
    private SkillInfo _passiveSkill1;
    private SkillInfo _passiveSkill2;
    private bool _hasPassiveSkill1;
    private bool _hasPassiveSkill2;
    private int _currentPassiveIndex = 0;

    private void Awake()
    {
        leftButton.onClick.AddListener(OnLeftButtonClick);
        rightButton.onClick.AddListener(OnRightButtonClick);

        // Nếu không có tham chiếu đến parent của icon, gán bằng chính gameObject của icon
        if (skillImageParent == null)
        {
            skillImageParent = skillImage.transform.parent.gameObject;
        }
    }

    private void OnDestroy()
    {
        leftButton.onClick.RemoveListener(OnLeftButtonClick);
        rightButton.onClick.RemoveListener(OnRightButtonClick);
    }

    // Setup cho passive skills
    public void SetupPassives(SkillInfo passiveSkill1, SkillInfo passiveSkill2)
    {
        _isPassiveSkill = true;
        _passiveSkill1 = passiveSkill1;
        _passiveSkill2 = passiveSkill2;

        _hasPassiveSkill1 = passiveSkill1 != null && !string.IsNullOrEmpty(passiveSkill1.description);
        _hasPassiveSkill2 = passiveSkill2 != null && !string.IsNullOrEmpty(passiveSkill2.description);

        // Mặc định hiển thị passive skill 1 nếu có
        _currentPassiveIndex = _hasPassiveSkill1 ? 0 : 1;

        // Thiết lập hiển thị cho circles - chỉ hiển thị 2 circle đầu tiên
        for (int i = 0; i < circles.Count; i++)
        {
            // Chỉ hiển thị circle 0 nếu có passive skill 1
            // Chỉ hiển thị circle 1 nếu có passive skill 2
            if (i == 0)
                circles[i].gameObject.SetActive(_hasPassiveSkill1);
            else if (i == 1)
                circles[i].gameObject.SetActive(_hasPassiveSkill2);
            else
                circles[i].gameObject.SetActive(false); // Ẩn các circle khác
        }

        // Hiển thị thông tin passive skill hiện tại
        UpdatePassiveDisplay();
    }

    private void UpdatePassiveDisplay()
    {
        // Lấy passive skill hiện tại
        SkillInfo currentPassive = _currentPassiveIndex == 0 ? _passiveSkill1 : _passiveSkill2;

        // Hiển thị thông tin
        if (currentPassive != null)
        {
            skillName.text = currentPassive.name;
            skillDescription.text = currentPassive.description;

            // Kiểm tra và hiển thị icon
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

        // Cập nhật hiển thị circles - highlight circle hiện tại
        for (int i = 0; i < 2; i++) // Chỉ xử lý 2 circle đầu tiên cho passive skills
        {
            if (circles[i].gameObject.activeSelf)
            {
                circles[i].sprite = (i == _currentPassiveIndex) ? highlight : normal;
            }
        }

        // Cập nhật trạng thái nút điều hướng
        bool canGoLeft = _currentPassiveIndex > 0 && _hasPassiveSkill1;
        bool canGoRight = _currentPassiveIndex < 1 && _hasPassiveSkill2;

        leftButton.gameObject.SetActive(canGoLeft);
        rightButton.gameObject.SetActive(canGoRight);
    }

    // Setup cho normal skill
    public void SetupNormal(Dictionary<SkillTurnType, List<SkillInfo>> skillInfos, int skillIndex, int turnTypeIndex)
    {
        _isPassiveSkill = false;
        _skills = skillInfos;
        _skillIndex = skillIndex;

        // Xác định các turn type có skill
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

        // Kiểm tra tính hợp lệ của turn type được chỉ định
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
            // Không có turn type nào hợp lệ
            currentTurnTypeIndex = -1;
        }

        UpdateNormalUI(availableTurnIndices);
    }

    private void UpdateNormalUI(List<int> availableTurnIndices)
    {
        // Ẩn/hiện các chỉ báo vòng tròn
        for (int i = 0; i < circles.Count; i++)
        {
            circles[i].gameObject.SetActive(availableTurnIndices.Contains(i));
        }

        // Nếu không có turn type nào khả dụng
        if (currentTurnTypeIndex < 0 || availableTurnIndices.Count == 0)
        {
            skillImageParent.SetActive(false);
            skillName.text = "";
            skillDescription.text = "";
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
            return;
        }

        // Lấy turn type hiện tại và hiển thị skill
        int turnType = availableTurnIndices[currentTurnTypeIndex];
        var skillInfo = _skills[(SkillTurnType)turnType][_skillIndex];

        // Hiển thị thông tin skill
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

        // Đánh dấu vòng tròn hiện tại
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

        // Cập nhật trạng thái nút điều hướng
        leftButton.gameObject.SetActive(currentTurnTypeIndex > 0);
        rightButton.gameObject.SetActive(currentTurnTypeIndex < availableTurnIndices.Count - 1);
    }

    private void OnLeftButtonClick()
    {
        if (_isPassiveSkill)
        {
            // Xử lý cho passive skill - chuyển từ passive 2 sang passive 1
            if (_currentPassiveIndex > 0 && _hasPassiveSkill1)
            {
                _currentPassiveIndex--;
                UpdatePassiveDisplay();
            }
        }
        else
        {
            // Xử lý cho normal skill
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
        }
    }

    private void OnRightButtonClick()
    {
        if (_isPassiveSkill)
        {
            // Xử lý cho passive skill - chuyển từ passive 1 sang passive 2
            if (_currentPassiveIndex < 1 && _hasPassiveSkill2)
            {
                _currentPassiveIndex++;
                UpdatePassiveDisplay();
            }
        }
        else
        {
            // Xử lý cho normal skill
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
        }
    }
}