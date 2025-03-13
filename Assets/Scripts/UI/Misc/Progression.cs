using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Progression : MonoBehaviour
{
    public TextMeshProUGUI progressText;
    public Button button;
    public int index;

    public void Init(int index, string text)
    {
        this.index = index;
        progressText.text = text;
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }

    private void OnClick()
    {
        UIManager.Instance.TryClosePopup(PopupType.LoadProcess);
        GameManager.Instance.StartGameAtSaveSlot(index);
    }
}