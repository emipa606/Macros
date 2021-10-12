using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.CactusPie.Macros.Ui
{
    public static class CustomWidgets
    {
        // The default CheckboxLabeled widget doesn't align the checkbox with
        // text correctly, so I made my own widget
        public static void CheckboxLabeled(Rect rect, string label, ref bool isChecked)
        {
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            
            Widgets.Label(rect, label);
            
            if (Widgets.ButtonInvisible(rect))
            {
                isChecked = !isChecked;
                if (isChecked)
                {
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                }
                else
                {
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                }
            }

            float textWidth = Text.CalcSize(label).x;
            const float checkboxSize = 24f;
            float checkboxYPosition = rect.y + ((rect.height - checkboxSize) / 2f);
            RenderCheckbox(rect.x + textWidth + 10f, checkboxYPosition, isChecked, checkboxSize);
            Text.Anchor = anchor;
        }
        
        private static void RenderCheckbox(float x, float y, bool isChecked, float size)
        {
            Texture texture = isChecked ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;
            var position = new Rect(x, y, size, size);
            GUI.DrawTexture(position, texture);
        }

    }
}