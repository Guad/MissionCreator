using System;
using System.Collections.Generic;
using System.IO;

namespace MissionCreator.StaticData
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

    public class CTuple<T1, T2>
    {
        public CTuple()
        {
            
        }

        public CTuple(T1 first, T2 sec)
        {
            Item1 = first;
            Item2 = sec;
        }

        public T1 Item1;
        public T2 Item2;
    }

    public class ObjectInfo
    {
        Dictionary<string, CTuple<string, uint>[]> MainInfo { get; set; }
    }
}