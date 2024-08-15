namespace BotRunner.Models
{
    public class SkillUpdate
    {
        public uint SkillInt1 { get; set; }
        public uint SkillInt2 { get; set; }
        public uint SkillInt3 { get; set; }
        public uint SkillInt4 { get; set; }
    }
    enum SkillUpdateState
    {
        SKILL_UNCHANGED = 0,
        SKILL_CHANGED = 1,
        SKILL_NEW = 2,
        SKILL_DELETED = 3
    }
}
