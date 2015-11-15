using EloBuddy;

namespace YasuoBuddy.TargetedSpells
{

    // Credits to Dektus for Spells/SpellClass
    
    public class TargetSpellData
    {
        public string ChampionName { get; private set; }
        public SpellSlot Spellslot { get; private set; }
        public string Name { get; private set; }
        public int Delay { get; private set; }

        public TargetSpellData(string champ, string spellname, SpellSlot slot, int delay = 0)
        {
            ChampionName = champ;
            Name = spellname;
            Spellslot = slot;
            Delay = delay;
        }
    }
}
