using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.UI
{
    public class MissionPassedScreen
    {
        public string Title { get; set; }



        private List<Tuple<string, string, TickboxState>> _items = new List<Tuple<string, string, TickboxState>>();
        private int _completionRate;
        private Medal _medal;

        public bool HasPressedContinue { get; set; }
        public bool Visible { get; set; }

        public MissionPassedScreen(string title, int completionRate, Medal medal)
        {
            Title = title;
            _completionRate = completionRate;
            _medal = medal;

            Visible = false;
        }

        public void AddItem(string label, string status, TickboxState state)
        {
            _items.Add(new Tuple<string, string, TickboxState>(label, status, state));
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

            new Sprite("mpentry", "mp_modenotselected_gradient", new Point(0, 10), new Size(Convert.ToInt32(res.Width), 450 + (_items.Count * 40)),
                0f, Color.FromArgb(200, 255, 255, 255)).Draw();

            new ResText("mission passed", new Point(middle, 100), 2.5f, Color.FromArgb(255, 199, 168, 87), Common.EFont.Pricedown, ResText.Alignment.Centered).Draw();

            new ResText(Title, new Point(middle, 230), 0.5f, Color.White, Common.EFont.ChaletLondon, ResText.Alignment.Centered).Draw();

            new ResRectangle(new Point(middle - 300, 290), new Size(600, 2), Color.White).Draw();

            for (int i = 0; i < _items.Count; i++)
            {
                new ResText(_items[i].Item1, new Point(middle - 230, 300 + (40 * i)), 0.35f, Color.White, Common.EFont.ChaletLondon, ResText.Alignment.Left).Draw();
                new ResText(_items[i].Item2, new Point(_items[i].Item3 == TickboxState.None ? middle + 265 : middle + 230, 300 + (40 * i)), 0.35f, Color.White, Common.EFont.ChaletLondon, ResText.Alignment.Right).Draw();
                if (_items[i].Item3 == TickboxState.None) continue;
                string spriteName = "shop_box_blank";
                switch (_items[i].Item3)
                {
                    case TickboxState.Tick:
                        spriteName = "shop_box_tick";
                        break;
                    case TickboxState.Cross:
                        spriteName = "shop_box_cross";
                        break;
                }
                new Sprite("commonmenu", spriteName, new Point(middle + 230, 290 + (40 * i)), new Size(48, 48)).Draw();
            }
            new ResRectangle(new Point(middle - 300, 300 + (40 * _items.Count)), new Size(600, 2), Color.White).Draw();

            new ResText("Completion", new Point(middle - 150, 320 + (40 * _items.Count)), 0.4f).Draw();
            new ResText(_completionRate + "%", new Point(middle + 150, 320 + (40 * _items.Count)), 0.4f, Color.White, Common.EFont.ChaletLondon, ResText.Alignment.Right).Draw();

            string medalSprite = "bronzemedal";
            switch (_medal)
            {
                case Medal.Silver:
                    medalSprite = "silvermedal";
                    break;
                case Medal.Gold:
                    medalSprite = "goldmedal";
                    break;
            }

            new Sprite("mpmissionend", medalSprite, new Point(middle + 150, 320 + (40 * _items.Count)), new Size(32, 32)).Draw();

            var scaleform = new Scaleform(0);
            scaleform.Load("instructional_buttons");
            scaleform.CallFunction("CLEAR_ALL");
            scaleform.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
            scaleform.CallFunction("CREATE_CONTAINER");

            scaleform.CallFunction("SET_DATA_SLOT", 0, NativeFunction.CallByHash(0x0499D7B09FC9B407,typeof(string), 2, (int)GameControl.FrontendAccept, 0), "Continue");
            scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
            scaleform.Render2D();

            NativeFunction.CallByName<uint>("DISABLE_ALL_CONTROL_ACTIONS", 0);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.FrontendAccept);

            if (Game.IsControlJustPressed(0, GameControl.FrontendAccept))
            {
                HasPressedContinue = true;
                NativeFunction.CallByHash<uint>(0xB4EDDC19532BFB85);
            }
        }

        public enum Medal
        {
            Bronze,
            Silver,
            Gold
        }

        public enum TickboxState
        {
            None,
            Empty,
            Tick,
            Cross,
        }
    }
}