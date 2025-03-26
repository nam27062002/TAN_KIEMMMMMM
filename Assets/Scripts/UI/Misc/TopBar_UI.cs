using System.Collections.Generic;
using UnityEngine;

public class TopBar_UI : MonoBehaviour
{
    [SerializeField] private Transform characterPool;
    [SerializeField] private GameObject avatarPrefab;
    [SerializeField] private float avatarSpacing = 80f;
    [SerializeField] private float focusedScale = 1f;
    [SerializeField] private float unfocusedScale = 0.7f;

    private readonly List<AVT_SpdUI> _avtSpdUI = new();

    private GameplayManager Gameplay => GameplayManager.Instance;

    public void Awake()
    {
        ClearAllChildren();
        GameManager.Instance.OnMainCharacterChanged += UpdateTopBar;
    }

    public void OnDestroy()
    {
        if (GameManager.HasInstance)
            GameManager.Instance.OnMainCharacterChanged -= UpdateTopBar;
    }

    private void UpdateTopBar()
    {
        TryInitUI();
        SetUI();
    }

    private void TryInitUI()
    {
        if (_avtSpdUI.Count == Gameplay.Characters.Count) return;
        _avtSpdUI.Clear();
        ClearAllChildren();
        foreach (var character in Gameplay.Characters)
        {
            if (character == null) continue;
            var go = Instantiate(avatarPrefab, characterPool);
            if (go == null) continue;
            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
            }
            var avtSpd = go.GetComponent<AVT_SpdUI>();
            if (avtSpd != null)
            {
                _avtSpdUI.Add(avtSpd);
            }
            else
            {
                Destroy(go);
            }
        }
    }

    private void SetUI()
    {
        var count = _avtSpdUI.Count;
        var centerIndex = (count % 2 == 0) ? count / 2 - 1 : count / 2;
        var startIndex = Gameplay.CurrentPlayerIndex - centerIndex;
        if (startIndex < 0)
        {
            startIndex += count;
        }
        var fixedY = 0f;
        if (_avtSpdUI.Count > 0)
        {
            fixedY = _avtSpdUI[0].GetComponent<RectTransform>().anchoredPosition.y;
        }
        var poolRect = characterPool.GetComponent<RectTransform>();
        var poolWidth = poolRect.rect.width;
        var totalGroupWidth = (count - 1) * avatarSpacing;
        var offset = (poolWidth - totalGroupWidth) / 2;
        for (var i = 0; i < count; i++)
        {
            var index = (startIndex + i) % count;
            var avatarRect = _avtSpdUI[index].GetComponent<RectTransform>();
            var targetPos = new Vector2(offset + i * avatarSpacing, fixedY);
            avatarRect.anchoredPosition = targetPos;
            var isFocused = Gameplay.Characters[index].IsMainCharacter;
            var targetScale = isFocused ? focusedScale : unfocusedScale;
            _avtSpdUI[index].transform.localScale = new Vector3(targetScale, targetScale, 1f);
            _avtSpdUI[index].SetupUI(Gameplay.Characters[index], this);
        }
    }

    private void ClearAllChildren()
    {
        foreach (Transform child in characterPool)
        {
            Destroy(child.gameObject);
        }
    }

    public void DestroyAvt(AVT_SpdUI avt)
    {
        if (avt == null || !_avtSpdUI.Remove(avt)) return;
        Destroy(avt.gameObject);
        SetUI();
    }
}