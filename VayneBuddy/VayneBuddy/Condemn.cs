using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace VayneBuddy
{
    internal static class Condemn
    {
        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static long LastCheck;
        public static int CheckCount;
        public static Spell.Skillshot ESpell;

        public static bool IsCondemable(this AIHeroClient unit, Vector2 pos = new Vector2())
        {
            if (unit.HasBuffOfType(BuffType.SpellImmunity) || unit.HasBuffOfType(BuffType.SpellShield) || LastCheck + 50 > Environment.TickCount || _Player.IsDashing()) return false;
            var prediction = ESpell.GetPrediction(unit);
            var predictionsList = pos.IsValid() ? new List<Vector3>() {pos.To3D()} :  new List<Vector3>
                        {
                            unit.ServerPosition,
                            unit.Position,
                            prediction.CastPosition,
                            prediction.UnitPosition
                        };

            var wallsFound = 0;
            Program.Points = new List<Vector2>();
            foreach (var position in predictionsList)
            {
                for (var i = 0; i < Program.CondemnMenu["pushDistance"].Cast<Slider>().CurrentValue; i += (int) unit.BoundingRadius)
                {
                    var cPos = _Player.Position.Extend(position, _Player.Distance(position) + i).To3D();
                    Program.Points.Add(cPos.To2D());
                    if (!cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) &&
                        !cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building)) continue;
                    wallsFound++;
                    break;
                }
            }
            return wallsFound/ predictionsList.Count >= Program.CondemnMenu["condemnPercent"].Cast<Slider>().CurrentValue/100f;
        }

        public static Vector2 GetFirstNonWallPos(Vector2 startPos, Vector2 endPos)
        {
            var distance = 0;
            for (var i = 0; i < Program.CondemnMenu["pushDistance"].Cast<Slider>().CurrentValue; i += 20)
            {
                var cell = startPos.Extend(endPos, endPos.Distance(startPos) + i).ToNavMeshCell().CollFlags;
                if (cell.HasFlag(CollisionFlags.Wall) || cell.HasFlag(CollisionFlags.Building))
                {
                    distance = i - 20;
                }
            }
            return startPos.Extend(endPos, distance + endPos.Distance(startPos));
        }

        public static AIHeroClient CondemnTarget()
        {
            var min = Program.CondemnPriorityMenu["minSliderAutoCondemn"].Cast<Slider>().CurrentValue;
            return
                ObjectManager.Get<AIHeroClient>()
                    .Where(
                        a =>
                            a.IsEnemy && a.IsValidTarget(Program.E.Range) && a.IsCondemable() &&
                            Program.CondemnPriorityMenu[a.ChampionName + "priority"].Cast<Slider>().CurrentValue >= min)
                    .OrderByDescending(a => Program.CondemnPriorityMenu[a.ChampionName + "priority"].Cast<Slider>().CurrentValue)
                    .FirstOrDefault();
        }
    }

}
