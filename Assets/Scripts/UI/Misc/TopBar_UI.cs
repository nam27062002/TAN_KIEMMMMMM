using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TopBar_UI : MonoBehaviour
{
    [SerializeField] private Transform characterPool;
    [SerializeField] private GameObject avatarPrefab;
    [SerializeField] private float avatarSpacing = 80f;
    [SerializeField] private float focusedScale = 1f;
    [SerializeField] private float unfocusedScale = 0.7f;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Ease animationEase = Ease.OutBack;
    [SerializeField] private float bounceEffectDuration = 0.3f;
    [SerializeField] private float bounceHeight = 15f;
    [SerializeField] private Ease bounceEase = Ease.OutQuad;

    private readonly List<AVT_SpdUI> _avtSpdUI = new();
    private readonly List<Tween> _activeTweens = new();

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
        KillAllTweens();

        var count = _avtSpdUI.Count;
        if (count == 0) return;

        var startIndex = Gameplay.CurrentPlayerIndex;
        var fixedY = _avtSpdUI[0].GetComponent<RectTransform>().anchoredPosition.y;
        var poolRect = characterPool.GetComponent<RectTransform>();
        var poolWidth = poolRect.rect.width;
        var W = _avtSpdUI[0].GetComponent<RectTransform>().rect.width;
        var D = avatarSpacing - W * unfocusedScale;

        List<float> scales = new List<float>();
        for (int i = 0; i < count; i++)
        {
            int index = (startIndex + i) % count;
            bool isFocused = Gameplay.Characters[index].IsMainCharacter;
            float S = isFocused ? focusedScale : unfocusedScale;
            scales.Add(S);
        }

        float sumWidths = 0;
        for (int i = 0; i < count; i++)
        {
            sumWidths += W * scales[i];
        }
        float totalWidth = sumWidths + (count - 1) * D;
        float P = (poolWidth - totalWidth) / 2;

        float leftEdge = P;
        for (int i = 0; i < count; i++)
        {
            int index = (startIndex + i) % count;
            var avatar = _avtSpdUI[index];
            var rt = avatar.GetComponent<RectTransform>();
            float S = scales[i];
            float centerX = leftEdge + (W * S / 2);
            bool isMainCharacter = Gameplay.Characters[index].IsMainCharacter;

            Tween scaleTween = rt.DOScale(new Vector3(S, S, 1f), animationDuration)
                                .SetEase(animationEase);
            _activeTweens.Add(scaleTween);

            Tween posTween = rt.DOAnchorPos(new Vector2(centerX, fixedY), animationDuration)
                              .SetEase(animationEase);
            _activeTweens.Add(posTween);

            leftEdge += W * S + D;
            avatar.SetupUI(Gameplay.Characters[index], this, false);
        }
    }

    private void KillAllTweens()
    {
        foreach (var tween in _activeTweens)
        {
            if (tween != null && tween.IsActive())
            {
                tween.Kill();
            }
        }
        _activeTweens.Clear();
    }

    private void OnDisable()
    {
        KillAllTweens();
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