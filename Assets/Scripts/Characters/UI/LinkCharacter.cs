using UnityEngine;

public class LinkCharacter : MonoBehaviour
{
    public LineRenderer lineRenderer;

    public void SetLine(Vector3 startPoint, Vector3 endPoint)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    public void ClearLine()
    {
        lineRenderer.positionCount = 0;
    }
}