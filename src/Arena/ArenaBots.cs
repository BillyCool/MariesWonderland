using MariesWonderland.Data;
using MariesWonderland.Helpers;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.Pvp;

namespace MariesWonderland.Arena;

/// <summary>One bot character: a costume, its main weapon, two sub-weapons and a companion (no memoirs, debris or boards).</summary>
public sealed record ArenaBotCharacter(
    int CostumeId, int CostumeLimitBreak, int CostumeLevel, int ActiveSkillLevel,
    int MainWeaponId, IReadOnlyList<int> SubWeaponIds, int WeaponLimitBreak, int WeaponLevel, int WeaponSkillLevel, int WeaponAbilityLevel,
    int CompanionId, int CompanionLevel, int Power);

/// <summary>A bot's deck and the force shown for it.</summary>
public sealed record ArenaBotDeck(IReadOnlyList<ArenaBotCharacter> Characters, int Power, int UserLevel);

/// <summary>
/// The bots that fill an Arena season, like the real game's brackets: one every 20 points from 50,000 down to 18,000,
/// with random names and decks that get stronger up the ladder (the top one near 77k force). They only defend, and
/// lose and gain points like players. Nothing about a bot's deck is stored: it's rebuilt from its seed and ladder
/// position, so it's the same every time.
/// The force shown is the stat part of the client's formula (costume, weapon and companion stats times the master
/// coefficients, <c>CalculatorPower</c>); it leaves skills out, so a bot is a little stronger than its number.
/// </summary>
public static class ArenaBots
{
    public const int TopPoints = 50_000;
    public const int BottomPoints = 18_000;
    public const int PointStep = 20;
    public const int Count = (TopPoints - BottomPoints) / PointStep + 1;

    public const int BottomForce = 15_000;
    public const int TopForce = 77_000;

    /// <summary>Bot player ids (and user ids): above every real player id, below every real user id.</summary>
    public const long IdBase = 3_000_000_000_000L;

    private const int CharacterCount = 3;
    private const int SubWeaponCount = 2;
    private const int MaxLimitBreak = 4;
    private const int CompanionMaxLevel = 50;

    /// <summary>Every costume skill in the real save tops out here, whatever the rarity or limit breaks.</summary>
    private const int CostumeSkillMaxLevel = 15;
    private const int WeaponCategoryWeapon = 1;

    private static readonly string[] NameStarts =
    [
        "Pale", "Iron", "Silent", "Ashen", "Hollow", "Crimson", "Lunar", "Quiet", "Wander", "Gloom", "Ivory", "Cinder",
        "Dusk", "Frost", "Amber", "Bramble", "Velvet", "Rusted", "Gilded", "Mist", "Thorn", "Echo", "Ember", "Sable",
    ];

    private static readonly string[] NameEnds =
    [
        "Lark", "Wren", "Moth", "Fox", "Lily", "Bell", "Rook", "Doll", "Cage", "Lantern", "Petal", "Shade", "Hare",
        "Vane", "Star", "Thread", "Mask", "Feather", "Key", "Tide", "Bloom", "Raven", "Spire", "Wisp",
    ];

    public static bool IsBot(long playerId) => playerId > IdBase && playerId <= IdBase + Count;

    /// <summary>Starting points of the bot at ladder position <paramref name="index"/> (0 = top).</summary>
    public static int PointsAt(int index) => TopPoints - index * PointStep;

    /// <summary>The season's bots, top first.</summary>
    public static List<ArenaEntry> Create(DarkMasterMemoryDatabase masterDb, ArenaSeason season)
    {
        StatTables stats = new(masterDb);
        List<ArenaEntry> bots = new(Count);
        for (int index = 0; index < Count; index++)
        {
            int seed = Seed(season.SeasonId, index);
            ArenaBotDeck deck = Deck(stats, seed, index);
            ArenaBotCharacter strongest = deck.Characters.OrderByDescending(c => c.Power).First();
            long id = IdBase + index + 1;
            bots.Add(new ArenaEntry
            {
                UserId = id,
                PlayerId = id,
                Points = PointsAt(index),
                PointsChangedAt = season.StartsAt + index,
                BotName = Name(seed),
                BotSeed = seed,
                BotFavoriteCostumeId = deck.Characters[0].CostumeId,
                DefenseDeckPower = deck.Power,
                DefenseCostumeIds = [.. deck.Characters.Select(c => c.CostumeId)],
                DefenseAttributeTypes = [.. deck.Characters.Select(c => (int)(masterDb.EntityMWeapon.Find(c.MainWeaponId)?.AttributeType ?? 0))],
                MostPowerfulCostumeId = strongest.CostumeId,
            });
        }
        return bots;
    }

