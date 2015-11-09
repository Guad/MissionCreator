using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MissionCreator.SerializableData.Cutscenes;
using Rage;

namespace MissionCreator.StaticData
{
    public static class StaticLists
    {
        public static List<dynamic> AmmoChoses = new List<dynamic>
        {
            10,
            50,
            100,
            300,
            1000,
            9999,
        };

        public static List<dynamic> HealthArmorChoses = new List<dynamic>
        {
            0,
            50,
            100,
            200,
            500,
            2500,
        };

        public static List<dynamic> AccuracyList = new List<dynamic>
        {
            0,
            25,
            40,
            50,
            70,
            80,
            90,
            100,
        };

        public static List<dynamic> VehicleHealthChoses = new List<dynamic>
        {
            -100,
            100,
            500,
            1000,
            2500,
            10000,
        };

        public static List<dynamic> ObjectiveTypeList = new List<dynamic>
        {
            "Destroy Vehicle",
            "Enter Vehicle",
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

        public static List<dynamic> WeatherTypes = new List<dynamic>(Enum.GetNames(typeof(WeatherType)));

        public static List<dynamic> TimesList = new List<dynamic>
        {
            "Day",
            "Noon",
            "Night",
            "Sunrise",
        };

        public static Dictionary<string, int> TimeTranslation = new Dictionary<string, int>
        {
            {"Day", 16},
            {"Noon", 20},
            {"Night", 02},
            {"Sunrise", 07},
        };

        public static List<dynamic> InterpolationList = new List<dynamic>(((InterpolationStyle[])Enum.GetValues(typeof(InterpolationStyle))).Select(n => (dynamic)n).ToList());

        public static List<dynamic> ObjectiveIndexList = new List<dynamic>(Enumerable.Range(0, 300).Select(n => (dynamic) n));

        public static List<dynamic> WantedList = new List<dynamic>(Enumerable.Range(0, 6).Select(n => (dynamic)n));

        public static List<dynamic> KnownColors = new List<dynamic>();

        public static void Init()
        {
            NumberMenu.AddRange(Enumerable.Range(0, 300).Select(n => (dynamic) n));

            RemoveAfterList.AddRange(Enumerable.Range(1, 300).Select(n => (dynamic) n));
            

            var colors = (KnownColor[]) Enum.GetValues(typeof (KnownColor));
            for (var i = 28; i < colors.Length - 8; i++)
            {
                KnownColors.Add(colors[i]);
            }
        }
    }
}