using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Interfaces;
using RimWorld.CactusPie.Macros.Ui;
using UnityEngine;
using Verse;

namespace RimWorld.CactusPie.Macros.Dialogs;

public class EditMacroDialog(IMacroManager macroManager, bool isShared = false, string macroName = null)
    : Window
{
    public delegate void MacroDetailsChangedEventHandler(string newName, bool isShared);

    private const string MacroNameFieldName = "MacroName";

    private const int MinimumNameLength = 1;

    private const int MaximumNameLength = 30;

    private string _currentName = macroName ?? "Macros_Add_Dialog_Default_Name".Translate();

    private bool _isTextFieldFocused;

    public override Vector2 InitialSize => new Vector2(500f, 200f);

    public event MacroDetailsChangedEventHandler MacroDetailsChanged;

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            Event.current.Use();
            OnAcceptedChanges();
            return;
        }

        var yPositionOffset = 0f;
        RenderEnterMacroNameLabel(inRect, ref yPositionOffset);
        RenderMacroNameTextField(inRect, ref yPositionOffset);
        RenderIsSharedCheckbox(ref yPositionOffset);
        RenderButtons(inRect);
    }

    private void RenderButtons(Rect inRect)
    {
        var num = (inRect.width - 10f) / 2f;
        var y = inRect.height - 35f;
        if (Widgets.ButtonText(new Rect(0f, y, num, 35f), "OK".Translate()))
        {
            OnAcceptedChanges();
        }
        else if (Widgets.ButtonText(new Rect(num + 10f, y, num, 35f), "Cancel".Translate()))
        {
            OnCanceled();
        }
    }

    private void RenderIsSharedCheckbox(ref float yPositionOffset)
    {
        var rect = new Rect(0f, yPositionOffset, 230f, 35f);
        CustomWidgets.CheckboxLabeled(rect, "Macros_Edit_Dialog_Is_Shared".Translate(), ref isShared);
        Text.Anchor = TextAnchor.UpperLeft;
        yPositionOffset += rect.height;
    }

    private void RenderMacroNameTextField(Rect inRect, ref float yPositionOffset)
    {
        GUI.SetNextControlName("MacroName");
        var rect = new Rect(0f, yPositionOffset, inRect.width, 35f);
        var text = Widgets.TextField(rect, _currentName, 30);
        if (text.Length <= 30)
        {
            _currentName = text;
        }

        if (!_isTextFieldFocused)
        {
            UI.FocusControl("MacroName", this);
            _isTextFieldFocused = true;
        }

        yPositionOffset += rect.height;
    }

    private static void RenderEnterMacroNameLabel(Rect inRect, ref float yPositionOffset)
    {
        var rect = new Rect(0f, yPositionOffset, inRect.width, 35f);
        Text.Anchor = TextAnchor.MiddleLeft;
        Text.Font = GameFont.Small;
        Widgets.Label(rect, "Macros_Add_Dialog_Name_Label".Translate());
        yPositionOffset += rect.height;
    }

    protected virtual void OnMacroNameChanged(string newName, bool newIsShared)
    {
        MacroDetailsChanged?.Invoke(newName, newIsShared);
    }

    private void OnAcceptedChanges()
    {
        var macroValidationData = IsMacroNameValid();
        if (!macroValidationData.IsMacroNameValid)
        {
            Messages.Message(macroValidationData.Message, MessageTypeDefOf.RejectInput, false);
            return;
        }

        OnMacroNameChanged(_currentName, isShared);
        Find.WindowStack.TryRemove(this);
    }

    private void OnCanceled()
    {
        Find.WindowStack.TryRemove(this);
    }

    private MacroValidationData IsMacroNameValid()
    {
        if (_currentName.Length < 1)
        {
            return new MacroValidationData
            {
                IsMacroNameValid = false,
                Message = "Macros_Name_Too_Short".Translate(1)
            };
        }

        if (_currentName.Length > 30)
        {
            return new MacroValidationData
            {
                IsMacroNameValid = false,
                Message = "Macros_Name_Too_Long".Translate(30)
            };
        }

        if (macroManager.SharedMacroExists(_currentName))
        {
            return new MacroValidationData
            {
                IsMacroNameValid = false,
                Message = "Macros_Shared_Macro_Exists".Translate()
            };
        }

        if (isShared)
        {
            var pawn = macroManager.FindPlayerPawnHavingMacroName(_currentName);
            if (pawn != null)
            {
                return new MacroValidationData
                {
                    IsMacroNameValid = false,
                    Message = "Macros_Named_Pawn_Macro_Exists".Translate(new NamedArgument(pawn.Name, "PawnName"))
                };
            }
        }
        else if (macroManager.PawnMacroExistsForCurrentPawn(_currentName))
        {
            return new MacroValidationData
            {
                IsMacroNameValid = false,
                Message = "Macros_Pawn_Macro_Exists".Translate()
            };
        }

        return new MacroValidationData
        {
            IsMacroNameValid = true
        };
    }
}