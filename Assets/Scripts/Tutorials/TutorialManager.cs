using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TutorialManager : SingletonMonoBehavior<TutorialManager>
{
    public SerializableDictionary<CharacterType, Character> charactersInTutorial;
    
    public List<GotoPos> lvdGotos;
    public List<GotoPos> dglGotos;
    public List<GotoPos> tnGotos;

    public Dictionary<CharacterType, List<GotoPos>> gotoPoses = new();

    public Dictionary<CharacterType, Character> charactersDict = new();

    private int _footStep;
    
    protected override void Awake()
    {
        base.Awake();
        
        gotoPoses[CharacterType.LyVoDanh] = lvdGotos;
        gotoPoses[CharacterType.DoanGiaLinh] = dglGotos;
        gotoPoses[CharacterType.ThietNhan] = tnGotos;
        
        SpawnCharactersInTutorial();
    }

    private void Start()
    {
        StartTutorial();
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
        }
    }

    private void StartTutorial()
    {
        // 1: di chuyển nhân vật vào map trong 3s
        MoveCharacter(3);
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
}