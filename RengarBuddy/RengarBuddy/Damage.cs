using EloBuddy;
using EloBuddy.SDK;

namespace RengarBuddy
{
    
    class Damage
    {
        private static AIHeroClient source { get { return ObjectManager.Player; } }
        public static double Q1(Obj_AI_Base target)
        {
            return source.CalculateDamageOnUnit(target, DamageType.Physical,
                (float) (new double[] {30, 60, 90, 120, 150}[Program.Q.Level - 1]
                         + new double[] {0, 5, 10, 15, 20}[Program.Q.Level - 1]/100
                         *(source.BaseAttackDamage + source.FlatPhysicalDamageMod)));
        }

        public static double Q2(Obj_AI_Base target)
        {
            return source.CalculateDamageOnUnit(target, DamageType.Physical,
                (float) (new double[] {30, 60, 90, 120, 150}[Program.Q.Level - 1]
                         + (new double[] {100, 105, 110, 115, 120}[Program.Q.Level - 1]/100 - 1)
                         *(source.BaseAttackDamage + source.FlatPhysicalDamageMod)));
        }

        public static double W(Obj_AI_Base target)
        {
            return source.CalculateDamageOnUnit(target, DamageType.Magical,
                (float) (new double[] {50, 80, 110, 140, 170}[Program.W.Level - 1]
                         + 0.8*source.FlatMagicDamageMod));
        }

        public static double E(Obj_AI_Base target)
        {
            return source.CalculateDamageOnUnit(target, DamageType.Physical,
                (float) (new double[] {50, 100, 150, 200, 250}[Program.E.Level - 1]
                         + 0.7*source.FlatPhysicalDamageMod));
        }

    }
}
