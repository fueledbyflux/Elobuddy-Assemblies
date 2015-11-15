using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace VayneBuddy
{
    internal static class Events
    {
        public static Obj_AI_Base AAedTarget = null;
        public static long LastAa;
        public static int AaStacks;
        public static int LastR;

        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static bool HasR
        {
            get { return LastR > Environment.TickCount; }
        }

        public static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!Program.GapCloserMenu["enableGapCloser"].Cast<CheckBox>().CurrentValue) return;
            if (e.End.Distance(_Player) < 200 && sender.IsValidTarget())
            {
                Program.E.Cast(sender);
            }
        }

        public static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var rengar = EntityManager.Heroes.Enemies.FirstOrDefault(a => a.Hero == Champion.Rengar);
            if (!Program.DrawMenu["antiRengar"].Cast<CheckBox>().CurrentValue || sender.Name != "Rengar_LeapSound.troy" ||
                !(ObjectManager.Player.Distance(Player.Instance.Position) <= Program.E.Range) || rengar == null) return;
            Program.E.Cast(rengar);
            Console.WriteLine("fuck rengar");
        }

        public static void ObjAiBaseOnOnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!(sender is AIHeroClient)) return;
            var target = (AIHeroClient) sender;
            if (!Program.DrawMenu["antiKalista"].Cast<CheckBox>().CurrentValue || !target.IsEnemy ||
                target.Hero != Champion.Kalista || !Program.Q.IsReady()) return;
            var pos = _Player.Position.Extend(Game.CursorPos, 300).Distance(target) <=
                      _Player.GetAutoAttackRange(target) &&
                      _Player.Position.Extend(Game.CursorPos, 300).Distance(target) > 100
                ? Game.CursorPos
                : _Player.Position.Extend(target.Position, 300).Distance(target) < 100
                    ? target.Position
                    : new Vector3();

            if (pos.IsValid())
            {
                Player.CastSpell(SpellSlot.Q, pos);
            }
        }

        public static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (!Program.ComboMenu["useRCombo"].Cast<CheckBox>().CurrentValue || !target.IsValidTarget() ||
                !(target is AIHeroClient)) return;
            if (Program.ComboMenu["noRUnderTurret"].Cast<CheckBox>().CurrentValue &&
                ((AIHeroClient)target).IsUnderAlliedTurret())
            {
                return;
            }

            Program.R.Cast();
        }

        public static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (!Program.InterruptorMenu["enableInterrupter"].Cast<CheckBox>().CurrentValue) return;
            var dangerLevel =
                new[]
                {
                    DangerLevel.Low, DangerLevel.Medium,
                    DangerLevel.High,
                }[Program.InterruptorMenu["dangerLevel"].Cast<Slider>().CurrentValue - 1];

            if (dangerLevel == DangerLevel.Medium && e.DangerLevel.HasFlag(DangerLevel.High) ||
                dangerLevel == DangerLevel.Low && e.DangerLevel.HasFlag(DangerLevel.High) &&
                e.DangerLevel.HasFlag(DangerLevel.Medium))
                return;

            if (e.Sender.IsValidTarget())
            {
                Program.E.Cast(e.Sender);
            }
        }

    }
}
