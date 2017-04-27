using HugsLib.Source.Detour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ConveyorBelts
{
    public interface ITweenThings
    {
        Vector3 Offset(Thing thing);
    }

    public class ThingTweener : Thing
    {
        [DetourProperty(typeof(Thing), nameof(Thing.DrawPos), DetourProperty.Getter)]
        public override Vector3 DrawPos
        {
            get
            {
                var tweener = Map.thingGrid.ThingAt<Building_ConveyorBase>(Position);
                if(tweener != null)
                {
                    return Gen.TrueCenter(this) + tweener.Offset(this);
                }
                    
                return Gen.TrueCenter(this);
            }
        }
    }
}
