using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using MissionCreator.SerializableData.Cutscenes;
using MissionCreator.SerializableData.Objectives;
using Rage;

namespace MissionCreator.SerializableData
{
    [XmlInclude(typeof(SerializableMarker))]
    [XmlInclude(typeof(SerializableCamera))]
    [XmlInclude(typeof(SerializableCutscene))]
    [XmlInclude(typeof(SerializableSubtitle))]
    [XmlInclude(typeof(SerializableObjective))]
    [XmlInclude(typeof(SerializableActorObjective))]
    [XmlInclude(typeof(SerializablePickupObjective))]
    [XmlInclude(typeof(SerializableVehicleObjective))]
    [Serializable]
    public class MissionData
    {
        public string Name { get; set; } 
        public string Description { get; set; }
        public string Author { get; set; }

        public int MaxWanted { get; set; }
        public int MinWanted { get; set; }

        public WeatherType Weather { get; set; }
        public int Time { get; set; }
        public int? TimeLimit { get; set; }

        public List<SerializableSpawnpoint> Spawnpoints { get; set; }

        public List<SerializablePed> Actors { get; set; } 
        public List<SerializableVehicle> Vehicles { get; set; }
        public List<SerializableObject> Objects { get; set; }
        public List<SerializablePickup> Pickups { get; set; }

        public List<SerializableObjective> Objectives { get; set; }
        public string[] ObjectiveNames { get; set; }

        public List<SerializableCutscene> Cutscenes { get; set; }
    }
}