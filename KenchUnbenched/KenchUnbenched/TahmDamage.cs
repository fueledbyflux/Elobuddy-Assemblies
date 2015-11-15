using System;
using EloBuddy;
using EloBuddy.SDK;

namespace KenchUnbenched
{
    internal static class TahmDamage
    {
        public static float QDamage(Obj_AI_Base target)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)
                    (new[] {80, 125, 170, 215, 260}[Player.GetSpell(SpellSlot.Q).Level - 1] +
                     (0.7*Player.Instance.FlatMagicDamageMod)));
        }

        public static float WDamage(Obj_AI_Base target)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                ((float)
                    (new[] {20, 23, 26, 29, 32}[Player.GetSpell(SpellSlot.W).Level - 1] +
                     (Math.Floor(Player.Instance.FlatMagicDamageMod/100)*2)))/100*target.MaxHealth);
        }

        public static float WPDamage(Obj_AI_Base target)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)
                    (new[] { 100, 150, 200, 250, 300 }[Player.GetSpell(SpellSlot.W).Level - 1] +
                     (0.6 * Player.Instance.FlatMagicDamageMod)));
        }
    }
}
