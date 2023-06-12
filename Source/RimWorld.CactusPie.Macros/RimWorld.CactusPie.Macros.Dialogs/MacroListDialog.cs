using System.Collections.Generic;
using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Interfaces;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.CactusPie.Macros.Dialogs;

public class MacroListDialog : Window
{
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

    private const float MacroEditButtonHeight = 36f;

    private const float MacroEditButtonWidth = 70f;

    private const float MacroCopyButtonWidth = 70f;

    private const float MacroCopyButtonHeight = 36f;

    private const float ScrollBarWidth = 20f;

    private const float ReorderButtonWidth = 24f;

    private const float ReorderButtonHeight = 18f;

    private readonly IMacroClipboard _macroClipboard;
    private readonly IMacroManager _macroManager;

    private readonly Pawn _pawn;

    private Vector2 _scrollPosition = Vector2.zero;

    public MacroListDialog(IMacroManager macroManager, IMacroClipboard macroClipboard, Pawn pawn)
    {
        _macroManager = macroManager;
        _macroClipboard = macroClipboard;
        _pawn = pawn;
    }

    public override Vector2 InitialSize => new Vector2(650f, 650f);

    public override void DoWindowContents(Rect inRect)
    {
        RenderMacroListScrollView(inRect);
        if (_macroClipboard.HasCopiedMacro())
        {
            var pastePawnMacroRect = RenderPastePawnMacroButton(inRect);
            RenderPasteSharedMacroButton(inRect, pastePawnMacroRect);
        }

        RenderOkButton(inRect);
    }

