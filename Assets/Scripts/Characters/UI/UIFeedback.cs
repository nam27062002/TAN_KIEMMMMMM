using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFeedback : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Character character;
    private readonly List<EffectUI> _effectUIs = new();

    [Title("Effects")] [SerializeField] private Image bgImage;
    [SerializeField] private EffectUI effectUI;
    [SerializeField] private RectTransform effectsPanel;

    [Title("Colors")]
    [SerializeField] private Color normalDamageColor = Color.white;
    [SerializeField] private Color critDamageColor = Color.red;

    private void Start()
    {
        bgImage.enabled = false;
        feedbackText.gameObject.SetActive(false);
    }

    public void ShowMessage(string message, bool isCrit = false)
    {
        StartCoroutine(ShowDamageReceiveCoroutine(message, isCrit));
    }

    private IEnumerator ShowDamageReceiveCoroutine(string message, bool isCrit)
    {
        bool isNumeric = int.TryParse(message, out _);
        bgImage.enabled = isCrit || isNumeric;
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = isCrit ? critDamageColor : normalDamageColor;
        yield return new WaitForSeconds(1f);
        feedbackText.gameObject.SetActive(false);
        bgImage.enabled = false;
    }

    private void FixedUpdate()
    {
        UpdateEffect();
    }

    private void UpdateEffect()
    {
        if (character == null || character.Info == null) return;
        
        foreach (var item in _effectUIs)
        {
            item.DestroyEffect();
        }
        
        _effectUIs.Clear();
        
        if (character.Info.EffectInfo == null) return;
        
        foreach (var item in character.Info.EffectInfo.Effects)
        {
            var go = Instantiate(effectUI.gameObject, effectsPanel);
            go.transform.SetAsFirstSibling();
            var cpn = go.GetComponent<EffectUI>();
            UIManager.Instance.effectIcons.TryGetValue(item.effectType, out var effectIcon);
            effectIcon = effectIcon ?? UIManager.Instance.defaultIcon;
            cpn.Initialize(effectIcon);
            _effectUIs.Add(cpn);
        }
    }

    private void OnValidate()
    {
        character ??= GetComponent<Character>();
    }
}