using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace YasuoBuddy.EvadePlus
{
    public class EvadePlus
    {
        public int ServerTimeBuffer
        {
            get { return EvadeMenu.MainMenu["serverTimeBuffer"].Cast<Slider>().CurrentValue; }
        }

        public bool EvadeEnabled
        {
            get { return EvadeMenu.ControlsMenu["enableEvade"].Cast<KeyBind>().CurrentValue; }
        }

        public bool DodgeDangerousOnly
        {
            get { return EvadeMenu.ControlsMenu["dodgeOnlyDangerous"].Cast<KeyBind>().CurrentValue; }
        }

        public int ExtraEvadeRange
        {
            get { return EvadeMenu.MainMenu["extraEvadeRange"].Cast<Slider>().CurrentValue; }
        }

        public bool RandomizeExtraEvadeRange
        {
            get { return EvadeMenu.MainMenu["randomizeExtraEvadeRange"].Cast<CheckBox>().CurrentValue; }
        }

        public bool AllowRecalculateEvade
        {
            get { return EvadeMenu.MainMenu["recalculatePosition"].Cast<CheckBox>().CurrentValue; }
        }

        public bool RestorePosition
        {
            get { return EvadeMenu.MainMenu["moveToInitialPosition"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool DisableDrawings
        {
            get { return EvadeMenu.DrawMenu["disableAllDrawings"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool DrawEvadePoint
        {
            get { return EvadeMenu.DrawMenu["drawEvadePoint"].Cast<CheckBox>().CurrentValue; }
        }

        public bool DrawEvadeStatus
        {
            get { return EvadeMenu.DrawMenu["drawEvadeStatus"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool DrawDangerPolygon
        {
            get { return EvadeMenu.DrawMenu["drawDangerPolygon"].Cast<CheckBox>().CurrentValue; }
        }

        private static int IssueOrderTickLimit
        {
            get { return 90; }
        }

        public SkillshotDetector SkillshotDetector { get; private set; }
        //public PathFinding PathFinding { get; private set; }

        private EvadeSkillshot[] Skillshots { get; set; }
        private Geometry.Polygon[] Polygons { get; set; }
        public List<Geometry.Polygon> ClippedPolygons { get; private set; }
        public Vector2 LastIssueOrderPos;

        private readonly Dictionary<EvadeSkillshot, Geometry.Polygon> _skillshotPolygonCache =
            new Dictionary<EvadeSkillshot, Geometry.Polygon>();

        private EvadeResult LastEvadeResult;
        private Text StatusText;
        private Text StatusTextShadow;
        private int EvadeIssurOrderTime;

        public EvadePlus(SkillshotDetector detector)
        {
            Skillshots = new EvadeSkillshot[] {};
            Polygons = new Geometry.Polygon[] {};
            ClippedPolygons = new List<Geometry.Polygon>();
            //PathFinding = new PathFinding(this);
            StatusText = new Text("EvadePlus Enabled", new Font("Calisto MT", 10F, FontStyle.Bold));
            StatusTextShadow = new Text("EvadePlus Enabled", new Font("Calisto MT", 10F, FontStyle.Bold));

            SkillshotDetector = detector;
            SkillshotDetector.OnUpdateSkillshots += OnUpdateSkillshots;
            SkillshotDetector.OnSkillshotActivation += OnSkillshotActivation;
            SkillshotDetector.OnSkillshotDetected += OnSkillshotDetected;
            SkillshotDetector.OnSkillshotDeleted += OnSkillshotDeleted;

            Player.OnIssueOrder += PlayerOnIssueOrder;
            Game.OnTick += Ontick;
            Drawing.OnDraw += OnDraw;
        }

        private void OnUpdateSkillshots(EvadeSkillshot skillshot, bool remove, bool isProcessSpell)
        {
            CacheSkillshots();
        }

        private void OnSkillshotActivation(EvadeSkillshot skillshot)
        {
            CacheSkillshots();
        }

        private static void OnSkillshotDetected(EvadeSkillshot skillshot, bool isProcessSpell)
        {
            
        }

        private static void OnSkillshotDeleted(EvadeSkillshot skillshot)
        {
        }

        private static void Ontick(EventArgs args)
        {

        }

        private void PlayerOnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }
            CacheSkillshots();
        }

        private void OnDraw(EventArgs args)
        {
            if (DisableDrawings)
            {
                return;
            }

            if (DrawEvadePoint && LastEvadeResult != null)
            {
                if (LastEvadeResult.IsValid && LastEvadeResult.EnoughTime && !LastEvadeResult.Expired())
                {
                    Circle.Draw(new ColorBGRA(255, 0, 0, 255), Player.Instance.BoundingRadius, 25,
                        LastEvadeResult.WalkPoint);
                }
            }

            if (!DrawDangerPolygon) return;
            foreach (
                var pol in
                    Geometry.ClipPolygons(SkillshotDetector.ActiveSkillshots.Select(c => c.ToPolygon()))
                        .ToPolygons())
            {
                pol.DrawPolygon(Color.Red, 2);
            }
        }

        public void CacheSkillshots()
        {
            Skillshots =
                (DodgeDangerousOnly
                    ? SkillshotDetector.ActiveSkillshots.Where(c => c.SpellData.IsDangerous)
                    : SkillshotDetector.ActiveSkillshots).ToArray();

            _skillshotPolygonCache.Clear();
            Polygons = Skillshots.Select(c =>
            {
                var pol = c.ToPolygon();
                _skillshotPolygonCache.Add(c, pol);

                return pol;
            }).ToArray();
            ClippedPolygons = Geometry.ClipPolygons(Polygons).ToPolygons();
        }

        public List<Geometry.Polygon> CustomPoly()
        {
            Skillshots =
                (DodgeDangerousOnly
                    ? SkillshotDetector.ActiveSkillshots.Where(c => c.SpellData.IsDangerous && EvadeMenu.IsSkillshotE(c) && Environment.TickCount - c.TimeDetected >= Yasuo.FleeMenu["Evade.WDelay"].Cast<Slider>().CurrentValue)
                    : SkillshotDetector.ActiveSkillshots).ToArray();

            _skillshotPolygonCache.Clear();
            Polygons = Skillshots.Select(c =>
            {
                var pol = c.ToPolygon();
                _skillshotPolygonCache.Add(c, pol);

                return pol;
            }).ToArray();
            return Geometry.ClipPolygons(Polygons).ToPolygons();
        }

        private bool IsPointSafe(Vector2 point)
        {
            return !ClippedPolygons.Any(p => p.IsInside(point));
        }

        public static bool IsPointSafe(IEnumerable<Geometry.Polygon> poly, Vector2 point)
        {
            return !poly.Any(p => p.IsInside(point));
        }

        private bool IsPathSafe(Vector2[] path)
        {
            for (var i = 0; i < path.Length - 1; i++)
            {
                var start = path[i];
                var end = path[i + 1];

                if (
                    ClippedPolygons.Any(
                        p => p.IsInside(end) || p.IsInside(start) || p.IsIntersectingWithLineSegment(start, end)))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsPathSafe(Vector3[] path)
        {
            return IsPathSafe(path.ToVector2());
        }

        public bool IsHeroInDanger(AIHeroClient hero = null)
        {
            hero = hero ?? Player.Instance;
            return !IsPointSafe(hero.ServerPosition.To2D());
        }

        private int GetTimeAvailable(AIHeroClient hero = null)
        {
            hero = hero ?? Player.Instance;
            var skillshots = Skillshots.Where(c => _skillshotPolygonCache[c].IsInside(hero.Position)).ToArray();

            if (!skillshots.Any())
                return short.MaxValue;

            var times =
                skillshots.Select(c => c.GetAvailableTime(hero))
                    .Where(t => t > 0)
                    .OrderByDescending(t => t);

            return times.Any() ? times.Last() : short.MaxValue;
        }

        public int GetDangerValue(AIHeroClient hero = null)
        {
            hero = hero ?? Player.Instance;
            var skillshots = Skillshots.Where(c => _skillshotPolygonCache[c].IsInside(hero.Position)).ToArray();

            if (!skillshots.Any())
                return 0;

            var values = skillshots.Select(c => c.SpellData.DangerValue).OrderByDescending(t => t);
            return values.Any() ? values.First() : 0;
        }

        public EvadeResult CalculateEvade(Vector2 anchor)
        {
            var playerPos = Player.Instance.ServerPosition.To2D();
            var polygons = ClippedPolygons.Where(p => p.IsInside(playerPos)).ToArray();
            var maxTime = GetTimeAvailable();
            var time = Math.Max(0, maxTime - (Game.Ping + ServerTimeBuffer));
            var moveRadius = (0.8F*time/1000F)*Player.Instance.MoveSpeed;
            var segments = new List<Vector2[]>();

            foreach (var pol in polygons)
            {
                for (var i = 0; i < pol.Points.Count; i++)
                {
                    var start = pol.Points[i];
                    var end = i == pol.Points.Count - 1 ? pol.Points[0] : pol.Points[i + 1];

                    var intersections =
                        Utils.GetLineCircleIntersectionPoints(playerPos, moveRadius, start, end)
                            .Where(p => p.IsInLineSegment(start, end))
                            .ToList();

                    switch (intersections.Count)
                    {
                        case 0:
                            if (start.Distance(playerPos, true) < moveRadius.Pow() &&
                                end.Distance(playerPos, true) < moveRadius.Pow())
                            {
                                intersections = new[] {start, end}.ToList();
                            }
                            else
                            {
                                continue;
                            }
                            break;
                        case 1:
                            intersections.Add(playerPos.Distance(start, true) > playerPos.Distance(end, true)
                                ? end
                                : start);
                            break;
                    }

                    segments.Add(intersections.ToArray());
                }
            }

            if (!segments.Any()) //not enough time
            {
                var point = GetClosestPoint(polygons, playerPos);
                return new EvadeResult(this, point, anchor, maxTime, time,
                    point.Distance(playerPos, true) <= (moveRadius + 10).Pow());
            }

            const int maxdist = 1500;
            const int division = 35;
            var points = new List<Vector2>();

            foreach (var segment in segments)
            {
                var dist = segment[0].Distance(segment[1]);
                if (dist > maxdist)
                {
                    segment[0] = segment[0].Extend(segment[1], dist/2 - maxdist/2);
                    segment[1] = segment[1].Extend(segment[1], dist/2 - maxdist/2);
                    dist = maxdist;
                }

                const int step = maxdist/division;
                var count = dist/step;

                for (var i = 0; i < count; i++)
                {
                    var point = segment[0].Extend(segment[1], i*step);
                    if (!point.IsWall() &&
                        !Polygons.Where(pol => !pol.IsInside(playerPos))
                            .Any(pol => pol.IsIntersectingWithLineSegment(playerPos, point.Extend(playerPos, -50))) &&
                        Player.Instance.GetPath(point.To3DWorld(), true).Length <= 3)
                    {
                        points.Add(point);
                    }
                }
            }

            if (!points.Any())
            {
                return new EvadeResult(this, GetClosestPoint(polygons, playerPos), anchor, maxTime, time, true);
            }

            var evadePoint = points.OrderByDescending(p => p.Distance(anchor) + p.Distance(playerPos)).Last();
            return new EvadeResult(this, evadePoint, anchor, maxTime, time, true);
        }

        private static Vector2 GetClosestPoint(IEnumerable<Geometry.Polygon> polygons, Vector2 from)
        {
            var polPoints =
                polygons.Select(pol => pol.ToDetailedPolygon())
                    .SelectMany(pol => pol.Points)
                    .OrderByDescending(p => p.Distance(from, true));

            return !polPoints.Any() ? Vector2.Zero : polPoints.Last();
        }

        public bool IsHeroPathSafe(EvadeResult evade, Vector3[] desiredPath, AIHeroClient hero = null)
        {
            return false; //temporarily disabled

            hero = hero ?? Player.Instance;

            var path = (desiredPath ?? hero.RealPath()).ToVector2();
            var polygons = ClippedPolygons;
            var points = new List<Vector2>();

            for (var i = 0; i < path.Length - 1; i++)
            {
                var start = path[i];
                var end = path[i + 1];

                foreach (var pol in polygons)
                {
                    var intersections = pol.GetIntersectionPointsWithLineSegment(start, end);
                    if (intersections.Length > 0 && !pol.IsInside(hero))
                    {
                        return false;
                    }

                    points.AddRange(intersections);
                }
            }

            if (points.Count == 1)
            {
                var walkTime = hero.WalkingTime(points[0]);
                return walkTime <= evade.TimeAvailable;
            }

            return false;
        }

        private bool MoveTo(Vector2 point, bool limit = true)
        {
            if (limit && EvadeIssurOrderTime + IssueOrderTickLimit > Environment.TickCount)
            {
                return false;
            }

            EvadeIssurOrderTime = Environment.TickCount;
            Player.IssueOrder(GameObjectOrder.MoveTo, point.To3DWorld(), false);
            return true;
        }

        public bool MoveTo(Vector3 point, bool limit = true)
        {
            return MoveTo(point.To2D(), limit);
        }

        public bool DoEvade(Vector3[] desiredPath = null, PlayerIssueOrderEventArgs args = null)
        {
            return false;
        }

        public class EvadeResult
        {
            private EvadePlus Evade;
            private int ExtraRange { get; set; }

            private int Time { get; set; }
            private Vector2 PlayerPos { get; set; }
            private Vector2 EvadePoint { get; set; }
            private Vector2 AnchorPoint { get; set; }
            public int TimeAvailable { get; private set; }
            private int TotalTimeAvailable { get; set; }
            public bool EnoughTime { get; private set; }

            public bool IsValid
            {
                get { return !EvadePoint.IsZero; }
            }

            public Vector3 WalkPoint
            {
                get
                {
                    var walkPoint = EvadePoint.Extend(PlayerPos, -60);
                    var newPoint = walkPoint.Extend(PlayerPos, -ExtraRange);
                    return Evade.IsPointSafe(newPoint) ? newPoint.To3DWorld() : walkPoint.To3DWorld();
                }
            }

            public EvadeResult(EvadePlus evade, Vector2 evadePoint, Vector2 anchorPoint, int totalTimeAvailable,
                int timeAvailable,
                bool enoughTime)
            {
                Evade = evade;
                PlayerPos = Player.Instance.Position.To2D();
                Time = Environment.TickCount;

                EvadePoint = evadePoint;
                AnchorPoint = anchorPoint;
                TotalTimeAvailable = totalTimeAvailable;
                TimeAvailable = timeAvailable;
                EnoughTime = enoughTime;

                // extra evade range
                if (Evade.ExtraEvadeRange > 0)
                {
                    ExtraRange = (Evade.RandomizeExtraEvadeRange
                        ? Utils.Random.Next(Evade.ExtraEvadeRange/3, Evade.ExtraEvadeRange)
                        : Evade.ExtraEvadeRange);
                }
            }

            public bool Expired(int time = 4000)
            {
                return Elapsed(time);
            }

            private bool Elapsed(int time)
            {
                return Elapsed() > time;
            }

            private int Elapsed()
            {
                return Environment.TickCount - Time;
            }
        }
    }
}