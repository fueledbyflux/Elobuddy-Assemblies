using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace YasuoBuddy.TargetedSpells
{
    internal static class SpellDetectorWindwaller
    {
        private static Menu _targetedMenu;
        public static void Init()
        {
            _targetedMenu = EvadePlus.EvadeMenu.MainMenu.AddSubMenu("Targeted Skills");
            _targetedMenu.AddGroupLabel("Targeted Skills");
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                _targetedMenu.AddSeparator();
                _targetedMenu.AddLabel(enemy.ChampionName);
                var enemy1 = enemy;
                foreach (var spell in TargetSpellDatabase.Spells.Where(a => a.ChampionName.Equals(enemy1.ChampionName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    _targetedMenu.Add(spell.Name + "/eyas", new CheckBox(spell.Name + ": " + spell.Spellslot));
                }
            }
            _targetedMenu.AddSeparator();
            _targetedMenu.AddLabel("Baron");
            foreach (var spell in TargetSpellDatabase.Spells.Where(a => a.ChampionName == "Baron"))
            {
                _targetedMenu.Add(spell.Name + "/eyas", new CheckBox(spell.Name + ": Baron"));
            }

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsAlly || !(sender is AIHeroClient) || args.Target == null || !args.Target.IsMe || Player.GetSpell(SpellSlot.W).State != SpellState.Ready) return;
            var spell = TargetSpellDatabase.GetByName(args.SData.Name);
            if (spell == null || _targetedMenu[spell.Name + "/eyas"] == null ||
                !_targetedMenu[spell.Name + "/eyas"].Cast<CheckBox>().CurrentValue) return;
            if (spell.Delay == 0)
            {
                Player.CastSpell(SpellSlot.W, sender.Position);
                return;
            }
            Core.DelayAction(() => Player.CastSpell(SpellSlot.W, sender.Position), spell.Delay);
        }
    }
}
