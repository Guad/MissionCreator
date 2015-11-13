using System;
using System.Collections.Generic;

namespace MissionCreator.StaticData
{
    public static class PedData
    {
        public static Dictionary<string, Tuple<string, uint>[]> Database = new Dictionary<string, Tuple<string, uint>[]>
        {
            {"Cops and Army", new[]
            {
            new Tuple<string, uint>("Male Cop", 1581098148),
            new Tuple<string, uint>("Female Cop", 368603149),
            new Tuple<string, uint>("Male Ranger", 0xEF7135AE),
            new Tuple<string, uint>("Female Ranger", 2680682039),
            new Tuple<string, uint>("NOOSE", 2374966032),
            new Tuple<string, uint>("IAA", 1650288984),
            new Tuple<string, uint>("FIB", 2072724299),
            new Tuple<string, uint>("Marine", 1925237458),
            new Tuple<string, uint>("Black Ops 1", 3019107892),
            new Tuple<string, uint>("Black Ops 2", 2047212121),
            new Tuple<string, uint>("Black Ops 2 (NV)", 1349953339),
            new Tuple<string, uint>("Black Ops 3", 788443093),
            new Tuple<string, uint>("Carrier Crew", 3387290987),
            new Tuple<string, uint>("Armored", 2512875213),
            new Tuple<string, uint>("Pilot 1", 2872052743),
            new Tuple<string, uint>("Pilot 2", 4131252449),
            new Tuple<string, uint>("Prison Guard", 1456041926),
            new Tuple<string, uint>("Male VIP", 3382649284),
            new Tuple<string, uint>("Female VIP", 826475330),
            }},
            {"Gang Members", new[]
            {
            new Tuple<string, uint>("Male Lost", 1330042375),
            new Tuple<string, uint>("Female Lost", 4250220510),
            new Tuple<string, uint>("Male Vagos", 832784782),
            new Tuple<string, uint>("Female Vagos", 1520708641),
            new Tuple<string, uint>("Vagos Boss", 1466037421),
            new Tuple<string, uint>("Family", 3896218551),
            new Tuple<string, uint>("Balla", 588969535),
            new Tuple<string, uint>("Chinese", 4285659174),
            new Tuple<string, uint>("Chinese Boss", 3118269184),
            new Tuple<string, uint>("Azteca", 1752208920),
            new Tuple<string, uint>("Korean", 611648169),
            new Tuple<string, uint>("Korean Boss", 891945583),
            new Tuple<string, uint>("Armenian", 4255728232),
            new Tuple<string, uint>("Armenian Boss", 4058522530),
            new Tuple<string, uint>("Street Punk", 4246489531),
            new Tuple<string, uint>("Salva", 2422005962),
            new Tuple<string, uint>("Salva Boss", 1822283721),
            new Tuple<string, uint>("Balla East", 4096714883),
            new Tuple<string, uint>("Balla West", 599294057),
            }},
            {"Female Pedestrians", new[]
            {
            new Tuple<string, uint>("Hippy", 2549481101),
            new Tuple<string, uint>("Business", 3083210802),
            new Tuple<string, uint>("Beach", 3349113128),
            new Tuple<string, uint>("Hippy", 343259175),
            new Tuple<string, uint>("Bodybuilder", 1004114196),
            new Tuple<string, uint>("Fitness", 3343476521),
            new Tuple<string, uint>("Fat", 951767867),
            new Tuple<string, uint>("Tramp", 1224306523),
            }},
            {"Female Special", new[]
            {
            new Tuple<string, uint>("Hooker", 348382215),
            new Tuple<string, uint>("Epsilonist", 1755064960),
            new Tuple<string, uint>("Maid", 3767780806),
            }},
            {"Male Pedestrians", new[]
            {
            new Tuple<string, uint>("Hipster", 587703123),
            new Tuple<string, uint>("Transvestite", 4144940484),
            new Tuple<string, uint>("Business", 2705543429),
            new Tuple<string, uint>("Business Casual", 2597531625),
            new Tuple<string, uint>("Beach", 3886638041),
            new Tuple<string, uint>("Bodybuilder", 1264920838),
            new Tuple<string, uint>("Fitness", 2218630415),
            new Tuple<string, uint>("Fat", 330231874),
            new Tuple<string, uint>("Tramp", 390939205),
            new Tuple<string, uint>("Hippy", 2097407511),
            new Tuple<string, uint>("Hillbilly 1", 1822107721),
            new Tuple<string, uint>("Hillbilly 2", 2064532783),
            }},
            {"Male Special", new[]
            {
            new Tuple<string, uint>("Construction Worker", 3621428889),
            new Tuple<string, uint>("Postal Service", 1936142927),
            new Tuple<string, uint>("Scientist", 1092080539),
            new Tuple<string, uint>("Paramedic", 3008586398),
            new Tuple<string, uint>("Seashark", 767028979),
            new Tuple<string, uint>("MotoX", 1694362237),
            new Tuple<string, uint>("Security Guard", 3613962792),
            new Tuple<string, uint>("Bodyguard", 4049719826),
            new Tuple<string, uint>("Prisoner", 2981862233),
            new Tuple<string, uint>("Robber", 3227390873),
            new Tuple<string, uint>("Epsilonist", 2010389054),
            new Tuple<string, uint>("Altruist", 1268862154),
            new Tuple<string, uint>("Clown", 71929310),
            new Tuple<string, uint>("Mariachi", 2124742566),
            new Tuple<string, uint>("Paparazzi", 3972697109),
            }},
            {"Animals", new [] {
            new Tuple<string,uint>("Shepherd", 0x431FC24C), 
            new Tuple<string, uint>("Deer", 0xD86B5A95),
            new Tuple<string, uint>("Boar", 0xCE5FF074),
            new Tuple<string, uint>("Cat", 0x573201B8),
            new Tuple<string, uint>("Chimp", 0xA8683715),
            new Tuple<string, uint>("Chop", 0x14EC17EA),
            new Tuple<string, uint>("Cormorant", 0x56E29962),
            new Tuple<string, uint>("Cow", 0xFCFA9E1E),
            new Tuple<string, uint>("Hammer Shark", 0x3C831724),
            new Tuple<string, uint>("Humpback", 0x471BE4B2),
            new Tuple<string, uint>("Retriever", 0x349F33E1),
            }},
            {"Main Characters", new []
            {
            new Tuple<string, uint>("Michael", 0xD7114C9),
            new Tuple<string, uint>("Franklin", 0x9B22DBAF),
            new Tuple<string, uint>("Trevor", 0x9B810FA2),
            }},
        };
    }
}