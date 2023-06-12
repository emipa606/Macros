using System;
using System.Collections.Generic;
using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Dialogs;
using RimWorld.CactusPie.Macros.Interfaces;
using UnityEngine;
using Verse;

namespace RimWorld.CactusPie.Macros;

public class MacroGizmo : Command_Action
{
    private readonly IMacroClipboard _macroClipboard;

    private readonly IMacroManager _macroManager;

    private readonly Pawn _pawn;

    public MacroGizmo(IMacroManager macroManager, IMacroClipboard macroClipboard, Pawn pawn)
    {
        _macroManager = macroManager;
        _macroClipboard = macroClipboard;
        _pawn = pawn;
        alsoClickIfOtherInGroupClicked = false;
    }

    public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => GetFloatMenuOptions();

    public override bool InheritFloatMenuInteractionsFrom(Gizmo other)
    {
        return true;
    }

    public override void ProcessInput(Event ev)
    {
        var canCreateMacroState = _macroManager.CanCreateMacro();
        if (!canCreateMacroState.CanCreateMacro)
        {
            Messages.Message(canCreateMacroState.Message, MessageTypeDefOf.RejectInput, false);
            return;
        }

        var editMacroDialog = new EditMacroDialog(_macroManager);
        editMacroDialog.MacroDetailsChanged += delegate(string name, bool isShared)
        {
            _macroManager.CreateNewMacro(name, isShared);
        };
        Find.WindowStack.Add(editMacroDialog);
    }

    private IEnumerable<FloatMenuOption> GetFloatMenuOptions()
    {
        var pawnMacros = _macroManager.GetPawnMacros();
        var addedMacro = false;
        GetMacroDelegates(out var executeMacro, out var formatMacroName);
        if (pawnMacros != null)
        {
            foreach (var pawnMacro in pawnMacros)
            {
                yield return new FloatMenuOption(formatMacroName(pawnMacro, false),
                    delegate { executeMacro(pawnMacro); });
                addedMacro = true;
            }
        }

        pawnMacros = _macroManager.GetSharedMacros();
        if (pawnMacros != null)
        {
            foreach (var sharedMacro in pawnMacros)
            {
                yield return new FloatMenuOption(formatMacroName(sharedMacro, true),
                    delegate { executeMacro(sharedMacro); });
                addedMacro = true;
            }
        }

        if (addedMacro || _macroClipboard.HasCopiedMacro())
        {
            yield return new FloatMenuOption("Macros_Gizmo_List".Translate(), OpenMacroList);
        }
    }

    private void GetMacroDelegates(out ExecuteMacroDelegate executeMacroDelegate,
        out FormatMacroNameDelegate formatMacroNameDelegate)
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftAlt))
            {
                GetMacroDelegatesForMatchingPawns(out executeMacroDelegate, out formatMacroNameDelegate,
                    ShouldClearCurrentJobs);
            }
            else
            {
                GetMacroDelegatesForAllPawns(out executeMacroDelegate, out formatMacroNameDelegate,
                    ShouldClearCurrentJobs);
            }
        }
        else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftAlt))
        {
            GetMacroDelegatesForSelectedPawns(out executeMacroDelegate, out formatMacroNameDelegate,
                ShouldClearCurrentJobs);
        }
        else
        {
            GetMacroDelegatesForSelectedMatchingPawns(out executeMacroDelegate, out formatMacroNameDelegate,
                ShouldClearCurrentJobs);
        }

        static bool ShouldClearCurrentJobs()
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                return !Input.GetKey(KeyCode.RightShift);
            }

            return false;
        }
    }

    private void GetMacroDelegatesForSelectedMatchingPawns(out ExecuteMacroDelegate executeMacroDelegate,
        out FormatMacroNameDelegate formatMacroNameDelegate, Func<bool> shouldClearCurrentJobs)
    {
        executeMacroDelegate = delegate(Macro macro)
        {
            _macroManager.ExecuteMatchingMacrosForSelectedPawns(macro.Name, shouldClearCurrentJobs());
        };
        formatMacroNameDelegate = delegate(Macro macro, bool isShared)
        {
            if (!isShared)
            {
                return macro.Name ?? "";
            }

            var taggedString = "Macros_Name_Shared".Translate();
            return $"{macro.Name} ({taggedString})";
        };
    }

    private void GetMacroDelegatesForAllPawns(out ExecuteMacroDelegate executeMacroDelegate,
        out FormatMacroNameDelegate formatMacroNameDelegate, Func<bool> shouldClearCurrentJobs)
    {
        executeMacroDelegate = delegate(Macro macro)
        {
            _macroManager.ExecuteMacroForAllPawns(macro, shouldClearCurrentJobs());
        };
        formatMacroNameDelegate = delegate(Macro macro, bool isShared)
        {
            var taggedString = "Macros_Name_All_Pawns".Translate();
            if (!isShared)
            {
                return $"{macro.Name} ({taggedString})";
            }

            var taggedString2 = "Macros_Name_Shared".Translate();
            return $"{macro.Name} ({taggedString2}) ({taggedString})";
        };
    }

    private void GetMacroDelegatesForMatchingPawns(out ExecuteMacroDelegate executeMacroDelegate,
        out FormatMacroNameDelegate formatMacroNameDelegate, Func<bool> shouldClearCurrentJobs)
    {
        executeMacroDelegate = delegate(Macro macro)
        {
            _macroManager.ExecuteMatchingMacrosForAllPawns(macro.Name, shouldClearCurrentJobs());
        };
        formatMacroNameDelegate = delegate(Macro macro, bool isShared)
        {
            var taggedString = "Macros_Name_All_Matching".Translate();
            if (!isShared)
            {
                return $"{macro.Name} ({taggedString})";
            }

            var taggedString2 = "Macros_Name_Shared".Translate();
            return $"{macro.Name} ({taggedString2}) ({taggedString})";
        };
    }

    private void GetMacroDelegatesForSelectedPawns(out ExecuteMacroDelegate executeMacroDelegate,
        out FormatMacroNameDelegate formatMacroNameDelegate, Func<bool> shouldClearCurrentJobs)
    {
        executeMacroDelegate = delegate(Macro macro)
        {
            _macroManager.ExecuteMacroForSelectedPawns(macro, shouldClearCurrentJobs());
        };
        formatMacroNameDelegate = delegate(Macro macro, bool isShared)
        {
            var taggedString = "Macros_Name_Selected_Pawns".Translate();
            if (!isShared)
            {
                return $"{macro.Name} ({taggedString})";
            }

            var taggedString2 = "Macros_Name_Shared".Translate();
            return $"{macro.Name} ({taggedString2}) ({taggedString})";
        };
    }

    private void OpenMacroList()
    {
        var window = new MacroListDialog(_macroManager, _macroClipboard, _pawn);
        Find.WindowStack.Add(window);
    }

    private delegate string FormatMacroNameDelegate(Macro macro, bool isShared);

    private delegate void ExecuteMacroDelegate(Macro macro);
}