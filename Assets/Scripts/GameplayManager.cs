using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayManager : SingletonMonoBehavior<GameplayManager>
{
    [SerializeField] private LevelConfig levelConfig;  
    [SerializeField] private CharacterManager characterManager;
    [ShowInInspector, ReadOnly] public MapManager MapManager { get; private set; }
    public int CurrentRound { get; private set; } = 0;

    public event EventHandler OnNewRound;
    
    protected override void Awake()
    {
        base.Awake();
        DOTween.Init(false, false, LogBehaviour.ErrorsOnly);
        DOTween.SetTweensCapacity(500, 125);
    }
    
    private void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        CurrentRound = 0;
        HUD.Instance.SetLevelName(levelConfig.levelName);
        LoadMapGame();
    }
    
    private void LoadMapGame()
    {
        var go = Instantiate(levelConfig.mapPrefab, transform);
        MapManager = go.GetComponent<MapManager>();
        MapManager.Initialize();
    }

    public void LoadCharacter()
    {
        characterManager.Initialize(levelConfig.spawnerConfig, MapManager);
    }

    public void HandleNewRound()
    {
        CurrentRound++;
        OnNewRound?.Invoke(this, EventArgs.Empty);
        Debug.Log($"NT - Gameplay: round {CurrentRound}");
    }
}