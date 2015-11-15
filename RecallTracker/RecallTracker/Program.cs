using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using RecallTracker.Properties;
using SharpDX;
using Color = System.Drawing.Color;

namespace RecallTracker
{
    internal static class Program
    {
        private static List<Recall> Recalls = new List<Recall>();

        private static readonly TextureLoader TextureLoader = new TextureLoader();
        private static Menu Menu;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static Sprite TopSprite { get; set; }
        private static Sprite BottomSprite { get; set; }
        private static Sprite BackSprite { get; set; }
        private static Text Text { get; set; }
        private static Text TextTwo { get; set; }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Menu = MainMenu.AddMenu("Recall Tracker", "RecallTracker");
            Menu.AddGroupLabel("Recall Tracker");
            Menu.AddLabel("X/Y Settings");
            Menu.Add("recallX", new Slider("X Offset", 0, -500, 500));
            Menu.Add("recallY", new Slider("Y Offset", 0, -500, 500));
            Menu.AddLabel("Misc Settings");
            var a = Menu.Add("resetDefault", new CheckBox("Reset X/Y", false));
            Menu.Add("alwaysDrawFrame", new CheckBox("Always Draw Frame", false));
            Menu.Add("drawPlayerNames", new CheckBox("Draw Player Names (All for One)", false));
            Menu.AddLabel("Who To Track:");
            Menu.Add("trackAllies", new CheckBox("Track Allies", false));
            Menu.Add("trackMe", new CheckBox("Track Me", false));
            a.OnValueChange += delegate
            {
                Menu["recallX"].Cast<Slider>().CurrentValue = 0;
                Menu["recallY"].Cast<Slider>().CurrentValue = 0;
            };

            TextureLoader.Load("top", Resources.TopHUD);
            TextureLoader.Load("bottom", Resources.BottomHUD);
            TextureLoader.Load("back", Resources.Back);

            TopSprite = new Sprite(() => TextureLoader["top"]);
            BottomSprite = new Sprite(() => TextureLoader["bottom"]);
            BackSprite = new Sprite(() => TextureLoader["back"]);

            Text = new Text("", new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold)) { Color = System.Drawing.Color.AntiqueWhite };
            TextTwo = new Text("", new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold)) { Color = System.Drawing.Color.AntiqueWhite };
            
            Teleport.OnTeleport += Teleport_OnTeleport;
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (!Recalls.Any() && !Menu["alwaysDrawFrame"].Cast<CheckBox>().CurrentValue) return;

            var x = (int) ((Drawing.Width * 0.846875) + Menu["recallX"].Cast<Slider>().CurrentValue);
            var y = (int) (Drawing.Height * 0.5555555555555556) + Menu["recallY"].Cast<Slider>().CurrentValue;

            TopSprite.Draw(new Vector2(x + 1, y));
            var bonus = 0;
            foreach (var recall in Recalls.ToList())
            {
                BackSprite.Draw(new Vector2(x, y + 18 + bonus));
                Text.Draw(Menu["drawPlayerNames"].Cast<CheckBox>().CurrentValue ? recall.Unit.Name.Truncate(10) : recall.Unit.ChampionName.Truncate(10), Color.White, x + 15, y + bonus + 27);
                Text.Draw(recall.PercentComplete() + "%", Color.White, new Vector2(x + 258, y + bonus + 26));
                Line.DrawLine(Color.White, 10, new Vector2[] {new Vector2(x + 80, y + bonus + 33), new Vector2(x + 250, y + bonus + 33) });
                Line.DrawLine(recall.IsAborted ? Color.Orange : BarColour(recall.PercentComplete()), 10, new Vector2[] { new Vector2(x + 80, y + bonus + 33), new Vector2(x + 80 + (170 * (recall.PercentComplete() / 100)), y + bonus + 33) });
                bonus += 31;

                if (recall.ExpireTime < Environment.TickCount && Recalls.Contains(recall))
                {
                    Recalls.Remove(recall);
                }
            }

            BottomSprite.Draw(new Vector2(x + 1, y + bonus + 18));
        }

        private static Color BarColour(float percent)
        {
            if (percent > 80)
            {
                return Color.LawnGreen;
            }
            if (percent > 60)
            {
                return Color.GreenYellow;
            }

            if (percent > 40)
            {
                return Color.MediumSpringGreen;
            }
            return percent > 20 ? Color.Aquamarine : Color.DeepSkyBlue;
        }

        private static void Teleport_OnTeleport(Obj_AI_Base sender, Teleport.TeleportEventArgs args)
        {
            if (args.Type != TeleportType.Recall || !(sender is AIHeroClient) || sender.IsAlly && !sender.IsMe && !Menu["trackAllies"].Cast<CheckBox>().CurrentValue || sender.IsMe && !Menu["trackMe"].Cast<CheckBox>().CurrentValue) return;

            switch (args.Status)
            {
                 case TeleportStatus.Abort:
                    foreach (var source in Recalls.Where(a => a.Unit == sender))
                    {
                        source.Abort();
                    }
                    break;
                case TeleportStatus.Start:
                    var recall = Recalls.FirstOrDefault(a => a.Unit == sender);
                    if (recall != null)
                    {
                        Recalls.Remove(recall);
                    }
                    Recalls.Add(new Recall((AIHeroClient)sender, Environment.TickCount, Environment.TickCount + args.Duration, args.Duration));
                    break;
                case TeleportStatus.Finish:
                    break;
                case TeleportStatus.Unknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
