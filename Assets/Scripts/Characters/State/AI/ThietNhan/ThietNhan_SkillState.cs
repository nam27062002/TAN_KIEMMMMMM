using System.Collections.Generic;
using System.Linq;

public class ThietNhan_SkillState : AISkillState
{
    public ThietNhan_SkillState(Character character) : base(character)
    {
    }

    //===================== SKILL 2 =====================
    protected override DamageTakenParams GetDamageParams_Skill2_MyTurn(Character character)
    {
        int baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(1, isCrit);
        int skillDamage = Roll.RollDice(1, 4, 0, isCrit);
        int totalDamage = baseDamage + skillDamage;

        // Log theo định dạng chung
        string critInfo = isCrit ? " (CRIT)" : "";
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá: Base Damage = {baseDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá: Skill Formula = {rollTimes}d4{critInfo}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá: Skill Damage = {rollTimes}d4 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá: Total Damage = {baseDamage} + {skillDamage} = {totalDamage}");

        var friends = GpManager.MapManager.GetAllTypeInRange(Info.Cell, CharacterType.ThietNhan, 1);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá: Có {friends.Count} Thiết Nhân đứng sát");

        int beforeFriendDamage = totalDamage;
        totalDamage = ProcessFriendAttacks(friends, totalDamage);
        
        // Log tổng damage sau khi tính cả sát thương từ bạn bè
        if (totalDamage > beforeFriendDamage)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá: Final Damage = {beforeFriendDamage} (Own) + {totalDamage - beforeFriendDamage} (Friends) = {totalDamage}");
        }

        return new DamageTakenParams
        {
            Damage = totalDamage,
            Effects = new List<EffectData>
            {
                new RollEffectData
                {
                    effectType = EffectType.ThietNhan_Poison,
                    duration = EffectConfig.DebuffRound,
                    rollData = new RollData
                    {
                        rollTime = 1,
                        rollValue = 4,
                        add = 0
                    },
                    Actor = Character
                },
                new ChangeStatEffect
                {
                    effectType = EffectType.ThietNhan_ReduceMoveRange,
                    duration = EffectConfig.DebuffRound,
                    value = 2,
                    Actor = Character
                },
                new()
                {
                    effectType = EffectType.ThietNhan_BlockAP,
                    duration = EffectConfig.DebuffRound,
                    Actor = Character
                }
            },
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill2_EnemyTurn(Character character)
    {
        int baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(1, isCrit);
        int skillDamage = Roll.RollDice(1, 4, 0, isCrit);

        var friends = GpManager.MapManager.GetAllTypeInRange(Info.Cell, CharacterType.ThietNhan, 3);
        int friendBonus = friends.Count;
        skillDamage += friendBonus;
        int totalDamage = baseDamage + skillDamage;

        // Log theo định dạng chung
        string critInfo = isCrit ? " (CRIT)" : "";
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá (EnemyTurn): Base Damage = {baseDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá (EnemyTurn): Có {friends.Count} Thiết Nhân đứng cạnh trong 3 ô");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá (EnemyTurn): Skill Formula = {rollTimes}d4 + {friendBonus}{critInfo}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá (EnemyTurn): Skill Damage = {rollTimes}d4 + {friendBonus} = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá (EnemyTurn): Total Damage = {baseDamage} + {skillDamage} = {totalDamage}");

        int beforeFriendDamage = totalDamage;
        totalDamage = ProcessFriendAttacks(friends, totalDamage);
        
        // Log tổng damage sau khi tính cả sát thương từ bạn bè
        if (totalDamage > beforeFriendDamage)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Ném Đá (EnemyTurn): Final Damage = {beforeFriendDamage} (Own) + {totalDamage - beforeFriendDamage} (Friends) = {totalDamage}");
        }

        return new DamageTakenParams
        {
            Damage = totalDamage,
            Effects = new List<EffectData>
            {
                new()
                {
                    effectType = EffectType.Poison,
                    duration = EffectConfig.DebuffRound,
                    Actor = Character
                },
                new()
                {
                    effectType = EffectType.Immobilize,
                    duration = EffectConfig.DebuffRound,
                    Actor = Character
                }
            }
        };
    }


    //===================== SKILL 3 =====================
    protected override DamageTakenParams GetDamageParams_Skill3_MyTurn(Character character)
    {
        int baseDamage = GetBaseDamage();
        bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
        int rollTimes = Roll.GetActualRollTimes(1, isCrit);
        int skillDamage = Roll.RollDice(1, 4, 0, isCrit);
        int totalDamage = baseDamage + skillDamage;

        // Log theo định dạng chung
        string critInfo = isCrit ? " (CRIT)" : "";
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm: Base Damage = {baseDamage} (ATK = {Info.GetCurrentDamage()})");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm: Skill Formula = {rollTimes}d4{critInfo}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm: Skill Damage = {rollTimes}d4 = {skillDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm: Total Damage = {baseDamage} (Base) + {skillDamage} (Skill) = {totalDamage}");
            
        // Xử lý tấn công từ Thiết Nhân đứng cạnh (tương tự skill 2)
        var friends = GpManager.MapManager.GetAllTypeInRange(Info.Cell, CharacterType.ThietNhan, 1);
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm: Có {friends.Count} Thiết Nhân đứng sát");
        int beforeFriendDamage = totalDamage;
        totalDamage = ProcessFriendAttacks(friends, totalDamage);
        
        // Log tổng damage sau khi tính cả sát thương từ bạn bè
        if (totalDamage > beforeFriendDamage)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm: Final Damage = {beforeFriendDamage} (Own) + {totalDamage - beforeFriendDamage} (Friends) = {totalDamage}");
        }
        
