using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class AutoResizeBackground : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    public float paddingTop = 10f;
    public float paddingBottom = 10f;

    private RectTransform _backgroundRect;

    void Start()
    {
        _backgroundRect = GetComponent<RectTransform>();
        UpdateBackgroundSize();
    }

    void Update()
    {
        UpdateBackgroundSize();
    }

    void UpdateBackgroundSize()
    {
        float textHeight = targetText.preferredHeight;
        _backgroundRect.sizeDelta = new Vector2(
            _backgroundRect.sizeDelta.x, 
            textHeight + paddingTop + paddingBottom
        );
    }
}