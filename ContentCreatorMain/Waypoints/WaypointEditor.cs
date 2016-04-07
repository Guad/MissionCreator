using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MissionCreator.Editor;
using MissionCreator.Editor.NestedMenus;
using MissionCreator.SerializableData.Waypoints;
using MissionCreator.SerializableData;
using MissionCreator.SerializableData.Objectives;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.Waypoints
{
    public class WaypointEditor
    {

        #region Constructors
        public WaypointEditor(SerializablePed actor) : this()
        {
            _mainPed = actor;
            _mainActorObjective = null;
        }

        public WaypointEditor(SerializableActorObjective actor) : this()
        {
            _mainPed = null;
            _mainActorObjective = actor;
        }

        private WaypointEditor()
        {
            CreateWaypointMenu = new UIMenu("", "WAYPOINTS", new Point(0, -107));
            _waypointPropertiesMenu = new UIMenu("", "EDIT WAYPOINT", new Point(0, -107));
            _children = new List<UIMenu>();

            CreateWaypointMenu.SetBannerType(new ResRectangle());
            CreateWaypointMenu.MouseControlsEnabled = false;
            //CreateWaypointMenu.ResetKey(Common.MenuControls.Back);
            CreateWaypointMenu.ResetKey(Common.MenuControls.Up);
            CreateWaypointMenu.ResetKey(Common.MenuControls.Down);
            CreateWaypointMenu.SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
            CreateWaypointMenu.SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);

            CreateWaypointMenu.OnMenuClose += sender =>
            {
                IsInEditor = false;
                OnEditorExit?.Invoke(this, EventArgs.Empty);
            };

            _waypointPropertiesMenu.SetBannerType(new ResRectangle());
            _waypointPropertiesMenu.MouseControlsEnabled = false;
            _waypointPropertiesMenu.ResetKey(Common.MenuControls.Up);
            _waypointPropertiesMenu.ResetKey(Common.MenuControls.Down);
            _waypointPropertiesMenu.SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
            _waypointPropertiesMenu.SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);

            BuildCreateWaypointMenu();

            _waypointPropertiesMenu.OnMenuClose += sender =>
            {
                CreateWaypointMenu.Visible = true;
            };
        }
        #endregion 

        public bool IsInEditor { get; set; }
        public event EventHandler OnEditorExit;

        private SerializablePed _mainPed;
        private SerializableActorObjective _mainActorObjective;
        public UIMenu CreateWaypointMenu;
        private UIMenu _waypointPropertiesMenu;
        private List<UIMenu> _children;

        private WaypointTypes _placingWaypointType;

        public void Enter()
        {
            Editor.Editor.DisableControlEnabling = false;
            IsInEditor = true;
            CreateWaypointMenu.Visible = true;
        }

        public void BuildCreateWaypointMenu()
        {
            CreateWaypointMenu.Clear();
            
            var list = new List<string>(Enum.GetNames(typeof(WaypointTypes)));
            _placingWaypointType = (WaypointTypes)Enum.Parse(typeof(WaypointTypes), list[0]);
            for (int i = 0; i < list.Count; i++)
            {
                var item = new UIMenuItem("Create Waypoint of Type: " + list[i]);
                CreateWaypointMenu.AddItem(item);
            }

            CreateWaypointMenu.OnIndexChange += (sender, index) =>
            {
                _placingWaypointType = (WaypointTypes)Enum.Parse(typeof(WaypointTypes), list[index]);
            };
            
            CreateWaypointMenu.RefreshIndex();
        }

        public void RebuildWaypointPropertiesMenu(SerializableWaypoint waypoint)
        {
            _children.Clear();
            _waypointPropertiesMenu.Clear();
            _waypointPropertiesMenu.Subtitle.Caption = waypoint.Type.ToString().ToUpper() + " PROPERTIES";

            {
                var item = new UIMenuItem("Duration", "Task duration in seconds. Leave blank to wait until the task is done.");
                _waypointPropertiesMenu.AddItem(item);

                item.SetRightLabel(waypoint.Duration == 0 ? "Wait Until Done" : waypoint.Duration.ToString());


                item.Activated += (sender, selectedItem) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        _waypointPropertiesMenu.ResetKey(Common.MenuControls.Back);
                        Editor.Editor.DisableControlEnabling = true;
                        string title = Util.GetUserInput();

                        Editor.Editor.DisableControlEnabling = false;
                        _waypointPropertiesMenu.SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);

                        if (string.IsNullOrEmpty(title))
                        {
                            waypoint.Duration = 0;
                            selectedItem.SetRightLabel("Wait Until Done");
                            return;
                        }
                        float duration;
                        if (!float.TryParse(title, NumberStyles.Float, CultureInfo.InvariantCulture, out duration))
                        {
                            waypoint.Duration = 0;
                            selectedItem.SetRightLabel("Wait Until Done");
                            Game.DisplayNotification("Incorrect format.");
                        }
                        waypoint.Duration = (int)(duration*1000);
                        selectedItem.SetRightLabel(duration.ToString());
                    });
                };
            }

            if (waypoint.Type == WaypointTypes.Animation)
            {
                var db = StaticData.AnimData.Database.ToDictionary(k => k.Key, k => k.Value.Select(x => x.Item1).ToArray());
                var menu = new CategorySelectionMenu(db, "Animation", true, "SELECT ANIMATION");
                var item = new UIMenuItem("Select Animation");
                menu.Build(db.ElementAt(0).Key);

                if (!string.IsNullOrEmpty(waypoint.AnimName))
                {
                    var categName = StaticData.AnimData.Database.FirstOrDefault(cats =>
                    {
                        return cats.Value.Any(v => v.Item3 == waypoint.AnimName);
                    }).Key;

                    var humanName = StaticData.AnimData.Database[categName].FirstOrDefault(v => v.Item3 == waypoint.AnimName)?.Item1;
                    if(!string.IsNullOrEmpty(humanName))
                        item.SetRightLabel(humanName);
                }

                menu.SelectionChanged += (sender, args) =>
                {
                    var pair =
                        StaticData.AnimData.Database[menu.CurrentSelectedCategory].First(
                            m => m.Item1 == menu.CurrentSelectedItem);
                    waypoint.AnimDict = pair.Item2;
                    waypoint.AnimName = pair.Item3;
                };

                _waypointPropertiesMenu.AddItem(item);
                _waypointPropertiesMenu.BindMenuToItem(menu, item);

                _children.Add(menu);
            }

            if (waypoint.Type == WaypointTypes.Drive)
            {
                {
                    var item = new UIMenuItem("Driving Speed", "Driving speed in meters per second.");
                    _waypointPropertiesMenu.AddItem(item);

                    item.SetRightLabel(waypoint.VehicleSpeed == 0 ? "Default" : waypoint.VehicleSpeed.ToString());


                    item.Activated += (sender, selectedItem) =>
                    {
                        GameFiber.StartNew(delegate
                        {
                            _waypointPropertiesMenu.ResetKey(Common.MenuControls.Back);
                            Editor.Editor.DisableControlEnabling = true;
                            string title = Util.GetUserInput();

                            Editor.Editor.DisableControlEnabling = false;
                            _waypointPropertiesMenu.SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);

                            if (string.IsNullOrEmpty(title))
                            {
                                waypoint.VehicleSpeed = 20;
                                selectedItem.SetRightLabel("Default");
                                return;
                            }
                            float duration;
                            if (!float.TryParse(title, NumberStyles.Float, CultureInfo.InvariantCulture, out duration))
                            {
                                waypoint.VehicleSpeed = 20;
                                selectedItem.SetRightLabel("Default");
                                Game.DisplayNotification("Incorrect format.");
                            }
                            waypoint.VehicleSpeed = duration;
                            selectedItem.SetRightLabel(duration.ToString());
                        });
                    };
                }

                {
                    var parsedList = StaticData.StaticLists.DrivingStylesList.Select(t => (dynamic)t.Item1);
                    var indexOf =
                        StaticData.StaticLists.DrivingStylesList.FindIndex(tup => tup.Item2 == waypoint.DrivingStyle);
                    var item = new UIMenuListItem("Driving Style", parsedList.ToList(), indexOf);
                    _waypointPropertiesMenu.AddItem(item);

                    item.OnListChanged += (sender, index) =>
                    {
                        waypoint.DrivingStyle = StaticData.StaticLists.DrivingStylesList[index].Item2;
                    };
                }
            }

            _waypointPropertiesMenu.RefreshIndex();
        }

        public void RepresentCurrentWaypoints()
        {
            var list = new List<SerializableWaypoint>();
            list.AddRange(_mainPed != null ? _mainPed.Waypoints : _mainActorObjective.Waypoints);
            for (int i = 0; i < list.Count; i++)
            {
                Util.DrawMarker(24, list[i].Position, new Vector3(), new Vector3(1,1,1), Color.GreenYellow);
                if(i == 0) continue;
                var from = list[i - 1].Position;
                var to = list[i].Position;
                NativeFunction.CallByName<uint>("DRAW_LINE", from.X, from.Y, from.Z, to.X, to.Y, to.Z, 255, 255, 255,
                    255);
            }
        }

        private void DrawCrosshair(Vector3 pos)
        {
            Util.DrawMarker(24, pos + new Vector3(0,0,1), new Vector3(), new Vector3(1, 1, 1), Color.DarkGreen);
        }

        public SerializableWaypoint CheckForIntersection(Vector3 pos)
        {
            var threshold = 1.5f;
            var list = new List<SerializableWaypoint>(_mainPed != null ? _mainPed.Waypoints : _mainActorObjective.Waypoints);

            foreach (var wpy in list)
            {
                if ((wpy.Position - pos).Length() > threshold) continue;
                Editor.Editor.RingData.Color = Color.Yellow;
                return wpy;
            }
            Editor.Editor.RingData.Color = Color.Gray;
            return null;
        }

        public SerializableWaypoint CreateWaypoint(WaypointTypes type, Vector3 pos, Entity ent)
        {
            var wpy = new SerializableWaypoint();
            wpy.Type = type;
            wpy.Position = pos;
            wpy.Duration = 0;
            wpy.VehicleSpeed = 20f;
            wpy.DrivingStyle = 0xC00AB;

            switch (type)
            {
                case WaypointTypes.EnterVehicle:
                    if (ent != null && ent.IsValid() && ent.IsVehicle())
                    {
                        wpy.VehicleTargetModel = ent.Model.Hash;
                    }
                    break;
            }


            if(_mainPed != null)
                _mainPed.Waypoints.Add(wpy);
            else
                _mainActorObjective.Waypoints.Add(wpy);

            return wpy;
        }

        private void DrawInstructionalButtonsScaleform()
        {
            var instructButts = new Scaleform();
            instructButts.Load("instructional_buttons");
            instructButts.CallFunction("CLEAR_ALL");
            instructButts.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
            instructButts.CallFunction("CREATE_CONTAINER");
            instructButts.CallFunction("SET_DATA_SLOT", 0, Util.GetControlButtonId(GameControl.Attack), "Place Waypoint");
            instructButts.CallFunction("SET_DATA_SLOT", 1, Util.GetControlButtonId(GameControl.Attack), "Waypoint Properties");
            instructButts.CallFunction("SET_DATA_SLOT", 2, Util.GetControlButtonId(GameControl.CreatorDelete), "Remove Waypoint");
            instructButts.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
        }

        public void Process(Vector3 markerPos, Entity crossent)
        {
            CreateWaypointMenu.ProcessControl();
            CreateWaypointMenu.Draw();

            _waypointPropertiesMenu.ProcessControl();
            _waypointPropertiesMenu.Draw();

            if (_children != null)
                foreach (var menu in _children)
                {
                    menu.ProcessControl();
                    menu.Draw();
                }

            if (!IsInEditor) return;
            RepresentCurrentWaypoints();
            DrawInstructionalButtonsScaleform();
            if (CreateWaypointMenu.Visible && Util.IsDisabledControlJustPressed(GameControl.CellphoneCancel))
            {
                
                return;
            }

            if (crossent != null && crossent.IsValid() && crossent.IsVehicle())
            {
                var item = CreateWaypointMenu.MenuItems.FirstOrDefault(i => i.Text.EndsWith("EnterVehicle"));
                if (item != null)
                    item.Enabled = true;
            }
            else
            {
                var item = CreateWaypointMenu.MenuItems.FirstOrDefault(i => i.Text.EndsWith("EnterVehicle"));
                if (item != null)
                    item.Enabled = false;
            }

            var wpy = CheckForIntersection(markerPos);
            if (wpy != null && CreateWaypointMenu.Visible)
            {
                if (Util.IsDisabledControlJustPressed(GameControl.Attack))
                {
                    RebuildWaypointPropertiesMenu(wpy);
                    CreateWaypointMenu.Visible = false;
                    _waypointPropertiesMenu.Visible = true;
                    return;
                }

                if (Util.IsDisabledControlJustPressed(GameControl.CreatorDelete))
                {
                    if (_mainPed != null)
                        _mainPed.Waypoints.Remove(wpy);
                    else _mainActorObjective.Waypoints.Remove(wpy);
                    return;
                }
            }

            if (CreateWaypointMenu.Visible)
            {
                DrawCrosshair(markerPos);
                //if (Game.IsControlJustPressed(0, GameControl.Attack))
                if (Util.IsDisabledControlJustPressed(GameControl.Attack))
                {
                    CreateWaypoint(_placingWaypointType, markerPos + new Vector3(0,0,1), crossent);
                    return;
                }
            }
        }
    }
}