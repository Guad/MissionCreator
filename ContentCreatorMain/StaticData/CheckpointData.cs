using System;
using System.Collections.Generic;

namespace MissionCreator.StaticData
{
    public static class CheckpointData
    {
        public static Dictionary<string, Tuple<string, int, float>[]> Database = new Dictionary<string, Tuple<string, int, float>[]>
        { // 3rd is height offset
            {"Marker", new []
            {
               new Tuple<string, int, float>("Cylinder", 1, 0f), 
               new Tuple<string, int, float>("Cone", 0, 1f),
               new Tuple<string, int, float>("Chevron", 21, 1f),
               new Tuple<string, int, float>("Flag", 4, 1f),
               new Tuple<string, int, float>("Lap", 24, 1f),
               new Tuple<string, int, float>("Sphere", 28, 1f),
            }},
        };
    }
}