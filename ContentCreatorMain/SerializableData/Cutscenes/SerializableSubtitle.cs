using System;

namespace ContentCreator.SerializableData.Cutscenes
{
    public class SerializableSubtitle
    {
        public string Content { get; set; }
        public TimeSpan PositionInTime { get; set; }
        public int DurationInMs { get; set; }
    }
}