using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ContentCreator.Editor.NestedMenus;
using ContentCreator.SerializableData;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace ContentCreator.Editor
{
    public class Editor
    {
        public Editor()
        {
            Children = new List<INestedMenu>();

            #region NativeUI Initialization
            _menuPool = new MenuPool();
            #region Main Menu
            _mainMenu = new UIMenu("Content Creator", "MAIN MENU");
            _mainMenu.ResetKey(Common.MenuControls.Back);
            _mainMenu.ResetKey(Common.MenuControls.Up);
            _mainMenu.ResetKey(Common.MenuControls.Down);
            _mainMenu.SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
            _mainMenu.SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);
            _menuPool.Add(_mainMenu);

            {
                var menuItem = new NativeMenuItem("Create a Mission", "Create a new mission.");
                menuItem.Activated += (sender, item) =>
                {
                    CreateNewMission();
                    EnterFreecam();
                };
                _mainMenu.AddItem(menuItem);
            }

            {
                var menuItem = new NativeMenuItem("Load Mission", "Load your mission for editing.");
                menuItem.Activated += (sender, item) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        string path = Util.GetUserInput();
                        if (string.IsNullOrEmpty(path)) return;
                        LoadMission(path);
                    });
                };
                _mainMenu.AddItem(menuItem);
            }

            {
                var menuItem = new NativeMenuItem("Exit to Grand Theft Auto V", "Leave the Content Creator");
                menuItem.Activated += (sender, item) =>
                {
                    LeaveEditor();
                };
                _mainMenu.AddItem(menuItem);
            }

            _menuPool.ToList().ForEach(menu =>
            {
                menu.RefreshIndex();
                menu.MouseControlsEnabled = false;
                menu.MouseEdgeEnabled = false;
            });
            #endregion

            #region Editor Menu
            _missionMenu = new UIMenu("Content Creator", "MISSION MAIN MENU");
            _missionMenu.ResetKey(Common.MenuControls.Back);
            _missionMenu.MouseControlsEnabled = false;
            _missionMenu.ResetKey(Common.MenuControls.Up);
            _missionMenu.ResetKey(Common.MenuControls.Down);
            _missionMenu.SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
            _missionMenu.SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);
            _menuPool.Add(_missionMenu);
            #endregion
            

            #endregion

            RingData = new RingData()
            {
                Display = true,
                Type = RingType.HorizontalCircleSkinny,
                Radius = 2f,
                Color = Color.Gray,
            };

            MarkerData = new MarkerData()
            {
                Display = false,
            };

            MarkerData.OnMarkerTypeChange += (sender, args) =>
            {
                if (string.IsNullOrEmpty(MarkerData.MarkerType))
                {
                    if (_mainObject != null && _mainObject.IsValid())
                        _mainObject.Delete();
                    return;
                }
                var pos = Game.LocalPlayer.Character.Position;
                if (_mainObject != null && _mainObject.IsValid())
                {
                    pos = _mainObject.Position;
                    _mainObject.Delete();
                }
                GameFiber.StartNew(delegate
                {
                    _mainObject = new Object(Util.RequestModel(MarkerData.MarkerType), pos);
                    NativeFunction.CallByName<uint>("SET_ENTITY_COLLISION", _mainObject.Handle.Value, false, 0);
                });
            };

            
        }

        #region NativeUI
        private MenuPool _menuPool;
        private UIMenu _mainMenu;

        private UIMenu _missionMenu;
        #endregion

        #region Public Variables and Properties

        public bool IsInEditor { get; set; }
        public bool IsInMainMenu { get; set; }
        public bool IsInFreecam { get; set; }
        public Camera MainCamera { get; set; }
        public bool BigMinimap { get; set; }
        public static bool DisableControlEnabling { get; set; }
        public static bool EnableBasicMenuControls { get; set; }
        public static RingData RingData { get; set; }
        public static MarkerData MarkerData { get; set; }
        public static MissionData CurrentMission { get; set; }
        public static bool PlayerSpawnOpen { get; set; }
        #endregion

        #region Private Variables
        private Object _mainObject;

        private float _ringRotation = 0f;
        private float _objectRotation = 0f;
        private List<INestedMenu> Children;
        private Entity _hoveringEntity;
        private UIMenu _placementMenu;
        private INestedMenu _propertiesMenu;
        #endregion

        #region Constants
        
        #endregion

        private void EnterFreecam()
        {
            Camera.DeleteAllCameras();
            MainCamera = new Camera(true);
            MainCamera.Active = true;
            MainCamera.Position = Game.LocalPlayer.Character.Position + new Vector3(0f, 0f, 10f);
            Game.LocalPlayer.Character.Opacity = 0;
            
            _mainMenu.Visible = false;
            _missionMenu.Visible = true;

            IsInFreecam = true;
            IsInMainMenu = false;
            IsInEditor = true;
            BigMinimap = true;
            DisableControlEnabling = false;
            EnableBasicMenuControls = false;
        }

        public void EnterEditor()
        {
            IsInMainMenu = true;
            _mainMenu.Visible = true;
            _mainMenu.RefreshIndex();
        }

        public void LeaveEditor()
        {
            if (CurrentMission != null)
            {
                CurrentMission.Vehicles.ForEach(v =>
                {
                    if (v.GetEntity() != null && v.GetEntity().IsValid())
                    {
                        v.GetEntity().Delete();
                    }
                });

                CurrentMission.Actors.ForEach(v =>
                {
                    if (v.GetEntity() != null && v.GetEntity().IsValid())
                    {
                        v.GetEntity().Delete();
                    }
                });

                CurrentMission.Objects.ForEach(v =>
                {
                    if (v.GetEntity() != null && v.GetEntity().IsValid())
                    {
                        v.GetEntity().Delete();
                    }
                });

                CurrentMission.Spawnpoints.ForEach(v =>
                {
                    if (v.GetEntity() != null && v.GetEntity().IsValid())
                    {
                        v.GetEntity().Delete();
                    }
                });
            }


            IsInMainMenu = false;
            _menuPool.CloseAllMenus();
            BigMinimap = false;
            IsInFreecam = false;
            IsInEditor = false;

            if(_mainObject != null && _mainObject.IsValid())
                _mainObject.Delete();

            NativeFunction.CallByHash<uint>(0x231C8F89D0539D8F, false, false);

            CurrentMission = null;
            if (MainCamera != null)
                MainCamera.Active = false;
            Game.LocalPlayer.Character.Opacity = 1f;
            Game.LocalPlayer.Character.Position -= new Vector3(0, 0, Game.LocalPlayer.Character.HeightAboveGround - 1f);
        }

        public void CreateNewMission()
        {
            CurrentMission = new MissionData();
            CurrentMission.Vehicles = new List<SerializableVehicle>();
            CurrentMission.Actors = new List<SerializablePed>();
            CurrentMission.Objects = new List<SerializableObject>();
            CurrentMission.Spawnpoints = new List<SerializableSpawnpoint>();
            menuDirty = true;
        }

        private bool menuDirty;

        public void LoadMission(string path)
        {
            
        }

        private void DisplayMarker(Vector3 pos, Vector3 directionalVector)
        {
            if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid())
            {
                MarkerData.RepresentedBy.Position = pos;
            }
            

            if (_mainObject != null && _mainObject.IsValid() && MarkerData.Display)
            {
                _mainObject.Position = pos + new Vector3(0f, 0f, 0.1f + MarkerData.HeightOffset);
                _mainObject.Rotation = new Rotator(0f, 0f, MarkerData.HeadingOffset + Util.DirectionToRotation(directionalVector).Z);
            }

            if (RingData.Display)
            {
                var pos2 = pos + new Vector3(0f, 0f, 0.1f + RingData.HeightOffset);
                NativeFunction.CallByName<uint>("DRAW_MARKER", (int)RingData.Type, pos2.X, pos2.Y, pos2.Z, 0f, 0f, 0f,
                    0f, 0f, RingData.Heading, RingData.Radius, RingData.Radius, 0.75f, (int)RingData.Color.R, (int)RingData.Color.G, (int)RingData.Color.B, (int)RingData.Color.A, false, false,
                    2, false, false, false, false);
            }
        }

        public void RebuildMissionMenu(MissionData data)
        {
            _missionMenu.Clear();
            Children.Clear();

            {
                var nestMenu = new MissionInfoMenu(CurrentMission);
                var nestItem = new NativeMenuItem("Mission Details");
                _missionMenu.AddItem(nestItem);
                _missionMenu.BindMenuToItem(nestMenu, nestItem);
                _menuPool.Add(nestMenu);
                Children.Add(nestMenu);
            }

            {
                var nestMenu = new PlacementMenu(CurrentMission);
                var nestItem = new NativeMenuItem("Placement");
                _missionMenu.AddItem(nestItem);
                _missionMenu.BindMenuToItem(nestMenu, nestItem);
                _menuPool.Add(nestMenu);
                Children.Add(nestMenu);
                _placementMenu = nestMenu;
            }

            {
                var exitItem = new NativeMenuItem("Exit");
                exitItem.Activated += (sender, item) =>
                {
                    LeaveEditor();
                };
                _missionMenu.AddItem(exitItem);
            }

            _missionMenu.RefreshIndex();
        }

        private void CheckForIntersection(Entity ent)
        {
            if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid() && ent != null && ent.IsValid())
            {
                if (MarkerData.RepresentedBy is Vehicle && ent.IsVehicle() &&
                    CurrentMission.Vehicles.Any(m => m.GetEntity().Handle.Value == ent.Handle.Value))
                {
                    MarkerData.RepresentedBy.Opacity = 0f;
                    MarkerData.HeadingOffset = 45f;
                    RingData.Color = Color.Red;
                    _hoveringEntity = ent;
                }

                else if (MarkerData.RepresentedBy is Ped && ent.IsPed() &&
                    (CurrentMission.Actors.Any(m => m.GetEntity().Handle.Value == ent.Handle.Value) || 
                    CurrentMission.Spawnpoints.Any(m => m.GetEntity().Handle.Value == ent.Handle.Value)))
                {
                    MarkerData.RepresentedBy.Opacity = 0f;
                    MarkerData.HeadingOffset = 45f;
                    RingData.Color = Color.Red;
                    _hoveringEntity = ent;
                }

                else if (MarkerData.RepresentedBy is Ped && ent.IsVehicle() &&
                    CurrentMission.Vehicles.Any(m => m.GetEntity().Handle.Value == ent.Handle.Value) &&
                    ((Vehicle)ent).GetFreeSeatIndex().HasValue)
                {
                    RingData.Color = Color.GreenYellow;
                    _hoveringEntity = ent;
                }

                else if (MarkerData.RepresentedBy is Object && ent.IsObject() &&
                    CurrentMission.Objects.Any(o => o.GetEntity().Handle.Value == ent.Handle.Value))
                {
                    MarkerData.RepresentedBy.Opacity = 0f;
                    MarkerData.HeadingOffset = 45f;
                    RingData.Color = Color.Red;
                    _hoveringEntity = ent;
                }
            }
            else if (_hoveringEntity != null && _hoveringEntity.IsValid() && MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid())
            {
                MarkerData.RepresentedBy.Opacity = 1f;
                MarkerData.HeadingOffset = 0f;
                RingData.Color = Color.MediumPurple;
                _hoveringEntity = null;
            }
        }

        private void CheckForPickup(Entity ent)
        {
            if (ent != null && ent.IsValid())
            {
                if (ent.IsVehicle() && CurrentMission.Vehicles.Any(m => m.GetEntity().Handle.Value == ent.Handle.Value))
                {
                    RingData.Color = Color.Yellow;
                    _hoveringEntity = ent;
                }

                else if (ent.IsPed() && (CurrentMission.Actors.Any(m => m.GetEntity().Handle.Value == ent.Handle.Value) ||
                    CurrentMission.Spawnpoints.Any(m => m.GetEntity().Handle.Value == ent.Handle.Value)))
                {
                    RingData.Color = Color.Yellow;
                    _hoveringEntity = ent;
                }

                else if (ent.IsObject() && CurrentMission.Objects.Any(o => o.GetEntity().Handle.Value == ent.Handle.Value))
                {
                    RingData.Color = Color.Yellow;
                    _hoveringEntity = ent;
                }
            }
            else if (_hoveringEntity != null && _hoveringEntity.IsValid())
            {
                RingData.Color = Color.Gray;
                _hoveringEntity = null;
            }
        }

        private void EnableControls()
        {
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.Attack);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.Aim);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.LookLeftRight);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.LookUpDown);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CursorX);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CursorY);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CursorScrollUp);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CursorScrollDown);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CreatorLT);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CreatorRT);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CellphoneSelect);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CellphoneRight);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CellphoneLeft);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.FrontendAccept);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.FrontendPause);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.FrontendPauseAlternate);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.MoveLeftRight);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.MoveUpDown);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.MoveLeftOnly);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.MoveRightOnly);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.MoveUpOnly);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.MoveDownOnly);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.FrontendLb);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.FrontendRb);
        }

        private void EnableMenuControls()
        {
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CursorX);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CursorY);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CellphoneSelect);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CellphoneRight);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CellphoneLeft);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CellphoneUp);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CellphoneDown);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.CellphoneCancel);
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.FrontendPauseAlternate);
        }

        public SerializablePed CreatePed(Model model, Vector3 pos, float heading)
        {
            var tmpPed = new Ped(model, pos, heading);
            tmpPed.IsPositionFrozen = false;

            var tmpObj = new SerializablePed();
            tmpObj.SetEntity(tmpPed);
            tmpObj.SpawnAfter = 0;
            tmpObj.RemoveAfter = 0;
            tmpObj.Behaviour = 0;
            tmpObj.RelationshipGroup = 3;
            tmpObj.WeaponAmmo = 9999;
            tmpObj.WeaponHash = 0;
            tmpObj.Health = 200;
            tmpObj.Armor = 0;
            tmpObj.SpawnInVehicle = false;
            CurrentMission.Actors.Add(tmpObj);
            return tmpObj;
        }

        public SerializableSpawnpoint CreateSpawnpoint(Model model, Vector3 pos, float heading)
        {
            var tmpPed = new Ped(model, pos, heading);
            tmpPed.IsPositionFrozen = true;

            var tmpObj = new SerializableSpawnpoint();
            tmpObj.SetEntity(tmpPed);
            tmpObj.SpawnAfter = 0;
            tmpObj.RemoveAfter = 0;
            tmpObj.WeaponAmmo = 9999;
            tmpObj.WeaponHash = 0;
            tmpObj.Health = 200;
            tmpObj.Armor = 0;
            tmpObj.SpawnInVehicle = false;
            CurrentMission.Spawnpoints.Add(tmpObj);
            return tmpObj;
        }

        public SerializableVehicle CreateVehicle(Model model, Vector3 pos, Rotator rotation, Color primColor, Color seconColor)
        {
            var tmpVeh = new Vehicle(model, pos)
            {
                PrimaryColor = primColor,
                SecondaryColor = seconColor,
            };
            tmpVeh.IsPositionFrozen = false;
            tmpVeh.Rotation = rotation;
            var tmpObj = new SerializableVehicle();
            tmpObj.SetEntity(tmpVeh);
            tmpObj.SpawnAfter = 0;
            tmpObj.RemoveAfter = 0;
            tmpObj.FailMissionOnDeath = false;
            tmpObj.Health = 1000;
            CurrentMission.Vehicles.Add(tmpObj);
            return tmpObj;
        }

        public SerializableObject CreateObject(Model model, Vector3 pos, Rotator rot)
        {
            var tmpObject = new Object(model, pos);
            tmpObject.Rotation = rot;
            tmpObject.Position = pos;
            tmpObject.IsPositionFrozen = false;
            var tmpObj = new SerializableObject();
            tmpObj.SetEntity(tmpObject);
            tmpObj.SpawnAfter = 0;
            tmpObj.RemoveAfter = 0;
            CurrentMission.Objects.Add(tmpObj);
            return tmpObj;
        }

        public void Tick(GraphicsEventArgs canvas)
        {
            if (menuDirty)
            {
                RebuildMissionMenu(CurrentMission);
                _propertiesMenu = null;
                menuDirty = false;
            }
            else
            {
                _menuPool.ProcessMenus();
                Children.ForEach(x => x.Process());
                if (_propertiesMenu != null)
                {
                    _propertiesMenu.Process();
                    ((UIMenu)_propertiesMenu).ProcessControl();
                    ((UIMenu)_propertiesMenu).ProcessMouse();
                    ((UIMenu)_propertiesMenu).Draw();
                }
            }

            if (IsInMainMenu)
            {
                NativeFunction.CallByName<uint>("HIDE_HUD_AND_RADAR_THIS_FRAME");
            }

            if (!IsInEditor) return;
            NativeFunction.CallByName<uint>("HIDE_HUD_AND_RADAR_THIS_FRAME");
            NativeFunction.CallByName<uint>("DISABLE_CONTROL_ACTION", 0, (int)GameControl.FrontendPauseAlternate);

            NativeFunction.CallByHash<uint>(0x231C8F89D0539D8F, BigMinimap, false);

            
            if (IsInFreecam)
            {
                var markerPos = Util.RaycastEverything(new Vector2(0, 0), MainCamera, MarkerData.RepresentedBy ?? _mainObject);

                #region Camera Movement

                if (!DisableControlEnabling)
                {
                    NativeFunction.CallByName<uint>("DISABLE_ALL_CONTROL_ACTIONS", 0);
                    EnableControls();
                }
                if (EnableBasicMenuControls)
                {
                    EnableMenuControls();
                }
                MainCamera.Active = true;

                var mouseX = NativeFunction.CallByName<float>("GET_CONTROL_NORMAL", 0, (int) GameControl.LookLeftRight);
                var mouseY = NativeFunction.CallByName<float>("GET_CONTROL_NORMAL", 0, (int) GameControl.LookUpDown);

                mouseX *= -1; //Invert
                mouseY *= -1;


                float movMod = 0.1f;
                float entMod = 1;

                if (Util.IsDisabledControlPressed(GameControl.Sprint))
                {
                    movMod = 0.5f;
                    entMod = 0.5f;
                }
                else if (Util.IsDisabledControlPressed(GameControl.CharacterWheel))
                {
                    movMod = 0.02f;
                    entMod = 0.02f;
                }

                bool zoomIn = false;
                bool zoomOut = false;

                if (Util.IsGamepadEnabled)
                {
                    mouseX *= 2; //TODO: settings
                    mouseY *= 2;

                    movMod *= 5f;

                    zoomIn = Game.IsControlPressed(0, GameControl.CreatorRT);
                    zoomOut = Game.IsControlPressed(0, GameControl.CreatorLT);
                }
                else
                {
                    mouseX *= 20;
                    mouseY *= 20;

                    movMod *= 10f;

                    zoomIn = Game.IsControlPressed(0, GameControl.CursorScrollUp);
                    zoomOut = Game.IsControlPressed(0, GameControl.CursorScrollDown);
                }



                MainCamera.Rotation = new Rotator((MainCamera.Rotation.Pitch + mouseY).Clamp(-85f, -30f), 0f,
                    MainCamera.Rotation.Yaw + mouseX);

                var dir = Util.RotationToDirection(new Vector3(MainCamera.Rotation.Pitch, MainCamera.Rotation.Roll,
                        MainCamera.Rotation.Yaw));
                var rotLeft = new Vector3(MainCamera.Rotation.Pitch, MainCamera.Rotation.Roll,
                    MainCamera.Rotation.Yaw - 10f);
                var rotRight = new Vector3(MainCamera.Rotation.Pitch, MainCamera.Rotation.Roll,
                    MainCamera.Rotation.Yaw + 10f);
                var right = Util.RotationToDirection(rotRight) - Util.RotationToDirection(rotLeft);

                Vector3 movementVector = new Vector3();

                if (zoomIn)
                {
                    var directionalVector = dir*movMod;
                    movementVector += directionalVector;
                }
                if (zoomOut)
                {
                    var directionalVector = dir*movMod;
                    movementVector -= directionalVector;
                }

                if (Game.IsControlPressed(0, GameControl.MoveUpOnly))
                {
                    var directionalVector = dir*movMod;
                    movementVector += new Vector3(directionalVector.X, directionalVector.Y, 0f);
                }
                if (Game.IsControlPressed(0, GameControl.MoveDownOnly))
                {
                    var directionalVector = dir*movMod;
                    movementVector -= new Vector3(directionalVector.X, directionalVector.Y, 0f);
                }
                if (Game.IsControlPressed(0, GameControl.MoveLeftOnly))
                {
                    movementVector += right*movMod;
                }
                if (Game.IsControlPressed(0, GameControl.MoveRightOnly))
                {
                    movementVector -= right*movMod;
                }
                MainCamera.Position += movementVector;
                Game.LocalPlayer.Character.Position = MainCamera.Position;

                #endregion

                var ent = Util.RaycastEntity(new Vector2(0, 0),
                    MainCamera.Position,
                    new Vector3(MainCamera.Rotation.Pitch, MainCamera.Rotation.Roll, MainCamera.Rotation.Yaw)
                    , null);

                if(MarkerData.RepresentedBy == null || !MarkerData.RepresentedBy.IsValid())
                    CheckForPickup(ent);
                else
                    CheckForIntersection(ent);
                
                DisplayMarker(markerPos, dir);
            }
            else // bug: a lot of things dont work in on foot mode
            {
                var gameplayCoord = NativeFunction.CallByName<Vector3>("GET_GAMEPLAY_CAM_COORD");
                var gameplayRot = NativeFunction.CallByName<Vector3>("GET_GAMEPLAY_CAM_ROT", 2);
                var markerPos = Util.RaycastEverything(new Vector2(0, 0), gameplayCoord, gameplayRot, Game.LocalPlayer.Character);

                #region Controls
                Game.DisableControlAction(0, GameControl.Phone, true);
                if (!DisableControlEnabling)
                {
                    EnableControls();
                }
                if (EnableBasicMenuControls)
                {
                    EnableMenuControls();
                }

                #endregion

                var ent = Util.RaycastEntity(new Vector2(0, 0),
                    gameplayCoord,
                    gameplayRot, Game.LocalPlayer.Character);

                if (MarkerData.RepresentedBy == null || !MarkerData.RepresentedBy.IsValid())
                    CheckForIntersection(ent);
                else
                    CheckForPickup(ent);

                DisplayMarker(markerPos, markerPos - NativeFunction.CallByName<Vector3>("GET_GAMEPLAY_CAM_COORD"));

                if (!_menuPool.IsAnyMenuOpen())
                {
                    Game.DisplayHelp("Press ~INPUT_PHONE~ to open menu.");
                    if (Util.IsDisabledControlJustPressed(GameControl.Phone))
                    {
                        _mainMenu.Visible = true;
                    }
                }
            }

            if (Game.IsControlJustPressed(0, GameControl.FrontendPause) && IsInFreecam)
            {
                GameFiber.StartNew(delegate
                {
                    Game.FadeScreenOut(800, true);
                    Game.LocalPlayer.Character.Position -= new Vector3(0, 0, Game.LocalPlayer.Character.HeightAboveGround - 1f);
                    IsInFreecam = false;
                    MainCamera.Active = false;
                    Game.LocalPlayer.Character.Opacity = 1f;
                    _missionMenu.SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                    _mainMenu.SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                    Game.FadeScreenIn(800);
                });
            }
            else if (Game.IsControlJustPressed(0, GameControl.FrontendPause) && !IsInFreecam)
            {
                GameFiber.StartNew(delegate
                {
                    Game.FadeScreenOut(800, true);
                    IsInFreecam = true;
                    MainCamera.Active = true;
                    Game.LocalPlayer.Character.Opacity = 0f;
                    _missionMenu.ResetKey(Common.MenuControls.Back);
                    _mainMenu.ResetKey(Common.MenuControls.Back);
                    _menuPool.CloseAllMenus();
                    Children.ForEach(x => x.Children.ForEach(n => n.Visible = false));
                    _missionMenu.Visible = true;
                    Game.FadeScreenIn(800);
                });
            }

            #region Marker Spawning/Deletion

            if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid() &&
                (_hoveringEntity == null || !_hoveringEntity.IsValid()) &&
                Game.IsControlPressed(0, GameControl.FrontendLb))
            {
                MarkerData.RepresentedBy.Rotation = new Rotator(MarkerData.RepresentedBy.Rotation.Pitch,
                                                                MarkerData.RepresentedBy.Rotation.Roll,
                                                                MarkerData.RepresentedBy.Rotation.Yaw + 3f);
                RingData.Heading += 3f;
            }

            if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid() &&
                (_hoveringEntity == null || !_hoveringEntity.IsValid()) &&
                Game.IsControlPressed(0, GameControl.FrontendRb))
            {
                MarkerData.RepresentedBy.Rotation = new Rotator(MarkerData.RepresentedBy.Rotation.Pitch,
                                                                MarkerData.RepresentedBy.Rotation.Roll,
                                                                MarkerData.RepresentedBy.Rotation.Yaw - 3f);
                RingData.Heading -= 3f;
            }

            if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid() &&
                Game.IsControlJustPressed(0, GameControl.CellphoneSelect) &&
                (_hoveringEntity == null || !_hoveringEntity.IsValid()))
            {
                if (MarkerData.RepresentedBy is Vehicle)
                {
                    CreateVehicle(MarkerData.RepresentedBy.Model, MarkerData.RepresentedBy.Position,
                                    MarkerData.RepresentedBy.Rotation,
                                    ((Vehicle)MarkerData.RepresentedBy).PrimaryColor,
                                    ((Vehicle)MarkerData.RepresentedBy).SecondaryColor);
                }

                else if (MarkerData.RepresentedBy is Ped && !PlayerSpawnOpen)
                {
                    CreatePed(MarkerData.RepresentedBy.Model, MarkerData.RepresentedBy.Position - new Vector3(0, 0, 1f), MarkerData.RepresentedBy.Heading);
                }

                else if (MarkerData.RepresentedBy is Ped && PlayerSpawnOpen)
                {
                    CreateSpawnpoint(MarkerData.RepresentedBy.Model, MarkerData.RepresentedBy.Position - new Vector3(0, 0, 1f), MarkerData.RepresentedBy.Heading);
                }

                else if (MarkerData.RepresentedBy is Object)
                {
                    CreateObject(MarkerData.RepresentedBy.Model, MarkerData.RepresentedBy.Position,
                        MarkerData.RepresentedBy.Rotation);
                }
            }
            else if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid() && 
                    _hoveringEntity != null && _hoveringEntity.IsValid() &&
                     Game.IsControlJustPressed(0, GameControl.CellphoneSelect))
            {
                if (_hoveringEntity.IsVehicle() && MarkerData.RepresentedBy.IsVehicle())
                {
                    foreach (var ped in ((Vehicle)_hoveringEntity).Occupants)
                    {
                        if (CurrentMission.Actors.Any(o => o.GetEntity().Handle.Value == ped.Handle.Value))
                        {
                            CurrentMission.Actors.First(o => o.GetEntity().Handle.Value == ped.Handle.Value)
                                .SpawnInVehicle = false;
                        }
                        else if (CurrentMission.Spawnpoints.Any(o => o.GetEntity().Handle.Value == ped.Handle.Value))
                        {
                            CurrentMission.Spawnpoints.First(o => o.GetEntity().Handle.Value == ped.Handle.Value)
                                .SpawnInVehicle = false;
                        }
                    }

                    CurrentMission.Vehicles.Remove(
                        CurrentMission.Vehicles.FirstOrDefault(
                            o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value));

                    _hoveringEntity.Delete();
                    if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid())
                        MarkerData.RepresentedBy.Opacity = 1f;
                    MarkerData.HeadingOffset = 0f;
                    RingData.Color = Color.MediumPurple;
                    _hoveringEntity = null;
                }
                else if (_hoveringEntity.IsVehicle() && MarkerData.RepresentedBy.IsPed())
                {
                    int? possibleSeat = ((Vehicle) _hoveringEntity).GetFreeSeatIndex();
                    if (possibleSeat.HasValue)
                    {
                        var newPed = CreatePed(MarkerData.RepresentedBy.Model, MarkerData.RepresentedBy.Position,
                            MarkerData.RepresentedBy.Heading);
                        ((Ped) newPed.GetEntity()).WarpIntoVehicle((Vehicle)_hoveringEntity, possibleSeat.Value);
                        newPed.SpawnInVehicle = true;
                        newPed.VehicleSeat = possibleSeat.Value;
                    }
                }
                else if(_hoveringEntity.IsPed() && MarkerData.RepresentedBy.IsPed() &&
                    CurrentMission.Actors.Any(m => m.GetEntity().Handle.Value == _hoveringEntity.Handle.Value))
                {
                    CurrentMission.Actors.Remove(
                        CurrentMission.Actors.FirstOrDefault(
                            o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value));

                    _hoveringEntity.Delete();
                    if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid())
                        MarkerData.RepresentedBy.Opacity = 1f;
                    MarkerData.HeadingOffset = 0f;
                    RingData.Color = Color.MediumPurple;
                    _hoveringEntity = null;
                }
                else if (_hoveringEntity.IsPed() && MarkerData.RepresentedBy.IsPed() &&
                    CurrentMission.Spawnpoints.Any(m => m.GetEntity().Handle.Value == _hoveringEntity.Handle.Value))
                {
                    CurrentMission.Spawnpoints.Remove(
                        CurrentMission.Spawnpoints.FirstOrDefault(
                            o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value));

                    _hoveringEntity.Delete();
                    if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid())
                        MarkerData.RepresentedBy.Opacity = 1f;
                    MarkerData.HeadingOffset = 0f;
                    RingData.Color = Color.MediumPurple;
                    _hoveringEntity = null;
                }
                else if (_hoveringEntity.IsObject() && MarkerData.RepresentedBy.IsObject())
                {
                    CurrentMission.Objects.Remove(
                        CurrentMission.Objects.FirstOrDefault(
                            o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value));

                    _hoveringEntity.Delete();
                    if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid())
                        MarkerData.RepresentedBy.Opacity = 1f;
                    MarkerData.HeadingOffset = 0f;
                    RingData.Color = Color.MediumPurple;
                    _hoveringEntity = null;
                }
                
            }
            else if (_hoveringEntity != null && _hoveringEntity.IsValid() &&
                     Game.IsControlJustPressed(0, GameControl.Attack) && 
                     (MarkerData.RepresentedBy == null || !MarkerData.RepresentedBy.IsValid()) &&
                     _propertiesMenu == null)
            {
                // TODO: Properties
                if (_hoveringEntity.IsPed() && CurrentMission.Actors.Any(o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value))
                {
                    _menuPool.CloseAllMenus();
                    Children.ForEach(x => x.Children.ForEach(n => n.Visible = false));

                    DisableControlEnabling = true;
                    EnableBasicMenuControls = true;
                    var newMenu = new ActorPropertiesMenu();
                    var actor = CurrentMission.Actors.FirstOrDefault(o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value);
                    newMenu.BuildFor(actor);

                    newMenu.OnMenuClose += sender =>
                    {
                        _missionMenu.Visible = true;
                        menuDirty = true;
                        RingData.Color = Color.Gray;
                        DisableControlEnabling = false;
                        EnableBasicMenuControls = false;
                    };


                    newMenu.Visible = true;
                    _propertiesMenu = newMenu;
                }
                else if (_hoveringEntity.IsPed() && CurrentMission.Spawnpoints.Any(o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value))
                {
                    _menuPool.CloseAllMenus();
                    Children.ForEach(x => x.Children.ForEach(n => n.Visible = false));

                    DisableControlEnabling = true;
                    EnableBasicMenuControls = true;
                    var newMenu = new SpawnpointPropertiesMenu();
                    var actor = CurrentMission.Spawnpoints.FirstOrDefault(o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value);
                    newMenu.BuildFor(actor);

                    newMenu.OnMenuClose += sender =>
                    {
                        _missionMenu.Visible = true;
                        menuDirty = true;
                        RingData.Color = Color.Gray;
                        DisableControlEnabling = false;
                        EnableBasicMenuControls = false;
                    };


                    newMenu.Visible = true;
                    _propertiesMenu = newMenu;
                }
                else if (_hoveringEntity.IsVehicle())
                {
                    _menuPool.CloseAllMenus();
                    Children.ForEach(x => x.Children.ForEach(n => n.Visible = false));

                    DisableControlEnabling = true;
                    EnableBasicMenuControls = true;
                    var newMenu = new VehiclePropertiesMenu();
                    var actor = CurrentMission.Vehicles.FirstOrDefault(o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value);
                    newMenu.BuildFor(actor);

                    newMenu.OnMenuClose += sender =>
                    {
                        _missionMenu.Visible = true;
                        menuDirty = true;
                        RingData.Color = Color.Gray;
                        DisableControlEnabling = false;
                        EnableBasicMenuControls = false;
                    };

                    newMenu.Visible = true;
                    _propertiesMenu = newMenu;
                }
                else if (_hoveringEntity.IsObject())
                {
                    //DisableControlEnabling = true;
                    //EnableBasicMenuControls = true;
                }
            }
        

            #endregion

        }
    }

    /* CC Props:
        - prop_mp_base_marker
        - prop_mp_cant_place_lrg
        - prop_mp_cant_place_med
        - prop_mp_cant_place_sm
        - prop_mp_max_out_lrg
        - prop_mp_max_out_med
        - prop_mp_max_out_sm
        - prop_mp_num_0 - 9
        - prop_mp_placement
        - prop_mp_placement_med
        - prop_mp_placement_lrg
        - prop_mp_placement_sm
        - prop_mp_placement_maxd
        - prop_mp_placement_red
        - prop_mp_repair (wrench)
        - prop_mp_repair_01
        - prop_mp_respawn_02
        - prop_mp_arrow_ring
        - prop_mp_halo
        - prop_mp_halo_lrg
        - prop_mp_halo_med
        - prop_mp_halo_point
        - prop_mp_halo_point_lrg
        - prop_mp_halo_point_med
        - prop_mp_halo_point_sm
        - prop_mp_halo_rotate
        - prop_mp_halo_rotate_lrg
        - prop_mp_halo_rotate_med
        - prop_mp_halo_rotate_sm
        - prop_mp_pointer_ring
        - prop_mp_solid_ring    
    */

}
