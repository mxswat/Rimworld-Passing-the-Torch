using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace PassingTheTorch
{
    [HarmonyPatch]
    public static class PawnGroupMakerUtility_GeneratePawns_Patch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(PawnGroupMakerUtility), "GeneratePawns", new Type[]
            {
                typeof(PawnGroupMakerParms),
                typeof(bool)
            });
        }

        public static void Postfix(ref IEnumerable<Pawn> __result, PawnGroupMakerParms parms)
        {
            if (parms?.faction == null || !parms.faction.def.defName.StartsWith("Torch_Ancestors"))
                return;

            if (parms.groupKind != PawnGroupKindDefOf.Settlement)
                return;

            var generated = __result.ToList();
            var allWorldPawns = Find.WorldPawns.AllPawnsAlive.ToList();
            var elders = allWorldPawns
                .Where(p => p.Faction == parms.faction && p.RaceProps.Humanlike && !p.Dead && !p.Destroyed && !p.Spawned && p.IsWorldPawn())
                .ToList();

            if (!elders.Any())
            {
                Log.Message($"[PassingTheTorch] No elders found in world pawns for {parms.faction.Name}. World pawn count: {allWorldPawns.Count}");
                return;
            }

            int replaceCount = Mathf.Min(elders.Count, generated.Count);
            if (replaceCount <= 0)
                return;

            Log.Message($"[PassingTheTorch] Replacing {replaceCount} of {generated.Count} generated pawns with elders from world pawns");
            for (int i = 0; i < replaceCount; i++)
            {
                Find.WorldPawns.RemovePawn(elders[i]);
                Log.Message($"[PassingTheTorch] Spawning elder {elders[i].NameShortColored}");
                generated[i] = elders[i];
            }

            __result = generated;
        }
    }
}
