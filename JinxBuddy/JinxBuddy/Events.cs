using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;

namespace JinxBuddy
{
    internal static class Events
    {
        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static float FishBonesBonus
        {
            get { return 75f + 25f*Program.Q.Level; }
        }

        public static float MinigunRange(Obj_AI_Base target = null)
        {
            return (590 + (target != null ? target.BoundingRadius : 0));
        }

        public static bool FishBonesActive
        {
            get { return _Player.AttackRange > 525; }
        }

        public const int AoeRadius = 200;

        public static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Program.MiscMenu["gapcloser"].Cast<CheckBox>().CurrentValue && sender.IsEnemy &&
                e.End.Distance(_Player) < 200)
            {
                Program.E.Cast(e.End);
            }
        }

    }
}
