using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PassingTheTorch
{
    public class Dialog_TorchDeparture : Window
    {
        private readonly List<Pawn> candidateHeirs;
        private readonly Action<List<Thing>> onAccept;
        private readonly int maxColonists;
        private readonly int maxAnimals;
        private readonly int maxItems;

        private readonly List<Thing> colonists = [];
        private readonly List<Thing> animals = [];
        private List<Thing> items = [];

        private readonly Dictionary<Thing, int> itemArchonexusAllowedStackCount = [];
        private readonly HashSet<Thing> selected = [];
        private int selectedItemCount;
        private Vector2 scrollPosition = Vector2.zero;

        private const float TitleHeight = 40f;
        private const float DescriptionHeight = 60f;
        private const float BottomAreaHeight = 70f;
        private readonly Vector2 BottomButtonSize = new(160f, 40f);
        private const float RowHeight = 30f;
        private const float CategoryHeaderHeight = 30f;

        private readonly Comparer<ThingDef> itemComparator = Comparer<ThingDef>.Create(TransferableComparer_Archonexus.Compare);

        public override Vector2 InitialSize => new(540f, 800f);

        public Dialog_TorchDeparture(List<Pawn> candidateHeirs, Action<List<Thing>> onAccept)
        {
            this.candidateHeirs = candidateHeirs;
            this.candidateHeirs.RemoveAll(x => x == null || x.Dead || x.Destroyed);
            this.onAccept = onAccept;
            forcePause = true;
            closeOnCancel = false;
            doCloseX = false;
            absorbInputAroundWindow = true;
            preventSave = true;

            maxColonists = Math.Max(PassingTheTorchMod.settings.maxColonists, candidateHeirs.Count);
            maxAnimals = PassingTheTorchMod.settings.maxAnimals;
            maxItems = 15;
        }

        private int ColonistCount => colonists.Count(selected.Contains);
        private int AnimalCount => animals.Count(selected.Contains);

        private AcceptanceReport AcceptanceReport
        {
            get
            {
                if (ColonistCount > maxColonists)
                {
                    return "MessageNewColonyMax".Translate(maxColonists, "People".Translate());
                }

                if (AnimalCount > maxAnimals)
                {
                    return "MessageNewColonyMax".Translate(maxAnimals, "AnimalsLower".Translate());
                }

                if (selectedItemCount > maxItems)
                {
                    return "MessageNewColonyMax".Translate(maxItems, "ItemsLower".Translate());
                }

                return AcceptanceReport.WasAccepted;
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
            colonists.Clear();
            animals.Clear();
            items.Clear();
            selected.Clear();
            selectedItemCount = 0;
            itemArchonexusAllowedStackCount.Clear();

            foreach (var p in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction)
           {
               if (!p.IsQuestLodger())
               {
                   if (p.IsColonist)
                   {
                       if (candidateHeirs.Contains(p))
                       {
                           colonists.Add(p);
                       }
                   }
                   else if (p.IsAnimal)
                   {
                       animals.Add(p);
                   }
               }
           }

            Dictionary<ThingDef, List<Thing>> itemDefMap = [];
            List<Thing> distinctItem = [];
            List<Thing> tmpList = [];

            foreach (var map in Find.Maps)
            {
                ThingOwnerUtility.GetAllThingsRecursively(map, tmpList, allowUnreal: false, WealthWatcher.WealthItemsFilter);
                foreach (var t in tmpList)
                {
                    CountThing(t);
                }
            }
            foreach (var caravan in Find.WorldObjects.Caravans)
            {
                if (caravan.IsPlayerControlled)
                {
                    ThingOwnerUtility.GetAllThingsRecursively(caravan, tmpList, allowUnreal: false, WealthWatcher.WealthItemsFilter);
                    foreach (var t in tmpList)
                    {
                        CountThing(t);
                    }
                }
            }

            foreach (var value in itemDefMap.Values)
            {
                items.AddRange(value);
            }

            items.AddRange(distinctItem);

            colonists.SortBy((Thing t) => t.Label);
            animals.SortBy((Thing t) => t.Label);
            items = items.OrderByDescending((Thing i) => i.def, itemComparator)
                         .ThenBy((Thing i) => i.def.label)
                         .ThenByDescending((Thing i) => { i.TryGetQuality(out var qc); return qc; })
                         .ToList();

            foreach (var heir in candidateHeirs)
            {
                if (!selected.Contains(heir))
                {
                    selected.Add(heir);
                }

                if (!colonists.Contains(heir))
                {
                    colonists.Add(heir);
                }
            }

            void CountThing(Thing t)
            {
                if (t.def.ArchonexusMaxAllowedCount != 0 && MoveColonyUtility.IsBringableItem(t))
                {
                    if (!itemDefMap.ContainsKey(t.def))
                    {
                        itemDefMap[t.def] = [];
                    }

                    if (MoveColonyUtility.IsDistinctArchonexusItem(t.def))
                    {
                        distinctItem.Add(t);
                    }
                    else
                    {
                        foreach (var existing in itemDefMap[t.def])
                        {
                            if (t.CanStackWith(existing))
                            {
                                itemArchonexusAllowedStackCount[existing] = Mathf.Min(t.def.ArchonexusMaxAllowedCount, t.stackCount + itemArchonexusAllowedStackCount[existing]);
                                return;
                            }
                        }
                        itemArchonexusAllowedStackCount[t] = Mathf.Min(t.def.ArchonexusMaxAllowedCount, t.stackCount);
                        itemDefMap[t.def].Add(t);
                    }
                }
            }
        }

        public override void DoWindowContents(Rect rect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, rect.width, TitleHeight), "Torch_DialogTitle".Translate());

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, TitleHeight, rect.width, DescriptionHeight), "Torch_DialogDesc".Translate());

            float availableWidth = rect.width;
            var outRect = new Rect(rect.x, rect.y + TitleHeight + DescriptionHeight + 10f, rect.width, rect.height - TitleHeight - DescriptionHeight - BottomAreaHeight - 10f);

            float contentHeight = CalculateSectionHeight(colonists.Count) +
                                  CalculateSectionHeight(animals.Count) +
                                  CalculateSectionHeight(items.Count);

            if (contentHeight > outRect.height)
            {
                availableWidth -= 16f;
            }

            var viewRect = new Rect(0f, 0f, availableWidth, contentHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float curY = 0f;
            if (colonists.Count > 0)
           {
               DrawThingList(colonists, ref curY, availableWidth, "ChoosePeopleDesc".Translate(maxColonists), false);
           }

           if (animals.Count > 0)
           {
               DrawThingList(animals, ref curY, availableWidth, "ChooseThingsDesc".Translate(maxAnimals, "AnimalsLower".Translate()), false);
           }

           if (items.Count > 0)
           {
               DrawThingList(items, ref curY, availableWidth, "ChooseThingsDesc".Translate(maxItems, "ItemsLower".Translate()), true);
           }

            Widgets.EndScrollView();

            DoBottomButtons(rect);
        }

        private void DoBottomButtons(Rect rect)
        {
            var bottomArea = new Rect(0f, rect.yMax - BottomAreaHeight, rect.width, BottomAreaHeight);
            var acceptRect = new Rect(bottomArea.xMax - BottomButtonSize.x, bottomArea.yMax - BottomButtonSize.y, BottomButtonSize.x, BottomButtonSize.y);

            var report = AcceptanceReport;

            if (!report.Accepted)
            {
                var warningRect = acceptRect;
                warningRect.x -= 162f;
                warningRect.width = 160f;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleRight;
                GUI.color = Color.red;
                Widgets.Label(warningRect, report.Reason);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }

            if (Widgets.ButtonText(acceptRect, "Torch_ButtonDepart".Translate()))
            {
                if (report.Accepted)
                {
                    foreach (var t in selected)
                    {
                        if (itemArchonexusAllowedStackCount.ContainsKey(t))
                        {
                            t.stackCount = itemArchonexusAllowedStackCount[t];
                        }

                        if (itemArchonexusAllowedStackCount.ContainsKey(t) || items.Contains(t))
                        {
                            t.HitPoints = t.MaxHitPoints;
                        }
                    }
                    Close();
                    onAccept(selected.ToList());
                }
                else
                {
                    Messages.Message(report.Reason, MessageTypeDefOf.RejectInput, historical: false);
                }
            }
        }

        private float CalculateSectionHeight(int count)
        {
            return count == 0 ? 0f : CategoryHeaderHeight + (count * RowHeight);
        }

        private void DrawThingList(List<Thing> things, ref float curY, float width, string title, bool isItem)
        {
            var headerRect = new Rect(0f, curY, width, CategoryHeaderHeight);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;
            Widgets.Label(headerRect, title);
            Widgets.DrawLineHorizontal(0f, curY + 21f, width);
            Text.Anchor = TextAnchor.UpperLeft;

            curY += CategoryHeaderHeight;

            for (int i = 0; i < things.Count; i++)
            {
                var rowRect = new Rect(0f, curY, width, RowHeight);
                DoRow(rowRect, things[i], i, isItem);
                curY += RowHeight;
            }
        }

        private void DoRow(Rect rect, Thing t, int index, bool isItem)
        {
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect);
            }

            Widgets.BeginGroup(rect);

            Widgets.InfoCardButton(0f, 0f, t);
            Widgets.ThingIcon(new Rect(36f, 0f, 27f, 27f), t);

            string label = t.LabelCap;
            var p = t as Pawn;
            if (p != null && p.RaceProps.Animal)
            {
                label = p.LabelCap + $" ({p.GetGenderLabel()}, {Mathf.FloorToInt(p.ageTracker.AgeBiologicalYearsFloat)})";
            }
            else if (isItem)
            {
                label = GenLabel.ThingLabel(t, 1, includeHp: false).CapitalizeFirst();
            }

            var labelRect = new Rect(80f, 0f, isItem ? 250f : 196f, rect.height);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;

            Widgets.Label(labelRect, label);
            Text.WordWrap = true;

            if (Mouse.IsOver(labelRect))
            {
                Widgets.DrawHighlight(labelRect);
                string tooltip = label;
                if (p != null)
                {
                    tooltip += "\n\n" + p.def.description;
                }
                else
                {
                    tooltip += "\n\n" + t.DescriptionDetailed;
                }

                TooltipHandler.TipRegion(labelRect, tooltip);
            }

            float rightX = 320f;
            if (p != null)
            {
                if (p.Ideo != null && !Find.IdeoManager.classicMode)
                {
                    var iconRect = new Rect(rightX, 0f, 27f, rect.height);
                    p.Ideo.DrawIcon(iconRect);
                    TooltipHandler.TipRegion(iconRect, p.Ideo.name);
                    rightX += 27f;
                }

                if (p.RaceProps.Animal)
                {
                    if (p.health.hediffSet.HasHediff(HediffDefOf.Pregnant, mustBeVisible: true))
                    {
                        TransferableUIUtility.DrawPregnancyIcon(p, new Rect(rightX, 2f, 23f, rect.height - 4f));
                        rightX += 27f;
                    }
                    if (p.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) != null)
                    {
                        TransferableUIUtility.DrawBondedIcon(p, new Rect(rightX, 2f, 23f, rect.height - 4f));
                    }
                }
            }

            if (isItem)
            {
                var countRect = new Rect(rect.width - 27f - 4f - 60f - 10f, 1f, 60f, rect.height);
                Text.Anchor = TextAnchor.MiddleRight;
                int count = MoveColonyUtility.IsDistinctArchonexusItem(t.def) ? t.stackCount : itemArchonexusAllowedStackCount[t];
                Widgets.Label(countRect, count.ToString());
                Text.Anchor = TextAnchor.UpperLeft;
            }

            var checkRect = new Rect(rect.width - 27f - 4f, 1f, 27f, 27f);

            bool isSelected = selected.Contains(t);
            bool wasSelected = isSelected;
            Widgets.Checkbox(checkRect.x, checkRect.y, ref isSelected, 24f);
            if (isSelected != wasSelected)
            {
                if (isSelected)
                {
                    selected.Add(t);
                    if (isItem)
                    {
                        selectedItemCount++;
                    }

                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                }
                else
                {
                    selected.Remove(t);
                    if (isItem)
                    {
                        selectedItemCount--;
                    }

                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                }
            }

            GenUI.ResetLabelAlign();
            Widgets.EndGroup();
        }
    }
}
