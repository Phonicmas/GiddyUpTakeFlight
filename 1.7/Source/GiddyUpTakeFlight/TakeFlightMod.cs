using System;
using System.Collections.Generic;
using GiddyUp;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace GiddyUpTakeFlight
{
    public class TakeFlightMod : Mod
    {
        public const string HarmonyId = "Phonicmas.GiddyUp.TakeFlight";

        private const float RowHeight = 28f;

        private static Vector2 scrollPosition;
        private static string searchText = "";
        private static string rowFilter;
        private static readonly List<ThingDef> RowDefs = new List<ThingDef>();
        private static readonly Dictionary<ThingDef, string> RowLabels = new Dictionary<ThingDef, string>();

        public TakeFlightMod(ModContentPack content) : base(content)
        {
            GetSettings<TakeFlightSettings>();
            new Harmony(HarmonyId).PatchAll();
        }

        public override string SettingsCategory() => "Giddy-Up 2: Take Flight";

        public override void WriteSettings()
        {
            base.WriteSettings();
            FlyerUtility.RebuildCache();
            rowFilter = null;
            RowLabels.Clear();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.CheckboxLabeled("Phon_TF_SettingIgnoreTerrain".Translate(), ref TakeFlightSettings.ignoreTerrain,
                "Phon_TF_SettingIgnoreTerrainDesc".Translate());
            listing.CheckboxLabeled("Phon_TF_SettingFlight".Translate(), ref TakeFlightSettings.flightEnabled,
                "Phon_TF_SettingFlightDesc".Translate());
            listing.CheckboxLabeled("Phon_TF_SettingOpenSky".Translate(), ref TakeFlightSettings.requireOpenSky,
                "Phon_TF_SettingOpenSkyDesc".Translate());

            TakeFlightSettings.flightRange = Mathf.Round(listing.SliderLabeled(
                "Phon_TF_SettingRange".Translate(Mathf.RoundToInt(TakeFlightSettings.flightRange).Named("RANGE")),
                TakeFlightSettings.flightRange, 10f, 50f));
            TakeFlightSettings.cooldownHours = Mathf.Round(listing.SliderLabeled(
                "Phon_TF_SettingCooldown".Translate(TakeFlightSettings.cooldownHours.ToString("0.#").Named("HOURS")),
                TakeFlightSettings.cooldownHours, 0f, 24f) * 2f) / 2f;

            TakeFlightSettings.takeFlightSpeed = Mathf.Round(listing.SliderLabeled(
                "Phon_TF_SettingTakeFlightSpeed".Translate(Mathf.RoundToInt(TakeFlightSettings.takeFlightSpeed).Named("SPEED")),
                TakeFlightSettings.takeFlightSpeed, 5f, 40f));

            listing.GapLine();
            listing.CheckboxLabeled("Phon_TF_SettingWorldFlight".Translate(), ref TakeFlightSettings.worldFlightEnabled,
                "Phon_TF_SettingWorldFlightDesc".Translate());
            TakeFlightSettings.defaultWorldRange = Mathf.RoundToInt(listing.SliderLabeled(
                "Phon_TF_SettingWorldRange".Translate(TakeFlightSettings.defaultWorldRange.Named("RANGE")),
                TakeFlightSettings.defaultWorldRange, 0f, 40f));
            TakeFlightSettings.maxRangeMultiplier = Mathf.Round(listing.SliderLabeled(
                "Phon_TF_SettingMaxMultiplier".Translate(TakeFlightSettings.maxRangeMultiplier.ToString("0.#").Named("MULT")),
                TakeFlightSettings.maxRangeMultiplier, 1f, 5f) * 10f) / 10f;
            TakeFlightSettings.worldFlightSpeed = Mathf.Round(listing.SliderLabeled(
                "Phon_TF_SettingWorldSpeed".Translate(Mathf.RoundToInt(TakeFlightSettings.worldFlightSpeed).Named("SPEED")),
                TakeFlightSettings.worldFlightSpeed, 10f, 400f) / 5f) * 5f;
            TakeFlightSettings.restHoursPerTile = Mathf.Round(listing.SliderLabeled(
                "Phon_TF_SettingRestPerTile".Translate(TakeFlightSettings.restHoursPerTile.ToString("0.##").Named("HOURS")),
                TakeFlightSettings.restHoursPerTile, 0f, 6f) * 4f) / 4f;

            listing.GapLine();
            listing.Label("Phon_TF_SettingFlyers".Translate());
            var searchRect = listing.GetRect(Text.LineHeight + 4f);
            searchText = Widgets.TextField(searchRect, searchText ?? "");
            var listTop = listing.CurHeight;
            listing.End();

            DrawFlyerList(new Rect(inRect.x, inRect.y + listTop + 4f, inRect.width, inRect.height - listTop - 4f));
        }

        private static void RebuildRows()
        {
            RowDefs.Clear();
            var filter = rowFilter;
            foreach (var def in Setup.AllAnimals)
            {
                if (def == null || !ModSettings_GiddyUp.MountableCache.Contains(def.shortHash))
                    continue;
                if (filter.Length > 0 &&
                    (def.label == null || def.label.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) &&
                    def.defName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                RowDefs.Add(def);
            }
        }

        private static string RowLabel(ThingDef def)
        {
            if (!RowLabels.TryGetValue(def, out var label))
            {
                label = def.LabelCap + " (" + def.defName + ")";
                RowLabels[def] = label;
            }
            return label;
        }

        private static void DrawFlyerList(Rect outRect)
        {
            var filter = searchText?.Trim() ?? "";
            if (rowFilter != filter)
            {
                rowFilter = filter;
                RebuildRows();
            }

            var viewRect = new Rect(0f, 0f, outRect.width - 16f, RowDefs.Count * RowHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            var first = Mathf.Max(0, Mathf.FloorToInt(scrollPosition.y / RowHeight));
            var last = Mathf.Min(RowDefs.Count - 1, Mathf.CeilToInt((scrollPosition.y + outRect.height) / RowHeight));
            for (var i = first; i <= last; i++)
            {
                var def = RowDefs[i];
                var row = new Rect(0f, i * RowHeight, viewRect.width, RowHeight);
                if (i % 2 == 1)
                    Widgets.DrawLightHighlight(row);
                Widgets.DrawHighlightIfMouseover(row);
                Widgets.ThingIcon(new Rect(row.x + 2f, row.y + 2f, RowHeight - 4f, RowHeight - 4f), def);

                var flyer = FlyerUtility.IsFlyerWithSettings(def);
                var before = flyer;
                Widgets.CheckboxLabeled(new Rect(row.x + RowHeight + 6f, row.y, row.width - RowHeight - 6f, RowHeight),
                    RowLabel(def), ref flyer);
                if (flyer != before)
                {
                    if (flyer == FlyerUtility.DefaultIsFlyer(def))
                        TakeFlightSettings.invertFlyerRules.Remove(def.defName);
                    else
                        TakeFlightSettings.invertFlyerRules.Add(def.defName);
                }
            }
            Widgets.EndScrollView();
        }
    }
}
