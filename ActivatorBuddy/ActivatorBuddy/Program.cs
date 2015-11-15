using System;
using ActivatorBuddy.Defencives;
using ActivatorBuddy.Items;
using ActivatorBuddy.Summoner_Spells;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;

namespace ActivatorBuddy
{
    internal static class Program
    {
        public static Menu Menu;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Menu = MainMenu.AddMenu("Activator", "activatorMenu");
            ItemManager.Init();
            SummonerSpells.Init();
            Defence.Init();
        }
    }
}