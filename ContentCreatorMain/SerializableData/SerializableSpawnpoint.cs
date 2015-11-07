namespace MissionCreator.SerializableData
{
    public class SerializableSpawnpoint : SerializableObject
    {
        public uint WeaponHash { get; set; }
        public int WeaponAmmo { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }

        public bool SpawnInVehicle { get; set; }
        public int VehicleSeat { get; set; }
    }
}