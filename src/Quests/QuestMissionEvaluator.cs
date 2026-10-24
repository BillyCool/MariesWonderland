using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Quests;

/// <summary>What the quest's battles added up to: deaths, skill uses, damage, crits, combos and the lowest end HP.</summary>
public sealed record QuestRunBattle(
    int CharactersNotAlive,
    long MaxDamage,
    int CostumeSkillUses,
    int WeaponSkillUses,
    int CompanionSkillUses,
    int CriticalCount,
    int ComboCount,
    long ComboMaxDamage,
    long RecoverPoint,
    double MinHpPercent);

/// <summary>The deck the quest was started with: its characters, main weapon attributes and costume weapon types.</summary>
public sealed record QuestRunDeck(
    IReadOnlyList<int> CharacterIds,
    IReadOnlyList<AttributeType> MainWeaponAttributes,
    IReadOnlyList<WeaponType> SkillfulWeaponTypes)
{
    public int CostumeCount => CharacterIds.Count;
}

/// <summary>One run of a quest as its missions see it. A part is null when the data for it is missing.</summary>
public sealed record QuestRun(QuestRunBattle? Battle, QuestRunDeck? Deck);

/// <summary>
/// Decides whether a quest mission's condition was met by a run, from the battle reports and the deck. A condition
/// this doesn't know is never met, so its reward isn't paid.
/// </summary>
public sealed class QuestMissionEvaluator(DarkMasterMemoryDatabase masterDb)
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;

    /// <summary>Builds the run from the quest's battle reports (one per battle) and the deck of the quest's session.</summary>
    public QuestRun BuildRun(DarkUserMemoryDatabase userDb, int questId, IReadOnlyList<EntitySBattleDetail> battles)
        => new(SummarizeBattles(battles), BuildDeck(userDb, questId));

    /// <summary>
    /// Whether the mission's condition was met. Skill uses, crits, recovery and deaths add up over the battles,
    /// damage and combos are the best of them, and HP is the lowest any character ended with.
    /// </summary>
    public bool IsMet(EntityMQuestMission mission, QuestRun run)
    {
        int value = mission.ConditionValue;
        QuestRunBattle? battle = run.Battle;
        QuestRunDeck? deck = run.Deck;

        return mission.QuestMissionConditionType switch
        {
            QuestMissionConditionType.LESS_THAN_OR_EQUAL_X_PEOPLE_NOT_ALIVE => battle != null && battle.CharactersNotAlive <= value,
            QuestMissionConditionType.MAX_DAMAGE => battle != null && battle.MaxDamage >= value,
            QuestMissionConditionType.GREATER_THAN_OR_EQUAL_X_COSTUME_SKILL_USE_COUNT => battle != null && battle.CostumeSkillUses >= value,
            QuestMissionConditionType.GREATER_THAN_OR_EQUAL_X_WEAPON_SKILL_USE_COUNT => battle != null && battle.WeaponSkillUses >= value,
            QuestMissionConditionType.GREATER_THAN_OR_EQUAL_X_COMPANION_SKILL_USE_COUNT => battle != null && battle.CompanionSkillUses >= value,
            QuestMissionConditionType.CRITICAL_COUNT_GE => battle != null && battle.CriticalCount >= value,
            QuestMissionConditionType.MIN_HP_PERCENTAGE_GE => battle != null && battle.MinHpPercent >= value,
            QuestMissionConditionType.COMBO_COUNT_GE => battle != null && battle.ComboCount >= value,
            QuestMissionConditionType.COMBO_MAX_DAMAGE_GE => battle != null && battle.ComboMaxDamage >= value,
            QuestMissionConditionType.LESS_THAN_OR_EQUAL_X_COSTUME_SKILL_USE_COUNT => battle != null && battle.CostumeSkillUses <= value,
            QuestMissionConditionType.LESS_THAN_OR_EQUAL_X_WEAPON_SKILL_USE_COUNT => battle != null && battle.WeaponSkillUses <= value,
            QuestMissionConditionType.LESS_THAN_OR_EQUAL_X_COMPANION_SKILL_USE_COUNT => battle != null && battle.CompanionSkillUses <= value,
            QuestMissionConditionType.WITHOUT_RECOVERY_SKILL => battle != null && battle.RecoverPoint == 0,
            QuestMissionConditionType.WITHOUT_COSTUME_SKILL => battle != null && battle.CostumeSkillUses == 0,
            QuestMissionConditionType.WITHOUT_WEAPON_SKILL => battle != null && battle.WeaponSkillUses == 0,
            QuestMissionConditionType.WITHOUT_COMPANION_SKILL => battle != null && battle.CompanionSkillUses == 0,
            QuestMissionConditionType.SPECIFIED_CHARACTER_IS_IN_DECK => deck != null && deck.CharacterIds.Contains(value),
            QuestMissionConditionType.CHARACTER_CONTAIN_ALL => deck != null && ContainsAll(deck.CharacterIds, GroupValues(mission)),
            QuestMissionConditionType.CHARACTER_CONTAIN_ANY => deck != null && GroupValues(mission).Any(deck.CharacterIds.Contains),
            QuestMissionConditionType.SPECIFIED_ATTRIBUTE_MAIN_WEAPON_IS_IN_DECK => deck != null && deck.MainWeaponAttributes.Contains((AttributeType)value),
            QuestMissionConditionType.SPECIFIED_ATTRIBUTE_MAIN_WEAPON_ALL_CHARACTER => deck != null && deck.MainWeaponAttributes.Count > 0 && deck.MainWeaponAttributes.All(a => a == (AttributeType)value),
            QuestMissionConditionType.COSTUME_SKILLFUL_WEAPON_ANY_CHARACTER => deck != null && deck.SkillfulWeaponTypes.Contains((WeaponType)value),
            QuestMissionConditionType.COSTUME_SKILLFUL_WEAPON_CONTAIN_ALL => deck != null && ContainsAll([.. deck.SkillfulWeaponTypes.Select(t => (int)t)], GroupValues(mission)),
            QuestMissionConditionType.DECK_COSTUME_NUM_EQ => deck != null && deck.CostumeCount == value,
            QuestMissionConditionType.DECK_COSTUME_NUM_GE => deck != null && deck.CostumeCount >= value,
            QuestMissionConditionType.DECK_COSTUME_NUM_LE => deck != null && deck.CostumeCount <= value,
            _ => false
        };
    }

    private static bool ContainsAll(IReadOnlyList<int> have, List<int> wanted)
        => wanted.Count > 0 && wanted.All(have.Contains);

    private List<int> GroupValues(EntityMQuestMission mission)
        => [.. _masterDb.EntityMQuestMissionConditionValueGroup
            .Where(g => g.QuestMissionConditionValueGroupId == mission.QuestMissionConditionValueGroupId)
            .Select(g => g.ConditionValue)];

    private static QuestRunBattle? SummarizeBattles(IReadOnlyList<EntitySBattleDetail> battles)
    {
        if (battles.Count == 0)
        {
            return null;
        }

        List<Proto.Battle.CostumeBattleInfo> characters = [.. battles.SelectMany(b => b.CostumeBattleInfoList)];
        double minHpPercent = characters.Count == 0
            ? 0
            : characters.Min(c => c.MaxHp > 0 ? c.RemainingHp * 100.0 / c.MaxHp : 0);

        return new QuestRunBattle(
            battles.Sum(b => b.CharacterDeathCount),
            battles.Max(b => (long)b.MaxDamage),
            battles.Sum(b => b.PlayerCostumeActiveSkillUsedCount),
            battles.Sum(b => b.PlayerWeaponActiveSkillUsedCount),
            battles.Sum(b => b.PlayerCompanionSkillUsedCount),
            battles.Sum(b => b.CriticalCount),
            battles.Max(b => b.ComboCount),
            battles.Max(b => (long)b.ComboMaxDamage),
            battles.Sum(b => b.TotalRecoverPoint),
            minHpPercent);
    }

    private QuestRunDeck? BuildDeck(DarkUserMemoryDatabase userDb, int questId)
    {
        int deckNumber = userDb.EntitySQuestSession.FirstOrDefault(s => s.QuestId == questId)?.UserDeckNumber ?? 0;
        EntityIUserDeck? deck = userDb.EntityIUserDeck.FirstOrDefault(d => d.DeckType == DeckType.QUEST && d.UserDeckNumber == deckNumber);
        if (deck == null)
        {
            return null;
        }

        List<int> characterIds = [];
        List<AttributeType> attributes = [];
        List<WeaponType> weaponTypes = [];
        foreach (string? deckCharacterUuid in new[] { deck.UserDeckCharacterUuid01, deck.UserDeckCharacterUuid02, deck.UserDeckCharacterUuid03 })
        {
            EntityIUserDeckCharacter? deckCharacter = userDb.EntityIUserDeckCharacter.FirstOrDefault(c => c.UserDeckCharacterUuid == deckCharacterUuid);
            int costumeId = userDb.EntityIUserCostume.FirstOrDefault(c => c.UserCostumeUuid == deckCharacter?.UserCostumeUuid)?.CostumeId ?? 0;
            EntityMCostume? costume = _masterDb.EntityMCostume.FirstOrDefault(c => c.CostumeId == costumeId);
            if (costume == null)
            {
                continue;
            }

            characterIds.Add(costume.CharacterId);
            weaponTypes.Add(costume.SkillfulWeaponType);

            int weaponId = userDb.EntityIUserWeapon.FirstOrDefault(w => w.UserWeaponUuid == deckCharacter?.MainUserWeaponUuid)?.WeaponId ?? 0;
            EntityMWeapon? weapon = _masterDb.EntityMWeapon.FirstOrDefault(w => w.WeaponId == weaponId);
            if (weapon != null)
            {
                attributes.Add(weapon.AttributeType);
            }
        }

        return new QuestRunDeck(characterIds, attributes, weaponTypes);
    }
}
