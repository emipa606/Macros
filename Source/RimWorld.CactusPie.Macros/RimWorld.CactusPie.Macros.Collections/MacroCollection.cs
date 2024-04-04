using System.Collections.Generic;
using System.Linq;
using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Interfaces;
using Verse;

namespace RimWorld.CactusPie.Macros.Collections;

public class MacroCollection : IMacroCollection, IExposable
{
    private Dictionary<string, ExposableList<Macro>> _pawnMacros;

    private List<Macro> _sharedMacros;

    public MacroCollection()
    {
        _pawnMacros = new Dictionary<string, ExposableList<Macro>>();
        _sharedMacros = [];
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref _sharedMacros, "Macros_sharedMacros", LookMode.Deep);
        Scribe_Collections.Look(ref _pawnMacros, "Macros_pawnMacros", LookMode.Value, LookMode.Deep);
    }

    public void AddMacroForPawn(string pawnId, Macro macro)
    {
        if (!_pawnMacros.TryGetValue(pawnId, out var value))
        {
            value = [];
            _pawnMacros.Add(pawnId, value);
        }

        value.Add(macro);
    }

    public void AddSharedMacro(Macro macro)
    {
        _sharedMacros.Add(macro);
    }

    public IList<Macro> GetPawnMacros(string pawnId)
    {
        if (_pawnMacros == null)
        {
            _pawnMacros = new Dictionary<string, ExposableList<Macro>>();
        }

        if (_pawnMacros.TryGetValue(pawnId, out var value))
        {
            return value;
        }

        value = [];
        _pawnMacros.Add(pawnId, value);

        return value;
    }

    public IEnumerable<string> GetPawnIdsHavingMacroWithName(string macroName)
    {
        if (_pawnMacros == null)
        {
            _pawnMacros = new Dictionary<string, ExposableList<Macro>>();
        }

        foreach (var pawnMacros in _pawnMacros)
        {
            foreach (var item in pawnMacros.Value)
            {
                if (item.Name == macroName)
                {
                    yield return pawnMacros.Key;
                }
            }
        }
    }

    public IList<Macro> GetSharedMacros()
    {
        return _sharedMacros;
    }

    public IEnumerable<Macro> GetPawnAndSharedMacros(string pawnId)
    {
        if (_pawnMacros.TryGetValue(pawnId, out var value))
        {
            foreach (var item in value)
            {
                yield return item;
            }
        }

        foreach (var sharedMacro in _sharedMacros)
        {
            yield return sharedMacro;
        }
    }

    public void DeletePawnMacroById(string macroId, string pawnId)
    {
        if (_pawnMacros.TryGetValue(pawnId, out var value))
        {
            DeleteMacroById(macroId, value);
        }
    }

    public void DeleteSharedMacroById(string macroId)
    {
        DeleteMacroById(macroId, _sharedMacros);
    }

    public bool PawnMacroExists(string pawnId, string macroName)
    {
        return GetPawnMacros(pawnId).Any(macro => macro.Name == macroName);
    }

    public bool SharedMacroExists(string macroName)
    {
        return GetSharedMacros().Any(macro => macro.Name == macroName);
    }

    private static void DeleteMacroById(string macroId, IList<Macro> macros)
    {
        for (var i = 0; i < macros.Count; i++)
        {
            if (macros[i].Id != macroId)
            {
                continue;
            }

            macros.RemoveAt(i);
            break;
        }
    }
}