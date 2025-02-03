// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
//
// public class CharacterManager : SingletonMonoBehavior<CharacterManager>
// {
//     // new
//     [SerializeField] private SerializableDictionary<CharacterType, Character> allCharacter = new();
//     
//     // old
//     // private
//     private List<Character> Players { get; set; } = new();
//     private List<Character> Enemies { get; set; } = new();
//
//
//     public MapManager MapManager { get; set; }
//     private CharacterSpawnerConfig _spawnerConfig;
//     
//     // public 
//     public Character SelectedCharacter { get; set; }
//     public Character MainCharacter { get; set; }
//     public Character FocusedCharacter { get; set; }
//     public List<Character> Characters { get; set; } = new();
//     public HashSet<Character> CharactersInRange { get; set; } = new();
//     public GameplayManager GPManager => GameplayManager.Instance;
//     
//     public void Initialize(CharacterSpawnerConfig config, MapManager mapManager)
//     {
//         _spawnerConfig = config;
//         MapManager = mapManager;
//         SpawnCharacter();
//     }
//
//     private void SpawnCharacter()
//     {
//         Characters.Clear();
//         Enemies.Clear();
//         Players.Clear();
//
//         foreach (var spawnPoint in _spawnerConfig.spawnPoints)
//         {
//             foreach (var point in spawnPoint.Value.points)
//             {
//                 var go = Instantiate(allCharacter[spawnPoint.Key], transform);
//                 var character = go.GetComponent<Character>();
//                 character.Initialize(this, MapManager.GetCell(point));
//                 Characters.Add(character);
//                 switch (character)
//                 {
//                     case AICharacter aiCharacter:
//                         Enemies.Add(aiCharacter);
//                         break;
//                     case PlayerCharacter playerCharacter:
//                         Players.Add(playerCharacter);
//                         break;
//                 }
//
//                 if (GPManager.IsTutorialLevel)
//                 {
//                     character.HideHpBar();
//                 }
//             }
//         }
//
//         SortCharacterBySpeed();
//         if (!GPManager.IsTutorialLevel) SetMainCharacter();
//     }
//
//     private void SortCharacterBySpeed()
//     {
//         Characters = Characters.OrderByDescending(c => c.characterInfo.Speed).ToList();
//     }
//
//     public void SetMainCharacter()
//     {
//         SetMainCharacter(Characters[GPManager.CharacterIndex]);
//     }
//
//     public void SetMainCharacter(Character character)
//     {
//         if (TutorialManager.Instance != null
//             && GPManager.CurrentRound == 1 
//             && character == Characters[0]
//             && !TutorialManager.Instance.EndTuto)
//         {
//             MainCharacter.OnUnSelected();
//             TutorialManager.Instance.OnNewRound();
//         }
//         else
//         {
//             if (character.characterInfo.CurrentHP <= 0)
//             {
//                 GPManager.HandleEndTurn();
//             }
//             else
//             {
//                 MainCharacter = character;
//                 MainCharacter.SetMainCharacter();
//                 SetSelectedCharacter(character);
//                 SetupCharacterFocus();   
//             }
//         }
//         if (Characters.Count > 0 && character == Characters[0])
//         {
//             GPManager.HandleNewRound();
//         }
//     }
//     
//     public void SetSelectedCharacter(Character character)
//     {
//
//         SelectedCharacter?.OnUnSelected();
//         SelectedCharacter = character;
//         HideMoveRange();
//         HideSkillRange();
//         character.OnSelected();
//         SetHUD();
//         AlkawaDebug.Log(ELogCategory.GAMEPLAY,$"SetSelectedCharacter: {character.characterConfig.characterName}");
//     }
//
//     #region HUD
//
//     private void SetupCharacterFocus()
//     {
//         // HUD.Instance.SetupCharacterFocus(Characters, GPManager.CharacterIndex);
//     }
//     
//     public void SetHUD()
//     {
//         // HUD.Instance.SetCharacterFocus(GetSelectedCharacterParams());
//     }
//
//     #endregion
//
//     public void HandleCastSkill(Character character)
//     {
//         HideMoveRange();
//         HideSkillRange();
//         FocusedCharacter = character;
//         SelectedCharacter.characterInfo.OnCastSkill(GPManager.SkillInfo);
//     }
//
//     public void OnCastSkillFinished()
//     {
//         SetHUD();
//         if (GPManager.SkillInfo.damageType.HasFlag(DamageTargetType.Enemies))
//         {
//             TryAttackEnemies(FocusedCharacter);
//         }
//         if (GPManager.SkillInfo.damageType.HasFlag(DamageTargetType.Team))
//         {
//             ApplyBuff();
//         }
//     }
//
//     private void ApplyBuff()
//     {
//         FocusedCharacter.characterInfo.ApplyBuff(GPManager.SkillInfo);
//     }
//     
//     private bool TryAttackEnemies(Character focusedCharacter)
//     {
//         var skillInfo = GPManager.SkillInfo;
//         // FocusedCharacter.characterInfo.OnDamageTaken(5, SetDamageTakenFinished);
//         // check crit
//         int damage = SelectedCharacter.characterInfo.BaseDamage;
//         var hitChange = SelectedCharacter.Roll.GetHitChange();
//         var isCritical = SelectedCharacter.Roll.IsCritical(hitChange);
//         if (!isCritical)
//         {
//             var dodge = focusedCharacter.characterInfo.Dodge;
//             if (dodge > hitChange)
//             {
//                 //AlkawaDebug.Log($"[Gameplay] Né skill, dodge = {dodge} - hitChange = {hitChange}");
//                 focusedCharacter.characterInfo.OnDamageTaken(0);
//             }
//             else
//             {
//                 if (skillInfo.hasApplyDamage)
//                 {
//                     damage += SelectedCharacter.Roll.RollDice(skillInfo.damageConfig);
//                 }
//                 // 
//                 // if (skillInfo.effectIndex != EffectIndex.None)
//                 {
//                     // var debuffResistance = RollManager.Instance.GetDebuffResistance(focusedCharacter.Info.CharacterAttributes);
//                     // if (debuffResistance < GameplayManager.EffectConstant
//                     //         .effectData[_skillData.effectConfig.effectIndex].debuffResistance)
//                     // {
//                     //     focusedCharacter.Info.ApplyDeBuff(_skillData.effectConfig, SelectedCharacter);
//                     //     Debug.Log($"[Gameplay] - Damage: {damage}");
//                     // }
//                 }
//                 focusedCharacter.characterInfo.OnDamageTaken(damage);
//             }
//         }
//         
//         else
//         {
//             damage *= 2;
//             focusedCharacter.characterInfo.OnDamageTaken(damage);
//             //AlkawaDebug.Log($"Gameplay: Get Damage Taken {damage}");
//         }
//         
//         return false;
//     }
//
//     public void SetDamageTakenFinished()
//     {
//         if (FocusedCharacter is PlayerCharacter && MainCharacter is AICharacter) // react
//         {
//             ReactMenu.Instance.Open();
//         }
//         else if (SelectedCharacter.characterInfo.IsReact)
//         {
//             OnEndReact();
//         }
//         
//     }
//
//     private void OnEndReact()
//     {
//         GPManager.HandleEndTurn();
//     }
//
//     public Character GetCharacterByType(CharacterType characterType)
//     {
//         return Characters.FirstOrDefault(character => character.characterType == characterType);
//     }
//     
//     #region Sub
//
//     private CharacterParams GetSelectedCharacterParams()
//     {
//         return new CharacterParams
//         {
//             Character = SelectedCharacter,
//             Skills = SelectedCharacter.GetSkillInfos(GetSkillType())
//         };
//     }
//     
//     public void ShowAllHPBar()
//     {
//         foreach (var character in Characters)
//         {
//             character.ShowHpBar();
//         }
//     }
//
//     public bool IsMainCharacterSelected => SelectedCharacter == MainCharacter;
//
//     public bool CanShowEndTurn => 
//         IsMainCharacterSelected && MainCharacter is PlayerCharacter || 
//         SelectedCharacter.characterInfo.IsReact;
//     #endregion
//
//     #region Cell Interact
//
//     public void ShowMoveRange()
//     {
//         var range = SelectedCharacter.characterInfo.GetMoveRange();
//         //AlkawaDebug.Log($"[Gameplay] [{SelectedCharacter.characterConfig.characterName}] Show Move Range: {range}");
//         MapManager.ShowMoveRange(SelectedCharacter.characterInfo.Cell, range);
//     }
//
//     public void HideMoveRange()
//     {
//         //AlkawaDebug.Log($"[Gameplay] {SelectedCharacter.characterConfig.characterName} Hide Move Range");
//         MapManager.HideMoveRange();
//     }
//
//     public void HideSkillRange()
//     {
//         //AlkawaDebug.Log($"[Gameplay] [{SelectedCharacter.characterConfig.characterName}] Hide Skill Range");
//         MapManager.HideSkillRange();
//         CharactersInRange.Clear();
//     }
//
//     public bool TryMoveToCell(Cell cell)
//     {
//         if (SelectedCharacter is PlayerCharacter && SelectedCharacter != null && MapManager.CanMove(cell))
//         {
//             var cellPath = MapManager.FindPath(SelectedCharacter.characterInfo.Cell, cell);
//             MapManager.HideMoveRange();
//             SelectedCharacter.MoveCharacter(cellPath);
//             return true;
//         }
//         return false;
//     }
//     
//     public List<Character> GetEnemiesInRange(Character character, int range)
//     {
//         var characters = MapManager.GetCharacterInRange(character.characterInfo.Cell, range);
//         return characters.Where(c => c.Type != character.Type).ToList();
//     }
//     
//     #endregion
//
//     #region Facing
//
//     public SkillType GetSkillType()
//     {
//         if (SelectedCharacter == MainCharacter) return SkillType.InternalSkill;
//         return SelectedCharacter.Type == MainCharacter.Type ? SkillType.MovementSkill : SkillType.CombatSkill;
//     }
//     
//     public SkillType GetSkillType(Character character)
//     {
//         if (character == MainCharacter) return SkillType.InternalSkill;
//         return character.Type == MainCharacter.Type ? SkillType.MovementSkill : SkillType.CombatSkill;
//     }
//
//     public SkillInfo GetSkillInfo(int index)
//     {
//         return SelectedCharacter.characterInfo.GetSkillInfo(index, GetSkillType());
//     }
//
//     public FacingType GetFacingType(Character character)
//     {
//         if (character == null)
//         {
//             //AlkawaDebug.LogWarning("GetFacingType: Character is null.");
//             return FacingType.Right;
//         }
//
//         var characterPosition = character.transform.position;
//         var nearestOpponent = Utils.FindNearestCharacter(character, character is AICharacter ? Players : Enemies);
//         if (nearestOpponent == null)
//         {
//             //AlkawaDebug.LogWarning("GetFacingType: No opponents found.");
//             return FacingType.Right;
//         }
//
//         var opponentPosition = nearestOpponent.transform.position;
//         return characterPosition.x > opponentPosition.x ? FacingType.Left : FacingType.Right;
//     }
//
//     #endregion
//     
//     public void OnConFirmClick()
//     {
//         //AlkawaDebug.Log($"[Gameplay] - OnConFirmClick");
//         FocusedCharacter.characterInfo.IsReact = true;
//         SetSelectedCharacter(FocusedCharacter);
//     }
//
//     public void OnCancelClick()
//     {
//         //AlkawaDebug.Log("[Gameplay] - OnCancelClick");
//         FocusedCharacter.characterInfo.IsReact = false;
//         OnEndReact();
//     }
//     
//     public void HandleCharacterDie(Character character)
//     {
//         Characters.Remove(character);
//         if (character is AICharacter)
//         {
//             Enemies.Remove(character);
//             if (Enemies.Count == 0)
//             {
//                 Invoke(nameof(OnWin), 1f);
//             }
//         }
//         else
//         {
//             Players.Remove(character);
//             if (Players.Count == 0)
//             {
//                 Invoke(nameof(OnLose), 1f);
//             }
//         }
//     }
//     
//     private void OnWin()
//     {
//         // HUD.Instance.SetUIWinGame();
//         // HandleWinCondition();
//     }
//
//     private void HandleWinCondition()
//     {
//         //AlkawaDebug.Log("WINNNN");
//     }
//
//     private void OnLose()
//     {
//         // HUDMenu.Instance.gameObject.SetActive(false);
//         // UIManager.Instance.ShowPopup(PopupType.Defeat);
//     }
// }
//
// public enum FacingType
// {
//     Left,
//     Right,
// }
//
