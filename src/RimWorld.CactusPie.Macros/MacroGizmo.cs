using System;
using System.Collections.Generic;
using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Dialogs;
using RimWorld.CactusPie.Macros.Interfaces;
using Verse;
using UnityEngine;

namespace RimWorld.CactusPie.Macros
{
    /// <summary>
    /// Macro button appearing when selecting a colonist
    /// </summary>
    public class MacroGizmo : Command_Action 
    {
        private readonly IMacroManager _macroManager;
        private readonly IMacroClipboard _macroClipboard;
        private readonly Pawn _pawn;

        private delegate string FormatMacroNameDelegate(Macro macro, bool isShared);
        private delegate void ExecuteMacroDelegate(Macro macro);

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => GetFloatMenuOptions();

        public override bool InheritFloatMenuInteractionsFrom(Gizmo other)
        {
            return true;
        }

        public MacroGizmo(IMacroManager macroManager, IMacroClipboard macroClipboard, Pawn pawn)
        {
            _macroManager = macroManager;
            _macroClipboard = macroClipboard;
            _pawn = pawn;
            alsoClickIfOtherInGroupClicked = false;
        }

        // Executes on left click. If there are no available macros
        // it will also execute on right-click
        public override void ProcessInput(Event ev)
        {
            CanCreateMacroState canCreateMacroState = _macroManager.CanCreateMacro();
            if (!canCreateMacroState.CanCreateMacro)
            {
                Messages.Message(canCreateMacroState.Message, MessageTypeDefOf.RejectInput, false);
                return;
            }
            
            var dialog = new EditMacroDialog(_macroManager);
            
            dialog.MacroDetailsChanged += (string name, bool isShared) =>
            {
                _macroManager.CreateNewMacro(name, isShared);
            };
            
            Find.WindowStack.Add(dialog);
        }

        /// <returns>Returns right-click menu options for the Gizmo</returns>
        private IEnumerable<FloatMenuOption> GetFloatMenuOptions()
        {
            IList<Macro> macros = _macroManager.GetPawnMacros();
            bool atLeastOneMacroAvailable = false;

            GetMacroDelegates(out ExecuteMacroDelegate executeMacro, out FormatMacroNameDelegate formatMacroName);

            if (macros != null)
            {
                foreach (Macro pawnMacro in macros)
                {
                    yield return new FloatMenuOption(formatMacroName(pawnMacro, false), () => executeMacro(pawnMacro));
                    atLeastOneMacroAvailable = true;
                }
            }

            macros = _macroManager.GetSharedMacros();
            if (macros != null)
            {
                foreach (Macro sharedMacro in macros)
                {
                    yield return new FloatMenuOption(formatMacroName(sharedMacro, true), () => executeMacro(sharedMacro));
                    atLeastOneMacroAvailable = true;
                }
            }

            if (atLeastOneMacroAvailable || _macroClipboard.HasCopiedMacro())
            {
                yield return new FloatMenuOption("Macros_Gizmo_List".Translate(), OpenMacroList);
            }
        }

        private void GetMacroDelegates(out ExecuteMacroDelegate executeMacroDelegate, out FormatMacroNameDelegate formatMacroNameDelegate)
        {
            bool ShouldClearCurrentJobs() => !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftAlt))
                {
                    GetMacroDelegatesForMatchingPawns(out executeMacroDelegate, out formatMacroNameDelegate, ShouldClearCurrentJobs);
                    return;
                }

