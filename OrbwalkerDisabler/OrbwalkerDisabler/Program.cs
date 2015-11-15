using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace OrbwalkerDisabler
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static Menu Menu;

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Menu = MainMenu.AddMenu("Orbwalker Disabler", "orbws");
            Menu.AddGroupLabel("Combo");
            var ac = Menu.Add("disableMovementCombo", new CheckBox("Disable Movement", false));
            var bc = Menu.Add("disableAttacksCombo", new CheckBox("Disable Attacks", false));

            Menu.AddGroupLabel("Harass");
            var ah = Menu.Add("disableMovementHarass", new CheckBox("Disable Movement", false));
            var bh = Menu.Add("disableAttacksHarass", new CheckBox("Disable Attacks", false));

            Menu.AddGroupLabel("WaveClear");
            var awc = Menu.Add("disableMovementWaveClear", new CheckBox("Disable Movement", false));
            var bwc = Menu.Add("disableAttacksWaveClear", new CheckBox("Disable Attacks", false));

            Menu.AddGroupLabel("LastHit");
            var alh = Menu.Add("disableMovementLastHit", new CheckBox("Disable Movement", false));
            var blh = Menu.Add("disableAttacksLastHit", new CheckBox("Disable Attacks", false));

            Menu.AddGroupLabel("Jungle");
            var ajg = Menu.Add("disableMovementJungle", new CheckBox("Disable Movement", false));
            var bjg = Menu.Add("disableAttacksJungle", new CheckBox("Disable Attacks", false));

            Menu.AddGroupLabel("Flee");
            var af = Menu.Add("disableMovementFlee", new CheckBox("Disable Movement", false));


            Game.OnTick += delegate
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Orbwalker.DisableAttacking = bc.CurrentValue;
                    Orbwalker.DisableMovement = ac.CurrentValue;
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    Orbwalker.DisableAttacking = bh.CurrentValue;
                    Orbwalker.DisableMovement = ah.CurrentValue;
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {
                    Orbwalker.DisableAttacking = bwc.CurrentValue;
                    Orbwalker.DisableMovement = awc.CurrentValue;
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                {
                    Orbwalker.DisableAttacking = blh.CurrentValue;
                    Orbwalker.DisableMovement = alh.CurrentValue;
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    Orbwalker.DisableAttacking = bjg.CurrentValue;
                    Orbwalker.DisableMovement = ajg.CurrentValue;
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                {
                    Orbwalker.DisableMovement = af.CurrentValue;
                }
            };
        }
    }
}
