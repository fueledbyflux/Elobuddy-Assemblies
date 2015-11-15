using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace LeeSinBuddy
{
    internal static class InsecManager
    {
        private static Obj_AI_Base AllyTarget;
        private static Vector3 InsecPos;
        private static bool InsecActive;
        private static bool WtfSecActive;
        private static long LastUpdate;
        private static Menu InsecMenu;
        private static bool ShouldFlash;

        private static AIHeroClient InsecTarget { get; set; }

        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static void Init()
        {
            InsecMenu = Program.menu.AddSubMenu("Insec Settings");
            InsecMenu.AddGroupLabel("Insec Settings");
            InsecMenu.AddLabeledSlider("insecPositionMode", "Insec Position Mode",
                new[] {"Ally Position", "Game Cursor", "Selected Unit"});
            InsecMenu.AddLabeledSlider("insecTargetMode", "Insec Targeting Mode", new[] {"Click", "TargetSelector"});
            InsecMenu.Add("useFlash", new CheckBox("Use Flash in Insec?", false));
            InsecMenu.Add("checkAllUnits", new CheckBox("Check Other Units for Insec"));
            InsecMenu.Add("insecDistance", new Slider("Insec Distance", 200, 100, 350));
            var insec = InsecMenu.Add("insecActive",
                new KeyBind("Insec Active", false, KeyBind.BindTypes.HoldActive, 'T'));
            var wtfsec = InsecMenu.Add("wtfsecActive",
                new KeyBind("WTFSec Active", false, KeyBind.BindTypes.HoldActive, 'Y'));

            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
            

            Game.OnTick += delegate
            {
                if (!InsecActive || LastUpdate + 200 <= Environment.TickCount)
                {
                    InsecPos = new Vector3();
                }

                InsecActive = false;
                WtfSecActive = false;

                if (insec.CurrentValue)
                {
                    if(Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) Chat.Print("LeeSinBuddy - Flee Is Active! Change the Insec or Flee Keybind or it will not go well.", Color.BlueViolet);
                    InsecActive = true;
                    Insec();
                }
                else if (wtfsec.CurrentValue)
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) Chat.Print("LeeSinBuddy - Flee Is Active! Change the WTFSec or Flee Keybind or it will not go well.", Color.BlueViolet);
                    InsecActive = true;
                    WtfSecActive = true;
                    WtfSec();
                }
            };
        }

        private static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            if (InsecTarget.IsValidTarget())
            {
                Circle.Draw(SharpDX.Color.Red, InsecTarget.BoundingRadius + 100, InsecTarget.Position);
            }
            if (AllyTarget.IsValidTarget())
            {
                Circle.Draw(SharpDX.Color.BlueViolet, AllyTarget.BoundingRadius + 100, AllyTarget.Position);
            }
            if (!InsecPos.IsValid() || !InsecActive || GetTargetForInsec() == null) return;
            var p1 = Drawing.WorldToScreen(WtfSecActive ? Game.CursorPos : GetBestInsecPos());
            Circle.Draw(SharpDX.Color.DodgerBlue, 100, InsecPos);
            Drawing.DrawLine(p1, Drawing.WorldToScreen(InsecPos), 3, Color.CornflowerBlue);
        }

        private static void WtfSec()
        {
            var target = InsecTarget;

            Orbwalker.OrbwalkTo(target.Position);

            if (target == null || !target.IsValidTarget())
                return;
            var allyPos = Game.CursorPos;
            if (InsecPos == new Vector3())
            {
                var insecPos = allyPos.Extend(target.Position, target.Distance(allyPos) + InsecMenu["insecDistance"].Cast<Slider>().CurrentValue);
                InsecPos = insecPos.To3D();
                LastUpdate = Environment.TickCount;
            }
            var jumpPos = InsecTarget.Position.Extend(_Player.Position, 130).To3D();
            if (InsecTarget.HasBuffOfType(BuffType.Knockback) && InsecTarget.HasQBuff() && Program.Q.IsReady() &&
                Program.Q.Instance().Name == Program.Spells["Q2"])
            {
                Core.DelayAction(delegate
                {
                    Program.Q2.Cast();
                }
            , 600);
            }
            if (!Program.R.IsReady())
                return;
            if (_Player.Distance(InsecTarget) < Program.R.Range && _Player.Distance(InsecPos) < 400)
            {
                Program.R.Cast(InsecTarget);
                Core.DelayAction(delegate
                {
                    var spell = _Player.Spellbook.Spells.FirstOrDefault(a => a.Name.ToLower().Contains("summonerflash"));
                    if (spell == null || !spell.IsReady) return;
                    _Player.Spellbook.CastSpell(spell.Slot, InsecPos);
                }, 200);
            }
            else if (Program.Q.Instance().Name == Program.Spells["Q1"] && Program.Q.IsReady())
            {
                SpellClass.SmiteQCast(InsecTarget);
                Program.LastSpellTime = Environment.TickCount;
            }
            else if (jumpPos.Distance(_Player.Position) < 600)
            {
                WardJumper.WardJump(jumpPos, false, true);
            }
        }

        private static void Insec()
        {
            var target = InsecTarget;

            Orbwalker.OrbwalkTo(InsecMenu["insecPositionMode"].Cast<Slider>().CurrentValue == 1 && target != null || GetBestInsecPos() == Game.CursorPos && target != null ? target.Position : Game.CursorPos);


            if (target == null || !target.IsValidTarget())
                return;
            var allyPos = GetBestInsecPos();
            if (InsecPos == new Vector3())
            {
                var insecPos = allyPos.Extend(target.Position, target.Distance(allyPos) + InsecMenu["insecDistance"].Cast<Slider>().CurrentValue).To3D();
                InsecPos = insecPos;
                LastUpdate = Environment.TickCount;
            }
            if (!Program.R.IsReady())
            {
                StateManager.Combo();
                return;
            }

            if (_Player.Distance(InsecPos) < 200)
            {
                Program.R.Cast(target);
                return;
            }
            if (_Player.Distance(InsecPos) < 600)
            {
                if (WardJumper.GetWardSlot() == null && InsecPos.Distance(_Player.Position) < 400 &&
                    WardJumper.LastWard + 1000 < Environment.TickCount)
                {
                    var spell =
                        _Player.Spellbook.Spells.FirstOrDefault(a => a.Name.ToLower().Contains("summonerflash"));
                    if (InsecMenu["useFlash"].Cast<CheckBox>().CurrentValue && spell != null && spell.IsReady)
                    {
                        _Player.Spellbook.CastSpell(spell.Slot, InsecPos);
                        return;
                    }
                }
                WardJumper.WardJump(InsecPos, false, true);
            }
            if (Program.Q.Instance().Name == Program.Spells["Q2"] && Extended.BuffedEnemy != null &&
                (Extended.BuffedEnemy.Distance(_Player) < 1400 && Program.Q.IsReady() &&
                 (target.HasQBuff() || Extended.BuffedEnemy.Distance(InsecPos) < 600)))
            {
                Program.Q2.Cast();
            }
            if (Program.Q.Instance().Name != Program.Spells["Q1"] || !Program.Q.IsReady() ||
                !(target.Distance(_Player) < Program.Q.Range)) return;
            {
                if (SpellClass.SmiteQCast(target) || !InsecMenu["checkAllUnits"].Cast<CheckBox>().CurrentValue) return;
                foreach (var pred in EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                    a => a.Distance(_Player) < Program.Q.Range && a.Distance(InsecPos) < 550).Select(unit => Program.Q.GetPrediction(unit)).Where(pred => pred.HitChance <= HitChance.Medium))
                {
                    Program.Q.Cast(pred.CastPosition);
                    break;
                }
            }
        }

        private static Vector3 InterceptionPoint(List<Obj_AI_Base> heroes)
        {
            var result = new Vector3();
            result = heroes.Aggregate(result, (current, hero) => current + hero.Position);
            result.X /= heroes.Count;
            result.Y /= heroes.Count;
            return result;
        }

        private static AIHeroClient GetTargetForInsec()
        {
            switch (InsecMenu["insecTargetMode"].Cast<Slider>().CurrentValue)
            {
                case 0:
                    return InsecTarget;
                default:
                    return TargetSelector.GetTarget(1400, DamageType.Physical);
            }
        }

        private static Vector3 GetBestInsecPos()
        {
            switch (InsecMenu["insecPositionMode"].Cast<Slider>().CurrentValue)
            {
                case 0:
                    var b =
                        ObjectManager.Get<Obj_AI_Base>()
                            .Where(
                                a =>
                                    (a is AIHeroClient || a is Obj_AI_Turret) && a.IsAlly &&
                                    a.Distance(InsecTarget.Position) < 2000 && !a.IsMe && a.Health > 0).ToList();

                    return b.Any() ? InterceptionPoint(b.ToList()) : Game.CursorPos;
                case 1:
                    return Game.CursorPos;
                case 2:
                    return AllyTarget.Position;
            }
            return new Vector3();
        }

        //Getting insec Target with Clicks
        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != 0x202) return;
            var enemyT =
            EntityManager.Heroes.Enemies
                .Where(
                    a =>
                        a.IsValid && a.Health > 0 && (a.IsEnemy) && a.Distance(Game.CursorPos) < 200)
                .ToList()
                .OrderBy(a => a.Distance(Game.CursorPos))
                .FirstOrDefault();

            if (enemyT != null)
            {
                InsecTarget = enemyT;
                return;
            }

            var allyT =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(
                        a =>
                            a.IsValid && a.Health > 0 && ( a.IsAlly) && a.Distance(Game.CursorPos) < 200 &&
                            (a is AIHeroClient || a is Obj_AI_Minion || a is Obj_AI_Turret) && !a.IsMe)
                    .ToList()
                    .OrderBy(a => a.Distance(Game.CursorPos))
                    .FirstOrDefault();
            if (allyT != null && InsecMenu["insecPositionMode"].Cast<Slider>().CurrentValue == 2)
            {
                AllyTarget = allyT;
                return;
            }

            AllyTarget = null;
            InsecTarget = null;
        }
    }
}