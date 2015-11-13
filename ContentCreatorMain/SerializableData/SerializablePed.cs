using System.Collections.Generic;
using MissionCreator.SerializableData.Waypoints;
using Rage;

namespace MissionCreator.SerializableData
{
    public class SerializablePed : SerializableObject
    {
        public List<SerializableWaypoint> Waypoints { get; set; }

        public bool FailMissionOnDeath { get; set; }
        public uint WeaponHash { get; set; }
        public int WeaponAmmo { get; set; }
        public int RelationshipGroup { get; set; }
        public int Behaviour { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }
        public int Accuracy { get; set; }

        public bool SpawnInVehicle { get; set; }
        public int VehicleSeat { get; set; }
    }
}