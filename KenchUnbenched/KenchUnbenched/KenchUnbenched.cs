using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;

namespace KenchUnbenched
{
    internal static class KenchUnbenched
    {
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static Menu Menu, ComboMenu, HarassMenu, FarmingMenu, DrawMenu, KillStealMenu, SaveMenu;

        public static Spell.Skillshot QSpell = new Spell.Skillshot(SpellSlot.Q, 800, SkillShotType.Linear, 100, 2000, 75);
        public static Spell.Targeted WSpellSwallow = new Spell.Targeted(SpellSlot.W, 250);
        public static Spell.Skillshot WSpellSpit = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Linear, 100, 900, 75);
        private static Spell.Active ESpell = new Spell.Active(SpellSlot.E);

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.TahmKench) return;

            Menu = MainMenu.AddMenu("Kench Unbenched", "kbswag");
            Menu.AddGroupLabel("Kench Unbenched");

            ComboMenu = Menu.AddSubMenu("Combo Menu", "combomenuKench");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("Combo.Q", new CheckBox("Use Q"));
            ComboMenu.Add("Combo.QOnlyStun", new CheckBox("Use Q Only Stun / Out of AA"));
            ComboMenu.Add("Combo.W.Enemy", new CheckBox("Use W on Enemy"));
            ComboMenu.Add("Combo.W.Minion", new CheckBox("Use W on Minions to Spit"));
            ComboMenu.Add("Combo.E", new CheckBox("Use E"));

            HarassMenu = Menu.AddSubMenu("Harass Menu", "harassmenuKench");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("Harass.Q", new CheckBox("Use Q"));
            HarassMenu.Add("Harass.W.Enemy", new CheckBox("Use W on Enemy"));
            HarassMenu.Add("Harass.W.Minion", new CheckBox("Use W on Minions to Spit"));
            HarassMenu.Add("Harass.E", new CheckBox("Use E"));

            FarmingMenu = Menu.AddSubMenu("Farm Menu", "farmmenuKench");
            FarmingMenu.AddGroupLabel("Farm Settings");
            FarmingMenu.AddLabel("LastHit Settings");
            FarmingMenu.Add("LastHit.Q", new CheckBox("Use Q"));
            FarmingMenu.AddLabel("WaveClear Settings");
            FarmingMenu.Add("WaveClear.Q", new CheckBox("Use Q"));
            FarmingMenu.AddLabel("Jungle Settings");
            FarmingMenu.Add("Jungle.Q", new CheckBox("Use Q"));

            KenchSaver.Initialize();

            KillStealMenu = Menu.AddSubMenu("KillSteal Menu");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KillSteal.Q", new CheckBox("Use Q"));
            KillStealMenu.Add("KillSteal.W.Swallow", new CheckBox("Use W Swallow"));
            KillStealMenu.Add("KillSteal.W.Spit", new CheckBox("Use W Swallow/Spit"));


            DrawMenu = Menu.AddSubMenu("Draw Menu", "drawMenuKench");
            DrawMenu.AddGroupLabel("Draw Settings");
            DrawMenu.Add("Draw.Q", new CheckBox("Draw Q"));
            DrawMenu.AddColourItem("Draw.Q.Colour");
            DrawMenu.AddSeparator();
            DrawMenu.Add("Draw.W", new CheckBox("Draw W"));
            DrawMenu.AddColourItem("Draw.W.Colour");
            DrawMenu.AddSeparator();
            DrawMenu.Add("Draw.E", new CheckBox("Draw E"));
            DrawMenu.AddColourItem("Draw.E.Colour");
            DrawMenu.AddSeparator();
            DrawMenu.AddLabel("Off CD Colour");
            DrawMenu.AddColourItem("Draw.OFF");

            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += KenchCheckManager.Obj_AI_Base_OnProcessSpellCast;
            Game.OnTick += Game_OnTick;
        }

        private static void Game_OnTick(EventArgs args)
        {
            StateHandler.KillSteal();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                StateHandler.Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                StateHandler.Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                StateHandler.LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                StateHandler.JungleClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                StateHandler.WaveClear();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["Draw.Q"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(QSpell.IsReady() ? DrawMenu.GetColour("Draw.Q.Colour") : DrawMenu.GetColour("Draw.OFF"), QSpell.Range, Player.Instance.Position);
            }
            if (DrawMenu["Draw.W"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(WSpellSwallow.IsReady() ? DrawMenu.GetColour("Draw.W.Colour") : DrawMenu.GetColour("Draw.OFF"), WSpellSwallow.Range, Player.Instance.Position);
                Circle.Draw(WSpellSpit.IsReady() ? DrawMenu.GetColour("Draw.W.Colour") : DrawMenu.GetColour("Draw.OFF"), WSpellSpit.Range, Player.Instance.Position);
            }
            if (DrawMenu["Draw.E"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(ESpell.IsReady() ? DrawMenu.GetColour("Draw.E.Colour") : DrawMenu.GetColour("Draw.OFF"), ESpell.Range, Player.Instance.Position);
            }
        }

    }
}
