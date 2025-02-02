using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))] 
public class UI_Button : MonoBehaviour
{
    [Header("References")]
    public Button button;
    public TextMeshProUGUI label;
    
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        button ??= GetComponentInChildren<Button>();
        label ??= GetComponentInChildren<TextMeshProUGUI>();
    }
#endif
}