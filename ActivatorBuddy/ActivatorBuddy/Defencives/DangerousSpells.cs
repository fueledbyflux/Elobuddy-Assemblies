using System.Collections.Generic;
using EloBuddy;

namespace ActivatorBuddy.Defencives
{

    public class DangerousSpells
    {
        public static List<DangerousSpell> Spells = new List<DangerousSpell>()
        {
            new DangerousSpell(Champion.Annie, SpellSlot.R),
            new DangerousSpell(Champion.Caitlyn, SpellSlot.R),
            new DangerousSpell(Champion.Darius, SpellSlot.R),
            new DangerousSpell(Champion.Malphite, SpellSlot.R),
            new DangerousSpell(Champion.Zed, SpellSlot.R, 4000) {IsCleanseable = true},
        }; 
    }

    public class DangerousSpell
    {
        public DangerousSpell(Champion champ, SpellSlot slot, int bonusDelay = 0)
        {
            Slot = slot;
            Champion = champ;
            BonusDelay = bonusDelay;
        }

        public SpellSlot Slot;
        public Champion Champion;
        public int BonusDelay;
        public bool IsCleanseable = false;
    }
}
