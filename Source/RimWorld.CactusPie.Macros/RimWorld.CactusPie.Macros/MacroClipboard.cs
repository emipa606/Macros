using System;
using System.Linq;
using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Interfaces;

namespace RimWorld.CactusPie.Macros;

public class MacroClipboard(IMacroCollection macroCollection) : IMacroClipboard
{
    private Macro _copiedMacro;

    public void CopyMacro(Macro macro)
    {
        _copiedMacro = macro.Clone(null);
    }

    public bool HasCopiedMacro()
    {
        return _copiedMacro != null;
    }

    public void PasteMacroForPawn(string pawnId)
    {
        var name = _copiedMacro.Name;
        if (!macroCollection.PawnMacroExists(pawnId, name) && !macroCollection.SharedMacroExists(name))
        {
            var macro = _copiedMacro.Clone(Guid.NewGuid().ToString());
            macroCollection.AddMacroForPawn(pawnId, macro);
            return;
        }

        var num = 1;
        while (true)
        {
            name = $"{_copiedMacro.Name} ({num})";
            if (!macroCollection.PawnMacroExists(pawnId, name) && !macroCollection.SharedMacroExists(name))
            {
                break;
            }

            num++;
        }

        var macro2 = _copiedMacro.Clone(Guid.NewGuid().ToString());
        macro2.Name = name;
        macroCollection.AddMacroForPawn(pawnId, macro2);
    }

    public void PasteSharedMacro()
    {
        var name = _copiedMacro.Name;
        if (!macroCollection.SharedMacroExists(name) && !macroCollection.GetPawnIdsHavingMacroWithName(name).Any())
        {
            var macro = _copiedMacro.Clone(Guid.NewGuid().ToString());
            macroCollection.AddSharedMacro(macro);
            return;
        }

        var num = 1;
        while (true)
        {
            name = $"{_copiedMacro.Name} ({num})";
            if (!macroCollection.SharedMacroExists(name) &&
                !macroCollection.GetPawnIdsHavingMacroWithName(name).Any())
            {
                break;
            }

            num++;
        }

        var macro2 = _copiedMacro.Clone(Guid.NewGuid().ToString());
        macro2.Name = name;
        macroCollection.AddSharedMacro(macro2);
    }
}