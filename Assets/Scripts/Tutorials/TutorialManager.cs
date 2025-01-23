using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : SingletonMonoBehavior<TutorialManager>
{
    public SerializableDictionary<CharacterType, Character> charactersInTutorial;
    
    public List<GotoPos> lvdGotos;
    public List<GotoPos> dglGotos;
    public List<GotoPos> tnGotos;
    public ConversationData conversation_1;
    public ConversationData conversation_2;
    
    private Dictionary<CharacterType, List<GotoPos>> gotoPoses = new();

    private Dictionary<CharacterType, Character> charactersDict = new();

    private int _footStep;
    
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameplayManager.Instance.OnLoadCharacterFinished -= OnLoadCharacterFinished;
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
        charactersDict[CharacterType.LyVoDanh].ChangeState(ECharacterState.Idle);
        charactersDict[CharacterType.DoanGiaLinh].ChangeState(ECharacterState.Idle);
        charactersDict[CharacterType.ThietNhan].ChangeState(ECharacterState.Idle);
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

    private void OnNextConversation(int index)
    {
        if (index == 2)
        {
            
        }
    }

    private void OnEndSecondConversation()
    {
        
    }
}