using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private GameObject bg;

    [Header("Typewriter Settings")]
    [SerializeField] private float typeSpeed = 0.05f;
    [SerializeField] private float autoNextDelay = 5f;
    [SerializeField] private bool autoNextEnabled = true; // Biến để bật/tắt tự động chuyển thoại

    [Header("Character Movement")]
    [SerializeField] private float moveDuration = 3f;

    private List<ConversationData.Data> _conversationData;
    private int _currentIndex = 0;
    private Coroutine _typeCoroutine;
    private Coroutine _autoNextCoroutine;
    private Action _onEndConversation;
    private Action<int> _onNextConversation;
    
    private Dictionary<Character, Coroutine> _activeMovements = new Dictionary<Character, Coroutine>();
    private Dictionary<Character, Vector3> _characterTargets = new Dictionary<Character, Vector3>();

    protected override bool ShowGreyBackground => false;

    #region Unity Lifecycle
    private void Start()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
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

            bool isOverUI = results.Any(r => r.gameObject == skipButton.gameObject || r.gameObject == nextButton.gameObject);
            
            if (!isOverUI)
            {
                OnNextButtonClicked();
            }
        }
    }
    #endregion

    #region Popup Implementation
    public override void Open(UIBaseParameters parameters = null)
    {
        gameObject.SetActiveIfNeeded(true);
        if (parameters is ConversationPopupParameters conversationParams)
        {
            InitializeConversation(conversationParams);
#if QUICK_CHECK
            EndConversation();
#endif
        }
    }

    public override void Close()
    {
        CleanupCharacters();
        gameObject.SetActiveIfNeeded(false);
    }
    #endregion

    #region Conversation Logic
    private void InitializeConversation(ConversationPopupParameters parameters)
    {
        _conversationData = parameters.Conversation;
        _currentIndex = 0;
        _onEndConversation = parameters.OnEndConversation;
        _onNextConversation = parameters.OnNextConversation;
        DisplayCurrentDialogue();
    }

    private void DisplayCurrentDialogue()
    {
        if (_currentIndex >= _conversationData.Count)
        {
            EndConversation();
            return;
        }

        var data = _conversationData[_currentIndex];
        SetupAvatar(data);
        HandleCharacterSpawning(data);

        if (data.text == "")
        {
            bg.SetActiveIfNeeded(false); 
            StartCoroutine(AutoSkipCoroutine());
        }
        else
        {
            // Nếu có text, hiển thị text và áp dụng logic hiện tại
            StartTextAnimation(data);
            if (autoNextEnabled)
            {
                StartAutoNext();
            }
        }
        
        if (data.shake) 
            TraumaInducer.Instance.Shake();
    }

    private void SetupAvatar(ConversationData.Data data)
    {
        avatarImage.enabled = data.useAvt;
        if (data.useAvt) avatarImage.sprite = data.avatar;
    }

    private void HandleCharacterSpawning(ConversationData.Data data)
    {
        if (!data.hasSpawnCharacter) return;

        foreach (var spawnData in data.spawnCharacters)
        {
            var character = InstantiateCharacter(spawnData);
            GameplayManager.Instance.charactersInConversation.Add(character);
            character.HideHpBar();
            if (spawnData.canMove)
            {
                character.AnimationData.PlayAnimation(AnimationParameterNameType.MoveRight);
                StartCharacterMovement(character.transform, spawnData.targetPosition);
            }
        }
    }

    private Character InstantiateCharacter(ConversationData.SpawnCharacter spawnData)
    {
        var instance = Instantiate(spawnData.character.gameObject, spawnData.position, Quaternion.identity);
        var character = instance.GetComponent<Character>();
        
        if (spawnData.facingType == FacingType.Left)
        {
            character.transform.localScale = new Vector3(-1, 1, 1);
        }
        
        return character;
    }

    private void StartCharacterMovement(Transform characterTransform, Vector3 targetPosition)
    {
        var character = characterTransform.GetComponent<Character>();
        if (_activeMovements.TryGetValue(character, out var movement))
        {
            StopCoroutine(movement);
        }

        Coroutine movementCoroutine = StartCoroutine(MoveCharacterCoroutine(
            characterTransform,
            targetPosition,
            moveDuration
        ));
        
        _activeMovements[character] = movementCoroutine;
        _characterTargets[character] = targetPosition;
    }

    private IEnumerator MoveCharacterCoroutine(Transform transform, Vector3 target, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = target;
        transform.TryGetComponent(out Character character);
        character.AnimationData.PlayAnimation(AnimationParameterNameType.Idle);
        _activeMovements.Remove(transform.GetComponent<Character>());
    }

    private void StartTextAnimation(ConversationData.Data data)
    {
        bg.SetActiveIfNeeded(data.text != "");
        conversationText.text = "";
        
        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        _typeCoroutine = StartCoroutine(TypeTextCoroutine(data.text));
    }

    private IEnumerator TypeTextCoroutine(string text)
    {
        foreach (char c in text)
        {
            conversationText.text += c;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }
        _typeCoroutine = null;
    }

    private void StartAutoNext()
    {
        if (_autoNextCoroutine != null) StopCoroutine(_autoNextCoroutine);
        _autoNextCoroutine = StartCoroutine(AutoNextCoroutine());
    }

    private IEnumerator AutoNextCoroutine()
    {
        yield return new WaitForSecondsRealtime(autoNextDelay);
        OnNextButtonClicked();
    }

    // Coroutine mới để xử lý skip tự động sau 2 giây nếu không có text
    private IEnumerator AutoSkipCoroutine()
    {
        yield return new WaitForSecondsRealtime(2f); // Đợi 2 giây
        OnNextButtonClicked(); // Chuyển sang đoạn hội thoại tiếp theo
    }
    #endregion

    #region Button Handlers
    private void OnNextButtonClicked()
    {
        if (_typeCoroutine != null)
        {
            ForceFinishTyping();
            return;
        }

        _currentIndex++;
        _onNextConversation?.Invoke(_currentIndex);
        DisplayCurrentDialogue();
    }

    private void OnSkipButtonClicked()
    {
        ForceFinishAll();
        EndConversation();
    }

    private void ForceFinishTyping()
    {
        if (_typeCoroutine != null)
        {
            StopCoroutine(_typeCoroutine);
            _typeCoroutine = null;
            conversationText.text = _conversationData[_currentIndex].text;
        }
    }

    private void ForceFinishAll()
    {
        ForceFinishTyping();
        InstantCompleteMovements();
        _onNextConversation?.Invoke(_conversationData.Count);
    }

    private void InstantCompleteMovements()
    {
        foreach (var pair in _activeMovements)
        {
            if (pair.Value != null) StopCoroutine(pair.Value);
            pair.Key.transform.position = _characterTargets[pair.Key];
        }
        _activeMovements.Clear();
        _characterTargets.Clear();
    }
    #endregion

    #region Cleanup
    private void CleanupCharacters()
    {
        // foreach (var character in GameplayManager.Instance.charactersInConversation)
        // {
        //     if (character != null) Destroy(character.gameObject);
        // }
        // GameplayManager.Instance.charactersInConversation.Clear();
        InstantCompleteMovements();
    }

    private void EndConversation()
    {
        CleanupCharacters();
        Close();
        _onEndConversation?.Invoke();
    }
    #endregion
}