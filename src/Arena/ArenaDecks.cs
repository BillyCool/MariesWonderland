using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Pvp;

namespace MariesWonderland.Arena;

/// <summary>
/// A player's Arena decks as the server sends them. The client builds an opponent only from
/// <see cref="StartBattleResponse"/>'s deck characters and looks nothing up in the opponent's data, so every value it
/// would compute for its own deck is filled here from the player's data, the way the client does
/// (<c>CalculatorDataBattleDeckForPvp</c>, <c>CalculatorDataBattleWeapon</c>, <c>CalculatorDataStainedGlassStatus</c>).
/// </summary>
public static class ArenaDecks
{
    /// <summary>The deck the client assumes defends when the player never chose one (<c>CalculatorDeck.InitialDeckNumber</c>).</summary>
    public const int InitialDeckNumber = 1;

    /// <summary>Important items of this type are stained glass; their <c>ExternalReferenceId</c> is the stained glass id.</summary>
    private const int StainedGlassImportantItemType = 5;

    private const int WeaponAwakenStatusUp = 1;
    private const int WeaponAwakenAbility = 2;

    public static int DefenseDeckNumber(DarkUserMemoryDatabase userDb)
    {
        int number = userDb.EntityIUserPvpDefenseDeck.FirstOrDefault()?.UserDeckNumber ?? 0;
        return number > 0 ? number : InitialDeckNumber;
    }

    /// <summary>The deck's characters in slot order (empty slots left out).</summary>
    public static List<EntityIUserDeckCharacter> DeckCharacters(DarkUserMemoryDatabase userDb, DeckType deckType, int deckNumber)
    {
        EntityIUserDeck? deck = userDb.EntityIUserDeck.FirstOrDefault(d => d.DeckType == deckType && d.UserDeckNumber == deckNumber);
        if (deck == null)
        {
            return [];
        }

        return [.. new[] { deck.UserDeckCharacterUuid01, deck.UserDeckCharacterUuid02, deck.UserDeckCharacterUuid03 }
            .Where(uuid => !string.IsNullOrEmpty(uuid))
            .Select(uuid => userDb.EntityIUserDeckCharacter.FirstOrDefault(c => c.UserDeckCharacterUuid == uuid))
            .OfType<EntityIUserDeckCharacter>()
            .Where(c => userDb.EntityIUserCostume.Any(costume => costume.UserCostumeUuid == c.UserCostumeUuid))];
    }

    /// <summary>The deck as cards and logs show it: force, costumes, main weapon attributes, strongest costume.</summary>
    public static ArenaDefense Summary(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, DeckType deckType, int deckNumber)
    {
        List<EntityIUserDeckCharacter> characters = DeckCharacters(userDb, deckType, deckNumber);
        if (characters.Count == 0)
        {
            return ArenaDefense.None;
        }

        List<int> costumeIds = [];
        List<int> attributes = [];
        foreach (EntityIUserDeckCharacter character in characters)
        {
            costumeIds.Add(userDb.EntityIUserCostume.First(c => c.UserCostumeUuid == character.UserCostumeUuid).CostumeId);
            int weaponId = userDb.EntityIUserWeapon.FirstOrDefault(w => w.UserWeaponUuid == character.MainUserWeaponUuid)?.WeaponId ?? 0;
            attributes.Add((int)(masterDb.EntityMWeapon.Find(weaponId)?.AttributeType ?? 0));
        }

        int strongest = characters.Select((c, i) => (c.Power, Index: i)).OrderByDescending(c => c.Power).ThenBy(c => c.Index).First().Index;
        EntityIUserDeck deck = userDb.EntityIUserDeck.First(d => d.DeckType == deckType && d.UserDeckNumber == deckNumber);
        int power = deck.Power > 0 ? deck.Power : characters.Sum(c => c.Power);
        return new ArenaDefense(power, costumeIds, attributes, costumeIds[strongest]);
    }

