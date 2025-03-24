using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIFeedback : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Character character;
    private readonly List<EffectUI> _effectUIs = new();
    [Title("Effects")] [SerializeField] private EffectUI effectUI;
    [SerializeField] private RectTransform effectsPanel;
    private void Start()
    {
        feedbackText.gameObject.SetActiveIfNeeded(false);
    }
    
    public void ShowMessage(string message)
    {
        StartCoroutine(ShowDamageReceiveCoroutine(message));
    }
    
    private IEnumerator ShowDamageReceiveCoroutine(string message)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        yield return new WaitForSeconds(1f);
        feedbackText.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        UpdateEffect();
    }

    private void UpdateEffect()
    {
        if (character == null) return;
        if (character.Info == null) return;
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
            if (effectIcon == null) effectIcon = UIManager.Instance.defaultIcon;
            cpn.Initialize(effectIcon);
            _effectUIs.Add(cpn);
        }
    }
    
    private void OnValidate()
    {
        character ??= GetComponent<Character>();
    }
}