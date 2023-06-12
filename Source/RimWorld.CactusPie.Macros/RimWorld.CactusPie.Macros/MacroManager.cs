using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Interfaces;
using Verse;
using Verse.AI;

namespace RimWorld.CactusPie.Macros;

public class MacroManager : IMacroManager
{
    private readonly IMacroCollection _macroCollection;

    private readonly Pawn _pawn;

    public MacroManager(IMacroCollection macroCollection, Pawn pawn)
    {
        _macroCollection = macroCollection;
        _pawn = pawn;
    }

    public IList<Macro> GetPawnMacros()
    {
        return _macroCollection.GetPawnMacros(_pawn.ThingID);
    }

    public IList<Macro> GetSharedMacros()
    {
        return _macroCollection.GetSharedMacros();
    }

    public CanCreateMacroState CanCreateMacro()
    {
        if (!_pawn.jobs.AllJobs().Any())
        {
            return new CanCreateMacroState
            {
                CanCreateMacro = false,
                Message = "Macros_Add_Error_No_Jobs".Translate()
            };
        }

        if (Find.Selector.SelectedPawns.Count > 1)
        {
            return new CanCreateMacroState
            {
                CanCreateMacro = false,
                Message = "Macros_Add_Error_Multiple_Selected".Translate()
            };
        }

        return new CanCreateMacroState
        {
            CanCreateMacro = true
        };
    }

    public void ExecuteMacro(Macro macro, bool clearCurrentJobs)
    {
        ExecuteMacroForPawn(macro, _pawn, clearCurrentJobs);
    }

