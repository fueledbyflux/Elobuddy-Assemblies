using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;

namespace YasuoBuddy
{
    static class StateManager
    {
        private static Obj_AI_Base ForcedMinion;

        public static void Combo()
        {
            var target = TargetSelector.SeletedEnabled && Yasuo.ComboMenu["combo.leftclickRape"].Cast<CheckBox>().CurrentValue && TargetSelector.SelectedTarget != null && TargetSelector.SelectedTarget.IsValidTarget(1375)
                    ? TargetSelector.SelectedTarget
                    : TargetSelector.GetTarget(SpellManager.Q.Range + 475 + 100, DamageType.Physical);
            if (target == null) return;

            if (SpellManager.R.IsReady() && SpellManager.GetLowestKnockupTime() <= 250 + Game.Ping &&
                Yasuo.ComboMenu["combo.R"].Cast<CheckBox>().CurrentValue &&
                (Yasuo.ComboMenu["combo.RTarget"].Cast<CheckBox>().CurrentValue && target.IsKnockedUp() && TargetSelector.SelectedTarget == target ||
                Yasuo.ComboMenu["combo.RKillable"].Cast<CheckBox>().CurrentValue && target.Health <= DamageHandler.RDamage(target) && target.Health > DamageHandler.QDamage(target) ||
                SpellManager.GetKnockedUpTargets() >= Yasuo.ComboMenu["combo.MinTargetsR"].Cast<Slider>().CurrentValue))
            {
                SpellManager.R.Cast();
            }

            if (target.IsValidTarget(SpellManager.Q.Range) && Yasuo.ComboMenu["combo.Q"].Cast<CheckBox>().CurrentValue)
            {
                if (Player.Instance.IsDashing())
                {
                    var pos = DashingManager.GetPlayerPosition(300);
                    if (SpellManager.Q.IsReady() && (target.Distance(pos) < 400))
                    {
                        Player.CastSpell(SpellSlot.Q);
                    }
                }
                else if (SpellManager.Q.IsReady())
                {
                    SpellManager.Q.Cast(target);
                    return;
                }
            }

            if (Yasuo.ComboMenu["combo.E"].Cast<CheckBox>().CurrentValue && target.GetDashPos().Distance(target) < 400 && target.CanDash() && target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(target))
            {
                SpellManager.E.Cast(target);
            }

            if (Yasuo.ComboMenu["combo.E"].Cast<CheckBox>().CurrentValue && target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(target) && !Player.Instance.IsDashing())
            {
                foreach (var unit in EntityManager.MinionsAndMonsters.CombinedAttackable.Where(unit => unit.GetDashPos().Distance(target) < Player.Instance.Distance(target) && (!unit.GetDashPos().IsUnderTower() || TargetSelector.SelectedTarget == target)))
                {
                    SpellManager.E.Cast(unit);
                    return;
                }
                foreach (var unit in EntityManager.Heroes.Enemies.Where(unit => unit.GetDashPos().Distance(target) < Player.Instance.Distance(target) && (!unit.GetDashPos().IsUnderTower() || TargetSelector.SelectedTarget == target)))
                {
                    SpellManager.E.Cast(unit);
                    return;
                }
            }
            if (Player.Instance.HasWhirlwind()) return;

            if (Yasuo.ComboMenu["combo.stack"].Cast<CheckBox>().CurrentValue) SpellManager.StackQ();
        }

        public static void Harass()
        {
            var target = TargetSelector.SeletedEnabled && TargetSelector.SelectedTarget != null && TargetSelector.SelectedTarget.IsValidTarget(1375)
                    ? TargetSelector.SelectedTarget
                    : TargetSelector.GetTarget(SpellManager.Q.Range + 475 + 100, DamageType.Physical);
            if (target == null) return;
            if (target.IsValidTarget(SpellManager.Q.Range) && Yasuo.HarassMenu["harass.Q"].Cast<CheckBox>().CurrentValue)
            {
                if (Player.Instance.IsDashing())
                {
                    var pos = DashingManager.GetPlayerPosition(300);
                    if (SpellManager.Q.IsReady() && (target.Distance(pos) < 400))
                    {
                        Player.CastSpell(SpellSlot.Q);
                    }
                }
                else if (SpellManager.Q.IsReady())
                {
                    SpellManager.Q.Cast(target);
                    return;
                }
            }

            var unit = target.GetClosestEUnit();
            if (Yasuo.HarassMenu["harass.E"].Cast<CheckBox>().CurrentValue && unit != null && unit.GetDashPos().Distance(target) < Player.Instance.Distance(target) && (!unit.GetDashPos().IsUnderTower() || TargetSelector.SelectedTarget == target))
            {
                SpellManager.E.Cast(unit);
            }
            if (Player.Instance.HasWhirlwind()) return;
            if (Yasuo.HarassMenu["harass.stack"].Cast<CheckBox>().CurrentValue) SpellManager.StackQ();
        }

        public static void Flee()
        {
            var unit = DashingManager.GetClosestEUnit(Game.CursorPos);
            if (Yasuo.FleeMenu["Flee.E"].Cast<CheckBox>().CurrentValue && unit != null && unit.GetDashPos().Distance(Game.CursorPos) < Player.Instance.Distance(Game.CursorPos))
            {
                SpellManager.E.Cast(unit);
                return;
            }
            if (Yasuo.FleeMenu["Flee.stack"].Cast<CheckBox>().CurrentValue && Player.Instance.HasWhirlwind() && !Player.Instance.IsDashing())
            {
                var target =
                    EntityManager.Heroes.Enemies.Where(a => a.IsValidTarget(SpellManager.Q.Range) && a.Health > 0 && !a.IsDead)
                        .OrderBy(a => a.Distance(Player.Instance))
                        .FirstOrDefault();
                if (target == null) return;
                SpellManager.Q.Cast(target);
                return;
            }
            if (Yasuo.FleeMenu["Flee.stack"].Cast<CheckBox>().CurrentValue) SpellManager.StackQ();
        }

