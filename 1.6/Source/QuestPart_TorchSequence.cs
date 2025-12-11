using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace PassingTheTorch
{
    public class QuestPart_TorchSequence : QuestPartActivable
    {
        public List<Pawn> candidateHeirs = [];
        private bool partyStarted;
        private Pawn organizer;
        
        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            
            if (State == QuestPartState.Enabled && !partyStarted)
            {
                StartFarewellParty();
            }
        }

        private void StartFarewellParty()
        {
            partyStarted = true;
            organizer = candidateHeirs.FirstOrDefault(x => x.Spawned && !x.Downed && !x.InMentalState);
            if (organizer != null && RCellFinder.TryFindGatheringSpot(organizer, GatheringDefOf.Party, ignoreRequiredColonistCount: true, out var spot))
            {
                var job = new LordJob_Joinable_Party(spot, organizer, GatheringDefOf.Party);
                var lord = LordMaker.MakeNewLord(Faction.OfPlayer, job, organizer.Map);
                lord.AddPawn(organizer);
                Find.LetterStack.ReceiveLetter("Torch_LetterLabel_FarewellParty".Translate(), "Torch_LetterText_FarewellParty".Translate(), LetterDefOf.PositiveEvent, new LookTargets(spot, organizer.Map));
            }
            else
            {
                Log.Message("Cant find spot for farevel party for organizer " + organizer);
            }
        }

        public override void QuestPartTick()
        {
            if (partyStarted && organizer?.GetLord() is null)
            {
                partyStarted = false;
                OpenDepartureDialog();
            }
        }

        private void OpenDepartureDialog()
        {
            candidateHeirs.RemoveAll(x => x == null || x.Dead || x.Destroyed);
            Find.WindowStack.Add(new Dialog_TorchDeparture(candidateHeirs, PostThingsSelected));
        }
        private void PostThingsSelected(List<Thing> selectedThings)
        {
            Find.WindowStack.Add(new Screen_TorchSettlementCinematics(
                () =>
                {
                    var firstPawn = selectedThings.OfType<Pawn>().FirstOrDefault();
                    if (firstPawn != null)
                    {
                        CameraJumper.TryJump(CameraJumper.GetWorldTarget(firstPawn));
                    }
                },
                () =>
                {
                    MoveColonyUtility.PickNewColonyTile(
                        (tile) => TileChosen(tile, selectedThings),
                        () => OpenDepartureDialog()
                    );
                    ScreenFader.StartFade(UnityEngine.Color.clear, 2f);
                }
            ));
        }
        private void TileChosen(PlanetTile tile, List<Thing> selectedThings)
        {
            LongEventHandler.QueueLongEvent(() =>
            {
                TorchTransitionUtility.ExecuteMove(tile, selectedThings, quest);
            }, "GeneratingMap", doAsynchronously: false, null);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref candidateHeirs, "heirs", LookMode.Reference);
            Scribe_Values.Look(ref partyStarted, "partyStarted");
            Scribe_References.Look(ref organizer, "organizer");
        }
    }
}
