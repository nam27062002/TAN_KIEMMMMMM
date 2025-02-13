using UnityEngine;

public abstract class PassiveSkill : MonoBehaviour
{
    [SerializeField] protected Character character;
    
    public virtual void RegisterEvents()
    {
        
    }

    public virtual void UnregisterEvents()
    {
        
    }

    public virtual void OnTrigger()
    {
           
    }
    
    private void OnValidate()
    {
        character ??= GetComponent<Character>();
    }
}