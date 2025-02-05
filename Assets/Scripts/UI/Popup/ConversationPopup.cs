using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

public class ConversationPopup : PopupBase
{
    [Title("Conversation Popup")]
    [SerializeField] private Image avatarImage;

    [SerializeField] private TextMeshProUGUI conversationText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    private List<ConversationData.Data> _conversationData = null;

    [Header("Typewriter Settings")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private float autoNextDelay = 5f;

    private int _currentIndex = 0;
    private Coroutine _typeCoroutine;
    private Coroutine _autoNextCoroutine;
    private Action _onEndConversation;
    private Action<int> _onNextConversation;
    protected override bool ShowGreyBackground => false;
    private void Start()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
    }
    
    public override void Open(UIBaseParameters parameters = null)
    {
        gameObject.SetActiveIfNeeded(true);
        if (parameters is ConversationPopupParameters conversationPopupParameters)
        {
            _conversationData = conversationPopupParameters.Conversation;
            _currentIndex = 0;
            DisplayCurrentDialogue();
            _onEndConversation = conversationPopupParameters.OnEndConversation;
            _onNextConversation = conversationPopupParameters.OnNextConversation;
// #if QUICK_CHECK
//             EndConversation();
// #endif
        }
    }
    
    private void DisplayCurrentDialogue()
    {
        if (_currentIndex < _conversationData.Count)
        {
            var data = _conversationData[_currentIndex];
            if (data.useAvt)
            {
                avatarImage.enabled = true;
                avatarImage.sprite = data.avatar;
            }
            else
            {
                avatarImage.enabled = false;
            }

            conversationText.text = "";
            if (_typeCoroutine != null)
            {
                StopCoroutine(_typeCoroutine);
            }

            _typeCoroutine = StartCoroutine(TypeText(data.text));

            // Reset auto-next coroutine
            if (_autoNextCoroutine != null)
            {
                StopCoroutine(_autoNextCoroutine);
            }

            _autoNextCoroutine = StartCoroutine(AutoNextAfterDelay(autoNextDelay));
        }
        else
        {
            EndConversation();
        }
    }

    private IEnumerator TypeText(string fullText)
    {
        foreach (char c in fullText)
        {
            conversationText.text += c;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }
    }

    private IEnumerator AutoNextAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        OnNextButtonClicked();
    }

    private void OnNextButtonClicked()
    {
        if (_typeCoroutine != null)
        {
            StopCoroutine(_typeCoroutine);
            _typeCoroutine = null;
            conversationText.text = _conversationData[_currentIndex].text;
        }

        _currentIndex++;
        _onNextConversation?.Invoke(_currentIndex);

        if (_currentIndex < _conversationData.Count)
        {
            DisplayCurrentDialogue();
        }
        else
        {
            EndConversation();
        }
    }

    private void OnSkipButtonClicked()
    {
        if (_autoNextCoroutine != null)
        {
            StopCoroutine(_autoNextCoroutine);
            _onNextConversation?.Invoke(_conversationData.Count - 1);
            _autoNextCoroutine = null;
        }

        EndConversation();
    }

    private void EndConversation()
    {
        if (_autoNextCoroutine != null)
        {
            StopCoroutine(_autoNextCoroutine);
            _autoNextCoroutine = null;
        }

        Close();
        _onEndConversation?.Invoke();
    }

    public override void Close()
    {
        gameObject.SetActiveIfNeeded(false);
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            bool isOverSkip = false;
            bool isOverNext = false;
            foreach (RaycastResult result in results)
            {
                if (result.gameObject == skipButton.gameObject)
                {
                    isOverSkip = true;
                    break;
                }
                if (result.gameObject == nextButton.gameObject)
                {
                    isOverNext = true;
                }
            }
            if (isOverSkip)
            {
                return;
            }
            if (isOverNext)
            {
                return;
            }
            OnNextButtonClicked();
        }
    }
}
