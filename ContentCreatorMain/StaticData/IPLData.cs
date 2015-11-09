﻿using System;
using System.Collections.Generic;

namespace MissionCreator.StaticData
{
    public static class IPLData
    {
        public static Dictionary<string, Tuple<bool, string[], string[]>> Database = new Dictionary<string, Tuple<bool, string[], string[]>>
        {
            { "North Yankton", new Tuple<bool, string[], string[]>(false, new []
            {
                "plg_01",
                "prologue01",
                "prologue01_lod",
                "prologue01c",
                "prologue01c_lod",
                "prologue01d",
                "prologue01d_lod",
                "prologue01e",
                "prologue01e_lod",
                "prologue01f",
                "prologue01f_lod",
                "prologue01g",
                "prologue01h",
                "prologue01h_lod",
                "prologue01i",
                "prologue01i_lod",
                "prologue01j",
                "prologue01j_lod",
                "prologue01k",
                "prologue01k_lod",
                "prologue01z",
                "prologue01z_lod",
                "plg_02",
                "prologue02",
                "prologue02_lod",
                "plg_03",
                "prologue03",
                "prologue03_lod",
                "prologue03b",
                "prologue03b_lod",
                "prologue03_grv_dug",
                "prologue03_grv_dug_lod",
                "prologue_grv_torch",
                "plg_04",
                "prologue04",
                "prologue04_lod",
                "prologue04b",
                "prologue04b_lod",
                "prologue04_cover",
                "des_protree_end",
                "des_protree_start",
                "des_protree_start_lod",
                "plg_05",
                "prologue05",
                "prologue05_lod",
                "prologue05b",
                "prologue05b_lod",
                "plg_06",
                "prologue06",
                "prologue06_lod",
                "prologue06b",
                "prologue06b_lod",
                "prologue06_int",
                "prologue06_int_lod",
                "prologue06_pannel",
                "prologue06_pannel_lod",
                "prologue_m2_door",
                "prologue_m2_door_lod",
                "plg_occl_00",
                "prologue_occl",
                "plg_rd",
                "prologuerd",
                "prologuerdb",
                "prologuerd_lod",
            }, new string[0])},
            {"Yacht", new Tuple<bool, string[], string[]>(true, new []
            {
                    "hei_yacht_heist",
                    "hei_yacht_heist_Bar",
                    "hei_yacht_heist_Bedrm",
                    "hei_yacht_heist_Bridge",
                    "hei_yacht_heist_DistantLights",
                    "hei_yacht_heist_enginrm",
                    "hei_yacht_heist_LODLights",
                    "hei_yacht_heist_Lounge",
            }, new string[0]) },
            {"Destroyed Hospital", new Tuple<bool, string[], string[]>(false, new []
            {
                "RC12B_Destroyed",
                "RC12B_HospitalInterior",
            }, new string[0]) },
            {"Jewelry Store", new Tuple<bool, string[], string[]>(false, new []
            {
                "post_hiest_unload",
            }, new []
            {
                "jewel2fake",
                "bh1_16_refurb",
            }) },
            {"Morgue", new Tuple<bool, string[], string[]>(false, new []
            {
                "Coroner_Int_on",
            }, new string[0]) },
            {"Cargo Ship", new Tuple<bool, string[], string[]>(false, new []
            {
                "cargoship"
            }, new []
            {
                "sunkcargoship"
            }) },
            {"Heist Carrier", new Tuple<bool, string[], string[]>(true, new []
            {
                "hei_carrier",
                "hei_carrier_DistantLights",
                "hei_Carrier_int1",
                "hei_Carrier_int2",
                "hei_Carrier_int3",
                "hei_Carrier_int4",
                "hei_Carrier_int5",
                "hei_Carrier_int6",
                "hei_carrier_LODLights",
            }, new string[0]) },
            {"O'Neil Ranch", new Tuple<bool, string[], string[]>(false, new []
            {
                "farm",
                "farm_props",
                "farmint",
            }, new []
            {
                "farm_burnt",
                "farm_burnt_props",
                "farmint_cap",
            }) },
            {"Life Invader Lobby", new Tuple<bool, string[], string[]>(false, new []
            {
                "facelobby",
            }, new []
            {
                "facelobbyfake",
            }) },
            {"FIB Lobby", new Tuple<bool, string[], string[]>(false, new []
            {
                "FIBlobby",
            }, new []
            {
                "FIBlobbyfake",
            }) },
        };
    }
}