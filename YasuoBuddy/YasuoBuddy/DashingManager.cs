using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using SharpDX;

namespace YasuoBuddy
{
    internal static class DashingManager
    {

        public static void Init()
        {
            Dash.OnDash += Dash_OnDash;
        }

        private static int _dashStartTime;
        private static int _dashEndTime;
        private static Vector3 _endPos;
        private static Vector3 _startPos;

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (!sender.IsMe) return;
            _dashStartTime = e.StartTick;
            _dashEndTime = e.EndTick;
            _endPos = e.EndPos;
            _startPos = e.StartPos;
        }

        public static Vector3 GetPlayerPosition(int timeModMs = 0)
        {
            if (Player.Instance.IsDashing() && _dashEndTime < Environment.TickCount + timeModMs)
            {
                return _startPos.Extend(_endPos, 475*((_dashEndTime - (Environment.TickCount + timeModMs))/((_dashStartTime - _dashEndTime) == 0 ? 1 : (_dashStartTime - _dashEndTime)))).To3D();
            }
            return Player.Instance.Position;
        }

        public static Vector3 GetDashPos(this Obj_AI_Base unit)
        {
            return Player.Instance.Position.Extend(Prediction.Position.PredictUnitPosition(unit, 250), 475).To3D();
        }

        public static Obj_AI_Base GetClosestEUnit(Vector3 pos)
        {
            var distance = 250000;
            Obj_AI_Base unit = null;
            foreach (var source in EntityManager.MinionsAndMonsters.CombinedAttackable.Where(a => a.CanDash() && !a.IsDead && a.Health > 0 && a.Distance(Player.Instance) < 475))
            {
                var dist = (int) source.GetDashPos().Distance(pos);
                if (dist >= distance) continue;
                distance = dist;
                unit = source;
            }
            if (unit != null) return unit;
            foreach (
                var source in
                    EntityManager.Heroes.Enemies.Where(
                        a => a.CanDash() && !a.IsDead && a.Health > 0 && a.Distance(Player.Instance) < 475))
            {
                var dist = (int) source.GetDashPos().Distance(pos);
                if (dist >= distance) continue;
                distance = dist;
                unit = source;
            }
            return unit;
        }

        public static Obj_AI_Base GetClosestEUnit(this Obj_AI_Base pos)
        {
            var distance = 250000;
            Obj_AI_Base unit = null;
            foreach (var source in EntityManager.MinionsAndMonsters.CombinedAttackable.Where(a =>a.CanDash() && !a.IsDead && a.Health > 0 && a.Distance(Player.Instance) < 475))
            {
                var dist = (int)source.GetDashPos().Distance(pos);
                if (dist >= distance) continue;
                distance = dist;
                unit = source;
            }
            if (unit != null) return unit;
            foreach (
                var source in
                    EntityManager.Heroes.Enemies.Where(
                        a => a.CanDash() && !a.IsDead && a.Health > 0 && a.Distance(Player.Instance) < 475))
            {
                var dist = (int)source.GetDashPos().Distance(pos);
                if (dist >= distance) continue;
                distance = dist;
                unit = source;
            }
            return unit;
        }
    }
}
