using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class LoadProcessPopup : PopupBase
{
    [Title("Load Process Popup")]
    public Transform content;
    public Progression progression;
    public RectTransform contentRect;
    public VerticalLayoutGroup contentVerticalLayoutGroup;
    public float height;
    public float spacing;

    public override void Open(UIBaseParameters parameters = null)
    {
        base.Open(parameters);
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = new Vector2(0, 0);
        contentVerticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
        contentVerticalLayoutGroup.spacing = spacing;

        var progressions = SaveLoadManager.Instance.levels;
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        int index = 0;
    
        foreach (var item in progressions)
        {
            var go = Instantiate(progression.gameObject, content);
            if (go.TryGetComponent(out Progression progressionComponent))
            {
                var datetime = item.SaveTime;
                string timeStr = datetime.ToString("HH:mm:ss");
                progressionComponent.Init(index, $"save_{index + 1}: {timeStr}");
            }
            index++;
        }
    
        if (progressions.Count > 0)
        {
            float contentHeight = height * progressions.Count + (progressions.Count - 1) * spacing;
            contentRect.sizeDelta = new Vector2(0, contentHeight); 
        }
        else
        {
            contentRect.sizeDelta = new Vector2(0, 0);
        }
    }
}