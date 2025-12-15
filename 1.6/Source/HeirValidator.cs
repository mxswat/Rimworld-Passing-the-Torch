using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PassingTheTorch
{
    public static class HeirValidator
    {
        private static readonly List<BackstoryCategoryFilter> BackstoryFiltersTribal =
        [
            new BackstoryCategoryFilter
            {
                categories = ["AdultTribal"]
            }
        ];

        private static readonly List<BackstoryCategoryFilter> BackstoryFiltersColonist =
        [
            new BackstoryCategoryFilter
            {
                categories = ["AdultColonist"]
            }
        ];

        private static readonly List<BackstoryCategoryFilter> VatgrowBackstoryFilter =
        [
            new BackstoryCategoryFilter
            {
                categories = ["VatGrown"]
            }
        ];

        public static bool IsValidHeir(Pawn p)
        {
            if (!p.ageTracker.Adult || p.story is null 
            || p.RaceProps.Humanlike is false || Find.World.GetComponent<TorchWorldComponent>().departedHeirs.Contains(p))
            {
                return false;
            }

            var childhood = p.story.GetBackstory(BackstorySlot.Childhood);
            var adulthood = p.story.GetBackstory(BackstorySlot.Adulthood);
            if (childhood != null)
            {
                if (p.ageTracker.vatGrowTicks >= 1200000)
                {
                    foreach (string cat in VatgrowBackstoryFilter.SelectMany(f => f.categories))
                    {
                        if (childhood.spawnCategories != null && childhood.spawnCategories.Contains(cat))
                        {
                            return true;
                        }
                    }
                }
            }

            var backstoryCategories = (Faction.OfPlayer.def == FactionDefOf.PlayerTribe) ? BackstoryFiltersTribal : BackstoryFiltersColonist;

            foreach (var filter in backstoryCategories)
            {
                foreach (string category in filter.categories)
                {
                    if ((childhood?.spawnCategories != null && childhood.spawnCategories.Contains(category)) || (adulthood?.spawnCategories != null && adulthood.spawnCategories.Contains(category)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsHeirCapable(Pawn p)
        {
            if (p.Downed || p.InMentalState || !p.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
            {
                return false;
            }
            return true;
        }
    }
}
