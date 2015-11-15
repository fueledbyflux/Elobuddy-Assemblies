using System;
using System.Drawing;
using System.Linq;
using ActivatorBuddy.Defencives;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;

namespace ActivatorBuddy.Summoner_Spells
{
    internal static class SummonerSpells
    {
        private static Spell.Targeted _ignite;
        private static Spell.Targeted _heal;
        private static Spell.Targeted _exhaust;
        private static Spell.Active _barrier;
        public static Spell.Targeted Smite;
        private static Menu _summonerMenu;
        private static Menu _smiteMenu;
        
        

        public static void Init()
        {
            _summonerMenu = Program.Menu.AddSubMenu("Summoner Spells");
            if (HasSpell("summonerdot"))
            {
                _summonerMenu.AddGroupLabel("Ignite Settings");
                _summonerMenu.Add("useIgnite", new CheckBox("Use Ignite"));
                _summonerMenu.Add("comboOnlyIgnite", new CheckBox("Combo Only"));
                _summonerMenu.Add("drawIngiteRange", new CheckBox("Draw Ignite Range"));
                _summonerMenu.AddSeparator();
                _ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
                Game.OnTick += IgniteEvent;
                Chat.Print("Activator: Ignite Loaded.", Color.LimeGreen);
            }
            if (HasSpell("summonerexhaust"))
            {
                _summonerMenu.AddGroupLabel("Exhaust Settings");
                _summonerMenu.Add("useExhaust", new CheckBox("Use Exhaust"));
                _summonerMenu.Add("comboOnlyExhaust", new CheckBox("Combo Only"));
                _summonerMenu.Add("drawExhaustRange", new CheckBox("Draw Exhaust Range"));
                foreach (var source in ObjectManager.Get<AIHeroClient>().Where(a => a.IsEnemy))
                {
                    _summonerMenu.Add(source.ChampionName + "exhaust",
                        new CheckBox("Exhaust " + source.ChampionName, false));
                }
                _summonerMenu.AddSeparator();
                _exhaust = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerexhaust"), 650);
                Game.OnTick += ExhaustEvent;
                Chat.Print("Activator: Exhaust Loaded.", Color.OrangeRed);
            }
            if (HasSpell("summonerheal"))
            {
                _summonerMenu.AddGroupLabel("Heal Settings");
                _summonerMenu.Add("useHeal", new CheckBox("Use Heal"));
                _summonerMenu.Add("comboOnlyHeal", new CheckBox("Combo Only"));
                _summonerMenu.Add("drawHealRange", new CheckBox("Draw Heal Range"));
                _summonerMenu.AddLabel("Champions");
                foreach (var source in ObjectManager.Get<AIHeroClient>().Where(a => a.IsAlly && !a.IsMe))
                {
                    _summonerMenu.Add(source.ChampionName + "heal", new CheckBox("Heal " + source.ChampionName, false));
                }
                _summonerMenu.AddSeparator();
                _heal = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerheal"), 850);
                Game.OnTick += HealEvent;
                Chat.Print("Activator: Heal Loaded.", Color.Aqua);
            }
            if (HasSpell("summonerbarrier"))
            {
                _summonerMenu.AddGroupLabel("Barrier Settings");
                _summonerMenu.Add("useBarrier", new CheckBox("Use Barrier"));
                _summonerMenu.Add("comboOnlyBarrier", new CheckBox("Combo Only"));
                _summonerMenu.AddSeparator();
                _barrier = new Spell.Active(ObjectManager.Player.GetSpellSlotFromName("summonerbarrier"), int.MaxValue);
                Game.OnTick += BarrierEvent;
                Chat.Print("Activator: Barrier Loaded.", Color.GreenYellow);
            }
            if (HasSpell("smite"))
            {
                _smiteMenu = Program.Menu.AddSubMenu("Smite Settings");
                _smiteMenu.AddGroupLabel("Camps");
                _smiteMenu.AddLabel("Epics");
                _smiteMenu.Add("SRU_Baron", new CheckBox("Bazza (Baron)"));
                _smiteMenu.Add("SRU_Dragon", new CheckBox("Dazza (Dragon)"));
                _smiteMenu.AddLabel("Buffs");
                _smiteMenu.Add("SRU_Blue", new CheckBox("Blue"));
                _smiteMenu.Add("SRU_Red", new CheckBox("Red"));
                _smiteMenu.AddLabel("Small Camps");
                _smiteMenu.Add("SRU_Gromp", new CheckBox("Gromp", false));
                _smiteMenu.Add("SRU_Murkwolf", new CheckBox("Murkwolf", false));
                _smiteMenu.Add("SRU_Krug", new CheckBox("Krug", false));
                _smiteMenu.Add("SRU_Razorbeak", new CheckBox("Razerbeak", false));
                _smiteMenu.Add("Sru_Crab", new CheckBox("Skuttles", false));
                _smiteMenu.AddSeparator();
                _smiteMenu.Add("drawSmiteRange", new CheckBox("Draw Smite Range"));
                _smiteMenu.Add("smiteActive",
                    new KeyBind("Smite Active (toggle)", true, KeyBind.BindTypes.PressToggle, 'H'));
                _smiteMenu.AddSeparator();
                _smiteMenu.Add("useSlowSmite", new CheckBox("KS with Slow Smite"));
                _smiteMenu.Add("comboWithDuelSmite", new CheckBox("Combo With Duel Smite"));

                Smite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonersmite"), 500);
                Game.OnTick += SmiteEvent;
                Chat.Print("Activator: Smite Loaded.", Color.Yellow);
            }
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void BarrierEvent(EventArgs args)
        {
            if (!_barrier.IsReady() || Player.Instance.IsDead) return;
            if (!_summonerMenu["useBarrier"].Cast<CheckBox>().CurrentValue || _summonerMenu["comboOnlyBarrier"].Cast<CheckBox>().CurrentValue &&
                !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                return;
            if (!Player.Instance.InDanger(true) ||
                !(Player.Instance.PredictedHealth() + 95 + (20*Player.Instance.Level) > 0)) return;
            _barrier.Cast();
        }

        private static void SmiteEvent(EventArgs args)
        {
            Smiter.SetSmiteSlot();
            if (!Smite.IsReady() || Player.Instance.IsDead) return;
            if (_smiteMenu["smiteActive"].Cast<KeyBind>().CurrentValue)
            {
                var unit =
                    EntityManager.MinionsAndMonsters.Monsters
                        .Where(
                            a =>
                                Smiter.SmiteableUnits.Contains(a.BaseSkinName) && a.Health < Smiter.GetSmiteDamage() &&
                                _smiteMenu[a.BaseSkinName].Cast<CheckBox>().CurrentValue)
                        .OrderByDescending(a => a.MaxHealth)
                        .FirstOrDefault();

                if (unit != null)
                {
                    Smite.Cast(unit);
                    return;
                }
            }
            if (_smiteMenu["useSlowSmite"].Cast<CheckBox>().CurrentValue &&
                Smite.Handle.Name == "s5_summonersmiteplayerganker")
            {
                foreach (
                    var target in
                        EntityManager.Heroes.Enemies
                            .Where(h => h.IsValidTarget(Smite.Range) && h.Health <= 20 + 8 * Player.Instance.Level))
                {
                    Smite.Cast(target);
                    return;
                }
            }
            if (!_smiteMenu["comboWithDuelSmite"].Cast<CheckBox>().CurrentValue ||
                Smite.Handle.Name != "s5_summonersmiteduel" ||
                !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;
            {
                foreach (
                    var target in
                        EntityManager.Heroes.Enemies
                            .Where(h => h.IsValidTarget(Smite.Range)).OrderByDescending(TargetSelector.GetPriority))
                {
                    Smite.Cast(target);
                    return;
                }
            }
        }

        #region Heal

        private static void HealEvent(EventArgs args)
        {
            if (!_heal.IsReady() || Player.Instance.IsDead) return;
            if (!_summonerMenu["useHeal"].Cast<CheckBox>().CurrentValue || _summonerMenu["comboOnlyHeal"].Cast<CheckBox>().CurrentValue &&
                !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                return;
            foreach (
                var source in
                    from source in
                        ObjectManager.Get<AIHeroClient>().Where(a => a.IsAlly && a.Distance(Player.Instance) < _heal.Range && !a.IsDead)
                    where
                        (source.IsMe || _summonerMenu[source.ChampionName + "heal"].Cast<CheckBox>().CurrentValue) && source.InDanger(true) && source.PredictedHealth() + 75 + (15 * Player.Instance.Level) > 0
                    select source)
            {
                _heal.Cast(source);
                return;
            }
        }

        #endregion

        #region Exhaust

        private static void ExhaustEvent(EventArgs args)
        {
            if (!_exhaust.IsReady() || Player.Instance.IsDead) return;
            if (!_summonerMenu["useExhaust"].Cast<CheckBox>().CurrentValue || _summonerMenu["comboOnlyExhaust"].Cast<CheckBox>().CurrentValue &&
                !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                return;
            foreach (
                var enemy in
                    EntityManager.Heroes.Enemies
                        .Where(a => a.IsValidTarget(_exhaust.Range))
                        .Where(enemy => _summonerMenu[enemy.ChampionName + "exhaust"].Cast<CheckBox>().CurrentValue))
            {
                if (enemy.IsFacing(Player.Instance))
                {
                    if (!(Player.Instance.HealthPercent < 50)) continue;
                    _exhaust.Cast(enemy);
                    return;
                }
                if (!(enemy.HealthPercent < 50)) continue;
                _exhaust.Cast(enemy);
                return;
            }
        }

        #endregion

        #region Ignite

        private static void IgniteEvent(EventArgs args)
        {
            if (!_ignite.IsReady() || Player.Instance.IsDead) return;
            if (!_summonerMenu["useIgnite"].Cast<CheckBox>().CurrentValue || _summonerMenu["comboOnlyIgnite"].Cast<CheckBox>().CurrentValue &&
                !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;
            foreach (
                var source in
                    EntityManager.Heroes.Enemies
                        .Where(
                            a => a.IsValidTarget(_ignite.Range) &&
                                a.Health < 50 + 20 * Player.Instance.Level - (a.HPRegenRate / 5 * 3)))
            {
                _ignite.Cast(source);
                return;
            }
        }

        #endregion

        #region Drawing

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (HasSpell("summonerexhaust") && _summonerMenu["drawExhaustRange"].Cast<CheckBox>().CurrentValue &&
                _exhaust.IsReady())
            {
                Circle.Draw(SharpDX.Color.OrangeRed, _exhaust.Range, Player.Instance.Position);
            }
            if (HasSpell("summonerheal") && _summonerMenu["drawHealRange"].Cast<CheckBox>().CurrentValue &&
                _heal.IsReady())
            {
                Circle.Draw(SharpDX.Color.DeepSkyBlue, _heal.Range, Player.Instance.Position);
            }
            if (HasSpell("smite") && _smiteMenu["drawSmiteRange"].Cast<CheckBox>().CurrentValue && _smiteMenu["smiteActive"].Cast<KeyBind>().CurrentValue)
            {
                Circle.Draw(SharpDX.Color.Yellow, Smite.Range, Player.Instance.Position);
            }
            if (HasSpell("summonerdot") && _summonerMenu["drawIngiteRange"].Cast<CheckBox>().CurrentValue &&
                _ignite.IsReady())
            {
                Circle.Draw(SharpDX.Color.Red, _ignite.Range, Player.Instance.Position);
            }
        }

        #endregion

        #region Util

        public static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }

        #endregion
    }
}