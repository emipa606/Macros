using RimWorld.CactusPie.Macros.Collections;
using RimWorld.CactusPie.Macros.Interfaces;
using RimWorld.Planet;
using Verse;

namespace RimWorld.CactusPie.Macros.Data;

public class SaveData(World world) : WorldComponent(world)
{
    private IMacroCollection _macroCollection = new MacroCollection();

    public IMacroCollection MacroCollection
    {
        get => _macroCollection;
        set => _macroCollection = value;
    }

    public override void ExposeData()
    {
        Scribe_Deep.Look(ref _macroCollection, "_macroCollection");
        base.ExposeData();
    }
}