using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace ActivatorBuddy.Defencives
{
    internal static class DefenceItems
    {
        private static InventorySlot Archangles;
        private static InventorySlot Locket;
        private static InventorySlot FaceOfTheMountain;
        private static InventorySlot ZhonyasHg;
        private static InventorySlot Mercs;
        private static InventorySlot Mikaels;

        private static Dictionary<ItemId, Func<float>> ShieldHeals = new Dictionary<ItemId, Func<float>>()
        {
            {ItemId.Face_of_the_Mountain, () => (float) (Player.Instance.MaxHealth*0.1)},
            {ItemId.Locket_of_the_Iron_Solari, () => 50 + Player.Instance.Level * 10 },
            {ItemId.Archangels_Staff, () => (float) (150 + 0.2 * Player.Instance.Mana)},
        };

        public static void Init()
        {
            Shop.OnBuyItem += delegate { UpdateItems(); };
            Shop.OnSellItem += delegate { UpdateItems(); };
            Shop.OnUndo += delegate { UpdateItems(); };
            Player.OnSwapItem += delegate { UpdateItems(); };
        }

        public static bool ShieldItems(AIHeroClient ally)
        {
            if (ally == null || ally.IsDead || ally.Health <= 0 || !ally.IsValid) return false;
            var predHp = ally.PredictedHealth();
            if (ally.IsMe && Defence.DefenceMenu["Archangels_Staff"].Cast<CheckBox>().CurrentValue && predHp + ShieldHeals[ItemId.Archangels_Staff].Invoke() > 0) // Seraphs
            {
                if (Archangles != null && Archangles.CanUseItem())
                {
                    Player.CastSpell(Archangles.SpellSlot);
                    Defence.LastSpellCast = Environment.TickCount + 250;
                    return true;
                }
            }
            if (Defence.DefenceMenu["Locket_of_the_Iron_Solari"].Cast<CheckBox>().CurrentValue && (ally.IsMe && Defence.DefenceMenu["Locket_of_the_Iron_Solari_self"].Cast<CheckBox>().CurrentValue || !ally.IsMe && Defence.DefenceMenu["Locket_of_the_Iron_Solari_ally"].Cast<CheckBox>().CurrentValue) && predHp + ShieldHeals[ItemId.Locket_of_the_Iron_Solari].Invoke() > 0 && ally.Distance(Player.Instance) < 600) // Locket
            {
                if (Locket != null && Locket.CanUseItem())
                {
                    Player.CastSpell(Locket.SpellSlot);
                    Defence.LastSpellCast = Environment.TickCount + 250;
                    return true;
                }
            }
            if (Defence.DefenceMenu["Face_of_the_Mountain"].Cast<CheckBox>().CurrentValue && (ally.IsMe && Defence.DefenceMenu["Face_of_the_Mountain_self"].Cast<CheckBox>().CurrentValue || !ally.IsMe) && predHp + ShieldHeals[ItemId.Face_of_the_Mountain].Invoke() > 0 && ally.Distance(Player.Instance) < 700) // FOM
            {
                if (FaceOfTheMountain != null && FaceOfTheMountain.CanUseItem())
                {
                    Player.CastSpell(FaceOfTheMountain.SpellSlot, ally);
                    Defence.LastSpellCast = Environment.TickCount + 250;
                    return true;
                }
            }
            if (!Defence.DefenceMenu["Mikaels_Crucible_Heal"].Cast<CheckBox>().CurrentValue || Mikaels == null ||
                !Mikaels.CanUseItem() || !(predHp + (150 + (0.1*ally.Health)) > 0) ||
                !(ally.Distance(Player.Instance) < 750)) return false;
            Player.CastSpell(Mikaels.SpellSlot, ally);
            Defence.LastSpellCast = Environment.TickCount + 250;
            return true;
        }

        public static bool CleanseItems(AIHeroClient ally)
        {
            if (ally == null || ally.IsDead || ally.Health <= 0 || !ally.IsValid) return false;
            if (!Defence.Damages[ally.NetworkId].DangerousSpells.Any(a => a.Value.IsCleanseable && (a.Value.BonusDelay == 0 || a.Key - Environment.TickCount < a.Value.BonusDelay / 2))) return false;
            if (Mercs != null && Mercs.CanUseItem())
            {
                Player.CastSpell(Mercs.SpellSlot);
                foreach (var spell in Defence.Damages[ally.NetworkId].DangerousSpells.Where(a => a.Value.IsCleanseable).ToList())
                {
                    Defence.Damages[ally.NetworkId].DangerousSpells.Remove(spell.Key);
                }
                Defence.LastSpellCast = Environment.TickCount + 250;
                return true;
            }
            if (Mikaels == null || !Mikaels.CanUseItem()) return false;
            {
                Player.CastSpell(Mikaels.SpellSlot, ally);
                foreach (var spell in Defence.Damages[ally.NetworkId].DangerousSpells.Where(a => a.Value.IsCleanseable).ToList())
                {
                    Defence.Damages[ally.NetworkId].DangerousSpells.Remove(spell.Key);
                }
                Defence.LastSpellCast = Environment.TickCount + 250;
                return true;
            }
        }

        public static bool Zhonyas(AIHeroClient ally)
        {
            if (!ally.IsMe) return false;
            if (ZhonyasHg == null || !ZhonyasHg.CanUseItem() ||
                (!Player.Instance.InDanger(true) &&
                 !Defence.Damages[Player.Instance.NetworkId].DangerousSpells.Any(
                     a =>
                         a.Value.BonusDelay > 0 && (a.Key - Environment.TickCount) <= 2200 + Game.Ping ||
                         a.Value.BonusDelay == 0))) return false;
            Player.CastSpell(ZhonyasHg.SpellSlot);
            Defence.LastSpellCast = Environment.TickCount + 250;
            return true;
        }

        private static void UpdateItems()
        {
            ZhonyasHg = Player.Instance.InventoryItems.FirstOrDefault( a => a.Id == ItemId.Zhonyas_Hourglass && Defence.DefenceMenu["Zhonyas_Hourglass"].Cast<CheckBox>().CurrentValue || ItemId.Wooglets_Witchcap == a.Id && Defence.DefenceMenu["Wooglets_Witchcap"].Cast<CheckBox>().CurrentValue);
            Mercs = Player.Instance.InventoryItems.FirstOrDefault( a => a.Id == ItemId.Dervish_Blade || a.Id == ItemId.Quicksilver_Sash || a.Id == ItemId.Mercurial_Scimitar);
            Mikaels = Player.Instance.InventoryItems.FirstOrDefault(a => a.Id == ItemId.Mikaels_Crucible);
            FaceOfTheMountain = Player.Instance.InventoryItems.FirstOrDefault(a => a.Id == ItemId.Face_of_the_Mountain);
            Locket = Player.Instance.InventoryItems.FirstOrDefault(a => a.Id == ItemId.Locket_of_the_Iron_Solari);
            Archangles = Player.Instance.InventoryItems.FirstOrDefault(a => (int) a.Id == 3040);
        }

    }
}
