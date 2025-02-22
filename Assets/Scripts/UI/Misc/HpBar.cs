using UnityEngine;
using UnityEngine.UI;

public class HpBar : ProcessBar
{
    [SerializeField] private Image shieldImage;
    public void SetShield(float value)
    {
        shieldImage.fillAmount = value;
    }
}