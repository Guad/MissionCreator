using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;


[assembly: Rage.Attributes.Plugin("Content Creator", Description = "DYOM Port.", Author = "Guadmaz")]

namespace ContentCreator
{
    public static class EntryPoint
    {
        public static Editor.Editor MainEditor;

        public static void Main()
        {
            StaticData.StaticLists.Init();

            MainEditor = new Editor.Editor();

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
            MainEditor.Tick(e);
            if (Game.IsControlJustPressed(0, GameControl.Context)) // FOR DEBUGGING PURPOSE ONLY
            {
                if(!MainEditor.IsInEditor && !MainEditor.IsInMainMenu)
                    MainEditor.EnterEditor();
                else
                    MainEditor.LeaveEditor();
            }
        }
    }
}
