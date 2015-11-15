using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace VayneBuddy
{
    static class States
    {
        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget((int)_Player.GetAutoAttackRange(), DamageType.Physical);
            var target2 = EntityManager.Heroes.Enemies.FirstOrDefault(a => a.HasWBuff() && Player.Instance.GetAutoAttackRange(target) >= target.Distance(Player.Instance));
            if (target2 != null)
            {
                target = target2;
            }
            var condemnTarget = TargetSelector.GetTarget((int)_Player.GetAutoAttackRange() + 300, DamageType.Physical);
            Orbwalker.ForcedTarget = target;

            if (!target.IsValidTarget() || Orbwalker.IsAutoAttacking) return;

            if (Program.E.IsReady() && target.IsValidTarget(Program.E.Range) && target.IsCondemable() &&
                Program.CondemnMenu["condemnCombo"].Cast<CheckBox>().CurrentValue)
            {
                Program.E.Cast(target);
                if (_Player.Spellbook.GetSpell(SpellSlot.Trinket).IsReady &&
                    _Player.Spellbook.GetSpell(SpellSlot.Trinket).SData.Name.ToLower().Contains("totem"))
                {
                    Core.DelayAction(delegate
                    {
                        if (!Program.CondemnMenu["condemnComboTrinket"].Cast<CheckBox>().CurrentValue) return;
                        var pos = Condemn.GetFirstNonWallPos(_Player.Position.To2D(), target.Position.To2D());
                        if (pos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Grass))
                        {
                            Player.CastSpell(SpellSlot.Trinket,
                                pos.To3D());
                        }
                    }, 200);
                }
            }

            if (Program.CondemnMenu["smartVsCheap"].Cast<CheckBox>().CurrentValue)
            {
                if (Program.E.IsReady() && Program.Q.IsReady() && condemnTarget.IsValidTarget(Program.E.Range + 300))
                {
                    if (condemnTarget.IsCondemable(_Player.Position.Extend(Game.CursorPos, 300)))
                    {
                        Player.CastSpell(SpellSlot.Q, Game.CursorPos);
                        return;
                    }
                    if (condemnTarget.IsCondemable(_Player.Position.Extend(target.Position, 300)))
                    {
                        Player.CastSpell(SpellSlot.Q, condemnTarget.Position);
                        return;
                    }
                }
            }

            if (Program.Q.IsReady() && Player.Instance.Distance(target) > Player.Instance.GetAutoAttackRange(target) && Player.Instance.Distance(target) < Player.Instance.GetAutoAttackRange(target) + 300 && Program.ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue)
            {
                Player.CastSpell(SpellSlot.Q, Game.CursorPos);
                return;
            }

            if (target.Health < Player.Instance.GetSpellDamage(target, SpellSlot.E) + (target.HasWBuff() ? Player.Instance.GetSpellDamage(target, SpellSlot.W, DamageLibrary.SpellStages.Passive) : 0) && Program.ComboMenu["useECombo"].Cast<CheckBox>().CurrentValue)
            {
                Program.E.Cast(target);
            }
        }

        public static void Harass()
        {
            var target = TargetSelector.GetTarget((int)_Player.AttackRange + 500, DamageType.Physical);
            Orbwalker.ForcedTarget = target;
            if (!target.IsValidTarget()) return;

            if (Program.Q.IsReady() && Program.ComboMenu["useQHarass"].Cast<CheckBox>().CurrentValue && Player.Instance.Distance(target) > Player.Instance.GetAutoAttackRange(target) && Player.Instance.Distance(target) < Player.Instance.GetAutoAttackRange(target) + 300)
            {
                Player.CastSpell(SpellSlot.Q, Game.CursorPos);
            }
        }
    }

}
