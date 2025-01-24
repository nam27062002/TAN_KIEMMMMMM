using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour
{
    [TabGroup("Config"), SerializeField] private CellType cellType = CellType.Walkable;
    [TabGroup("Config"), SerializeField] private Vector2Int cellPosition;

    [TabGroup("Sprites"), SerializeField] private SpriteRenderer backgroundSprite;
    [TabGroup("Sprites"), SerializeField] private SpriteRenderer highlightSprite;
    [TabGroup("Sprites"), SerializeField] private SpriteRenderer withinAttackRangeSprite;
    [TabGroup("Sprites"), SerializeField] private SpriteRenderer withinMoveRangeSprite;
    public Action OnCharacterReached;

    public CellType CellType
    {
        get => cellType;
        set => cellType = value;
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

#if QUICK_CHECK
        transform.localScale = Vector3.one;
        backgroundSprite.color = new Color(backgroundSprite.color.r, backgroundSprite.color.g, backgroundSprite.color.b, 1f);
        onComplete?.Invoke();
#else
        transform.localScale = Vector3.zero;
        backgroundSprite.color =
            new Color(backgroundSprite.color.r, backgroundSprite.color.g, backgroundSprite.color.b, 0f);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(Vector3.one, 0.5f).SetDelay(delay).SetEase(Ease.OutBack));
        sequence.Join(backgroundSprite.DOFade(1f, 0.5f).SetDelay(delay).SetEase(Ease.Linear));
        sequence.OnComplete(() => { onComplete?.Invoke(); });
#endif
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
        Character = character;
        cellType = CellType.Character;
    }

    private void OnMouseOver()
    {
        if ((!GameplayManager.Instance.IsTutorialLevel) && Input.GetMouseButtonDown(1))
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

    private void OnValidate()
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