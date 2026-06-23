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

            var elders = PawnsFinder.AllMapsWorldAndTemporary_Alive
                .Where(p => p.Faction == parms.faction && !p.Dead && !p.Destroyed && !p.Spawned && p.IsWorldPawn())
                .ToList();

            if (!elders.Any())
                return;

            var generated = __result.ToList();
            int replaceCount = Mathf.Min(elders.Count, generated.Count);

            for (int i = 0; i < replaceCount; i++)
            {
                Find.WorldPawns.RemovePawn(elders[i]);
                generated[i] = elders[i];
            }

            __result = generated;
        }
    }
}
