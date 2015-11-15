using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ActivatorBuddy.Summoner_Spells;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace ActivatorBuddy.Items
{
    internal static class ItemManager
    {
        private static Menu PotionsMenu;
        private static Menu OffensiveMenu;
        public static Menu Offensive2Menu;
        public static Menu DefensiveMenu;
        public static Menu Summoners;
        private static Menu Cleansers;

        private static Dictionary<BuffType, string> BuffTypes = new Dictionary<BuffType, string>
        {
            {BuffType.Stun, "stunActivator"},
            {BuffType.Polymorph, "polymorphActivator"},
            {BuffType.Snare, "rootActivator"},
            {BuffType.Slow, "slowActivator"},
            {BuffType.Knockup, "knockupActivator"},
            {BuffType.Taunt, "tauntActivator"},
            {BuffType.Fear, "fearActivator"}
        };

        private static Item[] Items =
        {
            new Item("botrk", 450, CastType.Targeted, ItemId.Blade_of_the_Ruined_King, ItemType.Offensive),
            new Item("cutlass", 450, CastType.Targeted, ItemId.Bilgewater_Cutlass, ItemType.Offensive),
            new Item("tiamat", 250, CastType.SelfCast, ItemId.Tiamat_Melee_Only, ItemType.Offensive, true),
            new Item("hydra", 250, CastType.SelfCast, ItemId.Ravenous_Hydra_Melee_Only, ItemType.Offensive, true),
            new Item("gunblade", 700, CastType.Targeted, ItemId.Hextech_Gunblade, ItemType.Offensive),
            new Item("ghostblade", 1500, CastType.SelfCast, ItemId.Youmuus_Ghostblade, ItemType.Offensive),
            new Item("refillpot", int.MaxValue, CastType.SelfCast, (ItemId) 2031, ItemType.Healing, false,
                "ItemCrystalFlask"),
            new Item("corruptpot", int.MaxValue, CastType.SelfCast, (ItemId) 2033, ItemType.Healing, false,
                "ItemDarkCrystalFlask"),
            new Item("corruptpot", int.MaxValue, CastType.SelfCast, (ItemId) 2033, ItemType.ManaRestore, false,
                "ItemDarkCrystalFlask"),
            new Item("huntersPot", int.MaxValue, CastType.SelfCast, (ItemId) 2032, ItemType.Healing, false,
                "ItemCrystalFlaskJungle"),
            new Item("huntersPot", int.MaxValue, CastType.SelfCast, (ItemId) 2032, ItemType.ManaRestore, false,
                "ItemCrystalFlaskJungle"),
            new Item("healthPotion", int.MaxValue, CastType.SelfCast, ItemId.Health_Potion, ItemType.Healing, false,
                "RegenerationPotion"),
            new Item("mikaelsCleanse", int.MaxValue, CastType.SelfCast, ItemId.Mikaels_Crucible, ItemType.Cleanse),
            new Item("mercurialScimitarCleanse", int.MaxValue, CastType.SelfCast, ItemId.Mercurial_Scimitar,
                ItemType.Cleanse),
            new Item("quicksilverSashCleanse", int.MaxValue, CastType.SelfCast, ItemId.Quicksilver_Sash,
                ItemType.Cleanse)
        };

        private static Spell.Active Cleanse;

        private static List<Item> ActiveItems = new List<Item>();

        public static void Init()
        {
            var menu = Program.Menu;

            OffensiveMenu = menu.AddSubMenu("Offensive Items", "offItems");
            OffensiveMenu.AddGroupLabel("Offensive Items");
            OffensiveMenu.AddLabel("(Activates With Combo)");
            OffensiveMenu.AddLabel("Blade Of The Ruined King");
            OffensiveMenu.Add("botrkManager", new CheckBox("Blade Of The Ruined King"));
            OffensiveMenu.Add("botrkManagerMinMeHP", new Slider("Self HP %", 80));
            OffensiveMenu.Add("botrkManagerMinEnemyHP", new Slider("Enemy HP HP %", 80));
            OffensiveMenu.AddLabel("Cutlass");
            OffensiveMenu.Add("cutlassManager", new CheckBox("Cutlass"));
            OffensiveMenu.Add("cutlassManagerMinMeHP", new Slider("Self HP %", 80));
            OffensiveMenu.Add("cutlassManagerMinEnemyHP", new Slider("Enemy HP HP %", 80));

            if (Player.Instance.IsMelee)
            {
                OffensiveMenu.AddLabel("Tiamat");
                OffensiveMenu.Add("tiamatManager", new CheckBox("Use Tiamat"));
                OffensiveMenu.Add("tiamatManagerMinMeHP", new Slider("Self HP %", 99));
                OffensiveMenu.Add("tiamatManagerMinEnemyHP", new Slider("Enemy HP HP %", 99));
                OffensiveMenu.AddLabel("Hydra");
                OffensiveMenu.Add("hydraManager", new CheckBox("Use Hydra"));
                OffensiveMenu.Add("hydraManagerMinMeHP", new Slider("Self HP %", 99));
                OffensiveMenu.Add("hydraManagerMinEnemyHP", new Slider("Enemy HP HP %", 99));
            }

            OffensiveMenu.AddLabel("Gunblade");
            OffensiveMenu.Add("gunbladeManager", new CheckBox("Use Gunblade"));
            OffensiveMenu.Add("gunbladeManagerMinMeHP", new Slider("Self HP %", 99));
            OffensiveMenu.Add("gunbladeManagerMinEnemyHP", new Slider("Enemy HP HP %", 99));
            OffensiveMenu.AddLabel("GhostBlade");
            OffensiveMenu.Add("ghostbladeManager", new CheckBox("Use GhostBlade"));
            OffensiveMenu.Add("ghostbladeManagerMinMeHP", new Slider("Self HP %", 99));
            OffensiveMenu.Add("ghostbladeManagerMinEnemyHP", new Slider("Enemy HP HP %", 99));

            PotionsMenu = menu.AddSubMenu("Potions", "potions");
            PotionsMenu.AddGroupLabel("Potion Items");
            PotionsMenu.Add("healthPotionManager", new CheckBox("Health Potion"));
            PotionsMenu.Add("healthPotionManagerMinMeHP", new Slider("Min HP %", 40));
            PotionsMenu.AddSeparator();
            PotionsMenu.Add("corruptpotManager", new CheckBox("Corrupt Potion"));
            PotionsMenu.Add("corruptpotManagerMinMeHP", new Slider("Min HP %", 40));
            PotionsMenu.Add("corruptpotManagerMinMeMana", new Slider("Min Mana %", 40));
            PotionsMenu.AddSeparator();
            PotionsMenu.Add("huntersPotManager", new CheckBox("Hunter's Potion"));
            PotionsMenu.Add("huntersPotManagerMinMeHP", new Slider("Min HP %", 40));
            PotionsMenu.Add("huntersPotManagerMinMeMana", new Slider("Min Mana %", 40));

            OffensiveMenu.AddSeparator();

            Cleansers = menu.AddSubMenu("Cleansers", "cleansers");
            Cleansers.AddGroupLabel("Cleansers Settings");
            Cleansers.Add("polymorphActivator", new CheckBox("PolyMorph"));
            Cleansers.Add("stunActivator", new CheckBox("Stun"));
            Cleansers.Add("tauntActivator", new CheckBox("Taunt"));
            Cleansers.Add("knockupActivator", new CheckBox("Knock-up"));
            Cleansers.Add("fearActivator", new CheckBox("Fear"));
            Cleansers.Add("rootActivator", new CheckBox("Root"));
            Cleansers.Add("slowActivator", new CheckBox("Slow"));
            Cleansers.AddSeparator();
            Cleansers.AddLabel("Cleanse Items / Summoner Spell");
            Cleansers.Add("mikaelsCleanser", new CheckBox("Mikael's Cruicble"));
            Cleansers.Add("mercurialScimitarCleanser", new CheckBox("Mercurial Scimitar"));
            Cleansers.Add("quicksilverSashCleanser", new CheckBox("Quicksilver Sash"));
            Cleansers.Add("summonerSpellCleanse", new CheckBox("Summoner Cleanse"));

            Cleanse = SummonerSpells.HasSpell("summonerboost") ? new Spell.Active(Player.Instance.GetSpellSlotFromName("summonerboost"), int.MaxValue) : null;

            foreach (var item in Items.Where(item => ActiveItems.All(a => a.Name != item.Name) && Player.Instance.InventoryItems.Any(a => a.Id == item.Id)))
            {
                ActiveItems.Add(item);
            }

            Game.OnTick += Game_OnTick;
            Shop.OnBuyItem += Shop_OnBuyItem;
            Shop.OnSellItem += Shop_OnSellItem;
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
            
            if(Player.Instance.Hero == Champion.Riven) Chat.Print("Tiamat/Hydra Disabled For Riven, it is inbuilt to RivenBuddy", Color.LimeGreen);
        }

        private static void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || !args.SData.IsAutoAttack() || !(args.Target is AIHeroClient) || args.Target.NetworkId != Orbwalker.LastTarget.NetworkId || Player.Instance.Hero == Champion.Riven) return;

            var target = (AIHeroClient) args.Target;
            if (target == null) return;
                

            foreach (var spellSlot in from item in ActiveItems where !Player.Instance.InventoryItems.All(a => a.Id != item.Id || !item.MeleeOnly) let menuItem = OffensiveMenu[item.Name + "Manager"].Cast<CheckBox>() let menuItemMe = OffensiveMenu[item.Name + "ManagerMinMeHP"].Cast<Slider>() let menuItemEnemy = OffensiveMenu[item.Name + "ManagerMinEnemyHP"].Cast<Slider>() where target.IsValidTarget() && !(target.Distance(Player.Instance) > item.Range) && menuItem.CurrentValue && !(menuItemMe.CurrentValue <= Player.Instance.HealthPercent) && !(menuItemEnemy.CurrentValue <= target.HealthPercent) select Player.Instance.InventoryItems.FirstOrDefault(a => a.Id == item.Id) into spellSlot where spellSlot != null && Player.GetSpell(spellSlot.SpellSlot).IsReady select spellSlot)
            {
                Player.CastSpell(spellSlot.SpellSlot);
                return;
            }
        }

        private static void Shop_OnSellItem(AIHeroClient sender, ShopActionEventArgs args)
        {
            Core.DelayAction(delegate
            {
                var item = ActiveItems.FirstOrDefault(a => a.Id == (ItemId) args.Id);
                if (item != null && Player.Instance.InventoryItems.All(a => a.Id != item.Id))
                {
                    ActiveItems.Remove(item);
                }
            }, 200);
        }

        private static void Shop_OnBuyItem(AIHeroClient sender, ShopActionEventArgs args)
        {
            if (!sender.IsMe) return;

            foreach (var item in Items.Where(item => ActiveItems.All(a => a.Name != item.Name) && (ItemId) args.Id == item.Id))
            {
                ActiveItems.Add(item);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            foreach (var item in ActiveItems.Where(item => !Player.Instance.InventoryItems.All(a => a.Id != item.Id || item.MeleeOnly)))
            {
                switch (item.ItemType)
                {
                    case ItemType.Offensive:
                    {
                        if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) continue;

                        var target = (Obj_AI_Base) Orbwalker.LastTarget;

                        var menuItem = OffensiveMenu[item.Name + "Manager"].Cast<CheckBox>();
                        var menuItemMe = OffensiveMenu[item.Name + "ManagerMinMeHP"].Cast<Slider>();
                        var menuItemEnemy = OffensiveMenu[item.Name + "ManagerMinEnemyHP"].Cast<Slider>();

                        if (!target.IsValidTarget() || target.Distance(Player.Instance) > item.Range || 
                            (item.MeleeOnly && !Player.Instance.IsMelee) || !menuItem.CurrentValue || menuItemMe.CurrentValue <= Player.Instance.HealthPercent || menuItemEnemy.CurrentValue <= target.HealthPercent) continue;

                        switch (item.CastType)
                        {
                            case CastType.Targeted:
                            {
                                var spellSlot = Player.Instance.InventoryItems.FirstOrDefault(a => a.Id == item.Id);
                                if (spellSlot != null && Player.GetSpell(spellSlot.SpellSlot).IsReady)
                                {
                                    Player.CastSpell(spellSlot.SpellSlot, target);
                                }
                            }
                                break;
                            case CastType.SelfCast:
                            {
                                var spellSlot = Player.Instance.InventoryItems.FirstOrDefault(a => a.Id == item.Id);
                                if (spellSlot != null && Player.GetSpell(spellSlot.SpellSlot).IsReady)
                                {
                                    Player.CastSpell(spellSlot.SpellSlot);
                                }
                            }
                                break;
                        }
                    }
                        break;
                    case ItemType.Healing:
                    {
                        var menuItem = PotionsMenu[item.Name + "Manager"].Cast<CheckBox>();
                        var menuItemMe = PotionsMenu[item.Name + "ManagerMinMeHP"].Cast<Slider>();
                        if (Player.Instance.IsInShopRange() || Player.Instance.HasBuff(item.BuffName) || !menuItem.CurrentValue ||
                            menuItemMe.CurrentValue < Player.Instance.HealthPercent) continue;
                        var spellSlot = Player.Instance.InventoryItems.FirstOrDefault(a => a.Id == item.Id);
                        if (spellSlot != null && Player.GetSpell(spellSlot.SpellSlot).IsReady)
                        {
                            Player.CastSpell(spellSlot.SpellSlot);
                        }
                    }
                        break;
                    case ItemType.ManaRestore:
                    {
                        var menuItem = PotionsMenu[item.Name + "Manager"].Cast<CheckBox>();
                        var menuItemMe = PotionsMenu[item.Name + "ManagerMinMeMana"].Cast<Slider>();
                        if (Player.Instance.IsInShopRange() || Player.Instance.HasBuff(item.BuffName) || !menuItem.CurrentValue ||
                            menuItemMe.CurrentValue < Player.Instance.ManaPercent) continue;
                        var spellSlot = Player.Instance.InventoryItems.FirstOrDefault(a => a.Id == item.Id);
                        if (spellSlot != null && Player.GetSpell(spellSlot.SpellSlot).IsReady)
                        {
                            Player.CastSpell(spellSlot.SpellSlot);
                        }
                    }
                        break;

                    case ItemType.Cleanse:
                    {
                        foreach (var buffInstance in Player.Instance.Buffs)
                        {
                            if (BuffTypes.ContainsKey(buffInstance.Type) &&
                                Cleansers[BuffTypes[buffInstance.Type]].Cast<CheckBox>().CurrentValue)
                            {
                                var spellSlot = Player.Instance.InventoryItems.FirstOrDefault(a => a.Id == item.Id);
                                if (spellSlot != null && Player.GetSpell(spellSlot.SpellSlot).IsReady)
                                {
                                    Player.CastSpell(spellSlot.SpellSlot, Player.Instance);
                                }
                            }
                        }
                    }
                        break;
                    case ItemType.Defensive:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (!Cleansers["summonerSpellCleanse"].Cast<CheckBox>().CurrentValue || Cleanse == null ||
                !Cleanse.IsReady()) return;
            {
                foreach (var buffInstance in Player.Instance.Buffs.Where(buffInstance => BuffTypes.ContainsKey(buffInstance.Type) && Cleansers[BuffTypes[buffInstance.Type]].Cast<CheckBox>().CurrentValue))
                {
                    Player.CastSpell(Cleanse.Slot);
                }
            }
        }

        public static bool HasBuff(this Obj_AI_Base unit, string s)
        {
            return unit.Buffs.Any(a => a.Name.ToLower().Contains(s.ToLower()) || a.DisplayName.ToLower().Contains(s.ToLower()));
        }
    }
}