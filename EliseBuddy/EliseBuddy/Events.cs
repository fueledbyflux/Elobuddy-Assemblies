using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;

namespace EliseBuddy
{
    internal static class Events
    {
        public static void Init()
        {
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!Elise.ComboMenu["antiGapcloser"].Cast<CheckBox>().CurrentValue || !sender.IsEnemy ||
                !sender.IsValidTarget() || !(e.End.Distance(Player.Instance.Position) < 300)) return;
            if (!EliseSpellManager.IsSpider)
            {
                EliseSpellManager.HumanESpell.Cast(sender);
            }
        }
    }
}
