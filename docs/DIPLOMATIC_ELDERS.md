# Diplomatic Elders: Discrepancy Between Observed and Expected Behavior

## What you see

When a visitor caravan or trader caravan arrives from the ancestral home, the pawns have:

- The same names as elders you left behind
- The same bios and backstories
- The special ideology roles they had (leader, moral guide, etc.)
- But **not** the gear they wore (no cataphract armor)

## What the code says should happen

### The patch is limited to Settlement kind

`PawnGroupMakerUtility_GeneratePawns_Patch.cs` only fires when `parms.groupKind == PawnGroupKindDefOf.Settlement`. This means it ONLY triggers when the ancestral home's *settlement defenders* are being generated — i.e., when you attack the settlement.

For visitors (`Peaceful`) and trader caravans (`Trader`), the patch returns immediately without touching anything. The visitors are generated fresh by RimWorld's normal pawn generation.

### Hospitality/RimQuest was checked — not the cause

Decompiled both `RimQuest.dll` and `HospitalityPatch.dll`. Neither mod pulls world pawns for visitors. They only add quest givers to existing visitor groups. The actual guest system is vanilla RimWorld's `IncidentWorker_VisitorGroup`.

### How fresh pawns can match elders

The ancestral home faction's backstory filter is:
```xml
<backstoryFilters>
  <li>
    <categories>
      <li>Offworld</li>
      <li>Civil</li>
    </categories>
  </li>
</backstoryFilters>
```

This is the same pool your original colonists came from. When RimWorld generates a fresh pawn for this faction, it picks a random backstory from this pool. Since the pool is relatively small (~20-30 backstories) and the faction shares your ideology, you'll see repeats:

| Observation | Cause |
|---|---|
| Same name | Backstory pool overlap + culture-linked name templates |
| Same backstory | Only 20-30 Offworld/Civil stories; odds of repeating are high |
| Same ideology role | Faction inherits player's ideology → generated pawns auto-fill required roles |
| Different gear | Fresh pawns get default faction equipment (`Villager`/`Tribal_Warrior` gear tables), not accumulated gear |

### Why elders in visitors have wrong gear

**They aren't elders.** They're new pawns generated from the same template. A visitor named "Jane" with a "Miner" backstory is NOT your elder Jane — it's a new pawn that happened to get that same backstory from the pool.

### How to verify

In dev mode, check the **World Pawns** window (Debug → World pawns). Your actual elders (with cataphract armor intact) should still be listed there. The pawns in the visitor caravan are separate generated objects.

## What happens when you attack the settlement

Our patch fires for `Settlement` kind. It scans world pawns for actual elders, removes them from the world pawn pool, and spawns them on the map as defenders. **These** elders have their original gear because they were stored intact in world pawns the whole time.

## Summary

| Scenario | Pawn source | Has old gear? | Has old name/bio? |
|---|---|---|---|
| Visitor caravan | Fresh generated from backstory pool | No (default gear) | Coincidentally yes (shared pool) |
| Trader caravan | Fresh generated from backstory pool | No (default gear) | Coincidentally yes (shared pool) |
| Settlement attack | Actual elders from world pawns | Yes | Actually yes |
