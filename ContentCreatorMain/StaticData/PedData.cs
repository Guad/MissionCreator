using System;
using System.Collections.Generic;

namespace ContentCreator.StaticData
{
    public static class PedData
    {
        public static Dictionary<string, Tuple<string, uint>[]> Database = new Dictionary<string, Tuple<string, uint>[]>
        {
            {"Gangsters", new []
            {
               new Tuple<string, uint>("Chinese Goon", 0x106D9A99),
               new Tuple<string, uint>("Chinese Boss", 0xB9DD0300), 
            }},
            {"Animals", new []
            {
                new Tuple<string, uint>("Shepherd", 0x431FC24C), 
                new Tuple<string, uint>("Deer", 0xD86B5A95),
            }},
        };
    }
}