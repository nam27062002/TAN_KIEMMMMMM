using System.Collections;
using TMPro;
using UnityEngine;

public class UIFeedback : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Character character;
    
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
    
    private void OnValidate()
    {
        character ??= GetComponent<Character>();
    }
}