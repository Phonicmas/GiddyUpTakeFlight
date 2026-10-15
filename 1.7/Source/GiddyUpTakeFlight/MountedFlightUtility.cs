using GiddyUp;
using RimWorld;
using UnityEngine;
using Verse;

namespace GiddyUpTakeFlight
{
    public static class MountedFlightUtility
    {
        public static bool Launching;

        private static readonly TargetingParameters LandingTargetParams = new TargetingParameters
        {
            canTargetLocations = true,
            canTargetSelf = false,
            canTargetPawns = false,
            canTargetBuildings = false,
            canTargetItems = false,
            canTargetPlants = false,
            canTargetFires = false,
            canTargetCorpses = false
        };

        /// <summary>
        /// Whether a rider can take off right now. Reason is null when the command shouldn't be shown at all.
        /// </summary>
        public static bool CanTakeOff(Pawn rider, Pawn mount, out string reason) =>
            CanTakeOff(rider, mount, LoadInfo.For(rider, mount), out reason);

        public static bool CanTakeOff(Pawn rider, Pawn mount, LoadInfo load, out string reason)
        {
            reason = null;
            var map = rider.Map;
            if (map == null || !mount.Spawned)
                return false;

            if (IsFatigued(mount, out reason))
                return false;

            if (load.Overloaded)
            {
                reason = WorldFlightUtility.OverloadedReason(mount, load);
                return false;
            }

            if (rider.carryTracker?.CarriedThing != null)
            {
                reason = "Phon_TF_CarryingSomething".Translate(rider.Named("RIDER"));
                return false;
            }

            if (map.Biome != null && map.Biome.inVacuum && !mount.RaceProps.canFlyInVacuum)
            {
                reason = "Phon_TF_Vacuum".Translate(mount.Named("MOUNT"));
                return false;
            }

            if (TakeFlightSettings.requireOpenSky && rider.Position.Roofed(map))
            {
                reason = "Phon_TF_RoofedTakeOff".Translate();
                return false;
            }

            return true;
        }

        public static bool IsFatigued(Pawn mount, out string reason)
        {
            reason = null;
            var fatigue = mount.health?.hediffSet?.GetFirstHediffOfDef(TakeFlightDefOf.Phon_FlightFatigue);
            if (fatigue == null)
                return false;
            var ticksLeft = fatigue.TryGetComp<HediffComp_Disappears>()?.ticksToDisappear ?? 0;
            reason = "Phon_TF_MountFatigued".Translate(mount.Named("MOUNT"),
                ticksLeft.ToStringTicksToPeriod().Named("TIME"));
            return true;
        }

        public static void ApplyFatigue(Pawn mount, int ticks)
        {
            if (ticks <= 0 || mount?.health == null)
                return;
            var fatigue = mount.health.AddHediff(TakeFlightDefOf.Phon_FlightFatigue);
            fatigue?.TryGetComp<HediffComp_Disappears>()?.SetDuration(ticks);
        }

        public static bool CanLandAt(Pawn rider, Pawn mount, IntVec3 cell, out string reason)
        {
            reason = null;
            var map = rider.Map;
            if (map == null || !cell.IsValid || !cell.InBounds(map) || cell == rider.Position)
                return false;

            if (cell.DistanceTo(rider.Position) > FlyerUtility.FlightRangeFor(mount))
            {
                reason = "Phon_TF_OutOfRange".Translate();
                return false;
            }

            if (cell.Fogged(map) || !cell.Standable(map) || !JumpUtility.ValidJumpTarget(mount, map, cell))
            {
                reason = "Phon_TF_CannotLand".Translate();
                return false;
            }

            if (TakeFlightSettings.requireOpenSky && cell.Roofed(map))
            {
                reason = "Phon_TF_RoofedLanding".Translate();
                return false;
            }

            return true;
        }

