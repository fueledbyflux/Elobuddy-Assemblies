﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace LeeSinBuddy
{
    class Smiter
    {
        public static readonly string[] SmiteableUnits =
        {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron"
        };

        private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3724, 3723, 3933 };
        private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719, 3932 };
        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714, 3931 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707, 3930 };

        public static Spell.Targeted Smite;
        public static Menu SmiteMenu;

        private static void SetSmiteSlot()
        {
            SpellSlot smiteSlot;
            if (SmiteBlue.Any(x => ObjectManager.Player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId)x) != null))
                smiteSlot = ObjectManager.Player.GetSpellSlotFromName("s5_summonersmiteplayerganker");
            else if (SmiteRed.Any(x => ObjectManager.Player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId)x) != null))
                smiteSlot = ObjectManager.Player.GetSpellSlotFromName("s5_summonersmiteduel");
            else if (SmiteGrey.Any(x => ObjectManager.Player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId)x) != null))
                smiteSlot = ObjectManager.Player.GetSpellSlotFromName("s5_summonersmitequick");
            else if (SmitePurple.Any(x => ObjectManager.Player.InventoryItems.FirstOrDefault(a => a.Id == (ItemId)x) != null))
                smiteSlot = ObjectManager.Player.GetSpellSlotFromName("itemsmiteaoe");
            else
                smiteSlot = ObjectManager.Player.GetSpellSlotFromName("summonersmite");
            Smite = new Spell.Targeted(smiteSlot, 500);
        }

        public static int GetSmiteDamage()
        {
            if (Smite == null || !Smite.IsReady())
            {
                return 0;
            }
            int level = ObjectManager.Player.Level;
            int[] smitedamage =
            {
                20*level + 370,
                30*level + 330,
                40*level + 240,
                50*level + 100
            };
            return smitedamage.Max();
        }

        public static void Init()
        {
            SmiteMenu = Program.menu.AddSubMenu("Smite Settings");
            SmiteMenu.AddGroupLabel("Smite Settings");
            SmiteMenu.AddLabel("Combo Settings");
            SmiteMenu.Add("smiteQ", new CheckBox("Q -> Smite"));
            SmiteMenu.AddLabel("Settings");
            SmiteMenu.Add("smiteEnabled", new KeyBind("Smite Enabled", false, KeyBind.BindTypes.PressToggle, 'H'));
            SmiteMenu.Add("regularSmite", new CheckBox("Regular Smite"));
            SmiteMenu.Add("QSmite", new CheckBox("Q => Smite => Q"));
            SmiteMenu.Add("drawSmite", new CheckBox("Draw Smite"));
            SmiteMenu.AddLabel("Camps");
            SmiteMenu.Add("SRU_Red", new CheckBox("Red"));
            SmiteMenu.Add("SRU_Blue", new CheckBox("Blue"));
            SmiteMenu.Add("SRU_Dragon", new CheckBox("Dragon"));
            SmiteMenu.Add("SRU_Baron", new CheckBox("Baron"));

            SetSmiteSlot();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SmiteMenu["drawSmite"].Cast<CheckBox>().CurrentValue && SmiteMenu["smiteEnabled"].Cast<KeyBind>().CurrentValue)
            {
                Circle.Draw(Color.White, Smite.Range, Player.Instance.Position);
            }
        }

        public static bool ForceSmite;

        private static void Game_OnUpdate(EventArgs args)
        {
            SetSmiteSlot();

            if (!SmiteMenu["smiteEnabled"].Cast<KeyBind>().CurrentValue) return;

            var minion = ObjectManager.Get<Obj_AI_Base>().Where(a => SmiteableUnits.Contains(a.BaseSkinName) && SmiteMenu[a.BaseSkinName].Cast<CheckBox>() != null && SmiteMenu[a.BaseSkinName].Cast<CheckBox>().CurrentValue).OrderByDescending(a => a.MaxHealth).FirstOrDefault(a => a.IsValidTarget(1400));
            if (minion == null) return;
            if (minion.IsValidTarget(Smite.Range) && minion.Health <= GetSmiteDamage() && SmiteMenu["regularSmite"].Cast<CheckBox>().CurrentValue || ForceSmite && Player.Instance.Distance(minion) < 100)
            {
                Smite.Cast(minion);
                ForceSmite = false;
                return;
            }
            if (Program.Q.IsReady() && minion.HasQBuff() && SmiteMenu["QSmite"].Cast<CheckBox>().CurrentValue && minion.Health <= Damage.Q2Damage(minion, GetSmiteDamage(), true) + GetSmiteDamage())
            {
                Program.Q2.Cast();
                ForceSmite = true;
                return;
            }
            if (Program.Q.IsReady() && SmiteMenu["QSmite"].Cast<CheckBox>().CurrentValue && Program.Q.Instance().Name == Program.Spells["Q1"] && minion.IsValidTarget(Program.Q.Range) &&
                minion.Health <=
                Damage.QDamage(minion) + Damage.Q2Damage(minion, Damage.QDamage(minion) + GetSmiteDamage(), true) +
                GetSmiteDamage())
            {
                Program.Q.Cast(minion);
            }
        }
    }
}