    /// <summary>The deck of a bot (from its stored seed and id).</summary>
    public static ArenaBotDeck Deck(DarkMasterMemoryDatabase masterDb, long botPlayerId, int seed)
        => Deck(new StatTables(masterDb), seed, (int)(botPlayerId - IdBase - 1));

    /// <summary>The bot's deck as the client fields it.</summary>
    public static List<DeckCharacter> Build(DarkMasterMemoryDatabase masterDb, ArenaBotDeck deck)
    {
        List<DeckCharacter> characters = [];
        foreach (ArenaBotCharacter bot in deck.Characters)
        {
            DeckCharacter character = new()
            {
                Costume = new CostumeInfo
                {
                    CostumeId = bot.CostumeId,
                    LimitBreakCount = bot.CostumeLimitBreak,
                    Level = bot.CostumeLevel,
                    ActiveSkillLevel = bot.ActiveSkillLevel,
                    CharacterLevel = bot.CostumeLevel,
                },
                Companion = new CompanionInfo { CompanionId = bot.CompanionId, Level = bot.CompanionLevel },
                MainWeapon = Weapon(masterDb, bot.MainWeaponId, bot),
                Thought = new ThoughtInfo(), // the client reads Thought.ThoughtId without a null check (see ArenaDecks)
            };
            character.SubWeapon.AddRange(bot.SubWeaponIds.Select(id => Weapon(masterDb, id, bot)));
            characters.Add(character);
        }
        return characters;
    }

    private static WeaponInfo Weapon(DarkMasterMemoryDatabase masterDb, int weaponId, ArenaBotCharacter bot)
    {
        // The client reads WeaponAwakenAbility without a null check, so it is always present (see ArenaDecks.Weapon).
        WeaponInfo info = new()
        {
            WeaponId = weaponId, LimitBreakCount = bot.WeaponLimitBreak, Level = bot.WeaponLevel, WeaponAwakenAbility = new AwakenAbilityInfo(),
        };
        EntityMWeapon? weapon = masterDb.EntityMWeapon.Find(weaponId);
        if (weapon == null)
        {
            return info;
        }

        info.WeaponSkill.AddRange(masterDb.EntityMWeaponSkillGroup
            .Where(g => g.WeaponSkillGroupId == weapon.WeaponSkillGroupId)
            .OrderBy(g => g.SlotNumber)
            .Select(g => new WeaponSkillInfo { SkillId = g.SkillId, Level = bot.WeaponSkillLevel }));
        info.WeaponAbility.AddRange(masterDb.EntityMWeaponAbilityGroup
            .Where(g => g.WeaponAbilityGroupId == weapon.WeaponAbilityGroupId)
            .OrderBy(g => g.SlotNumber)
            .Select(g => new WeaponAbilityInfo { AbilityId = g.AbilityId, Level = bot.WeaponAbilityLevel }));
        return info;
    }

    /// <summary>A name like "AshenLark" or "AshenLark7" (the client doesn't allow spaces in names).</summary>
    public static string Name(int seed)
    {
        Random random = new(seed);
        string name = $"{NameStarts[random.Next(NameStarts.Length)]}{NameEnds[random.Next(NameEnds.Length)]}";
        int number = random.Next(4);
        return number == 0 ? name : $"{name}{random.Next(2, 100)}";
    }

    private static int Seed(int seasonId, int index) => unchecked(seasonId * 10_007 + index * 7_919 + 17);

