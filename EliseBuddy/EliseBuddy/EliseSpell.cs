using System;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace EliseBuddy
{
    internal class EliseSpell
    {
        public float Cooldown;
        public long LastCastT;

        public EliseSpell(string s)
        {
            SpellName = s;
        }

        private string SpellName { get; set; }
    }

    internal static class EliseSpellManager
    {
        public static Dictionary<string, EliseSpell> SpellCasts = new Dictionary<string, EliseSpell>
        {
            {"EliseHumanQ", new EliseSpell("EliseHumanQ")},
            {"EliseHumanW", new EliseSpell("EliseHumanW")},
            {"EliseHumanE", new EliseSpell("EliseHumanE")},
            {"EliseSpiderQCast", new EliseSpell("EliseSpiderQCast")},
            {"EliseSpiderW", new EliseSpell("EliseSpiderW")},
            {"EliseSpiderEInitial", new EliseSpell("EliseSpiderEInitial")}
        };

        public static string HumanQ
        {
            get { return "EliseHumanQ"; }
        }

        public static string SpiderQ
        {
            get { return "EliseSpiderQCast"; }
        }

        public static string HumanW
        {
            get { return "EliseHumanW"; }
        }

        public static string SpiderW
        {
            get { return "EliseSpiderW"; }
        }

        public static string HumanE
        {
            get { return "EliseHumanE"; }
        }

        public static string SpiderE
        {
            get { return "EliseSpiderEInitial"; }
        }

        public static bool IsSpider
        {
            get
            {
                return Player.GetSpell(SpellSlot.Q).Name == SpiderQ ||
                       Player.GetSpell(SpellSlot.W).Name == SpiderW ||
                       Player.GetSpell(SpellSlot.E).Name == SpiderE;
            }
        }

        public static Spell.Targeted HumanQSpell = new Spell.Targeted(SpellSlot.Q, 625);
        public static Spell.Targeted SpiderQSpell = new Spell.Targeted(SpellSlot.Q, 475);
        public static Spell.Skillshot HumanWSpell = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Linear, 250, 1000, 100);
        public static Spell.Active SpiderWSpell = new Spell.Active(SpellSlot.W);
        public static Spell.Skillshot HumanESpell = new Spell.Skillshot(SpellSlot.E, 1075, SkillShotType.Linear, 250, 1300, 55) {AllowedCollisionCount = 0};
        public static Spell.Targeted SpiderESpell = new Spell.Targeted(SpellSlot.E, 750);
        public static Spell.Active RSpell = new Spell.Active(SpellSlot.R);

        public static bool SpellIsReady(string s)
        {
            return SpellCasts[s].LastCastT + SpellCasts[s].Cooldown < Environment.TickCount;
        }

        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || !SpellCasts.ContainsKey(args.SData.Name)) return;

            SpellCasts[args.SData.Name].Cooldown = args.SData.CooldownTime*1000;
            SpellCasts[args.SData.Name].LastCastT = Environment.TickCount;
        }
    }
}