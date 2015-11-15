using System;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace LeeSinBuddy
{
    internal static class Program
    {
        public static Menu Config;
        //Spells
        public static Spell.Skillshot Q;
        public static Spell.Active Q2;
        public static Spell.Targeted W;
        public static Spell.Active E;
        public static Spell.Active E2;
        public static Spell.Targeted R;
        private static Spell.Targeted R2;
        private static string LastSpell;
        public static long LastSpellTime;
        private static long PassiveTimer;
        public static int PassiveStacks;
        public static Menu menu;
        private static Menu DrawingMenu;

        public static Dictionary<string, string> Spells = new Dictionary<string, string>
        {
            {"Q1", "BlindMonkQOne"},
            {"W1", "BlindMonkWOne"},
            {"E1", "BlindMonkEOne"},
            {"W2", "blindmonkwtwo"},
            {"Q2", "blindmonkqtwo"},
            {"E2", "blindmonketwo"},
            {"R1", "BlindMonkRKick"}
        };

        public static Dictionary<string, long> LastCast = new Dictionary<string, long>
        {
            {"BlindMonkQOne", 0},
            {"BlindMonkWOne", 0},
            {"BlindMonkEOne", 0},
            {"blindmonkwtwo", 0},
            {"blindmonkqtwo", 0},
            {"blindmonketwo", 0},
            {"BlindMonkRKick", 0}
        };

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            if (args == null) return;
            try
            {
                Loading.OnLoadingComplete += Load_OnLoad;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void Load_OnLoad(EventArgs a)
        {
            if (Player.Instance.Hero != Champion.LeeSin) return;

            menu = MainMenu.AddMenu("Fluxy's LeeSinBuddy", "LeeSinBuddyMenu");
            menu.AddGroupLabel("Fluxy's LeeSinBuddy");
            menu.AddSeparator();
            menu.AddLabel("Made By Fluxy");
            menu.AddLabel("fleshlit by finn");
            menu.AddLabel("no jquery here :P");

            DrawingMenu = menu.AddSubMenu("Drawing Settings", "menuleesin");
            DrawingMenu.Add("drawQ1", new CheckBox("Draw Q1", false));
            DrawingMenu.Add("drawQ2", new CheckBox("Draw Q2", false));
            DrawingMenu.Add("drawW1", new CheckBox("Draw W1", false));
            DrawingMenu.Add("drawE1", new CheckBox("Draw E1", false));
            DrawingMenu.Add("drawE2", new CheckBox("Draw E2", false));
            DrawingMenu.Add("drawR", new CheckBox("Draw R", false));

            Q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 250, 1800, 70);
            Q2 = new Spell.Active(SpellSlot.Q, 1400); // 1400
            W = new Spell.Targeted(SpellSlot.W, 700);
            E = new Spell.Active(SpellSlot.E, 430); //430
            E2 = new Spell.Active(SpellSlot.E, 600); // 600
            R = new Spell.Targeted(SpellSlot.R, 375);
            R2 = new Spell.Targeted(SpellSlot.R, 800);

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnPostAttack += Orbwalker_OnAttack;
            InsecManager.Init();
            StateManager.Init();
            WardJumper.Init();
            Smiter.Init();
            Game.OnTick += Game_OnTick;

            Chat.Print("Fluxy's Lee Sin Buddy Loaded.", System.Drawing.Color.BlueViolet);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingMenu["drawQ1"].Cast<CheckBox>().CurrentValue && Q.Instance().Name == Spells["Q1"])
            {
               Circle.Draw(Q.IsReady() ? Color.BlueViolet : Color.OrangeRed, Q.Range, Player.Instance.Position); 
            }
            if (DrawingMenu["drawQ2"].Cast<CheckBox>().CurrentValue && Q.Instance().Name == Spells["Q2"])
            {
                Circle.Draw(Q.IsReady() ? Color.BlueViolet : Color.OrangeRed, Q2.Range, Player.Instance.Position);
            }
            if (DrawingMenu["drawW1"].Cast<CheckBox>().CurrentValue && W.Instance().Name == Spells["W1"])
            {
                Circle.Draw(W.IsReady() ? Color.BlueViolet : Color.OrangeRed, W.Range, Player.Instance.Position);
            }
            if (DrawingMenu["drawE1"].Cast<CheckBox>().CurrentValue && E.Instance().Name == Spells["E1"])
            {
                Circle.Draw(E.IsReady() ? Color.BlueViolet : Color.OrangeRed, E.Range, Player.Instance.Position);
            }
            if (DrawingMenu["drawE2"].Cast<CheckBox>().CurrentValue && E.Instance().Name == Spells["E2"])
            {
                Circle.Draw(E.IsReady() ? Color.BlueViolet : Color.OrangeRed, E2.Range, Player.Instance.Position);
            }
            if (DrawingMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(R.IsReady() ? Color.BlueViolet : Color.OrangeRed, R.Range, Player.Instance.Position);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            StateManager.KillSteal();
            if (PassiveTimer <= Environment.TickCount)
            {
                PassiveStacks = 0;
            }
        }

        private static void Orbwalker_OnAttack(AttackableUnit target, EventArgs args)
        {
                if (PassiveStacks > 0)
                {
                    PassiveStacks--;
                }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;

            if (!LastCast.ContainsKey(args.SData.Name)) return;

            LastSpellTime = Environment.TickCount;
            LastSpell = args.SData.Name;
            LastCast[args.SData.Name] = Environment.TickCount;
            PassiveTimer = Environment.TickCount + 3000;
            PassiveStacks = 2;
        }
    }
}