using EloBuddy;

namespace YasuoBuddy
{
    internal static class EventManager
    {
        public static bool CanDash(this Obj_AI_Base unit)
        {
            return !unit.HasBuff("YasuoDashWrapper");
        }
    }
}