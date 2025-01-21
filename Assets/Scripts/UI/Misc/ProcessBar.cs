using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProcessBar  : MonoBehaviour
{
    [SerializeField] private bool showText;
    [SerializeField] private Image progressBar;
    [SerializeField, ShowIf(nameof(showText))] private TextMeshProUGUI processText;
    
    private void Awake()
    {
        processText.gameObject.SetActiveIfNeeded(showText);
    }

    public virtual void SetValue(float value)
    {
        progressBar.fillAmount = value;
    }

    public virtual void SetValue(float value, string text)
    {
        SetValue(value);
        processText.text = text;
    }
}