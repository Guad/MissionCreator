using System;
using System.Drawing;
using Rage;

namespace ContentCreator.Editor
{
    public enum RingType
    {
        HorizontalCircleFat = 23,
        HorizontalCircleSkinny = 25,
        HorizontalCircleSkinnyArrow = 26,
        HorizontalSplitArrowCircle = 27,
    }
    
    public class RingData
    {
        public bool Display { get; set; }
        public RingType Type { get; set; }
        public Color Color { get; set; } 
        public float Radius { get; set; }
        public float Heading { get; set; }
        public float HeightOffset { get; set; }
    }

    public class MarkerData
    {
        private string _markerType;
        public event EventHandler OnMarkerTypeChange;

        public bool Display { get; set; }
        public float HeadingOffset { get; set; }
        public float HeightOffset { get; set; }
        public Entity RepresentedBy { get; set; }
        public float RepresentationHeightOffset { get; set; }
        public string MarkerType
        {
            get { return _markerType; }
            set
            {
                _markerType = value; 
                OnMarkerTypeChange?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
}