        // Tạo hiệu ứng ThietNhan_Infected và đăng ký sự kiện
        var infectedEffect = new ThietNhanInfectedEffect()
        {
            effectType = EffectType.ThietNhan_Infected,
            duration = EffectConfig.DebuffRound,
            Actor = Character,
            rollsRemaining = 2, // Số lần roll để kiểm tra biến thành Thiết Nhân
        };
        
        // Đăng ký hook để xử lý kiểm tra nhiễm bệnh trong vòng mới
        GameplayManager.Instance.OnNewRound += (sender, args) => {
            // Kiểm tra xem hiệu ứng còn tồn tại và nhân vật còn sống
            if (character != null && character.Info != null && 
                character.Info.EffectInfo.Effects.Contains(infectedEffect) && 
                !character.Info.IsDie)
            {
                infectedEffect.CheckInfection(character, Character);
            }
        };
        
        return new DamageTakenParams()
        {
            Damage = totalDamage,
            Effects = new List<EffectData>()
            {
                infectedEffect
            },
            ReceiveFromCharacter = Character
        };
    }

    protected override DamageTakenParams GetDamageParams_Skill3_EnemyTurn(Character character)
    {
        // Chặn đòn đánh của đối thủ
        int baseDamage = GetBaseDamage();
        int bloodDamage = Roll.RollDice(2, 4, 0); // 2d4 sát thương từ máu độc
        
        // Log theo định dạng chung
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm (EnemyTurn): Base Damage = {baseDamage} (ATK = {Info.GetCurrentDamage()})");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm (EnemyTurn): Blood Damage Formula = 2d4");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm (EnemyTurn): Blood Damage = 2d4 = {bloodDamage}");
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm (EnemyTurn): Total Base Damage = {baseDamage}");
        
        // Tính toán vị trí máu độc
        Cell bloodPositionCell = CalculateBloodPosition(character);
        
        // Chuẩn bị hiệu ứng độc và bỏng
        var effects = new List<EffectData>
        {
            // Độc (Poison) - gây 2d4 sát thương mỗi vòng
            new RollEffectData
            {
                effectType = EffectType.Poison,
                duration = EffectConfig.DebuffRound,
                rollData = new RollData
                {
                    rollTime = 2,
                    rollValue = 4,
                    add = 0
                },
                Actor = Character
            }
        };
        
        AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm (EnemyTurn): DOT Poison = 2d4 mỗi vòng");
        
        // Tỉ lệ gây suy giảm khả năng nhìn (30% cơ hội)
        if (UnityEngine.Random.value <= 0.3f)
        {
            // Thay thế bằng hiệu ứng Fear hoặc hiệu ứng làm giảm HitChange
            effects.Add(new EffectData
            {
                effectType = EffectType.Fear,
                duration = 2, // Kéo dài 2 vòng
                Actor = Character
            });
            
            // Thêm hiệu ứng giảm chỉ số nhìn (chẳng hạn như giảm Hit Change)
            effects.Add(new ChangeStatEffect
            {
                effectType = EffectType.ReduceHitChange,
                duration = 2,
                value = 5, // Giảm 5 điểm Hit Change
                Actor = Character
            });
            
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm (EnemyTurn): Gây mù lòa (Fear + -5 Hit Change) cho {character.characterConfig.characterName}");
        }
        
        // Nếu máu độc xuất hiện ở vị trí có cell
        if (bloodPositionCell != null)
        {
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm (EnemyTurn): Máu độc xuất hiện tại {bloodPositionCell.CellPosition}");
            
            // Tạo pool máu độc nếu cần
            effects.Add(new PoisonousBloodPoolEffect
            {
                effectType = EffectType.PoisonousBloodPool,
                duration = EffectConfig.DebuffRound,
                Actor = Character,
                impacts = new List<Cell> { bloodPositionCell }
            });
            
            AlkawaDebug.Log(ELogCategory.SKILL, $"[{CharName}] Lây Nhiễm (EnemyTurn): Pool độc gây thêm 2d4 sát thương");
        }
        
        return new DamageTakenParams
        {
            Damage = baseDamage,
            Effects = effects,
            ReceiveFromCharacter = Character
        };
    }
    
    // Phương thức tính toán vị trí xuất hiện máu độc
    private Cell CalculateBloodPosition(Character enemy)
    {
        if (enemy == null || enemy.Info == null || enemy.Info.SkillInfo == null)
        {
            // Nếu không có thông tin skill địch, lấy ô bên cạnh Thiết Nhân
            return GpManager.MapManager.GetHexagonsInMoveRange(Info.Cell, 1, DirectionType.All).FirstOrDefault();
        }
        
        int enemySkillRange = enemy.Info.SkillInfo.range;
        int bloodDistance = enemySkillRange - 6;
        
        // Nếu bloodDistance < 0, lấy ô bên cạnh Thiết Nhân
        if (bloodDistance < 0)
        {
            return GpManager.MapManager.GetHexagonsInMoveRange(Info.Cell, 1, DirectionType.All).FirstOrDefault();
        }
        
        // Tìm đường thẳng gần nhất giữa Thiết Nhân và kẻ ra đòn
        var path = GpManager.MapManager.FindShortestPath(Info.Cell, enemy.Info.Cell);
        if (path == null || path.Count <= bloodDistance)
        {
            // Nếu không tìm được đường đi hoặc đường đi quá ngắn, lấy ô bên cạnh Thiết Nhân
            return GpManager.MapManager.GetHexagonsInMoveRange(Info.Cell, 1, DirectionType.All).FirstOrDefault();
        }
        
        // Lấy ô ở vị trí bloodDistance trên đường đi
        return path[bloodDistance];
    }
    
    // Định nghĩa class cho hiệu ứng ThietNhan_Infected
    public class ThietNhanInfectedEffect : EffectData
    {
        public int rollsRemaining = 2; // Còn lại bao nhiêu lần roll
        
        // Không ghi đè OnNewRound vì nó không tồn tại trong EffectData
        // Thay vào đó, sử dụng hệ thống event hoặc update thủ công
        
        // Phương thức này sẽ được gọi từ bên ngoài để xử lý mỗi vòng mới
        public void CheckInfection(Character targetCharacter, Character sourceCharacter)
        {
            if (rollsRemaining <= 0) return;
            
            int roll = Roll.RollDice(1, 20, 0);
            AlkawaDebug.Log(ELogCategory.EFFECT, $"[Hạt nhân lây nhiễm] Roll 1d20 = {roll} để kiểm tra hóa Thiết Nhân (< 10 để thất bại)");
            
            if (roll < 10)
            {
                rollsRemaining--;
                AlkawaDebug.Log(ELogCategory.EFFECT, $"[Hạt nhân lây nhiễm] Thất bại! Còn lại {rollsRemaining} lần roll.");
                
                if (rollsRemaining <= 0 && targetCharacter != null && targetCharacter.Info != null)
                {
                    // Nếu thất bại 2 lần, nhân vật sẽ chết
                    AlkawaDebug.Log(ELogCategory.EFFECT, $"[Hạt nhân lây nhiễm] Đã thất bại 2 lần => Chết ngay lập tức!");
                    
                    // Gây sát thương chết người cho nhân vật
                    targetCharacter.Info.HandleDamageTaken(-targetCharacter.Info.CurrentHp, sourceCharacter);
                }
            }
            else
            {
                // Nhân vật vượt qua kiểm tra, loại bỏ hiệu ứng
                AlkawaDebug.Log(ELogCategory.EFFECT, $"[Hạt nhân lây nhiễm] Thành công! Nhân vật phá hủy được hạt nhân.");
                duration = 0; // Sẽ bị loại bỏ ở cuối vòng
            }
        }
    }

    private int ProcessFriendAttacks(IEnumerable<Character> friends, int totalDamage)
    {
        int initialDamage = totalDamage;
        foreach (var friend in friends)
        {
            bool isCrit = CheatManager.HasInstance && CheatManager.Instance.IsAlwaysCritActive();
            int roll = Roll.RollDice(1, 20, 0);
            AlkawaDebug.Log(ELogCategory.SKILL, "----------------------------------------------------------");

            if (roll >= 10)
            {
                var animName = GetAnimByIndex(_skillStateParams.SkillInfo.skillIndex);
                friend.StateMachine.GetCurrentState.PlayAnim(animName);
                AlkawaDebug.Log(ELogCategory.SKILL,
                    $"[{CharName}] Tấn công hỗ trợ: 1d20 = {roll} >= 10 => có thể cùng tấn công");

                int friendBaseDamage = GetBaseDamage();
                int rollTimes = Roll.GetActualRollTimes(1, isCrit);
                int friendSkillDamage = Roll.RollDice(1, 4, 0, isCrit);
                int friendTotalDamage = friendBaseDamage + friendSkillDamage;
                totalDamage += friendTotalDamage;

                string critInfo = isCrit ? " (CRIT)" : "";
                AlkawaDebug.Log(ELogCategory.SKILL, $"[{friend.characterConfig.characterName}] Tấn công hỗ trợ: Base Damage = {friendBaseDamage}");
                AlkawaDebug.Log(ELogCategory.SKILL, $"[{friend.characterConfig.characterName}] Tấn công hỗ trợ: Skill Formula = {rollTimes}d4{critInfo}");
                AlkawaDebug.Log(ELogCategory.SKILL, $"[{friend.characterConfig.characterName}] Tấn công hỗ trợ: Skill Damage = {rollTimes}d4 = {friendSkillDamage}");
                AlkawaDebug.Log(ELogCategory.SKILL, $"[{friend.characterConfig.characterName}] Tấn công hỗ trợ: Total Damage = {friendBaseDamage} + {friendSkillDamage} = {friendTotalDamage}");
            }
            else
            {
                AlkawaDebug.Log(ELogCategory.SKILL,
                    $"[{CharName}] Tấn công hỗ trợ: 1d20 = {roll} < 10 => Không thể cùng tấn công");
            }
        }

        return totalDamage;
    }
}