    /// <summary>Everything the client needs to field the deck as an opponent.</summary>
    public static List<DeckCharacter> Build(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, DeckType deckType, int deckNumber)
        => [.. DeckCharacters(userDb, deckType, deckNumber).Select(c => Build(userDb, masterDb, c))];

    private static DeckCharacter Build(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, EntityIUserDeckCharacter deckCharacter)
    {
        EntityIUserCostume costume = userDb.EntityIUserCostume.First(c => c.UserCostumeUuid == deckCharacter.UserCostumeUuid);
        EntityMCostume? costumeMaster = masterDb.EntityMCostume.Find(costume.CostumeId);
        int characterId = costumeMaster?.CharacterId ?? 0;
        string costumeUuid = costume.UserCostumeUuid;

        DeckCharacter result = new()
        {
            Costume = new CostumeInfo
            {
                CostumeId = costume.CostumeId,
                LimitBreakCount = costume.LimitBreakCount,
                Level = costume.Level,
                ActiveSkillLevel = Math.Max(userDb.EntityIUserCostumeActiveSkill.FirstOrDefault(s => s.UserCostumeUuid == costumeUuid)?.Level ?? 1, 1),
                CharacterLevel = userDb.EntityIUserCharacter.FirstOrDefault(c => c.CharacterId == characterId)?.Level ?? 1,
                CostumeLotteryEffectUnlockedSlotCount = userDb.EntityIUserCostumeLotteryEffect.Count(e => e.UserCostumeUuid == costumeUuid),
            },
        };

        EntityIUserCompanion? companion = userDb.EntityIUserCompanion.FirstOrDefault(c => c.UserCompanionUuid == deckCharacter.UserCompanionUuid);
        if (companion != null)
        {
            result.Companion = new CompanionInfo { CompanionId = companion.CompanionId, Level = companion.Level };
        }

        EntityIUserWeapon? mainWeapon = userDb.EntityIUserWeapon.FirstOrDefault(w => w.UserWeaponUuid == deckCharacter.MainUserWeaponUuid);
        if (mainWeapon != null)
        {
            result.MainWeapon = Weapon(userDb, masterDb, mainWeapon);
        }
        foreach (EntityIUserDeckSubWeaponGroup sub in userDb.EntityIUserDeckSubWeaponGroup
            .Where(s => s.UserDeckCharacterUuid == deckCharacter.UserDeckCharacterUuid)
            .OrderBy(s => s.SortOrder))
        {
            EntityIUserWeapon? weapon = userDb.EntityIUserWeapon.FirstOrDefault(w => w.UserWeaponUuid == sub.UserWeaponUuid);
            if (weapon != null)
            {
                result.SubWeapon.Add(Weapon(userDb, masterDb, weapon));
            }
        }

        foreach (EntityIUserDeckPartsGroup partsGroup in userDb.EntityIUserDeckPartsGroup
            .Where(p => p.UserDeckCharacterUuid == deckCharacter.UserDeckCharacterUuid)
            .OrderBy(p => p.SortOrder))
        {
            EntityIUserParts? parts = userDb.EntityIUserParts.FirstOrDefault(p => p.UserPartsUuid == partsGroup.UserPartsUuid);
            if (parts == null)
            {
                continue;
            }

            PartsInfo info = new() { PartsId = parts.PartsId, Level = parts.Level, PartsMainStatusId = parts.PartsStatusMainId };
            info.SubPartsStatus.AddRange(userDb.EntityIUserPartsStatusSub
                .Where(s => s.UserPartsUuid == parts.UserPartsUuid)
                .OrderBy(s => s.StatusIndex)
                .Select(s => new PartsSubStatusInfo
                {
                    Level = s.Level,
                    StatusKindType = (int)s.StatusKindType,
                    StatusCalculationType = (int)s.StatusCalculationType,
                    StatusChangeValue = s.StatusChangeValue,
                }));
            result.Parts.Add(info);
        }

        result.CharacterBoardAbilities.AddRange(userDb.EntityIUserCharacterBoardAbility
            .Where(a => a.CharacterId == characterId)
            .Select(a => new CharacterBoardAbilityInfo { AbilityId = a.AbilityId, Level = a.Level }));
        result.CharacterBoardStatusUps.AddRange(userDb.EntityIUserCharacterBoardStatusUp
            .Where(s => s.CharacterId == characterId)
            .Select(s => new CharacterBoardStatusUpInfo
            {
                StatusCalculationType = (int)s.StatusCalculationType, Hp = s.Hp, Attack = s.Attack, Vitality = s.Vitality,
                Agility = s.Agility, CriticalRatio = s.CriticalRatio, CriticalAttack = s.CriticalAttack,
            }));
        result.CostumeLevelBonusStatusUps.AddRange(userDb.EntityIUserCharacterCostumeLevelBonus
            .Where(s => s.CharacterId == characterId)
            .Select(s => new CostumeLevelBonusStatusUpInfo
            {
                StatusCalculationType = (int)s.StatusCalculationType, Hp = s.Hp, Attack = s.Attack, Vitality = s.Vitality,
                Agility = s.Agility, CriticalRatio = s.CriticalRatio, CriticalAttack = s.CriticalAttack,
            }));

        if (CostumeAwakenAbility(masterDb, costume) is { } awakenAbility)
        {
            result.AwakenAbilities.Add(awakenAbility);
        }
        result.AwakenStatusUps.AddRange(userDb.EntityIUserCostumeAwakenStatusUp
            .Where(s => s.UserCostumeUuid == costumeUuid)
            .Select(s => new AwakenStatusUpInfo
            {
                StatusCalculationType = (int)s.StatusCalculationType, Hp = s.Hp, Attack = s.Attack, Vitality = s.Vitality,
                Agility = s.Agility, CriticalRatio = s.CriticalRatio, CriticalAttack = s.CriticalAttack,
            }));

        int thoughtId = userDb.EntityIUserThought.FirstOrDefault(t => t.UserThoughtUuid == deckCharacter.UserThoughtUuid)?.ThoughtId ?? 0;
        if (thoughtId != 0)
        {
            result.Thought = new ThoughtInfo { ThoughtId = thoughtId };
        }

        result.StainedGlassStatusUps.Add(StainedGlass(userDb, masterDb, characterId, costumeMaster?.SkillfulWeaponType ?? WeaponType.UNKNOWN));

        result.CostumeLotteryEffectAbilities.AddRange(userDb.EntityIUserCostumeLotteryEffectAbility
            .Where(a => a.UserCostumeUuid == costumeUuid)
            .OrderBy(a => a.SlotNumber)
            .Select(a => new CostumeLotteryEffectAbilityInfo { AbilityId = a.AbilityId, Level = a.AbilityLevel }));
        result.CostumeLotteryEffectStatusUps.AddRange(userDb.EntityIUserCostumeLotteryEffectStatusUp
            .Where(s => s.UserCostumeUuid == costumeUuid)
            .Select(s => new CostumeLotteryEffectStatusUpInfo
            {
                StatusCalculationType = (int)s.StatusCalculationType, Hp = s.Hp, Attack = s.Attack, Vitality = s.Vitality,
                Agility = s.Agility, CriticalRatio = s.CriticalRatio, CriticalAttack = s.CriticalAttack,
            }));
        return result;
    }

