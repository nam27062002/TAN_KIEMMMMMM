using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

public class TutorialManager : SingletonMonoBehavior<TutorialManager>
{
    public SerializableDictionary<CharacterType, Character> charactersInTutorial;
    [SerializeField, ReadOnly] private SerializedDictionary<int, TutorialSequence> tutorialClickIndex = new();
    public TutorialConfig tutorialConfig;
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
        HUD.Instance.HideHUD();
        ShowFinalConversation();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void SpawnCharactersInTutorial()
    {
        foreach (var character in charactersInTutorial)
        {
            var go = Instantiate(character.Value.gameObject, gotoPoses[character.Key][0].transform.position, Quaternion.identity);
            var characterComponent = go.GetComponent<Character>();
            charactersDict[character.Key] = characterComponent;
            characterComponent.HideHpBar();
        }
    }

    private void StartTutorial()
    {
        // 1: di chuyển nhân vật vào map trong 3s
        float duration = 3f;
        MoveCharacter(duration);
        // 2: show hội thoại đầu tiên
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
        ConversationMenu.Instance.StartConversation(conversation_1, OnEndFirstConversation);
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
        Invoke(nameof(ShowSecondConversation), 1f);
    }

    private void ShowSecondConversation()
    {
        ConversationMenu.Instance.StartConversation(conversation_2, OnEndSecondConversation, OnNextConversation);
    }

    private void ShowFinalConversation()
    {
        ConversationMenu.Instance.StartConversation(conversation_3, OnEndFinal);
    }

    private void OnEndFinal()
    {
        HUD.Instance.ShowHUD();
        EndTuto = true;
        GameplayManager.Instance.characterManager.SetMainCharacter();
        StartCoroutine(ShowTutorial2());
    }

    private IEnumerator ShowTutorial2()
    {
        arrow.gameObject.SetActive(true);
        arrow.anchoredPosition = new Vector2(-494.5f, -460.7f);
        arrow.rotation = Quaternion.Euler(0f, 0f, 270f);
        MessageMenu.Instance.SetTutorialText("Sau một vòng, điểm hành động vàng chuyển thành màu xanh và có thể được sử dụng");
        yield return new WaitForSeconds(4f);
        MessageMenu.Instance.SetTutorialText("Điểm hành động đỏ chuyển thành màu vàng. Không thể được sử dụng");
        yield return new WaitForSeconds(4f);
        MessageMenu.Instance.HideTutorialText();
        GameplayManager.Instance.IsTutorialLevel = false;
        GameplayManager.Instance.characterManager.SetMainCharacter();
        Destroy(gameObject);
    }
    
    private void OnNextConversation(int index)
    {
        if (index == 1)
        {
            var character = GameplayManager.Instance.characterManager.GetCharacterByType(CharacterType.LyVoDanh);
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
        Debug.Log($"TutorialManager: OnTutorialClicked: {index}");
        tutorialIndex++;
        if (tutorialIndex < tutorialConfig.tutorials.Count)
            Invoke(nameof(SetTutorial), delayTime);
        else
        {
            Debug.Log("End Tutorial");
            EndFirstTutorial();
        }
    }

    private void EndFirstTutorial()
    {
        arrow.gameObject.SetActiveIfNeeded(false);
        MessageMenu.Instance.HideTutorialText();
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
            MessageMenu.Instance.SetTutorialText(tutorial.tutorialMenuText);
        }
        else
        {
            MessageMenu.Instance.HideTutorialText();
        }
    }
}