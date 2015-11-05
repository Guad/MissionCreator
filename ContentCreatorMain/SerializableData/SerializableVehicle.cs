using System.Drawing;
using Rage;

namespace ContentCreator.SerializableData
{
    public class SerializableVehicle : SerializableObject
    {
        public Vector3 PrimaryColor { get; set; }
        public Vector3 SecondaryColor { get; set; }
        public bool FailMissionOnDeath { get; set; }
        public int Health { get; set; }
    }
}