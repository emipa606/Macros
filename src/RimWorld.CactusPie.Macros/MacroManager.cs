using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.CactusPie.Macros.Data;
using RimWorld.CactusPie.Macros.Interfaces;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld.CactusPie.Macros
{
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
            List<Pawn> playerPawns = Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer);

            ExecuteMacroForPawns(macro, clearCurrentJobs, playerPawns);
        }
        
        public void ExecuteMacroForSelectedPawns(Macro macro, bool clearCurrentJobs)
        {
            List<Pawn> selectedPawns = Find.Selector.SelectedPawns;

            ExecuteMacroForPawns(macro, clearCurrentJobs, selectedPawns);
        }
        
        public void ExecuteMatchingMacrosForAllPawns(string macroName, bool clearCurrentJobs)
        {
            List<Pawn> playerPawns = Find.CurrentMap
                .mapPawns
                .PawnsInFaction(Faction.OfPlayer);
            
            ExecuteMatchingMacrosForPawns(macroName, clearCurrentJobs, playerPawns);
        }
        
        public void ExecuteMatchingMacrosForSelectedPawns(string macroName, bool clearCurrentJobs)
        {
            List<Pawn> selectedPawns = Find.Selector.SelectedPawns;
           
            ExecuteMatchingMacrosForPawns(macroName, clearCurrentJobs, selectedPawns);
        }

        public void CreateNewMacro(string macroName, bool isShared)
        {
            CanCreateMacroState canCreateMacroState = CanCreateMacro();
            if (!canCreateMacroState.CanCreateMacro)
            {
                Messages.Message(canCreateMacroState.Message, MessageTypeDefOf.RejectInput, false);
                return;
            }
            
            List<JobData> jobs =
            (
                from job in _pawn.jobs.AllJobs()
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
                }
            ).ToList();

            var macroData = new Macro
            {
                Id = Guid.NewGuid().ToString(),
                Jobs = jobs,
                Name = macroName,
            };

            if (isShared)
            {
                _macroCollection.AddSharedMacro(macroData);
            }
            else
            {
                _macroCollection.AddMacroForPawn(_pawn.ThingID, macroData);
            }
        }
        
        public void EditMacro(string macroId, string newName, bool newIsShared)
        {
            bool isExistingMacroShared = false;
            
            Macro existingMacro = _macroCollection
                .GetPawnMacros(_pawn.ThingID)
                .FirstOrDefault(x => x.Id == macroId);

            if (existingMacro == null)
            {
                isExistingMacroShared = true;
                
                existingMacro = _macroCollection
                    .GetSharedMacros()
                    .FirstOrDefault(x => x.Id == macroId);
            }
            
            if (existingMacro == null)
            {
                throw new InvalidOperationException($"Could not find macro id {macroId}");
            }

            existingMacro.Name = newName;

            if (isExistingMacroShared == newIsShared)
            {
                return;
            }

            if (newIsShared)
            {
                _macroCollection.DeletePawnMacroById(macroId, _pawn.ThingID);
                _macroCollection.AddSharedMacro(existingMacro);
            }
            else
            {
                _macroCollection.DeleteSharedMacroById(macroId);
                _macroCollection.AddMacroForPawn(_pawn.ThingID, existingMacro);
            }
        }
        
        public void DeleteMacro(string macroId)
        {
            _macroCollection.DeletePawnMacroById(macroId, _pawn.ThingID);
            _macroCollection.DeleteSharedMacroById(macroId);
        }

        public bool CanExecuteMacrosForPawn(Pawn pawn)
        {
            return pawn.Spawned && pawn.IsColonist && !pawn.Downed && !pawn.InMentalState && !pawn.Dead && pawn.Faction != null && pawn.Faction.IsPlayer;
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
            string pawnId = _macroCollection
                .GetPawnIdsHavingMacroWithName(macroName)
                .FirstOrDefault();

            Pawn pawn = PawnsFinder
                .AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction
                .FirstOrDefault(x => x.ThingID == pawnId);

            return pawn;
        }
        
        private void ExecuteMacroForPawns(Macro macro, bool clearCurrentJobs, IEnumerable<Pawn> pawns)
        {
            foreach (Pawn pawn in pawns)
            {
                if (!CanExecuteMacrosForPawn(pawn))
                {
                    continue;
                }
                
                ExecuteMacroForPawn(macro, pawn, clearCurrentJobs);
            }
        }
        
        private void ExecuteMatchingMacrosForPawns(string macroName, bool clearCurrentJobs, IEnumerable<Pawn> pawns)
        {
            foreach (Pawn pawn in pawns)
            {
                if (!CanExecuteMacrosForPawn(pawn))
                {
                    continue;
                }

                Macro matchingPawnMacro = _macroCollection
                    .GetPawnAndSharedMacros(pawn.ThingID)
                    .FirstOrDefault(x => x.Name == macroName);

                if (matchingPawnMacro != null)
                {
                    ExecuteMacroForPawn(matchingPawnMacro, pawn, clearCurrentJobs);
                }
            }
        }
        
        private void ExecuteMacroForPawn(Macro macro, Pawn pawn, bool clearCurrentJobs)
        {
            if (macro.Jobs != null && macro.Jobs.Any())
            {
                if (clearCurrentJobs)
                {
                    pawn.jobs.StopAll();
                }

                foreach (JobData job in macro.Jobs)
                {
                    if (job == null)
                    {
                        continue;
                    }

                    Job newJob = JobMaker.MakeJob(job.Def, job.TargetA, job.TargetB, job.TargetC);
                    newJob.playerForced = true;
                    newJob.interaction = job.Interaction;
                    newJob.count = job.Count;
                    newJob.haulMode = job.HaulMode;
                    newJob.plantDefToSow = job.PlantDefToSow;
                    newJob.thingDefToCarry = job.ThingDefToCarry;
                    newJob.canUseRangedWeapon = job.CanUseRangedWeapon;
                    newJob.interaction = job.Interaction;
                    newJob.takeInventoryDelay = job.TakeInventoryDelay;

                    pawn.jobs.TryTakeOrderedJob(newJob, JobTag.Misc, true);
                }
            }
        }
    }
}