    /// <summary>A weapon with its slot abilities and skills resolved to ids, and its awakening when it's awakened.</summary>
    private static WeaponInfo Weapon(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, EntityIUserWeapon weapon)
    {
        EntityMWeapon? master = masterDb.EntityMWeapon.Find(weapon.WeaponId);
        WeaponInfo info = new() { WeaponId = weapon.WeaponId, LimitBreakCount = weapon.LimitBreakCount, Level = weapon.Level };
        if (master == null)
        {
            return info;
        }

        foreach (EntityIUserWeaponAbility ability in userDb.EntityIUserWeaponAbility.Where(a => a.UserWeaponUuid == weapon.UserWeaponUuid).OrderBy(a => a.SlotNumber))
        {
            int abilityId = masterDb.EntityMWeaponAbilityGroup
                .FirstOrDefault(g => g.WeaponAbilityGroupId == master.WeaponAbilityGroupId && g.SlotNumber == ability.SlotNumber)?.AbilityId ?? 0;
            if (abilityId != 0)
            {
                info.WeaponAbility.Add(new WeaponAbilityInfo { AbilityId = abilityId, Level = ability.Level });
            }
        }
        foreach (EntityIUserWeaponSkill skill in userDb.EntityIUserWeaponSkill.Where(s => s.UserWeaponUuid == weapon.UserWeaponUuid).OrderBy(s => s.SlotNumber))
        {
            int skillId = masterDb.EntityMWeaponSkillGroup
                .FirstOrDefault(g => g.WeaponSkillGroupId == master.WeaponSkillGroupId && g.SlotNumber == skill.SlotNumber)?.SkillId ?? 0;
            if (skillId != 0)
            {
                info.WeaponSkill.Add(new WeaponSkillInfo { SkillId = skillId, Level = skill.Level });
            }
        }

        if (userDb.EntityIUserWeaponAwaken.Any(a => a.UserWeaponUuid == weapon.UserWeaponUuid))
        {
            AddWeaponAwaken(masterDb, weapon.WeaponId, info);
        }
        return info;
    }