    private void RenderMacroListScrollView(Rect inRect)
    {
        Text.Font = GameFont.Small;
        var pawnMacros = _macroManager.GetPawnMacros();
        var sharedMacros = _macroManager.GetSharedMacros();
        var scrollViewRowSize = new RowSize(inRect.width - 16f, 40f);
        var height = (pawnMacros.Count + sharedMacros.Count) * scrollViewRowSize.Height;
        var viewRect = new Rect(0f, 0f, inRect.width - 16f, height);
        var outRect =
            GenUI.TopPartPixels(
                height: !_macroClipboard.HasCopiedMacro()
                    ? inRect.height - ButtonHeight - ButtonBottomMargin - ScrollViewBottomMargin
                    : inRect.height - 100f - ScrollViewBottomMargin, rect: inRect);
        var rowYPosition = 0f;
        var renderedMacroCount = 0;
        Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);
        RenderRows(ref renderedMacroCount, ref rowYPosition, scrollViewRowSize, pawnMacros, false);
        RenderRows(ref renderedMacroCount, ref rowYPosition, scrollViewRowSize, sharedMacros, true);
        Widgets.EndScrollView();
    }

    private void RenderRows(ref int renderedMacroCount, ref float rowYPosition, RowSize scrollViewRowSize,
        IList<Macro> macros, bool isShared)
    {
        for (var i = 0; i < macros.Count; i++)
        {
            RenderMacroRow(ref rowYPosition, scrollViewRowSize, renderedMacroCount, i, macros, isShared);
            renderedMacroCount++;
        }
    }

    private void RenderMacroRow(ref float rowYPosition, RowSize rowSize, int renderedMacroCount, int listIndex,
        IList<Macro> macros, bool isShared)
    {
        var macro = macros[listIndex];
        GUI.color = Color.white;
        var rect = new Rect(0f, rowYPosition, rowSize.Width, rowSize.Height);
        if (renderedMacroCount % 2 == 0)
        {
            Widgets.DrawAltRect(rect);
        }

        GUI.BeginGroup(rect);
        var rowBaselineYPosition = (rect.height - 36f) / 2f;
        var deleteButtonRect = RenderDeleteButton(macro, rect, rowBaselineYPosition);
        var editButtonRect = RenderEditMacroButton(macro, isShared, deleteButtonRect, rowBaselineYPosition);
        RenderCopyMacroButton(macro, editButtonRect, rowBaselineYPosition);
        RenderMacroNameLabel(macro, rowBaselineYPosition, isShared, rect);
        RenderReorderButtons(listIndex, macros, rowSize);
        Text.Anchor = TextAnchor.UpperLeft;
        rowYPosition += rowSize.Height;
        GUI.EndGroup();
    }

    private void RenderOkButton(Rect inRect)
    {
        GUI.color = Color.white;
        var width = inRect.width - 16f;
        var y = inRect.height - ButtonHeight - ButtonBottomMargin;
        if (Widgets.ButtonText(new Rect(WindowLeftAndRightPadding, y, width, ButtonHeight), "OK".Translate()))
        {
            Find.WindowStack.TryRemove(this);
        }
    }

    private Rect RenderPastePawnMacroButton(Rect inRect)
    {
        GUI.color = Color.white;
        var width = ((inRect.width - 16f) / 2f) - PasteMacroButtonMargin;
        var y = inRect.height - 70f - 30f;
        var rect = new Rect(WindowLeftAndRightPadding, y, width, ButtonHeight);
        if (Widgets.ButtonText(rect, "Macros_List_Dialog_Paste_Pawn_Macro".Translate(), true, true,
                _macroClipboard.HasCopiedMacro()))
        {
            _macroClipboard.PasteMacroForPawn(_pawn.ThingID);
        }

        return rect;
    }

    private void RenderPasteSharedMacroButton(Rect inRect, Rect pastePawnMacroRect)
    {
        GUI.color = Color.white;
        var width = (inRect.width - 16f) / 2f;
        var y = pastePawnMacroRect.y;
        var x = WindowLeftAndRightPadding + pastePawnMacroRect.width + PasteMacroButtonMargin;
        if (Widgets.ButtonText(new Rect(x, y, width, ButtonHeight), "Macros_List_Dialog_Paste_Shared_Macro".Translate(),
                true,
                true, _macroClipboard.HasCopiedMacro()))
        {
            _macroClipboard.PasteSharedMacro();
        }
    }

    private static void RenderMacroNameLabel(Macro macro, float rowBaselineYPosition, bool isShared, Rect rowRect)
    {
        var color = GUI.color;
        GUI.color = new Color(1f, 1f, 0.6f);
        var x = 37f;
        var width = rowRect.width - 70f - 36f - WindowLeftAndRightPadding;
        var rect = new Rect(x, rowBaselineYPosition, width, 36f);
        Text.Anchor = TextAnchor.MiddleLeft;
        Text.Font = GameFont.Small;
        var label = !isShared
            ? macro.Name
            : string.Format(arg1: "Macros_Name_Shared".Translate(), format: "{0} ({1})", arg0: macro.Name);
        Widgets.Label(rect, label);
        GUI.color = color;
    }

    private static void RenderReorderButtons(int index, IList<Macro> macros, RowSize rowSize)
    {
        var baseColor = new Color(1f, 0.7f, 0.7f, 0.7f);
        var num = (rowSize.Height - 36f) / 2f;
        if (index > 0 && Widgets.ButtonImage(new Rect(RowLeftPadding, num, 24f, ReorderButtonHeight),
                TexButton.ReorderUp, baseColor))
        {
            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            SwapMacros(macros, index, index - 1);
        }

        if (index >= macros.Count - 1)
        {
            return;
        }

        var y = RowTopBottomPadding + num;
        if (!Widgets.ButtonImage(new Rect(RowLeftPadding, y, 24f, ReorderButtonHeight), TexButton.ReorderDown,
                baseColor))
        {
            return;
        }

        SoundDefOf.Tick_Low.PlayOneShotOnCamera();
        SwapMacros(macros, index, index + 1);
    }

    private Rect RenderEditMacroButton(Macro macro, bool isShared, Rect deleteButtonRect, float rowBaselineYPosition)
    {
        Text.Font = GameFont.Small;
        var rect = new Rect(deleteButtonRect.x - 70f, rowBaselineYPosition, 70f, 36f);
        if (!Widgets.ButtonText(rect, "Macros_List_Dialog_Edit".Translate()))
        {
            return rect;
        }

        var editMacroDialog = new EditMacroDialog(_macroManager, isShared, macro.Name);
        editMacroDialog.MacroDetailsChanged += delegate(string newName, bool newIsShared)
        {
            _macroManager.EditMacro(macro.Id, newName, newIsShared);
        };
        Find.WindowStack.Add(editMacroDialog);

        return rect;
    }

    private void RenderCopyMacroButton(Macro macro, Rect editButtonRect, float rowBaselineYPosition)
    {
        Text.Font = GameFont.Small;
        var x = editButtonRect.x - 70f;
        if (Widgets.ButtonText(new Rect(x, rowBaselineYPosition, 70f, 36f), "Macros_List_Dialog_Copy".Translate()))
        {
            _macroClipboard.CopyMacro(macro);
        }
    }

    private Rect RenderDeleteButton(Macro macro, Rect rowRect, float rowBaselineYPosition)
    {
        var x = rowRect.width - 16f - 20f;
        var rect = new Rect(x, rowBaselineYPosition, 36f, 36f);
        if (!Widgets.ButtonImage(rect, TexButton.DeleteX, Color.white, GenUI.SubtleMouseoverColor))
        {
            return rect;
        }

        var window =
            Dialog_MessageBox.CreateConfirmation("Macros_Delete_Confirmation".Translate(macro.Name), OnClick, true);
        Find.WindowStack.Add(window);

        return rect;

        void OnClick()
        {
            _macroManager.DeleteMacro(macro.Id);
        }
    }

    private static void SwapMacros(IList<Macro> macros, int index1, int intex2)
    {
        (macros[index1], macros[intex2]) = (macros[intex2], macros[index1]);
    }
}