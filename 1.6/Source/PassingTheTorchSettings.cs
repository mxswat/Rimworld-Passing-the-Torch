using Verse;

namespace PassingTheTorch
{
    public enum TorchRelationResetMode
    {
        None,
        Neutral,
        HalfStrength
    }

    public class PassingTheTorchSettings : ModSettings
    {
        public int maxColonists = 10;
        public int maxAnimals = 10;
        public bool keepResearch = true;
        public TorchRelationResetMode relationResetMode = TorchRelationResetMode.Neutral;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref maxColonists, "maxColonists", 10);
            Scribe_Values.Look(ref maxAnimals, "maxAnimals", 10);
            Scribe_Values.Look(ref keepResearch, "keepResearch", true);
            Scribe_Values.Look(ref relationResetMode, "relationResetMode", TorchRelationResetMode.Neutral);
        }
    }
}
