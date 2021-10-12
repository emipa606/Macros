using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Interfaces;
using RimWorld.CactusPie.Macros.Ui;
using UnityEngine;
using Verse;

namespace RimWorld.CactusPie.Macros.Dialogs
{
    public class EditMacroDialog : Window
    {
        private readonly IMacroManager _macroManager;

        private string _currentName;
        private bool _isShared;
        private bool _isTextFieldFocused;
        private const string MacroNameFieldName = "MacroName";

        private const int MinimumNameLength = 1;
        private const int MaximumNameLength = 30;

        public override Vector2 InitialSize => new Vector2(500f, 200f);

        public delegate void MacroDetailsChangedEventHandler(string newName, bool isShared);
        public event MacroDetailsChangedEventHandler MacroDetailsChanged;
        
        public EditMacroDialog(IMacroManager macroManager, bool isShared = false, string macroName = null)
        {
            _macroManager = macroManager;
            _isShared = isShared;
            _currentName = macroName ?? "Macros_Add_Dialog_Default_Name".Translate();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                Event.current.Use();
                OnAcceptedChanges();
                return;
            }

            float yPositionOffset = 0;
            
            RenderEnterMacroNameLabel(inRect, ref yPositionOffset);
            RenderMacroNameTextField(inRect, ref yPositionOffset);
            RenderIsSharedCheckbox(ref yPositionOffset);
            RenderButtons(inRect);
        }

        private void RenderButtons(Rect inRect)
        {
            const float buttonMargin = 10.0f;
            const float buttonHeight = 35f;
            const float bottomMargin = 35f;
            
            float buttonWidth = (inRect.width - buttonMargin) / 2.0f;
            float buttonYPosition = inRect.height - bottomMargin;
            
            var okButtonRect = new Rect(0f, buttonYPosition, buttonWidth, buttonHeight);
            
            if (Widgets.ButtonText(okButtonRect, "OK".Translate()))
            {
                OnAcceptedChanges();
                return;
            }

            var cancelButtonRect = new Rect(buttonWidth + buttonMargin, buttonYPosition, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(cancelButtonRect, "Cancel".Translate()))
            {
                OnCanceled();
                return;
            }
        }

        private void RenderIsSharedCheckbox(ref float yPositionOffset)
        {
            var checkboxRect = new Rect(0f, yPositionOffset, 230f, 35f);
            CustomWidgets.CheckboxLabeled(checkboxRect, "Macros_Edit_Dialog_Is_Shared".Translate(), ref _isShared);
            Text.Anchor = TextAnchor.UpperLeft;
            yPositionOffset += checkboxRect.height;
        }

        private void RenderMacroNameTextField(Rect inRect, ref float yPositionOffset)
        {
            GUI.SetNextControlName(MacroNameFieldName);
            var textBoxRect = new Rect(0f, yPositionOffset, inRect.width, 35f);
            string newName = Widgets.TextField(textBoxRect, _currentName, MaximumNameLength);

            if (newName.Length <= MaximumNameLength)
            {
                _currentName = newName;
            }

            if (!_isTextFieldFocused)
            {
                UI.FocusControl(MacroNameFieldName, this);
                _isTextFieldFocused = true;
            }

            yPositionOffset += textBoxRect.height;
        }

        private static void RenderEnterMacroNameLabel(Rect inRect, ref float yPositionOffset)
        {
            var enterMacroNameLabelRect = new Rect(0f, yPositionOffset, inRect.width, 35f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;
            Widgets.Label(enterMacroNameLabelRect, "Macros_Add_Dialog_Name_Label".Translate());

            yPositionOffset += enterMacroNameLabelRect.height;
        }

        protected virtual void OnMacroNameChanged(string newName, bool newIsShared)
        {
            MacroDetailsChangedEventHandler handler = MacroDetailsChanged;

            if (handler != null)
            {
                MacroDetailsChanged?.Invoke(newName, newIsShared);
            }
        }

        private void OnAcceptedChanges()
        {
            MacroValidationData macroValidationData = IsMacroNameValid();

            if (!macroValidationData.IsMacroNameValid)
            {
                Messages.Message(macroValidationData.Message, MessageTypeDefOf.RejectInput, false);
            }
            else
            {
                OnMacroNameChanged(_currentName, _isShared);
                Find.WindowStack.TryRemove(this);
            }
        }

        private void OnCanceled()
        {
            Find.WindowStack.TryRemove(this);
        }

        private MacroValidationData IsMacroNameValid()
        {
            if (_currentName.Length < MinimumNameLength)
            {
                return new MacroValidationData
                {
                    IsMacroNameValid = false,
                    Message = "Macros_Name_Too_Short".Translate(MinimumNameLength)
                };
            }
            
            if (_currentName.Length > MaximumNameLength)
            {
                return new MacroValidationData
                {
                    IsMacroNameValid = false,
                    Message = "Macros_Name_Too_Long".Translate(MaximumNameLength)
                };
            }

            if (_macroManager.SharedMacroExists(_currentName))
            {
                return new MacroValidationData
                {
                    IsMacroNameValid = false,
                    Message = "Macros_Shared_Macro_Exists".Translate()
                };
            }

            if (_isShared)
            {
                Pawn pawnWithMacro = _macroManager.FindPlayerPawnHavingMacroName(_currentName);
                if (pawnWithMacro != null)
                {
                    return new MacroValidationData
                    {
                        IsMacroNameValid = false,
                        Message = "Macros_Named_Pawn_Macro_Exists".Translate(new NamedArgument(pawnWithMacro.Name, "PawnName"))
                    };
                }
            }
            else if (_macroManager.PawnMacroExistsForCurrentPawn(_currentName))
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
}