﻿using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[SelectionBase]
public class Cell : MonoBehaviour
{
    [TabGroup("Config"), SerializeField] private CellType cellType = CellType.Walkable;
    [TabGroup("Config"), SerializeField] private Vector2Int cellPosition;

    [TabGroup("Sprites"), SerializeField] private SpriteRenderer backgroundSprite;
    [TabGroup("Sprites"), SerializeField] private SpriteRenderer highlightSprite;
    [TabGroup("Sprites"), SerializeField] private SpriteRenderer withinAttackRangeSprite;
    [TabGroup("Sprites"), SerializeField] private SpriteRenderer withinMoveRangeSprite;
    [TabGroup("Sprites"), SerializeField] private SpriteRenderer shieldSprite;
    [TabGroup("Sprites"), SerializeField] private SpriteRenderer shieldImpactSprite;
    [TabGroup("Sprites"), SerializeField] public SpriteRenderer mainCharacterSprite;
    [TabGroup("Sprites"), SerializeField] public SpriteRenderer poisonousBloodPool;
    
    [TabGroup("Shield")] public Sprite shield_100_sprite;
    [TabGroup("Shield")] public Type shieldType;
    [TabGroup("Shield")] public Sprite shield_40_sprite;
    [TabGroup("Shield")] public int shieldHeath = 3;
    [TabGroup("Shield")] public int currentShieldHP = 0;
    [TabGroup("Shield")] public HpBar hpBar;
    [TabGroup("Shield"), ReadOnly] public Cell mainShieldCell;
    public Cell mainBlockProjectile;
    
    public CellType CellType
    {
        get => cellType;
        set => cellType = value;
    }

    private void Start()
    {
        shieldSprite.gameObject.SetActiveIfNeeded(false);
        GameplayManager.Instance.OnSetMainCharacter += InstanceOnOnSetMainCharacter;
    }

    private void OnDestroy()
    {
        if (GameplayManager.HasInstance)
            GameplayManager.Instance.OnSetMainCharacter -= InstanceOnOnSetMainCharacter;
    }

    private void InstanceOnOnSetMainCharacter(object sender, Cell e)
    {
        mainCharacterSprite.enabled = e == this;
    }

    public Vector2Int CellPosition
    {
        get => cellPosition;
        set => cellPosition = value;
    }

    public Character Character { get; set; }

