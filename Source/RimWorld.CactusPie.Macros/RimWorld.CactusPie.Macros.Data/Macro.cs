using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.CactusPie.Macros.Data;

public class Macro : IExposable
{
    private string _id;

    private List<JobData> _jobs;

    private string _name;

    public string Id
    {
        get => _id;
        set => _id = value;
    }

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public List<JobData> Jobs
    {
        get => _jobs;
        set => _jobs = value;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref _id, "Macros_MacroId");
        Scribe_Values.Look(ref _name, "Macros_MacroName");
        Scribe_Collections.Look(ref _jobs, "Macros_Jobs", LookMode.Deep);
    }

    public Macro Clone(string newId)
    {
        return new Macro
        {
            Id = newId,
            Name = Name,
            Jobs = Jobs.Select(x => new JobData
            {
                Def = x.Def,
                TargetA = x.TargetA,
                TargetB = x.TargetB,
                TargetC = x.TargetC,
                Count = x.Count,
                HaulMode = x.HaulMode,
                PlantDefToSow = x.PlantDefToSow,
                ThingDefToCarry = x.ThingDefToCarry,
                CanUseRangedWeapon = x.CanUseRangedWeapon,
                Interaction = x.Interaction,
                TakeInventoryDelay = x.TakeInventoryDelay
            }).ToList()
        };
    }
}