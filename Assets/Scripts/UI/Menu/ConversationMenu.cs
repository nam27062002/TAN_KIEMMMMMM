using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationMenu : SingletonMonoBehavior<ConversationMenu>
{
    [Header("UI Components")] [SerializeField]
    private Image avatarImage;

    [SerializeField] private TextMeshProUGUI conversationText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    private ConversationData _conversationData = null;

    [Header("Typewriter Settings")] [SerializeField]
    private float typeSpeed = 0.05f;

    [SerializeField] private float autoNextDelay = 5f;

    private int currentIndex = 0;
    private Coroutine typeCoroutine;
    private Coroutine autoNextCoroutine;
    private Action _onEndConversation;
    private Action<int> _onNextConversation;

    private void Start()
    {
        gameObject.SetActive(false);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
    }

    public void StartConversation(ConversationData data, Action OnEndConversation,
        Action<int> OnNextConversation = null)
    {
        gameObject.SetActive(true);
        _conversationData = data;
        currentIndex = 0;
        DisplayCurrentDialogue();
        _onEndConversation = OnEndConversation;
        _onNextConversation = OnNextConversation;
#if QUICK_CHECK
        EndConversation();
#endif
    }

    private void DisplayCurrentDialogue()
    {
        if (currentIndex < _conversationData.conversation.Count)
        {
            var data = _conversationData.conversation[currentIndex];
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
            if (typeCoroutine != null)
            {
                StopCoroutine(typeCoroutine);
            }

            typeCoroutine = StartCoroutine(TypeText(data.text));

            // Reset auto-next coroutine
            if (autoNextCoroutine != null)
            {
                StopCoroutine(autoNextCoroutine);
            }

            autoNextCoroutine = StartCoroutine(AutoNextAfterDelay(autoNextDelay));
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
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
            conversationText.text = _conversationData.conversation[currentIndex].text;
        }

        currentIndex++;
        _onNextConversation?.Invoke(currentIndex);

        if (currentIndex < _conversationData.conversation.Count)
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
        if (autoNextCoroutine != null)
        {
            StopCoroutine(autoNextCoroutine);
            _onNextConversation?.Invoke(_conversationData.conversation.Count - 1);
            autoNextCoroutine = null;
        }

        EndConversation();
    }

    private void EndConversation()
    {
        if (autoNextCoroutine != null)
        {
            StopCoroutine(autoNextCoroutine);
            autoNextCoroutine = null;
        }

        gameObject.SetActive(false);
        _onEndConversation?.Invoke();
    }
}