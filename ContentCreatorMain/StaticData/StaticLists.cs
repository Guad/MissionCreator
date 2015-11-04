using System.Collections.Generic;
using System.Linq;

namespace ContentCreator.StaticData
{
    public static class StaticLists
    {
        public static List<dynamic> AmmoChoses = new List<dynamic>
        {
             10, 50, 100, 300, 1000, 9999,
        };

        public static List<dynamic> HealthArmorChoses = new List<dynamic>
        {
             0, 50, 100, 200, 500, 2500,
        };

        public static List<dynamic> VehicleHealthChoses = new List<dynamic>
        {
             -100, 100, 500, 1000, 2500, 10000,
        };

        public static List<dynamic> RelationshipGroups = new List<dynamic>
        {
            "Follow Player",
            "Respect",
            "Like",
            "Neutral",
            "Dislike",
            "Hate",
            "Group 1",
            "Group 2",
        };

        public static List<dynamic> Behaviour = new List<dynamic>
        {
            "Default",
            "Stand Still",
            "Defend",
            "Attack",
            "Follow Waypoints",
        };

        public static List<dynamic> NumberMenu = new List<dynamic>();


        public static List<dynamic> RemoveAfterList = new List<dynamic>
        {
            "Never",
        };

        public static void Init()
        {
            NumberMenu.AddRange(Enumerable.Range(0, 300).Select(n => (dynamic)n));

            RemoveAfterList.AddRange(Enumerable.Range(1, 300).Select(n => (dynamic)n));

        }
    }
}