    /// <summary>An awakened weapon's ability and status ups, from master data (<c>CalculatorDataBattleWeapon.SetAwakenInfo</c>).</summary>
    public static void AddWeaponAwaken(DarkMasterMemoryDatabase masterDb, int weaponId, WeaponInfo info)
    {
        EntityMWeaponAwaken? awaken = masterDb.EntityMWeaponAwaken.FirstOrDefault(a => a.WeaponId == weaponId);
        if (awaken == null)
        {
            return;
        }

        foreach (EntityMWeaponAwakenEffectGroup effect in masterDb.EntityMWeaponAwakenEffectGroup.Where(e => e.WeaponAwakenEffectGroupId == awaken.WeaponAwakenEffectGroupId))
        {
            if (effect.WeaponAwakenEffectType == WeaponAwakenAbility
                && masterDb.EntityMWeaponAwakenAbility.FirstOrDefault(a => a.WeaponAwakenAbilityId == effect.WeaponAwakenEffectId) is { } ability)
            {
                info.WeaponAwakenAbility = new AwakenAbilityInfo { AbilityId = ability.AbilityId, Level = ability.AbilityLevel };
            }
            else if (effect.WeaponAwakenEffectType == WeaponAwakenStatusUp)
            {
                foreach (IGrouping<StatusCalculationType, EntityMWeaponAwakenStatusUpGroup> byType in masterDb.EntityMWeaponAwakenStatusUpGroup
                    .Where(s => s.WeaponAwakenStatusUpGroupId == effect.WeaponAwakenEffectId)
                    .GroupBy(s => s.StatusCalculationType))
                {
                    StatusTotals totals = new();
                    foreach (EntityMWeaponAwakenStatusUpGroup row in byType)
                    {
                        totals.Add(row.StatusKindType, row.EffectValue);
                    }
                    info.WeaponAwakenStatusUps.Add(new AwakenStatusUpInfo
                    {
                        StatusCalculationType = (int)byType.Key, Hp = totals.Hp, Attack = totals.Attack, Vitality = totals.Vitality,
                        Agility = totals.Agility, CriticalRatio = totals.CriticalRatio, CriticalAttack = totals.CriticalAttack,
                    });
                }
            }
        }
    }