    public void Initialize(float delay = 0f, Action onComplete = null)
    {
        if (cellType == CellType.CannotWalk)
        {
            onComplete?.Invoke();
            return;
        }

        transform.localScale = Vector3.zero;
        backgroundSprite.color =
            new Color(backgroundSprite.color.r, backgroundSprite.color.g, backgroundSprite.color.b, 0f);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(Vector3.one, 0.5f).SetDelay(delay).SetEase(Ease.OutBack));
        sequence.Join(backgroundSprite.DOFade(1f, 0.5f).SetDelay(delay).SetEase(Ease.Linear));
        sequence.OnComplete(() => { onComplete?.Invoke(); });
    }

    public void DestroyCell()
    {
        transform.DOKill();
        if (backgroundSprite != null) backgroundSprite.DOKill();
        if (highlightSprite != null) highlightSprite.DOKill();
        if (shieldSprite != null) shieldSprite.DOKill();
        if (withinAttackRangeSprite != null) withinAttackRangeSprite.DOKill();
        if (withinMoveRangeSprite != null) withinMoveRangeSprite.DOKill();
        if (shieldImpactSprite != null) shieldImpactSprite.DOKill();
        Destroy(gameObject);
    }

    public void SetShield(Type shieldType, int range)
    {
        this.shieldType = shieldType;
        shieldSprite.gameObject.SetActiveIfNeeded(true);
        shieldSprite.sprite = shield_100_sprite;
        currentShieldHP = shieldHeath;
        hpBar.SetValue(currentShieldHP * 1f / shieldHeath);
        var cells = GameplayManager.Instance.MapManager.GetAllHexagonInRange(this, range);
        foreach (var cell in cells)
        {
            cell.SetShieldImpact(this);
        }

        SetShieldImpact(this);
    }

    public void SetMainProjectile()
    {
        var cells = GameplayManager.Instance.MapManager.GetAllHexagonInRange(this, 4);
        foreach (var cell in cells)
        {
            cell.SetMainBlockProjectile(this);
        }

        SetMainBlockProjectile(this);
    }

    public void UnSetMainProjectile()
    {
        var cells = GameplayManager.Instance.MapManager.GetAllHexagonInRange(this, 4);
        foreach (var cell in cells)
        {
            cell.UnSetMainBlockProjectile(this);
        }

        UnSetMainBlockProjectile(this);
    }

    public void ReceiveDamage(Character to, Character from)
    {
        currentShieldHP--;
        hpBar.SetValue(currentShieldHP * 1f / shieldHeath);
        AlkawaDebug.Log(ELogCategory.EFFECT,
            $"Hiểu Nhật Quang Lâm: chặn sát thương cho {to.characterConfig.characterName} từ đòn đánh của {from.characterConfig.characterName}");
        if (currentShieldHP <= 0)
        {
            UnsetShieldImpact(3); // hard code
        }
    }

    public void UnsetShieldImpact(int range)
    {
        var cells = GameplayManager.Instance.MapManager.GetAllHexagonInRange(this, range);
        foreach (var cell in cells)
        {
            cell.UnsetShieldImpact(this);
        }

        UnsetShieldImpact(this);
        shieldSprite.gameObject.SetActiveIfNeeded(false);
        AlkawaDebug.Log(ELogCategory.EFFECT, $"Hiểu Nhật Quang Lâm: tháp bị phá hủy");
    }

    private void SetShieldImpact(Cell cell)
    {
        mainShieldCell = cell;
        shieldImpactSprite.enabled = true;
    }

    public void UnsetShieldImpact(Cell cell)
    {
        mainShieldCell = null;
        shieldImpactSprite.enabled = false;
    }

    private void SetMainBlockProjectile(Cell cell)
    {
        mainBlockProjectile = cell;
        shieldImpactSprite.enabled = true;
    }

    private void UnSetMainBlockProjectile(Cell cell)
    {
        mainBlockProjectile = null;
        shieldImpactSprite.enabled = false;
    }

    public void ShowFocus()
    {
        highlightSprite.enabled = true;
    }

    public void HideFocus()
    {
        highlightSprite.enabled = false;
    }

    public void ShowMoveRange()
    {
        withinMoveRangeSprite.enabled = true;
    }

    public void HideMoveRange()
    {
        withinMoveRangeSprite.enabled = false;
    }

    public void ShowSkillRange()
    {
        withinAttackRangeSprite.enabled = true;
    }

    public void HideSkillRange()
    {
        withinAttackRangeSprite.enabled = false;
    }

    public void OnCharacterRegister(Character character)
    {
        if (Character != null)
        {
            Character.OnDeath -= OnDeath;
        }
        Character = character;
        Character.OnDeath += OnDeath;
        cellType = CellType.Character;
    }

    private void OnDeath(object sender, Character character)
    {
        cellType = CellType.Walkable;
        Character = null;
    }

    private void OnMouseOver()
    {
        if ((!GameplayManager.Instance.IsTutorialLevel) && Input.GetMouseButtonDown(1) && 
            GameplayManager.Instance.CanInteract)
        {
            if (cellType == CellType.Character)
                GameplayManager.Instance.ShowInfo(Character);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (!GameplayManager.Instance.IsTutorialLevel) HandleCellClicked();
        }
    }

    public void HandleCellClicked()
    {
        GameplayManager.Instance.OnCellClicked(this);
    }

#if UNITY_EDITOR
    [HideInInspector] public string iconName;
    [HideInInspector] public bool hasIcon;

    public void ShowIcon(string icon)
    {
        iconName = icon;
        hasIcon = true;
    }

    public void HideIcon()
    {
        hasIcon = false;
    }

    public void OnValidate()
    {
        backgroundSprite.enabled = cellType != CellType.CannotWalk;
    }

    private void OnDrawGizmos()
    {
        if (!hasIcon) return;
        var pos = transform.position;
        pos.y += 0.25f;
        Gizmos.DrawIcon(pos, iconName, true);
    }
#endif
}