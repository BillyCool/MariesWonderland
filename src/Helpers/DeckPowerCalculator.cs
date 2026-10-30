using MariesWonderland.Arena;
using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Pvp;

namespace MariesWonderland.Helpers;

/// <summary>
/// A deck's force (power) the way the client works it out (<c>CalculatorPower.CalculateDeckPower</c>): per character, the
/// stat power of the costume, weapons, companion and parts, plus the power of every skill and ability, which refers to
/// the power of the character or the party. The client sends its own value in <c>RefreshDeckPower</c>, but a deck saved
/// by <c>ReplaceDeck</c> has none yet, and the Arena only offers decks with a force above 0.
///
/// <para>
/// Follows the client's own code (decompiled <c>CalculatorPower</c>, <c>CalculatorStatus</c>, <c>CalculatorStatusOutgame</c>),
/// which differs from the datamine project's port in several places (see docs/systems/arena.md). Checked against the 83
/// quest and Arena decks of the real save, whose stored force the client computed: the quest decks (42 characters) match
/// exactly for 34 of them, 0.27% off on average, and the two most recently refreshed quest decks exactly. The Arena
/// decks (41 characters, each with a companion and lottery abilities) are 3.8% off on average, cause unknown.
/// </para>
/// </summary>
public static class DeckPowerCalculator
{
    private const int MaxSubWeapons = 2;
    private const int MaxParts = 3;
    private const int PartsMainOptionMaxStep = 13;
    private const int WeaponAwakenStatusUpEffect = 1;
    private const int WeaponAwakenAbilityEffect = 2;

