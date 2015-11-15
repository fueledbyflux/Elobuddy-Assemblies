using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace ActivatorBuddy.Defencives
{
    internal static class Defence
    {
        public static Dictionary<int, SpellDamageClass> Damages =
            new Dictionary<int, SpellDamageClass>();

        public static Menu DefenceMenu;
        private static Menu DamageEngine;

        private static List<SpellSlot> SpellSlots = new List<SpellSlot>()
        {
            SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R
        };

        public static int LastSpellCast;

        public static void Init()
        {
            DefenceMenu = Program.Menu.AddSubMenu("Defensive Items", "defmenuactiv");

            DefenceMenu.AddGroupLabel("Shield/Heal Items (self)");
            DefenceMenu.Add("Archangels_Staff", new CheckBox("Serahph's Embrace"));

            DefenceMenu.AddGroupLabel("Shield/Heal Items (ally/self)");
            DefenceMenu.Add("Mikaels_Crucible_Heal", new CheckBox("Mikaels Crucible"));
            DefenceMenu.AddLabel("Locket of the Iron Solari");
            DefenceMenu.Add("Locket_of_the_Iron_Solari", new CheckBox("Locket of the Iron Solari"));
            DefenceMenu.AddSeparator(0);
            DefenceMenu.Add("Locket_of_the_Iron_Solari_ally", new CheckBox("Ally"));
            DefenceMenu.Add("Locket_of_the_Iron_Solari_self", new CheckBox("Self"));
            DefenceMenu.AddLabel("Face of the Mountain");
            DefenceMenu.Add("Face_of_the_Mountain", new CheckBox("Face of the Mountain"));
            DefenceMenu.AddSeparator(0);
            DefenceMenu.Add("Face_of_the_Mountain_ally", new CheckBox("Ally"));
            DefenceMenu.Add("Face_of_the_Mountain_self", new CheckBox("Self"));

            DefenceMenu.AddGroupLabel("Cleanse Items (Dangerous Spells)");
            DefenceMenu.Add("Quicksilver_Sash", new CheckBox("Quicksilver Sash"));
            DefenceMenu.Add("Dervish_Blade", new CheckBox("Dervish Blade"));
            DefenceMenu.Add("Mercurial_Scimitar", new CheckBox("Mercurial Scimitar"));
            DefenceMenu.Add("Mikaels_Crucible_Cleanse", new CheckBox("Mikaels Crucible"));

            DefenceMenu.AddGroupLabel("Zhonyas Items");
            DefenceMenu.Add("Zhonyas_Hourglass", new CheckBox("Zhonyas Hourglass"));
            DefenceMenu.Add("Wooglets_Witchcap", new CheckBox("Wooglets Witchcap"));

            DefenceMenu.AddGroupLabel("Dangerous Spells");
            foreach (var dangerousSpell in DangerousSpells.Spells.Where(a => EntityManager.Heroes.Enemies.Any(b => b.Hero == a.Champion)))
            {
                DefenceMenu.Add(dangerousSpell.Champion.ToString() + dangerousSpell.Slot,
                    new CheckBox(dangerousSpell.Champion + ": " + dangerousSpell.Slot + (dangerousSpell.IsCleanseable ? " (Cleanseable)" : "")));
            }

            DamageEngine = Program.Menu.AddSubMenu("Damage Engine (Advanced)", "damageengineactiv");

            DamageEngine.AddGroupLabel("Danger Settings");
            DamageEngine.AddLabel("HP Tracking");
            DamageEngine.Add("HPDanger", new CheckBox("HP For Danger"));
            DamageEngine.Add("HPDangerSlider", new Slider("HP % For Danger", 15));
            DamageEngine.Add("EnemiesDanger", new CheckBox("Require Enemies"));
            DamageEngine.Add("EnemiesDangerSlider", new Slider("Enemies Around", 1, 1, 5));
            DamageEngine.Add("EnemiesDangerRange", new Slider("Range", 850, 1, 2000));
            DamageEngine.AddLabel("Damage Tracking");
            DamageEngine.Add("TrackDamage", new CheckBox("Track Incoming Damage"));
            DamageEngine.AddLabel("Please note, disabiling this and HP Danger will stop ALL defence modes.");
            DamageEngine.AddGroupLabel("Engine Settings");
            DamageEngine.Add("ConsiderSpells", new CheckBox("Consider Spells"));
            DamageEngine.Add("ConsiderSkillshots", new CheckBox("Consider Skillshots"));
            DamageEngine.Add("ConsiderTargeted", new CheckBox("Consider Targeted"));
            DamageEngine.Add("ConsiderAttacks", new CheckBox("Consider Basic Attacks"));
            DamageEngine.Add("ConsiderMinions", new CheckBox("Consider Non-Champions"));
            DamageEngine.Add("DisableExecuteCheck", new CheckBox("Disable Execute Check", false));


            foreach (var ally in EntityManager.Heroes.Allies)
            {
                Damages.Add(ally.NetworkId, new SpellDamageClass());
            }

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Base.OnBasicAttack += Obj_AI_Base_OnBasicAttack;
            Game.OnUpdate += GameOnOnUpdate;
        }

        private static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsAlly || !DamageEngine["TrackDamage"].Cast<CheckBox>().CurrentValue || !DamageEngine["ConsiderAttacks"].Cast<CheckBox>().CurrentValue || sender.IsMinion() && !DamageEngine["ConsiderMinions"].Cast<CheckBox>().CurrentValue) return;
            if (!Damages.ContainsKey(args.Target.NetworkId)) return;
            var target = (Obj_AI_Base) args.Target;
            Damages[args.Target.NetworkId].AddDamage(args.SData.Name, sender.GetAutoAttackDamage(target),
                (target.IsMelee ? sender.AttackDelay : target.Distance(sender)/args.SData.MissileSpeed)*1000);
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            if (LastSpellCast > Environment.TickCount) return;
            foreach (var ally in EntityManager.Heroes.Allies.Where(a => Damages.ContainsKey(a.NetworkId) && a.InDanger()))
            {
                if (DefenceItems.CleanseItems(ally)) return;
                if (DefenceItems.Zhonyas(ally)) return;
                if (DefenceItems.ShieldItems(ally)) return;
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var caster = sender as AIHeroClient;
            if (caster == null || sender.IsAlly || !SpellSlots.Contains(args.Slot) || !DamageEngine["ConsiderSpells"].Cast<CheckBox>().CurrentValue || !DamageEngine["TrackDamage"].Cast<CheckBox>().CurrentValue) return;
            foreach (var target in EntityManager.Heroes.Allies)
            {
                if (!Damages.ContainsKey(target.NetworkId)) return;

                var dangerSpell =
                        DangerousSpells.Spells.FirstOrDefault(
                            a => a.Champion == caster.Hero && args.Slot == a.Slot && DefenceMenu[a.Champion.ToString() + a.Slot].Cast<CheckBox>().CurrentValue);
                if (dangerSpell != null)
                {
                    Damages[target.NetworkId].DangerousSpells.Add(Environment.TickCount + (dangerSpell.BonusDelay > 0 ? dangerSpell.BonusDelay : 2000), dangerSpell);
                }

                if (args.Target != null && args.Target.NetworkId == target.NetworkId && DamageEngine["ConsiderTargeted"].Cast<CheckBox>().CurrentValue || DamageEngine["ConsiderSkillshots"].Cast<CheckBox>().CurrentValue && args.End != Vector3.Zero && args.End.Distance(target) < 200)
                {
                    Damages[target.NetworkId].AddDamage(args.SData.Name, caster.GetSpellDamage(target, args.Slot),
                        args.SData.MissileSpeed > 100 ? (target.Distance(sender)/args.SData.MissileSpeed)*1000 : 1500);
                }
            }
        }

        public static float PredictedHealth(this AIHeroClient target)
        {
            if (!Damages.ContainsKey(target.NetworkId)) return target.Health;
            return target.Health - Damages[target.NetworkId].Damage;
        }

        public static bool InDanger(this AIHeroClient target, bool execute = false)
        {
            if (!Damages.ContainsKey(target.NetworkId)) return false;
            if (execute && DamageEngine["DisableExecuteCheck"].Cast<CheckBox>().CurrentValue) execute = false;
            return Damages[target.NetworkId].Damage > target.Health 
                || !execute && DamageEngine["HPDanger"].Cast<CheckBox>().CurrentValue && target.HealthPercent <= DamageEngine["HPDangerSlider"].Cast<Slider>().CurrentValue && (!DamageEngine["EnemiesDanger"].Cast<CheckBox>().CurrentValue || target.CountEnemiesInRange(DamageEngine["EnemiesDangerRange"].Cast<Slider>().CurrentValue) >= DamageEngine["EnemiesDangerSlider"].Cast<Slider>().CurrentValue) 
                || (Damages[target.NetworkId].Damage/target.MaxHealth) > 0.15 && !execute 
                || target.NetworkId == Player.Instance.NetworkId && Damages[target.NetworkId].DangerousSpells.Any() && !execute;
        }
    }

    public class DamageSpell
    {
        public float Damage;
        public float EndTime;

        public DamageSpell(float endTime, float damage)
        {
            EndTime = (int) endTime;
            Damage = damage;
        }
    }

    public class SpellDamageClass
    {
        private readonly ConcurrentDictionary<string, DamageSpell> _damages =
            new ConcurrentDictionary<string, DamageSpell>();

        public Dictionary<int, DangerousSpell> DangerousSpells = new Dictionary<int, DangerousSpell>();

        public SpellDamageClass()
        {
            foreach (var source in DangerousSpells.ToList().Where(source => source.Key < Environment.TickCount))
            {
                DangerousSpells.Remove(source.Key);
            }
        }

        public float Damage
        {
            get
            {
                float dmg = 0;
                var damages = _damages;
                foreach (var damage in damages)
                {
                    if (damage.Value.EndTime < Environment.TickCount)
                    {
                        DamageSpell l;
                        _damages.TryRemove(damage.Key, out l);
                        continue;
                    }
                    dmg += damage.Value.Damage;
                }
                return dmg;
            }
        }

        public void AddDamage(string spellName, float dmg, float endTime = 1500)
        {
            if (_damages.ContainsKey(spellName))
            {
                _damages[spellName] = new DamageSpell(Environment.TickCount + endTime, dmg);
                return;
            }
            _damages.TryAdd(spellName, new DamageSpell(Environment.TickCount + endTime, dmg));
        }
    }
}