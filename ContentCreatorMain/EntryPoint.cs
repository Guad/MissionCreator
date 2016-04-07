using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rage;
using Rage.Native;
using RAGENativeUI.Elements;


[assembly: Rage.Attributes.Plugin("Mission Creator", Description = "Create your own mission!", Author = "Guadmaz")]

namespace MissionCreator
{
    public static class EntryPoint
    {
        public static Editor.Editor MainEditor;
        public static MissionPlayer MissionPlayer;

        public static void Main()
        {
            StaticData.StaticLists.Init();
            StaticData.RelationshipGroups.Init();

            MainEditor = new Editor.Editor();
            MissionPlayer = new MissionPlayer();

            Game.FrameRender += FrameRender;

            for(;;)
            {
                Process();
                GameFiber.Yield();
            }
        }

        //private static Dictionary<string, List<Tuple<string, uint>>> _dictionary = new Dictionary<string, List<Tuple<string, uint>>>();

        private static string currentCat;

        private static void SaveDict()
        {
            /* // R* Mission Creator ripping
            string output = "{\r\n";

            foreach (var pair in _dictionary)
            {
                output += "{\"" + pair.Key + "\", new[]\r\n";
                output += "{\r\n";
                foreach (var tuple in pair.Value)
                {
                    output += "new Tuple<string, uint>(\"" + tuple.Item1 + "\", " + tuple.Item2 + "),\r\n";
                }
                output += "}},\r\n";
            }
            output += "};";

            File.WriteAllText("pedHashes.txt", output);*/
        }

        public static void Process()
        {
            /*
            if (Util.IsDisabledControlJustPressed(GameControl.Aim))
            {
                GameFiber.StartNew(SaveDict);
            }

            if (Util.IsDisabledControlJustPressed(GameControl.Duck))
            {
                GameFiber.StartNew(delegate
                {
                    currentCat = Util.GetUserInput();
                });
            }
            if(string.IsNullOrEmpty(currentCat))  return;
            if (!_dictionary.ContainsKey(currentCat))
            {
                _dictionary.Add(currentCat, new List<Tuple<string, uint>>());
                return;
            }

            var cam = Camera.RenderingCamera;
            if (cam == null)
            {
                Game.DisplaySubtitle("Cam is null.", 1000);
                return;
            }
            var peds = World.GetAllPeds().OrderBy(x => (x.Position - cam.Position).Length());
            var ped = peds.ToArray()[1];

            Util.DrawMarker(0, ped.Position + new Vector3(0,0,1), new Vector3(), new Vector3(1,1,1), Color.Yellow);

            if (!_dictionary[currentCat].Any(tup => tup.Item2 == ped.Model.Hash))
            {
                _dictionary[currentCat].Add(new Tuple<string, uint>(ped.Model.Name, ped.Model.Hash));
            }*/
        }

        private static GameFiber lel;
        private static bool debug;
        public static void FrameRender(object sender, GraphicsEventArgs e)
        {
            MissionPlayer.Tick();
            MainEditor.Tick(e);
            if (Game.IsKeyDown(Keys.F8))
            {
                if(!MainEditor.IsInEditor && !MainEditor.IsInMainMenu)
                    MainEditor.EnterEditor();
            }
        }
    }
}
