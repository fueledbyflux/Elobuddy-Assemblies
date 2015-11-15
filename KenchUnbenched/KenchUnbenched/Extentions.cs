using System.Linq;
using EloBuddy;
using EloBuddy.SDK;

namespace KenchUnbenched
{
    internal static class Extentions
    {
        public static Obj_AI_Base Caster(this BuffInstance buffInstance)
        {
            var caster = EntityManager.Heroes.AllHeroes.FirstOrDefault(o => o.Name == buffInstance.SourceName);
            return caster ?? Player.Instance;
        }

        public static bool HasBuff(this Obj_AI_Base t, string s)
        {
            return t.Buffs.Any(a => a.Name.ToLower().Contains(s.ToLower()));
        }
    }
}
