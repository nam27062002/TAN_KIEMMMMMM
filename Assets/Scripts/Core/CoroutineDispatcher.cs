using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class CoroutineWrapper
{
    private IEnumerator m_coroutine;

    public CoroutineWrapper(IEnumerator _coroutine)
    {
        m_coroutine = _coroutine;
    }

    public IEnumerator RunCoroutine()
    {
        yield return m_coroutine;
    }
}

public class CoroutineDispatcher : SingletonMonoBehavior<CoroutineDispatcher>
{
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    
    private readonly Queue<CoroutineWrapper> m_coroutines = new Queue<CoroutineWrapper>();
    
    private int m_pendingCoroutineCount = 0;
    private const int ConcurrentOperations = 1;
    
    public void Update()
    {
        if (CanRunCoroutine())
        {
            RunNextCoroutine();
        }
    }
    
    void RunNextCoroutine()
    {
        StartCoroutine(RunCoroutine(m_coroutines.Dequeue()));
    }

    IEnumerator RunCoroutine(CoroutineWrapper _coroutine)
    {
        m_pendingCoroutineCount++;
        yield return _coroutine.RunCoroutine();
        m_pendingCoroutineCount--;
    }

    private bool CanRunCoroutine()
    {
        return m_pendingCoroutineCount < ConcurrentOperations && m_coroutines.Count > 0;
    }
    
    private void AddCoroutineInternal(IEnumerator _coroutine)
    {
        m_coroutines.Enqueue(new CoroutineWrapper(_coroutine));
    }

    public static void QueueCoroutine(IEnumerator _coroutine)
    {
        if (!HasInstance)
            return;
        
        Instance.AddCoroutineInternal(_coroutine);
    }
    
    public static Coroutine RunCoroutine(IEnumerator _coroutine)
    {
        if (!HasInstance)
            return null;

        return Instance.StartCoroutine(_coroutine);
    }

    public static void CancelCoroutine(Coroutine _coroutine)
    {
        if (!HasInstance)
            return;

        Instance.StopCoroutine(_coroutine);
    }
    
    public static Coroutine Invoke(Action action, float delay)
    {
        if (!HasInstance)
            return null;

        return Instance.StartCoroutine(InvokeCoroutine(action, delay));
    }

    private static IEnumerator InvokeCoroutine(Action action, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        action?.Invoke();
    }
}
