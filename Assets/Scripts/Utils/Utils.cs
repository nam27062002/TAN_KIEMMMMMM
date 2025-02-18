using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static void SetActiveIfNeeded(this GameObject gameObject, bool active)
    {
        if (gameObject == null)
        {
            return;
        }
        if (gameObject.activeSelf != active)
        {
            gameObject.SetActive(active);
        }
    }
    
    public static Character FindNearestCharacter(Character origin, List<Character> targets)
    {
        if (targets == null || targets.Count == 0)
            return null;

        Character nearest = null;
        var minDistance = float.MaxValue;
        var originPosition = origin.transform.position;

        foreach (var target in targets)
        {
            if (target == null && origin.characterType == target.characterType)
                continue;
            var distance = Vector3.Distance(originPosition, target.transform.position);
            if (!(distance < minDistance)) continue;
            minDistance = distance;
            nearest = target;
        }

        return nearest;
    }
    
    public static GameObject FindChildByName(this GameObject parent, string childName)
    {
        if (parent == null)
        {
            //AlkawaDebug.LogWarning("Parent object is null.");
            return null;
        }
        
        foreach (Transform child in parent.transform)
        {
            if (child.name == childName)
            {
                return child.gameObject;
            }
            
            GameObject foundChild = FindChildByName(child.gameObject, childName);
            if (foundChild != null)
            {
                return foundChild;
            }
        }
        return null;
    }
    
    public static int RoundNumber(double number)
    {
        double integerPart = Math.Floor(number);
        
        double decimalPart = number - integerPart;
        if(decimalPart >= 0.5)
        {
            return (int)(integerPart + 1);
        }
        else
        {
            return (int)integerPart;
        }
    }
}