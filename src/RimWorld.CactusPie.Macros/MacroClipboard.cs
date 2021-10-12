using System;
using System.Linq;
using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Interfaces;

namespace RimWorld.CactusPie.Macros
{
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
            string macroName = _copiedMacro.Name;

            if (!_macroCollection.PawnMacroExists(pawnId, macroName))
            {
                Macro macro = _copiedMacro.Clone(Guid.NewGuid().ToString());
                _macroCollection.AddMacroForPawn(pawnId, macro);
                return;
            }
            
            int index = 1;
            while (true)
            {
                macroName = $"{_copiedMacro.Name} ({index})";
                if (!_macroCollection.PawnMacroExists(pawnId, macroName))
                {
                    Macro macro = _copiedMacro.Clone(Guid.NewGuid().ToString());
                    macro.Name = macroName;
                    _macroCollection.AddMacroForPawn(pawnId, macro);
                    return;
                }

                index++;
            }
        }
        
        public void PasteSharedMacro()
        {
            string macroName = _copiedMacro.Name;

            if (!_macroCollection.SharedMacroExists(macroName))
            {
                Macro macro = _copiedMacro.Clone(Guid.NewGuid().ToString());
                _macroCollection.AddSharedMacro(macro);
                return;
            }
            
            int index = 1;
            while (true)
            {
                macroName = $"{_copiedMacro.Name} ({index})";
                if (!_macroCollection.SharedMacroExists(macroName))
                {
                    Macro macro = _copiedMacro.Clone(Guid.NewGuid().ToString());
                    macro.Name = macroName;
                    _macroCollection.AddSharedMacro(macro);
                    return;
                }

                index++;
            }
        }
    }
}