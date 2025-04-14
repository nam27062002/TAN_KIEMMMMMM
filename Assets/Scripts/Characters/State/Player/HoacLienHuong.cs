using System;
using System.Collections.Generic;
using UnityEngine;

public class HoacLienHuong : PlayerCharacter
{
    [SerializeField] private float goldenAPChance = 0.25f;
    [NonSerialized] public Cell CurrentShield;
    public Vector2Int CurrentShieldPosition; // Vị trí của shield để lưu trong save
    [SerializeField] private int additionalDamage = 0; // Sát thương cộng thêm theo vòng đấu
    [SerializeField] private int maxAdditionalDamage = 6; // Sát thương cộng thêm tối đa
    [SerializeField] private int critReduction = 0; // Giảm số cần để crit
    [SerializeField] private int maxCritReduction = 3; // Giảm số cần để crit tối đa
    
    private bool _hasAppliedSelfDamageThisSkill = false; // Cờ đánh dấu đã gọi ApplySelfDamageIfMaxDamage
    
    // Thêm biến để theo dõi đồng minh được gắn shield
    private Character _linkedTeammate;
    
    protected override void SetStateMachine()
    {
        StateMachine = new CharacterStateMachine(this,
            new IdleState(this),
            new MoveState(this),
            new PlayerDamageTakenState(this),
            new HoacLienHuong_SkillState(this));
    }
    
    // Thêm phương thức đăng ký lắng nghe khi shield bị phá
    public void RegisterShieldBreakListener(Character linkedCharacter)
    {
        // Lưu đồng đội được liên kết
        _linkedTeammate = linkedCharacter;
        
        // Đăng ký sự kiện OnShieldChanged của nhân vật và đồng đội
        Info.OnShieldChanged += OnShieldStatusChanged;
        linkedCharacter.Info.OnShieldChanged += OnTeammateShieldStatusChanged;
        
        AlkawaDebug.Log(ELogCategory.SKILL, 
            $"[{characterConfig.characterName}] Đã kết nối shield với {linkedCharacter.characterConfig.characterName}");
    }
    
    // Xử lý khi shield của Hoắc Liên Hương thay đổi
    private void OnShieldStatusChanged(object sender, float value)
    {
        // Nếu shield về 0 và có đồng đội được liên kết
        if (value <= 0 && _linkedTeammate != null)
        {
            BreakLinkedShields();
        }
    }
    
    // Xử lý khi shield của đồng đội thay đổi
    private void OnTeammateShieldStatusChanged(object sender, float value)
    {
        // Nếu shield về 0 và có đồng đội được liên kết
        if (value <= 0 && _linkedTeammate != null)
        {
            BreakLinkedShields();
        }
    }
    
    // Xử lý việc phá hủy cả hai shield và hiệu ứng Long Giáp/Xà Giáp
    private void BreakLinkedShields()
    {
        if (_linkedTeammate == null) return;
        
        AlkawaDebug.Log(ELogCategory.SKILL, 
            $"[{characterConfig.characterName}] Shield bị vỡ - Xóa shield và hiệu ứng Long Giáp/Xà Giáp");
        
        // Xóa hiệu ứng shield của đồng đội
        _linkedTeammate.Info.RemoveAllEffect(EffectType.Shield);
        
        // Xóa hiệu ứng shield của bản thân
        Info.RemoveAllEffect(EffectType.Shield);
        
        // Xóa hiệu ứng Long Giáp của đồng đội
        _linkedTeammate.Info.RemoveAllEffect(EffectType.DragonArmor);
        
        // Xóa hiệu ứng Xà Giáp của bản thân
        Info.RemoveAllEffect(EffectType.SnakeArmor);
        
        // Thông báo
        UIManager.Instance.ShowNotification("Liên kết giáp bị phá vỡ!", 2f);
        
        // Hủy đăng ký sự kiện
        Info.OnShieldChanged -= OnShieldStatusChanged;
        _linkedTeammate.Info.OnShieldChanged -= OnTeammateShieldStatusChanged;
        
        // Xóa tham chiếu
        _linkedTeammate = null;
    }
    
    protected override void SetSpeed()
    {
        base.SetSpeed();
    }
    
