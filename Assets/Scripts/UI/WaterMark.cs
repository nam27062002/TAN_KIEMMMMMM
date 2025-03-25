using TMPro;
using UnityEngine;

public class WaterMark : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI versionText;

    private void Start()
    {
        string version = Application.version;
        versionText.text = $"Ver: {version}";
    }
}