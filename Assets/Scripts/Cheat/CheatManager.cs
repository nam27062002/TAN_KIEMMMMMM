using UnityEngine;

public class CheatManager : SingletonMonoBehavior<CheatManager>
{
    [Header("Cheat Keys")]
    [SerializeField] private KeyCode alwaysCritKey = KeyCode.C;

    // Cheat States
    private bool alwaysCrit = false;
    private bool previousCritState = false;  // Thêm biến để theo dõi trạng thái trước đó

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Handle Always Crit key press
        if (Input.GetKeyDown(alwaysCritKey))
        {
            alwaysCrit = !alwaysCrit;
            Debug.Log($"Always Crit: {(alwaysCrit ? "ON" : "OFF")}");
        }

        // Chỉ cập nhật trạng thái crit khi trạng thái thay đổi
        if (alwaysCrit != previousCritState)
        {
            Roll.SetCriticalHit(alwaysCrit);
            previousCritState = alwaysCrit;
        }
    }

    // Public methods to activate cheat from other scripts
    public void SetAlwaysCrit(bool active)
    {
        if (alwaysCrit != active)
        {
            alwaysCrit = active;
            Debug.Log($"Always Crit: {(active ? "ON" : "OFF")}");
        }
    }

    public bool IsAlwaysCritActive()
    {
        return alwaysCrit;
    }
}