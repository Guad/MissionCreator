using System;
using Rage;

namespace MissionCreator.SerializableData.Cutscenes
{
    public enum InterpolationStyle
    {
        None,
        Linear,
        Smooth,
    }

    public class SerializableCamera
    {
        public Vector3 Position { get; set; }
        public Rotator Rotation { get; set; }

        public InterpolationStyle InterpolationStyle { get; set; }
        public int PositionInTime { get; set; }
    }
}