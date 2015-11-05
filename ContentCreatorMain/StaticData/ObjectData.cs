using System;
using System.Collections.Generic;

namespace ContentCreator.StaticData
{
    public static class ObjectData
    {
        public static Dictionary<string, Tuple<string, uint>[]> Database = new Dictionary<string, Tuple<string, uint>[]>
        {
            {"Ramps", new []
            {
               new Tuple<string, uint>("Ramp 1", 0xB157C9E4),
               new Tuple<string, uint>("Ramp 2", 0xF4F1511E), 
            }},
            {"Barrels", new []
            {
                new Tuple<string, uint>("Radioactive Waste", 0x478A8882), 
                new Tuple<string, uint>("Barrel", 0xAFDD8CBB),
            }},
        };
    }
}