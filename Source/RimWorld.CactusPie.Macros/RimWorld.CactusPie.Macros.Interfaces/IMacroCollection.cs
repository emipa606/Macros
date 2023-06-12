using System.Collections.Generic;
using RimWorld.CactusPie.Macros.Data;

namespace RimWorld.CactusPie.Macros.Interfaces;

public interface IMacroCollection
{
    void AddMacroForPawn(string pawnId, Macro macro);

    void AddSharedMacro(Macro macro);

    IList<Macro> GetPawnMacros(string pawnId);

    IEnumerable<string> GetPawnIdsHavingMacroWithName(string macroName);

    IList<Macro> GetSharedMacros();

    IEnumerable<Macro> GetPawnAndSharedMacros(string pawnId);

    void DeletePawnMacroById(string macroId, string pawnId);

    void DeleteSharedMacroById(string macroId);

    bool PawnMacroExists(string pawnId, string macroName);

    bool SharedMacroExists(string macroName);
}