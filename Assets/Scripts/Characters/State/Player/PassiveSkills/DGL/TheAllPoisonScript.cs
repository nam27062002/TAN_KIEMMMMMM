using UnityEngine;

public class TheAllPoisonScript : PassiveSkill
{
    [SerializeField] private int hpPerParasite = 3;

    private int _venomousParasite = 0;

    private int _damageAccumulator = 0;

    public override void RegisterEvents()
    {
        base.RegisterEvents();
        character.CharacterInfo.OnHpChanged += OnHpChanged;
    }

    public override void UnregisterEvents()
    {
        base.UnregisterEvents();
        if (character.CharacterInfo != null)
        {
            character.CharacterInfo.OnHpChanged -= OnHpChanged;
        }
    }

    private void OnHpChanged(object sender, int value)
    {
        _damageAccumulator += value;
        int parasitesEarned = _damageAccumulator / hpPerParasite;
        _damageAccumulator %= hpPerParasite;
        if (parasitesEarned > 0)
        {
            _venomousParasite += parasitesEarned;
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{character.characterConfig.characterName}] Độc Điển: mất {value} máu => nhận {parasitesEarned} độc trùng");
        }
    }
}