    /// <summary>
    /// Works out the force of the deck and of each of its characters and stores them (<c>Power</c> of the deck, of its
    /// characters, and the best force of the deck type). Does nothing when there is no such deck.
    /// </summary>
    public static void Refresh(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, DeckType deckType, int deckNumber)
    {
        EntityIUserDeck? deck = userDb.EntityIUserDeck.FirstOrDefault(d => d.DeckType == deckType && d.UserDeckNumber == deckNumber);
        if (deck == null)
        {
            return;
        }

        List<EntityIUserDeckCharacter> characters = ArenaDecks.DeckCharacters(userDb, deckType, deckNumber);
        int[] powers = Calculate(userDb, masterDb, characters);
        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].Power = powers[i];
        }

        deck.Power = powers.Sum();

        EntityIUserDeckTypeNote? note = userDb.EntityIUserDeckTypeNote.FirstOrDefault(n => n.DeckType == deckType);
        if (note == null)
        {
            note = new EntityIUserDeckTypeNote { UserId = userDb.UserId, DeckType = deckType };
            userDb.EntityIUserDeckTypeNote.Add(note);
        }

        note.MaxDeckPower = Math.Max(note.MaxDeckPower, deck.Power);
    }

    /// <summary>The force of each of the characters, in the order given (a deck's force is their sum).</summary>
    public static int[] Calculate(DarkUserMemoryDatabase userDb, DarkMasterMemoryDatabase masterDb, IReadOnlyList<EntityIUserDeckCharacter> characters)
    {
        Context context = new(masterDb);
        List<Actor> actors = [.. characters.Select(c => BuildActor(context, userDb, c))];

        foreach (Actor actor in actors)
        {
            actor.Pool = [.. actor.Statuses];
            foreach (Actor other in actors)
            {
                if (other != actor)
                {
                    actor.Pool.AddRange(other.Statuses.Where(s => s.Scope == AbilityBehaviourStatusApplyScopeType.PARTY));
                }
            }
        }

        foreach (Actor actor in actors)
        {
            StatPower(context, actor);
        }

        return [.. actors.Select(a => SkillPower(context, a, actors) + a.HpPower + a.AttackPower + a.VitalityPower)];
    }

    // ---- building the data the formula reads ----

    private static Actor BuildActor(Context context, DarkUserMemoryDatabase userDb, EntityIUserDeckCharacter deckCharacter)
    {
        DarkMasterMemoryDatabase masterDb = context.MasterDb;
        EntityIUserCostume costume = userDb.EntityIUserCostume.First(c => c.UserCostumeUuid == deckCharacter.UserCostumeUuid);
        EntityMCostume costumeMaster = masterDb.EntityMCostume.Find(costume.CostumeId)!;
        Actor actor = new()
        {
            Costume = costume,
            CostumeMaster = costumeMaster,
            Skillful = costumeMaster.SkillfulWeaponType,
            ProperAttributeBonus = masterDb.EntityMCostumeProperAttributeHpBonus.Find(costume.CostumeId),
        };

        EntityMCostumeBaseStatus baseStatus = masterDb.EntityMCostumeBaseStatus.Find(costumeMaster.CostumeBaseStatusId)!;
        EntityMCostumeStatusCalculation calc = masterDb.EntityMCostumeStatusCalculation.Find(costumeMaster.CostumeStatusCalculationId)!;
        int level = costume.Level;
        actor.CostumeBase = new Status(
            Agility: context.Growth(baseStatus.Agility, calc.AgilityNumericalFunctionId, level, context.CostumeThreshold, context.CostumeOverCoefficient),
            Attack: context.Growth(baseStatus.Attack, calc.AttackNumericalFunctionId, level, context.CostumeThreshold, context.CostumeOverCoefficient),
            CriticalAttack: context.Growth(baseStatus.CriticalAttackRatioPermil, calc.CriticalAttackRatioPermilNumericalFunctionId, level, context.CostumeThreshold, context.CostumeOverCoefficient),
            CriticalRatio: context.Growth(baseStatus.CriticalRatioPermil, calc.CriticalRatioPermilNumericalFunctionId, level, context.CostumeThreshold, context.CostumeOverCoefficient),
            EvasionRatio: context.Growth(baseStatus.EvasionRatioPermil, calc.EvasionRatioPermilNumericalFunctionId, level, context.CostumeThreshold, context.CostumeOverCoefficient),
            Hp: context.Growth(baseStatus.Hp, calc.HpNumericalFunctionId, level, context.CostumeThreshold, context.CostumeOverCoefficient),
            Vitality: context.Growth(baseStatus.Vitality, calc.VitalityNumericalFunctionId, level, context.CostumeThreshold, context.CostumeOverCoefficient));

        actor.Main = BuildWeapon(context, userDb, deckCharacter.MainUserWeaponUuid);
        foreach (EntityIUserDeckSubWeaponGroup sub in userDb.EntityIUserDeckSubWeaponGroup
            .Where(s => s.UserDeckCharacterUuid == deckCharacter.UserDeckCharacterUuid)
            .OrderBy(s => s.SortOrder)
            .Take(MaxSubWeapons))
        {
            actor.Subs.Add(BuildWeapon(context, userDb, sub.UserWeaponUuid));
        }

        // The abilities: their status changes feed the stats, their passive skills and the skills the force.
        List<AbilityData> abilities = actor.Abilities;
        foreach (EntityMCostumeAbilityGroup group in masterDb.EntityMCostumeAbilityGroup
            .Where(g => g.CostumeAbilityGroupId == costumeMaster.CostumeAbilityGroupId)
            .OrderBy(g => g.SlotNumber))
        {
            EntityMCostumeAbilityLevelGroup? levelGroup = Closest(
                masterDb.EntityMCostumeAbilityLevelGroup.Where(g => g.CostumeAbilityLevelGroupId == group.CostumeAbilityLevelGroupId),
                g => g.CostumeLimitBreakCountLowerLimit, costume.LimitBreakCount);
            AddAbility(context, abilities, group.AbilityId, levelGroup?.AbilityLevel ?? 0);
        }

        // The costume's lottery effects: abilities (counted like the costume's own) and status ups (see CostumeStatus).
        foreach (EntityIUserCostumeLotteryEffectAbility lottery in userDb.EntityIUserCostumeLotteryEffectAbility.Where(a => a.UserCostumeUuid == costume.UserCostumeUuid))
        {
            AddAbility(context, abilities, lottery.AbilityId, lottery.AbilityLevel);
        }

        EntityMCostumeAwaken? awaken = masterDb.EntityMCostumeAwaken.Find(costume.CostumeId);
        if (awaken != null)
        {
            foreach (EntityMCostumeAwakenEffectGroup effect in masterDb.EntityMCostumeAwakenEffectGroup.Where(e =>
                e.CostumeAwakenEffectGroupId == awaken.CostumeAwakenEffectGroupId
                && e.CostumeAwakenEffectType == CostumeAwakenEffectType.ABILITY
                && e.AwakenStep <= costume.AwakenCount))
            {
                EntityMCostumeAwakenAbility? awakenAbility = masterDb.EntityMCostumeAwakenAbility.Find(effect.CostumeAwakenEffectId);
                if (awakenAbility != null)
                {
                    AddAbility(context, abilities, awakenAbility.AbilityId, awakenAbility.AbilityLevel);
                }
            }
        }

        EntityMCharacter? character = masterDb.EntityMCharacter.Find(costumeMaster.CharacterId);
        int rank = userDb.EntityIUserCharacter.FirstOrDefault(c => c.CharacterId == costumeMaster.CharacterId)?.Level ?? 1;
        if (character != null)
        {
            foreach (EntityMCharacterLevelBonusAbilityGroup group in masterDb.EntityMCharacterLevelBonusAbilityGroup.Where(g =>
                g.CharacterLevelBonusAbilityGroupId == character.CharacterLevelBonusAbilityGroupId && g.ActivationCharacterLevel <= rank))
            {
                AddAbility(context, abilities, group.AbilityId, group.AbilityLevel);
            }
        }

        foreach (EntityIUserCharacterBoardAbility boardAbility in userDb.EntityIUserCharacterBoardAbility.Where(a => a.CharacterId == costumeMaster.CharacterId))
        {
            AddAbility(context, abilities, boardAbility.AbilityId, boardAbility.Level);
        }

        EntityIUserCompanion? userCompanion = userDb.EntityIUserCompanion.FirstOrDefault(c => c.UserCompanionUuid == deckCharacter.UserCompanionUuid);
        if (userCompanion != null)
        {
            actor.Companion = BuildCompanion(context, abilities, userCompanion);
        }

        foreach (Weapon weapon in new[] { actor.Main }.Concat(actor.Subs))
        {
            abilities.AddRange(weapon.Abilities);
        }

        EntityIUserThought? userThought = userDb.EntityIUserThought.FirstOrDefault(t => t.UserThoughtUuid == deckCharacter.UserThoughtUuid);
        EntityMThought? thought = userThought == null ? null : masterDb.EntityMThought.Find(userThought.ThoughtId);
        if (thought != null)
        {
            AddAbility(context, abilities, thought.AbilityId, thought.AbilityLevel);
        }

        EntityIUserCostumeActiveSkill? activeSkill = userDb.EntityIUserCostumeActiveSkill.FirstOrDefault(s => s.UserCostumeUuid == costume.UserCostumeUuid);
        EntityMCostumeActiveSkillGroup? activeSkillGroup = masterDb.EntityMCostumeActiveSkillGroup
            .FirstOrDefault(g => g.CostumeActiveSkillGroupId == costumeMaster.CostumeActiveSkillGroupId && g.CostumeLimitBreakCountLowerLimit == 0);
        if (activeSkill != null && activeSkillGroup != null)
        {
            actor.ActiveSkill = context.Skill(activeSkillGroup.CostumeActiveSkillId, activeSkill.Level);
        }

        foreach (EntityIUserDeckPartsGroup partsGroup in userDb.EntityIUserDeckPartsGroup
            .Where(p => p.UserDeckCharacterUuid == deckCharacter.UserDeckCharacterUuid)
            .OrderBy(p => p.SortOrder)
            .Take(MaxParts))
        {
            EntityIUserParts? parts = userDb.EntityIUserParts.FirstOrDefault(p => p.UserPartsUuid == partsGroup.UserPartsUuid);
            EntityMPartsStatusMain? main = parts == null ? null : masterDb.EntityMPartsStatusMain.Find(parts.PartsStatusMainId);
            if (parts != null && main != null)
            {
                actor.Parts.Add(new Part(parts, main, [.. userDb.EntityIUserPartsStatusSub.Where(s => s.UserPartsUuid == parts.UserPartsUuid).OrderBy(s => s.StatusIndex)]));
            }
        }

        // A parts series' bonus is the one whose set count equals the number of parts of that series worn.
        foreach (IGrouping<int, Part> series in actor.Parts.GroupBy(p => SeriesOf(masterDb, p.Parts.PartsId)))
        {
            EntityMPartsSeries? seriesMaster = masterDb.EntityMPartsSeries.Find(series.Key);
            if (seriesMaster == null)
            {
                continue;
            }

            foreach (EntityMPartsSeriesBonusAbilityGroup bonus in masterDb.EntityMPartsSeriesBonusAbilityGroup.Where(b =>
                b.PartsSeriesBonusAbilityGroupId == seriesMaster.PartsSeriesBonusAbilityGroupId && b.SetCount == series.Count()))
            {
                AddAbility(context, abilities, bonus.AbilityId, bonus.AbilityLevel);
            }
        }

        actor.Statuses = [.. abilities.SelectMany(a => a.Statuses)];

        actor.Board = [.. userDb.EntityIUserCharacterBoardStatusUp.Where(s => s.CharacterId == costumeMaster.CharacterId)
            .Select(s => (s.StatusCalculationType, new Status(s.Agility, s.Attack, s.CriticalAttack, s.CriticalRatio, 0, s.Hp, s.Vitality)))];
        actor.LevelBonus = [.. userDb.EntityIUserCharacterCostumeLevelBonus.Where(s => s.CharacterId == costumeMaster.CharacterId)
            .Select(s => (s.StatusCalculationType, new Status(s.Agility, s.Attack, s.CriticalAttack, s.CriticalRatio, 0, s.Hp, s.Vitality)))];
        actor.CostumeAwaken = [.. userDb.EntityIUserCostumeAwakenStatusUp.Where(s => s.UserCostumeUuid == costume.UserCostumeUuid)
            .Select(s => (s.StatusCalculationType, new Status(s.Agility, s.Attack, s.CriticalAttack, s.CriticalRatio, 0, s.Hp, s.Vitality)))];
        actor.LotteryEffect = [.. userDb.EntityIUserCostumeLotteryEffectStatusUp.Where(s => s.UserCostumeUuid == costume.UserCostumeUuid)
            .Select(s => (s.StatusCalculationType, new Status(s.Agility, s.Attack, s.CriticalAttack, s.CriticalRatio, 0, s.Hp, s.Vitality)))];

        StainedGlassStatusUpInfo glass = ArenaDecks.StainedGlass(userDb, masterDb, costumeMaster.CharacterId, costumeMaster.SkillfulWeaponType);
        actor.StainedGlass = new Status(glass.Agility, glass.Attack, glass.CriticalAttack, glass.CriticalRatio, 0, glass.Hp, glass.Vitality);

        return actor;
    }

    private static int SeriesOf(DarkMasterMemoryDatabase masterDb, int partsId)
    {
        EntityMParts? parts = masterDb.EntityMParts.Find(partsId);
        return parts == null ? 0 : masterDb.EntityMPartsGroup.Find(parts.PartsGroupId)?.PartsSeriesId ?? 0;
    }

    private static Weapon BuildWeapon(Context context, DarkUserMemoryDatabase userDb, string userWeaponUuid)
    {
        DarkMasterMemoryDatabase masterDb = context.MasterDb;
        EntityIUserWeapon userWeapon = userDb.EntityIUserWeapon.First(w => w.UserWeaponUuid == userWeaponUuid);
        EntityMWeapon master = masterDb.EntityMWeapon.Find(userWeapon.WeaponId)!;
        EntityMWeaponBaseStatus baseStatus = masterDb.EntityMWeaponBaseStatus.Find(master.WeaponBaseStatusId)!;
        EntityMWeaponStatusCalculation calc = masterDb.EntityMWeaponStatusCalculation.Find(master.WeaponStatusCalculationId)!;
        int level = userWeapon.Level;

        Weapon weapon = new()
        {
            Type = master.WeaponType,
            Attribute = master.AttributeType,
            Attack = context.Growth(baseStatus.Attack, calc.AttackNumericalFunctionId, level, context.WeaponThreshold, context.WeaponOverCoefficient),
            Hp = context.Growth(baseStatus.Hp, calc.HpNumericalFunctionId, level, context.WeaponThreshold, context.WeaponOverCoefficient),
            Vitality = context.Growth(baseStatus.Vitality, calc.VitalityNumericalFunctionId, level, context.WeaponThreshold, context.WeaponOverCoefficient),
        };

        // An awakened weapon's status ups, one added and one multiplying total.
        EntityMWeaponAwaken? awaken = userDb.EntityIUserWeaponAwaken.Any(a => a.UserWeaponUuid == userWeaponUuid)
            ? masterDb.EntityMWeaponAwaken.Find(userWeapon.WeaponId)
            : null;
        if (awaken != null)
        {
            List<EntityMWeaponAwakenStatusUpGroup> ups = [];
            foreach (EntityMWeaponAwakenEffectGroup effect in masterDb.EntityMWeaponAwakenEffectGroup.Where(e => e.WeaponAwakenEffectGroupId == awaken.WeaponAwakenEffectGroupId))
            {
                if (effect.WeaponAwakenEffectType == WeaponAwakenStatusUpEffect)
                {
                    ups.AddRange(masterDb.EntityMWeaponAwakenStatusUpGroup.Where(s => s.WeaponAwakenStatusUpGroupId == effect.WeaponAwakenEffectId));
                }
                else if (effect.WeaponAwakenEffectType == WeaponAwakenAbilityEffect
                    && masterDb.EntityMWeaponAwakenAbility.Find(effect.WeaponAwakenEffectId) is { } awakenAbility)
                {
                    // An awakened weapon has one more ability, counted like its own.
                    AddAbility(context, weapon.Abilities, awakenAbility.AbilityId, awakenAbility.AbilityLevel);
                }
            }

            foreach (StatusCalculationType type in new[] { StatusCalculationType.MULTIPLY, StatusCalculationType.ADD })
            {
                Status total = default;
                foreach (EntityMWeaponAwakenStatusUpGroup up in ups.Where(u => u.StatusCalculationType == type))
                {
                    total = total.With(up.StatusKindType, up.EffectValue);
                }

                weapon.Awaken.Add((type, total));
            }
        }

        foreach (EntityMWeaponAbilityGroup group in masterDb.EntityMWeaponAbilityGroup.Where(g => g.WeaponAbilityGroupId == master.WeaponAbilityGroupId).OrderBy(g => g.SlotNumber))
        {
            int abilityLevel = userDb.EntityIUserWeaponAbility.FirstOrDefault(a => a.UserWeaponUuid == userWeaponUuid && a.SlotNumber == group.SlotNumber)?.Level ?? 0;
            AddAbility(context, weapon.Abilities, group.AbilityId, abilityLevel);
        }

        foreach (EntityMWeaponSkillGroup group in masterDb.EntityMWeaponSkillGroup.Where(g => g.WeaponSkillGroupId == master.WeaponSkillGroupId).OrderBy(g => g.SlotNumber))
        {
            int skillLevel = userDb.EntityIUserWeaponSkill.FirstOrDefault(s => s.UserWeaponUuid == userWeaponUuid && s.SlotNumber == group.SlotNumber)?.Level ?? 0;
            if (skillLevel > 0)
            {
                weapon.Skills.Add(context.Skill(group.SkillId, skillLevel));
            }
        }

        return weapon;
    }

    private static CompanionData BuildCompanion(Context context, List<AbilityData> abilities, EntityIUserCompanion userCompanion)
    {
        DarkMasterMemoryDatabase masterDb = context.MasterDb;
        EntityMCompanion master = masterDb.EntityMCompanion.Find(userCompanion.CompanionId)!;
        EntityMCompanionBaseStatus baseStatus = masterDb.EntityMCompanionBaseStatus.Find(master.CompanionBaseStatusId)!;
        EntityMCompanionStatusCalculation calc = masterDb.EntityMCompanionStatusCalculation.Find(master.CompanionStatusCalculationId)!;
        int level = userCompanion.Level;

        EntityMCompanionSkillLevel? skillLevel = Closest(masterDb.EntityMCompanionSkillLevel, l => l.CompanionLevelLowerLimit, level);
        EntityMCompanionAbilityLevel? abilityLevel = Closest(masterDb.EntityMCompanionAbilityLevel, l => l.CompanionLevelLowerLimit, level);
        EntityMCompanionAbilityGroup? abilityGroup = masterDb.EntityMCompanionAbilityGroup
            .FirstOrDefault(g => g.CompanionAbilityGroupId == master.CompanionAbilityGroupId && g.SlotNumber == 1);
        if (abilityGroup != null && abilityLevel != null)
        {
            AddAbility(context, abilities, abilityGroup.AbilityId, abilityLevel.AbilityLevel);
        }

        return new CompanionData
        {
            Attribute = master.AttributeType,
            Attack = baseStatus.Attack + context.Evaluate(calc.AttackNumericalFunctionId, level),
            Hp = baseStatus.Hp + context.Evaluate(calc.HpNumericalFunctionId, level),
            Vitality = baseStatus.Vitality + context.Evaluate(calc.VitalityNumericalFunctionId, level),
            Skill = skillLevel == null ? null : context.Skill(master.SkillId, skillLevel.SkillLevel),
        };
    }

    /// <summary>Adds the ability at the level (nothing when it has none: level 0, or no such ability).</summary>
    private static void AddAbility(Context context, List<AbilityData> list, int abilityId, int level)
    {
        DarkMasterMemoryDatabase masterDb = context.MasterDb;
        EntityMAbility? ability = level <= 0 ? null : masterDb.EntityMAbility.Find(abilityId);
        EntityMAbilityLevelGroup? levelGroup = ability == null
            ? null
            : Closest(masterDb.EntityMAbilityLevelGroup.Where(g => g.AbilityLevelGroupId == ability.AbilityLevelGroupId), g => g.LevelLowerLimit, level);
        EntityMAbilityDetail? detail = levelGroup == null ? null : masterDb.EntityMAbilityDetail.Find(levelGroup.AbilityDetailId);
        if (detail == null)
        {
            return;
        }

        AbilityData data = new();
        foreach (EntityMAbilityBehaviourGroup behaviourGroup in masterDb.EntityMAbilityBehaviourGroup
            .Where(g => g.AbilityBehaviourGroupId == detail.AbilityBehaviourGroupId)
            .OrderBy(g => g.AbilityBehaviourIndex))
        {
            EntityMAbilityBehaviour? behaviour = masterDb.EntityMAbilityBehaviour.Find(behaviourGroup.AbilityBehaviourId);
            if (behaviour == null)
            {
                continue;
            }

            if (behaviour.AbilityBehaviourType == AbilityBehaviourType.PASSIVE_SKILL)
            {
                EntityMAbilityBehaviourActionPassiveSkill? passive = masterDb.EntityMAbilityBehaviourActionPassiveSkill.Find(behaviour.AbilityBehaviourActionId);
                if (passive != null)
                {
                    data.Skills.Add(context.SkillOfDetail(passive.SkillDetailId));
                }
            }
            else if (behaviour.AbilityBehaviourType == AbilityBehaviourType.STATUS)
            {
                EntityMAbilityBehaviourActionStatus? action = masterDb.EntityMAbilityBehaviourActionStatus.Find(behaviour.AbilityBehaviourActionId);
                EntityMAbilityStatus? status = action == null ? null : masterDb.EntityMAbilityStatus.Find(action.AbilityStatusId);
                if (action != null && status != null)
                {
                    data.Statuses.Add(new AbilityStatus(
                        action.AttributeConditionType, action.ApplyScopeType, action.AbilityBehaviourStatusChangeType, action.AbilityOrganizationConditionType,
                        new Status(status.Agility, status.Attack, status.CriticalAttackRatioPermil, status.CriticalRatioPermil, status.EvasionRatioPermil, status.Hp, status.Vitality)));
                }
            }
        }

        list.Add(data);
    }

    /// <summary>The row with the greatest key not above <paramref name="value"/> (the client's "closest lower limit" lookup).</summary>
    private static T? Closest<T>(IEnumerable<T> rows, Func<T, int> key, int value) where T : class
        => rows.Where(r => key(r) <= value).OrderByDescending(key).FirstOrDefault();

    // ---- the formula ----

    /// <summary>The stat power of the actor's costume, weapons, companion and parts (<c>CalculateDeckActorBasePower</c>).</summary>
    private static void StatPower(Context context, Actor actor)
    {
        Status costume = CostumeStatus(actor);
        actor.HpPower = context.Power(costume.Hp, PowerCalculationConstantValueType.COSTUME_HP_COEFFICIENT);
        actor.AttackPower = context.Power(costume.Attack, PowerCalculationConstantValueType.COSTUME_ATTACK_COEFFICIENT);
        actor.VitalityPower = context.Power(costume.Vitality, PowerCalculationConstantValueType.COSTUME_VITALITY_COEFFICIENT);

        bool isMain = true;
        foreach (Weapon weapon in new[] { actor.Main }.Concat(actor.Subs))
        {
            Status status = WeaponStatus(actor, weapon, isMain);
            actor.HpPower += context.Power(status.Hp, PowerCalculationConstantValueType.WEAPON_HP_COEFFICIENT);
            actor.AttackPower += context.Power(status.Attack, PowerCalculationConstantValueType.WEAPON_ATTACK_COEFFICIENT);
            actor.VitalityPower += context.Power(status.Vitality, PowerCalculationConstantValueType.WEAPON_VITALITY_COEFFICIENT);
            isMain = false;
        }

        if (actor.Companion != null)
        {
            Status status = CompanionStatus(actor, actor.Companion);
            actor.HpPower += context.Power(status.Hp, PowerCalculationConstantValueType.COMPANION_HP_COEFFICIENT);
            actor.AttackPower += context.Power(status.Attack, PowerCalculationConstantValueType.COMPANION_ATTACK_COEFFICIENT);
            actor.VitalityPower += context.Power(status.Vitality, PowerCalculationConstantValueType.COMPANION_VITALITY_COEFFICIENT);
        }

        if (actor.Parts.Count > 0)
        {
            (int hp, int attack, int vitality) = PartsPower(context, actor.Parts);
            actor.HpPower += hp;
            actor.AttackPower += attack;
            actor.VitalityPower += vitality;
        }
    }

    private static Status CostumeStatus(Actor actor)
    {
        Status baseStatus = actor.CostumeBase;
        Status result = baseStatus
            + AbilityDiff(baseStatus, AttributeType.UNKNOWN, AbilityBehaviourStatusOrganizationConditionType.COSTUME, actor.Pool)
            + Changes(baseStatus, actor.Board)
            + Changes(baseStatus, actor.LevelBonus)
            + Changes(baseStatus, actor.CostumeAwaken)
            + actor.StainedGlass
            + Changes(baseStatus, actor.LotteryEffect);

        result = result with
        {
            Agility = Math.Max(result.Agility, 0),
            Attack = Math.Max(result.Attack, 1),
            CriticalAttack = Math.Max(result.CriticalAttack, 1000),
            CriticalRatio = Math.Max(result.CriticalRatio, 0),
            EvasionRatio = Math.Max(result.EvasionRatio, 0),
            Hp = Math.Max(result.Hp, 1),
            Vitality = Math.Max(result.Vitality, 1),
        };

        // A costume gains HP for each of its weapons of the attribute it is proper to.
        EntityMCostumeProperAttributeHpBonus? bonus = actor.ProperAttributeBonus;
        if (bonus != null)
        {
            int extra = actor.Main.Attribute == bonus.CostumeProperAttributeType ? bonus.MainWeaponHpAdditionalValue : 0;
            extra += actor.Subs.Count(w => w.Attribute == bonus.CostumeProperAttributeType) * bonus.SubWeaponHpAdditionalValue;
            result = result with { Hp = result.Hp + extra };
        }

        return result;
    }

    /// <summary>A weapon not of the costume's type counts half, and a sub weapon half again (rounding toward zero).</summary>
    private static Status WeaponStatus(Actor actor, Weapon weapon, bool isMain)
    {
        int attack = weapon.Attack;
        int hp = weapon.Hp;
        int vitality = weapon.Vitality;
        if (weapon.Type != actor.Skillful)
        {
            vitality = Halve(vitality);
            hp = Halve(hp);
            attack = Halve(attack);
        }

        if (!isMain)
        {
            hp = Halve(hp);
            vitality = Halve(vitality);
            attack = Halve(attack);
        }

        Status baseStatus = new(0, attack, 0, 0, 0, hp, vitality);
        return AtLeastOne(baseStatus
            + AbilityDiff(baseStatus, weapon.Attribute, AbilityBehaviourStatusOrganizationConditionType.WEAPON, actor.Pool)
            + Changes(baseStatus, weapon.Awaken));
    }

    /// <summary>Weapon and companion stats are at least 1 for attack, HP and vitality (<c>CalculatorStatus.kMin…Value</c>); nothing else counts.</summary>
    private static Status AtLeastOne(Status status) => new(0, Math.Max(status.Attack, 1), 0, 0, 0, Math.Max(status.Hp, 1), Math.Max(status.Vitality, 1));

    private static Status CompanionStatus(Actor actor, CompanionData companion)
    {
        Status baseStatus = new(0, companion.Attack, 0, 0, 0, companion.Hp, companion.Vitality);
        return AtLeastOne(baseStatus + AbilityDiff(baseStatus, companion.Attribute, AbilityBehaviourStatusOrganizationConditionType.COMPANION, actor.Pool));
    }

    private static int Halve(int value) => (value < 0 ? value + 1 : value) >> 1;

    /// <summary>What the abilities that apply here change: the added values, and the multiplying ones as parts per thousand of the base.</summary>
    private static Status AbilityDiff(Status baseStatus, AttributeType attribute, AbilityBehaviourStatusOrganizationConditionType organization, List<AbilityStatus> pool)
    {
        List<(StatusCalculationType, Status)> changes = [];
        foreach (AbilityStatus ability in pool)
        {
            bool organizationMatches = (ability.Organization == AbilityBehaviourStatusOrganizationConditionType.ALL
                    && (organization == AbilityBehaviourStatusOrganizationConditionType.COSTUME || ability.ChangeType != AbilityBehaviourStatusChangeType.ADD))
                || ability.Organization == organization;
            if (organizationMatches && AttributeMatches(attribute, ability.Attribute))
            {
                changes.Add(((StatusCalculationType)(int)ability.ChangeType, ability.Value));
            }
        }

        return Changes(baseStatus, changes);
    }

    /// <summary>The added changes plus the multiplying ones applied to the base (parts per thousand, rounding toward zero).</summary>
    private static Status Changes(Status baseStatus, IEnumerable<(StatusCalculationType Type, Status Value)> changes)
    {
        Status add = default;
        Status multiply = default;
        foreach ((StatusCalculationType type, Status value) in changes)
        {
            if (type == StatusCalculationType.MULTIPLY)
            {
                multiply += value;
            }
            else if (type == StatusCalculationType.ADD)
            {
                add += value;
            }
        }

        return add + baseStatus.Permil(multiply);
    }

    private static bool AttributeMatches(AttributeType attribute, AttributeConditionType condition)
    {
        if (condition == AttributeConditionType.ALL)
        {
            return true;
        }

        return condition switch
        {
            AttributeConditionType.DARK => attribute == AttributeType.DARK,
            AttributeConditionType.FIRE => attribute == AttributeType.FIRE,
            AttributeConditionType.LIGHT => attribute == AttributeType.LIGHT,
            AttributeConditionType.WATER => attribute == AttributeType.WATER,
            AttributeConditionType.WIND => attribute == AttributeType.WIND,
            AttributeConditionType.NOTHING => attribute == AttributeType.NOTHING,
            _ => false,
        };
    }

    /// <summary>
    /// The power of the parts, as the client adds it up (<c>CalculateDeckPartsStatusPower</c>): each main and sub status
    /// is worked out alone and goes to the total of its own kind: attack for kinds 1 to 5, then HP, then vitality.
    /// </summary>
    private static (int Hp, int Attack, int Vitality) PartsPower(Context context, List<Part> parts)
    {
        int hp = 0;
        int attack = 0;
        int vitality = 0;
        foreach (Part part in parts)
        {
            EntityMPartsStatusMain main = part.Main;
            Add(main.StatusKindType, main.StatusCalculationType, default(Status).With(main.StatusKindType, context.PartsMainValue(main, part.Parts.Level)));
            foreach (EntityIUserPartsStatusSub sub in part.Subs)
            {
                Add(sub.StatusKindType, sub.StatusCalculationType, default(Status).With(sub.StatusKindType, sub.StatusChangeValue));
            }
        }

        return (hp, attack, vitality);

        void Add(StatusKindType kind, StatusCalculationType type, Status status)
        {
            // Only added and multiplying statuses count (an unknown type has no power anyway).
            int power = PartsStatusPower(context, status, type);
            if (type is not (StatusCalculationType.ADD or StatusCalculationType.MULTIPLY))
            {
                return;
            }

            if (kind is >= StatusKindType.AGILITY and <= StatusKindType.EVASION_RATIO)
            {
                attack += power;
            }
            else if (kind == StatusKindType.VITALITY)
            {
                vitality += power;
            }
            else if (kind == StatusKindType.HP)
            {
                hp += power;
            }
        }
    }

    /// <summary>The power of one parts status, added (coefficients) or multiplying (base values), each as parts per thousand.</summary>
    private static int PartsStatusPower(Context context, Status status, StatusCalculationType type)
    {
        if (type == StatusCalculationType.UNKNOWN)
        {
            return 0;
        }

        bool multiply = type == StatusCalculationType.MULTIPLY;
        return context.Power(status.Agility, multiply ? PowerCalculationConstantValueType.PARTS_MUL_POWER_AGILITY_BASE_VALUE : PowerCalculationConstantValueType.PARTS_ADD_POWER_AGILITY_COEFFICIENT)
            + context.Power(status.Attack, multiply ? PowerCalculationConstantValueType.PARTS_MUL_POWER_ATTACK_BASE_VALUE : PowerCalculationConstantValueType.PARTS_ADD_POWER_ATTACK_COEFFICIENT)
            + context.Power(status.CriticalAttack, multiply ? PowerCalculationConstantValueType.PARTS_MUL_POWER_CRITICAL_ATTACK_BASE_VALUE : PowerCalculationConstantValueType.PARTS_ADD_POWER_CRITICAL_ATTACK_COEFFICIENT)
            + context.Power(status.CriticalRatio, multiply ? PowerCalculationConstantValueType.PARTS_MUL_POWER_CRITICAL_RATIO_BASE_VALUE : PowerCalculationConstantValueType.PARTS_ADD_POWER_CRITICAL_RATIO_COEFFICIENT)
            + context.Power(status.EvasionRatio, multiply ? PowerCalculationConstantValueType.PARTS_MUL_POWER_EVASION_RATIO_BASE_VALUE : PowerCalculationConstantValueType.PARTS_ADD_POWER_EVASION_RATIO_COEFFICIENT)
            + context.Power(status.Hp, multiply ? PowerCalculationConstantValueType.PARTS_MUL_POWER_HP_BASE_VALUE : PowerCalculationConstantValueType.PARTS_ADD_POWER_HP_COEFFICIENT)
            + context.Power(status.Vitality, multiply ? PowerCalculationConstantValueType.PARTS_MUL_POWER_VITALITY_BASE_VALUE : PowerCalculationConstantValueType.PARTS_ADD_POWER_VITALITY_COEFFICIENT);
    }

    /// <summary>The power of every skill and ability of the actor.</summary>
    private static int SkillPower(Context context, Actor actor, List<Actor> party)
    {
        int total = 0;
        if (actor.ActiveSkill != null)
        {
            total += SkillPowerOf(context, actor, party, actor.ActiveSkill);
        }

        total += actor.Main.Skills.Sum(s => SkillPowerOf(context, actor, party, s));
        if (actor.Companion?.Skill != null)
        {
            total += SkillPowerOf(context, actor, party, actor.Companion.Skill);
        }

        total += actor.Abilities.Sum(a => a.Skills.Sum(s => SkillPowerOf(context, actor, party, s)));
        return total;
    }

    /// <summary>A skill's base value plus its share of the power of the actor or the party (<c>CalculateStatusReferenceValue</c>).</summary>
    private static int SkillPowerOf(Context context, Actor owner, List<Actor> party, SkillPowerData skill)
    {
        int reference = 0;
        if (skill.ReferenceType is not (PowerCalculationReferenceStatusType.UNKNOWN or PowerCalculationReferenceStatusType.NONE))
        {
            foreach (PowerReference setting in skill.References)
            {
                foreach (Actor actor in party)
                {
                    if (skill.ReferenceType == PowerCalculationReferenceStatusType.SELF && actor != owner)
                    {
                        continue;
                    }

                    if (!AttributeMatches(actor.Main.Attribute, setting.Condition))
                    {
                        continue;
                    }

                    // Kinds 1 to 5 count the attack power, vitality and HP their own, anything else nothing.
                    int power = setting.Kind switch
                    {
                        StatusKindType.AGILITY or StatusKindType.ATTACK or StatusKindType.CRITICAL_ATTACK
                            or StatusKindType.CRITICAL_RATIO or StatusKindType.EVASION_RATIO => actor.AttackPower,
                        StatusKindType.VITALITY => actor.VitalityPower,
                        StatusKindType.HP => actor.HpPower,
                        _ => 0,
                    };
                    reference += (int)((long)power * setting.CoefficientPermil / 1000);
                }
            }
        }

        return reference + skill.BaseValue;
    }

    // ---- data ----

    private readonly record struct Status(int Agility, int Attack, int CriticalAttack, int CriticalRatio, int EvasionRatio, int Hp, int Vitality)
    {
        public static Status operator +(Status a, Status b) => new(
            a.Agility + b.Agility, a.Attack + b.Attack, a.CriticalAttack + b.CriticalAttack, a.CriticalRatio + b.CriticalRatio,
            a.EvasionRatio + b.EvasionRatio, a.Hp + b.Hp, a.Vitality + b.Vitality);

        /// <summary>This status times <paramref name="permil"/> (per kind), parts per thousand, rounding toward zero.</summary>
        public Status Permil(Status permil) => new(
            Mul(Agility, permil.Agility), Mul(Attack, permil.Attack), Mul(CriticalAttack, permil.CriticalAttack),
            Mul(CriticalRatio, permil.CriticalRatio), Mul(EvasionRatio, permil.EvasionRatio), Mul(Hp, permil.Hp), Mul(Vitality, permil.Vitality));

        public Status With(StatusKindType kind, int value) => kind switch
        {
            StatusKindType.AGILITY => this with { Agility = Agility + value },
            StatusKindType.ATTACK => this with { Attack = Attack + value },
            StatusKindType.CRITICAL_ATTACK => this with { CriticalAttack = CriticalAttack + value },
            StatusKindType.CRITICAL_RATIO => this with { CriticalRatio = CriticalRatio + value },
            StatusKindType.EVASION_RATIO => this with { EvasionRatio = EvasionRatio + value },
            StatusKindType.HP => this with { Hp = Hp + value },
            StatusKindType.VITALITY => this with { Vitality = Vitality + value },
            _ => this,
        };

        private static int Mul(int value, int permil) => (int)((long)value * permil / 1000);
    }

    private sealed record AbilityStatus(
        AttributeConditionType Attribute, AbilityBehaviourStatusApplyScopeType Scope, AbilityBehaviourStatusChangeType ChangeType,
        AbilityBehaviourStatusOrganizationConditionType Organization, Status Value);

    private sealed record PowerReference(StatusKindType Kind, AttributeConditionType Condition, int CoefficientPermil);

    private sealed record SkillPowerData(int BaseValue, PowerCalculationReferenceStatusType ReferenceType, List<PowerReference> References);

    private sealed class AbilityData
    {
        public List<AbilityStatus> Statuses { get; } = [];

        public List<SkillPowerData> Skills { get; } = [];
    }

    private sealed class Weapon
    {
        public WeaponType Type;
        public AttributeType Attribute;
        public int Attack;
        public int Hp;
        public int Vitality;
        public List<(StatusCalculationType Type, Status Value)> Awaken { get; } = [];
        public List<AbilityData> Abilities { get; } = [];
        public List<SkillPowerData> Skills { get; } = [];
    }

    private sealed class CompanionData
    {
        public AttributeType Attribute;
        public int Attack;
        public int Hp;
        public int Vitality;
        public SkillPowerData? Skill;
    }

    private sealed record Part(EntityIUserParts Parts, EntityMPartsStatusMain Main, List<EntityIUserPartsStatusSub> Subs);

    private sealed class Actor
    {
        public required EntityIUserCostume Costume;
        public required EntityMCostume CostumeMaster;
        public WeaponType Skillful;
        public Status CostumeBase;
        public Weapon Main = null!;
        public List<Weapon> Subs { get; } = [];
        public CompanionData? Companion;
        public SkillPowerData? ActiveSkill;
        public List<AbilityData> Abilities { get; } = [];
        public List<AbilityStatus> Statuses = [];
        public List<AbilityStatus> Pool = [];
        public List<(StatusCalculationType Type, Status Value)> Board = [];
        public List<(StatusCalculationType Type, Status Value)> LevelBonus = [];
        public List<(StatusCalculationType Type, Status Value)> CostumeAwaken = [];
        public List<(StatusCalculationType Type, Status Value)> LotteryEffect = [];
        public Status StainedGlass;
        public List<Part> Parts { get; } = [];
        public int HpPower;
        public int AttackPower;
        public int VitalityPower;

        public EntityMCostumeProperAttributeHpBonus? ProperAttributeBonus;
    }

    /// <summary>The master data lookups the formula needs, indexed once per calculation.</summary>
    private sealed class Context
    {
        private readonly ILookup<int, EntityMNumericalFunctionParameterGroup> _parameters;
        private readonly Dictionary<PowerCalculationConstantValueType, int> _constants;

        public Context(DarkMasterMemoryDatabase masterDb)
        {
            MasterDb = masterDb;
            _parameters = masterDb.EntityMNumericalFunctionParameterGroup.ToLookup(p => p.NumericalFunctionParameterGroupId);
            _constants = masterDb.EntityMPowerCalculationConstantValue.ToDictionary(c => c.PowerCalculationConstantValueType, c => c.ConstantValue);
            CostumeThreshold = ConfigInt("COSTUME_STATUS_FUNCTION_THRESHOLD_LEVEL");
            CostumeOverCoefficient = ConfigInt("COSTUME_STATUS_FUNCTION_THRESHOLD_OVER_COEFFICIENT");
            WeaponThreshold = ConfigInt("WEAPON_STATUS_FUNCTION_THRESHOLD_LEVEL");
            WeaponOverCoefficient = ConfigInt("WEAPON_STATUS_FUNCTION_THRESHOLD_OVER_COEFFICIENT");
        }

        public DarkMasterMemoryDatabase MasterDb { get; }

        public int CostumeThreshold { get; }

        public int CostumeOverCoefficient { get; }

        public int WeaponThreshold { get; }

        public int WeaponOverCoefficient { get; }

        private int ConfigInt(string key)
            => int.Parse(MasterDb.EntityMConfig.First(c => c.ConfigKey == key).Value);

        /// <summary>The status at <paramref name="level"/>; past the threshold level only part of the growth counts.</summary>
        public int Growth(int initial, int functionId, int level, int threshold, int overCoefficient)
        {
            int current = initial + Evaluate(functionId, level);
            if (threshold >= level)
            {
                return current;
            }

            int atThreshold = initial + Evaluate(functionId, threshold);
            return atThreshold + (int)((long)(current - atThreshold) * overCoefficient / 1000);
        }

        public int Evaluate(int functionId, int value)
        {
            EntityMNumericalFunction? function = MasterDb.EntityMNumericalFunction.Find(functionId);
            return function == null ? 0 : Evaluate(function, value);
        }

        private int Evaluate(EntityMNumericalFunction function, int value)
        {
            int[] p = [.. _parameters[function.NumericalFunctionParameterGroupId].OrderBy(x => x.ParameterIndex).Select(x => x.ParameterValue)];
            return NumericalFunctionHelper.Evaluate(function.NumericalFunctionType, p, value);
        }

        /// <summary>A parts main status at <paramref name="level"/>: its initial value plus the growth (the main option function steps up to level 14, then adds a last step).</summary>
        public int PartsMainValue(EntityMPartsStatusMain main, int level)
        {
            EntityMNumericalFunction? function = MasterDb.EntityMNumericalFunction.Find(main.StatusNumericalFunctionId);
            if (function == null)
            {
                return main.StatusChangeInitialValue;
            }

            if (function.NumericalFunctionType != NumericalFunctionType.PARTS_MAIN_OPTION)
            {
                return main.StatusChangeInitialValue + Evaluate(function, level);
            }

            int[] p = [.. _parameters[function.NumericalFunctionParameterGroupId].OrderBy(x => x.ParameterIndex).Select(x => x.ParameterValue)];
            int steps = Math.Min(PartsMainOptionMaxStep, Math.Max(level - 1, 0));
            int last = Math.Min(1, Math.Max(level - PartsMainOptionMaxStep - 1, 0));
            return main.StatusChangeInitialValue + (int)((long)p[0] * steps / 1000) + p[1] * last;
        }

        /// <summary>The power of a stat value: it times the master coefficient, per thousand.</summary>
        public int Power(int value, PowerCalculationConstantValueType coefficient)
            => (int)((long)value * _constants.GetValueOrDefault(coefficient) / 1000);

        public SkillPowerData Skill(int skillId, int level)
        {
            EntityMSkill? skill = MasterDb.EntityMSkill.Find(skillId);
            EntityMSkillLevelGroup? group = skill == null
                ? null
                : MasterDb.EntityMSkillLevelGroup.Where(g => g.SkillLevelGroupId == skill.SkillLevelGroupId && g.LevelLowerLimit <= level)
                    .OrderByDescending(g => g.LevelLowerLimit).FirstOrDefault();
            return group == null ? new SkillPowerData(0, PowerCalculationReferenceStatusType.NONE, []) : SkillOfDetail(group.SkillDetailId);
        }

        public SkillPowerData SkillOfDetail(int skillDetailId)
        {
            EntityMSkillDetail? detail = MasterDb.EntityMSkillDetail.Find(skillDetailId);
            if (detail == null)
            {
                return new SkillPowerData(0, PowerCalculationReferenceStatusType.NONE, []);
            }

            return new SkillPowerData(
                detail.SkillPowerBaseValue,
                detail.PowerCalculationReferenceStatusType,
                [.. MasterDb.EntityMPowerReferenceStatusGroup
                    .Where(r => r.PowerReferenceStatusGroupId == detail.PowerReferenceStatusGroupId)
                    .Select(r => new PowerReference(r.ReferenceStatusType, r.AttributeConditionType, r.CoefficientValuePermil))]);
        }
    }
}
