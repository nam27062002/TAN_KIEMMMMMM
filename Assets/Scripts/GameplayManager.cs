using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayManager : SingletonMonoBehavior<GameplayManager>
{
    [SerializeField] private LevelConfig levelConfig;  
    [SerializeField] private CharacterManager characterManager;
    [ShowInInspector, ReadOnly] public MapManager MapManager { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();
        DOTween.Init(false, false, LogBehaviour.ErrorsOnly);
        DOTween.SetTweensCapacity(500, 125);
    }
    
    private void Start()
    {
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
}