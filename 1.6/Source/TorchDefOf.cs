using RimWorld;

namespace PassingTheTorch
{
    [DefOf]
    public static class TorchDefOf
    {
        public static FactionDef Torch_Ancestors_Neolithic;
        public static FactionDef Torch_Ancestors_Medieval;
        public static FactionDef Torch_Ancestors_Industrial;
        public static FactionDef Torch_Ancestors_Spacer;
        public static FactionDef Torch_Ancestors_Ultra;
        public static QuestScriptDef Torch_GenerationalDeparture;
        public static ThoughtDef Torch_FarewellParty;
        public static GoodwillSituationDef Torch_AncestorsGoodwill;

        static TorchDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TorchDefOf));
        }
    }
}
