using RimWorld;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

namespace PassingTheTorch
{
    public class QuestNode_Root_PassingTheTorch : QuestNode
    {
        public override void RunInt()
        {
            var quest = QuestGen.quest;
            var slate = QuestGen.slate;

            var heirs = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction
                .Where(HeirValidator.IsValidHeir)
                .ToList();

            slate.Set("heirList", heirs.Select((Pawn p) => p.NameFullColored.Resolve()).ToLineList("  - ", capitalizeItems: true));

            var sequence = new QuestPart_TorchSequence
            {
                candidateHeirs = heirs,
                inSignalEnable = slate.Get<string>("inSignal")
            };
            quest.AddPart(sequence);
        }

        public override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }
}
