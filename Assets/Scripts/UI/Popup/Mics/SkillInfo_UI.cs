using System.Collections.Generic;
using Sirenix.OdinInspector;
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
        currentIndex = index;
        _skillIndex = skillIndex;
        SetupUI();
    }

    private void SetupUI()
    {
        var skillInfo = _skills[(SkillTurnType)currentIndex][_skillIndex];
        skillImage.sprite = skillInfo.icon;
        skillName.text = skillInfo.name;
        skillDescription.text = skillInfo.description;
        for (int i = 0; i < circles.Count; i++)
        {
            circles[i].sprite = i == currentIndex ? highlight : normal;
        }
        leftButton.gameObject.SetActiveIfNeeded(currentIndex != 0);
        rightButton.gameObject.SetActiveIfNeeded(currentIndex != circles.Count - 1);
    }
    
    private void OnLeftButtonClick()
    {
        currentIndex--;
        currentIndex = Mathf.Max(0, currentIndex);
        SetupUI();
    }

    private void OnRightButtonClick()
    {
        currentIndex++;
        currentIndex = Mathf.Min(circles.Count - 1, currentIndex);
        SetupUI();
    }
    
}