    /// <summary>
    /// The costume's awakening ability: the first ability step it has reached
    /// (<c>CalculatorDataBattleAbility.CreateActorAwakenAbilityList</c> stops at the first one).
    /// </summary>
    private static AwakenAbilityInfo? CostumeAwakenAbility(DarkMasterMemoryDatabase masterDb, EntityIUserCostume costume)
    {
        EntityMCostumeAwaken? awaken = masterDb.EntityMCostumeAwaken.FirstOrDefault(a => a.CostumeId == costume.CostumeId);
        if (awaken == null || costume.AwakenCount <= 0)
        {
            return null;
        }

        EntityMCostumeAwakenEffectGroup? effect = masterDb.EntityMCostumeAwakenEffectGroup
            .Where(e => e.CostumeAwakenEffectGroupId == awaken.CostumeAwakenEffectGroupId
                && e.CostumeAwakenEffectType == CostumeAwakenEffectType.ABILITY
                && e.AwakenStep <= costume.AwakenCount)
            .OrderBy(e => e.AwakenStep)
            .FirstOrDefault();
        EntityMCostumeAwakenAbility? ability = effect == null
            ? null
            : masterDb.EntityMCostumeAwakenAbility.FirstOrDefault(a => a.CostumeAwakenAbilityId == effect.CostumeAwakenEffectId);
        return ability == null ? null : new AwakenAbilityInfo { AbilityId = ability.AbilityId, Level = ability.AbilityLevel };
    }

    /// <summary>
    /// The stained glass bonus for one character: every stained glass the player owns whose targets name the character
    /// or the costume's weapon type, summed into one added bonus (<c>CalculatorDataStainedGlassStatus.CreateStainedGlassStatusList</c>).
    /// </summary>
    private static StainedGlassStatusUpInfo StainedGlass(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, int characterId, WeaponType skillfulWeaponType)
    {
        StatusTotals totals = new();
        foreach (EntityMImportantItem item in masterDb.EntityMImportantItem.Where(i => i.ImportantItemType == StainedGlassImportantItemType))
        {
            if (!userDb.EntityIUserImportantItem.Any(i => i.ImportantItemId == item.ImportantItemId))
            {
                continue;
            }

            EntityMStainedGlass? glass = masterDb.EntityMStainedGlass.FirstOrDefault(g => g.StainedGlassId == item.ExternalReferenceId);
            if (glass == null || !masterDb.EntityMStainedGlassStatusUpTargetGroup.Any(t =>
                t.StainedGlassStatusUpTargetGroupId == glass.StainedGlassStatusUpTargetGroupId
                && ((t.StatusUpTargetType == StainedGlassStatusUpTargetType.CHARACTER && t.TargetValue == characterId)
                    || (t.StatusUpTargetType == StainedGlassStatusUpTargetType.SKILLFUL_WEAPON && t.TargetValue == (int)skillfulWeaponType))))
            {
                continue;
            }

            foreach (EntityMStainedGlassStatusUpGroup row in masterDb.EntityMStainedGlassStatusUpGroup.Where(g => g.StainedGlassStatusUpGroupId == glass.StainedGlassStatusUpGroupId))
            {
                totals.Add(row.StatusKindType, row.EffectValue);
            }
        }

        return new StainedGlassStatusUpInfo
        {
            StatusCalculationType = (int)StatusCalculationType.ADD, Hp = totals.Hp, Attack = totals.Attack, Vitality = totals.Vitality,
            Agility = totals.Agility, CriticalRatio = totals.CriticalRatio, CriticalAttack = totals.CriticalAttack,
        };
    }

    /// <summary>Status values summed by kind (evasion isn't sent to the client).</summary>
    private sealed class StatusTotals
    {
        public int Hp;
        public int Attack;
        public int Vitality;
        public int Agility;
        public int CriticalRatio;
        public int CriticalAttack;

        public void Add(StatusKindType kind, int value)
        {
            switch (kind)
            {
                case StatusKindType.HP: Hp += value; break;
                case StatusKindType.ATTACK: Attack += value; break;
                case StatusKindType.VITALITY: Vitality += value; break;
                case StatusKindType.AGILITY: Agility += value; break;
                case StatusKindType.CRITICAL_RATIO: CriticalRatio += value; break;
                case StatusKindType.CRITICAL_ATTACK: CriticalAttack += value; break;
            }
        }
    }
}
