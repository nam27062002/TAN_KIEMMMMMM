using UnityEngine;

public class TheAllPoisonScript : PassiveSkill
{
    [SerializeField] private int hpPerParasite = 3;

    public int VenomousParasite
    {
        get; 
        set;
    } = 0;

    private int _damageAccumulator = 0;

    public override void RegisterEvents()
    {
        base.RegisterEvents();
        character.Info.OnHpChanged += OnHpChanged;
    }

    public override void UnregisterEvents()
    {
        base.UnregisterEvents();
        if (character.Info != null)
        {
            character.Info.OnHpChanged -= OnHpChanged;
        }
    }

    private void OnHpChanged(object sender, int value)
    {
        // Chỉ tích lũy khi mất máu (value < 0)
        if (value < 0)
        {
            _damageAccumulator += Mathf.Abs(value); // Lấy giá trị tuyệt đối để đảm bảo tích lũy đúng
            int parasitesEarned = _damageAccumulator / hpPerParasite;
            _damageAccumulator %= hpPerParasite;
            
            if (parasitesEarned > 0)
            {
                VenomousParasite += parasitesEarned;
                AlkawaDebug.Log(ELogCategory.SKILL, $"[{character.characterConfig.characterName}] The All Poison: lost {Mathf.Abs(value)} hp => gained {parasitesEarned} venomous parasites");
            }
        }
    }
}
