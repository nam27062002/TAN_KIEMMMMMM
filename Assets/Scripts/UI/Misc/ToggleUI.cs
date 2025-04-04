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
    
    // Phương thức này sẽ được gọi mỗi khi có thay đổi trong Inspector
    private void OnValidate()
    {
        // Tự động tìm Toggle component nếu chưa được gán
        if (toggle == null)
            toggle = GetComponent<Toggle>();
            
        // Tự động tìm Image component nếu chưa được gán
        if (image == null && toggle != null)
            image = toggle.targetGraphic as Image;
            
        // Cập nhật sprite ngay trong Editor
        if (toggle != null && image != null)
        {
            image.sprite = toggle.isOn ? onSprite : offSprite;
            
            // Đảm bảo Unity cập nhật trong Inspector
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(image);
            #endif
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

