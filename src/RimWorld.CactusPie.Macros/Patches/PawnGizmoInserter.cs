using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RimWorld.CactusPie.Macros.Patches 
{
	[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
	internal static class PawnGizmoInserter
	{
		private static MacroClipboard _macroClipboard = new MacroClipboard(MacrosMod.SaveData.MacroCollection);

		static PawnGizmoInserter()
		{
			MacrosMod.WorldLoadedEvent += OnWorldLoaded;	
		}

		[HarmonyPostfix]
		public static void InsertMacroGizmo(Pawn __instance, ref IEnumerable<Gizmo> __result)
		{
			__result = GetGizmos(__result, __instance);
		}

		private static IEnumerable<Gizmo> GetGizmos(IEnumerable<Gizmo> gizmos, Pawn pawn)
		{
			foreach (Gizmo gizmo in gizmos)
			{
				yield return gizmo;
			}

			var macroManager = new MacroManager(MacrosMod.SaveData.MacroCollection, pawn);

			if (!macroManager.CanExecuteMacrosForPawn(pawn))
			{
				yield break;			
			}

			yield return new MacroGizmo
			(
				new MacroManager(MacrosMod.SaveData.MacroCollection, pawn), 
				_macroClipboard,
				pawn
			)
			{
				defaultLabel = "Macros_Gizmo_Label".Translate(),
				defaultDesc = "Macros_Gizmo_Description".Translate(),
				hotKey = KeyBindingDef.Named("MACROS_HOTKEY_GIZMO"),
				icon = ContentFinder<Texture2D>.Get("MacroGizmoIcon"),
				activateSound = SoundDefOf.Tick_Tiny,
			};
		}

		private static void OnWorldLoaded(object sender, EventArgs e)
		{
			_macroClipboard = new MacroClipboard(MacrosMod.SaveData.MacroCollection);
		}
	}
}