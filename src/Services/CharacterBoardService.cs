using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Models.Type;
using MariesWonderland.Proto.CharacterBoard;

namespace MariesWonderland.Services;

public class CharacterBoardService(DarkMasterMemoryDatabase masterDb, UserDataStore store)
    : MariesWonderland.Proto.CharacterBoard.CharacterboardService.CharacterboardServiceBase
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly UserDataStore _store = store;

    /// <summary>Unlocks character board panels: deducts material costs, sets release bits, and applies stat/ability effects.</summary>
    public override Task<ReleasePanelResponse> ReleasePanel(ReleasePanelRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        foreach (int panelId in request.CharacterBoardPanelId)
        {
            EntityMCharacterBoardPanel? panel = null;
            foreach (EntityMCharacterBoardPanel p in _masterDb.EntityMCharacterBoardPanel)
            {
                if (p.CharacterBoardPanelId == panelId)
                {
                    panel = p;
                    break;
                }
            }

            if (panel == null)
            {
                continue;
            }

            ConsumeCosts(userDb, panel);
            SetReleaseBit(userDb, panel);
            ApplyEffects(userDb, panel);
        }

        return Task.FromResult(new ReleasePanelResponse());
    }

    /// <summary>Deducts the material/gem costs required to release a character board panel.</summary>
    private void ConsumeCosts(DarkUserMemoryDatabase userDb, EntityMCharacterBoardPanel panel)
    {
        foreach (EntityMCharacterBoardPanelReleasePossessionGroup cost in _masterDb.EntityMCharacterBoardPanelReleasePossessionGroup)
        {
            if (cost.CharacterBoardPanelReleasePossessionGroupId != panel.CharacterBoardPanelReleasePossessionGroupId)
            {
                continue;
            }

            DeductPossession(userDb, cost.PossessionType, cost.PossessionId, cost.Count);
        }
    }

    /// <summary>Subtracts a possession (material, consumable, or gem) from the user's inventory.</summary>
    private static void DeductPossession(DarkUserMemoryDatabase userDb, PossessionType possessionType, int possessionId, int count)
    {
        switch (possessionType)
        {
            case PossessionType.MATERIAL:
            {
                EntityIUserMaterial? mat = null;
                foreach (EntityIUserMaterial m in userDb.EntityIUserMaterial)
                {
                    if (m.MaterialId == possessionId)
                    {
                        mat = m;
                        break;
                    }
                }

                if (mat != null)
                {
                    mat.Count -= count;
                    if (mat.Count <= 0)
                    {
                        userDb.EntityIUserMaterial.Remove(mat);
                    }
                }
                break;
            }
            case PossessionType.CONSUMABLE_ITEM:
            {
                EntityIUserConsumableItem? item = null;
                foreach (EntityIUserConsumableItem ci in userDb.EntityIUserConsumableItem)
                {
                    if (ci.ConsumableItemId == possessionId)
                    {
                        item = ci;
                        break;
                    }
                }

                if (item != null)
                {
                    item.Count -= count;
                    if (item.Count <= 0)
                    {
                        userDb.EntityIUserConsumableItem.Remove(item);
                    }
                }
                break;
            }
            case PossessionType.PAID_GEM:
            {
                EntityIUserGem? gem = userDb.EntityIUserGem.Count > 0 ? userDb.EntityIUserGem[0] : null;
                if (gem != null)
                {
                    gem.PaidGem -= count;
                }
                break;
            }
            case PossessionType.FREE_GEM:
            {
                EntityIUserGem? gem = userDb.EntityIUserGem.Count > 0 ? userDb.EntityIUserGem[0] : null;
                if (gem != null)
                {
                    gem.FreeGem -= count;
                }
                break;
            }
        }
    }

    /// <summary>Sets the release bit for a panel on the user's character board, using bitfield-packed storage (32 panels per field).</summary>
    private static void SetReleaseBit(DarkUserMemoryDatabase userDb, EntityMCharacterBoardPanel panel)
    {
        int boardId = panel.CharacterBoardId;

        EntityIUserCharacterBoard? board = null;
        foreach (EntityIUserCharacterBoard b in userDb.EntityIUserCharacterBoard)
        {
            if (b.CharacterBoardId == boardId)
            {
                board = b;
                break;
            }
        }

        if (board == null)
        {
            board = new EntityIUserCharacterBoard
            {
                UserId = userDb.UserId,
                CharacterBoardId = boardId
            };
            userDb.EntityIUserCharacterBoard.Add(board);
        }

        int bitFieldIndex = (panel.SortOrder - 1) / 32;
        int bitPosition = (panel.SortOrder - 1) % 32;
        int mask = 1 << bitPosition;

        switch (bitFieldIndex)
        {
            case 0:
                board.PanelReleaseBit1 |= mask;
                break;
            case 1:
                board.PanelReleaseBit2 |= mask;
                break;
            case 2:
                board.PanelReleaseBit3 |= mask;
                break;
            case 3:
                board.PanelReleaseBit4 |= mask;
                break;
        }
    }

    /// <summary>Applies the panel's release effects (ability unlocks or stat boosts) to the character.</summary>
    private void ApplyEffects(DarkUserMemoryDatabase userDb, EntityMCharacterBoardPanel panel)
    {
        foreach (EntityMCharacterBoardPanelReleaseEffectGroup eff in _masterDb.EntityMCharacterBoardPanelReleaseEffectGroup)
        {
            if (eff.CharacterBoardPanelReleaseEffectGroupId != panel.CharacterBoardPanelReleaseEffectGroupId)
            {
                continue;
            }

            switch (eff.CharacterBoardEffectType)
            {
                case CharacterBoardEffectType.ABILITY:
                    ApplyAbilityEffect(userDb, eff);
                    break;
                case CharacterBoardEffectType.STATUS_UP:
                    ApplyStatusUpEffect(userDb, eff);
                    break;
            }
        }
    }

    /// <summary>Grants or levels up a character ability from a board panel release, capped by the master-defined max level.</summary>
    private void ApplyAbilityEffect(DarkUserMemoryDatabase userDb, EntityMCharacterBoardPanelReleaseEffectGroup eff)
    {
        EntityMCharacterBoardAbility? ability = null;
        foreach (EntityMCharacterBoardAbility a in _masterDb.EntityMCharacterBoardAbility)
        {
            if (a.CharacterBoardAbilityId == eff.CharacterBoardEffectId)
            {
                ability = a;
                break;
            }
        }

        if (ability == null)
        {
            return;
        }

        int characterId = ResolveCharacterId(ability.CharacterBoardEffectTargetGroupId);
        if (characterId == 0)
        {
            return;
        }

        // Find or create ability state
        EntityIUserCharacterBoardAbility? state = null;
        foreach (EntityIUserCharacterBoardAbility a in userDb.EntityIUserCharacterBoardAbility)
        {
            if (a.CharacterId == characterId && a.AbilityId == ability.AbilityId)
            {
                state = a;
                break;
            }
        }

        if (state == null)
        {
            state = new EntityIUserCharacterBoardAbility
            {
                UserId = userDb.UserId,
                CharacterId = characterId,
                AbilityId = ability.AbilityId,
                Level = 0
            };
            userDb.EntityIUserCharacterBoardAbility.Add(state);
        }

        state.Level += eff.EffectValue;

        // Clamp to max level if defined
        foreach (EntityMCharacterBoardAbilityMaxLevel maxLvl in _masterDb.EntityMCharacterBoardAbilityMaxLevel)
        {
            if (maxLvl.CharacterId == characterId && maxLvl.AbilityId == ability.AbilityId)
            {
                if (state.Level > maxLvl.MaxLevel)
                {
                    state.Level = maxLvl.MaxLevel;
                }
                break;
            }
        }
    }

    /// <summary>Applies a stat increase (HP, ATK, AGI, VIT, CRIT) to a character from a board panel release.</summary>
    private void ApplyStatusUpEffect(DarkUserMemoryDatabase userDb, EntityMCharacterBoardPanelReleaseEffectGroup eff)
    {
        EntityMCharacterBoardStatusUp? statusUp = null;
        foreach (EntityMCharacterBoardStatusUp s in _masterDb.EntityMCharacterBoardStatusUp)
        {
            if (s.CharacterBoardStatusUpId == eff.CharacterBoardEffectId)
            {
                statusUp = s;
                break;
            }
        }

        if (statusUp == null)
        {
            return;
        }

        int characterId = ResolveCharacterId(statusUp.CharacterBoardEffectTargetGroupId);
        if (characterId == 0)
        {
            return;
        }

        StatusCalculationType calcType = StatusUpTypeToCalcType(statusUp.CharacterBoardStatusUpType);

        // Find or create status up state
        EntityIUserCharacterBoardStatusUp? state = null;
        foreach (EntityIUserCharacterBoardStatusUp s in userDb.EntityIUserCharacterBoardStatusUp)
        {
            if (s.CharacterId == characterId && s.StatusCalculationType == calcType)
            {
                state = s;
                break;
            }
        }

        if (state == null)
        {
            state = new EntityIUserCharacterBoardStatusUp
            {
                UserId = userDb.UserId,
                CharacterId = characterId,
                StatusCalculationType = calcType
            };
            userDb.EntityIUserCharacterBoardStatusUp.Add(state);
        }

        switch (statusUp.CharacterBoardStatusUpType)
        {
            case CharacterBoardStatusUpType.AGILITY_ADD:
            case CharacterBoardStatusUpType.AGILITY_MULTIPLY:
                state.Agility += eff.EffectValue;
                break;
            case CharacterBoardStatusUpType.ATTACK_ADD:
            case CharacterBoardStatusUpType.ATTACK_MULTIPLY:
                state.Attack += eff.EffectValue;
                break;
            case CharacterBoardStatusUpType.CRITICAL_ATTACK_ADD:
                state.CriticalAttack += eff.EffectValue;
                break;
            case CharacterBoardStatusUpType.CRITICAL_RATIO_ADD:
                state.CriticalRatio += eff.EffectValue;
                break;
            case CharacterBoardStatusUpType.HP_ADD:
            case CharacterBoardStatusUpType.HP_MULTIPLY:
                state.Hp += eff.EffectValue;
                break;
            case CharacterBoardStatusUpType.VITALITY_ADD:
            case CharacterBoardStatusUpType.VITALITY_MULTIPLY:
                state.Vitality += eff.EffectValue;
                break;
        }
    }

    /// <summary>Resolves a CharacterBoardEffectTargetGroupId to its target character ID.</summary>
    private int ResolveCharacterId(int targetGroupId)
    {
        foreach (EntityMCharacterBoardEffectTargetGroup t in _masterDb.EntityMCharacterBoardEffectTargetGroup)
        {
            if (t.CharacterBoardEffectTargetGroupId == targetGroupId && t.TargetValue != 0)
            {
                return t.TargetValue;
            }
        }
        return 0;
    }

    /// <summary>Maps a CharacterBoardStatusUpType to its calculation type (ADD or MULTIPLY).</summary>
    private static StatusCalculationType StatusUpTypeToCalcType(CharacterBoardStatusUpType t)
    {
        return t switch
        {
            CharacterBoardStatusUpType.AGILITY_MULTIPLY or
            CharacterBoardStatusUpType.ATTACK_MULTIPLY or
            CharacterBoardStatusUpType.HP_MULTIPLY or
            CharacterBoardStatusUpType.VITALITY_MULTIPLY => StatusCalculationType.MULTIPLY,
            _ => StatusCalculationType.ADD
        };
    }
}
