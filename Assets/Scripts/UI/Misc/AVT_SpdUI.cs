using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class AVT_SpdUI : MonoBehaviour
{
    [Title("Image")]
    public Image background;
    public Image icon;

    [Title("Sprite")] 
    public Sprite main;
    public Sprite enemy;
    public Sprite team;

    public GameObject focusObject;

    public void SetupUI(bool isFocused, Type characterType, Sprite iconSprite)
    {
        icon.sprite = iconSprite;
        background.sprite = isFocused ? main : (characterType == Type.AI ? enemy : team);
        focusObject.SetActive(isFocused);
    }
        
    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}