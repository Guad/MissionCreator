using System;
using System.Collections.Generic;

namespace ContentCreator.StaticData
{
    public static class PickupData
    {
        public static Dictionary<string, Tuple<string, uint>[]> Database = new Dictionary<string, Tuple<string, uint>[]>
        {
            {"Pistols", new []
            {
               new Tuple<string, uint>("Pistol", 0x1B06D571),
               new Tuple<string, uint>("AP Pistol", 0x22D8FE39), 
            }},
            {"Rifles", new []
            {
                new Tuple<string, uint>("Carbine Rifle", 0x83BF0278), 
                new Tuple<string, uint>("Special Carbine", 0xC0A3098D),
            }},
        };

        /*public static Dictionary<string, Tuple<string, uint, uint>[]> Database = new Dictionary<string, Tuple<string, uint, uint>[]>
        {
            {"Pistols", new []
            {
               new Tuple<string, uint, uint>("Pistol", 0x1B06D571, Convert.ToUInt32(-105925489)),
               new Tuple<string, uint, uint>("AP Pistol ", 0x22D8FE39, Convert.ToUInt32(996550793)), 
            }},
            {"Rifles", new []
            {
                new Tuple<string, uint, uint>("Carbine Rifle", 0x83BF0278, Convert.ToUInt32(-546236071)), 
                new Tuple<string, uint, uint>("Special Carbine", 0xC0A3098D, Convert.ToUInt32(-214137936)),
            }},
        };*/
    }
}