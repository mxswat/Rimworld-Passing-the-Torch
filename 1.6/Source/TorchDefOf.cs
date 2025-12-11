using RimWorld;

namespace PassingTheTorch
{
    [DefOf]
    public static class TorchDefOf
    {
        public static FactionDef Torch_Ancestors;
        public static QuestScriptDef Torch_GenerationalDeparture;
        public static ThoughtDef Torch_FarewellParty;
        public static GoodwillSituationDef Torch_AncestorsGoodwill;

        static TorchDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TorchDefOf));
        }
    }
}
