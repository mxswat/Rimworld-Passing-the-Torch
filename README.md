# Passing the Torch

A generational RimWorld ending inspired by the Archonexus finale. When your colony has raised at least three grown-up heirs, a quest fires: organize a farewell party, pack supplies, and send a new generation to found their own colony on a new tile. The original settlement becomes an independent NPC faction, the Ancestral Home, that remembers your legacy.

## Features

- **Quest-driven departure.** Once 3+ valid heirs exist, the "Passing the Torch" quest becomes available. Heirs are adults with `AdultColonist` / `AdultTribal` / `VatGrown` backstories who have not already departed.

- **Farewell party.** Accepting triggers a colony-wide gathering. All non-drafted, non-downed colonists join. Once the party ends, the selection dialog opens.

- **Selection dialog.** Choose which pawns, animals, and items the heirs take (Vanilla Archonexus-style limits: 15 items, configurable pawn/animal caps). Departing pawns get the `Torch_FarewellParty` thought (+15 mood, 15 days).

- **Cinematic handoff.** A full-screen fade-to-black transition plays, then you pick the new settlement tile.

- **New colony generation.** The heirs caravan to the new tile, a new map is generated, and the old maps are deinitialized.

- **Ancestral Home faction.** All staying pawns (free colonists not in the departing group) are transferred to a new `Torch_Ancestors` faction. The eldest becomes the faction leader. Old settlements become Ancestral Home settlements at the same tiles. The Ancestral Home inherits all of the player's diplomatic relationships (bypasses vanilla permanent-enemy restrictions for Empire, rough outlanders, etc.), keeping your colony's legacy intact.

- **Research handling.** Optionally resets research to starting tech defaults (configurable in mod settings).

## Mod Settings

| Setting | Default | Description |
|---|---|---|
| Max colonists to take | 10 | How many pawns the heirs can bring |
| Max animals to take | 10 | How many animals the heirs can bring |
| Keep research | Yes | Preserve all research on departure |
| Diplomacy on departure | Reset to neutral | Controls the new colony's starting relations: Keep All (no change), Reset to Neutral (0 goodwill with everyone), or Half Strength (half the player's existing goodwill) |

## Dependencies

- RimWorld 1.6 (Royalty, Ideology, Biotech, Anomaly, Odyssey)
- Harmony

## Notes

- The Ancestral Home uses outlander-style trading, backstories, and settlement generation. It retains the player's former ideology and faction name.
- The departing pawns' old settlements and maps are fully removed from the world.
- Heirs who have already departed (tracked via `TorchWorldComponent`) cannot be selected again.
- If all heirs are incapacitated or away in caravans, the quest will not allow acceptance.

## Credits

Created by Mx. Inspired by the vanilla Archonexus endgame sequence.
