using EloBuddy;
using EloBuddy.SDK;
using SharpDX;

namespace JinxBuddy
{
    internal static class UltimateHandler
    {
        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static bool IsKillableByR(this AIHeroClient target)
        {
            return RDamage(target) > target.Health + target.AttackShield;
        }

        private static float RDamage(Obj_AI_Base target)
        {
            if (!Program.R.IsLearned) return 0;
            var level = Program.R.Level - 1;

            if (target.Distance(_Player) < 1350)
            {
                return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                    (float)
                        (new double[] {25, 35, 45}[level] +
                         new double[] {25, 30, 35}[level]/100*(target.MaxHealth - target.Health) +
                         0.1*_Player.FlatPhysicalDamageMod));
            }

            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)
                    (new double[] {250, 350, 450}[level] +
                     new double[] {25, 30, 35}[level]/100*(target.MaxHealth - target.Health) +
                     1*_Player.FlatPhysicalDamageMod));
        }

        internal static float UltSpeed(Vector3 endPosition)
        {
            return 1700f;
        }
    }
}
