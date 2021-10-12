using RimWorld.CactusPie.Macros.Collections;
using RimWorld.CactusPie.Macros.Interfaces;
using RimWorld.Planet;
using Verse;

namespace RimWorld.CactusPie.Macros.Data
{
    public class SaveData : WorldComponent
    {
        private IMacroCollection _macroCollection;

        public IMacroCollection MacroCollection
        {
            get => _macroCollection;
            set => _macroCollection = value;
        }

        public SaveData(World world) : base(world)
        {
            _macroCollection = new MacroCollection();
        }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref _macroCollection, nameof(_macroCollection));
            base.ExposeData();
        }
    }
}