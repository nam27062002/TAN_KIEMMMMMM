using TMPro;
using UnityEngine;
using DG.Tweening;

public class MessagePopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float fadeDuration = 1f;

        private CanvasGroup _canvasGroup;
        private Tween _currentTween;
        private string _pendingText;
        private bool _isFadingOut = false;
        public bool IsOpen { get; set; }
        
        protected void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _canvasGroup.alpha = 0;
        }

        public override void Open(UIBaseParameters parameters = null)
        {
            if (parameters is MessagePopupParameters messagePopupParameters)
            {
                if (_isFadingOut)
                {
                    _pendingText = messagePopupParameters.Message;
                    return;
                }

                if (gameObject.activeSelf && _canvasGroup.alpha > 0)
                {
                    _pendingText = messagePopupParameters.Message;
                    FadeOut(() =>
                    {
                        DisplayNewText(_pendingText);
                        _pendingText = null;
                    });
                }
                else
                {
                    DisplayNewText(messagePopupParameters.Message);
                }
            }
            
        }

        private void ShowTutorial()
        {
            gameObject.SetActive(true);
            FadeIn();
        }
        private void DisplayNewText(string str)
        {
            text.text = str;
            ShowTutorial();
        }

        private void FadeIn()
        {
            IsOpen = true;
            _currentTween?.Kill();
            _canvasGroup.alpha = 0;
            _currentTween = _canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true).OnComplete(() =>
            {
                _currentTween = null;
            });
        }

        private void FadeOut(System.Action onComplete = null)
        {
            IsOpen = false;
            _isFadingOut = true;
            _currentTween?.Kill();
            _isFadingOut = false;
            gameObject.SetActiveIfNeeded(false);
            _currentTween = null;
            onComplete?.Invoke();
        }

        public override void Close()
        {
            if (_isFadingOut || !gameObject.activeSelf)
                return;
            FadeOut();
        }

        public void ForceHideTutorialText()
        {
            _currentTween?.Kill();
            _canvasGroup.alpha = 0;
            gameObject.SetActive(false);
            _isFadingOut = false;
            _pendingText = null;
        }
    }