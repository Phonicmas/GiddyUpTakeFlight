using RimWorld;
using UnityEngine;
using Verse;

namespace GiddyUpTakeFlight
{
    [DefOf]
    public static class TakeFlightDefOf
    {
        public static ThingDef Phon_MountedPawnFlyer;
        public static HediffDef Phon_FlightFatigue;

        public static ThingDef Phon_MountedFlyerTransporter;
        public static ThingDef Phon_MountedFlyerActive;
        public static ThingDef Phon_MountedFlyerLeaving;
        public static ThingDef Phon_MountedFlyerIncoming;
        public static WorldObjectDef Phon_TravellingMountedFlyers;

        static TakeFlightDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TakeFlightDefOf));
        }
    }

    [StaticConstructorOnStartup]
    public static class TakeFlightTextures
    {
        public static readonly Texture2D TakeFlight = ContentFinder<Texture2D>.Get("UI/Commands/Phon_TakeFlight");
        public static readonly Texture2D FlyWorld = ContentFinder<Texture2D>.Get("UI/Commands/Phon_FlyWorld");
    }
}