    /// <summary>
    /// Picks three characters and their gear, then raises one shared progress (levels, limit breaks, skill levels as a
    /// share of each item's maximum) until the force reaches the bot's target, or everything is maxed.
    /// </summary>
    private static ArenaBotDeck Deck(StatTables stats, int seed, int index)
    {
        double strength = (double)(PointsAt(index) - BottomPoints) / (TopPoints - BottomPoints);
        int target = (int)(BottomForce + strength * (TopForce - BottomForce));
        Random random = new(seed);

        RarityType costumeRarity = strength < 0.2 ? RarityType.S_RARE : RarityType.SS_RARE;
        List<(EntityMCostume Costume, int MainWeaponId, int[] SubWeaponIds, int CompanionId)> picks = [];
        HashSet<int> characters = [];
        List<EntityMCostume> costumes = stats.Costumes(costumeRarity);
        for (int attempt = 0; picks.Count < CharacterCount && attempt < 200; attempt++)
        {
            EntityMCostume costume = costumes[random.Next(costumes.Count)];
            List<EntityMWeapon> mains = stats.Weapons(costume.SkillfulWeaponType, costumeRarity);
            if (mains.Count == 0 || !characters.Add(costume.CharacterId))
            {
                continue;
            }

            EntityMWeapon main = mains[random.Next(mains.Count)];
            List<EntityMWeapon> anyWeapon = stats.Weapons(null, costumeRarity);
            int[] subs = [.. anyWeapon.Where(w => w.WeaponId != main.WeaponId).OrderBy(_ => random.Next()).Take(SubWeaponCount).Select(w => w.WeaponId)];
            picks.Add((costume, main.WeaponId, subs, stats.CompanionIds[random.Next(stats.CompanionIds.Count)]));
        }

        double low = 0;
        double high = 1;
        ArenaBotDeck best = Equip(stats, picks, 1, strength);
        if (best.Power > target)
        {
            for (int step = 0; step < 14; step++)
            {
                double middle = (low + high) / 2;
                ArenaBotDeck deck = Equip(stats, picks, middle, strength);
                if (deck.Power >= target)
                {
                    best = deck;
                    high = middle;
                }
                else
                {
                    low = middle;
                }
            }
        }
        return best;
    }

    private static ArenaBotDeck Equip(StatTables stats, List<(EntityMCostume Costume, int MainWeaponId, int[] SubWeaponIds, int CompanionId)> picks, double progress, double strength)
    {
        List<ArenaBotCharacter> characters = [];
        foreach ((EntityMCostume costume, int mainWeaponId, int[] subWeaponIds, int companionId) in picks)
        {
            (int costumeLevel, int costumeLimitBreak) = stats.CostumeLevel(costume, progress);
            int activeSkillLevel = Share(progress, CostumeSkillMaxLevel);

            EntityMWeapon main = stats.Weapon(mainWeaponId);
            (int weaponLevel, int weaponLimitBreak) = stats.WeaponLevel(main, progress);
            int skillLevel = Share(progress, stats.WeaponSkillMaxLevel(main, weaponLimitBreak));
            int abilityLevel = Share(progress, stats.WeaponAbilityMaxLevel(main, weaponLimitBreak));
            int companionLevel = Share(progress, CompanionMaxLevel);

            int power = stats.CostumePower(costume, costumeLevel)
                + stats.WeaponPower(main, weaponLevel)
                + subWeaponIds.Sum(id => stats.WeaponPower(stats.Weapon(id), weaponLevel))
                + stats.CompanionPower(companionId, companionLevel);
            characters.Add(new ArenaBotCharacter(
                costume.CostumeId, costumeLimitBreak, costumeLevel, activeSkillLevel,
                mainWeaponId, subWeaponIds, weaponLimitBreak, weaponLevel, skillLevel, abilityLevel,
                companionId, companionLevel, power));
        }
        return new ArenaBotDeck(characters, characters.Sum(c => c.Power), 10 + (int)(strength * 140));
    }

    /// <summary>The level at <paramref name="progress"/> of the way to <paramref name="max"/>, at least 1.</summary>
    private static int Share(double progress, int max) => Math.Max(1, (int)Math.Round(progress * Math.Max(max, 1)));

    /// <summary>The master data a bot needs, indexed once per batch (generating a season's bots evaluates many formulas).</summary>
    private sealed class StatTables
    {
        private readonly DarkMasterMemoryDatabase _masterDb;
        private readonly Dictionary<int, EntityMNumericalFunction> _functions;
        private readonly ILookup<int, EntityMNumericalFunctionParameterGroup> _parameters;
        private readonly Dictionary<PowerCalculationConstantValueType, int> _coefficients;
        private readonly Dictionary<(WeaponType?, RarityType), List<EntityMWeapon>> _weapons = [];
        private readonly Dictionary<RarityType, List<EntityMCostume>> _costumes = [];

