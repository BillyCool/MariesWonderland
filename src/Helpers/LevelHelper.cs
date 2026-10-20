using MariesWonderland.Data;
using MariesWonderland.Models.Entities;

namespace MariesWonderland.Helpers;

/// <summary>Level lookups from EXP thresholds in <see cref="EntityMNumericalParameterMap"/>.</summary>
public static class LevelHelper
{
    /// <summary>Numerical parameter map holding the user level EXP thresholds.</summary>
    public const int UserExpMapId = 1;

    /// <summary>Numerical parameter map holding the character level EXP thresholds.</summary>
    public const int CharacterExpMapId = 31;

    /// <summary>
    /// Returns the highest level whose EXP threshold in the given map is at or below <paramref name="exp"/>,
    /// or 1 when none is.
    /// </summary>
    public static int LevelForExp(DarkMasterMemoryDatabase masterDb, int numericalParameterMapId, int exp)
    {
        int level = 1;
        foreach (EntityMNumericalParameterMap row in masterDb.EntityMNumericalParameterMap
            .Where(r => r.NumericalParameterMapId == numericalParameterMapId)
            .OrderBy(r => r.ParameterKey))
        {
            if (exp < row.ParameterValue)
            {
                break;
            }

            level = row.ParameterKey;
        }

        return level;
    }
}
