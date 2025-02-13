using System;
using UnityEngine;

public class CharacterAnimationData : MonoBehaviour
{
    public SerializableDictionary<AnimationParameterNameType, int> animationParameterHash;
    [SerializeField] private Animator anim;
    private AnimationParameterNameType _lastParameter = AnimationParameterNameType.None;
        
    private Action _endAction;

    public void PlayAnimation(AnimationParameterNameType animationParameterNameType, Action callback = null)
    {
        if (_lastParameter == animationParameterNameType || anim == null) return;
        if (_lastParameter != AnimationParameterNameType.None)
        {
            anim.SetBool(_lastParameter.ToString(), false);
        }
        anim.SetBool(animationParameterNameType.ToString(), true);
        _endAction = callback;
        _lastParameter = animationParameterNameType;
        AlkawaDebug.Log(ELogCategory.ANIMATION, $"Play animation: {animationParameterNameType}");
    }

    public void OnEndAnimation()
    {
        _endAction?.Invoke();
        PlayAnimation(AnimationParameterNameType.Idle);
    }
    
    private void SetAnimationParameterHash()
    {
        animationParameterHash.Clear();
        foreach (AnimationParameterNameType type in Enum.GetValues(typeof(AnimationParameterNameType)))
        {
            animationParameterHash[type] = Animator.StringToHash(type.ToString());
        }
    }
    
    private void OnValidate()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        if (animationParameterHash == null || animationParameterHash.Count == 0)
        {
            SetAnimationParameterHash();
        }
    }
}

[Serializable]
public enum AnimationParameterNameType
{
    None = 0,
    Idle,
    MoveLeft,
    MoveRight,
    OnDamageTaken,
    Skill1,
    Skill2,
    Skill3,
    Skill4,
}