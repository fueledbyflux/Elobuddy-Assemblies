using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;

namespace EliseBuddy
{
    internal static class StateManager
    {
        //soontm
        //Cocoon big minions with E, Deal AoE dmg to all with W in spider form and last hit minions with the priority towards the big ones with Q in spider form, also poison big ones at the beginning with Q in human form and always use W in human form if available
        public static void Combo()
        {
            var target = TargetSelector.GetTarget(1100, DamageType.Physical);
            if (target == null || !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;
            if (EliseSpellManager.IsSpider)
            {
                if (Elise.ComboMenu["comboSpiderQ"].Cast<CheckBox>().CurrentValue && EliseSpellManager.SpiderQSpell.IsReady() && target.IsValidTarget(EliseSpellManager.SpiderQSpell.Range))
                {
                    EliseSpellManager.SpiderQSpell.Cast(target);
                }
                else if (Elise.ComboMenu["comboSpiderW"].Cast<CheckBox>().CurrentValue && EliseSpellManager.SpiderWSpell.IsReady() && target.IsValidTarget(Player.Instance.GetAutoAttackRange(target)))
                {
                    EliseSpellManager.SpellCasts[EliseSpellManager.SpiderW].LastCastT = Environment.TickCount;
                    EliseSpellManager.SpiderWSpell.Cast();
                }
                else if (Elise.ComboMenu["comboSpiderE"].Cast<CheckBox>().CurrentValue && EliseSpellManager.SpiderESpell.IsReady() && !target.IsValidTarget(Player.Instance.GetAutoAttackRange(target) + 200) && target.IsValidTarget(EliseSpellManager.SpiderESpell.Range))
                {
                    EliseSpellManager.SpiderESpell.Cast(target);
                }
                else if (Elise.ComboMenu["comboR"].Cast<CheckBox>().CurrentValue && !(target.IsValidTarget(Player.Instance.GetAutoAttackRange(target)) && EliseSpellManager.SpellCasts[EliseSpellManager.SpiderW].LastCastT + 3000 < Environment.TickCount) &&
                    (EliseSpellManager.SpellIsReady(EliseSpellManager.HumanQ) && EliseSpellManager.HumanQSpell.IsInRange(target) 
                    || EliseSpellManager.SpellIsReady(EliseSpellManager.HumanW) && EliseSpellManager.HumanWSpell.IsInRange(target)
                    || EliseSpellManager.SpellIsReady(EliseSpellManager.HumanE) && EliseSpellManager.HumanESpell.IsInRange(target)))
                {
                    EliseSpellManager.RSpell.Cast();
                }
            }
            else
            {
                if (Elise.ComboMenu["comboHumanQ"].Cast<CheckBox>().CurrentValue && EliseSpellManager.HumanESpell.IsReady() && target.IsValidTarget(EliseSpellManager.HumanESpell.Range) && EliseSpellManager.HumanESpell.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    EliseSpellManager.HumanESpell.Cast(target);
                }
                else if (Elise.ComboMenu["comboHumanW"].Cast<CheckBox>().CurrentValue && EliseSpellManager.HumanQSpell.IsReady() && target.IsValidTarget(EliseSpellManager.HumanQSpell.Range))
                {
                    EliseSpellManager.HumanQSpell.Cast(target);
                }
                else if (Elise.ComboMenu["comboHumanE"].Cast<CheckBox>().CurrentValue && EliseSpellManager.HumanWSpell.IsReady() && target.IsValidTarget(Player.Instance.GetAutoAttackRange(target)))
                {
                    EliseSpellManager.HumanWSpell.Cast(target);
                }
                else if (Elise.ComboMenu["comboR"].Cast<CheckBox>().CurrentValue && (EliseSpellManager.SpellIsReady(EliseSpellManager.SpiderE) && target.IsValidTarget(EliseSpellManager.SpiderESpell.Range) &&
                         (EliseSpellManager.SpellIsReady(EliseSpellManager.SpiderQ) || EliseSpellManager.SpellIsReady(EliseSpellManager.SpiderW))
                         || EliseSpellManager.SpellIsReady(EliseSpellManager.SpiderQ) && target.IsValidTarget(EliseSpellManager.SpiderQSpell.Range)
                         || EliseSpellManager.SpellIsReady(EliseSpellManager.SpiderW) && target.IsValidTarget(190 + target.BoundingRadius)))
                {
                    EliseSpellManager.RSpell.Cast();
                }
            }
        }
        public static void Harass()
        {
            var target = TargetSelector.GetTarget(1100, DamageType.Physical);
            if (target == null) return;
            if (EliseSpellManager.IsSpider)
            {
                if (Elise.HarassMenu["harassSpiderQ"].Cast<CheckBox>().CurrentValue && EliseSpellManager.SpiderQSpell.IsReady() && target.IsValidTarget(EliseSpellManager.SpiderQSpell.Range))
                {
                    EliseSpellManager.SpiderQSpell.Cast(target);
                }
                else if (Elise.HarassMenu["harassSpiderW"].Cast<CheckBox>().CurrentValue && EliseSpellManager.SpiderWSpell.IsReady() && target.IsValidTarget(Player.Instance.GetAutoAttackRange(target)))
                {
                    EliseSpellManager.SpiderWSpell.Cast();
                }
                else if (Elise.HarassMenu["harassSpiderE"].Cast<CheckBox>().CurrentValue && EliseSpellManager.SpiderESpell.IsReady() && !target.IsValidTarget(Player.Instance.GetAutoAttackRange(target) + 200) && target.IsValidTarget(EliseSpellManager.SpiderESpell.Range))
                {
                    EliseSpellManager.SpiderESpell.Cast(target);
                }
            }
            else
            {
                if (Elise.HarassMenu["harassHumanE"].Cast<CheckBox>().CurrentValue && EliseSpellManager.HumanESpell.IsReady() && target.IsValidTarget(EliseSpellManager.HumanESpell.Range) && EliseSpellManager.HumanESpell.GetPrediction(target).HitChance >= HitChance.Medium)
                {
                    EliseSpellManager.HumanESpell.Cast(target);
                }
                else if (Elise.HarassMenu["harassHumanQ"].Cast<CheckBox>().CurrentValue && EliseSpellManager.HumanQSpell.IsReady() && target.IsValidTarget(EliseSpellManager.HumanQSpell.Range))
                {
                    EliseSpellManager.HumanQSpell.Cast(target);
                }
                else if (Elise.HarassMenu["harassHumanW"].Cast<CheckBox>().CurrentValue && EliseSpellManager.HumanWSpell.IsReady() && target.IsValidTarget(Player.Instance.GetAutoAttackRange(target)))
                {
                    EliseSpellManager.HumanWSpell.Cast(target);
                }
            }
        }

        public static void LastHit()
        {
            var target = EntityManager.MinionsAndMonsters.GetLaneMinions().OrderByDescending(a => a.Health).FirstOrDefault(a => a.IsValidTarget(EliseSpellManager.IsSpider ? 475 : 675));
            if (target == null) return;
            if (EliseSpellManager.IsSpider)
            {
                if (Elise.FarmMenu["lhSpiderQ"].Cast<CheckBox>().CurrentValue && EliseSpellManager.SpiderQSpell.IsReady() && target.IsValidTarget(EliseSpellManager.SpiderQSpell.Range) && target.Health < EliseDamage.QSpiderDamage(target))
                {
                    EliseSpellManager.SpiderQSpell.Cast(target);
                }
            }
            else
            {
                if (Elise.FarmMenu["lhHumanQ"].Cast<CheckBox>().CurrentValue && EliseSpellManager.HumanQSpell.IsReady() && target.IsValidTarget(EliseSpellManager.HumanQSpell.Range) && target.Health < EliseDamage.QHumanDamage(target))
                {
                    EliseSpellManager.HumanQSpell.Cast(target);
                }
            }
        }

        public static void WaveClear()
        {
            var target = EntityManager.MinionsAndMonsters.GetLaneMinions().OrderByDescending(a => a.Health).FirstOrDefault(a => a.IsValidTarget(EliseSpellManager.IsSpider ? 475 : 675));
            if (target == null) return;
            if (EliseSpellManager.IsSpider)
            {
                if (Elise.FarmMenu["wcSpiderQ"].Cast<CheckBox>().CurrentValue && EliseSpellManager.SpiderQSpell.IsReady() && target.IsValidTarget(EliseSpellManager.SpiderQSpell.Range) && target.Health < EliseDamage.QSpiderDamage(target))
                {
                    EliseSpellManager.SpiderQSpell.Cast(target);
                }
            }
            else
            {
                if (Elise.FarmMenu["wcHumanQ"].Cast<CheckBox>().CurrentValue && EliseSpellManager.HumanQSpell.IsReady() && target.IsValidTarget(EliseSpellManager.HumanQSpell.Range) && target.Health < EliseDamage.QHumanDamage(target))
                {
                    EliseSpellManager.HumanQSpell.Cast(target);
                }
            }
        }

        public static void Jungle()
        {
            var target = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(a => a.MaxHealth).FirstOrDefault(a => a.IsValidTarget(900));
            if (target == null) return;
            if (EliseSpellManager.IsSpider)
            {
                if (Elise.FarmMenu["jgSpiderQ"].Cast<CheckBox>().CurrentValue && EliseSpellManager.SpiderQSpell.IsReady() && target.IsValidTarget(EliseSpellManager.SpiderQSpell.Range))
                {
                    EliseSpellManager.SpiderQSpell.Cast(target);
                }
                else if (Elise.FarmMenu["jgSpiderW"].Cast<CheckBox>().CurrentValue && EliseSpellManager.SpiderWSpell.IsReady() && target.IsValidTarget(Player.Instance.GetAutoAttackRange(target)))
                {
                    EliseSpellManager.SpellCasts[EliseSpellManager.SpiderW].LastCastT = Environment.TickCount;
                    EliseSpellManager.SpiderWSpell.Cast();
                }
                else if (
                    Elise.FarmMenu["jungleR"].Cast<CheckBox>().CurrentValue && !(target.IsValidTarget(Player.Instance.GetAutoAttackRange(target)) && EliseSpellManager.SpellCasts[EliseSpellManager.SpiderW].LastCastT + 3000 < Environment.TickCount) &&
                    (EliseSpellManager.SpellIsReady(EliseSpellManager.HumanQ) && EliseSpellManager.HumanQSpell.IsInRange(target)
                    || EliseSpellManager.SpellIsReady(EliseSpellManager.HumanW) && EliseSpellManager.HumanWSpell.IsInRange(target)
                    || EliseSpellManager.SpellIsReady(EliseSpellManager.HumanE) && EliseSpellManager.HumanESpell.IsInRange(target)))
                {
                    EliseSpellManager.RSpell.Cast();
                }
            }
            else
            {
                if (Elise.FarmMenu["jgHumanW"].Cast<CheckBox>().CurrentValue && EliseSpellManager.HumanQSpell.IsReady() && target.IsValidTarget(EliseSpellManager.HumanQSpell.Range))
                {
                    EliseSpellManager.HumanQSpell.Cast(target);
                }
                else if (Elise.FarmMenu["jgHumanW"].Cast<CheckBox>().CurrentValue && EliseSpellManager.HumanWSpell.IsReady() && target.IsValidTarget(Player.Instance.GetAutoAttackRange(target)))
                {
                    EliseSpellManager.HumanWSpell.Cast(target);
                }
                else if (Elise.FarmMenu["jungleR"].Cast<CheckBox>().CurrentValue && (EliseSpellManager.SpellIsReady(EliseSpellManager.SpiderQ) && target.IsValidTarget(EliseSpellManager.SpiderQSpell.Range)
                         || EliseSpellManager.SpellIsReady(EliseSpellManager.SpiderW) && target.IsValidTarget(190 + target.BoundingRadius)))
                {
                    EliseSpellManager.RSpell.Cast();
                }
            }
        }
    }
}
