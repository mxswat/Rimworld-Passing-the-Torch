using HarmonyLib;
using UnityEngine;
using Verse;

namespace PassingTheTorch
{
    public class PassingTheTorchMod : Mod
    {
        public static PassingTheTorchSettings settings;

        public PassingTheTorchMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<PassingTheTorchSettings>();
            new Harmony("Mx.PassingTheTorch").PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("Torch_SettingMaxColonists".Translate(settings.maxColonists));
            settings.maxColonists = (int)listing.Slider(settings.maxColonists, 1, 50);

            listing.Label("Torch_SettingMaxAnimals".Translate(settings.maxAnimals));
            settings.maxAnimals = (int)listing.Slider(settings.maxAnimals, 0, 50);

            listing.Gap();
            listing.CheckboxLabeled("Torch_SettingKeepResearch".Translate(), ref settings.keepResearch, "Torch_SettingKeepResearchDesc".Translate());

            listing.Gap();
            Text.Font = GameFont.Tiny;
            listing.Label(("Torch_RelationReset_" + settings.relationResetMode + "_Desc").Translate());
            Text.Font = GameFont.Small;
            if (listing.ButtonText("Torch_SettingRelationReset".Translate() + ": " + ("Torch_RelationReset_" + settings.relationResetMode).Translate()))
            {
                var values = System.Enum.GetValues(typeof(TorchRelationResetMode));
                var currentIndex = System.Array.IndexOf(values, settings.relationResetMode);
                settings.relationResetMode = (TorchRelationResetMode)values.GetValue((currentIndex + 1) % values.Length);
            }

            listing.End();
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }
}
