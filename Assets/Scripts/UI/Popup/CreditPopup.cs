using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CreditPopup : PopupBase
{
    [Header("Animation Settings")]
    [SerializeField] private float panelAnimationTime = 2f;
    [SerializeField] private float textFadeDuration = 1f;
    [SerializeField] private float delayBetweenCredits = 0.5f;
    [SerializeField] private float scrollDuration = 5f;

    [Header("UI References")]
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private TextMeshProUGUI creditText;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private List<string> credits = new List<string>();
    private Coroutine creditAnimationCoroutine;

    public override void Open(UIBaseParameters parameters = null)
    {
        base.Open(parameters);
        InitializeCredits();
        StartCoroutine(PlayCreditSequence());
    }

    public override void Close()
    {
        StopAllCoroutines();
        base.Close();
    }

    private void InitializeCredits()
    {
        creditText.text = "";
        contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, 100);
        scrollRect.normalizedPosition = Vector2.one;
    }

    private IEnumerator PlayCreditSequence()
    {
        yield return AnimatePanelHeight(0, 1080, panelAnimationTime);
        yield return PlayTextCredits();
        ClearText();
        yield return AutoScrollToBottom();
    }

    private IEnumerator AnimatePanelHeight(float startHeight, float endHeight, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float height = Mathf.Lerp(startHeight, endHeight, elapsed / duration);
            contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, height);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, endHeight);
    }

    private IEnumerator PlayTextCredits()
    {
        foreach (string credit in credits)
        {
            yield return TypewriterEffect(credit, textFadeDuration);
            yield return new WaitForSecondsRealtime(delayBetweenCredits);
        }
    }

    private IEnumerator TypewriterEffect(string text, float duration)
    {
        creditText.text = "";
        float charDelay = duration / text.Length;

        foreach (char c in text)
        {
            creditText.text += c;
            yield return new WaitForSecondsRealtime(charDelay);
        }
    }

    private void ClearText()
    {
        creditText.text = "";
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
    }

    private IEnumerator AutoScrollToBottom()
    {
        float elapsed = 0f;
        float startPos = scrollRect.verticalNormalizedPosition;
        
        while (elapsed < scrollDuration)
        {
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPos, 0f, elapsed / scrollDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        scrollRect.verticalNormalizedPosition = 0f;
    }
}