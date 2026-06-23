using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PassingTheTorch
{
    public static class TorchTransitionUtility
    {
        public static void ExecuteMove(PlanetTile tileID, List<Thing> selectedThings, Quest quest)
        {
            var departingPawns = selectedThings.OfType<Pawn>().ToList();
            var worldComp = Find.World.GetComponent<TorchWorldComponent>();
            worldComp.departedHeirs.AddRange(departingPawns);
            var departingItems = selectedThings.Where(t => t is not Pawn).ToList();
            var allPlayerMaps = Find.Maps.Where(m => m.IsPlayerHome).ToList();

            foreach (var map in allPlayerMaps)
            {
                var things = map.listerThings.AllThings.ToList();
                foreach (var thing in things)
                {
                    var goodwillComp = thing.TryGetComp<CompDissolutionEffect_Goodwill>();
                    if (goodwillComp != null)
                    {
                        thing.Destroy();
                    }
                }
            }

            var parms = new FactionGeneratorParms(GetFactionDefForAncestors());
            var ancestors = FactionGenerator.NewGeneratedFaction(parms);
            ancestors.Name = Faction.OfPlayer.Name;
            if (ancestors.ideos != null && Faction.OfPlayer.ideos.PrimaryIdeo != null)
            {
                ancestors.ideos.SetPrimary(Faction.OfPlayer.ideos.PrimaryIdeo);
            }
            ancestors.hidden = false;
            Find.FactionManager.Add(ancestors);
            ancestors.TryAffectGoodwillWith(Faction.OfPlayer, 100, false, false, null, null);
            Log.Message($"[PassingTheTorch] Created ancestral home with def: {ancestors.def.defName}, techLevel: {ancestors.def.techLevel}");

            foreach (var other in Find.FactionManager.AllFactionsListForReading)
            {
                if (other == null || other == Faction.OfPlayer || other == ancestors)
                    continue;
                if (other.defeated)
                    continue;

                int playerGoodwill = Faction.OfPlayer.GoodwillWith(other);

                var ancestorRelation = ancestors.RelationWith(other);
                ancestorRelation.baseGoodwill = Mathf.Clamp(playerGoodwill, -100, 100);
                ancestorRelation.CheckKindThresholds(ancestors, false, null, GlobalTargetInfo.Invalid, out _);

                var otherRelation = other.RelationWith(ancestors);
                otherRelation.baseGoodwill = ancestorRelation.baseGoodwill;
                otherRelation.kind = ancestorRelation.kind;
            }

            if (PassingTheTorchMod.settings.relationResetMode != TorchRelationResetMode.None)
            {
                foreach (var other in Find.FactionManager.AllFactionsListForReading)
                {
                    if (other == null || other == Faction.OfPlayer || other == ancestors)
                        continue;
                    if (other.defeated)
                        continue;

                    int currentGoodwill = Faction.OfPlayer.GoodwillWith(other);
                    int targetGoodwill;

                    switch (PassingTheTorchMod.settings.relationResetMode)
                    {
                        case TorchRelationResetMode.Neutral:
                            targetGoodwill = 0;
                            break;
                        case TorchRelationResetMode.HalfStrength:
                            targetGoodwill = Mathf.RoundToInt(currentGoodwill * 0.5f);
                            break;
                        default:
                            targetGoodwill = currentGoodwill;
                            break;
                    }

                    int delta = targetGoodwill - currentGoodwill;
                    if (delta != 0)
                    {
                        Faction.OfPlayer.TryAffectGoodwillWith(other, delta, false, false, null, null);
                    }
                }
                Log.Message($"[PassingTheTorch] Player diplomacy reset mode: {PassingTheTorchMod.settings.relationResetMode}");
            }

            var stayingPawns = new List<Pawn>();
            foreach (var map in allPlayerMaps)
            {
                stayingPawns.AddRange(map.mapPawns.FreeColonists.Where(p => !departingPawns.Contains(p)));
            }
            var playerSettlements = Find.WorldObjects.Settlements.Where(s => s.Faction == Faction.OfPlayer).ToList();
            var playerSettlementTileIDs = playerSettlements.Select(s => s.Tile).ToList();
            var playerSettlementNames = playerSettlements.Select(s => s.Label).ToList();

            if (playerSettlements.Count == 0)
            {
                Log.Error("No player settlements found for torch transition");
                return;
            }
            foreach (var p in stayingPawns)
            {
                if (p.Faction == Faction.OfPlayer)
                {
                    p.SetFaction(ancestors);
                }

                if (p.Spawned)
                {
                    p.DeSpawn();
                }

                if (!p.IsWorldPawn())
                {
                    Find.WorldPawns.PassToWorld(p, PawnDiscardDecideMode.KeepForever);
                }
            }
            var newLeader = stayingPawns.OrderByDescending(x => x.ageTracker.AgeBiologicalYears).FirstOrDefault();
            if (newLeader != null)
            {
                ancestors.leader = newLeader;
            }
            else
            {
                ancestors.TryGenerateNewLeader();
            }
            foreach (var p in departingPawns)
            {
                p.needs?.mood?.thoughts?.memories?.TryGainMemory(TorchDefOf.Torch_FarewellParty);
                if (p.Spawned)
                {
                    p.DeSpawn();
                }
            }
            foreach (var item in departingItems)
            {
                if (item.Spawned)
                {
                    item.DeSpawn();
                }

                item.holdingOwner?.Remove(item);
            }
            foreach (var map in allPlayerMaps)
            {
                Current.Game.DeinitAndRemoveMap(map, notifyPlayer: false);
            }

            CompDissolutionEffect_Goodwill.pendingGoodwillEvents.Clear();

            if (!PassingTheTorchMod.settings.keepResearch)
            {
                Find.ResearchManager.ResetAllProgress();
                ResearchUtility.ApplyPlayerStartingResearch();
            }
            for (int i = 0; i < playerSettlementTileIDs.Count; i++)
            {
                var tileIDToUse = playerSettlementTileIDs[i];
                string settlementName = playerSettlementNames[i];

                var ancestorSettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                ancestorSettlement.SetFaction(ancestors);
                ancestorSettlement.Tile = tileIDToUse;
                ancestorSettlement.Name = settlementName;
                Find.WorldObjects.Add(ancestorSettlement);
            }

            var newSettlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            newSettlement.SetFaction(Faction.OfPlayer);
            newSettlement.Tile = tileID;
            newSettlement.Name = SettlementNameGenerator.GenerateSettlementName(newSettlement, Faction.OfPlayer.def.playerInitialSettlementNameMaker);
            Find.WorldObjects.Add(newSettlement);

            var newMap = MapGenerator.GenerateMap(Find.World.info.initialMapSize, newSettlement, newSettlement.MapGeneratorDef, newSettlement.ExtraGenStepDefs, null);

            if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(newMap) && !c.Fogged(newMap), newMap, CellFinder.EdgeRoadChance_Always, out var spawnSpot))
            {
                spawnSpot = CellFinder.RandomCell(newMap);
            }
            var caravan = CaravanMaker.MakeCaravan(departingPawns, Faction.OfPlayer, tileID, addToWorldPawnsIfNotAlready: true);
            foreach (var item in departingItems)
            {
                caravan.AddPawnOrItem(item, true);
            }
            if (caravan.Tile != tileID)
            {
                caravan.pather.StartPath(tileID, null, repathImmediately: true);
            }
            CaravanEnterMapUtility.Enter(caravan, newMap, CaravanEnterMode.Center, CaravanDropInventoryMode.DropInstantly, false,
                (IntVec3 x) => x.Standable(newMap) && !x.Fogged(newMap));

            quest.End(QuestEndOutcome.Success);

            Current.Game.CurrentMap = newMap;
            Find.CameraDriver.JumpToCurrentMapLoc(spawnSpot);

            Find.LetterStack.ReceiveLetter("Torch_LetterLabel_TransitionSuccess".Translate(), "Torch_LetterText_TransitionSuccess".Translate(),
                LetterDefOf.PositiveEvent, departingPawns.FirstOrDefault());
        }

        private static FactionDef GetFactionDefForAncestors()
        {
            static int CountFinished(TechLevel level) =>
                DefDatabase<ResearchProjectDef>.AllDefsListForReading
                    .Count(p => p.techLevel == level && p.IsFinished);

            TechLevel floor = Faction.OfPlayer.def.techLevel;

            TechLevel best = floor;
            if (CountFinished(TechLevel.Ultra) >= 2) best = TechLevel.Ultra;
            else if (CountFinished(TechLevel.Spacer) >= 5) best = TechLevel.Spacer;
            else if (CountFinished(TechLevel.Industrial) >= 5) best = TechLevel.Industrial;
            else if (CountFinished(TechLevel.Medieval) >= 5) best = TechLevel.Medieval;
            best = (TechLevel)Mathf.Max((int)best, (int)floor);

            return best switch
            {
                TechLevel.Ultra => TorchDefOf.Torch_Ancestors_Ultra,
                TechLevel.Spacer => TorchDefOf.Torch_Ancestors_Spacer,
                TechLevel.Industrial => TorchDefOf.Torch_Ancestors_Industrial,
                TechLevel.Medieval => TorchDefOf.Torch_Ancestors_Medieval,
                _ => TorchDefOf.Torch_Ancestors_Industrial,
            };
        }
    }
}
