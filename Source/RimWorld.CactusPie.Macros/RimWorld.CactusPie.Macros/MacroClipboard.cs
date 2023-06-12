using System;
using System.Linq;
using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Interfaces;

namespace RimWorld.CactusPie.Macros;

public class MacroClipboard : IMacroClipboard
{
    private readonly IMacroCollection _macroCollection;

    private Macro _copiedMacro;

    public MacroClipboard(IMacroCollection macroCollection)
    {
        _macroCollection = macroCollection;
    }

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
        if (!_macroCollection.PawnMacroExists(pawnId, name) && !_macroCollection.SharedMacroExists(name))
        {
            var macro = _copiedMacro.Clone(Guid.NewGuid().ToString());
            _macroCollection.AddMacroForPawn(pawnId, macro);
            return;
        }

        var num = 1;
        while (true)
        {
            name = $"{_copiedMacro.Name} ({num})";
            if (!_macroCollection.PawnMacroExists(pawnId, name) && !_macroCollection.SharedMacroExists(name))
            {
                break;
            }

            num++;
        }

        var macro2 = _copiedMacro.Clone(Guid.NewGuid().ToString());
        macro2.Name = name;
        _macroCollection.AddMacroForPawn(pawnId, macro2);
    }

    public void PasteSharedMacro()
    {
        var name = _copiedMacro.Name;
        if (!_macroCollection.SharedMacroExists(name) && !_macroCollection.GetPawnIdsHavingMacroWithName(name).Any())
        {
            var macro = _copiedMacro.Clone(Guid.NewGuid().ToString());
            _macroCollection.AddSharedMacro(macro);
            return;
        }

        var num = 1;
        while (true)
        {
            name = $"{_copiedMacro.Name} ({num})";
            if (!_macroCollection.SharedMacroExists(name) &&
                !_macroCollection.GetPawnIdsHavingMacroWithName(name).Any())
            {
                break;
            }

            num++;
        }

        var macro2 = _copiedMacro.Clone(Guid.NewGuid().ToString());
        macro2.Name = name;
        _macroCollection.AddSharedMacro(macro2);
    }
}