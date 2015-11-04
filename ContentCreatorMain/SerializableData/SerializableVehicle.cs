using System.Drawing;
using Rage;

namespace ContentCreator.SerializableData
{
    public class SerializableVehicle : SerializableObject
    {
        public Color PrimaryColor { get; set; }
        public Color SecondaryColor { get; set; }
        public bool FailMissionOnDeath { get; set; }
        public int Health { get; set; }
    }
}