using System.Collections.Generic;

namespace ContentCreator.SerializableData
{
    public class MissionData
    {
        public string Name { get; set; } 
        public string Description { get; set; }
        public string Author { get; set; }

        public List<SerializableSpawnpoint> Spawnpoints { get; set; }

        public List<SerializablePed> Actors { get; set; } 
        public List<SerializableVehicle> Vehicles { get; set; }
        public List<SerializableObject> Objects { get; set; } 
    }
}