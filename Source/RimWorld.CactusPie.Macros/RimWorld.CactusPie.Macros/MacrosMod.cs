using System;
using System.Reflection;
using HarmonyLib;
using HugsLib;
using RimWorld.CactusPie.Macros.Data;
using Verse;

namespace RimWorld.CactusPie.Macros;

public class MacrosMod : ModBase
{
    public MacrosMod()
    {
        Log.Message("[MACROS] Mod loaded");
        new Harmony("RimWorld.CactusPie.Macros").PatchAll(Assembly.GetExecutingAssembly());
    }

    internal static SaveData SaveData { get; private set; }

    public static event EventHandler WorldLoadedEvent;

    public override void WorldLoaded()
    {
        SaveData = Find.World.GetComponent<SaveData>();
        WorldLoadedEvent?.Invoke(this, EventArgs.Empty);
    }
}