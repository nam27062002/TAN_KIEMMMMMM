using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TopBar_UI : MonoBehaviour
{
    [SerializeField] private Transform characterPool;
    [SerializeField] private GameObject avatarPrefab;
    [SerializeField] private float avatarSpacing = 80f;
    [SerializeField] private float focusedScale = 1f;
    [SerializeField] private float unfocusedScale = 0.7f;

    private readonly List<AVT_SpdUI> _avtSpdUI = new();
    private GameplayManager Gameplay => GameplayManager.Instance;

    #region Unity Lifecycle
    private void Awake()
    {
        ClearAllChildren();
        GameManager.Instance.OnMainCharacterChanged += UpdateTopBar;
    }

    private void OnDestroy()
    {
        if (GameManager.HasInstance)
            GameManager.Instance.OnMainCharacterChanged -= UpdateTopBar;
    }
    #endregion

    #region Public Methods
    public void DestroyAvt(AVT_SpdUI avt)
    {
        if (avt == null || !_avtSpdUI.Remove(avt)) return;
        Destroy(avt.gameObject);
        SetUI();
    }
    #endregion

    #region Private Methods
    private void UpdateTopBar()
    {
        TryInitUI();
        SetUI();
    }

    private void TryInitUI()
    {
        try 
        {
            ValidateRequiredComponents();
            if (_avtSpdUI.Count == Gameplay.Characters.Count) return;
            
            ReinitializeAvatars();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void ValidateRequiredComponents()
    {
        if (Gameplay == null)
            throw new System.NullReferenceException("GameplayManager.Instance is null");

        if (Gameplay.Characters == null)
            throw new System.NullReferenceException("GameplayManager.Characters is null");

        if (characterPool == null)
            throw new System.NullReferenceException("characterPool Transform is null");

        if (avatarPrefab == null)
            throw new System.NullReferenceException("avatarPrefab is null");

        if (_avtSpdUI == null)
            throw new System.NullReferenceException("_avtSpdUI List is null");
    }

    private void ReinitializeAvatars()
    {
        _avtSpdUI.Clear();
        ClearAllChildren();
        
        foreach (var character in Gameplay.Characters)
        {
            if (character == null)
            {
                Debug.LogException(new System.NullReferenceException("Character in Characters list is null"));
                continue;
            }
            
            CreateAvatarForCharacter(character);
        }
    }

    private void CreateAvatarForCharacter(Character character)
    {
        var go = Instantiate(avatarPrefab, characterPool);
        if (!ValidateAvatarGameObject(go)) return;

        var rt = SetupRectTransform(go);
        if (rt == null) return;

        var avtSpd = go.GetComponent<AVT_SpdUI>();
        if (avtSpd == null)
        {
            Debug.LogException(new System.Exception($"AVT_SpdUI component missing on {go.name}"));
            Destroy(go);
            return;
        }

        _avtSpdUI.Add(avtSpd);
    }

    private bool ValidateAvatarGameObject(GameObject go)
    {
        if (go != null) return true;
        
        Debug.LogException(new System.Exception("Failed to instantiate avatarPrefab"));
        return false;
    }

    private RectTransform SetupRectTransform(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null)
        {
            Debug.LogException(new System.Exception($"RectTransform component missing on {go.name}"));
            Destroy(go);
            return null;
        }

        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        return rt;
    }

    private void SetUI()
    {
        try
        {
            if (!ValidateUISetup(out var count)) return;

            ReorderAvatars();

            var currentIndex = Gameplay.CurrentPlayerIndex;
            var startIndex = currentIndex;
            
            if (currentIndex >= count)
            {
                startIndex = currentIndex % count;
                Debug.Log($"Điều chỉnh startIndex từ {currentIndex} thành {startIndex} (count: {count})");
            }

            var layoutInfo = CalculateLayoutInfo(count);
            UpdateAvatarPositions(count, startIndex, layoutInfo);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    private bool ValidateUISetup(out int count)
    {
        count = _avtSpdUI?.Count ?? 0;
        if (count == 0) return false;

        ValidateRequiredComponents();
        return true;
    }

    private (float offset, float fixedY) CalculateLayoutInfo(int count)
    {
        var fixedY = _avtSpdUI[0]?.GetComponent<RectTransform>()?.anchoredPosition.y ?? 0f;
        var totalGroupWidth = (count - 1) * avatarSpacing;
        
        var poolRect = characterPool.GetComponent<RectTransform>();
        if (poolRect == null)
            throw new System.NullReferenceException("poolRect RectTransform is null");
            
        var poolWidth = poolRect.rect.width;
        var offset = (poolWidth - totalGroupWidth) / 2;

        return (offset, fixedY);
    }

    private void UpdateAvatarPositions(int count, int startIndex, (float offset, float fixedY) layoutInfo)
    {
        for (var i = 0; i < count; i++)
        {
            var index = (startIndex + i) % count;
            if (!ValidateAvatarIndex(index)) continue;

            UpdateSingleAvatarPosition(i, index, layoutInfo);
        }
    }

    private bool ValidateAvatarIndex(int index)
    {
        if (index >= _avtSpdUI.Count || _avtSpdUI[index] == null)
        {
            Debug.LogException(new System.IndexOutOfRangeException($"Invalid index {index} or null AVT_SpdUI"));
            return false;
        }

        if (index >= Gameplay.Characters.Count || Gameplay.Characters[index] == null)
        {
            Debug.LogException(new System.IndexOutOfRangeException($"Invalid character index {index} or null Character"));
            return false;
        }

        return true;
    }

    private void UpdateSingleAvatarPosition(int i, int index, (float offset, float fixedY) layoutInfo)
    {
        var avatarRect = _avtSpdUI[index].GetComponent<RectTransform>();
        if (avatarRect == null)
        {
            Debug.LogException(new System.NullReferenceException($"RectTransform missing on AVT_SpdUI at index {index}"));
            return;
        }

        var targetPos = new Vector2(layoutInfo.offset + i * avatarSpacing, layoutInfo.fixedY);
        avatarRect.anchoredPosition = targetPos;

        var isFocused = Gameplay.Characters[index].IsMainCharacter;
        var targetScale = isFocused ? focusedScale : unfocusedScale;
        _avtSpdUI[index].transform.localScale = new Vector3(targetScale, targetScale, 1f);
        _avtSpdUI[index].SetupUI(Gameplay.Characters[index], this, false);
    }

    private void ClearAllChildren()
    {
        foreach (Transform child in characterPool)
        {
            Destroy(child.gameObject);
        }
    }

    private void ReorderAvatars()
    {
        var newOrder = new List<AVT_SpdUI>();
        
        foreach (var character in Gameplay.Characters)
        {
            var avatar = _avtSpdUI.FirstOrDefault(avt => 
            {
                try 
                {
                    return avt != null && avt.Character != null && avt.Character == character;
                }
                catch (System.Exception)
                {
                    return false;
                }
            });

            if (avatar != null)
            {
                newOrder.Add(avatar);
            }
        }

        _avtSpdUI.Clear();
        _avtSpdUI.AddRange(newOrder);
    }
    #endregion
}