        public static Command MakeTakeFlightCommand(Pawn rider, Pawn mount, LoadInfo load)
        {
            var range = FlyerUtility.FlightRangeFor(mount);
            var cooldown = FlyerUtility.CooldownTicksFor(mount);
            var description = "Phon_TF_TakeFlightDesc".Translate(mount.Named("MOUNT"), Mathf.RoundToInt(range).Named("RANGE"));
            if (cooldown > 0)
                description += "\n\n" + "Phon_TF_TakeFlightCooldown".Translate(mount.Named("MOUNT"),
                    cooldown.ToStringTicksToPeriod().Named("TIME"));

            var command = new Command_Action
            {
                defaultLabel = "Phon_TF_TakeFlight".Translate(),
                defaultDesc = description,
                icon = TakeFlightTextures.TakeFlight,
                groupable = false,
                action = () => BeginTargeting(rider, mount)
            };

            if (!CanTakeOff(rider, mount, load, out var reason))
                command.Disable(reason);
            return command;
        }

        private static void BeginTargeting(Pawn rider, Pawn mount)
        {
            Find.Targeter.BeginTargeting(LandingTargetParams,
                target => TryLaunch(rider, mount, target.Cell),
                target =>
                {
                    if (CanLandAt(rider, mount, target.Cell, out _))
                        GenDraw.DrawTargetHighlight(target);
                },
                target => CanLandAt(rider, mount, target.Cell, out _),
                rider,
                null,
                TakeFlightTextures.TakeFlight,
                true,
                target =>
                {
                    if (!CanLandAt(rider, mount, target.Cell, out var reason) && !reason.NullOrEmpty())
                        Widgets.MouseAttachedLabel(reason);
                },
                _ =>
                {
                    if (rider.Spawned)
                        GenDraw.DrawRadiusRing(rider.Position,
                            Mathf.Min(FlyerUtility.FlightRangeFor(mount), GenRadial.MaxRadialPatternRadius - 1f));
                });
        }

        private static void TryLaunch(Pawn rider, Pawn mount, IntVec3 cell)
        {
            if (!rider.Spawned || !FlyerUtility.TryGetFlyingMount(rider, out var currentMount) || currentMount != mount)
                return;
            if (!CanTakeOff(rider, mount, out _) || !CanLandAt(rider, mount, cell, out _))
                return;
            Launch(rider, mount, cell);
        }

        /// <summary>
        /// Launches a mounted rider and mount to a cell. Giddy-Up's mounted state is kept for the whole flight.
        /// </summary>
        public static void Launch(Pawn rider, Pawn mount, IntVec3 destination)
        {
            var map = rider.Map;
            var start = rider.Position;
            var riderSelected = Find.Selector.IsSelected(rider);
            var riderData = rider.GetExtendedPawnData();

            var facing = Rot4.FromAngleFlat((destination - start).AngleFlat);
            rider.Rotation = facing;
            mount.Rotation = facing;

            LifeStageUtility.PlayNearestLifestageSound(mount, ls => ls.soundCall, g => g.soundCall, m => m.soundCall);
            FleckMaker.ThrowDustPuff(start.ToVector3Shifted() + Gen.RandomHorizontalVector(0.5f), map,
                Mathf.Clamp(mount.BodySize, 1.5f, 4f));

            Launching = true;
            try
            {
                riderData.Mount = null;
                mount.jobs?.StopAll();
                riderData.Mount = mount;
                rider.jobs?.StopAll();

                var flyer = (MountedPawnFlyer)PawnFlyer.MakeFlyer(TakeFlightDefOf.Phon_MountedPawnFlyer, mount,
                    destination, null, null);
                flyer.AddRider(rider);
                GenSpawn.Spawn(flyer, destination, map);
            }
            finally
            {
                Launching = false;
            }

            ApplyFatigue(mount, FlyerUtility.CooldownTicksFor(mount));

            if (riderSelected)
                Find.Selector.Select(rider, false, false);
        }
    }
}
