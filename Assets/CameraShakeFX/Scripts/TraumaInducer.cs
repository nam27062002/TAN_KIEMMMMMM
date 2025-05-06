using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
 
public class TraumaInducer : SingletonMonoBehavior<TraumaInducer> 
{
    [Tooltip("Seconds to wait before trigerring the explosion particles and the trauma effect")]
    public float Delay = 1;
    [Tooltip("Maximum stress the effect can inflict upon objects Range([0,1])")]
    public float MaximumStress = 0.6f;
    [Tooltip("Maximum distance in which objects are affected by this TraumaInducer")]
    public float Range = 45;

    [Button("Shake")]
    public void Shake()
    {
        StartCoroutine(ShakeCoroutine());
    }
    
    public IEnumerator ShakeCoroutine()
    {
        yield return new WaitForSeconds(Delay);
        var targets = FindObjectsOfType<GameObject>();
        for(int i = 0; i < targets.Length; ++i)
        {
            var receiver = targets[i].GetComponent<StressReceiver>();
            if(receiver == null) continue;
            float distance = Vector3.Distance(transform.position, targets[i].transform.position);
            if(distance > Range) continue;
            float distance01 = Mathf.Clamp01(distance / Range);
            float stress = (1 - Mathf.Pow(distance01, 2)) * MaximumStress;
            receiver.InduceStress(stress);
        }
    }
}