                GetMacroDelegatesForAllPawns(out executeMacroDelegate, out formatMacroNameDelegate, ShouldClearCurrentJobs);
            }
            else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftAlt))
            {
                GetMacroDelegatesForSelectedPawns(out executeMacroDelegate, out formatMacroNameDelegate, ShouldClearCurrentJobs);
            }
            else
            {
                GetMacroDelegatesForSelectedMatchingPawns(out executeMacroDelegate, out formatMacroNameDelegate, ShouldClearCurrentJobs);
            }
        }

        /// <summary>
        /// Generates delegates for executing macros in FloatMenuOptions
        /// and formatting their names. In this case when a macro is executed
        /// we will find and execute a macro with the same name on each of the
        /// selected pawns. If a pawn doesn't have a matching macro nothing will happen
        /// </summary>
        private void GetMacroDelegatesForSelectedMatchingPawns
        (
            out ExecuteMacroDelegate executeMacroDelegate, 
            out FormatMacroNameDelegate formatMacroNameDelegate,
            Func<bool> shouldClearCurrentJobs
        )
        {
            executeMacroDelegate = macro => _macroManager.ExecuteMatchingMacrosForSelectedPawns(macro.Name, shouldClearCurrentJobs());

            formatMacroNameDelegate = (macro, isShared) =>
            {
                if (isShared)
                {
                    TaggedString sharedLabel = "Macros_Name_Shared".Translate();
                    return $"{macro.Name} ({sharedLabel})";
                }

                return $"{macro.Name}";
            };
        }

        /// <summary>
        /// Generates delegates for executing macros in FloatMenuOptions
        /// and formatting their names. In this case when a macro is executed
        /// we will execute that macro for all player-controlled pawns
        /// </summary>
        private void GetMacroDelegatesForAllPawns
        (
            out ExecuteMacroDelegate executeMacroDelegate, 
            out FormatMacroNameDelegate formatMacroNameDelegate,
            Func<bool> shouldClearCurrentJobs
        )
        {
            executeMacroDelegate = macro => _macroManager.ExecuteMacroForAllPawns(macro, shouldClearCurrentJobs());
            formatMacroNameDelegate = (macro, isShared) =>
            {
                TaggedString allPawnsLabel = "Macros_Name_All_Pawns".Translate();

                if (isShared)
                {
                    TaggedString sharedLabel = "Macros_Name_Shared".Translate();
                    return $"{macro.Name} ({sharedLabel}) ({allPawnsLabel})";
                }

                return $"{macro.Name} ({allPawnsLabel})";
            };
        }

        /// <summary>
        /// Generates delegates for executing macros in FloatMenuOptions
        /// and formatting their names. In this case when a macro is executed
        /// we will find and execute a macro with the same name on each of the
        /// player controlled pawns - whether or not they are currently selected.
        /// If a pawn doesn't have a matching macro nothing will happen
        /// </summary>
        private void GetMacroDelegatesForMatchingPawns
        (
            out ExecuteMacroDelegate executeMacroDelegate, 
            out FormatMacroNameDelegate formatMacroNameDelegate,
            Func<bool> shouldClearCurrentJobs
        )
        {
            executeMacroDelegate = macro => _macroManager.ExecuteMatchingMacrosForAllPawns(macro.Name, shouldClearCurrentJobs());
            formatMacroNameDelegate = (macro, isShared) =>
            {
                TaggedString allMatchingLabel = "Macros_Name_All_Matching".Translate();

                if (isShared)
                {
                    TaggedString sharedLabel = "Macros_Name_Shared".Translate();
                    return $"{macro.Name} ({sharedLabel}) ({allMatchingLabel})";
                }

                return $"{macro.Name} ({allMatchingLabel})";
            };
        }

        /// <summary>
        /// Generates delegates for executing macros in FloatMenuOptions
        /// and formatting their names. In this case when a macro is executed
        /// we will execute this exact macro on all player controlled pawns.
        /// </summary>
        private void GetMacroDelegatesForSelectedPawns
        (
            out ExecuteMacroDelegate executeMacroDelegate, 
            out FormatMacroNameDelegate formatMacroNameDelegate,
            Func<bool> shouldClearCurrentJobs
        )
        {
            executeMacroDelegate = macro => _macroManager.ExecuteMacroForSelectedPawns(macro, shouldClearCurrentJobs());
            formatMacroNameDelegate = (macro, isShared) =>
            {
                TaggedString selectedPawnsLabel = "Macros_Name_Selected_Pawns".Translate();
                
                if (isShared)
                {
                    TaggedString sharedLabel = "Macros_Name_Shared".Translate();
                    return $"{macro.Name} ({sharedLabel}) ({selectedPawnsLabel})";
                }

                return $"{macro.Name} ({selectedPawnsLabel})";
            };
        }

        private void OpenMacroList()
        {
            var dialog = new MacroListDialog(_macroManager, _macroClipboard, _pawn);
            Find.WindowStack.Add(dialog);
        }
    }
}