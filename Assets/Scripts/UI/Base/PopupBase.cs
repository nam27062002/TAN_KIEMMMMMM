using System;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public abstract class PopupBase : UIBase
{
    [Header("Popup Animation Settings")]
    [SerializeField] private float openDuration = 0.3f;
    [SerializeField] private float closeDuration = 0.2f;
    [SerializeField] private Ease openEase = Ease.OutBack;
    [SerializeField] private Ease closeEase = Ease.InBack;
    
    [SerializeField] private PopupType popupType;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform mainPanel;

    
    [Title("Close Popup Button")]
    [SerializeField] private Button closePopupButton;
    [SerializeField] private Vector3 originalScale = Vector3.one;
    private bool _isAnimating;
    
    public static event EventHandler OnOpen;
    public static event EventHandler OnClose;
    
    protected override string OnCloseMessage => $"Closed popup: {popupType}";
    protected override string OnOpenMessage => $"Opened popup: {popupType}";
    protected virtual bool ShowGreyBackground => true;

    protected override void RegisterEvents()
    {
        base.RegisterEvents();
        closePopupButton?.onClick.AddListener(Close);
    }

    protected override void UnregisterEvents()
    {
        base.UnregisterEvents();
        closePopupButton?.onClick.RemoveListener(Close);
    }
    
    public override void Open(UIBaseParameters parameters = null)
    {
        if (_isAnimating) return;
        base.Open(parameters);
        StartOpenAnimation();
        Time.timeScale = 0;
        GameplayManager.Instance.SetInteract(false);
    }
    
    public override void Close()
    {
        if (_isAnimating) return;
        StartCloseAnimation(() =>
        {
            OnClose?.Invoke(this, EventArgs.Empty);
            Time.timeScale = 1;
            base.Close();
            GameplayManager.Instance.SetInteract(true);
        });   
    }
    
    private void StartOpenAnimation()
    {
        _isAnimating = true;
        canvasGroup.alpha = 0f;
        mainPanel.localScale = originalScale * 0.7f;
        var openSequence = DOTween.Sequence()
            .SetUpdate(true)
            .Append(canvasGroup.DOFade(1f, openDuration))
            .Join(mainPanel.DOScale(originalScale, openDuration).SetEase(openEase))
            .OnComplete(() =>
            {
                _isAnimating = false;
                if (ShowGreyBackground) 
                    OnOpen?.Invoke(this, EventArgs.Empty);
            });
        openSequence.Play();
    }

    private void StartCloseAnimation(TweenCallback onComplete)
    {
        _isAnimating = true;
        var closeSequence = DOTween.Sequence()
            .SetUpdate(true) 
            .Append(canvasGroup.DOFade(0f, closeDuration))
            .Join(mainPanel.DOScale(originalScale * 0.7f, closeDuration).SetEase(closeEase))
            .OnComplete(() =>
            {
                _isAnimating = false;
                onComplete?.Invoke();
            });
        closeSequence.Play();
    }
    
    private void OnDisable()
    {
        mainPanel.localScale = originalScale;
        canvasGroup.alpha = 1f;
    }

    private void OnValidate()
    {
        if (mainPanel == null) mainPanel = GetComponent<RectTransform>(); 
        if (canvasGroup == null) canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }
}