using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using ContentCreator.CutsceneEditor;
using ContentCreator.Editor.NestedMenus;
using ContentCreator.SerializableData;
using ContentCreator.SerializableData.Cutscenes;
using ContentCreator.SerializableData.Objectives;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;
using Object = Rage.Object;

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
                var menuItem = new NativeMenuItem("Play Mission", "Play a mission.");
                menuItem.Activated += (sender, item) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        Editor.DisableControlEnabling = true;
                        string path = Util.GetUserInput();
                        if (string.IsNullOrEmpty(path))
                        {
                            Editor.DisableControlEnabling = false;
                            return;
                        }
                        Editor.DisableControlEnabling = false;
                        var miss = ReadMission(path);
                        if (miss == null)
                        {
                            Game.DisplayNotification("Mission not found.");
                            return;
                        }
                        LeaveEditor();
                        EntryPoint.MissionPlayer.Load(miss);
                    });
                };
                _mainMenu.AddItem(menuItem);
            }

            {
                var menuItem = new NativeMenuItem("Load Mission", "Load your mission for editing.");
                menuItem.Activated += (sender, item) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        Editor.DisableControlEnabling = true;
                        string path = Util.GetUserInput();
                        if (string.IsNullOrEmpty(path))
                        {
                            Editor.DisableControlEnabling = false;
                            return;
                        }
                        Editor.DisableControlEnabling = false;
                        LoadMission(path);
                    });
                };
                _mainMenu.AddItem(menuItem);
            }

            {
                var menuItem = new NativeMenuItem("Exit to Grand Theft Auto V", "Leave the Content Creator");
                menuItem.Activated += (sender, item) =>
                {
                    if(!EntryPoint.MissionPlayer.IsMissionPlaying)
                        LeaveEditor();
                    else
                    {
                        GameFiber.StartNew(delegate
                        {
                            _mainMenu.Visible = false;
                            EntryPoint.MissionPlayer.FailMission(reason: "You canceled the mission.");

                            IsInMainMenu = false;
                            _menuPool.CloseAllMenus();
                            BigMinimap = false;
                            IsInFreecam = false;
                            IsInEditor = false;
                        });
                    }
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

            _cutsceneUi = new CutsceneUi();

            CameraClampMax = -30f;
            CameraClampMin = -85f;

            _blips = new List<Blip>();
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
        public static bool IsPlacingObjective { get; set; }
        public static List<INestedMenu> Children;
        public static int PlacedWeaponHash { get; set; }
        public static int? ObjectiveMarkerId { get; set; }
        public static float CameraClampMax { get; set; }
        public static float CameraClampMin { get; set; }
        #endregion

        #region Private Variables
        private Object _mainObject;
        private CutsceneUi _cutsceneUi;
        private float _ringRotation = 0f;
        private float _objectRotation = 0f;
        private Entity _hoveringEntity;
        private UIMenu _placementMenu;
        private INestedMenu _propertiesMenu;
        private SerializableMarker _selectedMarker;
        private List<Blip> _blips;
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

        public void ClearCurrentMission()
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

                CurrentMission.Pickups.ForEach(v =>
                {
                    if (v.GetEntity() != null && v.GetEntity().IsValid())
                    {
                        v.GetEntity().Delete();
                    }
                });

                CurrentMission.Objectives.ForEach(o =>
                {
                    var v = o as SerializableActorObjective;
                    if (v?.GetPed() != null && v.GetPed().IsValid())
                    {
                        v.GetPed().Delete();
                    }
                    var p = o as SerializableVehicleObjective;
                    if (p?.GetVehicle() != null && p.GetVehicle().IsValid())
                    {
                        p.GetVehicle().Delete();
                    }
                    var c = o as SerializablePickupObjective;
                    if (c?.GetObject() != null && c.GetObject().IsValid())
                    {
                        c.GetObject().Delete();
                    }
                });
                CurrentMission = null;
            }

            _blips.ForEach(b =>
            {
                if(b.IsValid())
                    b.Delete();
            });
        }

        public void LeaveEditor()
        {
            ClearCurrentMission();

            IsInMainMenu = false;
            _menuPool.CloseAllMenus();
            BigMinimap = false;
            IsInFreecam = false;
            IsInEditor = false;

            if(_mainObject != null && _mainObject.IsValid())
                _mainObject.Delete();

            NativeFunction.CallByHash<uint>(0x231C8F89D0539D8F, false, false);

            if (MainCamera != null)
                MainCamera.Active = false;
            Game.LocalPlayer.Character.Opacity = 1f;
            Game.LocalPlayer.Character.Position -= new Vector3(0, 0, Game.LocalPlayer.Character.HeightAboveGround);
        }

        public void CreateNewMission()
        {
            CurrentMission = new MissionData();
            CurrentMission.Weather = WeatherType.ExtraSunny;
            CurrentMission.Time = StaticData.StaticLists.TimeTranslation["Day"];
            CurrentMission.TimeLimit = null;
            CurrentMission.MaxWanted = 5;
            CurrentMission.MinWanted = 0;

            CurrentMission.Vehicles = new List<SerializableVehicle>();
            CurrentMission.Actors = new List<SerializablePed>();
            CurrentMission.Objects = new List<SerializableObject>();
            CurrentMission.Spawnpoints = new List<SerializableSpawnpoint>();
            CurrentMission.Objectives = new List<SerializableObjective>();
            CurrentMission.Pickups = new List<SerializablePickup>();
            CurrentMission.ObjectiveNames = new string[301];

            CurrentMission.Cutscenes = new List<SerializableCutscene>();
            menuDirty = true;
        }

        private bool menuDirty;

        public MissionData ReadMission(string path)
        {
            if (!File.Exists(path) && File.Exists(path + ".xml"))
                path += ".xml";
            if (!File.Exists(path))
            {
                Game.DisplayNotification("File not found.");
                return null;
            }

            var serializer = new XmlSerializer(typeof(MissionData));
            MissionData tmpMiss = null;
            using (var stream = File.OpenRead(path))
                tmpMiss = (MissionData)serializer.Deserialize(stream);
            return tmpMiss;
        }

        public void LoadMission(string path)
        {
            if (!File.Exists(path) && File.Exists(path + ".xml"))
                path += ".xml";
            if (!File.Exists(path))
            {
                Game.DisplayNotification("File not found.");
                return;
            }

            ClearCurrentMission();
            var serializer = new XmlSerializer(typeof(MissionData));
            MissionData tmpMiss = null;
            using (var stream = File.OpenRead(path))
                tmpMiss = (MissionData)serializer.Deserialize(stream);

            tmpMiss.Objectives = new List<SerializableObjective>(tmpMiss.Objectives);
            tmpMiss.Actors = new List<SerializablePed>(tmpMiss.Actors);
            tmpMiss.Cutscenes = new List<SerializableCutscene>(tmpMiss.Cutscenes);
            tmpMiss.Objects = new List<SerializableObject>(tmpMiss.Objects);
            tmpMiss.Pickups = new List<SerializablePickup>(tmpMiss.Pickups);
            tmpMiss.Spawnpoints = new List<SerializableSpawnpoint>(tmpMiss.Spawnpoints);
            tmpMiss.Vehicles = new List<SerializableVehicle>(tmpMiss.Vehicles);

            GameFiber.StartNew(delegate
            {
                foreach (var vehicle in tmpMiss.Vehicles)
                {
                    var newv = new Vehicle(Util.RequestModel(vehicle.ModelHash), vehicle.Position)
                    {
                        PrimaryColor = Color.FromArgb((int)vehicle.PrimaryColor.X, (int)vehicle.PrimaryColor.Y,
                            (int)vehicle.PrimaryColor.Z),
                        SecondaryColor = Color.FromArgb((int)vehicle.SecondaryColor.X, (int)vehicle.SecondaryColor.Y,
                            (int)vehicle.SecondaryColor.Z),
                    };

                    var blip = newv.AttachBlip();
                    blip.Color = Color.Orange;
                    blip.Scale = 0.7f;
                    _blips.Add(blip);

                    newv.Rotation = vehicle.Rotation;
                    vehicle.SetEntity(newv);
                }

                foreach (var ped in tmpMiss.Actors)
                {
                    ped.SetEntity(new Ped(Util.RequestModel(ped.ModelHash), ped.Position, ped.Rotation.Yaw)
                    {
                        BlockPermanentEvents = true,
                    });
                    var blip = ped.GetEntity().AttachBlip();
                    blip.Color = Color.Orange;
                    blip.Scale = 0.7f;
                    _blips.Add(blip);
                    if (ped.WeaponHash != 0)
                        ((Ped)ped.GetEntity()).GiveNewWeapon(ped.WeaponHash, ped.WeaponAmmo, true);

                }

                foreach (var o in tmpMiss.Objects)
                {
                    var newo = new Object(o.ModelHash, o.Position);
                    newo.Position = o.Position;
                    o.SetEntity(newo);
                }

                foreach (var spawnpoint in tmpMiss.Spawnpoints)
                {
                    spawnpoint.SetEntity(new Ped(spawnpoint.ModelHash, spawnpoint.Position, spawnpoint.Rotation.Yaw)
                    {
                        BlockPermanentEvents = true,
                    });
                    if(spawnpoint.WeaponHash != 0)
                    ((Ped)spawnpoint.GetEntity()).GiveNewWeapon(spawnpoint.WeaponHash, spawnpoint.WeaponAmmo, true);
                    var blip = spawnpoint.GetEntity().AttachBlip();
                    blip.Color = Color.White;
                    _blips.Add(blip);
                }

                foreach (var pickup in tmpMiss.Pickups)
                {
                    var tmpObject = new Rage.Object("prop_mp_repair", pickup.Position);
                    tmpObject.Rotation = pickup.Rotation;
                    tmpObject.Position = pickup.Position;
                    tmpObject.IsPositionFrozen = true;
                    pickup.SetEntity(tmpObject);
                }

                foreach (var ped in tmpMiss.Objectives.OfType<SerializableActorObjective>())
                {
                    ped.SetPed(new Ped(Util.RequestModel(ped.ModelHash), ped.Position, ped.Rotation.Yaw)
                    {
                        BlockPermanentEvents = true,
                    });
                    if (ped.WeaponHash != 0)
                        ((Ped)ped.GetPed()).GiveNewWeapon(ped.WeaponHash, ped.WeaponAmmo, true);
                    var blip = ped.GetPed().AttachBlip();
                    blip.Color = Color.Red;
                    blip.Scale = 0.7f;
                    _blips.Add(blip);
                }

                foreach (var vehicle in tmpMiss.Objectives.OfType<SerializableVehicleObjective>())
                {
                    var newv = new Vehicle(Util.RequestModel(vehicle.ModelHash), vehicle.Position)
                    {
                        PrimaryColor = Color.FromArgb((int)vehicle.PrimaryColor.X, (int)vehicle.PrimaryColor.Y,
                            (int)vehicle.PrimaryColor.Z),
                        SecondaryColor = Color.FromArgb((int)vehicle.SecondaryColor.X, (int)vehicle.SecondaryColor.Y,
                            (int)vehicle.SecondaryColor.Z),
                    };
                    newv.Rotation = vehicle.Rotation;
                    var blip = newv.AttachBlip();
                    blip.Color = Color.Red;
                    blip.Scale = 0.7f;
                    _blips.Add(blip);
                    vehicle.SetVehicle(newv);
                }

                foreach (var pickup in tmpMiss.Objectives.OfType<SerializablePickupObjective>())
                {
                    var tmpObject = new Rage.Object("prop_mp_repair", pickup.Position);

                    tmpObject.Rotation = pickup.Rotation;
                    tmpObject.Position = pickup.Position;
                    tmpObject.IsPositionFrozen = true;
                    pickup.SetObject(tmpObject);
                }
            });
            CurrentMission = tmpMiss;

            EnterFreecam();
            menuDirty = true;
        }

        public void SaveMission(MissionData mission, string path)
        {
            foreach (var ped in mission.Actors)
            {
                ped.Position = ped.GetEntity().Position;
                ped.Rotation = ped.GetEntity().Rotation;
                ped.ModelHash = ped.GetEntity().Model.Hash;
            }

            foreach (var ped in mission.Objects)
            {
                ped.Position = ped.GetEntity().Position;
                ped.Rotation = ped.GetEntity().Rotation;
                ped.ModelHash = ped.GetEntity().Model.Hash;
            }

            foreach (var ped in mission.Spawnpoints)
            {
                ped.Position = ped.GetEntity().Position;
                ped.Rotation = ped.GetEntity().Rotation;
                ped.ModelHash = ped.GetEntity().Model.Hash;
            }

            foreach (var ped in mission.Vehicles)
            {
                ped.Position = ped.GetEntity().Position;
                ped.Rotation = ped.GetEntity().Rotation;
                ped.ModelHash = ped.GetEntity().Model.Hash;

                var veh = (Vehicle)ped.GetEntity();
                ped.PrimaryColor = new Vector3(veh.PrimaryColor.R, veh.PrimaryColor.G, veh.PrimaryColor.B);
                ped.SecondaryColor = new Vector3(veh.SecondaryColor.R, veh.SecondaryColor.G, veh.SecondaryColor.B);
            }

            foreach (var ped in mission.Pickups)
            {
                ped.Position = ped.GetEntity().Position;
                ped.Rotation = ped.GetEntity().Rotation;
            }

            foreach (var ped in mission.Objectives.OfType<SerializableActorObjective>())
            {
                ped.Position = ped.GetPed().Position;
                ped.Rotation = ped.GetPed().Rotation;
                ped.ModelHash = ped.GetPed().Model.Hash;
            }

            foreach (var ped in mission.Objectives.OfType<SerializableVehicleObjective>())
            {
                var veh = ped.GetVehicle();
                ped.Position = veh.Position;
                ped.Rotation = veh.Rotation;
                ped.ModelHash = veh.Model.Hash;
                ped.PrimaryColor = new Vector3(veh.PrimaryColor.R, veh.PrimaryColor.G, veh.PrimaryColor.B);
                ped.SecondaryColor = new Vector3(veh.SecondaryColor.R, veh.SecondaryColor.G, veh.SecondaryColor.B);
            }

            foreach (var ped in mission.Objectives.OfType<SerializablePickupObjective>())
            {
                ped.Position = ped.GetObject().Position;
                ped.Rotation = ped.GetObject().Rotation;
            }

            if (!path.EndsWith(".xml"))
                path += ".xml";

            if(File.Exists(path))
                File.Delete(path);

            XmlSerializer serializer = new XmlSerializer(typeof(MissionData));
            using(var stream = File.OpenWrite(path))
                serializer.Serialize(stream, mission);
            Game.DisplayNotification("Saved mission as ~h~" + path + "~h~!");
        }

        private void DisplayMarker(Vector3 pos, Vector3 directionalVector)
        {
            if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid())
            {
                MarkerData.RepresentedBy.Position = pos + new Vector3(0f, 0f, MarkerData.RepresentationHeightOffset);
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

            if (ObjectiveMarkerId.HasValue)
            {
                Util.DrawMarker(ObjectiveMarkerId.Value, pos + new Vector3(0,0,MarkerData.HeightOffset), new Vector3(), new Vector3(1,1,1),
                        Color.FromArgb(100, Color.Yellow.R, Color.Yellow.G, Color.Yellow.B));
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
                var nestItem = new NativeMenuItem("Cutscenes");
                _missionMenu.AddItem(nestItem);
                _missionMenu.BindMenuToItem(_cutsceneUi.CutsceneMenus, nestItem);
                nestItem.Activated += (sender, item) =>
                {
                    _cutsceneUi.Enter();
                };
            }

            {
                var item = new NativeMenuItem("Save Mission");
                _missionMenu.AddItem(item);
                item.Activated += (sender, selectedItem) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        DisableControlEnabling = true;
                        string path = Util.GetUserInput();
                        if (string.IsNullOrEmpty(path))
                        {
                            DisableControlEnabling = false;
                            return;
                        }
                        DisableControlEnabling = false;
                        SaveMission(CurrentMission, path);
                    });
                };
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

        public enum EntityType
        {
            None,
            NormalVehicle,
            NormalActor,
            NormalObject,
            NormalPickup,
            ObjectiveVehicle,
            ObjectiveActor,
            ObjectiveMarker,
            ObjectivePickup,
            ObjectiveTimer,
            Spawnpoint
        }

        public static EntityType GetEntityType(Entity ent)
        {
            if (ent.IsPed())
            {
                if(CurrentMission.Actors.Any(o => o.GetEntity().Handle.Value == ent.Handle.Value))
                    return EntityType.NormalActor;
                if(CurrentMission.Spawnpoints.Any(o => o.GetEntity().Handle.Value == ent.Handle.Value))
                    return EntityType.Spawnpoint;
                if (CurrentMission.Objectives.Any(o =>
                {
                    var act = o as SerializableActorObjective;
                    return act?.GetPed().Handle.Value == ent.Handle.Value;
                })) 
                    return EntityType.ObjectiveActor;
            }
            else if (ent.IsVehicle())
            {
                if(CurrentMission.Vehicles.Any(o => o.GetEntity().Handle.Value == ent.Handle.Value))
                    return EntityType.NormalVehicle;
                if (CurrentMission.Objectives.Any(o =>
                {
                    var act = o as SerializableVehicleObjective;
                    return act?.GetVehicle().Handle.Value == ent.Handle.Value;
                }))
                    return EntityType.ObjectiveVehicle;
            }
            else if (ent.IsObject())
            {
                if(CurrentMission.Objects.Any(o => o.GetEntity().Handle.Value == ent.Handle.Value))
                    return EntityType.NormalObject;
                if(CurrentMission.Pickups.Any(o => o.GetEntity().Handle.Value == ent.Handle.Value))
                    return EntityType.NormalPickup;
                if(CurrentMission.Objectives.Any(o =>
                {
                    var d = o as SerializablePickupObjective;
                    return d?.GetObject().Handle.Value == ent.Handle.Value;
                }))
                    return EntityType.ObjectivePickup;
            }
            return EntityType.None;
        }

        private void CheckForIntersection(Entity ent)
        {
            if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid() && ent != null && ent.IsValid())
            {
                var type = GetEntityType(ent);
                if (MarkerData.RepresentedBy is Vehicle && type == EntityType.NormalVehicle && !IsPlacingObjective)
                {
                    MarkerData.RepresentedBy.Opacity = 0f;
                    MarkerData.HeadingOffset = 45f;
                    RingData.Color = Color.Red;
                    _hoveringEntity = ent;
                }

                if (MarkerData.RepresentedBy is Vehicle && IsPlacingObjective && type == EntityType.ObjectiveVehicle)
                {
                    MarkerData.RepresentedBy.Opacity = 0f;
                    MarkerData.HeadingOffset = 45f;
                    RingData.Color = Color.Red;
                    _hoveringEntity = ent;
                }

                else if (MarkerData.RepresentedBy is Ped && ent.IsPed() && !PlayerSpawnOpen && !IsPlacingObjective && type == EntityType.NormalActor)
                {
                    MarkerData.RepresentedBy.Opacity = 0f;
                    MarkerData.HeadingOffset = 45f;
                    RingData.Color = Color.Red;
                    _hoveringEntity = ent;
                }

                else if (MarkerData.RepresentedBy is Ped && ent.IsPed() && PlayerSpawnOpen && type == EntityType.Spawnpoint)
                {
                    MarkerData.RepresentedBy.Opacity = 0f;
                    MarkerData.HeadingOffset = 45f;
                    RingData.Color = Color.Red;
                    _hoveringEntity = ent;
                }

                else if (MarkerData.RepresentedBy is Ped && ent.IsPed() && IsPlacingObjective && type == EntityType.ObjectiveActor)
                {
                    MarkerData.RepresentedBy.Opacity = 0f;
                    MarkerData.HeadingOffset = 45f;
                    RingData.Color = Color.Red;
                    _hoveringEntity = ent;
                }

                else if (MarkerData.RepresentedBy is Ped && ent.IsVehicle() &&
                    type == EntityType.NormalVehicle &&
                    ((Vehicle)ent).GetFreeSeatIndex().HasValue)
                {
                    RingData.Color = Color.GreenYellow;
                    _hoveringEntity = ent;
                }

                else if (MarkerData.RepresentedBy is Object && ent.IsObject() &&
                    type == EntityType.NormalObject && PlacedWeaponHash == 0)
                {
                    MarkerData.RepresentedBy.Opacity = 0f;
                    MarkerData.HeadingOffset = 45f;
                    RingData.Color = Color.Red;
                    _hoveringEntity = ent;
                }
            }
            else if (_hoveringEntity != null && _hoveringEntity.IsValid() && MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid() && PlacedWeaponHash == 0)
            {
                MarkerData.RepresentedBy.Opacity = 1f;
                MarkerData.HeadingOffset = 0f;
                RingData.Color = Color.MediumPurple;
                _hoveringEntity = null;
            }
        }

        private void CheckForPickup(Vector3 pos, bool entNull)
        {
            if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid() && (PlacedWeaponHash != 0 || ObjectiveMarkerId.HasValue))
            {
                var threshold = 1.5f;
                if (!IsPlacingObjective && !ObjectiveMarkerId.HasValue)
                foreach (SerializablePickup pickup in CurrentMission.Pickups)
                {
                    if(pickup.GetEntity() == null || !pickup.GetEntity().IsValid()) continue;
                    if ((pickup.GetEntity().Position - pos).Length() > threshold) continue;
                    MarkerData.RepresentedBy.Opacity = 0f;
                    MarkerData.HeadingOffset = 45f;
                    RingData.Color = Color.Red;
                    _hoveringEntity = pickup.GetEntity();
                    return;
                }
                if(IsPlacingObjective && !ObjectiveMarkerId.HasValue)
                foreach (SerializablePickupObjective pickup in CurrentMission.Objectives.Where(obj => obj is SerializablePickupObjective))
                {
                    if (pickup.GetObject() == null || !pickup.GetObject().IsValid()) continue;
                    if ((pickup.GetObject().Position - pos).Length() > threshold) continue;
                    MarkerData.RepresentedBy.Opacity = 0f;
                    MarkerData.HeadingOffset = 45f;
                    RingData.Color = Color.Red;
                    _hoveringEntity = pickup.GetObject();
                    return;
                }

                
                if (IsPlacingObjective && ObjectiveMarkerId.HasValue)
                    foreach (var mark in CurrentMission.Objectives.OfType<SerializableMarker>())
                {
                    
                    if ((mark.Position - pos).Length() > threshold) continue;
                    MarkerData.RepresentedBy.Opacity = 0f;
                    MarkerData.HeadingOffset = 45f;
                    RingData.Color = Color.Red;
                    _selectedMarker = mark;
                    return;
                }


                MarkerData.RepresentedBy.Opacity = 1f;
                MarkerData.HeadingOffset = 0f;
                RingData.Color = Color.MediumPurple;
                _hoveringEntity = null;
                _selectedMarker = null;
            }
            else if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid() && entNull)
            {
                MarkerData.RepresentedBy.Opacity = 1f;
                MarkerData.HeadingOffset = 0f;
                RingData.Color = Color.MediumPurple;
                _hoveringEntity = null;
                _selectedMarker = null;
            }
        }

        private void CheckForProperty(Entity ent)
        {
            if (ent != null && ent.IsValid())
            {
                var type = GetEntityType(ent);
                if (type != EntityType.None)
                {
                    RingData.Color = Color.Yellow;
                    _hoveringEntity = ent;
                }
            }
            else if (_hoveringEntity != null && _hoveringEntity.IsValid())
            {
                var type = GetEntityType(_hoveringEntity);
                if (type == EntityType.NormalPickup || type == EntityType.ObjectivePickup || type == EntityType.ObjectiveMarker) return;
                RingData.Color = Color.Gray;
                _hoveringEntity = null;
            }
        }

        private void CheckForPickupProperty(Vector3 pos, bool entNull)
        {
            var threshold = 1.5f;
            foreach (SerializablePickup pickup in CurrentMission.Pickups)
            {
                if (pickup.GetEntity() == null || !pickup.GetEntity().IsValid()) continue;
                if ((pickup.GetEntity().Position - pos).Length() > threshold) continue;
                RingData.Color = Color.Yellow;
                _hoveringEntity = pickup.GetEntity();
                return;
            }

            foreach (SerializablePickupObjective objective in CurrentMission.Objectives.OfType<SerializablePickupObjective>())
            {
                if (objective.GetObject() == null || !objective.GetObject().IsValid()) continue;
                if ((objective.GetObject().Position - pos).Length() > threshold) continue;
                RingData.Color = Color.Yellow;
                _hoveringEntity = objective.GetObject();
                return;
            }

            foreach (var mark in CurrentMission.Objectives.OfType<SerializableMarker>())
            {
                if ((mark.Position - pos).Length() > threshold) continue;
                RingData.Color = Color.Yellow;
                _selectedMarker = mark;
                return;
            }

            if (entNull)
            {
                RingData.Color = Color.Gray;
                _hoveringEntity = null;
                _selectedMarker = null;
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
            NativeFunction.CallByName<uint>("ENABLE_CONTROL_ACTION", 0, (int)GameControl.HUDSpecial);
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

        public void CloseAllMenus()
        {
            _menuPool.CloseAllMenus();
            CloseAllMenusRecursively(Children);
        }

        private void CloseAllMenusRecursively(List<INestedMenu> menus)
        {
            foreach (var menu in menus)
            {
                if(menu == null) return;
                CloseAllMenusRecursively(menu.Children.Select(m => m as INestedMenu).ToList());
                ((UIMenu) menu).Visible = false;
            }
        }

        private void CheckMenusForAlerts()
        {
            foreach (var pair in _missionMenu.Children)
            {
                if(pair.Value.MenuItems.Any(m => m.RightBadge == NativeMenuItem.BadgeStyle.Alert))
                    pair.Key?.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                else
                    pair.Key?.SetRightBadge(NativeMenuItem.BadgeStyle.None);
            }
        }

        public SerializablePed CreatePed(Model model, Vector3 pos, float heading)
        {
            var tmpPed = new Ped(model, pos, heading);
            tmpPed.IsPositionFrozen = false;
            tmpPed.BlockPermanentEvents = true;

            var blip = tmpPed.AttachBlip();
            blip.Color = Color.Orange;
            blip.Scale = 0.7f;
            _blips.Add(blip);

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
            tmpObj.Accuracy = 50;
            tmpObj.SpawnInVehicle = false;
            CurrentMission.Actors.Add(tmpObj);
            return tmpObj;
        }

        public SerializableActorObjective CreatePedObjective(Model model, Vector3 pos, float heading)
        {
            var tmpPed = new Ped(model, pos, heading);
            tmpPed.IsPositionFrozen = false;
            tmpPed.BlockPermanentEvents = true;

            var blip = tmpPed.AttachBlip();
            blip.Color = Color.Red;
            blip.Scale = 0.7f;
            _blips.Add(blip);

            var tmpObj = new SerializableActorObjective();
            tmpObj.SetPed(tmpPed);
            tmpObj.SpawnAfter = 0;
            tmpObj.ActivateAfter = 0;
            tmpObj.Behaviour = 2;
            tmpObj.RelationshipGroup = 5;
            tmpObj.WeaponAmmo = 9999;
            tmpObj.WeaponHash = 0;
            tmpObj.Health = 200;
            tmpObj.Accuracy = 50;
            tmpObj.Armor = 0;
            tmpObj.SpawnInVehicle = false;
            CurrentMission.Objectives.Add(tmpObj);
            return tmpObj;
        }

        public SerializableVehicleObjective CreateVehicleObjective(Model model, Vector3 pos, Rotator rotation, Color primColor, Color seconColor)
        {
            var tmpVeh = new Vehicle(model, pos)
            {
                PrimaryColor = primColor,
                SecondaryColor = seconColor,
            };
            var blip = tmpVeh.AttachBlip();
            blip.Color = Color.Red;
            blip.Scale = 0.7f;
            _blips.Add(blip);

            tmpVeh.IsPositionFrozen = false;
            tmpVeh.Rotation = rotation;
            var tmpObj = new SerializableVehicleObjective();
            tmpObj.SetVehicle(tmpVeh);
            tmpObj.SpawnAfter = 0;
            tmpObj.ActivateAfter = 0;
            tmpObj.Health = 1000;
            CurrentMission.Objectives.Add(tmpObj);
            return tmpObj;
        }

        public SerializableSpawnpoint CreateSpawnpoint(Model model, Vector3 pos, float heading)
        {
            var tmpPed = new Ped(model, pos, heading);
            tmpPed.IsPositionFrozen = true;
            tmpPed.BlockPermanentEvents = true;

            var blip = tmpPed.AttachBlip();
            blip.Color = Color.White;
            _blips.Add(blip);

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

            var blip = tmpVeh.AttachBlip();
            blip.Color = Color.Orange;
            blip.Scale = 0.7f;
            _blips.Add(blip);

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
            var tmpObj = new SerializableObject();
            tmpObj.SetEntity(tmpObject);
            tmpObj.SpawnAfter = 0;
            tmpObj.RemoveAfter = 0;
            CurrentMission.Objects.Add(tmpObj);
            return tmpObj;
        }

        public SerializableObject CreatePickup(int weaponHash, Vector3 pos, Rotator rot)
        {
            var tmpObject = new Rage.Object(new Model("prop_mp_repair"), pos);
            tmpObject.Rotation = rot;
            tmpObject.Position = pos;
            tmpObject.IsPositionFrozen = true;

            var tmpObj = new SerializablePickup();
            tmpObj.SetEntity(tmpObject);
            tmpObj.SpawnAfter = 0;
            tmpObj.RemoveAfter = 0;
            tmpObj.Respawn = false;
            tmpObj.Ammo = 9999;
            tmpObj.PickupHash = weaponHash;
            CurrentMission.Pickups.Add(tmpObj);
            return tmpObj;
        }

        public SerializablePickupObjective CreatePickupObjective(int weaponHash, Vector3 pos, Rotator rot)
        {
            var tmpObject = new Rage.Object(new Model("prop_mp_repair"), pos);
            tmpObject.Rotation = rot;
            tmpObject.Position = pos;
            tmpObject.IsPositionFrozen = true;
            var tmpObj = new SerializablePickupObjective();
            tmpObj.SetObject(tmpObject);
            tmpObj.SpawnAfter = 0;
            tmpObj.ActivateAfter = 0;
            tmpObj.Respawn = false;
            tmpObj.Ammo = 9999;
            tmpObj.PickupHash = weaponHash;
            CurrentMission.Objectives.Add(tmpObj);
            return tmpObj;
        }

        public SerializableMarker CreateCheckpoint(Vector3 pos, Color col, Vector3 scale)
        {
            var tmpObj = new SerializableMarker();
            tmpObj.SpawnAfter = 0;
            tmpObj.ActivateAfter = 0;
            tmpObj.Alpha = col.A;
            tmpObj.Color = new Vector3(col.R, col.G, col.B);
            tmpObj.Scale = scale;
            tmpObj.Position = pos;
            tmpObj.Type = ObjectiveMarkerId.Value;
            CurrentMission.Objectives.Add(tmpObj);
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
                CheckMenusForAlerts();
                _menuPool.ProcessMenus();
                Children.ForEach(x => x.Process());
                if (_propertiesMenu != null)
                {
                    _propertiesMenu.Process();
                    ((UIMenu)_propertiesMenu).ProcessControl();
                    ((UIMenu)_propertiesMenu).ProcessMouse();
                    ((UIMenu)_propertiesMenu).Draw();
                }
                _cutsceneUi.Process();
            }
            if (IsInMainMenu)
            {
                NativeFunction.CallByName<uint>("HIDE_HUD_AND_RADAR_THIS_FRAME");
            }

            if (!IsInEditor) return;
            NativeFunction.CallByName<uint>("DISABLE_CONTROL_ACTION", 0, (int)GameControl.FrontendPauseAlternate);


            
            foreach (var objective in CurrentMission.Objectives)
            {
                //TODO: Get model size
                if (objective is SerializableActorObjective)
                {
                    var ped = ((SerializableActorObjective) objective).GetPed();
                    if (ped == null) continue;
                    Util.DrawMarker(0, ped.Position + new Vector3(0, 0, 2f),
                        new Vector3(ped.Rotation.Pitch, ped.Rotation.Roll, ped.Rotation.Yaw),
                        new Vector3(1f, 1f, 1f), Color.FromArgb(100, 255, 10, 10) );
                }
                else if (objective is SerializableVehicleObjective)
                {
                    var ped = ((SerializableVehicleObjective)objective).GetVehicle();
                    if (ped == null) continue;
                    Util.DrawMarker(0, ped.Position + new Vector3(0, 0, 2f),
                        new Vector3(ped.Rotation.Pitch, ped.Rotation.Roll, ped.Rotation.Yaw),
                        new Vector3(1f, 1f, 1f), Color.FromArgb(100, 255, 10, 10));
                }
                
                else if (objective is SerializablePickupObjective)
                {
                    var pickup = ((SerializablePickupObjective) objective).GetObject();
                    if (pickup == null) continue;
                    Util.DrawMarker(1, pickup.Position - new Vector3(0, 0, 1f),
                    new Vector3(pickup.Rotation.Pitch, pickup.Rotation.Roll, pickup.Rotation.Yaw),
                    new Vector3(1f, 1f, 1f), Color.FromArgb(100, 255, 10, 10));
                }

                else if (objective is SerializableMarker)
                {
                    var obj = ((SerializableMarker) objective);
                    Util.DrawMarker(obj.Type, obj.Position, new Vector3(), obj.Scale,
                        Color.FromArgb(obj.Alpha, (int)obj.Color.X, (int)obj.Color.Y, (int)obj.Color.Z));
                }
            }
            foreach (var pickup in CurrentMission.Pickups)
            {
                if(pickup.GetEntity() == null) continue;
                Util.DrawMarker(1, pickup.GetEntity().Position - new Vector3(0,0,1f),
                    new Vector3(pickup.GetEntity().Rotation.Pitch, pickup.GetEntity().Rotation.Roll, pickup.GetEntity().Rotation.Yaw),
                    new Vector3(1f, 1f, 1f), Color.FromArgb(100, 10, 100, 255));
            }

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



                MainCamera.Rotation = new Rotator((MainCamera.Rotation.Pitch + mouseY).Clamp(CameraClampMin, CameraClampMax), 0f,
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
                var head = MainCamera.Rotation.Yaw;
                if (head < 0f)
                    head += 360f;
                Game.LocalPlayer.Character.Heading = head;

                #endregion

                var ent = Util.RaycastEntity(new Vector2(0, 0),
                    MainCamera.Position,
                    new Vector3(MainCamera.Rotation.Pitch, MainCamera.Rotation.Roll, MainCamera.Rotation.Yaw)
                    , null);

                if (MarkerData.RepresentedBy == null || !MarkerData.RepresentedBy.IsValid())
                {
                    CheckForProperty(ent);
                    CheckForPickupProperty(markerPos, ent==null);
                }
                else
                {
                    
                    CheckForIntersection(ent);
                    CheckForPickup(markerPos, ent == null);
                }
                
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
                    CheckForProperty(ent);

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

            NativeFunction.CallByHash<uint>(0x231C8F89D0539D8F, BigMinimap, false);
            Game.SetRadarZoomLevelThisFrame(Game.IsControlPressed(0, GameControl.HUDSpecial) ? 300 : 100);

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
                if (MarkerData.RepresentedBy is Vehicle && !IsPlacingObjective)
                {
                    CreateVehicle(MarkerData.RepresentedBy.Model, MarkerData.RepresentedBy.Position,
                                    MarkerData.RepresentedBy.Rotation,
                                    ((Vehicle)MarkerData.RepresentedBy).PrimaryColor,
                                    ((Vehicle)MarkerData.RepresentedBy).SecondaryColor);
                }

                else if (MarkerData.RepresentedBy is Vehicle && IsPlacingObjective)
                {
                    CreateVehicleObjective(MarkerData.RepresentedBy.Model, MarkerData.RepresentedBy.Position,
                                    MarkerData.RepresentedBy.Rotation,
                                    ((Vehicle)MarkerData.RepresentedBy).PrimaryColor,
                                    ((Vehicle)MarkerData.RepresentedBy).SecondaryColor);
                }

                else if (MarkerData.RepresentedBy is Ped && !PlayerSpawnOpen && !IsPlacingObjective)
                {
                    CreatePed(MarkerData.RepresentedBy.Model, MarkerData.RepresentedBy.Position - new Vector3(0, 0, 1f), MarkerData.RepresentedBy.Heading);
                }

                else if (MarkerData.RepresentedBy is Ped && PlayerSpawnOpen && !IsPlacingObjective)
                {
                    CreateSpawnpoint(MarkerData.RepresentedBy.Model, MarkerData.RepresentedBy.Position - new Vector3(0, 0, 1f), MarkerData.RepresentedBy.Heading);
                }

                else if (MarkerData.RepresentedBy is Ped && !PlayerSpawnOpen && IsPlacingObjective)
                {
                    CreatePedObjective(MarkerData.RepresentedBy.Model, MarkerData.RepresentedBy.Position - new Vector3(0, 0, 1f), MarkerData.RepresentedBy.Heading);
                }

                else if (MarkerData.RepresentedBy is Object && PlacedWeaponHash == 0 && !IsPlacingObjective && !ObjectiveMarkerId.HasValue)
                {
                    CreateObject(MarkerData.RepresentedBy.Model, MarkerData.RepresentedBy.Position,
                        MarkerData.RepresentedBy.Rotation);
                }

                else if (MarkerData.RepresentedBy is Object && PlacedWeaponHash != 0 && !IsPlacingObjective && !ObjectiveMarkerId.HasValue)
                {
                    CreatePickup(PlacedWeaponHash, MarkerData.RepresentedBy.Position,
                        MarkerData.RepresentedBy.Rotation);
                }

                else if (MarkerData.RepresentedBy is Object && PlacedWeaponHash != 0 && IsPlacingObjective && !ObjectiveMarkerId.HasValue)
                {
                    CreatePickupObjective(PlacedWeaponHash, MarkerData.RepresentedBy.Position,
                        MarkerData.RepresentedBy.Rotation);
                }

                else if (MarkerData.RepresentedBy is Object && PlacedWeaponHash == 0 && IsPlacingObjective && ObjectiveMarkerId.HasValue && _selectedMarker == null)
                {
                    CreateCheckpoint(MarkerData.RepresentedBy.Position + new Vector3(0,0,MarkerData.HeightOffset),
                        Color.FromArgb(100, Color.Yellow.R, Color.Yellow.G, Color.Yellow.B), new Vector3(1, 1, 1));
                }
            }
            else if (_hoveringEntity != null && _hoveringEntity.IsValid() &&
                     Game.IsControlJustPressed(0, GameControl.Attack) && 
                     (MarkerData.RepresentedBy == null || !MarkerData.RepresentedBy.IsValid()) &&
                     _propertiesMenu == null)
            {
                var type = GetEntityType(_hoveringEntity);
                // TODO: Properties
                if (type == EntityType.NormalActor)
                {
                    CloseAllMenus();

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
                else if (type == EntityType.Spawnpoint)
                {
                    CloseAllMenus();

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
                else if (type == EntityType.ObjectiveActor)
                {
                    CloseAllMenus();

                    DisableControlEnabling = true;
                    EnableBasicMenuControls = true;
                    var newMenu = new ActorObjectivePropertiesMenu();
                    var actor = CurrentMission.Objectives.FirstOrDefault(o =>
                    {
                        var act = o as SerializableActorObjective;
                        return act?.GetPed().Handle.Value == _hoveringEntity.Handle.Value;
                    });
                    newMenu.BuildFor((SerializableActorObjective)actor);

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
                else if(type == EntityType.NormalVehicle)
                {
                    CloseAllMenus();

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
                else if (type == EntityType.ObjectiveVehicle)
                {
                    CloseAllMenus();

                    DisableControlEnabling = true;
                    EnableBasicMenuControls = true;
                    var newMenu = new VehicleObjectivePropertiesMenu();
                    var actor = CurrentMission.Objectives.OfType<SerializableVehicleObjective>()
                        .FirstOrDefault(o => o.GetVehicle().Handle.Value == _hoveringEntity.Handle.Value);
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
                else if (type == EntityType.NormalObject)
                {
                    CloseAllMenus();

                    DisableControlEnabling = true;
                    EnableBasicMenuControls = true;
                    var newMenu = new ObjectPropertiesMenu();
                    var actor = CurrentMission.Objects.FirstOrDefault(o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value);
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
                else if (type == EntityType.NormalPickup)
                {
                    CloseAllMenus();

                    DisableControlEnabling = true;
                    EnableBasicMenuControls = true;
                    var newMenu = new PickupPropertiesMenu();
                    var actor = CurrentMission.Pickups.FirstOrDefault(o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value);
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
                else if (type == EntityType.ObjectivePickup)
                {
                    CloseAllMenus();

                    DisableControlEnabling = true;
                    EnableBasicMenuControls = true;
                    var newMenu = new PickupObjectivePropertiesMenu();
                    var actor = CurrentMission.Objectives.OfType<SerializablePickupObjective>()
                        .FirstOrDefault(o => o.GetObject().Handle.Value == _hoveringEntity.Handle.Value);
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
            }
            else if (_selectedMarker != null &&
                     Game.IsControlJustPressed(0, GameControl.Attack) &&
                     (MarkerData.RepresentedBy == null || !MarkerData.RepresentedBy.IsValid()) &&
                     _propertiesMenu == null)
            {
                CloseAllMenus();

                DisableControlEnabling = true;
                EnableBasicMenuControls = true;
                var newMenu = new MarkerPropertiesMenu();
                newMenu.BuildFor(_selectedMarker);

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
            else if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid() &&
                     _hoveringEntity != null && _hoveringEntity.IsValid() &&
                     Game.IsControlJustPressed(0, GameControl.CellphoneSelect))
            {
                var type = GetEntityType(_hoveringEntity);
                if (_hoveringEntity.IsVehicle() && MarkerData.RepresentedBy.IsVehicle())
                {
                    foreach (var ped in ((Vehicle) _hoveringEntity).Occupants)
                    {
                        var pedType = GetEntityType(ped);
                        if (pedType == EntityType.NormalActor)
                        {
                            CurrentMission.Actors.First(o => o.GetEntity().Handle.Value == ped.Handle.Value)
                                .SpawnInVehicle = false;
                        }
                        else if (pedType == EntityType.Spawnpoint)
                        {
                            CurrentMission.Spawnpoints.First(o => o.GetEntity().Handle.Value == ped.Handle.Value)
                                .SpawnInVehicle = false;
                        }
                        else if (pedType == EntityType.ObjectiveActor)
                        {
                            ((SerializableActorObjective) CurrentMission.Objectives.First(o =>
                            {
                                var p = o as SerializableActorObjective;
                                return p?.GetPed().Handle.Value == ped.Handle.Value;
                            })).SpawnInVehicle = false;
                        }
                    }

                    if (type == EntityType.NormalVehicle)
                    {
                        CurrentMission.Vehicles.Remove(
                            CurrentMission.Vehicles.FirstOrDefault(
                                o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value));
                    }
                    else if (type == EntityType.ObjectiveVehicle)
                    {
                        var myVeh = ((SerializableVehicleObjective) CurrentMission.Objectives.First(o =>
                        {
                            var p = o as SerializableVehicleObjective;
                            return p?.GetVehicle().Handle.Value == _hoveringEntity.Handle.Value;
                        }));
                        CurrentMission.Objectives.Remove(myVeh);
                    }
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
                        ((Ped) newPed.GetEntity()).WarpIntoVehicle((Vehicle) _hoveringEntity, possibleSeat.Value);
                        newPed.SpawnInVehicle = true;
                        newPed.VehicleSeat = possibleSeat.Value;
                    }
                }
                else if (MarkerData.RepresentedBy.IsPed() && type == EntityType.NormalActor)
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
                else if (MarkerData.RepresentedBy.IsPed() && type == EntityType.Spawnpoint)
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
                else if (_hoveringEntity.IsPed() && MarkerData.RepresentedBy.IsPed() && type == EntityType.ObjectiveActor)
                {
                    CurrentMission.Objectives.Remove(
                        CurrentMission.Objectives.FirstOrDefault(m =>
                        {
                            var o = m as SerializableActorObjective;
                            return o?.GetPed().Handle.Value == _hoveringEntity.Handle.Value;
                        }));

                    _hoveringEntity.Delete();
                    if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid())
                        MarkerData.RepresentedBy.Opacity = 1f;
                    MarkerData.HeadingOffset = 0f;
                    RingData.Color = Color.MediumPurple;
                    _hoveringEntity = null;
                }
                else if (_hoveringEntity.IsObject() && MarkerData.RepresentedBy.IsObject() && PlacedWeaponHash == 0 &&
                         !IsPlacingObjective)
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
                else if (_hoveringEntity.IsObject() && MarkerData.RepresentedBy.IsObject() &&
                         PlacedWeaponHash != 0 && !IsPlacingObjective)
                {
                    CurrentMission.Pickups.Remove(
                        CurrentMission.Pickups.FirstOrDefault(
                            o => o.GetEntity().Handle.Value == _hoveringEntity.Handle.Value));

                    _hoveringEntity.Delete();
                    if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid())
                        MarkerData.RepresentedBy.Opacity = 1f;
                    MarkerData.HeadingOffset = 0f;
                    RingData.Color = Color.MediumPurple;
                    _hoveringEntity = null;
                }
                else if (_hoveringEntity.IsObject() && MarkerData.RepresentedBy.IsObject() &&
                         PlacedWeaponHash != 0 && IsPlacingObjective)
                {
                    CurrentMission.Objectives.Remove(
                        CurrentMission.Objectives.FirstOrDefault(o =>
                        {
                            var d = o as SerializablePickupObjective;
                            return d?.GetObject().Handle.Value == _hoveringEntity.Handle.Value;
                        }));

                    _hoveringEntity.Delete();
                    if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid())
                        MarkerData.RepresentedBy.Opacity = 1f;
                    MarkerData.HeadingOffset = 0f;
                    RingData.Color = Color.MediumPurple;
                    _hoveringEntity = null;
                }
            }
            if (MarkerData.RepresentedBy != null && MarkerData.RepresentedBy.IsValid() &&
                     _selectedMarker != null && Game.IsControlJustPressed(0, GameControl.CellphoneSelect))
            {
                CurrentMission.Objectives.Remove(_selectedMarker);
                _selectedMarker = null;
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
