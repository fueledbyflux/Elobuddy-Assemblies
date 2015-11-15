using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using YasuoBuddy.EvadePlus;

namespace YasuoBuddy
{
    internal static class Yasuo
    {
        private static Menu Menu;
        public static Menu ComboMenu, HarassMenu, FarmMenu, FleeMenu;
        private static Menu DrawMenu;
        public static Menu MiscSettings;
        private static int _cleanUpTime;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Yasuo) return;

            Menu = MainMenu.AddMenu("YasuoBuddy", "yasuobuddyfluxy");

            ComboMenu = Menu.AddSubMenu("Combo", "yasuCombo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("combo.Q", new CheckBox("Use Q"));
            ComboMenu.Add("combo.E", new CheckBox("Use E"));
            ComboMenu.Add("combo.stack", new CheckBox("Stack Q"));
            ComboMenu.Add("combo.leftclickRape", new CheckBox("Left Click Rape"));
            ComboMenu.AddSeparator();
            ComboMenu.AddLabel("R Settings");
            ComboMenu.Add("combo.R", new CheckBox("Use R"));
            ComboMenu.Add("combo.RTarget", new CheckBox("Use R always on Selected Target"));
            ComboMenu.Add("combo.RKillable", new CheckBox("Use R Execute"));
            ComboMenu.Add("combo.MinTargetsR", new Slider("Use R Min Targets", 2, 1, 5));

            HarassMenu = Menu.AddSubMenu("Harass", "yasuHarass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("harass.Q", new CheckBox("Use Q"));
            HarassMenu.Add("harass.E", new CheckBox("Use E"));
            HarassMenu.Add("harass.stack", new CheckBox("Stack Q"));

            FarmMenu = Menu.AddSubMenu("Farming Settings", "yasuoFarm");
            FarmMenu.AddGroupLabel("Farming Settings");
            FarmMenu.AddLabel("Last Hit");
            FarmMenu.Add("LH.Q", new CheckBox("Use Q"));
            FarmMenu.Add("LH.E", new CheckBox("Use E"));
            FarmMenu.Add("LH.EUnderTower", new CheckBox("Use E Under Tower", false));

            FarmMenu.AddLabel("WaveClear");
            FarmMenu.Add("WC.Q", new CheckBox("Use Q"));
            FarmMenu.Add("WC.E", new CheckBox("Use E"));
            FarmMenu.Add("WC.EUnderTower", new CheckBox("Use E Under Tower", false));

            FarmMenu.AddLabel("Jungle");
            FarmMenu.Add("JNG.Q", new CheckBox("Use Q"));
            FarmMenu.Add("JNG.E", new CheckBox("Use E"));

            FleeMenu = Menu.AddSubMenu("Flee/Evade", "yasuoFlee");
            FleeMenu.AddGroupLabel("Flee Settings");
            FleeMenu.Add("Flee.E", new CheckBox("Use E"));
            FleeMenu.Add("Flee.stack", new CheckBox("Stack Q"));
            FleeMenu.AddGroupLabel("Evade Settings");
            FleeMenu.Add("Evade.E", new CheckBox("Use E to Evade"));
            FleeMenu.Add("Evade.W", new CheckBox("Use W to Evade"));
            FleeMenu.Add("Evade.WDelay", new Slider("Humanizer Delay (ms)", 0, 0, 1000));

            MiscSettings = Menu.AddSubMenu("Misc Settings");
            MiscSettings.AddGroupLabel("KillSteal Settings");
            MiscSettings.Add("KS.Q", new CheckBox("Use Q"));
            MiscSettings.Add("KS.E", new CheckBox("Use E"));
            MiscSettings.AddGroupLabel("Auto Q Settings");
            MiscSettings.Add("Auto.Q3", new CheckBox("Use Q3"));
            MiscSettings.Add("Auto.Active", new KeyBind("Auto Q Enemy", false, KeyBind.BindTypes.PressToggle, 'M'));

            Program.Main(null);

            DrawMenu = Menu.AddSubMenu("Draw", "yasuoDraw");
            DrawMenu.AddGroupLabel("Draw Settings");

            DrawMenu.Add("Draw.Q", new CheckBox("Draw Q", false));
            DrawMenu.AddColourItem("Draw.Q.Colour");
            DrawMenu.AddSeparator();

            DrawMenu.Add("Draw.E", new CheckBox("Draw E", false));
            DrawMenu.AddColourItem("Draw.E.Colour");
            DrawMenu.AddSeparator();

            DrawMenu.Add("Draw.R", new CheckBox("Draw R", false));
            DrawMenu.AddColourItem("Draw.R.Colour");
            DrawMenu.AddSeparator();

            DrawMenu.AddLabel("When Spell is Down Colour = ");
            DrawMenu.AddColourItem("Draw.Down", 7);
            
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            EEvader.Init();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["Draw.Q"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(
                    SpellManager.Q.IsReady() ? DrawMenu.GetColour("Draw.Q.Colour") : DrawMenu.GetColour("Draw.Down"),
                    SpellManager.Q.Range, Player.Instance.Position);
            }
            if (DrawMenu["Draw.R"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(
                    SpellManager.R.IsReady() ? DrawMenu.GetColour("Draw.R.Colour") : DrawMenu.GetColour("Draw.Down"),
                    SpellManager.R.Range, Player.Instance.Position);
            }
            if (DrawMenu["Draw.E"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(
                    SpellManager.E.IsReady() ? DrawMenu.GetColour("Draw.E.Colour") : DrawMenu.GetColour("Draw.Down"),
                    SpellManager.E.Range, Player.Instance.Position);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_cleanUpTime < Environment.TickCount)
            {
                GC.Collect();
                _cleanUpTime = Environment.TickCount + 1000000;
            }
            StateManager.KillSteal();
            if (MiscSettings["Auto.Active"].Cast<KeyBind>().CurrentValue)
            {
                StateManager.AutoQ();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                StateManager.Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                StateManager.Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                StateManager.Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                StateManager.LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                StateManager.WaveClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                StateManager.Jungle();
            }
        }
    }
}