using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace JinxBuddy
{
    internal static class Program
    {
        public static Spell.Active Q = new Spell.Active(SpellSlot.Q);

        public static Spell.Skillshot W = new Spell.Skillshot(SpellSlot.W, 1450, SkillShotType.Linear, 600, 3300, 100)
        {
            MinimumHitChance = HitChance.High, AllowedCollisionCount = 0
        };

        public static Spell.Skillshot E = new Spell.Skillshot(SpellSlot.E, 900, SkillShotType.Circular, 1200, 1750, 1);
        public static Spell.Skillshot R = new Spell.Skillshot(SpellSlot.R, 3000, SkillShotType.Linear, 600, 1700, 140);

        private static Menu Menu;
        private static Menu ComboMenu;
        private static Menu HarassMenu;
        private static Menu FarmMenu;
        public static Menu MiscMenu;
        private static Menu DrawMenu;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Jinx) return;

            Menu = MainMenu.AddMenu("Jinx Buddy", "jinxBuddy");
            Menu.AddGroupLabel("Jinx Buddy");
            Menu.AddLabel("made by the flauxbauss");
            Menu.AddSeparator();

            ComboMenu = Menu.AddSubMenu("Combo", "ComboJinx");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("useQCombo", new CheckBox("Use Q"));
            ComboMenu.Add("useQSplash", new CheckBox("Use Q Splash"));
            ComboMenu.Add("useWCombo", new CheckBox("Use W"));
            ComboMenu.Add("useECombo", new CheckBox("Use E"));
            ComboMenu.Add("useRCombo", new CheckBox("Use R"));

            HarassMenu = Menu.AddSubMenu("Harass", "HarassJinx");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("useQHarass", new CheckBox("Use Q"));
            HarassMenu.Add("useWHarass", new CheckBox("Use W"));

            FarmMenu = Menu.AddSubMenu("Farm", "FarmJinx");
            FarmMenu.AddGroupLabel("Farm Settings");
            FarmMenu.AddLabel("WaveClear");
            FarmMenu.Add("useQFarm", new CheckBox("Use Q"));
            FarmMenu.Add("disableRocketsWC", new CheckBox("Disable Rockets (only if use q off)", false));
            FarmMenu.AddLabel("Last Hit");
            FarmMenu.Add("disableRocketsLH", new CheckBox("Disable Rockets"));

            MiscMenu = Menu.AddSubMenu("Misc", "MiscMenuJinx");
            MiscMenu.AddGroupLabel("Misc Settings");
            MiscMenu.Add("gapcloser", new CheckBox("Gapcloser E"));
            MiscMenu.Add("interruptor", new CheckBox("Interruptor E"));
            MiscMenu.Add("CCE", new CheckBox("On CC'd E"));

            DrawMenu = Menu.AddSubMenu("Drawing Settings");
            DrawMenu.Add("drawRange", new CheckBox("Draw Other AA Range"));
            DrawMenu.Add("drawW", new CheckBox("Draw W"));
            DrawMenu.Add("drawE", new CheckBox("Draw E"));

            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Events.Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["drawRange"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Color.HotPink, !Events.FishBonesActive ? Events.FishBonesBonus + Events.MinigunRange() - Player.Instance.BoundingRadius/2 : Events.MinigunRange() - Player.Instance.BoundingRadius/2, Player.Instance.Position);
            }
            if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(W.IsReady() ? Color.HotPink : Color.DarkRed, W.Range, Player.Instance.Position);
            }
            if (DrawMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(W.IsReady() ? Color.HotPink : Color.DarkRed, W.Range, Player.Instance.Position);
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs e)
        {
            if (MiscMenu["interruptor"].Cast<CheckBox>().CurrentValue && sender.IsEnemy &&
                e.DangerLevel == DangerLevel.High && sender.IsValidTarget(900))
            {
                E.Cast(sender);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) Harass();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) WaveClear();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)) LastHit();

            if (!MiscMenu["CCE"].Cast<CheckBox>().CurrentValue) return;
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(enemy => enemy.Distance(Player.Instance) < E.Range &&
                                                                              (enemy.HasBuffOfType(BuffType.Stun)
                                                                               || enemy.HasBuffOfType(BuffType.Snare)
                                                                               || enemy.HasBuffOfType(BuffType.Suppression))))
            {
                E.Cast(enemy);
            }
        }

        private static void LastHit()
        {
            if (FarmMenu["disableRocketsLH"].Cast<CheckBox>().CurrentValue && Events.FishBonesActive)
            {
                Q.Cast();
            }
        }

        private static void WaveClear()
        {
            if (Orbwalker.IsAutoAttacking) return;
            if (FarmMenu["useQFarm"].Cast<CheckBox>().CurrentValue)
            {
                var unit =
                    EntityManager.MinionsAndMonsters.GetLaneMinions()
                        .Where(
                            a =>
                                a.IsValidTarget(Events.MinigunRange(a) + Events.FishBonesBonus) &&
                                a.Health < Player.Instance.GetAutoAttackDamage(a)*1.1)
                        .FirstOrDefault(minion => EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                            a => a.Distance(minion) < 150 && a.Health < Player.Instance.GetAutoAttackDamage(a)*1.1) > 1);

                if (unit != null)
                {
                    if (!Events.FishBonesActive)
                    {
                        Q.Cast();
                    }
                    Orbwalker.ForcedTarget = unit;
                    return;
                }

                if (Events.FishBonesActive)
                {
                    Q.Cast();
                }
            }
            else if (FarmMenu["disableRocketsLH"].Cast<CheckBox>().CurrentValue && Events.FishBonesActive)
            {
                Q.Cast();
            }
        }

        private static void Harass()
        {
            var targetW = TargetSelector.SelectedTarget != null &&
                         TargetSelector.SelectedTarget.Distance(Player.Instance) < W.Range
                ? TargetSelector.SelectedTarget
                : TargetSelector.GetTarget(W.Range, DamageType.Physical);

            var target = TargetSelector.SelectedTarget != null &&
                         TargetSelector.SelectedTarget.Distance(Player.Instance) < (!Events.FishBonesActive ? Player.Instance.GetAutoAttackRange() + Events.FishBonesBonus : Player.Instance.GetAutoAttackRange()) + 300
                ? TargetSelector.SelectedTarget
                : TargetSelector.GetTarget((!Events.FishBonesActive ? Player.Instance.GetAutoAttackRange() + Events.FishBonesBonus : Player.Instance.GetAutoAttackRange()) + 300, DamageType.Physical);

            Orbwalker.ForcedTarget = null;

            if (Orbwalker.IsAutoAttacking) return;

            if (targetW != null)
            {
                // W out of range
                if (HarassMenu["useWHarass"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                    target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(targetW) &&
                    targetW.IsValidTarget(W.Range))
                {
                    W.Cast(targetW);
                }
            }

            if (target == null) return;
            if (!HarassMenu["useQHarass"].Cast<CheckBox>().CurrentValue) return;
            // Aoe Logic
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(
                a => a.IsValidTarget(Events.MinigunRange(a) + Events.FishBonesBonus))
                .OrderBy(TargetSelector.GetPriority).Where(enemy => enemy.CountEnemiesInRange(150) > 1 && (enemy.NetworkId == target.NetworkId || enemy.Distance(target) < 150)))
            {
                if (!Events.FishBonesActive)
                {
                    Q.Cast();
                }
                Orbwalker.ForcedTarget = enemy;
                return;
            }

            // Regular Q Logic
            if (Events.FishBonesActive)
            {
                if (target.Distance(Player.Instance) <= Player.Instance.GetAutoAttackRange(target) - Events.FishBonesBonus)
                {
                    Q.Cast();
                }
            }
            else
            {
                if (target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(target))
                {
                    Q.Cast();
                }
            }
        }

        private static void Combo()
        {

            var targetW = TargetSelector.SelectedTarget != null &&
                         TargetSelector.SelectedTarget.Distance(Player.Instance) < W.Range
                ? TargetSelector.SelectedTarget
                : TargetSelector.GetTarget(W.Range, DamageType.Physical);


            var target = TargetSelector.SelectedTarget != null &&
                         TargetSelector.SelectedTarget.Distance(Player.Instance) < (!Events.FishBonesActive ? Player.Instance.GetAutoAttackRange() + Events.FishBonesBonus : Player.Instance.GetAutoAttackRange()) + 300
                ? TargetSelector.SelectedTarget
                : TargetSelector.GetTarget((!Events.FishBonesActive ? Player.Instance.GetAutoAttackRange() + Events.FishBonesBonus : Player.Instance.GetAutoAttackRange()) + 300, DamageType.Physical);
            var rtarget = TargetSelector.GetTarget(3000, DamageType.Physical);

            Orbwalker.ForcedTarget = null;

            if (Orbwalker.IsAutoAttacking) return;

            // E on immobile

            if (target != null && ComboMenu["useECombo"].Cast<CheckBox>().CurrentValue && (target.HasBuffOfType(BuffType.Snare) ||  target.HasBuffOfType(BuffType.Stun)))
            {
                E.Cast(target);
            }

            if (rtarget != null)
            {
                // W/R KS
                var wPred = W.GetPrediction(rtarget);

                if (ComboMenu["useWCombo"].Cast<CheckBox>().CurrentValue &&
                    wPred.HitChance >= HitChance.Medium && W.IsReady() && rtarget.IsValidTarget(W.Range) &&
                    Damages.WDamage(target) >= rtarget.Health)
                {
                    W.Cast(rtarget);
                }
                else if (ComboMenu["useRCombo"].Cast<CheckBox>().CurrentValue && rtarget != null &&
                         rtarget.Distance(Player.Instance) > Events.MinigunRange(target) + Events.FishBonesBonus &&
                         rtarget.IsKillableByR())
                {
                    R.Cast(rtarget);
                }
            }

            // W out of range
            if (targetW != null && ComboMenu["useWCombo"].Cast<CheckBox>().CurrentValue && W.IsReady() && targetW.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(targetW) &&
                targetW.IsValidTarget(W.Range))
            {
                W.Cast(targetW);
            }

            if (target != null && ComboMenu["useQSplash"].Cast<CheckBox>().CurrentValue)
            {
                // Aoe Logic
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(
                    a => a.IsValidTarget(Events.MinigunRange(a) + Events.FishBonesBonus))
                    .OrderBy(TargetSelector.GetPriority).Where(enemy => enemy.CountEnemiesInRange(150) > 1 && (enemy.NetworkId == target.NetworkId || enemy.Distance(target) < 150)))
                {
                    if (!Events.FishBonesActive)
                    {
                        Q.Cast();
                    }
                    Orbwalker.ForcedTarget = enemy;
                    return;
                }
            }

            // Regular Q Logic
            if (target != null && ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue && Events.FishBonesActive)
            {
                if (target.Distance(Player.Instance) <= Player.Instance.GetAutoAttackRange(target) - Events.FishBonesBonus)
                {
                    Q.Cast();
                }
            }
            else if(target != null && ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue)
            {
                if (target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(target))
                {
                    Q.Cast();
                }
            }
        }
    }
}