using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFeedback : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private RectTransform effectsPanel;
    [SerializeField] private EffectUI effectUI;
    [SerializeField] private SerializableDictionary<EffectType, Sprite> effectIcons;
    [SerializeField] private Character character;
    
    private readonly List<EffectUI> _effectUIs = new();
    
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

    public void UpdateEffectIcons()
    {
        foreach (var item in _effectUIs)
        {
            item.DestroyEffect();
        }
        _effectUIs.Clear();
        foreach (var item in character.CharacterInfo.EffectInfo.Effects)
        {
            var go = Instantiate(effectUI.gameObject, effectsPanel);
            var cpn = go.GetComponent<EffectUI>();
            cpn.Initialize(effectIcons[item.EffectType]);
            _effectUIs.Add(cpn);
        }
    }

    private void OnValidate()
    {
        character ??= GetComponent<Character>();
    }
}