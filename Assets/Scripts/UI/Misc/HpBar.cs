using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class HpBar : ProcessBar
{
    public TextMeshProUGUI damageTakenText;

    private void Start()
    {
        damageTakenText.gameObject.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        StartCoroutine(ShowDamageReceiveCoroutine(message));
    }
    
    private IEnumerator ShowDamageReceiveCoroutine(string message)
    {
        damageTakenText.gameObject.SetActive(true);
        damageTakenText.text = message;
        yield return new WaitForSeconds(1f);
        damageTakenText.gameObject.SetActive(false);
    }
}