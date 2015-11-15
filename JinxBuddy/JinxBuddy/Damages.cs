using EloBuddy;

namespace JinxBuddy
{
    internal static class Damages
    {
        public static int WDamage(Obj_AI_Base target)
        {
            return
                (int)
                    (new int[] {10, 60, 110, 160, 210}[Program.W.Level - 1] +
                     1.4*(ObjectManager.Player.TotalAttackDamage));
        }

        public static int EDamage(Obj_AI_Base target)
        {
            return
                (int)
                    (new double[] { 80, 135, 190, 245, 300 }[Program.E.Level -1]
                                    + 1 * ObjectManager.Player.FlatMagicDamageMod);
        }
    }
}
