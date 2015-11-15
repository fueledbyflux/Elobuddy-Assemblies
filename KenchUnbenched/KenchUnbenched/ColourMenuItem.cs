using System.Linq;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace KenchUnbenched
{
    internal static class ColourMenuItem
    {
        private static readonly Color[] Colours =
        {
            Color.BlueViolet, Color.Aqua, Color.MidnightBlue, Color.Gold,
            Color.LimeGreen, Color.Violet, Color.WhiteSmoke, Color.DarkRed
        };
        private static readonly string[] ColoursName =
        {
            "BlueViolet", "Aqua", "MidnightBlue", "Gold", "LimeGreen", "Violet", "WhiteSmoke", "DarkRed"
        };

        public static void AddColourItem(this Menu menu, string uniqueId, int defaultColour = 0)
        {
            var a = menu.Add(uniqueId, new Slider("Colour Picker: ", defaultColour, 0, Colours.Count() - 1));
            a.DisplayName = "Colour Picker: " + ColoursName[a.CurrentValue];
            a.OnValueChange += delegate { a.DisplayName = "Colour Picker: " + ColoursName[a.CurrentValue]; };
        }

        public static Color GetColour(this Menu m, string id)
        {
            var number = m[id].Cast<Slider>();
            return number != null ? Colours[number.CurrentValue] : Color.AliceBlue;
        }
    }
}