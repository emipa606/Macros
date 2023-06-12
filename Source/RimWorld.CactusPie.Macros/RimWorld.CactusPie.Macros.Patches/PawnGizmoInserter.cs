using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RimWorld.CactusPie.Macros.Patches;

[HarmonyPatch(typeof(Pawn), "GetGizmos")]
internal static class PawnGizmoInserter
{
    private static MacroClipboard _macroClipboard;

    static PawnGizmoInserter()
    {
        _macroClipboard = new MacroClipboard(MacrosMod.SaveData.MacroCollection);
        MacrosMod.WorldLoadedEvent += OnWorldLoaded;
    }

    [HarmonyPostfix]
    public static void InsertMacroGizmo(Pawn __instance, ref IEnumerable<Gizmo> __result)
    {
        __result = GetGizmos(__result, __instance);
    }

    private static IEnumerable<Gizmo> GetGizmos(IEnumerable<Gizmo> gizmos, Pawn pawn)
    {
        foreach (var gizmo in gizmos)
        {
            yield return gizmo;
        }

        if (new MacroManager(MacrosMod.SaveData.MacroCollection, pawn).CanExecuteMacrosForPawn(pawn))
        {
            yield return new MacroGizmo(new MacroManager(MacrosMod.SaveData.MacroCollection, pawn), _macroClipboard,
                pawn)
            {
                defaultLabel = "Macros_Gizmo_Label".Translate(),
                defaultDesc = "Macros_Gizmo_Description".Translate(),
                hotKey = KeyBindingDef.Named("MACROS_HOTKEY_GIZMO"),
                icon = ContentFinder<Texture2D>.Get("MacroGizmoIcon"),
                activateSound = SoundDefOf.Tick_Tiny
            };
        }
    }

    private static void OnWorldLoaded(object sender, EventArgs e)
    {
        _macroClipboard = new MacroClipboard(MacrosMod.SaveData.MacroCollection);
    }
}