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

            var requirements = new QuestPart_TorchRequirements();
            quest.AddPart(requirements);

            var sequence = new QuestPart_TorchSequence
            {
                candidateHeirs = heirs,
                inSignalEnable = slate.Get<string>("inSignal")
            };
            quest.AddPart(sequence);
            var choice = new QuestPart_Choice();
            choice.inSignalChoiceUsed = QuestGenUtility.HardcodedSignalWithQuestID("torchSequence.Completed");
            var rewardChoice = new QuestPart_Choice.Choice();
            rewardChoice.rewards.Add(new Reward_TorchGenerationalDeparture());
            choice.choices.Add(rewardChoice);
            quest.AddPart(choice);
        }

        public override bool TestRunInt(Slate slate)
        {
            return true;
        }
    }
}
