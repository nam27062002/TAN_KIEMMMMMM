using UnityEngine;
using UnityEngine.UI;

public class EffectUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    public void Initialize(Sprite sprite)
    {
        if (sprite == null) return;
        icon.sprite = sprite;
        icon.SetNativeSize();

        AspectRatioFitter arf = icon.GetComponent<AspectRatioFitter>();
        if (arf == null)
        {
            arf = icon.gameObject.AddComponent<AspectRatioFitter>();
        }
        float spriteWidth = sprite.rect.width;
        float spriteHeight = sprite.rect.height;
        arf.aspectRatio = spriteWidth / spriteHeight;
        arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
    }
    
    public void DestroyEffect()
    {
        Destroy(gameObject);
    }
}