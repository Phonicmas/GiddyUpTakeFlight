using GiddyUp;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace GiddyUpTakeFlight.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetPathContext))]
    public static class Patch_Pawn_GetPathContext
    {
        public static void Postfix(Pawn __instance, Pathing pathing, ref PathingContext __result)
        {
            if (!TakeFlightSettings.ignoreTerrain || pathing?.Flying == null)
                return;
            if (FlyerUtility.IsRidingFlyer(__instance))
                __result = pathing.Flying;
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.WaterCellCost), MethodType.Getter)]
    public static class Patch_Pawn_WaterCellCost
    {
        public static void Postfix(Pawn __instance, ref int? __result)
        {
            if (!TakeFlightSettings.ignoreTerrain || __result == 1)
                return;
            if (FlyerUtility.IsRidingFlyer(__instance))
                __result = 1;
        }
    }

    [HarmonyPatch(typeof(PathFinder), nameof(PathFinder.CreateRequest), typeof(IntVec3), typeof(LocalTargetInfo), typeof(IntVec3?), typeof(TraverseParms), typeof(PathFinderCostTuning?), typeof(PathEndMode), typeof(Pawn), typeof(PathRequest.IPathGridCustomizer))]
    public static class Patch_PathFinder_CreateRequest
    {
        public static void Postfix(PathRequest __result, Pawn pawn)
        {
            if (__result == null || pawn == null || __result.customizer != null || !TakeFlightSettings.ignoreTerrain)
                return;
            if (FlyerUtility.IsRidingFlyer(pawn))
                __result.customizer = FlyerPathGridCustomizer.Instance;
        }
    }

    [HarmonyPatch(typeof(PathFinderMapData), nameof(PathFinderMapData.ParameterizeGridJob))]
    public static class Patch_PathFinderMapData_ParameterizeGridJob
    {
        private static readonly AccessTools.FieldRef<PathFinderMapData, CostSource> FlyingCost =
            AccessTools.FieldRefAccess<PathFinderMapData, CostSource>("flyingCost");

        public static void Postfix(PathFinderMapData __instance, PathRequest request, ref PathGridJob job)
        {
            if (request?.customizer is not FlyerPathGridCustomizer)
                return;
            var flyingCost = FlyingCost(__instance);
            if (flyingCost != null)
                job.pathGridDirect = flyingCost.Data;
        }
    }

    [HarmonyPatch(typeof(MountUtility), nameof(MountUtility.Dismount))]
    public static class Patch_MountUtility_Dismount
    {
        public static void Postfix(Pawn rider, Pawn animal)
        {
            if (MountedFlightUtility.Launching || !UnityData.IsInMainThread)
                return;
            FlyerUtility.MoveOffUnwalkableCell(rider);
            FlyerUtility.MoveOffUnwalkableCell(animal);
        }
    }
}
