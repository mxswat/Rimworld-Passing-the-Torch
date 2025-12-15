using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;
using RimWorld;

namespace PassingTheTorch
{
    [StaticConstructorOnStartup]
    public class Reward_TorchGenerationalDeparture : Reward
    {
        private static readonly Texture2D Icon = ThingDefOf.TorchLamp.uiIcon;

        public override IEnumerable<GenUI.AnonymousStackElement> StackElements
        {
            get
            {
                yield return QuestPartUtility.GetStandardRewardStackElement("Reward_TorchGenerationalDeparture_Label".Translate(), Icon, () => GetDescription(default).CapitalizeFirst() + ".");
            }
        }

        public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
        {
            throw new NotImplementedException();
        }

        public override string GetDescription(RewardsGeneratorParams parms)
        {
            return "Reward_TorchGenerationalDeparture".Translate().Resolve();
        }
    }
}
