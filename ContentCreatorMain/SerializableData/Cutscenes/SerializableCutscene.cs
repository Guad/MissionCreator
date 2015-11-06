using System;
using System.Collections.Generic;

namespace ContentCreator.SerializableData.Cutscenes
{
    public class SerializableCutscene
    {
        public int PlayAt { get; set; }
        public string Name { get; set; }
        public TimeSpan Length { get; set; }
        public List<SerializableCamera> Cameras { get; set; }
        public List<SerializableSubtitle> Subtitles { get; set; }
    }
}