using System;
using Rage;

namespace MissionCreator.SerializableData
{
    public class SerializableObject : ISerializableEntity, ICloneable
    {
        public Vector3 Position { get; set; }
        public Rotator Rotation { get; set; }
        public uint ModelHash { get; set; }
        public int SpawnAfter { get; set; }
        public int RemoveAfter { get; set; }

        private Entity _veh;

        public virtual void SetEntity(Entity veh)
        {
            _veh = veh;
        }

        public virtual Entity GetEntity()
        {
            return _veh;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}