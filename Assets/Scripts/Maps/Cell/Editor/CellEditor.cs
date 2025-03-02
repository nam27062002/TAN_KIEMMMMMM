#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cell))]
[CanEditMultipleObjects]
public class CellEditor : UnityEditor.Editor
{
    private static float lastClickTime = 0f;
    private const float DOUBLE_CLICK_TIME = 0.3f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); 
    }

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponentInParent<Cell>();
                if (cell != null)
                {
                    float currentTime = Time.realtimeSinceStartup;
                    if (currentTime - lastClickTime <= DOUBLE_CLICK_TIME)
                    {
                        Undo.RecordObject(cell, "Change Cell Type"); 
                        cell.CellType = CellType.CannotWalk;
                        cell.OnValidate();
                        EditorUtility.SetDirty(cell);
                        Debug.Log($"Cell at {cell.CellPosition} set to CannotWalk");
                    }
                    lastClickTime = currentTime;
                    Selection.activeGameObject = cell.gameObject;
                    e.Use(); 
                }
            }
        }
    }
}
#endif