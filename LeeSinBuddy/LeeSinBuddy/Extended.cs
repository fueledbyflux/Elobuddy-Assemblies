using System.Linq;
using EloBuddy;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace LeeSinBuddy
{
    internal static class Extended
    {
        public static Obj_AI_Base BuffedEnemy
        {
            get { return ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(unit => unit.IsEnemy && unit.HasQBuff()); }
        }

        public static void AddLabeledSlider(this Menu menu, string name, string displayName, string[] values,
            int defaultValue = 0)
        {
            var slider = menu.Add(name, new Slider(displayName, defaultValue, 0, values.Length - 1));
            slider.DisplayName = values[slider.CurrentValue];
            slider.OnValueChange += delegate { slider.DisplayName = values[slider.CurrentValue]; };
        }

        public static bool HasEBuff(this Obj_AI_Base unit)
        {
            return (unit.HasBuffs("BlindMonkEOne") || unit.HasBuffs("BlindMonkEOne"));
        }

        public static bool HasQBuff(this Obj_AI_Base unit)
        {
            return (unit.HasBuffs("BlindMonkQOne") || unit.HasBuffs("blindmonkqonechaos"));
        }

        private static bool HasBuffs(this Obj_AI_Base unit, string s)
        {
            return
                unit.Buffs.Any(
                    a => a.Name.ToLower().Contains(s.ToLower()) || a.DisplayName.ToLower().Contains(s.ToLower()));
        }
    }
}