using Rage;

namespace ContentCreator.SerializableData.Objectives
{
    public class SerializableActorObjective : SerializableObjective
    {
        public uint ModelHash { get; set; }
        public uint WeaponHash { get; set; }
        public int WeaponAmmo { get; set; }
        public int RelationshipGroup { get; set; }
        public int Behaviour { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }

        public bool ShowHealthBar { get; set; }
        public string Name { get; set; }

        public bool SpawnInVehicle { get; set; }
        public int VehicleSeat { get; set; }

        private Ped _veh;

        public virtual void SetPed(Ped veh)
        {
            _veh = veh;
        }

        public virtual Ped GetPed()
        {
            return _veh;
        }
    }
}