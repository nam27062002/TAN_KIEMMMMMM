using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionPointUI : MonoBehaviour
{
    public Sprite yellow;
    public Sprite red;
    public Sprite green;
        
    public List<Image> actionPoints;

    private readonly Dictionary<int, Sprite> _actionPointSprites = new Dictionary<int, Sprite>();

    public void SetActionPoints(List<int> points)
    {
        if (_actionPointSprites.Count == 0)
        {
            _actionPointSprites[1] = red;
            _actionPointSprites[2] = yellow;
            _actionPointSprites[3] = green;
        }
        points.Sort();
        points.Reverse();
        for (int i = 0; i < points.Count; i++)
        {
            actionPoints[i].sprite = _actionPointSprites[points[i]];
        }
    }
}