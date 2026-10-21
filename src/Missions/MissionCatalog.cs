using MariesWonderland.Data;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;

namespace MariesWonderland.Missions;

/// <summary>One mission with everything the engine needs, resolved from master data and the filter table.</summary>
public sealed record MissionDefinition(
    EntityMMission Mission,
    MissionCategoryType Category,
    int SubCategoryId,
    long StartDatetime,
    long EndDatetime,
    MissionUnlockConditionType UnlockType,
    int UnlockValue,
    MissionFilter? Filter,
    bool IsFilterUnmapped,
    IReadOnlyList<EntityMMissionReward> Rewards)
{
    public int MissionId => Mission.MissionId;
    public int MissionGroupId => Mission.MissionGroupId;
    public MissionClearConditionType ConditionType => Mission.MissionClearConditionType;
    public int ClearValue => Mission.ClearConditionValue;

    /// <summary>Daily missions start over at each game day.</summary>
    public bool IsDaily => Category is MissionCategoryType.DAILY or MissionCategoryType.MISSION_PASS_DAILY;

    public bool IsInTerm(long nowMs) => nowMs >= StartDatetime && nowMs <= EndDatetime;
}

/// <summary>
/// Every mission in the master data, resolved once per master-data snapshot. Missions whose term has ended are
/// kept: the term only decides whether a mission is available, so content brought back later just works.
/// </summary>
public sealed class MissionCatalog
{
    private readonly IReadOnlyDictionary<int, MissionDefinition> _byId;
    private readonly ILookup<MissionClearConditionType, MissionDefinition> _byConditionType;
    private readonly ILookup<int, MissionDefinition> _byGroupId;

    public MissionCatalog(DarkMasterMemoryDatabase masterDb, MissionFilterTable filters)
    {
        Dictionary<int, EntityMMissionGroup> groups = masterDb.EntityMMissionGroup.ToDictionary(g => g.MissionGroupId);
        Dictionary<int, EntityMMissionTerm> terms = masterDb.EntityMMissionTerm.ToDictionary(t => t.MissionTermId);
        Dictionary<int, EntityMMissionUnlockCondition> unlocks = masterDb.EntityMMissionUnlockCondition.ToDictionary(u => u.MissionUnlockConditionId);
        ILookup<int, EntityMMissionReward> rewards = masterDb.EntityMMissionReward.ToLookup(r => r.MissionRewardId);

        List<MissionDefinition> definitions = [];
        foreach (EntityMMission mission in masterDb.EntityMMission)
        {
            EntityMMissionGroup? group = groups.GetValueOrDefault(mission.MissionGroupId);
            EntityMMissionTerm? term = terms.GetValueOrDefault(mission.MissionTermId);
            EntityMMissionUnlockCondition? unlock = unlocks.GetValueOrDefault(mission.MissionUnlockConditionId);
            int optionGroupId = mission.MissionClearConditionOptionGroupId;
            MissionFilter? filter = filters.FindForMission(mission.MissionId, optionGroupId);

            definitions.Add(new MissionDefinition(
                mission,
                group?.MissionCategoryType ?? MissionCategoryType.UNKNOWN,
                group?.MissionSubCategoryId ?? 0,
                term?.StartDatetime ?? 0,
                term?.EndDatetime ?? long.MaxValue,
                unlock?.MissionUnlockConditionType ?? MissionUnlockConditionType.UNKNOWN,
                unlock?.ConditionValue ?? 0,
                filter,
                IsFilterUnmapped: optionGroupId != 0 && filter == null,
                [.. rewards[mission.MissionRewardId].Where(r => r.Count > 0)]));
        }

        _byId = definitions.ToDictionary(d => d.MissionId);
        _byConditionType = definitions.ToLookup(d => d.ConditionType);
        _byGroupId = definitions.ToLookup(d => d.MissionGroupId);
    }

    public IEnumerable<MissionDefinition> All => _byId.Values;

    public MissionDefinition? Find(int missionId) => _byId.GetValueOrDefault(missionId);

    public IEnumerable<MissionDefinition> ByConditionType(MissionClearConditionType conditionType) => _byConditionType[conditionType];

    public IEnumerable<MissionDefinition> ByGroup(int missionGroupId) => _byGroupId[missionGroupId];
}
