using Verse;

namespace PassingTheTorch
{
    public class PassingTheTorchSettings : ModSettings
    {
        public int maxColonists = 10;
        public int maxAnimals = 10;
        public bool keepResearch = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref maxColonists, "maxColonists", 10);
            Scribe_Values.Look(ref maxAnimals, "maxAnimals", 10);
            Scribe_Values.Look(ref keepResearch, "keepResearch", true);
        }
    }
}
