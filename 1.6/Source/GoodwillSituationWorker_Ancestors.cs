using RimWorld;
using Verse;

namespace PassingTheTorch
{
    public class GoodwillSituationWorker_Ancestors : GoodwillSituationWorker
    {
        public override string GetPostProcessedLabel(Faction other)
        {
            return "Torch_GoodwillLabel_SameFamily".Translate();
        }

        public override int GetNaturalGoodwillOffset(Faction other)
        {
            return Applies(other) ? def.naturalGoodwillOffset : 0;
        }

        public override int GetMaxGoodwill(Faction other)
        {
            return Applies(other) ? def.baseMaxGoodwill : base.GetMaxGoodwill(other);
        }

        private bool Applies(Faction other)
        {
            return Applies(Faction.OfPlayer, other) || Applies(other, Faction.OfPlayer);
        }

        private bool Applies(Faction a, Faction b)
        {
            if (a == null || b == null)
            {
                return false;
            }
            if (a.def == TorchDefOf.Torch_Ancestors && b.IsPlayer)
            {
                return true;
            }
            return b.def == TorchDefOf.Torch_Ancestors && a.IsPlayer;
        }
    }
}
