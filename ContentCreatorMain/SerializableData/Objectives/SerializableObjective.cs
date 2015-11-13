using System;
using Rage;

namespace MissionCreator.SerializableData.Objectives
{
    public class SerializableObjective : ICloneable
    {
        public Vector3 Position { get; set; }
        public Rotator Rotation { get; set; }
        public int SpawnAfter { get; set; }
        public int ActivateAfter { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}