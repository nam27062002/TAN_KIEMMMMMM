using UnityEngine;
using UnityEngine.UI;

public class EffectUI : MonoBehaviour
{
    [SerializeField] private Image icon;

    public void Initialize(Sprite sprite)
    {
        icon.sprite = sprite;
    }
    
    public void DestroyEffect()
    {
        Destroy(gameObject);
    }
}