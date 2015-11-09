using MissionCreator.SerializableData.Cutscenes;
using Rage;

namespace MissionCreator.CutsceneEditor
{
    public class CameraMarker : TimeMarker
    {
        public Vector3 CameraPos { get; set; }
        public Rotator CameraRot { get; set; }
        public InterpolationStyle Interpolation { get; set; }
    }

    public class SubtitleMarker : TimeMarker
    {
        public string Content { get; set; }
        public int Duration { get; set; }
    }

    public abstract class TimeMarker
    {
        public int Time { get; set; }
    }
}