        public StatTables(DarkMasterMemoryDatabase masterDb)
        {
            _masterDb = masterDb;
            _functions = masterDb.EntityMNumericalFunction.ToDictionary(f => f.NumericalFunctionId);
            _parameters = masterDb.EntityMNumericalFunctionParameterGroup.ToLookup(p => p.NumericalFunctionParameterGroupId);
            _coefficients = masterDb.EntityMPowerCalculationConstantValue.ToDictionary(c => c.PowerCalculationConstantValueType, c => c.ConstantValue);
            HashSet<int> npcCompanions = [.. masterDb.EntityMBattleNpcCompanion.Select(c => c.CompanionId)];
            CompanionIds = [.. masterDb.EntityMCompanion.Select(c => c.CompanionId).Where(id => !npcCompanions.Contains(id))];
        }

        /// <summary>Companions a player can own (the ones battle NPCs field are separate rows with ids from 8000001).</summary>
        public List<int> CompanionIds { get; }

        /// <summary>Player costumes of a rarity (enemy-only costumes are all the lowest rarity and have no active skill).</summary>
        public List<EntityMCostume> Costumes(RarityType rarity)
        {
            if (!_costumes.TryGetValue(rarity, out List<EntityMCostume>? list))
            {
                HashSet<int> playable = [.. _masterDb.EntityMCharacter.Select(c => c.CharacterId)];
                list = [.. _masterDb.EntityMCostume
                    .Where(c => c.RarityType == rarity && c.CostumeActiveSkillGroupId != 0 && playable.Contains(c.CharacterId)
                        && Weapons(c.SkillfulWeaponType, rarity).Count > 0)
                    .OrderBy(c => c.CostumeId)];
                _costumes[rarity] = list;
            }
            return list;
        }

        /// <summary>Weapons (not enhancement material) of a type, or of any type, at the costume's rarity or one above.</summary>
        public List<EntityMWeapon> Weapons(WeaponType? type, RarityType costumeRarity)
        {
            if (!_weapons.TryGetValue((type, costumeRarity), out List<EntityMWeapon>? list))
            {
                list = [.. _masterDb.EntityMWeapon
                    .Where(w => w.WeaponCategoryType == WeaponCategoryWeapon
                        && (type == null || w.WeaponType == type)
                        && (w.RarityType == costumeRarity || (int)w.RarityType == (int)costumeRarity + 10))
                    .OrderBy(w => w.WeaponId)];
                _weapons[(type, costumeRarity)] = list;
            }
            return list;
        }

        public EntityMWeapon Weapon(int weaponId) => _masterDb.EntityMWeapon.Find(weaponId)!;

        public (int Level, int LimitBreak) CostumeLevel(EntityMCostume costume, double progress)
        {
            int function = _masterDb.EntityMCostumeRarity.Find(costume.RarityType)?.MaxLevelNumericalFunctionId ?? 0;
            return LevelAndLimitBreak(function, progress);
        }

        public (int Level, int LimitBreak) WeaponLevel(EntityMWeapon weapon, double progress)
            => LevelAndLimitBreak(WeaponHelper.ResolveEnhance(_masterDb, weapon)?.MaxLevelNumericalFunctionId ?? 0, progress);

        public int WeaponSkillMaxLevel(EntityMWeapon weapon, int limitBreak)
            => Evaluate(WeaponHelper.ResolveEnhance(_masterDb, weapon)?.MaxSkillLevelNumericalFunctionId ?? 0, limitBreak);

        public int WeaponAbilityMaxLevel(EntityMWeapon weapon, int limitBreak)
            => Evaluate(WeaponHelper.ResolveEnhance(_masterDb, weapon)?.MaxAbilityLevelNumericalFunctionId ?? 0, limitBreak);

