using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace RengarBuddy
{
    internal class Program
    {
        public static Menu ComboMenu, FarmMenu, JungleMenu, HarassMenu, DrawMenu, menu;
        public static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Active R;
        public static bool DisableAntiSkills = true;
        public static AIHeroClient _Player { get { return ObjectManager.Player; } }
        public static int Mana { get { return (int) _Player.Mana; } }
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != Champion.Rengar.ToString()) return;

            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Active(SpellSlot.W, 500);
            E = new Spell.Skillshot(SpellSlot.E, 1000, SkillShotType.Linear, 250, 1500, 70);
            R = new Spell.Active(SpellSlot.R);

            menu = MainMenu.AddMenu("RengarBuddy", "rengarbuddy");
            menu.AddGroupLabel("Rengar Buddy");
            menu.AddLabel("query me? jgottabekiddingme");
            menu.AddLabel("that was a jq joke, i hope you got it");
            menu.AddLabel("oh yea, this is made by fluxy (woot?)");

            ComboMenu = menu.AddSubMenu("Combo Menu", "comboMenuRB");

            ComboMenu.AddGroupLabel("5 Ferocity Settings");
            ComboMenu.Add("ferocity", new CheckBox("Use 5 Ferocity"));
            ComboMenu.AddLabel("Smart Mode Settings");
            ComboMenu.Add("modeType", new CheckBox("Smart Mode Active"));
            ComboMenu.AddLabel("Smart Spell Settings");
            ComboMenu.Add("qInRange", new CheckBox("Q In AA Range"));
            ComboMenu.Add("eOutOfRange", new CheckBox("E Out Of Range"));
            ComboMenu.Add("wHealthPercent", new CheckBox("W <= % Health"));
            ComboMenu.Add("wHealthPercentSlider", new Slider("% Health", 30));
            ComboMenu.AddSeparator();
            ComboMenu.AddLabel("Regular Mode Settings");
            var stacks = ComboMenu.Add("selectedStackedSpell", new Slider("Selected Spell", 0, 0, 2));
            stacks.OnValueChange += delegate
            {
                stacks.DisplayName = "5 Ferocity Priority: " + new[] {"Q", "W", "E"}[stacks.CurrentValue];
            };
            stacks.DisplayName = "5 Ferocity Priority: " + new[] { "Q", "W", "E" }[stacks.CurrentValue];
            ComboMenu.AddSeparator();
            ComboMenu.AddGroupLabel("Regular Combo");
            ComboMenu.Add("qCombo", new CheckBox("Use Q"));
            ComboMenu.Add("wCombo", new CheckBox("Use W"));
            ComboMenu.Add("eCombo", new CheckBox("Use E"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("useYomuus", new CheckBox("use Yomuus in R"));


            HarassMenu = menu.AddSubMenu("Harass Menu", "harassMenuRB");
            HarassMenu.Add("save5StacksHarass", new CheckBox("Save 5 Stacks"));
            var stacks2 = HarassMenu.Add("selectedStackedSpellHarass", new Slider("Selected Spell", 0, 0, 2));
            stacks2.OnValueChange += delegate
            {
                stacks2.DisplayName = "5 Ferocity Priority: " + new[] { "Q", "W", "E" }[stacks.CurrentValue];
            };
            stacks.DisplayName = "5 Ferocity Priority: " + new[] { "Q", "W", "E" }[stacks.CurrentValue];
            HarassMenu.AddSeparator();
            HarassMenu.Add("qHarass", new CheckBox("Use Q"));
            HarassMenu.Add("wHarass", new CheckBox("Use W"));
            HarassMenu.Add("eHarass", new CheckBox("Use E"));


            FarmMenu = menu.AddSubMenu("Farm Menu", "farmMenuRB");
            FarmMenu.AddGroupLabel("Wave Clear");
            FarmMenu.Add("qWaveClear", new CheckBox("Use Q"));
            FarmMenu.Add("wWaveClear", new CheckBox("Use W"));
            FarmMenu.Add("eWaveClear", new CheckBox("Use E"));
            FarmMenu.Add("saveStacksWC", new CheckBox("Save Stacks In WC"));
            var stacks3 = FarmMenu.Add("selectedStackedSpellWC", new Slider("Selected Spell", 0, 0, 2));
            stacks3.OnValueChange += delegate
            {
                stacks3.DisplayName = "5 Ferocity Priority: " + new[] { "Q", "W", "E" }[stacks.CurrentValue];
            };
            FarmMenu.AddGroupLabel("Last Hit");
            FarmMenu.Add("qLastHit", new CheckBox("Use Q"));
            FarmMenu.Add("eLastHit", new CheckBox("Use E"));


            JungleMenu = menu.AddSubMenu("Jungle Menu", "jungleMenuRB");
            JungleMenu.Add("saveStacksJungle", new CheckBox("Save Stacks In Jungle"));
            var stacks4 = JungleMenu.Add("selectedStackedSpellJNG", new Slider("Selected Spell", 0, 0, 2));
            stacks4.OnValueChange += delegate
            {
                stacks4.DisplayName = "5 Ferocity Priority: " + new[] { "Q", "W", "E" }[stacks.CurrentValue];
            };
            JungleMenu.Add("qJng", new CheckBox("Use Q"));
            JungleMenu.Add("wJng", new CheckBox("Use W"));
            JungleMenu.Add("eJng", new CheckBox("Use E"));

            DrawMenu = menu.AddSubMenu("Draw Menu", "drawmenu");
            DrawMenu.Add("drawW", new CheckBox("Draw W"));
            DrawMenu.Add("drawE", new CheckBox("Draw E"));
            
            Game.OnTick += Game_OnTick;
            Obj_AI_Base.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
            Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.IsAutoAttack() && Player.HasBuff("RengarR") && DisableAntiSkills)
            {
                DisableAntiSkills = false;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(W.IsReady() ? Color.BlueViolet : Color.OrangeRed, W.Range, Player.Instance.Position);
            }
            if (DrawMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(E.IsReady() ? Color.BlueViolet : Color.OrangeRed, E.Range, Player.Instance.Position);
            }
        }

        private static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "RengarR")
            {
                Core.DelayAction(() => DisableAntiSkills = true, 300);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (!Player.HasBuff("RengarR") || Player.Instance.GetAutoAttackRange() < 200)
            {
                DisableAntiSkills = false;
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                StateManager.Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                StateManager.Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                StateManager.WaveClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                StateManager.Jungle();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                StateManager.LastHit();
            }
        }
    }


}
