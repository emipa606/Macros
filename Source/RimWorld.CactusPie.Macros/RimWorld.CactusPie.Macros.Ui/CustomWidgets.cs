using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.CactusPie.Macros.Ui;

public static class CustomWidgets
{
    public static void CheckboxLabeled(Rect rect, string label, ref bool isChecked)
    {
        var anchor = Text.Anchor;
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

        var x = Text.CalcSize(label).x;
        RenderCheckbox(y: rect.y + ((rect.height - 24f) / 2f), x: rect.x + x + 10f, isChecked: isChecked, size: 24f);
        Text.Anchor = anchor;
    }

    private static void RenderCheckbox(float x, float y, bool isChecked, float size)
    {
        Texture image = isChecked ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;
        GUI.DrawTexture(new Rect(x, y, size, size), image);
    }
}