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
    
    private IEnumerator AddTutorial()
    {
        while (TutorialManager.Instance == null)
        {
            yield return null;
        }
        TutorialManager.Instance.tutorialClickIndex.Add(index, this);
    }
    
    protected bool CanClick()
    {
        if (TutorialManager.Instance == null) return false;
        return TutorialManager.Instance?.tutorialIndex == index;
        return false;
    }
    
    protected void OnFinishTutorial()
    {
        TutorialManager.Instance?.OnTutorialClicked(index);
    }
    
    
    public abstract void PrepareTutorial();
}