using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MissionCreator.SerializableData;
using MissionCreator.SerializableData.Cutscenes;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.CutsceneEditor
{
    public class CutsceneUi
    {
        public UIMenu CutsceneMenus;
        public EditCutsceneMenu EditCutsceneMenu;
        public TimelineMarkerMenu MarkerMenu;

        public bool IsInCutsceneEditor;
        public SerializableCutscene CurrentCutscene;
        public int CurrentTimestamp;
        public List<TimeMarker> Markers;

        
        private List<UIMenu> _children;
        public CutsceneUi()
        {
            #region NativeUI Init
            {
                CutsceneMenus = new UIMenu("Cutscene Creator", "CUTSCENES");
                CutsceneMenus.MouseControlsEnabled = false;
                CutsceneMenus.ResetKey(Common.MenuControls.Up);
                CutsceneMenus.ResetKey(Common.MenuControls.Down);
                CutsceneMenus.SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
                CutsceneMenus.SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);

                MarkerMenu = new TimelineMarkerMenu(this);
                
                EditCutsceneMenu = new EditCutsceneMenu(this);
                EditCutsceneMenu.ParentMenu = CutsceneMenus;
            }
            #endregion

            _children = new List<UIMenu>();
        }

        public void Enter()
        {
            RebuildCutsceneMenu();
        }

        public void EnterCutsceneEditor(SerializableCutscene cutscene)
        {
            Markers = new List<TimeMarker>();

            Editor.Editor.CameraClampMax = 30f;
            CurrentCutscene = cutscene;
            IsInCutsceneEditor = true;

            if(cutscene.Cameras != null)
            foreach (var camera in cutscene.Cameras)
            {
                Markers.Add(new CameraMarker()
                {
                    Time = camera.PositionInTime,
                    CameraPos = camera.Position,
                    CameraRot = camera.Rotation,
                    Interpolation = camera.InterpolationStyle,
                });
            }

            if(cutscene.Subtitles != null)
            foreach (var subtitle in cutscene.Subtitles)
            {
                Markers.Add(new SubtitleMarker()
                {
                    Content = subtitle.Content,
                    Duration = subtitle.DurationInMs,
                    Time = subtitle.PositionInTime,
                });
            }

            CurrentTimestamp = 0;
            MarkerMenu = new TimelineMarkerMenu(this);
            MarkerMenu.Visible = true;
            MarkerMenu.BuildFor(Markers.FirstOrDefault(m => m.Time == CurrentTimestamp));
            MarkerMenu.OnMenuClose += sender =>
            {
                LeaveCutsceneEditor();
            };
        }

        public void LeaveCutsceneEditor()
        {
            Editor.Editor.CameraClampMax = -30f;
            IsInCutsceneEditor = false;
            CurrentCutscene = null;
            RebuildCutsceneMenu();
            CutsceneMenus.Visible = true;
        }

        public void RebuildCutsceneMenu()
        {
            CutsceneMenus.Clear();
            _children.Clear();

            {
                var menu = new CreateCutsceneMenu(this);
                var item = new UIMenuItem("Create Cutscene");
                CutsceneMenus.AddItem(item);
                CutsceneMenus.BindMenuToItem(menu, item);
                _children.Add(menu);
            }

            foreach (var cutscene in Editor.Editor.CurrentMission.Cutscenes)
            {
                var item = new UIMenuItem(cutscene.Name);
                CutsceneMenus.AddItem(item);
                item.Activated += (sender, selectedItem) =>
                {
                    CutsceneMenus.Visible = false;
                    EditCutsceneMenu.Build(cutscene);
                    EditCutsceneMenu.Visible = true;
                };
            }

            CutsceneMenus.RefreshIndex();
        }


        private DateTime _lastPress = DateTime.Now;
        private uint counter = 0;
        public void Tick()
        {
            NativeFunction.CallByName<uint>("HIDE_HUD_AND_RADAR_THIS_FRAME");
            TimeBar();
            Game.DisplaySubtitle(TimeSpan.FromMilliseconds(CurrentTimestamp).ToString("g"), 200);

            if (Game.IsControlPressed(0, GameControl.CellphoneLeft) &&
                CurrentTimestamp > 0 &&
                DateTime.Now.Subtract(_lastPress).TotalMilliseconds > 50f &&
                MarkerMenu.CanMoveMarker)
            {
                CurrentTimestamp = (int) Math.Max(CurrentTimestamp - 200, 0);
                _lastPress = DateTime.Now;
            }
            else if (Game.IsControlPressed(0, GameControl.CellphoneRight) &&
                CurrentTimestamp < CurrentCutscene.Length &&
                DateTime.Now.Subtract(_lastPress).TotalMilliseconds > 50f &&
                MarkerMenu.CanMoveMarker)
            {
                CurrentTimestamp = (int)Math.Min(CurrentTimestamp + 200, CurrentCutscene.Length);
                _lastPress = DateTime.Now;
            }

            if (Game.IsControlJustPressed(0, GameControl.FrontendLb))
            {
                var prevNodes = Markers.Where(m => m.Time < CurrentTimestamp);
                if (prevNodes.Any())
                {
                    CurrentTimestamp = prevNodes.OrderBy(m => Math.Abs(m.Time - CurrentTimestamp)).First().Time;
                }
            }
            else if (Game.IsControlJustPressed(0, GameControl.FrontendRb))
            {
                var nextNodes = Markers.Where(m => m.Time > CurrentTimestamp);
                if (nextNodes.Any())
                {
                    CurrentTimestamp = nextNodes.OrderBy(m => Math.Abs(m.Time - CurrentTimestamp)).First().Time;
                }
            }

            if (((Game.IsControlJustReleased(0, GameControl.CellphoneLeft) ||
                Game.IsControlJustReleased(0, GameControl.CellphoneRight)) && MarkerMenu.CanMoveMarker) ||
                    Game.IsControlJustReleased(0, GameControl.FrontendRb) ||
                    Game.IsControlJustReleased(0, GameControl.FrontendLb))
            {
                _lastPress = DateTime.MinValue;
                MarkerMenu.BuildFor(Markers.FirstOrDefault(m => m.Time == CurrentTimestamp));
            }

            if (Game.IsControlJustReleased(0, GameControl.CellphoneLeft) ||
                Game.IsControlJustReleased(0, GameControl.CellphoneRight))
                SaveCurrentCutscene();

            counter++;
            if (counter%100 == 0)
            {
                SaveCurrentCutscene();
            }
            if (counter > 1000)
                counter = 0;

            DrawInstructionalButtonsScaleform();
        }

        private void SaveCurrentCutscene()
        {
            CurrentCutscene.Cameras = new List<SerializableCamera>();
            CurrentCutscene.Subtitles = new List<SerializableSubtitle>();
            foreach (var marker in Markers)
            {
                if (marker is CameraMarker)
                {
                    var cm = ((CameraMarker) marker);
                    CurrentCutscene.Cameras.Add(new SerializableCamera()
                    {
                        InterpolationStyle = cm.Interpolation,
                        Position = cm.CameraPos,
                        Rotation = cm.CameraRot,
                        PositionInTime = cm.Time,
                    });
                }

                else if (marker is SubtitleMarker)
                {
                    var cm = ((SubtitleMarker)marker);
                    CurrentCutscene.Subtitles.Add(new SerializableSubtitle()
                    {
                        Content = cm.Content,
                        DurationInMs = cm.Duration,
                        PositionInTime = cm.Time,
                    });
                }
            }
        }

        private void TimeBar()
        {
            var res = UIMenu.GetScreenResolutionMantainRatio();
            Color bg = Color.FromArgb(150, 0, 0, 0);
            Color beforeCurrent = Color.DeepSkyBlue;
            Color afterCurrent = Color.SteelBlue;

            var bgStart = new Point(60, ((int)res.Height) - 100);
            var bgSize = new Size(((int)res.Width) - 60*2, 20);

            var barStart = new Point(70, ((int)res.Height) - 95);

            int fullBar = ((int) res.Width) - 70*2;

            new ResRectangle(bgStart, bgSize, bg).Draw();
            new ResRectangle(barStart, new Size((int)((double)CurrentTimestamp/CurrentCutscene.Length*fullBar), 10), beforeCurrent).Draw();
            new ResRectangle(new Point(barStart.X + (int)((double)CurrentTimestamp / CurrentCutscene.Length*fullBar), barStart.Y), new Size(fullBar - (int)((float)CurrentTimestamp / CurrentCutscene.Length*fullBar), 10), afterCurrent).Draw();
            
            foreach (var marker in Markers)
            {
                var x = barStart.X + (int)((double)marker.Time / CurrentCutscene.Length * fullBar);
                new Sprite("golfputting", "puttingmarker", new Point(x - 16, barStart.Y - 32), new Size(32,32), 0f, marker is SubtitleMarker ? Color.Yellow : Color.White).Draw();
                new ResRectangle(new Point(x, barStart.Y), new Size(2, 10), bg).Draw();
            }
        }
        
        private void RuleOfThirds()
        {
            var res = UIMenu.GetScreenResolutionMantainRatio();
            const int numOfLines = 3;
            var widthInterval = (int)(res.Width / numOfLines);
            var heightInterval = (int)(res.Height / numOfLines);
            for (int i = 1; i <= numOfLines; i++)
            {
                new ResRectangle(new Point(widthInterval*i, 0), new Size(2, (int)res.Height), Color.White).Draw();
                new ResRectangle(new Point(0, heightInterval*i), new Size((int)res.Width, 2), Color.White).Draw();
            }
        }

        private void DrawInstructionalButtonsScaleform()
        {
            var instructButts = new Scaleform();
            instructButts.Load("instructional_buttons");
            instructButts.CallFunction("CLEAR_ALL");
            instructButts.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
            instructButts.CallFunction("CREATE_CONTAINER");
            instructButts.CallFunction("SET_DATA_SLOT", 0, Util.GetControlButtonId(GameControl.CellphoneRight), "");
            instructButts.CallFunction("SET_DATA_SLOT", 1, Util.GetControlButtonId(GameControl.CellphoneLeft), "Navigate");
            
            instructButts.CallFunction("SET_DATA_SLOT", 2, Util.GetControlButtonId(GameControl.FrontendRb), "");
            instructButts.CallFunction("SET_DATA_SLOT", 3, Util.GetControlButtonId(GameControl.FrontendLb), "Jump to Markers");


            instructButts.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
        }

        public void Process()
        {
            if (IsInCutsceneEditor)
            {
                RuleOfThirds();
            }

            CutsceneMenus.ProcessControl();
            CutsceneMenus.Draw();

            MarkerMenu?.ProcessControl();
            MarkerMenu?.Draw();

            EditCutsceneMenu?.ProcessControl();
            EditCutsceneMenu?.Draw();

            _children.ForEach(m =>
            {
                m.ProcessControl();
                m.Draw();
            });
        }
    }
}