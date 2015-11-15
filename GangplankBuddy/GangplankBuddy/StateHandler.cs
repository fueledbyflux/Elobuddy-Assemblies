using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace GangplankBuddy
{
    internal static class StateHandler
    {
        public static void Combo()
        {
            var target = TargetSelector.GetTarget(SpellManager.E.Range + 175, DamageType.Physical);
            Orbwalker.ForcedTarget = null;

            if (target == null || !target.IsValidTarget()) return;

            var barrel = BarrelManager.KillableBarrelAroundUnit(target);
            if (Program.ComboMenu["useQBarrels"].Cast<CheckBox>().CurrentValue && barrel != null &&
                SpellManager.Q.IsReady())
            {
                SpellManager.Q.Cast(barrel);
            } else if (barrel != null && barrel.Distance(Player.Instance) < Player.Instance.GetAutoAttackRange(barrel))
            {
                Orbwalker.ForcedTarget = barrel;
            }
            else
            {
                var maxChain = Program.ComboMenu["useEMaxChain"].Cast<Slider>().CurrentValue;
                if (Program.ComboMenu["useQBarrels"].Cast<CheckBox>().CurrentValue && SpellManager.Q.IsReady() &&
                    maxChain > 1)
                {
                    foreach (var barrels in BarrelManager.Barrels.Where(a => a.Distance(target) < 350))
                    {
                        var killableBarrel =
                            BarrelManager.Killablebarrels.Where(a => a.Distance(Player.Instance) < SpellManager.Q.Range)
                                .FirstOrDefault(a => a.Distance(barrels) < 700);
                        if (killableBarrel != null)
                        {
                            if (SpellManager.Q.IsReady())
                            {
                                SpellManager.Q.Cast(killableBarrel);
                                return;
                            }
                            if (killableBarrel.Distance(Player.Instance) < Player.Instance.GetAutoAttackRange(killableBarrel))
                            {
                                Orbwalker.ForcedTarget = killableBarrel;
                                return;
                            }
                        }
                        if (maxChain <= 2) continue;
                        {
                            foreach (var killableBarrel2 in BarrelManager.Barrels.Where(a => a.Distance(barrels) < 350).Select(barrels2 => BarrelManager.Killablebarrels.Where(
                                a => a.Distance(Player.Instance) < SpellManager.Q.Range)
                                .FirstOrDefault(a => a.Distance(barrels2) < 700)).Where(killableBarrel2 => killableBarrel2 != null))
                            {
                                if (SpellManager.Q.IsReady())
                                {
                                    SpellManager.Q.Cast(killableBarrel2);
                                    return;
                                }
                                if (
                                    !(killableBarrel2.Distance(Player.Instance) <
                                      Player.Instance.GetAutoAttackRange(killableBarrel2))) continue;
                                Orbwalker.ForcedTarget = killableBarrel2;
                                return;
                            }
                        }
                    }
                }
                if (Program.ComboMenu["useE"].Cast<CheckBox>().CurrentValue &&
                    !BarrelManager.Barrels.Any(a => target.Distance(a) < 350) && SpellManager.E.IsReady())
                {
                    SpellManager.E.Cast(target);
                    return;
                }
            }
            if (Program.ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue &&
                target.IsValidTarget(SpellManager.Q.Range) && SpellManager.Q.IsReady())
            {
                SpellManager.Q.Cast(target);
            }
        }

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(SpellManager.E.Range + 175, DamageType.Physical);
            Orbwalker.ForcedTarget = null;

            if (target == null || !target.IsValidTarget()) return;

            var barrel = BarrelManager.KillableBarrelAroundUnit(target);
            if (Program.HarassMenu["useQBarrelsHarass"].Cast<CheckBox>().CurrentValue && barrel != null &&
                SpellManager.Q.IsReady())
            {
                SpellManager.Q.Cast(barrel);
            }
            else if (barrel != null && barrel.Distance(Player.Instance) < Player.Instance.GetAutoAttackRange(barrel))
            {
                Orbwalker.ForcedTarget = barrel;
            }
            else
            {
                var maxChain = Program.HarassMenu["useEMaxChainHarass"].Cast<Slider>().CurrentValue;
                if (Program.HarassMenu["useQBarrelsHarass"].Cast<CheckBox>().CurrentValue &&
                    maxChain > 1)
                {
                    foreach (var barrels in BarrelManager.Barrels.Where(a => a.Distance(target) < 350))
                    {
                        var killableBarrel =
                            BarrelManager.Killablebarrels.Where(a => a.Distance(Player.Instance) < SpellManager.Q.Range)
                                .FirstOrDefault(a => a.Distance(barrels) < 700);
                        if (killableBarrel != null)
                        {
                            if (SpellManager.Q.IsReady())
                            {
                                SpellManager.Q.Cast(killableBarrel);
                                return;
                            }
                            if (
                                !(killableBarrel.Distance(Player.Instance) <
                                  Player.Instance.GetAutoAttackRange(killableBarrel))) return;
                            Orbwalker.ForcedTarget = killableBarrel;
                            return;
                        }
                        if (maxChain <= 2) continue;
                        {
                            foreach (var killableBarrel2 in BarrelManager.Barrels.Where(a => a.Distance(barrels) < 350).Select(barrels2 => BarrelManager.Killablebarrels.Where(
                                a => a.Distance(Player.Instance) < SpellManager.Q.Range)
                                .FirstOrDefault(a => a.Distance(barrels2) < 700)).Where(killableBarrel2 => killableBarrel2 != null))
                            {
                                if (SpellManager.Q.IsReady())
                                {
                                    SpellManager.Q.Cast(killableBarrel2);
                                    return;
                                }
                                if (
                                    !(killableBarrel2.Distance(Player.Instance) <
                                      Player.Instance.GetAutoAttackRange(killableBarrel2))) continue;
                                Orbwalker.ForcedTarget = killableBarrel2;
                                return;
                            }
                        }
                    }
                }
                if (Program.HarassMenu["useEHarass"].Cast<CheckBox>().CurrentValue &&
                    !BarrelManager.Barrels.Any(a => target.Distance(a) < 350) && SpellManager.E.IsReady())
                {
                    SpellManager.E.Cast(target);
                    return;
                }
            }
            if (Program.HarassMenu["useQHarass"].Cast<CheckBox>().CurrentValue &&
                target.IsValidTarget(SpellManager.Q.Range) && SpellManager.Q.IsReady())
            {
                SpellManager.Q.Cast(target);
            }
        }

        public static void LastHit()
        {
            if (!SpellManager.Q.IsReady() || !Program.FarmingMenu["useQLastHit"].Cast<CheckBox>().CurrentValue) return;

            var minion =
                ObjectManager.Get<Obj_AI_Minion>()
                    .FirstOrDefault(
                        a =>
                            a.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(a) &&
                            !a.BaseSkinName.ToLower().Contains("gang") &&
                            Player.Instance.Distance(a) <= SpellManager.Q.Range && a.IsEnemy &&
                            a.Health <= GpDmg.QDamage(a));
            if (minion == null) return;
            SpellManager.Q.Cast(minion);
        }

        public static void Waveclear()
        {
            if (Program.FarmingMenu["useEQKill"].Cast<CheckBox>().CurrentValue)
            {
                foreach (
                    var killableBarrel in
                        BarrelManager.Killablebarrels.Where(
                            killableBarrel =>
                                killableBarrel.IsValidTarget(SpellManager.Q.Range) &&
                                EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                                    b =>
                                        b.Distance(killableBarrel) < 350 &&
                                        b.Health < GpDmg.EDamage(b, GpDmg.QDamage(b))) >
                                Program.FarmingMenu["useEQKillMin"].Cast<Slider>().CurrentValue))
                {
                    SpellManager.Q.Cast(killableBarrel);
                    return;
                }
            }
            if (Program.FarmingMenu["useEWaveClear"].Cast<CheckBox>().CurrentValue)
            {
                var count = 0;
                Obj_AI_Base target = null;
                foreach (
                    var source in
                        EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                            a => a.Distance(Player.Instance) < SpellManager.E.Range))
                {
                    var count2 = EntityManager.MinionsAndMonsters.EnemyMinions.Count(a => a.Distance(source) < 350);
                    if (count2 <= count) continue;
                    count = count2;
                    target = source;
                }
                if (target != null && count >= Program.FarmingMenu["useEWaveClearMin"].Cast<Slider>().CurrentValue &&
                    !BarrelManager.Killablebarrels.Any(a => a.Distance(target.Position) < 350) &&
                    !BarrelManager.Barrels.Any(a => a.Distance(target.Position) < 350))
                {
                    SpellManager.E.Cast(target.Position);
                    return;
                }
            }
            var minion =
                EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(
                    a =>
                        a.Health < GpDmg.QDamage(a) &&
                        a.IsValidTarget(SpellManager.Q.Range));
            if (Program.FarmingMenu["useQWaveClear"].Cast<CheckBox>().CurrentValue && SpellManager.Q.IsReady() &&
                minion != null)
            {
                SpellManager.Q.Cast(minion);
            }
        }

        public static void Healing()
        {
            if (!SpellManager.W.IsReady() || !Program.HealingMenu["enableHeal"].Cast<CheckBox>().CurrentValue) return;
            if (Player.Instance.HealthPercent <= Program.HealingMenu["healMin"].Cast<Slider>().CurrentValue)
            {
                SpellManager.W.Cast();
                return;
            }
            if (Program.HealingMenu["healStun"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Stun)
                || Program.HealingMenu["healRoot"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Snare))
            {
                SpellManager.W.Cast();
            }
        }
    }
}