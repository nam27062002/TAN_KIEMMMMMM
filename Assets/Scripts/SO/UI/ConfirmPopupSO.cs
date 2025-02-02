using UnityEngine;

[CreateAssetMenu(fileName = "ConfirmPopup", menuName = "SO/UI/ConfirmPopup")]
public class ConfirmPopupSO : ScriptableObject
{
    public string title;
    public string message;
    public string confirmText;
    public string cancelText;
}