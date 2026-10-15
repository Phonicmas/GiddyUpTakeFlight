using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace GiddyUpTakeFlight.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
    public static class Patch_Pawn_GetGizmos
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (var gizmo in __result)
                yield return gizmo;

            if (!TakeFlightSettings.flightEnabled && !TakeFlightSettings.worldFlightEnabled)
                yield break;
            if (!__instance.Spawned || !__instance.IsColonistPlayerControlled)
                yield break;
            if (!FlyerUtility.TryGetFlyingMount(__instance, out var mount))
                yield break;

            var load = LoadInfo.For(__instance, mount);
            if (TakeFlightSettings.flightEnabled)
                yield return MountedFlightUtility.MakeTakeFlightCommand(__instance, mount, load);
            if (TakeFlightSettings.worldFlightEnabled)
                yield return WorldFlightUtility.MakeMapWorldFlightCommand(__instance, mount, load);
        }
    }

    [HarmonyPatch(typeof(Caravan), nameof(Caravan.GetGizmos))]
    public static class Patch_Caravan_GetGizmos
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Caravan __instance)
        {
            foreach (var gizmo in __result)
                yield return gizmo;

            if (!TakeFlightSettings.worldFlightEnabled || !__instance.IsPlayerControlled)
                yield break;
            var command = WorldFlightUtility.CaravanWorldFlightGizmo(__instance);
            if (command != null)
                yield return command;
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.JobTrackerTickInterval))]
    public static class Patch_Pawn_JobTracker_JobTrackerTickInterval
    {
        public static bool Prefix(Pawn ___pawn)
        {
            if (___pawn == null)
                return true;
            var holder = ___pawn.ParentHolder;
            return holder is not MountedPawnFlyer &&
                   !(holder is ActiveTransporterInfo info && WorldFlightUtility.IsOurTransporter(info));
        }
    }

    [HarmonyPatch(typeof(TravellingTransporters), "TraveledPctStepPerTick", MethodType.Getter)]
    public static class Patch_TravellingTransporters_TraveledPctStepPerTick
    {
        public static void Postfix(TravellingTransporters __instance, ref float __result)
        {
            if (__instance is TravellingMountedFlyers flyers && __result < 1f)
                __result *= flyers.SpeedFactor;
        }
    }
}
