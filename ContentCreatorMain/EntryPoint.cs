using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rage;
using Rage.Native;
using RAGENativeUI.Elements;


[assembly: Rage.Attributes.Plugin("Content Creator", Description = "DYOM Port.", Author = "Guadmaz")]

namespace ContentCreator
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

        public static void Process()
        {
            
        }

        public static void FrameRender(object sender, GraphicsEventArgs e)
        {
            MissionPlayer.Tick();
            MainEditor.Tick(e);
            if (Game.IsKeyDown(Keys.F8))
            {
                if(!MainEditor.IsInEditor && !MainEditor.IsInMainMenu)
                    MainEditor.EnterEditor();
                else
                    MainEditor.LeaveEditor();
            }
        }
    }
}
