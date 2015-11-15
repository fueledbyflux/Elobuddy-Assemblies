using System;
using EloBuddy;

namespace KenchUnbenched
{
    internal static class KenchCheckManager
    {

        // W Stuff
        private static int[] WSpellDelay = { 4000, 4500, 5000, 5500, 6000 };
        private static int WExpire;
        public static Obj_AI_Base WTarget;


        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;

            var target = (Obj_AI_Base)args.Target;

            if (args.Slot == SpellSlot.W && args.Target != null && target.Name != Player.Instance.Name)
            {
                if (target.IsAlly || target.IsMinion)
                {
                    WExpire = Environment.TickCount + WSpellDelay[KenchUnbenched.WSpellSwallow.Level - 1];
                    WTarget = target;
                    return;
                }
                if (target.IsEnemy && target is AIHeroClient)
                {
                    WExpire = Environment.TickCount + (WSpellDelay[KenchUnbenched.WSpellSwallow.Level - 1] / 2);
                    WTarget = target;
                    return;
                }
            }

            if (args.Slot != SpellSlot.W || !IsSwallowed()) return;
            WExpire = Environment.TickCount - 1;
            WTarget = null;
        }

        public static bool IsEmpowered(this AIHeroClient target)
        {
            return target.HasBuff("tahmkenchpdevourable");
        }

        public static bool IsSwallowed()
        {
            if (!Player.Instance.IsDead && WExpire >= Environment.TickCount) return true;
            WTarget = null;
            return false;
        }
    }
}