    public void ExecuteMacroForAllPawns(Macro macro, bool clearCurrentJobs)
    {
        var pawns = Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer);
        ExecuteMacroForPawns(macro, clearCurrentJobs, pawns);
    }

    public void ExecuteMacroForSelectedPawns(Macro macro, bool clearCurrentJobs)
    {
        var selectedPawns = Find.Selector.SelectedPawns;
        ExecuteMacroForPawns(macro, clearCurrentJobs, selectedPawns);
    }

    public void ExecuteMatchingMacrosForAllPawns(string macroName, bool clearCurrentJobs)
    {
        var pawns = Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer);
        ExecuteMatchingMacrosForPawns(macroName, clearCurrentJobs, pawns);
    }

    public void ExecuteMatchingMacrosForSelectedPawns(string macroName, bool clearCurrentJobs)
    {
        var selectedPawns = Find.Selector.SelectedPawns;
        ExecuteMatchingMacrosForPawns(macroName, clearCurrentJobs, selectedPawns);
    }

    public void CreateNewMacro(string macroName, bool isShared)
    {
        var canCreateMacroState = CanCreateMacro();
        if (!canCreateMacroState.CanCreateMacro)
        {
            Messages.Message(canCreateMacroState.Message, MessageTypeDefOf.RejectInput, false);
            return;
        }

        var jobs = (from job in _pawn.jobs.AllJobs()
            select new JobData
            {
                Def = job.def,
                TargetA = job.targetA,
                TargetB = job.targetB,
                TargetC = job.targetC,
                Count = job.count,
                HaulMode = job.haulMode,
                PlantDefToSow = job.plantDefToSow,
                ThingDefToCarry = job.thingDefToCarry,
                CanUseRangedWeapon = job.canUseRangedWeapon,
                Interaction = job.interaction,
                TakeInventoryDelay = job.takeInventoryDelay
            }).ToList();
        var macro = new Macro
        {
            Id = Guid.NewGuid().ToString(),
            Jobs = jobs,
            Name = macroName
        };
        if (isShared)
        {
            _macroCollection.AddSharedMacro(macro);
        }
        else
        {
            _macroCollection.AddMacroForPawn(_pawn.ThingID, macro);
        }
    }

    public void EditMacro(string macroId, string newName, bool newIsShared)
    {
        var sharedMacroUsed = false;
        var macro = _macroCollection.GetPawnMacros(_pawn.ThingID).FirstOrDefault(x => x.Id == macroId);
        if (macro == null)
        {
            sharedMacroUsed = true;
            macro = _macroCollection.GetSharedMacros().FirstOrDefault(x => x.Id == macroId);
        }

        if (macro == null)
        {
            throw new InvalidOperationException("Could not find macro id " + macroId);
        }

        macro.Name = newName;
        if (sharedMacroUsed == newIsShared)
        {
            return;
        }

        if (newIsShared)
        {
            _macroCollection.DeletePawnMacroById(macroId, _pawn.ThingID);
            _macroCollection.AddSharedMacro(macro);
        }
        else
        {
            _macroCollection.DeleteSharedMacroById(macroId);
            _macroCollection.AddMacroForPawn(_pawn.ThingID, macro);
        }
    }

    public void DeleteMacro(string macroId)
    {
        _macroCollection.DeletePawnMacroById(macroId, _pawn.ThingID);
        _macroCollection.DeleteSharedMacroById(macroId);
    }

    public bool CanExecuteMacrosForPawn(Pawn pawn)
    {
        if (pawn.Spawned && pawn.IsColonist && !pawn.Downed && !pawn.InMentalState && !pawn.Dead &&
            pawn.Faction != null)
        {
            return pawn.Faction.IsPlayer;
        }

        return false;
    }

    public bool PawnMacroExistsForCurrentPawn(string macroName)
    {
        return _macroCollection.GetPawnMacros(_pawn.ThingID).Any(macro => macro.Name == macroName);
    }

    public bool SharedMacroExists(string macroName)
    {
        return _macroCollection.SharedMacroExists(macroName);
    }

    public Pawn FindPlayerPawnHavingMacroName(string macroName)
    {
        var pawnId = _macroCollection.GetPawnIdsHavingMacroWithName(macroName).FirstOrDefault();
        return Enumerable.FirstOrDefault(PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction,
            x => x.ThingID == pawnId);
    }

    private void ExecuteMacroForPawns(Macro macro, bool clearCurrentJobs, IEnumerable<Pawn> pawns)
    {
        foreach (var pawn in pawns)
        {
            if (CanExecuteMacrosForPawn(pawn))
            {
                ExecuteMacroForPawn(macro, pawn, clearCurrentJobs);
            }
        }
    }

    private void ExecuteMatchingMacrosForPawns(string macroName, bool clearCurrentJobs, IEnumerable<Pawn> pawns)
    {
        foreach (var pawn in pawns)
        {
            if (!CanExecuteMacrosForPawn(pawn))
            {
                continue;
            }

            var macro = _macroCollection.GetPawnAndSharedMacros(pawn.ThingID)
                .FirstOrDefault(x => x.Name == macroName);
            if (macro != null)
            {
                ExecuteMacroForPawn(macro, pawn, clearCurrentJobs);
            }
        }
    }

    private void ExecuteMacroForPawn(Macro macro, Pawn pawn, bool clearCurrentJobs)
    {
        if (macro.Jobs == null || !macro.Jobs.Any())
        {
            return;
        }

        if (clearCurrentJobs)
        {
            pawn.jobs.StopAll();
        }

        foreach (var job2 in macro.Jobs)
        {
            if (job2 == null)
            {
                continue;
            }

            var job = JobMaker.MakeJob(job2.Def, job2.TargetA, job2.TargetB, job2.TargetC);
            job.playerForced = true;
            job.interaction = job2.Interaction;
            job.count = job2.Count;
            job.haulMode = job2.HaulMode;
            job.plantDefToSow = job2.PlantDefToSow;
            job.thingDefToCarry = job2.ThingDefToCarry;
            job.canUseRangedWeapon = job2.CanUseRangedWeapon;
            job.interaction = job2.Interaction;
            job.takeInventoryDelay = job2.TakeInventoryDelay;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc, true);
        }
    }
}