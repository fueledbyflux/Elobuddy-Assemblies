using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;

namespace KenchUnbenched
{
    internal static class StateHandler
    {
        public static void Combo()
        {
            var target = TargetSelector.GetTarget(900, DamageType.Magical);
            if (target == null) return;

            if (KenchUnbenched.ComboMenu["Combo.Q"].Cast<CheckBox>().CurrentValue && KenchUnbenched.QSpell.IsReady() && target.IsValidTarget(800) && (!KenchUnbenched.ComboMenu["Combo.QOnlyStun"].Cast<CheckBox>().CurrentValue || !Player.Instance.IsInAutoAttackRange(target) || target.IsEmpowered()))
            {
                KenchUnbenched.QSpell.Cast(target);
            }

            if (KenchUnbenched.ComboMenu["Combo.W.Enemy"].Cast<CheckBox>().CurrentValue && !KenchCheckManager.IsSwallowed() && target.IsEmpowered())
            {
                KenchUnbenched.WSpellSwallow.Cast(target);
            }

            if (!KenchUnbenched.ComboMenu["Combo.W.Minion"].Cast<CheckBox>().CurrentValue) return;
            if (KenchUnbenched.WSpellSpit.GetPrediction(target).HitChance >= HitChance.Medium)
            {
                foreach (var minion in EntityManager.MinionsAndMonsters.EnemyMinions.Where(minion => minion.Distance(Player.Instance) < KenchUnbenched.WSpellSwallow.Range))
                {
                    KenchUnbenched.WSpellSwallow.Cast(minion);
                    break;
                }
            }

            if (KenchCheckManager.IsSwallowed() && KenchCheckManager.WTarget != null &&
                KenchCheckManager.WTarget.IsMinion)
            {
                KenchUnbenched.WSpellSpit.Cast(target);
            }
        }

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(900, DamageType.Magical);
            if (target == null) return;

            if (KenchUnbenched.HarassMenu["Harass.Q"].Cast<CheckBox>().CurrentValue && KenchUnbenched.QSpell.IsReady() && target.IsValidTarget(800) && (!KenchUnbenched.HarassMenu["Harass.QOnlyStun"].Cast<CheckBox>().CurrentValue || !Player.Instance.IsInAutoAttackRange(target) || target.IsEmpowered()))
            {
                KenchUnbenched.QSpell.Cast(target);
            }

            if (KenchUnbenched.HarassMenu["Harass.W.Enemy"].Cast<CheckBox>().CurrentValue && !KenchCheckManager.IsSwallowed() && target.IsEmpowered())
            {
                KenchUnbenched.WSpellSwallow.Cast(target);
            }

            if (!KenchUnbenched.HarassMenu["Harass.W.Minion"].Cast<CheckBox>().CurrentValue) return;
            if (KenchUnbenched.WSpellSpit.GetPrediction(target).HitChance >= HitChance.Medium)
            {
                foreach (var minion in EntityManager.MinionsAndMonsters.EnemyMinions.Where(minion => minion.Distance(Player.Instance) < KenchUnbenched.WSpellSwallow.Range))
                {
                    KenchUnbenched.WSpellSwallow.Cast(minion);
                    break;
                }
            }

            if (KenchCheckManager.IsSwallowed() && KenchCheckManager.WTarget != null &&
                KenchCheckManager.WTarget.IsMinion)
            {
                KenchUnbenched.WSpellSpit.Cast(target);
            }
        }

        public static void LastHit()
        {
            if (!KenchUnbenched.FarmingMenu["LastHit.Q"].Cast<CheckBox>().CurrentValue ||
                !KenchUnbenched.QSpell.IsReady()) return;
            foreach (var enemies in EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                a => a.Distance(Player.Instance) <= 900 && a.Health <= TahmDamage.QDamage(a)).Where(enemies => KenchUnbenched.QSpell.GetPrediction(enemies).HitChance >= HitChance.Medium))
            {
                KenchUnbenched.QSpell.Cast(enemies);
                break;
            }
        }

        public static void WaveClear()
        {
            if (!KenchUnbenched.FarmingMenu["WaveClear.Q"].Cast<CheckBox>().CurrentValue ||
                !KenchUnbenched.QSpell.IsReady()) return;
            foreach (var enemies in EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                a => a.Distance(Player.Instance) <= 900 && a.Health <= TahmDamage.QDamage(a)).Where(enemies => KenchUnbenched.QSpell.GetPrediction(enemies).HitChance >= HitChance.Medium))
            {
                KenchUnbenched.QSpell.Cast(enemies);
                break;
            }
        }

        public static void JungleClear()
        {
            var target = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderBy(a => a.MaxHealth).FirstOrDefault(a => a.Distance(Player.Instance) <= KenchUnbenched.QSpell.Range);
            if (target == null) return;

            if (KenchUnbenched.FarmingMenu["Jungle.Q"].Cast<CheckBox>().CurrentValue && KenchUnbenched.QSpell.IsReady() && KenchUnbenched.QSpell.GetPrediction(target).HitChance >= HitChance.Medium)
            {
                KenchUnbenched.QSpell.Cast(target);
            }
        }

        public static void KillSteal()
        {
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                if (enemy.IsDead || enemy.Health == 0 || enemy.IsZombie) return;
                if (KenchUnbenched.QSpell.IsReady() && TahmDamage.QDamage(enemy) > enemy.Health &&
                    KenchUnbenched.KillStealMenu["KillSteal.Q"].Cast<CheckBox>().CurrentValue)
                {
                    KenchUnbenched.QSpell.Cast(enemy);
                    return;
                }
                if (KenchUnbenched.WSpellSwallow.IsReady() && TahmDamage.WDamage(enemy) > enemy.Health && enemy.IsEmpowered() &&
                    KenchUnbenched.KillStealMenu["KillSteal.W.Swallow"].Cast<CheckBox>().CurrentValue)
                {
                    KenchUnbenched.QSpell.Cast(enemy);
                    return;
                }

                var pred = KenchUnbenched.WSpellSpit.GetPrediction(enemy);
                if (KenchCheckManager.IsSwallowed() && KenchUnbenched.KillStealMenu["KillSteal.W.Spit"].Cast<CheckBox>().CurrentValue && TahmDamage.WPDamage(enemy) > enemy.Health)
                {
                    KenchUnbenched.WSpellSpit.Cast(enemy);
                    return;
                }
                if (!KenchUnbenched.WSpellSwallow.IsReady() || !(TahmDamage.WPDamage(enemy) > enemy.Health) ||
                    (pred.CollisionObjects.Any() &&
                     (pred.CollisionObjects.Length != 1 || !pred.CollisionObjects[0].IsMinion ||
                      !(pred.CollisionObjects[0].Distance(Player.Instance) <= 250))) || !enemy.IsEmpowered() ||
                    !KenchUnbenched.KillStealMenu["KillSteal.W.Spit"].Cast<CheckBox>().CurrentValue) continue;
                if (pred.CollisionObjects.Length == 1 && pred.CollisionObjects[0].IsMinion)
                {
                    KenchUnbenched.WSpellSwallow.Cast(pred.CollisionObjects[0]);
                    return;
                }
                if (pred.CollisionObjects.Any()) continue;
                var unit =
                    EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(
                        a => a.Distance(Player.Instance) <= 250);
                if (unit != null)
                    KenchUnbenched.WSpellSwallow.Cast(unit);
            }
        }
    }
}
