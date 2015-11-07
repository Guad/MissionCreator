using Rage;

namespace MissionCreator.SerializableData
{
    public interface ISerializableEntity
    {
        Vector3 Position { get; set; } 
        Rotator Rotation { get; set; }
        uint ModelHash { get; set; }
        int SpawnAfter { get; set; }
        int RemoveAfter { get; set; }
    }
}