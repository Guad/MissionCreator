using System;
using System.Collections.Generic;

namespace MissionCreator.StaticData
{
    public static class WeaponsData
    {
        public static Dictionary<string, Tuple<string, uint>[]> Database = new Dictionary<string, Tuple<string, uint>[]>
        {
            {"Melee", new []
            {
                new Tuple<string, uint>("Unarmed", 0xA2719263),
                new Tuple<string, uint>("Knife", 0x99B507EA),
                new Tuple<string, uint>("Nightstick", 0x678B81B1),
                new Tuple<string, uint>("Hammer", 0x4E875F73),
                new Tuple<string, uint>("Bat", 0x958A4A8F),
                new Tuple<string, uint>("Golf Club", 0x440E4788),
                new Tuple<string, uint>("Flashlight", 0x8BB05FD7),
            } },
            {"Pistols", new []
            {
                new Tuple<string, uint>("Pistol", 0x1B06D571),
                new Tuple<string, uint>("AP Pistol", 0x22D8FE39), 
                new Tuple<string, uint>("Pistol .50", 0x99AEEB3B),
                new Tuple<string, uint>("Combat Pistol", 0x5EF9FEC4), 
                new Tuple<string, uint>("Heavy Pistol", 0xD205520E), 
                new Tuple<string, uint>("Marksman Pistol", 0xDC4DB296), 
                new Tuple<string, uint>("SNS Pistol", 0xBFD21232),
                new Tuple<string, uint>("Vintage Pistol", 0x83839C4),
            } },
            { "Submachine", new []
            {
                new Tuple<string, uint>("SMG", 0x2BE6766B), 
                new Tuple<string, uint>("Assault SMG", 0xEFE7E2DF), 
                new Tuple<string, uint>("Micro SMG", 0x13532244), 
                new Tuple<string, uint>("Combat PDW", 0x0A3D4D34), 
                new Tuple<string, uint>("Machine Gun", 0x9D07F764), 
                new Tuple<string, uint>("Combat Machine Gun", 0x7FD62962), 
                new Tuple<string, uint>("Gusenberg", 0x61012683), 
            }},
            { "Assault Rifles", new []
            {
                new Tuple<string, uint>("Assault Rifle", 0xBFEFFF6D), 
                new Tuple<string, uint>("Advanced Rifle", 0xAF113F99), 
                new Tuple<string, uint>("Bullpup Rifle", 0x7F229F94), 
                new Tuple<string, uint>("Carbine Rifle", 0x83BF0278), 
                new Tuple<string, uint>("Special Carbine", 0xC0A3098D), 
            }},
            {"Sniper Rifles", new []
            {
                new Tuple<string, uint>("Sniper Rifle", 0x5FC3C11), 
                new Tuple<string, uint>("Heavy Sniper Rifle", 0xC472FE2), 
                new Tuple<string, uint>("Marksman Rifle", 0xC734385A), 
            } },
            { "Shotguns", new []
            {
                new Tuple<string, uint>("Assault Shotgun", 0xE284C527), 
                new Tuple<string, uint>("Bullpup Shotgun", 0x9D61E50F), 
                new Tuple<string, uint>("Heavy Shotgun", 0x3AABBBAA), 
                new Tuple<string, uint>("Pump Shotgun", 0x1D073A89), 
                new Tuple<string, uint>("Sawn Off Shotgun", 0x7846A318), 
                new Tuple<string, uint>("Musket", 0xA89CB99E),
            }},
            {"Heavy", new []
            {
                new Tuple<string, uint>("Grenade Launcher", 0xA284510B), 
                new Tuple<string, uint>("Homing Launcher", 0x63AB0442), 
                new Tuple<string, uint>("RPG" ,0xB1CA77B1), 
                new Tuple<string, uint>("Railgun", 0x6D544C99), 
                new Tuple<string, uint>("Minigun", 0x42BF8A85), 
            }},
            {"Explosives", new []
            {
                new Tuple<string, uint>("Grenade", 0x93E220BD), 
                new Tuple<string, uint>("BZ Gas", 0xA0973D5E), 
                new Tuple<string, uint>("Smoke Grenade", 0xFDBC8A50), 
                new Tuple<string, uint>("Molotov", 0x24B17070), 
                new Tuple<string, uint>("Sticky Bomb", 0x2C3731D9), 
                new Tuple<string, uint>("Proximity Mine", 0xAB564B93), 
            } }
        };
    }
}