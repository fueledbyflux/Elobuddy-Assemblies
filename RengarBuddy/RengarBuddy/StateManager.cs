using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace RengarBuddy
{
    internal static class StateManager
    {
        private static AIHeroClient _Player { get { return ObjectManager.Player; } }

        private static float GetCustomRange()
        {
            return new[] { Program.E.IsReady() ? Program.E.Range : 0, Program.W.IsReady() ? Program.W.Range : 0, Player.Instance.GetAutoAttackRange()}.Max();
        }

        public static void Combo()
        {
            if (Player.HasBuff("RengarR") && Program.DisableAntiSkills)
            {
                var item = _Player.InventoryItems.FirstOrDefault(a => a.Id == ItemId.Youmuus_Ghostblade);
                if (Program.ComboMenu["useYomuus"].Cast<CheckBox>().CurrentValue && item != null && item.CanUseItem())
                {
                    item.Cast();
                }
                return;
            }
            var target = TargetSelector.GetTarget(GetCustomRange(), DamageType.Physical);
            switch ((int) _Player.Mana)
            {
                case 5:
                    if (Program.ComboMenu["ferocity"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Program.ComboMenu["modeType"].Cast<CheckBox>().CurrentValue)
                        {
                            var healthPercent = Program.ComboMenu["wHealthPercentSlider"].Cast<Slider>().CurrentValue;
                            if (Program.W.IsReady() && _Player.HealthPercent <= healthPercent && Program.ComboMenu["wHealthPercent"].Cast<CheckBox>().CurrentValue)
                            {
                                Program.W.Cast();
                            }
                            else if (!Orbwalker.IsAutoAttacking && Program.ComboMenu["qInRange"].Cast<CheckBox>().CurrentValue && Program.Q.IsReady() && target.IsValidTarget(_Player.GetAutoAttackRange(target)))
                            {
                                Program.Q.Cast();
                            }
                            else if (Program.E.IsReady() && Program.ComboMenu["eOutOfRange"].Cast<CheckBox>().CurrentValue)
                            {
                                Program.E.Cast(target);
                            }
                        }
                        else
                        {
                            var ferocityMode = Program.ComboMenu["selectedStackedSpell"].Cast<Slider>().CurrentValue;
                            switch (ferocityMode)
                            {
                                case 0:
                                    if (Orbwalker.IsAutoAttacking || target == null || !target.IsValidTarget(_Player.GetAutoAttackRange(target)) || !Program.Q.IsReady()) return;
                                    Program.Q.Cast();
                                    break;
                                case 1:
                                    if (!Program.W.IsReady()) return;
                                    Program.W.Cast();
                                    break;
                                case 2:
                                    if (target == null || !Program.E.IsReady()) return;
                                    Program.E.Cast(target);
                                    break;
                            }
                        }
                    }
                    break;
                default:
                    if (Program.W.IsReady() && target.IsValidTarget(Program.W.Range) && Program.ComboMenu["wCombo"].Cast<CheckBox>().CurrentValue)
                    {
                        Program.W.Cast();
                        return;
                    }
                    if (Program.Q.IsReady() && Program.ComboMenu["qCombo"].Cast<CheckBox>().CurrentValue && !Orbwalker.IsAutoAttacking && target.IsValidTarget(_Player.GetAutoAttackRange(target)))
                    {
                        Program.Q.Cast();
                        return;
                    }
                    if (Program.E.IsReady() && Program.ComboMenu["eCombo"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(Program.E.Range))
                    {
                        Program.E.Cast(target);
                    }
                    break;
            }
        }

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(GetCustomRange(), DamageType.Physical);
            switch ((int)_Player.Mana)
            {
                case 5:
                    if (Program.HarassMenu["save5StacksHarass"].Cast<CheckBox>().CurrentValue)
                        return;
                    var ferocityMode = Program.HarassMenu["selectedStackedSpellHarass"].Cast<Slider>().CurrentValue;
                    switch (ferocityMode)
                    {
                        case 0:
                            if (Orbwalker.IsAutoAttacking || target == null || !target.IsValidTarget(_Player.GetAutoAttackRange(target)) || !Program.Q.IsReady()) return;
                            Program.Q.Cast();
                            break;
                        case 1:
                            if (!Program.W.IsReady()) return;
                            Program.W.Cast();
                            break;
                        case 2:
                            if (target == null || !Program.E.IsReady()) return;
                            Program.E.Cast(target);
                            break;
                    }
                    break;
                default:
                    if (Program.W.IsReady() && target.IsValidTarget(Program.W.Range) && Program.HarassMenu["wHarass"].Cast<CheckBox>().CurrentValue)
                    {
                        Program.W.Cast();
                    }
                    if (Program.Q.IsReady() && Program.HarassMenu["qHarass"].Cast<CheckBox>().CurrentValue && !Orbwalker.IsAutoAttacking && target.IsValidTarget(_Player.GetAutoAttackRange(target)))
                    {
                        Program.Q.Cast();
                    }
                    if (Program.E.IsReady() && Program.HarassMenu["eHarass"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(Program.E.Range))
                    {
                        Program.E.Cast(target);
                    }
                    break;
            }
        }

        public static void LastHit()
        {
            if (Orbwalker.IsAutoAttacking) return;
            Orbwalker.ForcedTarget = null;
            var source = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.Position, GetCustomRange()).OrderBy(a => a.Health).FirstOrDefault();

            if (Program.FarmMenu["saveStacksWC"].Cast<CheckBox>().CurrentValue && (int)_Player.Mana == 5 || source == null)
                return;
            if (Program.FarmMenu["qLastHit"].Cast<CheckBox>().CurrentValue && Damage.Q1(source) > source.Health && source.Distance(_Player) < _Player.GetAutoAttackRange(source))
                {
                    Program.Q.Cast();
                    return;
                }
                if (Damage.W(source) > source.Health && source.Distance(_Player) < Program.W.Range)
                {
                    Program.W.Cast();
                    return;
                }
            if (!Program.FarmMenu["eLastHit"].Cast<CheckBox>().CurrentValue || !(Damage.E(source) > source.Health) ||
                !(source.Distance(_Player) < Program.E.Range)) return;
            Program.E.Cast(source);
        }

        public static void WaveClear()
        {
            if (Orbwalker.IsAutoAttacking) return;
            Orbwalker.ForcedTarget = null;
            var source = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.Position, GetCustomRange()).OrderBy(a => a.Health).FirstOrDefault();

            if (Program.FarmMenu["saveStacksWC"].Cast<CheckBox>().CurrentValue && (int) _Player.Mana == 5 || source == null)
                    return;
                if((int) _Player.Mana == 5) { 
                    var ferocityMode = Program.FarmMenu["selectedStackedSpellWC"].Cast<Slider>().CurrentValue;
                    switch (ferocityMode)
                    {
                        case 0:
                            if (Orbwalker.IsAutoAttacking || !source.IsValidTarget(_Player.GetAutoAttackRange(source)) || !Program.Q.IsReady()) return;
                            Program.Q.Cast();
                            break;
                        case 1:
                            if (!Program.W.IsReady()) return;
                            Program.W.Cast();
                            break;
                        case 2:
                            if (!Program.E.IsReady()) return;
                            Program.E.Cast(source);
                            break;
                    }
                    return;
                }

                if (Program.Q.IsReady() && Program.FarmMenu["qWaveClear"].Cast<CheckBox>().CurrentValue && Damage.Q1(source) > source.Health && source.Distance(_Player) < _Player.GetAutoAttackRange(source))
                {
                    Program.Q.Cast();
                    return;
                }
                if (Program.W.IsReady() && Program.FarmMenu["wWaveClear"].Cast<CheckBox>().CurrentValue && Damage.W(source) > source.Health && source.Distance(_Player) < Program.W.Range)
                {
                    Program.W.Cast();
                    return;
                }
            if (!Program.E.IsReady() || !Program.FarmMenu["eWaveClear"].Cast<CheckBox>().CurrentValue ||
                !(Damage.E(source) > source.Health) || !(source.Distance(_Player) < Program.E.Range)) return;
            Program.E.Cast(source);
        }

        public static void Jungle()
        {
            if (Orbwalker.IsAutoAttacking) return;
            Orbwalker.ForcedTarget = null;

            var source = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, GetCustomRange() + 200).OrderByDescending(a => a.MaxHealth).FirstOrDefault();

            if (Program.JungleMenu["saveStacksJungle"].Cast<CheckBox>().CurrentValue && (int)_Player.Mana == 5 || source == null)
                return;
            if ((int) _Player.Mana == 5)
            {
                var ferocityMode = Program.JungleMenu["selectedStackedSpellJNG"].Cast<Slider>().CurrentValue;
                switch (ferocityMode)
                {
                    case 0:
                        if (Orbwalker.IsAutoAttacking || !source.IsValidTarget(_Player.GetAutoAttackRange(source)) ||
                            !Program.Q.IsReady()) return;
                        Program.Q.Cast();
                        break;
                    case 1:
                        if (!Program.W.IsReady()) return;
                        Program.W.Cast();
                        break;
                    case 2:
                        if (!Program.E.IsReady()) return;
                        Program.E.Cast(source);
                        break;
                }
                return;
            }
            
            if (Program.Q.IsReady() && Program.JungleMenu["qJng"].Cast<CheckBox>().CurrentValue && source.Distance(_Player) < _Player.GetAutoAttackRange(source))
                {
                    Program.Q.Cast();
                    return;
                }
                if (Program.W.IsReady() && Program.JungleMenu["wJng"].Cast<CheckBox>().CurrentValue && source.Distance(_Player) < Program.W.Range)
                {
                    Program.W.Cast();
                    return;
                }
                if (Program.E.IsReady() && Program.JungleMenu["eJng"].Cast<CheckBox>().CurrentValue && source.Distance(_Player) < Program.E.Range)
                {
                    Program.E.Cast(source);
                }
        }
    }
}
