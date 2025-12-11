using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

namespace PassingTheTorch
{
    [HarmonyPatch(typeof(TickManager), "DoSingleTick")]
    public static class TickManager_DoSingleTick_Patch
    {
        public static void Postfix()
        {
            if (Find.TickManager.TicksGame % 300 != 0)
            {
                return;
            }

            if (Find.QuestManager.QuestsListForReading.Any(q => q.root == TorchDefOf.Torch_GenerationalDeparture && (q.State == QuestState.Ongoing || q.State == QuestState.NotYetAccepted)))
            {
                return;
            }

            int validHeirs = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction.Count(HeirValidator.IsValidHeir);
            if (validHeirs >= 3)
            {
                var slate = new Slate();
                var quest = QuestUtility.GenerateQuestAndMakeAvailable(TorchDefOf.Torch_GenerationalDeparture, slate);
                QuestUtility.SendLetterQuestAvailable(quest);
            }
        }
    }
}