        public static void WaveClear()
        {
            ForcedMinion = null;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) < SpellManager.Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (minion == null) return;
            if (Yasuo.FarmMenu["WC.Q"].Cast<CheckBox>().CurrentValue && Player.Instance.IsDashing())
            {
                var pos = DashingManager.GetPlayerPosition(300);
                if (SpellManager.Q.IsReady() && (minion.Distance(pos) < 475 &&
                    DamageHandler.QDamage(minion) > minion.Health) || EntityManager.MinionsAndMonsters.GetLaneMinions().Count(a => a.Distance(pos) < 475) > 1)
                {
                    Player.CastSpell(SpellSlot.Q);
                }
            }
            if (Yasuo.FarmMenu["WC.E"].Cast<CheckBox>().CurrentValue && SpellManager.E.IsReady() && (!minion.GetDashPos().IsUnderTower() || Yasuo.FarmMenu["WC.EUnderTower"].Cast<CheckBox>().CurrentValue))
            {
                if (DamageHandler.EDamage(minion) > minion.Health)
                {
                    SpellManager.E.Cast(minion);
                    return;
                }
                if (DamageHandler.EDamage(minion) + Player.Instance.GetAutoAttackDamage(minion) > minion.Health)
                {
                    Orbwalker.ForcedTarget = minion;
                    ForcedMinion = minion;
                }
            }
            if (Yasuo.FarmMenu["WC.Q"].Cast<CheckBox>().CurrentValue && SpellManager.Q.IsReady() && DamageHandler.QDamage(minion) > minion.Health)
            {
                SpellManager.Q.Cast(minion);
                return;
            }
            if (!Yasuo.FarmMenu["WC.Q"].Cast<CheckBox>().CurrentValue ||
                !Yasuo.FarmMenu["WC.E"].Cast<CheckBox>().CurrentValue || !SpellManager.E.IsReady() ||
                !SpellManager.Q.IsReady() ||
                !(DamageHandler.EDamage(minion) + DamageHandler.QDamage(minion) > minion.Health) ||
                minion.GetDashPos().IsUnderTower()) return;
            SpellManager.Q.Cast(minion);
        }

        public static void LastHit()
        {
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) < 475).OrderBy(a => a.Health).FirstOrDefault();
            if (minion == null) return;
            if (Yasuo.FarmMenu["LH.E"].Cast<CheckBox>().CurrentValue && SpellManager.E.IsReady() && DamageHandler.EDamage(minion) > minion.Health && (!minion.GetDashPos().IsUnderTower() || Yasuo.FarmMenu["LH.EUnderTower"].Cast<CheckBox>().CurrentValue))
            {
                SpellManager.E.Cast(minion);
                return;
            }
            if (Yasuo.FarmMenu["LH.Q"].Cast<CheckBox>().CurrentValue && SpellManager.Q.IsReady() && DamageHandler.QDamage(minion) > minion.Health)
            {
                SpellManager.Q.Cast(minion);
            }
        }

        public static void AutoQ()
        {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(a => a.Health > 0 && !a.IsDead && !a.IsZombie))
            {
                if (Player.Instance.HasWhirlwind() && !Yasuo.MiscSettings["Auto.Q3"].Cast<CheckBox>().CurrentValue)
                    return;
                if (SpellManager.Q.IsReady() && SpellManager.Q.IsInRange(enemy))
                {
                    SpellManager.Q.Cast(enemy);
                }
            }
        }

        public static void KillSteal()
        {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(a => a.Health > 0 && !a.IsDead && !a.IsZombie))
            {
                if (Yasuo.MiscSettings["KS.Q"].Cast<CheckBox>().CurrentValue && SpellManager.Q.IsReady() && SpellManager.Q.IsInRange(enemy) && DamageHandler.QDamage(enemy) > enemy.Health)
                {
                    SpellManager.Q.Cast(enemy);
                }
                if (Yasuo.MiscSettings["KS.E"].Cast<CheckBox>().CurrentValue && SpellManager.E.IsReady() && SpellManager.E.IsInRange(enemy) && enemy.CanDash() && DamageHandler.EDamage(enemy) > enemy.Health)
                {
                    SpellManager.E.Cast(enemy);
                }
            }
        }

        public static void Jungle()
        {
            var minion = EntityManager.MinionsAndMonsters.GetJungleMonsters().Where(a => a.Distance(Player.Instance) < SpellManager.Q.Range).OrderByDescending(a => a.Health).FirstOrDefault();
            if (minion == null) return;
            if (Yasuo.FarmMenu["JNG.E"].Cast<CheckBox>().CurrentValue && SpellManager.E.IsReady() && DamageHandler.EDamage(minion) > minion.Health && !minion.GetDashPos().IsUnderTower())
            {
                SpellManager.E.Cast(minion);
                return;
            }
            if (Yasuo.FarmMenu["JNG.Q"].Cast<CheckBox>().CurrentValue && SpellManager.Q.IsReady())
            {
                SpellManager.Q.Cast(minion);
            }
        }
    }
}
