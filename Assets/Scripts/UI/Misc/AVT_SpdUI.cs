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
    private Character _character;
    private TopBar_UI _topBar;
    
    public void SetupUI(Character character, TopBar_UI topBar)
    {
        _topBar = topBar;
        if (_character != null)
        {
            _character.OnDeath -= OnDeath;
        }
        _character = character;
        _character.OnDeath += OnDeath;
        icon.sprite = _character.characterConfig.slideBarIcon;
        background.sprite = character.IsMainCharacter ? main : (character.Type == Type.AI ? enemy : team);
        focusObject.SetActive(character.IsMainCharacter);
    }

    private void OnDeath()
    {
        _topBar.DestroyAvt(this);
        Destroy(gameObject);
    }
}