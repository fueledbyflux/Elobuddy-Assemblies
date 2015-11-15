using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace EliseBuddy
{
    internal static class Elise
    {
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static Menu Menu;
        public static Menu ComboMenu, HarassMenu, FarmMenu;
        private static Menu DrawMenu;

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Elise) return;
            
            Menu = MainMenu.AddMenu("Elise Buddy", "EliseBuddy");

            ComboMenu = Menu.AddSubMenu("Combo", "eliseCombo");
            ComboMenu.Add("comboR", new CheckBox("R"));
            ComboMenu.AddLabel("Human");
            ComboMenu.Add("comboHumanQ", new CheckBox("Human Q"));
            ComboMenu.Add("comboHumanW", new CheckBox("Human W"));
            ComboMenu.Add("comboHumanE", new CheckBox("Human E"));
            ComboMenu.AddLabel("Spider");
            ComboMenu.Add("comboSpiderQ", new CheckBox("Spider Q"));
            ComboMenu.Add("comboSpiderW", new CheckBox("Spider W"));
            ComboMenu.Add("comboSpiderE", new CheckBox("Spider E"));
            ComboMenu.Add("antiGapcloser", new CheckBox("Anti-Gapcloser"));


            HarassMenu = Menu.AddSubMenu("Harass", "eliseharass");
            HarassMenu.AddLabel("Human");
            HarassMenu.Add("harassHumanQ", new CheckBox("Human Q"));
            HarassMenu.Add("harassHumanW", new CheckBox("Human W"));
            HarassMenu.Add("harassHumanE", new CheckBox("Human E"));
            HarassMenu.AddLabel("Spider");
            HarassMenu.Add("harassSpiderQ", new CheckBox("Spider Q"));
            HarassMenu.Add("harassSpiderW", new CheckBox("Spider W"));
            HarassMenu.Add("harassSpiderE", new CheckBox("Spider E"));


            FarmMenu = Menu.AddSubMenu("Farm", "elisefarm");

            FarmMenu.AddGroupLabel("Last Hit");
            FarmMenu.AddLabel("Human");
            FarmMenu.Add("lhHumanQ", new CheckBox("Human Q"));
            FarmMenu.AddLabel("Spider");
            FarmMenu.Add("lhSpiderQ", new CheckBox("Spider Q"));

            FarmMenu.AddGroupLabel("Wave Clear");
            FarmMenu.AddLabel("Human");
            FarmMenu.Add("wcHumanQ", new CheckBox("Human Q"));
            FarmMenu.AddLabel("Spider");
            FarmMenu.Add("wcSpiderQ", new CheckBox("Spider Q"));

            FarmMenu.AddGroupLabel("Jungle");
            FarmMenu.Add("jungleR", new CheckBox("R"));
            FarmMenu.AddLabel("Human");
            FarmMenu.Add("jgHumanQ", new CheckBox("Human Q"));
            FarmMenu.Add("jgHumanW", new CheckBox("Human W"));
            FarmMenu.AddLabel("Spider");
            FarmMenu.Add("jgSpiderQ", new CheckBox("Spider Q"));
            FarmMenu.Add("jgSpiderW", new CheckBox("Spider W"));

            DrawMenu = Menu.AddSubMenu("Drawing", "drawelise");
            DrawMenu.Add("drawHumanQ", new CheckBox("Human Q", false));
            DrawMenu.Add("drawHumanW", new CheckBox("Human W", false));
            DrawMenu.Add("drawHumanE", new CheckBox("Human E", false));
            DrawMenu.Add("drawSpiderQ", new CheckBox("Spider Q", false));
            DrawMenu.Add("drawSpiderE", new CheckBox("Spider E", false));


            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Obj_AI_Base.OnProcessSpellCast += EliseSpellManager.Obj_AI_Base_OnProcessSpellCast;
            Events.Init();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["drawHumanQ"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(EliseSpellManager.SpellIsReady(EliseSpellManager.HumanQ) ? Color.BlueViolet : Color.OrangeRed, EliseSpellManager.HumanQSpell.Range, Player.Instance.Position);
            }
            if (DrawMenu["drawHumanW"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(EliseSpellManager.SpellIsReady(EliseSpellManager.HumanW) ? Color.BlueViolet : Color.OrangeRed, EliseSpellManager.HumanWSpell.Range, Player.Instance.Position);
            }
            if (DrawMenu["drawHumanE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(EliseSpellManager.SpellIsReady(EliseSpellManager.HumanE) ? Color.BlueViolet : Color.OrangeRed, EliseSpellManager.HumanESpell.Range, Player.Instance.Position);
            }
            if (DrawMenu["drawSpiderQ"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(EliseSpellManager.SpellIsReady(EliseSpellManager.SpiderQ) ? Color.DeepPink : Color.Silver, EliseSpellManager.SpiderQSpell.Range, Player.Instance.Position);
            }
            if (DrawMenu["drawSpiderE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(EliseSpellManager.SpellIsReady(EliseSpellManager.SpiderE) ? Color.DeepPink : Color.Silver, EliseSpellManager.SpiderESpell.Range, Player.Instance.Position);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                StateManager.Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                StateManager.Harass();
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
