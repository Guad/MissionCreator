using System;
using Rage;

namespace ContentCreator.SerializableData.Cutscenes
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
        public TimeSpan PositionInTime { get; set; }
    }
}