using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditPopup : PopupBase
{
    [Header("Animation Settings")]
    [SerializeField] private float panelAnimationTime = 2f;
    [SerializeField] private float textFadeDuration = 1f;
    [SerializeField] private float delayBetweenCredits = 0.5f;
    [SerializeField] private float scrollDuration = 5f;
    [SerializeField] private Button closeCreditsButton;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject panel;
    [Header("UI References")]
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private TextMeshProUGUI creditText;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private List<string> credits = new List<string>();

    [SerializeField] private RectMask2D rectMask2D;
    private Coroutine creditAnimationCoroutine;

    public override void Open(UIBaseParameters parameters = null)
    {
        base.Open(parameters);
        InitializeCredits();
        StartCoroutine(PlayCreditSequence());
        rectMask2D.enabled = true;
        closeCreditsButton.gameObject.SetActiveIfNeeded(false);
        closeCreditsButton.onClick.AddListener(OpenPanel);
        replayButton.onClick.AddListener(OnReplayButtonClick);
        quitButton.onClick.AddListener(OnExitButtonClick);
    }

    public override void Close()
    {
        StopAllCoroutines();
        closeCreditsButton.onClick.RemoveListener(OpenPanel);
        quitButton.onClick.RemoveListener(OnExitButtonClick);
        base.Close();
    }

    private void OnReplayButtonClick()
    {
        GameplayManager.Instance.IsReplay = true;
        GameplayManager.Instance.DestroyGameplay();
        Close();
        AlkawaDebug.Log(ELogCategory.UI, "Replay button clicked");
    }
    
    private void OnExitButtonClick()
    {
        SceneLoader.LoadSceneAsync(ESceneType.MainMenu, LoadSceneMode.Additive);
        SceneLoader.UnloadSceneAsync(ESceneType.Game);
        UIManager.Instance.CloseCurrentMenu();
        Close();
        AlkawaDebug.Log(ELogCategory.UI, "Exit button clicked");
    }
    
    private void OpenPanel()
    {
        panel.SetActive(true);
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
            creditText.text = credit;
            // yield return TypewriterEffect(credit, textFadeDuration);
#if UNITY_EDITOR
            yield return new WaitForSecondsRealtime(0);
#else
            yield return new WaitForSecondsRealtime(delayBetweenCredits);
#endif
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
        closeCreditsButton.gameObject.SetActiveIfNeeded(true);
    }
}