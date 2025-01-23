using System;
using System.Collections;
using UnityEngine;

public abstract class TutorialSequence : MonoBehaviour
{
    public int index;
    protected TutorialManager Tutorial => TutorialManager.Instance;
    public Highlightable highlightable;
    
    public virtual void Start()
    {
        StartCoroutine(AddTutorial());
    }

    protected virtual void OnEnable()
    {
        StartCoroutine(AddTutorial());
    }

    protected virtual IEnumerator AddTutorial()
    {
        while (TutorialManager.Instance == null)
        {
            yield return null;
        }
        TutorialManager.Instance.AddTutorialClick(index, this);
        Debug.Log($"NT - Add tutorial index: {index}");
    }
    
    protected bool CanClick()
    {
        if (TutorialManager.Instance == null) return false;
        return TutorialManager.Instance?.tutorialIndex == index;
    }
    
    protected void OnFinishTutorial()
    {
        TutorialManager.Instance?.OnTutorialClicked(index);
    }
    
    
    public abstract void PrepareTutorial();
}