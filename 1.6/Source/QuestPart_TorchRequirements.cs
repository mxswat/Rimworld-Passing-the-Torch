using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace PassingTheTorch
{
    public class QuestPart_TorchRequirements : QuestPart_RequirementsToAccept
    {
        public override AcceptanceReport CanAccept()
        {
            var sb = new StringBuilder();
            var caravans = Find.WorldObjects.Caravans;
            var playerCaravans = caravans.Where(c => c.IsPlayerControlled && c.pawns.Any((Pawn p) => p.Faction == Faction.OfPlayer)).ToList();
            
            if (playerCaravans.Any())
            {
                sb.AppendLine("Torch_CaravanBlockers".Translate());
            }
            var heirs = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction
                .Where(HeirValidator.IsValidHeir)
                .ToList();

            var incapacitatedHeirs = heirs.Where(h => !HeirValidator.IsHeirCapable(h)).ToList();
            if (incapacitatedHeirs.Any())
            {
                sb.AppendLine("Torch_IncapacitatedHeirs".Translate());
            }
            if (sb.Length > 0)
            {
                var message = "Torch_Requirements".Translate() + "\n\n" + sb.ToString().TrimEndNewlines();
                return new AcceptanceReport(message);
            }

            return AcceptanceReport.WasAccepted;
        }
    }
}
