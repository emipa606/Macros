using Verse;
using Verse.AI;

namespace RimWorld.CactusPie.Macros.Data;

public class JobData : IExposable
{
    private bool _canUseRangedWeapon;

    private int _count;
    private JobDef _def;

    private HaulMode _haulMode;

    private InteractionDef _interaction;

    private ThingDef _plantDefToSow;

    private int _takeInventoryDelay;

    private LocalTargetInfo _targetA;

    private LocalTargetInfo _targetB;

    private LocalTargetInfo _targetC;

    private ThingDef _thingDefToCarry;

    public JobDef Def
    {
        get => _def;
        set => _def = value;
    }

    public LocalTargetInfo TargetA
    {
        get => _targetA;
        set => _targetA = value;
    }

    public LocalTargetInfo TargetB
    {
        get => _targetB;
        set => _targetB = value;
    }

    public LocalTargetInfo TargetC
    {
        get => _targetC;
        set => _targetC = value;
    }

    public int Count
    {
        get => _count;
        set => _count = value;
    }

    public HaulMode HaulMode
    {
        get => _haulMode;
        set => _haulMode = value;
    }

    public ThingDef PlantDefToSow
    {
        get => _plantDefToSow;
        set => _plantDefToSow = value;
    }

    public ThingDef ThingDefToCarry
    {
        get => _thingDefToCarry;
        set => _thingDefToCarry = value;
    }

    public bool CanUseRangedWeapon
    {
        get => _canUseRangedWeapon;
        set => _canUseRangedWeapon = value;
    }

    public InteractionDef Interaction
    {
        get => _interaction;
        set => _interaction = value;
    }

    public int TakeInventoryDelay
    {
        get => _takeInventoryDelay;
        set => _takeInventoryDelay = value;
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref _def, "Macros_def");
        Scribe_TargetInfo.Look(ref _targetA, "Macros_targetA");
        Scribe_TargetInfo.Look(ref _targetB, "Macros_targetB");
        Scribe_TargetInfo.Look(ref _targetC, "Macros_targetC");
        Scribe_Values.Look(ref _count, "Macros_count");
        Scribe_Values.Look(ref _haulMode, "Macros_haulMode");
        Scribe_Defs.Look(ref _plantDefToSow, "Macros_plantDefToSow");
        Scribe_Defs.Look(ref _thingDefToCarry, "Macros_thingDefToCarry");
        Scribe_Values.Look(ref _canUseRangedWeapon, "Macros_canUseRangedWeapon");
        Scribe_Defs.Look(ref _interaction, "Macros_interaction");
        Scribe_Values.Look(ref _takeInventoryDelay, "Macros_takeInventoryDelay");
    }
}