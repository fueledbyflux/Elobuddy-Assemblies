using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Menu.Values;

namespace KenchUnbenched
{

    //THIS IS ALL HELLSING'S, FLAME HIM IF IT DOES NOT WORK <3

    public class SaveUnit
    {
        public SaveUnit(AIHeroClient unit)
        {
            Unit = unit;
        }

        public AIHeroClient Unit;

        public readonly Dictionary<float, float> IncDamage = new Dictionary<float, float>();
        public readonly Dictionary<float, float> InstDamage = new Dictionary<float, float>();
        public float IncomingDamage
        {
            get { return IncDamage.Sum(e => e.Value) + InstDamage.Sum(e => e.Value); }
        }

        public bool IsEnabled
        {
            get { return KenchUnbenched.SaveMenu["save" + Unit.ChampionName].Cast<CheckBox>().CurrentValue; }
        }
    }

    internal static class KenchSaver
    {
        private static Spell.Targeted W
        {
            get { return KenchUnbenched.WSpellSwallow; }
        }

        public static void Initialize()
        {
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;

            KenchUnbenched.SaveMenu = KenchUnbenched.Menu.AddSubMenu("Save Settings");
            KenchUnbenched.SaveMenu.AddGroupLabel("Saving Allies Settings");
            KenchUnbenched.SaveMenu.Add("enableSaving", new CheckBox("Saving Enabled"));
            KenchUnbenched.SaveMenu.AddSeparator();

            foreach (var source in EntityManager.Heroes.Allies.Where(a => !a.IsMe))
            {
                KenchUnbenched.SaveMenu.Add("save" + source.ChampionName, new CheckBox("Save " + source.ChampionName));
                UnitsToSave.Add(new SaveUnit(source));
            }
        }

        private static List<SaveUnit> UnitsToSave = new List<SaveUnit>(); 
            

        private static void OnUpdate(EventArgs args)
        {
            if (KenchUnbenched.SaveMenu["enableSaving"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                foreach (var saveUnit in UnitsToSave.Where(a => a.IsEnabled && a.Unit.Distance(Player.Instance) < 300).Where(saveUnit => saveUnit.Unit.HealthPercent <= 10 && saveUnit.Unit.CountEnemiesInRange(800) > 0 ||
                                                                                                                                         saveUnit.IncomingDamage > saveUnit.Unit.Health))
                {
                    Player.CastSpell(SpellSlot.W, saveUnit.Unit);
                }
            }
            foreach (var unit in UnitsToSave)
            {
                // Check spell arrival
                foreach (var entry in unit.IncDamage.Where(entry => entry.Key < Game.Time).ToArray())
                {
                    unit.IncDamage.Remove(entry.Key);
                }

                // Instant damage removal
                foreach (var entry in unit.InstDamage.Where(entry => entry.Key < Game.Time).ToArray())
                {
                    unit.InstDamage.Remove(entry.Key);
                }
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsEnemy) return;
            foreach (var saveUnit in UnitsToSave.Where(a => a.IsEnabled).Where(saveUnit => saveUnit.Unit != null && KenchUnbenched.SaveMenu["enableSaving"].Cast<CheckBox>().CurrentValue))
            {
                // Auto attacks
                if ((!(sender is AIHeroClient) || args.SData.IsAutoAttack()) && args.Target != null &&
                    args.Target.NetworkId == saveUnit.Unit.NetworkId)
                {
                    // Calculate arrival time and damage
                    saveUnit.IncDamage[
                        saveUnit.Unit.ServerPosition.Distance(sender.ServerPosition)/args.SData.MissileSpeed +
                        Game.Time] = sender.GetAutoAttackDamage(saveUnit.Unit);
                }
                // Sender is a hero
                else
                {
                    var attacker = sender as AIHeroClient;
                    if (attacker == null) continue;
                    var slot = attacker.GetSpellSlotFromName(args.SData.Name);

                    if (slot == SpellSlot.Unknown) continue;
                    if (slot == attacker.GetSpellSlotFromName("SummonerDot") && args.Target != null &&
                        args.Target.NetworkId == saveUnit.Unit.NetworkId)
                    {
                        // Ingite damage (dangerous)
                        saveUnit.InstDamage[Game.Time + 2] = attacker.GetSummonerSpellDamage(saveUnit.Unit,
                            DamageLibrary.SummonerSpells.Ignite);
                    }
                    else
                    {
                        switch (slot)
                        {
                            case SpellSlot.Q:
                            case SpellSlot.W:
                            case SpellSlot.E:
                            case SpellSlot.R:

                                if ((args.Target != null && args.Target.NetworkId == saveUnit.Unit.NetworkId) ||
                                    args.End.Distance(saveUnit.Unit.ServerPosition) <
                                    Math.Pow(args.SData.LineWidth, 2))
                                {
                                    // Instant damage to target
                                    saveUnit.InstDamage[Game.Time + 2] = attacker.GetSpellDamage(saveUnit.Unit, slot);
                                }

                                break;
                            case SpellSlot.Unknown:
                                break;
                            case SpellSlot.Summoner1:
                                break;
                            case SpellSlot.Summoner2:
                                break;
                            case SpellSlot.Item1:
                                break;
                            case SpellSlot.Item2:
                                break;
                            case SpellSlot.Item3:
                                break;
                            case SpellSlot.Item4:
                                break;
                            case SpellSlot.Item5:
                                break;
                            case SpellSlot.Item6:
                                break;
                            case SpellSlot.Trinket:
                                break;
                            case SpellSlot.Recall:
                                break;
                            case SpellSlot.OathSworn:
                                break;
                            case SpellSlot.CapturePoint:
                                break;
                            case SpellSlot.Internal:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }
    }
}
