using Rage;

namespace ContentCreator.SerializableData.Objectives
{
    public class SerializableObjective
    {
        public Vector3 Position { get; set; }
        public Rotator Rotation { get; set; }
        public int SpawnAfter { get; set; }
        public int ActivateAfter { get; set; }
    }
}