using Rage;

namespace ContentCreator.SerializableData
{
    public class SerializablePed : SerializableObject
    {
        public bool FailMissionOnDeath { get; set; }
        public uint WeaponHash { get; set; }
        public int WeaponAmmo { get; set; }
        public int RelationshipGroup { get; set; }
        public int Behaviour { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }

        public bool SpawnInVehicle { get; set; }
        public int VehicleSeat { get; set; }
    }
}