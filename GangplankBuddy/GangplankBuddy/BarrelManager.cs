using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = SharpDX.Color;

namespace GangplankBuddy
{
    internal static class BarrelManager
    {
        public static void Init()
        {
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
        }

        private static void Game_OnTick(EventArgs args)
        {
            Barrels = ObjectManager.Get<Obj_AI_Base>().Where(a => a.BaseSkinName.ToLower().Contains("gangplankbarrel") && a.Health > 1 && a.HasBuffFromMe("gangplankebarrelactive")).ToList();
            Killablebarrels = ObjectManager.Get<Obj_AI_Base>().Where(a => a.BaseSkinName.ToLower().Contains("gangplankbarrel") && a.Health == 1 && a.HasBuffFromMe("gangplankebarrelactive")).ToList();
        }

        public static List<Obj_AI_Base> Barrels = new List<Obj_AI_Base>();

        public static List<Obj_AI_Base> Killablebarrels = new List<Obj_AI_Base>();

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Program.DrawingMenu["drawUnKillable"].Cast<CheckBox>().CurrentValue)
            {
                foreach (var gameObject in Barrels)
                {
                    Circle.Draw(Color.Wheat, 350, gameObject.Position);
                }
            }
            if (!Program.DrawingMenu["drawKillable"].Cast<CheckBox>().CurrentValue) return;
            {
                foreach (var gameObject in Killablebarrels)
                {
                    Circle.Draw(Color.Red, 350, gameObject.Position);
                }
            }
        }

        public static bool CanTriggerBarrel(Vector3 pos)
        {
            return Killablebarrels.Any(a => a.Distance(pos) < 350);
        }

        public static Obj_AI_Base KillableBarrelAroundUnit(Obj_AI_Base unit)
        {
            return Killablebarrels.FirstOrDefault(a => a.Distance(unit) < 350);
        }

        private static bool HasBuffFromMe(this Obj_AI_Base unit, string buff)
        {
            return unit.Buffs.Any(a => a.Name.ToLower().Contains(buff) && a.Caster.NetworkId == Player.Instance.NetworkId);
        }
    }
}
