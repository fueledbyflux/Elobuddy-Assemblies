using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;

namespace YasuoBuddy.TargetedSpells
{
        public static class TargetSpellDatabase
        {
            public static List<TargetSpellData> Spells;

            static TargetSpellDatabase()
            {
                Spells = new List<TargetSpellData>
            {
                new TargetSpellData("Syndra", "syndrar", SpellSlot.R),
                new TargetSpellData("VeigarR", "veigarprimordialburst", SpellSlot.R),
                new TargetSpellData("Malzahar", "alzaharnethergrasp", SpellSlot.R),
                new TargetSpellData("Caitlyn", "CaitlynAceintheHole", SpellSlot.R, 1000),
                new TargetSpellData("Caitlyn", "CaitlynHeadshotMissile", SpellSlot.Unknown),
                new TargetSpellData("Brand", "BrandWildfire", SpellSlot.R),
                new TargetSpellData("Brand", "brandconflagrationmissile", SpellSlot.E),
                new TargetSpellData("Kayle", "judicatorreckoning", SpellSlot.Q),
                new TargetSpellData("Pantheon", "PantheonQ", SpellSlot.Q),
                new TargetSpellData("Taric", "Dazzle", SpellSlot.Q),
                new TargetSpellData("TwistedFate", "GoldCardAttack", SpellSlot.W),
                new TargetSpellData("Viktor", "viktorpowertransfer", SpellSlot.Q),
                new TargetSpellData("Ahri", "ahrifoxfiremissiletwo", SpellSlot.W),
                new TargetSpellData("Elise", "EliseHumanQ", SpellSlot.Q),
                new TargetSpellData("Shaco", "TwoShivPoison", SpellSlot.E),
                new TargetSpellData("Urgot", "UrgotHeatseekingHomeMissile", SpellSlot.Q),
                new TargetSpellData("Lucian", "LucianPassiveShot", SpellSlot.Unknown),
                new TargetSpellData("Baron", "BaronAcidBall", SpellSlot.Unknown),
                new TargetSpellData("Baron", "BaronAcidBall2", SpellSlot.Unknown),
                new TargetSpellData("Baron", "BaronDeathBreathProj1", SpellSlot.Unknown),
                new TargetSpellData("Baron", "BaronDeathBreathProj3", SpellSlot.Unknown),
                new TargetSpellData("Baron", "BaronSpike", SpellSlot.Unknown),
                new TargetSpellData("Leblanc", "LeblancChaosOrbM", SpellSlot.Q),
                new TargetSpellData("Annie", "disintegrate", SpellSlot.Q),
                new TargetSpellData("Twisted Fate", "GoldCardAttack", SpellSlot.W),
                new TargetSpellData("Twisted Fate", "RedCardAttack", SpellSlot.W),
                new TargetSpellData("Twisted Fate", "RedCardAttack", SpellSlot.W),
                new TargetSpellData("Kassadin", "NullLance", SpellSlot.Q),
                new TargetSpellData("Teemo", "BlindingDart", SpellSlot.Q),
                new TargetSpellData("Malphite", "SeismicShard", SpellSlot.Q),
                new TargetSpellData("Vayne", "VayneCondemn", SpellSlot.E),
                new TargetSpellData("Nunu", "IceBlast", SpellSlot.E),
                new TargetSpellData("Tristana", "BusterShot", SpellSlot.R),
                new TargetSpellData("Cassiopeia", "CassiopeiaTwinFang", SpellSlot.E),
                new TargetSpellData("Pantheon", "Pantheon_Throw", SpellSlot.Q),
                new TargetSpellData("Akali", "AkaliMot", SpellSlot.Q),
                new TargetSpellData("Leblanc", "LeblancChaosOrbM", SpellSlot.Q),
                new TargetSpellData("Anivia", "Frostbite", SpellSlot.E),
        };
            }

            public static TargetSpellData GetByName(string spellName)
            {
                spellName = spellName.ToLower();
                return Spells.FirstOrDefault(spell => spell.Name.Equals(spellName, StringComparison.CurrentCultureIgnoreCase));
            }
        }
}
