using System;
using System.Reflection;
using HarmonyLib;
using HugsLib;
using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Patches;
using Verse;

namespace RimWorld.CactusPie.Macros
{
    public class MacrosMod : ModBase
    {
        internal static SaveData SaveData { get; private set; }
        public static event EventHandler WorldLoadedEvent;
        
        public MacrosMod()
        {
            Log.Message("[MACROS] Mod loaded");

            var harmony = new Harmony("RimWorld.CactusPie.Macros");
            
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override void WorldLoaded()
        {
            SaveData = Find.World.GetComponent<SaveData>();

            EventHandler handler = WorldLoadedEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}