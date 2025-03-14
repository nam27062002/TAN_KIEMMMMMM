using UnityEngine;

public abstract class PassiveSkill : MonoBehaviour
{
    [SerializeField] protected Character character;
    
    public virtual void RegisterEvents()
    {
        character.OnDeath += OnDeath;
    }

    public virtual void UnregisterEvents()
    {
        character.OnDeath -= OnDeath;
    }

    private void OnDeath(object sender, Character _)
    {
        UnregisterEvents();
    }
    
    public virtual void OnTrigger()
    {
           
    }
    
    private void OnValidate()
    {
        character ??= GetComponent<Character>();
    }
}