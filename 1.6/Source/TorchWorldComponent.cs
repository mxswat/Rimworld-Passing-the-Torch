using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace PassingTheTorch
{
    public class TorchWorldComponent : WorldComponent
    {
        public List<Pawn> departedHeirs = new List<Pawn>();

        public TorchWorldComponent(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref departedHeirs, "departedHeirs", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                departedHeirs ??= new List<Pawn>();
            }
        }
    }
}
