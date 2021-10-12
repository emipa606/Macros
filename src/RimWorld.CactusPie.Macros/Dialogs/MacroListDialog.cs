using System;
using System.Collections.Generic;
using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Interfaces;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.CactusPie.Macros.Dialogs
{
    public class MacroListDialog : Window
    {
        private readonly IMacroManager _macroManager;
        private readonly IMacroClipboard _macroClipboard;
        private readonly Pawn _pawn;
        private Vector2 _scrollPosition = Vector2.zero;
        
        private const float WindowLeftAndRightPadding = 8f;
        private const float ButtonHeight = 35f;
        private const float ButtonBottomMargin = 15f;
        private const float ScrollViewBottomMargin = 10f;
        private const float PasteMacroButtonMargin = 10f;
        
        private const float RowLeftPadding = 5f;
        private const float RowTopBottomPadding = 18f;
        private const float MacroNameLabelHeight = 36f;
        private const float DeleteButtonHeight = 36f;
        private const float DeleteButtonWidth = 36f;
        private const float MacroEditButtonHeight = DeleteButtonHeight;
        private const float MacroEditButtonWidth = 70f;
        private const float MacroCopyButtonWidth = MacroEditButtonWidth;
        private const float MacroCopyButtonHeight = MacroEditButtonHeight;
        private const float ScrollBarWidth = 20f;
        private const float ReorderButtonWidth = 24f;
        private const float ReorderButtonHeight = 18f;
        
        public override Vector2 InitialSize => new Vector2(650F, 650f);

        public MacroListDialog(IMacroManager macroManager, IMacroClipboard macroClipboard, Pawn pawn)
        {
            _macroManager = macroManager;
            _macroClipboard = macroClipboard;
            _pawn = pawn;
        }

        public override void DoWindowContents(Rect inRect)
        {
            RenderMacroListScrollView(inRect);

            if (_macroClipboard.HasCopiedMacro())
            {
                Rect pastePawnMacroButtonRect = RenderPastePawnMacroButton(inRect);
                RenderPasteSharedMacroButton(inRect, pastePawnMacroButtonRect);
            }
            
            RenderOkButton(inRect);
        }

        private void RenderMacroListScrollView(Rect inRect)
        {
            Text.Font = GameFont.Small;
            
            IList<Macro> pawnMacros = _macroManager.GetPawnMacros();
            IList<Macro> sharedMacros = _macroManager.GetSharedMacros();
            
            var scrollViewRowSize = new RowSize(inRect.width - (2f * WindowLeftAndRightPadding), 40f);
            
            float innerScrollViewHeight = (pawnMacros.Count + sharedMacros.Count) * scrollViewRowSize.Height;
            var innerScrollViewRect = new Rect(0f, 0f, inRect.width - (WindowLeftAndRightPadding * 2f), innerScrollViewHeight);

            // Actual height of the scroll view inside the dialog box (inside inRect)
            float outerScrollViewHeight;
            if (_macroClipboard.HasCopiedMacro())
            {
                outerScrollViewHeight = inRect.height - 2f * (ButtonHeight + ButtonBottomMargin) - ScrollViewBottomMargin;
            }
            else
            {
                outerScrollViewHeight = inRect.height - ButtonHeight - ButtonBottomMargin - ScrollViewBottomMargin;
            }

            Rect outerScrollViewRect = inRect.TopPartPixels(outerScrollViewHeight);
            
            float rowYPosition = 0.0f;
            int renderedMacroCount = 0; // number of all rendered macros (shared + non-shared)

            Widgets.BeginScrollView(outerScrollViewRect, ref _scrollPosition, innerScrollViewRect);

            RenderRows(ref renderedMacroCount, ref rowYPosition, scrollViewRowSize, pawnMacros, false);
            RenderRows(ref renderedMacroCount, ref rowYPosition, scrollViewRowSize, sharedMacros, true);

            Widgets.EndScrollView();
        }

        private void RenderRows(ref int renderedMacroCount, ref float rowYPosition, RowSize scrollViewRowSize, IList<Macro> macros, bool isShared)
        {
            for (var i = 0; i < macros.Count; i++)
            {
                RenderMacroRow(ref rowYPosition, scrollViewRowSize, renderedMacroCount, i, macros, isShared);
                renderedMacroCount++;
            }
        }

        /// <summary>
        /// Renders a macro row on the macro list inside the ScrollView
        /// </summary>
        /// <param name="rowYPosition">Y position of the row</param>
        /// <param name="rowSize">Struct containing row size (width and height)</param>
        /// <param name="renderedMacroCount">The number of already rendered macros (both shared and non-shared</param>
        /// <param name="listIndex">The index of the macro in the current list (separate for shared and non-shared)</param>
        /// <param name="macros">Currently processed macro list (either shared or non-shared)</param>
        /// <param name="isShared">True if it's a shared macro, false otherwise</param>
        private void RenderMacroRow
        (
            ref float rowYPosition, 
            RowSize rowSize,
            int renderedMacroCount, 
            int listIndex, 
            IList<Macro> macros,
            bool isShared
        )
        {
            Macro macro = macros[listIndex];
            
            GUI.color = Color.white;
            
            var rowRect = new Rect(0.0f, rowYPosition, rowSize.Width, rowSize.Height);

            if (renderedMacroCount % 2 == 0)
            {
                Widgets.DrawAltRect(rowRect);
            }
            
            GUI.BeginGroup(rowRect);

            float rowBaselineYPosition = (rowRect.height - 2 * RowTopBottomPadding) / 2.0f;
            
            Rect deleteButtonRect = RenderDeleteButton(macro, rowRect, rowBaselineYPosition);
            Rect macroEditButtonRect = RenderEditMacroButton(macro, isShared, deleteButtonRect, rowBaselineYPosition);
            RenderCopyMacroButton(macro, macroEditButtonRect, rowBaselineYPosition);
            RenderMacroNameLabel(macro, rowBaselineYPosition, isShared, rowRect);
            RenderReorderButtons(listIndex, macros, rowSize);

            Text.Anchor = TextAnchor.UpperLeft;
            rowYPosition += rowSize.Height;

            GUI.EndGroup();
        }

        private void RenderOkButton(Rect inRect)
        {
            GUI.color = Color.white;
            float buttonWidth = inRect.width - 2f * WindowLeftAndRightPadding;
            float buttonYPosition = inRect.height - ButtonHeight - ButtonBottomMargin;
            var okButtonRect = new Rect(WindowLeftAndRightPadding, buttonYPosition, buttonWidth, ButtonHeight);
            if (Widgets.ButtonText(okButtonRect, "OK".Translate()))
            {
                Find.WindowStack.TryRemove(this);
            }
        }
        
        private Rect RenderPastePawnMacroButton(Rect inRect)
        {
            GUI.color = Color.white;
            float buttonWidth = ((inRect.width - 2f * WindowLeftAndRightPadding) / 2f) - PasteMacroButtonMargin;
            float buttonYPosition = inRect.height - 2f * ButtonHeight - 2f * ButtonBottomMargin;
            var pasteButtonRect = new Rect(WindowLeftAndRightPadding, buttonYPosition, buttonWidth, ButtonHeight);
            if (Widgets.ButtonText(pasteButtonRect, "Macros_List_Dialog_Paste_Pawn_Macro".Translate(), active: _macroClipboard.HasCopiedMacro()))
            {
                _macroClipboard.PasteMacroForPawn(_pawn.ThingID);
            }

            return pasteButtonRect;
        }
        
        private void RenderPasteSharedMacroButton(Rect inRect, Rect pastePawnMacroRect)
        {
            GUI.color = Color.white;
            float buttonWidth =  (inRect.width - 2f * WindowLeftAndRightPadding) / 2f;
            float buttonYPosition = pastePawnMacroRect.y;
            float buttonXPosition = WindowLeftAndRightPadding + pastePawnMacroRect.width + PasteMacroButtonMargin;
            var pasteButtonRect = new Rect(buttonXPosition, buttonYPosition, buttonWidth, ButtonHeight);
            if (Widgets.ButtonText(pasteButtonRect, "Macros_List_Dialog_Paste_Shared_Macro".Translate(), active: _macroClipboard.HasCopiedMacro()))
            {
                _macroClipboard.PasteSharedMacro();
            }
        }

        private static void RenderMacroNameLabel(Macro macro, float rowBaselineYPosition, bool isShared, Rect rowRect)
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 0.6f);

            float macroNameLabelXPosition = WindowLeftAndRightPadding + RowLeftPadding + ReorderButtonWidth;
            float macroNameLabelWidth = rowRect.width - MacroEditButtonWidth - DeleteButtonWidth - WindowLeftAndRightPadding;

            var macroNameRect = new Rect(macroNameLabelXPosition, rowBaselineYPosition, macroNameLabelWidth, MacroNameLabelHeight);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;


            string macroName;
            if (isShared)
            {
                TaggedString sharedLabel = "Macros_Name_Shared".Translate();
                macroName = $"{macro.Name} ({sharedLabel})";
            }
            else
            {
                macroName = macro.Name;
            }
            
            Widgets.Label(macroNameRect, macroName);
            GUI.color = previousColor;
        }

        private static void RenderReorderButtons(int index, IList<Macro> macros, RowSize rowSize)
        {
            var color = new Color(1f, 0.7f, 0.7f, 0.7f);
            float topMargin = (rowSize.Height - 2f * ReorderButtonHeight) / 2f;

            if (index > 0)
            {
                var orderUpButtonRect = new Rect(RowLeftPadding, topMargin, ReorderButtonWidth, ReorderButtonHeight);
                if (Widgets.ButtonImage(orderUpButtonRect, TexButton.ReorderUp, color))
                {
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    SwapMacros(macros, index, index - 1);
                }
            }

            if (index < macros.Count - 1)
            {
                float orderDownButtonYPosition = ReorderButtonHeight + topMargin;
                var orderDownButtonRect = new Rect(RowLeftPadding, orderDownButtonYPosition, ReorderButtonWidth, ReorderButtonHeight);
                if (Widgets.ButtonImage(orderDownButtonRect, TexButton.ReorderDown, color))
                {
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    SwapMacros(macros, index, index + 1);
                }
            }
        }

        private Rect RenderEditMacroButton(Macro macro, bool isShared, Rect deleteButtonRect, float rowBaselineYPosition)
        {
            Text.Font = GameFont.Small;
            var macroEditButtonRect = new Rect(deleteButtonRect.x - MacroEditButtonWidth, rowBaselineYPosition, MacroEditButtonWidth, MacroEditButtonHeight);
            if (Widgets.ButtonText(macroEditButtonRect, "Macros_List_Dialog_Edit".Translate()))
            {
                var macroNameDialog = new EditMacroDialog(_macroManager, isShared, macro.Name);

                macroNameDialog.MacroDetailsChanged += (newName, newIsShared) =>
                {
                    _macroManager.EditMacro(macro.Id, newName, newIsShared);
                };

                Find.WindowStack.Add(macroNameDialog);
            }

            return macroEditButtonRect;
        }
        
        private void RenderCopyMacroButton(Macro macro, Rect editButtonRect, float rowBaselineYPosition)
        {
            Text.Font = GameFont.Small;

            float copyButtonXPosition = editButtonRect.x - MacroCopyButtonWidth;
            var macroCopyButtonRect = new Rect(copyButtonXPosition, rowBaselineYPosition, MacroCopyButtonWidth, MacroCopyButtonHeight);
            if (Widgets.ButtonText(macroCopyButtonRect, "Macros_List_Dialog_Copy".Translate()))
            {
                _macroClipboard.CopyMacro(macro);
            }
        }

        private Rect RenderDeleteButton(Macro macro, Rect rowRect, float rowBaselineYPosition)
        {
            float deleteButtonXPosition = rowRect.width - (2f * WindowLeftAndRightPadding) - ScrollBarWidth;
            var deleteButtonRect = new Rect(deleteButtonXPosition, rowBaselineYPosition, DeleteButtonWidth, DeleteButtonHeight);
            
            if (Widgets.ButtonImage(deleteButtonRect, TexButton.DeleteX, Color.white, GenUI.SubtleMouseoverColor))
            {
                void OnClick() => _macroManager.DeleteMacro(macro.Id);
                TaggedString message = "Macros_Delete_Confirmation".Translate(macro.Name);
                var confirmDeleteDialog = Dialog_MessageBox.CreateConfirmation(message, OnClick, true);
                Find.WindowStack.Add(confirmDeleteDialog);
            }

            return deleteButtonRect;
        }
        
        private static void SwapMacros(IList<Macro> macros, int index1, int intex2)
        {
            Macro tmp = macros[index1];
            macros[index1] = macros[intex2];
            macros[intex2] = tmp;
        }
    }
}