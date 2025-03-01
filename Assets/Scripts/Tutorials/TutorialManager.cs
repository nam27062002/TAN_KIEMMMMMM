using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class TutorialManager : SingletonMonoBehavior<TutorialManager>
{
    // Public fields
    public SerializableDictionary<CharacterType, Character> charactersInTutorial;
    [SerializeField, ReadOnly] private SerializedDictionary<int, TutorialSequence> tutorialClickIndex = new();
    [FormerlySerializedAs("tutorialConfigSO")]
    [SerializeField] private TutorialConfig tutorialConfig;
    public List<GotoPos> lvdGotos;
    public List<GotoPos> dglGotos;
    public List<GotoPos> tnGotos;
    public ConversationData conversation_1;
    public ConversationData conversation_2;
    public ConversationData conversation_3;
    public int tutorialIndex;
    public bool EndTuto;
    public RectTransform arrow;

    // Private fields
    private Dictionary<CharacterType, List<GotoPos>> gotoPoses = new();
    private Dictionary<CharacterType, Character> charactersDict = new();
    private int _footStep;

    #region Initialization

    protected override void Awake()
    {
        base.Awake();
        gotoPoses[CharacterType.LyVoDanh] = lvdGotos;
        gotoPoses[CharacterType.DoanGiaLinh] = dglGotos;
        gotoPoses[CharacterType.ThietNhan] = tnGotos;
        SpawnCharactersInTutorial();
        GameplayManager.Instance.OnLoadCharacterFinished += OnLoadCharacterFinished;
    }

    private void Start()
    {
        StartTutorial();
    }

    public void AddTutorialClick(int key, TutorialSequence tutorialSequence)
    {
        tutorialClickIndex ??= new SerializedDictionary<int, TutorialSequence>();
        tutorialClickIndex[key] = tutorialSequence;
    }

    public void OnNewRound()
    {
        GameplayManager.Instance.IsTutorialLevel = true;
        ((UI_Ingame)UIManager.Instance.CurrentMenu).HideAllUI();
        ShowFinalConversation();
    }

    private void SpawnCharactersInTutorial()
    {
        foreach (var kvp in charactersInTutorial)
        {
            CharacterType characterType = kvp.Key;
            Character characterPrefab = kvp.Value;
            Vector3 spawnPosition = gotoPoses[characterType][0].transform.position;

            var go = Instantiate(characterPrefab.gameObject, spawnPosition, Quaternion.identity);
            Character characterComponent = go.GetComponent<Character>();
            charactersDict[characterType] = characterComponent;
            characterComponent.HideHpBar();
            go.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    #endregion

    #region Character Movement & Conversations

    private void StartTutorial()
    {
        float duration = 3f;
        MoveCharacters(duration);
        Invoke(nameof(ShowFirstConversation), duration);
    }

    private void MoveCharacters(float duration)
    {
        _footStep++;
        if (_footStep < lvdGotos.Count)
        {
            foreach (var kvp in charactersDict)
            {
                CharacterType characterType = kvp.Key;
                Character character = kvp.Value;
                if (character != null && gotoPoses[characterType][_footStep] != null)
                {
                    MoveCharacter(character, gotoPoses[characterType][_footStep].transform.position, duration);
                }
            }
        }
    }

    public void MoveCharacter(Character character, Vector3 targetPos, float duration)
    {
        if (character == null || character.transform == null)
            return;
        
        AnimationParameterNameType animType = targetPos.x > transform.position.x
            ? AnimationParameterNameType.MoveRight
            : AnimationParameterNameType.MoveLeft;
        character.AnimationData.PlayAnimation(animType);

        character.transform.DOMove(targetPos, duration)
            .SetEase(Ease.Linear)
            .SetTarget(character);
    }

    private void ShowFirstConversation()
    {
        UIManager.Instance.OpenPopup(PopupType.Conversation, new ConversationPopupParameters
        {
            Conversation = conversation_1.conversation,
            OnEndConversation = OnEndFirstConversation,
            OnNextConversation = null
        });
    }

    private void OnEndFirstConversation()
    {
        MoveCharacters(1f);
        GameplayManager.Instance.LoadMapGame();
        Invoke(nameof(SetFacing), 1f);
    }

    private void SetFacing()
    {
        charactersDict[CharacterType.LyVoDanh].AnimationData.PlayAnimation(AnimationParameterNameType.Idle);
        charactersDict[CharacterType.DoanGiaLinh].AnimationData.PlayAnimation(AnimationParameterNameType.Idle);
        charactersDict[CharacterType.ThietNhan].AnimationData.PlayAnimation(AnimationParameterNameType.Idle);

        charactersDict[CharacterType.ThietNhan].transform.localScale = new Vector3(-1, 1, 1);
        charactersDict[CharacterType.LyVoDanh].transform.localScale = new Vector3(1, 1, 1);
        charactersDict[CharacterType.DoanGiaLinh].transform.localScale = new Vector3(1, 1, 1);
    }

    private void OnLoadCharacterFinished(object sender, EventArgs e)
    {
        GameplayManager.Instance.OnLoadCharacterFinished -= OnLoadCharacterFinished;

        // foreach (var kvp in charactersDict)
        // {
        //     DOTween.Kill(kvp.Value.transform);
        //     kvp.Value.DestroyCharacter();
        // }

        ((UI_Ingame)UIManager.Instance.CurrentMenu).HideAllUI();
        GameplayManager.Instance.Characters[0].OnUnSelected();
        Invoke(nameof(ShowSecondConversation), 1f);
    }

    private void ShowSecondConversation()
    {
        UIManager.Instance.OpenPopup(PopupType.Conversation, new ConversationPopupParameters
        {
            Conversation = conversation_2.conversation,
            OnEndConversation = OnEndSecondConversation,
            OnNextConversation = OnNextConversation
        });
    }

    private void ShowFinalConversation()
    {
        UIManager.Instance.OpenPopup(PopupType.Conversation, new ConversationPopupParameters
        {
            Conversation = conversation_3.conversation,
            OnEndConversation = OnEndFinal,
        });
    }

    private void OnEndFinal()
    {
        EndTuto = true;
        GameplayManager.Instance.SetMainCharacter();
        StartCoroutine(ShowTutorial2());
    }

    private IEnumerator ShowTutorial2()
    {
        arrow.gameObject.SetActive(true);
        arrow.anchoredPosition = new Vector2(-494.5f, -460.7f);
        arrow.rotation = Quaternion.Euler(0f, 0f, 270f);

        UIManager.Instance.OpenPopup(PopupType.Message, new MessagePopupParameters
        {
            Message = "Sau một vòng, điểm hành động vàng chuyển thành màu xanh và có thể được sử dụng",
        });
        yield return new WaitForSeconds(4f);

        UIManager.Instance.OpenPopup(PopupType.Message, new MessagePopupParameters
        {
            Message = "Điểm hành động đỏ chuyển thành màu vàng. Không thể được sử dụng",
        });
        yield return new WaitForSeconds(4f);

        UIManager.Instance.TryClosePopup(PopupType.Message);
        GameplayManager.Instance.IsTutorialLevel = false;
        GameplayManager.Instance.SetMainCharacter();
        Destroy(gameObject);
    }

    private void OnNextConversation(int index)
    {
        if (index == 1)
        {
            var character = GameplayManager.Instance.GetCharacterByType(CharacterType.LyVoDanh);
            character.AnimationData.PlayAnimation(AnimationParameterNameType.Skill1);
        }
    }

    private void OnEndSecondConversation()
    {
        Invoke(nameof(OnEndSecondConversationDelay), 1f);
    }

    private void OnEndSecondConversationDelay()
    {
        GameplayManager.Instance.HandleEndSecondConversation();
        SetTutorial();
    }

    #endregion

    #region Tutorial Handling

    public void OnTutorialClicked(int index, float delayTime = 0f)
    {
        if (index != tutorialIndex)
            return;

        AlkawaDebug.Log(ELogCategory.GAMEPLAY, $"TutorialManager: OnTutorialClicked: {index}");
        tutorialIndex++;

        if (tutorialIndex < tutorialConfig.tutorials.Count)
            Invoke(nameof(SetTutorial), delayTime);
        else
        {
            AlkawaDebug.Log(ELogCategory.GAMEPLAY, "End Tutorial");
            EndFirstTutorial();
        }
    }

    private void EndFirstTutorial()
    {
        arrow.gameObject.SetActiveIfNeeded(false);
        UIManager.Instance.TryClosePopup(PopupType.Message);
        GameplayManager.Instance.ShowLevelName();
        GameplayManager.Instance.IsTutorialLevel = false;
    }

    private void SetTutorial()
    {
        if (!tutorialClickIndex.ContainsKey(tutorialIndex))
            return;

        ApplyTutorial(tutorialConfig.tutorials[tutorialIndex]);
        tutorialClickIndex[tutorialIndex].PrepareTutorial();
    }

    private void ApplyTutorial(TutorialConfig.TutorialData tutorial)
    {
        if (tutorial.tutorialTypes.HasFlag(TutorialType.Arrow))
        {
            arrow.gameObject.SetActive(true);
            arrow.anchoredPosition = tutorial.arrowPosition;
            arrow.rotation = tutorial.arrowRotation;
        }
        else
        {
            arrow.gameObject.SetActive(false);
        }

        if (tutorial.tutorialTypes.HasFlag(TutorialType.Menu))
        {
            UIManager.Instance.OpenPopup(PopupType.Message, new MessagePopupParameters
            {
                Message = tutorial.tutorialMenuText,
            });
        }
        else
        {
            UIManager.Instance.TryClosePopup(PopupType.Message);
        }
    }

    #endregion
}
