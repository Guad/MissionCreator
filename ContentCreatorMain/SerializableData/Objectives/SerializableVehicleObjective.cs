using System.Drawing;
using Rage;

namespace ContentCreator.SerializableData.Objectives
{
    public class SerializableVehicleObjective : SerializableObjective
    {
        public uint ModelHash { get; set; }
        public Vector3 PrimaryColor { get; set; }
        public Vector3 SecondaryColor { get; set; }
        public int Health { get; set; }

        private Vehicle _veh;

        public virtual void SetVehicle(Vehicle veh)
        {
            _veh = veh;
        }

        public virtual Vehicle GetVehicle()
        {
            return _veh;
        }
    }
}