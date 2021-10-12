using System.Collections.Generic;
using RimWorld.CactusPie.Macros.Data;
using Verse;

namespace RimWorld.CactusPie.Macros.Interfaces
{
    public interface IMacroManager
    {
        IList<Macro> GetPawnMacros();
        
        IList<Macro> GetSharedMacros();
        
        CanCreateMacroState CanCreateMacro();
        
        void ExecuteMacro(Macro macro, bool clearCurrentJobs);
        
        void ExecuteMacroForAllPawns(Macro macro, bool clearCurrentJobs);
        
        void ExecuteMacroForSelectedPawns(Macro macro, bool clearCurrentJobs);
        
        void ExecuteMatchingMacrosForAllPawns(string macroName, bool clearCurrentJobs);
        
        void ExecuteMatchingMacrosForSelectedPawns(string macroName, bool clearCurrentJobs);
        
        void CreateNewMacro(string macroName, bool isShared);
        
        void EditMacro(string macroId, string newName, bool newIsShared);
        
        void DeleteMacro(string macroId);
        
        bool CanExecuteMacrosForPawn(Pawn pawn);
        
        bool PawnMacroExistsForCurrentPawn(string macroName);
        
        bool SharedMacroExists(string macroName);

        Pawn FindPlayerPawnHavingMacroName(string macroName);
    }
}