using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionPointUI : MonoBehaviour
{
    public Sprite yellow;
    public Sprite red;
    public Sprite green;
    public List<Image> actionPoints;

    private readonly Dictionary<int, Sprite> _actionPointSprites = new();

    private void Awake()
    {
        _actionPointSprites[1] = red;
        _actionPointSprites[2] = yellow;
        _actionPointSprites[3] = green;
    }

    public void SetActionPoints(List<int> points)
    {
        foreach (var item in actionPoints)
        {
            item.gameObject.SetActiveIfNeeded(false);
        }
        
        points.Sort();
        points.Reverse();
        
        int maxPoints = Mathf.Min(points.Count, actionPoints.Count);
        for (int i = 0; i < maxPoints; i++)
        {
            if (_actionPointSprites.TryGetValue(points[i], out var sprite))
            {
                actionPoints[i].sprite = sprite;
                actionPoints[i].gameObject.SetActiveIfNeeded(true);
            }
        }
    }
}