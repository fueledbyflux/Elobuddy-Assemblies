using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;

namespace YasuoBuddy
{
    internal static class Extentions
    {
        public static bool HasWhirlwind(this AIHeroClient unit)
        {
            return unit.HasBuff("YasuoQ3W");
        }

        public static bool IsKnockedUp(this Obj_AI_Base unit)
        {
            return unit.HasBuffOfType(BuffType.Knockback) || unit.HasBuffOfType(BuffType.Knockup);
        }

        public static bool IsUnderTower(this Vector3 v)
        {
            return EntityManager.Turrets.Enemies.Where(a => a.Health > 0 && !a.IsDead).Any(a => a.Distance(v) < 950);
        }
    }
}
