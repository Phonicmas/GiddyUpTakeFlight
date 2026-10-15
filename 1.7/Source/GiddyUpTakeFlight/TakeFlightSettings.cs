using System.Collections.Generic;
using Verse;

namespace GiddyUpTakeFlight
{
    public class TakeFlightSettings : ModSettings
    {
        public const float DefaultFlightRange = 30f;
        public const float DefaultCooldownHours = 2f;
        public const int DefaultWorldRange = 5;
        public const float DefaultMaxRangeMultiplier = 3f;
        public const float DefaultWorldSpeed = 120f;
        public const float DefaultTakeFlightSpeed = 18f;
        public const float DefaultRestHoursPerTile = 1f;

        public static bool ignoreTerrain = true;
        public static bool flightEnabled = true;
        public static bool requireOpenSky = true;
        public static float flightRange = DefaultFlightRange;
        public static float cooldownHours = DefaultCooldownHours;
        public static float takeFlightSpeed = DefaultTakeFlightSpeed;

        public static bool worldFlightEnabled = true;
        public static int defaultWorldRange = DefaultWorldRange;
        public static float maxRangeMultiplier = DefaultMaxRangeMultiplier;
        public static float worldFlightSpeed = DefaultWorldSpeed;
        public static float restHoursPerTile = DefaultRestHoursPerTile;

        public static HashSet<string> invertFlyerRules = new HashSet<string>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ignoreTerrain, "ignoreTerrain", true);
            Scribe_Values.Look(ref flightEnabled, "flightEnabled", true);
            Scribe_Values.Look(ref requireOpenSky, "requireOpenSky", true);
            Scribe_Values.Look(ref flightRange, "flightRange", DefaultFlightRange);
            Scribe_Values.Look(ref cooldownHours, "cooldownHours", DefaultCooldownHours);
            Scribe_Values.Look(ref takeFlightSpeed, "takeFlightSpeed", DefaultTakeFlightSpeed);
            Scribe_Values.Look(ref worldFlightEnabled, "worldFlightEnabled", true);
            Scribe_Values.Look(ref defaultWorldRange, "defaultWorldRange", DefaultWorldRange);
            Scribe_Values.Look(ref maxRangeMultiplier, "maxRangeMultiplier", DefaultMaxRangeMultiplier);
            Scribe_Values.Look(ref worldFlightSpeed, "worldFlightSpeed", DefaultWorldSpeed);
            Scribe_Values.Look(ref restHoursPerTile, "restHoursPerTile", DefaultRestHoursPerTile);
            Scribe_Collections.Look(ref invertFlyerRules, "invertFlyerRules", LookMode.Value);

            invertFlyerRules ??= new HashSet<string>();
        }
    }
}