        public int CostumePower(EntityMCostume costume, int level)
        {
            EntityMCostumeBaseStatus? status = _masterDb.EntityMCostumeBaseStatus.Find(costume.CostumeBaseStatusId);
            EntityMCostumeStatusCalculation? calc = _masterDb.EntityMCostumeStatusCalculation.Find(costume.CostumeStatusCalculationId);
            if (status == null || calc == null)
            {
                return 0;
            }

            return Power(status.Hp + Evaluate(calc.HpNumericalFunctionId, level), PowerCalculationConstantValueType.COSTUME_HP_COEFFICIENT)
                + Power(status.Attack + Evaluate(calc.AttackNumericalFunctionId, level), PowerCalculationConstantValueType.COSTUME_ATTACK_COEFFICIENT)
                + Power(status.Vitality + Evaluate(calc.VitalityNumericalFunctionId, level), PowerCalculationConstantValueType.COSTUME_VITALITY_COEFFICIENT);
        }

        public int WeaponPower(EntityMWeapon weapon, int level)
        {
            EntityMWeaponBaseStatus? status = _masterDb.EntityMWeaponBaseStatus.Find(weapon.WeaponBaseStatusId);
            EntityMWeaponStatusCalculation? calc = _masterDb.EntityMWeaponStatusCalculation.Find(weapon.WeaponStatusCalculationId);
            if (status == null || calc == null)
            {
                return 0;
            }

            return Power(status.Hp + Evaluate(calc.HpNumericalFunctionId, level), PowerCalculationConstantValueType.WEAPON_HP_COEFFICIENT)
                + Power(status.Attack + Evaluate(calc.AttackNumericalFunctionId, level), PowerCalculationConstantValueType.WEAPON_ATTACK_COEFFICIENT)
                + Power(status.Vitality + Evaluate(calc.VitalityNumericalFunctionId, level), PowerCalculationConstantValueType.WEAPON_VITALITY_COEFFICIENT);
        }

        public int CompanionPower(int companionId, int level)
        {
            EntityMCompanion? companion = _masterDb.EntityMCompanion.Find(companionId);
            EntityMCompanionBaseStatus? status = companion == null ? null : _masterDb.EntityMCompanionBaseStatus.Find(companion.CompanionBaseStatusId);
            EntityMCompanionStatusCalculation? calc = companion == null ? null : _masterDb.EntityMCompanionStatusCalculation.Find(companion.CompanionStatusCalculationId);
            if (status == null || calc == null)
            {
                return 0;
            }

            return Power(status.Hp + Evaluate(calc.HpNumericalFunctionId, level), PowerCalculationConstantValueType.COMPANION_HP_COEFFICIENT)
                + Power(status.Attack + Evaluate(calc.AttackNumericalFunctionId, level), PowerCalculationConstantValueType.COMPANION_ATTACK_COEFFICIENT)
                + Power(status.Vitality + Evaluate(calc.VitalityNumericalFunctionId, level), PowerCalculationConstantValueType.COMPANION_VITALITY_COEFFICIENT);
        }

        /// <summary>The level at <paramref name="progress"/> of the fully limit-broken maximum, and the fewest limit breaks that allow it.</summary>
        private (int Level, int LimitBreak) LevelAndLimitBreak(int maxLevelFunctionId, double progress)
        {
            int level = Share(progress, Evaluate(maxLevelFunctionId, MaxLimitBreak));
            int limitBreak = 0;
            while (limitBreak < MaxLimitBreak && Evaluate(maxLevelFunctionId, limitBreak) < level)
            {
                limitBreak++;
            }
            return (level, limitBreak);
        }

        /// <summary><c>PowerServal.calcStatusPower</c>: the stat times a per-mille coefficient.</summary>
        private int Power(int stat, PowerCalculationConstantValueType coefficient)
            => (int)((long)stat * _coefficients.GetValueOrDefault(coefficient) / 1000);

        /// <summary>Same as <see cref="NumericalFunctionHelper.Evaluate"/>, from the indexed tables.</summary>
        private int Evaluate(int functionId, int value)
        {
            if (!_functions.TryGetValue(functionId, out EntityMNumericalFunction? function))
            {
                return 0;
            }

            int[] p = [.. _parameters[function.NumericalFunctionParameterGroupId].OrderBy(x => x.ParameterIndex).Select(x => x.ParameterValue)];
            return NumericalFunctionHelper.Evaluate(function.NumericalFunctionType, p, value);
        }
    }
}
