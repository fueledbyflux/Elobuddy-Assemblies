using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace GangplankBuddy
{
    internal static class SpellManager
    {
        public static Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, 625);
        public static Spell.Active W = new Spell.Active(SpellSlot.W);
        public static Spell.Skillshot E = new Spell.Skillshot(SpellSlot.E, 1200, SkillShotType.Circular);
        public static Spell.Skillshot R = new Spell.Skillshot(SpellSlot.R, uint.MaxValue, SkillShotType.Circular);
    }
}
