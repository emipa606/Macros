using RimWorld.CactusPie.Macros.Data;

namespace RimWorld.CactusPie.Macros.Interfaces
{
    public interface IMacroClipboard
    {
        void CopyMacro(Macro macro);
        
        bool HasCopiedMacro();
        
        void PasteMacroForPawn(string pawnId);
        
        void PasteSharedMacro();
    }
}