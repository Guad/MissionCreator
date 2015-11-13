using Rage;

namespace MissionCreator.SerializableData.Waypoints
{
    public enum WaypointTypes
    {
        Walk,
        Run,
        Animation,
        EnterVehicle,
        ExitVehicle,
        Wander,
        Shoot,
        Wait,
        Drive
    }

    public class SerializableWaypoint 
    {
        public Vector3 Position { get; set; }
        public int Duration { get; set; }

        public WaypointTypes Type { get; set; }

        public float VehicleSpeed { get; set; }
        public uint VehicleTargetModel { get; set; }
        public int DrivingStyle { get; set; }

        public string AnimDict { get; set; }
        public string AnimName { get; set; }
    }
}