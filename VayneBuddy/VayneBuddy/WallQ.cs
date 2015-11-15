using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace VayneBuddy
{
    internal static class WallQ
    {
        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }


        public static void Drawing_OnDraw()
        {
            if (Game.MapId != GameMapId.SummonersRift) return;
            var drakeWallQPos = new Vector2(12050, 4827);
            var midWallQPos = new Vector2(6962, 8952);
            if (drakeWallQPos.Distance(_Player) < 3000)
                new Circle { Color = _Player.Distance(drakeWallQPos) <= 100 ? Color.DodgerBlue : Color.White, Radius = 100 }.Draw(drakeWallQPos.To3D());
            if (midWallQPos.Distance(_Player) < 3000)
                new Circle { Color = _Player.Distance(midWallQPos) <= 100 ? Color.DodgerBlue : Color.White,  Radius = 100 }.Draw(midWallQPos.To3D());

        }

        public static void WallTumble()
        {
            if (Game.MapId != GameMapId.SummonersRift) return;
            if (!Program.Q.IsReady())
            {
                Orbwalker.DisableMovement = false;
                return;
            }
            Orbwalker.DisableMovement = true;

            var drakeWallQPos = new Vector2(11514, 4462);
            var midWallQPos = new Vector2(6667, 8794);

            var selectedPos = drakeWallQPos.Distance(_Player) < midWallQPos.Distance(_Player) ? drakeWallQPos :  midWallQPos;
            var walkPos = drakeWallQPos.Distance(_Player) < midWallQPos.Distance(_Player)
                ? new Vector2(12050, 4827)
                : new Vector2(6962, 8952);
            if (_Player.Distance(walkPos) < 200 && _Player.Distance(walkPos) > 60)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, walkPos.To3D());
            }
            else if(_Player.Distance(walkPos) <= 50)
            {
                Player.CastSpell(SpellSlot.Q, selectedPos.To3D());
            }
        }
    }
}
