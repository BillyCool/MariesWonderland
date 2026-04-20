namespace MariesWonderland.Models.Type;

public enum SkillBehaviourActivationConditionType
{
    UNKNOWN = 0,
    ALWAYS = 1,
    IN_SKILL_FLOW = 2,
    IN_SKILL_FLOW_COUNTER = 3,
    IS_SKILL_TARGET = 4,
    IS_SKILL_EXECUTOR = 5,
    HP_RATIO = 6,
    ACTIVATION_UPPER_COUNT = 7,
    WAVE_NUMBER = 8,
    IS_EXECUTOR_ALIVE = 9,
    ATTRIBUTE = 10,
    SKILLFUL_MAIN_WEAPON_TYPE = 11,
    IS_SKILL_EXECUTOR_OR_COUNTER = 12
}
