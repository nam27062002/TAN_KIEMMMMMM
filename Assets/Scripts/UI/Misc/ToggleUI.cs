using UnityEngine;
using UnityEngine.UI;

public class ToggleUI : MonoBehaviour
{
    public Toggle toggle;
    public Image image;
    public Sprite onSprite;
    public Sprite offSprite;

    private void Awake()
    {
        // Đăng ký sự kiện cho toggle
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    private void OnEnable()
    {
        // Cập nhật sprite ban đầu dựa trên trạng thái toggle
        if (toggle != null && image != null)
        {
            image.sprite = toggle.isOn ? onSprite : offSprite;
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện khi component bị hủy
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }

    public void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            image.sprite = onSprite;
        }
        else
        {
            image.sprite = offSprite;
        }
    }
}

