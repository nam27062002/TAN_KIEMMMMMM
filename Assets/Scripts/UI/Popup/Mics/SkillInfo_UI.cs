using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillInfo_UI : MonoBehaviour
{
    public Image skillImage;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI skillDescription;

    public void Setup(SkillInfo skillInfo)
    {
        skillImage.sprite = skillInfo.icon;
        skillName.text = skillInfo.name;
        skillDescription.text = skillInfo.description;
    }
}