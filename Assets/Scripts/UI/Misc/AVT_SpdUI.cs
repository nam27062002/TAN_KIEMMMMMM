using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class AVT_SpdUI : MonoBehaviour
{
    #region Serialized Fields
    [Title("UI Components")]
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject startObject;
    [SerializeField] private GameObject focusObject;

    [Title("Background Sprites")] 
    [SerializeField] private Sprite mainCharacterSprite;
    [SerializeField] private Sprite enemySprite;
    [SerializeField] private Sprite teamMemberSprite;
    #endregion

    #region Private Fields
    private Character _character;
    private TopBar_UI _topBar;
    #endregion

    #region Properties
    public Character Character => _character;
    #endregion

    #region Public Methods
    public void SetupUI(Character character, TopBar_UI topBar, bool isStartCharacter)
    {
        if (character == null)
        {
            Debug.LogError("Attempted to setup AVT_SpdUI with null character");
            return;
        }

        UnsubscribeFromCurrentCharacter();
        InitializeNewCharacter(character, topBar);
        UpdateVisuals(character, isStartCharacter);
    }
    #endregion

    #region Private Methods
    private void UnsubscribeFromCurrentCharacter()
    {
        if (_character != null)
        {
            _character.OnDeath -= HandleCharacterDeath;
        }
    }

    private void InitializeNewCharacter(Character character, TopBar_UI topBar)
    {
        _character = character;
        _topBar = topBar;
        _character.OnDeath += HandleCharacterDeath;
    }

    private void UpdateVisuals(Character character, bool isStartCharacter)
    {
        startObject.SetActiveIfNeeded(isStartCharacter);
        UpdateCharacterIcon(character);
        UpdateBackgroundSprite(character);
        UpdateFocusIndicator(character);
    }

    private void UpdateCharacterIcon(Character character)
    {
        icon.sprite = character.characterConfig.slideBarIcon;
    }

    private void UpdateBackgroundSprite(Character character)
    {
        background.sprite = GetBackgroundSpriteForCharacter(character);
    }

    private Sprite GetBackgroundSpriteForCharacter(Character character)
    {
        if (character.IsMainCharacter) return mainCharacterSprite;
        return character.Type == Type.AI ? enemySprite : teamMemberSprite;
    }

    private void UpdateFocusIndicator(Character character)
    {
        focusObject.SetActive(character.IsMainCharacter);
    }

    private void HandleCharacterDeath(object sender, Character character)
    {
        if (_topBar != null)
        {
            _topBar.DestroyAvt(this);
        }
        Destroy(gameObject);
    }
    #endregion

    #region Unity Lifecycle
    private void OnDestroy()
    {
        UnsubscribeFromCurrentCharacter();
    }
    #endregion
}