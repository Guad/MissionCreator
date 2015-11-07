using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.UI
{
    public class MissionFailedScreen
    {
        public string Reason { get; set; }
        public bool Visible { get; set; }
        public bool HasPressedContinue { get; set; }

        public MissionFailedScreen(string reason)
        {
            Reason = reason;
            Visible = false;
        }

        public void Show()
        {
            Visible = true;
            HasPressedContinue = false;
            NativeFunction.CallByHash<uint>(0x2206BF9A37B7F724, "DeathFailOut", -1, 1);
        }

        public void Draw()
        {
            if (!Visible) return;

            SizeF res = UIMenu.GetScreenResolutionMantainRatio();
            int middle = Convert.ToInt32(res.Width / 2);

            new Sprite("mpentry", "mp_modenotselected_gradient", new Point(0, 30), new Size(Convert.ToInt32(res.Width), 300),
                0f, Color.FromArgb(230, 255, 255, 255)).Draw();

            new ResText("mission failed", new Point(middle, 100), 2.5f, Color.FromArgb(255, 148, 27, 46), Common.EFont.Pricedown, ResText.Alignment.Centered).Draw();

            new ResText(Reason, new Point(middle, 230), 0.5f, Color.White, Common.EFont.ChaletLondon, ResText.Alignment.Centered).Draw();

            var scaleform = new Scaleform(0);
            scaleform.Load("instructional_buttons");
            scaleform.CallFunction("CLEAR_ALL");
            scaleform.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
            scaleform.CallFunction("CREATE_CONTAINER");

            scaleform.CallFunction("SET_DATA_SLOT", 0, NativeFunction.CallByHash(0x0499D7B09FC9B407, typeof(string), 2, (int)GameControl.FrontendAccept, 0), "Continue");
            scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
            scaleform.Render2D();

            NativeFunction.CallByName<uint>("DISABLE_ALL_CONTROL_ACTIONS", 0);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int) GameControl.FrontendAccept);

            if (Game.IsControlJustPressed(0, GameControl.FrontendAccept))
            {
                HasPressedContinue = true;
                NativeFunction.CallByHash<uint>(0xB4EDDC19532BFB85);
            }
        }
    }
}