using System;
using System.Collections.Generic;

namespace MissionCreator.StaticData
{
    public static class PickupData
    {
        public static Dictionary<string, Tuple<string, int>[]> Database = new Dictionary<string, Tuple<string, int>[]>
        {
            {"Melee", new []
            {
               new Tuple<string, int>("Knife", 663586612),
               new Tuple<string, int>("Bat", -2115084258),
               new Tuple<string, int>("Hammer", 693539241),
               new Tuple<string, int>("Crowbar", -2027042680), 
               new Tuple<string, int>("Golf Club", -1997886297), 
               new Tuple<string, int>("Night Stick", 1587637620), 
               new Tuple<string, int>("Fire Extinguisher", -887893374), 
            }},
            {"Pistols", new []
            {
               new Tuple<string, int>("Pistol", -105925489),
               new Tuple<string, int>("Combat Pistol", -1989692173),
               new Tuple<string, int>("AP Pistol", 996550793), 
            }},
            {"Sub-Machine Guns", new []
            {
               new Tuple<string, int>("Micro SMG", 496339155),
               new Tuple<string, int>("SMG", 978070226),
               new Tuple<string, int>("Machine Gun", -2050315855), 
               new Tuple<string, int>("Combat Machine Gun", -1298986476), 

            }},
            {"Rifles", new []
            {
                new Tuple<string, int>("Assault Rifle", -214137936), 
                new Tuple<string, int>("Carbine Rifle", -546236071), 
                new Tuple<string, int>("Special Carbine", -1296747938),
            }},
            {"Shotguns", new []
            {
                new Tuple<string, int>("Sawn-Off Shotgun", -1766583645),
                new Tuple<string, int>("Pump Shotgun", -1456120371),
                new Tuple<string, int>("Assault Shotgun", -1835415205),
            }},
            {"Sniper Rifles", new []
            {
                new Tuple<string, int>("Sniper Rifle", -30788308),
                new Tuple<string, int>("Heavy Sniper Rifle", 1765114797),
                new Tuple<string, int>("Assault Shotgun", -1835415205),
            }},
            {"Heavy", new []
            {
               new Tuple<string, int>("Grenade Launcher", 779501861),
               new Tuple<string, int>("RPG", 1295434569),
               new Tuple<string, int>("Minigun", 792114228),
            }},
            {"Throwable", new []
            {
               new Tuple<string, int>("Grenade", 1577485217),
               new Tuple<string, int>("Sticky Bomb", 2081529176),
               new Tuple<string, int>("Molotov", 792114228),
               new Tuple<string, int>("Petrol Can", -962731009), 
               new Tuple<string, int>("Smoke Grenade", 483787975), 
            }},
            {"Misc", new []
            {
                new Tuple<string, int>("Armor", 1274757841),
                new Tuple<string, int>("Health", -1888453608),
                new Tuple<string, int>("Parachute", 1735599485), 
                new Tuple<string, int>("Money Bag", 341217064),
                new Tuple<string, int>("Bullet Ammo", 1426343849),
                new Tuple<string, int>("Missle Ammo", -107080240),
                new Tuple<string, int>("Case", -562499202), 
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