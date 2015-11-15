using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace GangplankBuddy
{
    internal static class Program
    {
        private static Menu Menu;
        public static Menu ComboMenu, HarassMenu, FarmingMenu, HealingMenu, DrawingMenu;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Game_OnStart;
        }

        private static void Game_OnStart(EventArgs args)
        {
            Menu = MainMenu.AddMenu("Gangplank", "gangbang");
            Menu.AddGroupLabel("Gangplank, You Are A Pirate!");
            Menu.AddLabel("Yarr Harr Fiddle me D, Being a Pirate is all that you need!");
            Menu.AddLabel("Do what you want cause a pirate is free!");
            Menu.AddLabel("You are a pirate!");

            ComboMenu = Menu.AddSubMenu("Combo Settings", "comboSettings");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddLabel("Q Settings");
            ComboMenu.Add("useQCombo", new CheckBox("Use Q on Enemies"));
            ComboMenu.Add("useQBarrels", new CheckBox("Use Q on Barrels"));
            ComboMenu.AddLabel("E Settings");
            ComboMenu.Add("useE", new CheckBox("Use Barrels"));
            ComboMenu.Add("useEMaxChain", new Slider("Max Barrel Chain", 3, 1, 3));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "HarassSettings");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.AddLabel("Q Settings");
            HarassMenu.Add("useQHarass", new CheckBox("Use Q on Enemies"));
            HarassMenu.Add("useQBarrelsHarass", new CheckBox("Use Q on Barrels"));
            HarassMenu.AddLabel("E Settings");
            HarassMenu.Add("useEHarass", new CheckBox("Use Barrels"));
            HarassMenu.Add("useEMaxChainHarass", new Slider("Max Barrel Chain", 3, 1, 3));

            FarmingMenu = Menu.AddSubMenu("Farming Settings", "farmsettings");
            FarmingMenu.AddGroupLabel("Farming Settings");
            FarmingMenu.AddSeparator();
            FarmingMenu.AddGroupLabel("Last Hit Settings");
            FarmingMenu.Add("useQLastHit", new CheckBox("Use Q Execute"));

            FarmingMenu.AddGroupLabel("WaveClear Settings");
            FarmingMenu.Add("useQWaveClear", new CheckBox("Use Q Execute"));
            FarmingMenu.AddLabel("Barrel Settings");
            FarmingMenu.Add("useEWaveClear", new CheckBox("Use E"));
            FarmingMenu.Add("useEWaveClearMin", new Slider("E Min Units", 3, 1, 8));
            FarmingMenu.Add("useEQKill", new CheckBox("Use Q on Barrel with Min Killable units"));
            FarmingMenu.Add("useEQKillMin", new Slider("Min Units", 2, 1, 8));

            HealingMenu = Menu.AddSubMenu("Healing Settings", "healSettings");
            HealingMenu.AddGroupLabel("Healing Settings");
            HealingMenu.Add("enableHeal", new CheckBox("Heal with W"));
            HealingMenu.Add("healMin", new Slider("Min % HP for Heal", 20, 1));
            HealingMenu.AddLabel("CC To Heal on");
            HealingMenu.Add("healStun", new CheckBox("Stun", false));
            HealingMenu.Add("healRoot", new CheckBox("Root", false));

            DrawingMenu = Menu.AddSubMenu("Drawing Settings", "drawSettings");
            DrawingMenu.AddGroupLabel("Drawing Settings");
            DrawingMenu.Add("drawQ", new CheckBox("Draw Q Range", false));
            DrawingMenu.Add("drawE", new CheckBox("Draw E Range", false));
            DrawingMenu.Add("drawKillable", new CheckBox("Draw Killable Barrels", false));
            DrawingMenu.Add("drawUnKillable", new CheckBox("Draw Un-Killable Barrels", false));

            BarrelManager.Init();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.Wheat, SpellManager.Q.Range, Player.Instance.Position);
            }
            if (DrawingMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.Wheat, SpellManager.E.Range, Player.Instance.Position);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            StateHandler.Healing();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                StateHandler.Combo();
            }
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                StateHandler.Harass();
            }
            else if(Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                StateHandler.Waveclear();
            }
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                StateHandler.LastHit();
            }
        }
    }
}
