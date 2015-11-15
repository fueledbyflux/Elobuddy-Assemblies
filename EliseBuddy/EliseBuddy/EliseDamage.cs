using EloBuddy;
using EloBuddy.SDK;

namespace EliseBuddy
{
    internal static class EliseDamage
    {
        public static float QHumanDamage(Obj_AI_Base target)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                (float) (new double[] {40, 75, 110, 145, 180}[Player.GetSpell(SpellSlot.Q).Level - 1]
                         + (0.08 + 0.03/100*Player.Instance.FlatMagicDamageMod)*target.Health));
        }

        public static float QSpiderDamage(Obj_AI_Base target)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new double[] { 60, 100, 140, 180, 220 }[Player.GetSpell(SpellSlot.Q).Level - 1]
                                    + (0.08 + 0.03 / 100 * Player.Instance.FlatMagicDamageMod)
                                    * (target.MaxHealth - target.Health)));
        }
        public static float WHumanDamage(Obj_AI_Base target)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new double[] { 75, 125, 175, 225, 275 }[Player.GetSpell(SpellSlot.W).Level - 1]
                                    + 0.8 * Player.Instance.FlatMagicDamageMod));
        }
    }
}
