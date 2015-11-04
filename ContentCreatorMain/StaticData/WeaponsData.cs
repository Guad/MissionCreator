using System;
using System.Collections.Generic;

namespace ContentCreator.StaticData
{
    public static class WeaponsData
    {
        public static Dictionary<string, Tuple<string, uint>[]> Database = new Dictionary<string, Tuple<string, uint>[]>
        {
            {"Pistols", new []
            {
               new Tuple<string, uint>("Pistol", 0x1B06D571),
               new Tuple<string, uint>("AP Pistol ", 0x22D8FE39), 
            }},
            {"Rifles", new []
            {
                new Tuple<string, uint>("Carbine Rifle", 0x83BF0278), 
                new Tuple<string, uint>("Special Carbine", 0xC0A3098D),
            }},
        };
    }
}