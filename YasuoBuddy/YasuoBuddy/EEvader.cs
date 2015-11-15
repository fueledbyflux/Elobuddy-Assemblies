using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using YasuoBuddy.EvadePlus;
using Color = System.Drawing.Color;

namespace YasuoBuddy
{
    internal static class EEvader
    {
        public static int WallCastT;
        private static Vector2 YasuoWallCastedPos;
        private static int WDelay;
        private static GameObject Wall;
        private static Geometry.Polygon.Rectangle WallPolygon;
        private static int _resetWall;

        public static void Init()
        {
            GameObject.OnCreate += GameObject_OnCreate;
            Game.OnTick += delegate { UpdateTask(); };
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (WallPolygon != null)
            {
                WallPolygon.DrawPolygon(Color.AliceBlue);
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(
                        sender.Name, "_w_windwall.\\.troy",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                Wall = sender;
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValid || sender.Team != ObjectManager.Player.Team || args.SData.Name != "YasuoWMovingWall")
                return;
            YasuoWallCastedPos = sender.ServerPosition.To2D();
            _resetWall = Environment.TickCount + 4000;
        }

        private static void UpdateTask()
        {
            if (Program.Evade == null) return;
            if (_resetWall - Environment.TickCount > 3400 && Wall != null)
            {
                var level = Player.GetSpell(SpellSlot.W).Level;
                var wallWidth = (300 + 50 * level);
                var wallDirection = (Wall.Position.To2D() - YasuoWallCastedPos).Normalized().Perpendicular();
                var wallStart = Wall.Position.To2D() + wallWidth / 2 * wallDirection;
                var wallEnd = wallStart - wallWidth * wallDirection;
                WallPolygon = new Geometry.Polygon.Rectangle(wallStart, wallEnd, 75);
            }
            if (_resetWall < Environment.TickCount)
            {
                Wall = null;
                WallPolygon = null;
            }
            if (Wall != null && YasuoWallCastedPos.IsValid() && WallPolygon != null)
            {
                foreach (var activeSkillshot in Program.Evade.SkillshotDetector.ActiveSkillshots.Where(EvadeMenu.IsSkillshotW).Where(activeSkillshot => WallPolygon.IsInside(activeSkillshot.GetPosition())))
                {
                    activeSkillshot.IsValid = false;
                }
            }

            Program.Evade.CacheSkillshots();

            if (!Program.Evade.IsHeroInDanger(Player.Instance)) return;
            {
                if (Yasuo.FleeMenu["Evade.W"].Cast<CheckBox>().CurrentValue && Player.GetSpell(SpellSlot.W).State == SpellState.Ready)
                {
                    foreach (var activeSkillshot in Program.Evade.SkillshotDetector.ActiveSkillshots.Where(a => EvadeMenu.IsSkillshotW(a) && Environment.TickCount - a.TimeDetected >= Yasuo.FleeMenu["Evade.WDelay"].Cast<Slider>().CurrentValue).Where(activeSkillshot => activeSkillshot.ToPolygon().IsInside(Player.Instance)))
                    {
                        Player.CastSpell(SpellSlot.W, activeSkillshot.GetPosition());
                        WDelay = Environment.TickCount + 500;
                        return;
                    }
                }

                if (WDelay > Environment.TickCount) return;

                var poly = Program.Evade.CustomPoly();

                if (Yasuo.FleeMenu["Evade.E"].Cast<CheckBox>().CurrentValue && Player.GetSpell(SpellSlot.E).State == SpellState.Ready)
                {
                    foreach (
                        var source in
                            EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                                a => a.Team != Player.Instance.Team && a.Distance(Player.Instance) < 475 && a.CanDash()))
                    {
                        if(source.GetDashPos().IsUnderTower()) continue;
                        if (EvadePlus.EvadePlus.IsPointSafe(poly, source.GetDashPos().To2D()))
                        {
                            int count = 0;
                            for (int i = 0; i < 10; i += 47)
                            {
                                if(!EvadePlus.EvadePlus.IsPointSafe(poly, Player.Instance.Position.Extend(source.GetDashPos(), i)))
                                {
                                    count ++;
                                }
                            }
                            if (count > 3) continue;
                            Player.CastSpell(SpellSlot.E, source);
                            break;
                        }
                    }
                    foreach (
                        var source in
                            EntityManager.Heroes.Enemies.Where(
                                a => a.IsEnemy && a.Distance(Player.Instance) < 475 && a.CanDash()))
                    {
                        if (source.GetDashPos().IsUnderTower()) continue;
                        if (!EvadePlus.EvadePlus.IsPointSafe(poly, source.GetDashPos().To2D())) continue;
                        var count = 0;
                        for (var i = 0; i < 10; i += 47)
                        {
                            if(!EvadePlus.EvadePlus.IsPointSafe(poly, Player.Instance.Position.Extend(source.GetDashPos(), i)))
                            {
                                count ++;
                            }
                        }
                        if (count > 3) continue;
                        Player.CastSpell(SpellSlot.E, source);
                        break;
                    }
                }
            }
        }
    }
}
