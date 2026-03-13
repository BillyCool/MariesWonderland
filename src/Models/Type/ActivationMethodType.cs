namespace MariesWonderland.Models.Type;

public enum ActivationMethodType
{
    UNKNOWN = 0,
    IMMEDIATE = 1,
    AFTER_SKILL_EXECUTE = 2,
    BEFORE_CALCULATE_SKILL_BEHAVIOUR = 3,
    AFTER_CALCULATE_SKILL_BEHAVIOUR = 4,
    TO_SKILL_BEHAVIOUR_HIT = 5,
    BATTLE_START = 6,
    PRE_IMMEDIATE = 7
}
