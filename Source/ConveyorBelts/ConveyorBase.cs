using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ConveyorBelts
{
    public class Building_ConveyorBase : Building, ITweenThings
    {
        public int StartingTicksToMoveThing (Thing thing) => 120;

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);

            var tiles = AdjacentConveyorTiles.ToArray();

            // make adjacent nodes that point at nothing point at this
            foreach (var adjacent in tiles)
            {
                if(Map.thingGrid.ThingAt<Building_ConveyorBase>(adjacent.NextCell) == null)
                    adjacent.Rotation = Rot4.FromIntVec3(Position - adjacent.Position);
            }
            // if this points at nothing, point at first thing that doesn't point at this
            var pointsAt = Map.thingGrid.ThingAt<Building_ConveyorBase>(NextCell);
            if (pointsAt == null || tiles.Contains(pointsAt))
            {
                var next = tiles.FirstOrDefault(x => x.NextCell != Position);
                if(next != null)
                    Rotation = Rot4.FromIntVec3(next.Position - Position);
            }
        }

        public IEnumerable<Building_ConveyorBase> AdjacentConveyorTiles =>
            GenAdj.CardinalDirections.Select(x => x + Position).Select(x => Map.thingGrid.ThingAt<Building_ConveyorBase>(x)).Where(x => x != null);

        public override string GetInspectString() => $"{base.GetInspectString()}\r\nRotation: {Rotation.AsInt}";

        public IntVec3 NextCell => Position + Rotation.FacingCell;


        public Dictionary<Thing, int> StartingTicks { get; } = new Dictionary<Thing, int>();
        public Dictionary<Thing, int> RemainingTicks { get; } = new Dictionary<Thing, int>();

        public Gizmo Button => new Command_Action
        {
            action = () => {
                var rotation = Rotation;
                rotation.Rotate(RotationDirection.Clockwise);
                Rotation = rotation;
                },
            defaultDesc = "Rotate the direction of the belt clockwise",
            defaultLabel = "Rotate",
            disabled = false
        };

        public override IEnumerable<Gizmo> GetGizmos()
        {
            return base.GetGizmos().Concat(new[] {
                Button
            });
        }

        public override void Tick()
        {
            base.Tick();

            var things = Map.thingGrid.ThingsAt(Position).Where(x => x.def.category == ThingCategory.Item);

            foreach(var thing in things)
            {
                if (!StartingTicks.ContainsKey(thing))
                {
                    StartingTicks[thing] = RemainingTicks[thing] = StartingTicksToMoveThing(thing);
                }

                RemainingTicks[thing]--;

                if(RemainingTicks[thing] == 0)
                {
                    thing.Position = NextCell;
                    Map.mapDrawer.MapMeshDirty(NextCell, MapMeshFlag.Things, true, false);
                    StartingTicks.Remove(thing);
                    RemainingTicks.Remove(thing);
                }
            }
        }

        public Vector3 Offset(Thing thing)
        {
            if (!StartingTicks.ContainsKey(thing))
                return Vector3.zero;
            float num = (float)RemainingTicks[thing] / StartingTicks[thing];
            return (NextCell.ToVector3Shifted() - Position.ToVector3Shifted()) * (1f - num);
        }
    }
}
