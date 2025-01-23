using TMPro;
using UnityEngine;
using DG.Tweening;

public class MessageMenu : SingletonMonoBehavior<MessageMenu>
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float fadeDuration = 1f;

        private CanvasGroup canvasGroup;
        private Tween currentTween;
        private string pendingText;
        private bool isFadingOut = false;
        public bool IsOpen { get; set; }
        
        protected override void Awake()
        {
            base.Awake();
            gameObject.SetActive(false);
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0;
        }

        public void SetTutorialText(string str)
        {
            if (isFadingOut)
            {
                pendingText = str;
                return;
            }

            if (gameObject.activeSelf && canvasGroup.alpha > 0)
            {
                pendingText = str;
                FadeOut(() =>
                {
                    DisplayNewText(pendingText);
                    pendingText = null;
                });
            }
            else
            {
                DisplayNewText(str);
            }
        }

        public void ShowTutorial()
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
            currentTween?.Kill();
            canvasGroup.alpha = 0;
            currentTween = canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true).OnComplete(() =>
            {
                currentTween = null;
            });
        }

        private void FadeOut(System.Action onComplete = null)
        {
            IsOpen = false;
            isFadingOut = true;
            currentTween?.Kill();
            currentTween = canvasGroup.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
            {
                isFadingOut = false;
                gameObject.SetActive(false);
                currentTween = null;

                onComplete?.Invoke();
            });
        }

        public void HideTutorialText()
        {
            if (isFadingOut || !gameObject.activeSelf)
                return;

            FadeOut();
        }

        public void ForceHideTutorialText()
        {
            currentTween?.Kill();
            canvasGroup.alpha = 0;
            gameObject.SetActive(false);
            isFadingOut = false;
            pendingText = null;
        }
    }