using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class TutorialManager : SingletonMonoBehavior<TutorialManager>
{
    public SerializableDictionary<CharacterType, Character> charactersInTutorial;
    [SerializeField, ReadOnly] private SerializedDictionary<int, TutorialSequence> tutorialClickIndex = new();
    [FormerlySerializedAs("tutorialConfigSO")] [SerializeField] private TutorialConfig tutorialConfig;
    public List<GotoPos> lvdGotos;
    public List<GotoPos> dglGotos;
    public List<GotoPos> tnGotos;
    public ConversationData conversation_1;
    public ConversationData conversation_2;
    public ConversationData conversation_3;
    public int tutorialIndex;
    private Dictionary<CharacterType, List<GotoPos>> gotoPoses = new();

    private Dictionary<CharacterType, Character> charactersDict = new();

    private int _footStep;
    public bool EndTuto;
    public RectTransform arrow;

    protected override void Awake()
    {
        base.Awake();

        gotoPoses[CharacterType.LyVoDanh] = lvdGotos;
        gotoPoses[CharacterType.DoanGiaLinh] = dglGotos;
        gotoPoses[CharacterType.ThietNhan] = tnGotos;

        SpawnCharactersInTutorial();

        GameplayManager.Instance.OnLoadCharacterFinished += OnLoadCharacterFinished;
    }

    public void AddTutorialClick(int key, TutorialSequence tutorialSequence)
    {
        tutorialClickIndex ??= new SerializedDictionary<int, TutorialSequence>();
        tutorialClickIndex[key] = tutorialSequence;
    }

    private void Start()
    {
        StartTutorial();
    }

    public void OnNewRound()
    {
        GameplayManager.Instance.IsTutorialLevel = true;
        ((UI_Ingame)UIManager.Instance.CurrentMenu).HideAllUI();
        ShowFinalConversation();
    }

    private void SpawnCharactersInTutorial()
    {
        foreach (var character in charactersInTutorial)
        {
            var go = Instantiate(character.Value.gameObject, gotoPoses[character.Key][0].transform.position,
                Quaternion.identity);
            var characterComponent = go.GetComponent<Character>();
            charactersDict[character.Key] = characterComponent;
            characterComponent.HideHpBar();
        }
    }

    private void StartTutorial()
    {
        float duration = 3f;
        MoveCharacter(duration);
        Invoke(nameof(ShowFirstConversation), duration);
    }

    private void MoveCharacter(float time)
    {
        _footStep++;
        if (_footStep < lvdGotos.Count)
        {
            foreach (var character in charactersDict)
            {
                character.Value.MoveCharacter(gotoPoses[character.Key][_footStep].transform.position, time);
            }
        }
    }

    private void ShowFirstConversation()
    {
        UIManager.Instance.OpenPopup(PopupType.Conversation, new ConversationPopupParameters()
        {
            Conversation = conversation_1.conversation,
            OnEndConversation = OnEndFirstConversation,
            OnNextConversation = null
        });
    }

    private void OnEndFirstConversation()
    {
        MoveCharacter(1);
        GameplayManager.Instance.LoadMapGame();
        Invoke(nameof(SetFacing), 1f);
    }

    private void SetFacing()
    {
        charactersDict[CharacterType.LyVoDanh].PlayAnim(AnimationParameterNameType.Idle);
        charactersDict[CharacterType.DoanGiaLinh].PlayAnim(AnimationParameterNameType.Idle);
        charactersDict[CharacterType.ThietNhan].PlayAnim(AnimationParameterNameType.Idle);
        charactersDict[CharacterType.ThietNhan].transform.localScale = new Vector3(-1, 1, 1);
    }

    private void OnLoadCharacterFinished(object sender, EventArgs e)
    {
        foreach (var character in charactersDict)
        {
            character.Value.DestroyCharacter();
        }

        ((UI_Ingame)UIManager.Instance.CurrentMenu).HideAllUI();
        GameplayManager.Instance.Characters[0].OnUnSelected();
        Invoke(nameof(ShowSecondConversation), 1f);
    }

    private void ShowSecondConversation()
    {
        UIManager.Instance.OpenPopup(PopupType.Conversation, new ConversationPopupParameters()
        {
            Conversation = conversation_2.conversation,
            OnEndConversation = OnEndSecondConversation,
            OnNextConversation = OnNextConversation
        });
    }

    private void ShowFinalConversation()
    {
        UIManager.Instance.OpenPopup(PopupType.Conversation, new ConversationPopupParameters()
        {
            Conversation = conversation_3.conversation,
            OnEndConversation = OnEndFinal,
        });
    }

    private void OnEndFinal()
    {
        // HUD.Instance.ShowHUD();
        EndTuto = true;
        GameplayManager.Instance.SetMainCharacter();
        StartCoroutine(ShowTutorial2());
    }

    private IEnumerator ShowTutorial2()
    {
        arrow.gameObject.SetActive(true);
        arrow.anchoredPosition = new Vector2(-494.5f, -460.7f);
        arrow.rotation = Quaternion.Euler(0f, 0f, 270f);
        UIManager.Instance.OpenPopup(PopupType.Message, new MessagePopupParameters()
        {
            Message = "Sau một vòng, điểm hành động vàng chuyển thành màu xanh và có thể được sử dụng",
        });
        yield return new WaitForSeconds(4f);
        UIManager.Instance.OpenPopup(PopupType.Message, new MessagePopupParameters()
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
            character.PlayAnim(AnimationParameterNameType.Skill1);
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

    public void OnTutorialClicked(int index, float delayTime = 0f)
    {
        if (index != tutorialIndex) return;
        AlkawaDebug.Log(ELogCategory.GAMEPLAY,$"TutorialManager: OnTutorialClicked: {index}");
        tutorialIndex++;
        if (tutorialIndex < tutorialConfig.tutorials.Count)
            Invoke(nameof(SetTutorial), delayTime);
        else
        {
            AlkawaDebug.Log(ELogCategory.GAMEPLAY,"End Tutorial");
            EndFirstTutorial();
        }
    }

    private void EndFirstTutorial()
    {
        arrow.gameObject.SetActiveIfNeeded(false);
        UIManager.Instance.TryClosePopup(PopupType.Message);
        GameplayManager.Instance.IsTutorialLevel = false;
    }

    private void SetTutorial()
    {
        if (!tutorialClickIndex.ContainsKey(tutorialIndex)) return;
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
            UIManager.Instance.OpenPopup(PopupType.Message, new MessagePopupParameters()
            {
                Message = tutorial.tutorialMenuText,
            });
        }
        else
        {
            UIManager.Instance.TryClosePopup(PopupType.Message);
        }
    }
}