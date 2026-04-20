using Grpc.Core;
using MariesWonderland.Data;
using MariesWonderland.Extensions;
using MariesWonderland.Models.Entities;
using MariesWonderland.Proto.Character;

namespace MariesWonderland.Services;

public class CharacterService(DarkMasterMemoryDatabase masterDb, UserDataStore store, GameConfig gameConfig)
    : MariesWonderland.Proto.Character.CharacterService.CharacterServiceBase
{
    private readonly DarkMasterMemoryDatabase _masterDb = masterDb;
    private readonly UserDataStore _store = store;
    private readonly GameConfig _gameConfig = gameConfig;

    /// <summary>Performs character rebirth: deducts gold and materials per step, advancing the rebirth count.</summary>
    public override Task<RebirthResponse> Rebirth(RebirthRequest request, ServerCallContext context)
    {
        long userId = context.GetUserId();
        DarkUserMemoryDatabase userDb = _store.GetOrCreate(userId);

        // Look up the rebirth step group for this character
        EntityMCharacterRebirth? rebirthMaster = null;
        foreach (EntityMCharacterRebirth r in _masterDb.EntityMCharacterRebirth)
        {
            if (r.CharacterId == request.CharacterId)
            {
                rebirthMaster = r;
                break;
            }
        }

        if (rebirthMaster == null)
        {
            return Task.FromResult(new RebirthResponse());
        }

        int stepGroupId = rebirthMaster.CharacterRebirthStepGroupId;

        // Find or create user rebirth record
        EntityIUserCharacterRebirth? userRebirth = null;
        foreach (EntityIUserCharacterRebirth ur in userDb.EntityIUserCharacterRebirth)
        {
            if (ur.CharacterId == request.CharacterId)
            {
                userRebirth = ur;
                break;
            }
        }

        if (userRebirth == null)
        {
            userRebirth = new EntityIUserCharacterRebirth
            {
                UserId = userId,
                CharacterId = request.CharacterId,
                RebirthCount = 0
            };
            userDb.EntityIUserCharacterRebirth.Add(userRebirth);
        }

        int currentCount = userRebirth.RebirthCount;
        int targetCount = currentCount + request.RebirthCount;

        int completedCount = currentCount;
        for (int count = currentCount; count < targetCount; count++)
        {
            // Find the step row for this group and count
            EntityMCharacterRebirthStepGroup? step = null;
            foreach (EntityMCharacterRebirthStepGroup s in _masterDb.EntityMCharacterRebirthStepGroup)
            {
                if (s.CharacterRebirthStepGroupId == stepGroupId && s.BeforeRebirthCount == count)
                {
                    step = s;
                    break;
                }
            }

            if (step == null)
            {
                break;
            }

            // Deduct gold
            EntityIUserConsumableItem? gold = null;
            foreach (EntityIUserConsumableItem ci in userDb.EntityIUserConsumableItem)
            {
                if (ci.ConsumableItemId == _gameConfig.ConsumableItemIdForGold)
                {
                    gold = ci;
                    break;
                }
            }

            if (gold != null)
            {
                gold.Count = Math.Max(gold.Count - _gameConfig.CharacterRebirthConsumeGold, 0);
            }

            // Deduct materials
            foreach (EntityMCharacterRebirthMaterialGroup mat in _masterDb.EntityMCharacterRebirthMaterialGroup)
            {
                if (mat.CharacterRebirthMaterialGroupId != step.CharacterRebirthMaterialGroupId)
                {
                    continue;
                }

                EntityIUserMaterial? userMat = null;
                foreach (EntityIUserMaterial m in userDb.EntityIUserMaterial)
                {
                    if (m.MaterialId == mat.MaterialId)
                    {
                        userMat = m;
                        break;
                    }
                }

                if (userMat != null)
                {
                    userMat.Count -= mat.Count;
                    if (userMat.Count <= 0)
                    {
                        userDb.EntityIUserMaterial.Remove(userMat);
                    }
                }
            }

            completedCount = count + 1;
        }

        userRebirth.RebirthCount = completedCount;

        return Task.FromResult(new RebirthResponse());
    }
}
