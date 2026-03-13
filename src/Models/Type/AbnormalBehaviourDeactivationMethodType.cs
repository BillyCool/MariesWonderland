namespace MariesWonderland.Models.Type;

public enum AbnormalBehaviourDeactivationMethodType
{
    UNKNOWN = 0,
    AFTER_SKILL_EXECUTE = 1,
    AFTER_SKILL_EXECUTE_SELF = 2,
    BEFORE_SKILL_EXECUTE = 3,
    BEFORE_SKILL_EXECUTE_SELF = 4,
    END_TURN = 5,
    END_TURN_SELF = 6,
    START_TURN = 7,
    START_TURN_SELF = 8,
    NOTHING = 9,
    ON_DETACH = 10
}