    protected override void OnNewRound(object sender, EventArgs e)
    {
        base.OnNewRound(sender, e);
        
        // Tăng sát thương cộng thêm và giảm số cần để crit mỗi vòng đấu
        if (additionalDamage < maxAdditionalDamage)
        {
            additionalDamage += 2;
            if (additionalDamage > maxAdditionalDamage)
                additionalDamage = maxAdditionalDamage;
                
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{characterConfig.characterName}] Yêu Cung: Tăng sát thương cộng thêm lên {additionalDamage}");
        }
        
        if (critReduction < maxCritReduction)
        {
            critReduction += 1;
            if (critReduction > maxCritReduction)
                critReduction = maxCritReduction;
                
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{characterConfig.characterName}] Yêu Cung: Giảm số cần để crit xuống {critReduction}");
        }
    }
    
    protected override bool CanBlockSkill(DamageTakenParams damageTakenParams)
    {
        if (base.CanBlockSkill(damageTakenParams)) return true;
        var path = MapManager.FindShortestPath(damageTakenParams.SkillStateParams.Source.Info.Cell, SkillStateParams.TargetCell);
        if (path == null) return false;
        var canDodge = path.Count > damageTakenParams.SkillStateParams.SkillInfo.range;
        Debug.Log($"Khoảng cách hiện tại = {path.Count} | Khoảng cách skill = {damageTakenParams.SkillStateParams.SkillInfo.range} => né = {canDodge}");
        return canDodge;
    }
    
    // Thêm method để kiểm tra xem đã đạt tối đa chưa
    public bool IsMaxAdditionalDamage()
    {
        return additionalDamage >= maxAdditionalDamage;
    }
    
    // Thêm method để gây sát thương lên bản thân khi đạt tối đa
    public void ApplySelfDamageIfMaxDamage()
    {
        if (IsMaxAdditionalDamage() && !_hasAppliedSelfDamageThisSkill)
        {
            // Gây 1d4 sát thương lên bản thân
            int selfDamage = Roll.RollDice(1, 4, 0);
            Info.HandleDamageTaken(-selfDamage, this);
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{characterConfig.characterName}] Yêu Cung đạt tối đa: Hồi {selfDamage} máu (1d4)");
            
            // Đánh dấu đã gọi
            _hasAppliedSelfDamageThisSkill = true;
        }
    }
    
    // Reset cờ khi bắt đầu skill mới
    public void ResetSelfDamageFlag()
    {
        _hasAppliedSelfDamageThisSkill = false;
    }
    
    // Hàm để lấy sát thương cộng thêm cho các skill
    public int GetAdditionalDamage()
    {
        return additionalDamage;
    }
    
    // Hàm để lấy giảm số cần để crit
    public int GetCritReduction()
    {
        return critReduction;
    }
    
    public override int GetSkillActionPoints(SkillTurnType skillTurnType)
    {
        if (skillTurnType == SkillTurnType.EnemyTurn)
        {
            float roll = UnityEngine.Random.value;
            int ap = roll < 0.25f ? 1 : 2;
            AlkawaDebug.Log(ELogCategory.SKILL,$"Thân pháp: Lượt địch - Kết quả roll AP: {roll} => AP = {ap}");
            
            if(roll < goldenAPChance)
            {
                goldenAPChance = 0.25f;
                AlkawaDebug.Log(ELogCategory.SKILL,$"Thân pháp: Kích hoạt AP vàng! (Xác suất: {goldenAPChance}, Roll: {roll})");
                AlkawaDebug.Log(ELogCategory.SKILL,$"Thân pháp: Xác suất AP vàng được reset: {goldenAPChance}");
            }
            else
            {
                AlkawaDebug.Log(ELogCategory.SKILL,$"Thân pháp: Kích hoạt AP đỏ. (Xác suất: {goldenAPChance}, Roll: {roll})");
                goldenAPChance = Mathf.Min(goldenAPChance + 0.10f, 1f);
                AlkawaDebug.Log(ELogCategory.SKILL,$"Thân pháp: Xác suất AP vàng tăng lên: {goldenAPChance}");
            }
            return ap;
        }
        
        return base.GetSkillActionPoints(skillTurnType);
    }
}