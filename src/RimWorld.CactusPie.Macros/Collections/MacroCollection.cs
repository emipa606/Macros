using System.Collections.Generic;
using System.Linq;
using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Interfaces;
using Verse;

namespace RimWorld.CactusPie.Macros.Collections
{
    /// <summary>
    /// A class providing easy access to operations on macro collections,
    /// such as adding, editing or removing macros
    /// </summary>
    public class MacroCollection : IMacroCollection, IExposable
    {
        private Dictionary<string, ExposableList<Macro>> _pawnMacros;
        private List<Macro> _sharedMacros;

        public MacroCollection()
        {
            _pawnMacros = new Dictionary<string, ExposableList<Macro>>();
            _sharedMacros = new List<Macro>();
        }
        
        public void AddMacroForPawn(string pawnId, Macro macro)
        {
            if (!_pawnMacros.TryGetValue(pawnId, out ExposableList<Macro> pawnMacros))
            {
                pawnMacros = new ExposableList<Macro>();
                _pawnMacros.Add(pawnId, pawnMacros);
            }
            
            pawnMacros.Add(macro);
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
            
            if (!_pawnMacros.TryGetValue(pawnId, out ExposableList<Macro> pawnMacros))
            {
                pawnMacros = new ExposableList<Macro>();
                _pawnMacros.Add(pawnId, pawnMacros);
            }

            return pawnMacros;
        }
        
        /// <summary>
        /// Returns list of PawnIds (ThingId) that already have a macro with the specified name
        /// </summary>
        /// <param name="macroName">Macro name to find</param>
        /// <returns>Collection of pawn ids (ThingId)</returns>
        public IEnumerable<string> GetPawnIdsHavingMacroWithName(string macroName)
        {
            if (_pawnMacros == null)
            {
                _pawnMacros = new Dictionary<string, ExposableList<Macro>>();
            }

            foreach (KeyValuePair<string,ExposableList<Macro>> pawnMacros in _pawnMacros)
            {
                foreach (Macro pawnMacro in pawnMacros.Value)
                {
                    if (pawnMacro.Name == macroName)
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
            if (_pawnMacros.TryGetValue(pawnId, out ExposableList<Macro> pawnMacros))
            {
                foreach (Macro pawnMacro in pawnMacros)
                {
                    yield return pawnMacro;
                }
            }
            
            foreach (Macro sharedMacro in _sharedMacros)
            {
                yield return sharedMacro;
            }
        }
        
        public void DeletePawnMacroById(string macroId, string pawnId)
        {
            if (!_pawnMacros.TryGetValue(pawnId, out ExposableList<Macro> pawnMacros))
            {
                return;
            }

            DeleteMacroById(macroId, pawnMacros);
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
            for (int i = 0; i < macros.Count; i++)
            {
                if (macros[i].Id == macroId)
                {
                    macros.RemoveAt(i);
                    break;
                }
            }
        }
        
        public void ExposeData()
        {
            Scribe_Collections.Look(ref _sharedMacros, "Macros_sharedMacros", LookMode.Deep);
            Scribe_Collections.Look(ref _pawnMacros, "Macros_pawnMacros", LookMode.Value, LookMode.Deep);
        }
    }
}