using RimWorld;
using Verse;

namespace PassingTheTorch
{
    public class LordJob_Joinable_FarewellParty : LordJob_Joinable_Party
    {
        public LordJob_Joinable_FarewellParty()
        {
        }

        public LordJob_Joinable_FarewellParty(IntVec3 spot, Pawn organizer, GatheringDef gatheringDef) : base(spot, organizer, gatheringDef)
        {
        }

        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
            if (p.Faction != Faction.OfPlayer)
            {
                return 0f;
            }
            if (!p.IsColonist || p.IsPrisoner || p.IsSlave || p.IsQuestLodger())
            {
                return 0f;
            }
            if (p.Downed || p.InMentalState || !p.RaceProps.Humanlike)
            {
                return 0f;
            }
            if (p.Map != Map)
            {
                return 0f;
            }
            if (p.Drafted)
            {
                return 0f;
            }
            return 100f;
        }
    }
}