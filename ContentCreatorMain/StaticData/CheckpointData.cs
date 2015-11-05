using System;
using System.Collections.Generic;

namespace ContentCreator.StaticData
{
    public static class CheckpointData
    {
        public static Dictionary<string, Tuple<string, int, float>[]> Database = new Dictionary<string, Tuple<string, int, float>[]>
        { // 3rd is height offset
            {"Marker", new []
            {
               new Tuple<string, int, float>("Corona", 0, 1f),
               new Tuple<string, int, float>("Entrance", 1, 0f), 
            }},
        };
    }
}