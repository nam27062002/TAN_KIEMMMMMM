using System;
using UnityEngine;
using DG.Tweening;
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

    [SerializeField] private Vector3 originalScale = Vector3.one;
    private bool _isAnimating;
    
    public static event EventHandler OnOpen;
    public static event EventHandler OnClose;
    
    protected override string OnCloseMessage => $"Closed popup: {popupType}";
    protected override string OnOpenMessage => $"Opened popup: {popupType}";
    
    public override void Open(UIBaseParameters parameters = null)
    {
        if (_isAnimating) return;
        base.Open(parameters);
        StartOpenAnimation();
    }
    
    public override void Close()
    {
        if (_isAnimating) return;
        StartCloseAnimation(() =>
        {
            OnClose?.Invoke(this, EventArgs.Empty);
            base.Close();
        });   
    }
    
    private void StartOpenAnimation()
    {
        _isAnimating = true;
        
        canvasGroup.alpha = 0f;
        mainPanel.localScale = originalScale * 0.7f;

        Sequence openSequence = DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, openDuration))
            .Join(mainPanel.DOScale(originalScale, openDuration).SetEase(openEase))
            .OnComplete(() =>
            {
                _isAnimating = false;
                OnOpen?.Invoke(this, EventArgs.Empty);
            });

        openSequence.Play();
    }

    private void StartCloseAnimation(TweenCallback onComplete)
    {
        _isAnimating = true;

        Sequence closeSequence = DOTween.Sequence()
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