using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ContentCreator.SerializableData;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace ContentCreator.Editor.NestedMenus
{
    public class PlacementMenu : UIMenu, INestedMenu
    {
        public PlacementMenu(MissionData data) : base("Content Creator", "PLACEMENT")
        {
            MouseEdgeEnabled = false;
            MouseControlsEnabled = false;
            Children = new List<UIMenu>();
            Display(data);
            ResetKey(Common.MenuControls.Up);
            ResetKey(Common.MenuControls.Down);
            SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
            SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);
        }

        public List<UIMenu> Children { get; set; }

        public void Display(MissionData data)
        {
            Clear();

            #region Player
            {
                var item = new NativeMenuItem("Player");

                var dict = StaticData.PedData.Database.ToDictionary(k => k.Key, k => k.Value.Select(x => x.Item1).ToArray());
                var menu = new CategorySelectionMenu(dict, "Player Skin", "PLACE SPAWNPOINT");
                menu.Build("Gangsters");
                Children.Add(menu);
                AddItem(item);
                BindMenuToItem(menu, item);

                item.Activated += (men, itm) =>
                {
                    Editor.PlayerSpawnOpen = true;
                    Editor.RingData.Color = Color.MediumPurple;
                    Editor.RingData.Type = RingType.HorizontalSplitArrowCircle;
                    Editor.MarkerData.Display = true;
                    Editor.MarkerData.MarkerType = "prop_mp_placement";
                    GameFiber.StartNew(delegate
                    {
                        var hash =
                            StaticData.PedData.Database[menu.CurrentSelectedCategory].First(
                                tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;
                        var veh = new Ped(Util.RequestModel(hash), Game.LocalPlayer.Character.Position, 0f);

                        veh.IsPositionFrozen = true;
                        Editor.MarkerData.RepresentedBy = veh;
                        NativeFunction.CallByName<uint>("SET_ENTITY_COLLISION", veh.Handle.Value, false, 0);
                    });
                };

                menu.OnMenuClose += (men) =>
                {
                    if (Editor.MarkerData.RepresentedBy != null && Editor.MarkerData.RepresentedBy.IsValid())
                        Editor.MarkerData.RepresentedBy.Delete();

                    Editor.MarkerData.RepresentedBy = null;
                    Editor.MarkerData.MarkerType = null;
                    Editor.MarkerData.Display = false;
                    Editor.MarkerData.HeightOffset = 0f;
                    Editor.RingData.HeightOffset = 0f;
                    Editor.RingData.Color = Color.Gray;
                    Editor.RingData.Type = RingType.HorizontalCircleSkinny;
                    Editor.PlayerSpawnOpen = false;
                };

                menu.SelectionChanged += (sender, eventargs) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        var heading = 0f;
                        if (Editor.MarkerData.RepresentedBy != null && Editor.MarkerData.RepresentedBy.IsValid())
                        {
                            heading = Editor.MarkerData.RepresentedBy.Heading;
                            Editor.MarkerData.RepresentedBy.Delete();
                        }

                        var hash =
                            StaticData.PedData.Database[menu.CurrentSelectedCategory].First(
                                tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;
                        var veh = new Ped(Util.RequestModel(hash), Game.LocalPlayer.Character.Position, 0f);
                        veh.Heading = heading;
                        veh.IsPositionFrozen = true;
                        Editor.MarkerData.RepresentedBy = veh;
                        NativeFunction.CallByName<uint>("SET_ENTITY_COLLISION", veh.Handle.Value, false, 0);

                        //var dims = Util.GetModelDimensions(veh.Model);
                        //Editor.RingData.HeightOffset = 1f;
                        //Editor.MarkerData.HeightOffset = 1f;
                    });
                };

            }
            #endregion

            #region Objectives
            {
                var item = new NativeMenuItem("Objectives");
                var newMenu = new ObjectivePlacementMenu(data);
                Editor.Children.Add(newMenu);
                BindMenuToItem(newMenu, item);
                AddItem(item);
            }
            #endregion

            #region Actors
            {
                var item = new NativeMenuItem("Actors");

                var dict = StaticData.PedData.Database.ToDictionary(k => k.Key, k => k.Value.Select(x => x.Item1).ToArray());
                var menu = new CategorySelectionMenu(dict, "Actor");
                menu.Build("Gangsters");
                Children.Add(menu);
                AddItem(item);
                BindMenuToItem(menu, item);

                item.Activated += (men, itm) =>
                {
                    Editor.RingData.Color = Color.MediumPurple;
                    Editor.RingData.Type = RingType.HorizontalSplitArrowCircle;
                    Editor.MarkerData.Display = true;
                    Editor.MarkerData.MarkerType = "prop_mp_placement";
                    GameFiber.StartNew(delegate
                    {
                        var hash =
                            StaticData.PedData.Database[menu.CurrentSelectedCategory].First(
                                tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;
                        var veh = new Ped(Util.RequestModel(hash), Game.LocalPlayer.Character.Position, 0f);

                        veh.IsPositionFrozen = true;
                        Editor.MarkerData.RepresentedBy = veh;
                        NativeFunction.CallByName<uint>("SET_ENTITY_COLLISION", veh.Handle.Value, false, 0);
                    });
                };

                menu.OnMenuClose += (men) =>
                {
                    if (Editor.MarkerData.RepresentedBy != null && Editor.MarkerData.RepresentedBy.IsValid())
                        Editor.MarkerData.RepresentedBy.Delete();

                    Editor.MarkerData.RepresentedBy = null;
                    Editor.MarkerData.MarkerType = null;
                    Editor.MarkerData.Display = false;
                    Editor.MarkerData.HeightOffset = 0f;
                    Editor.RingData.HeightOffset = 0f;
                    Editor.RingData.Color = Color.Gray;
                    Editor.RingData.Type = RingType.HorizontalCircleSkinny;
                };

                menu.SelectionChanged += (sender, eventargs) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        var heading = 0f;
                        if (Editor.MarkerData.RepresentedBy != null && Editor.MarkerData.RepresentedBy.IsValid())
                        {
                            heading = Editor.MarkerData.RepresentedBy.Heading;
                            Editor.MarkerData.RepresentedBy.Delete();
                        }
                        
                        var hash =
                            StaticData.PedData.Database[menu.CurrentSelectedCategory].First(
                                tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;
                        var veh = new Ped(Util.RequestModel(hash), Game.LocalPlayer.Character.Position, 0f);
                        veh.Heading = heading;
                        veh.IsPositionFrozen = true;
                        Editor.MarkerData.RepresentedBy = veh;
                        NativeFunction.CallByName<uint>("SET_ENTITY_COLLISION", veh.Handle.Value, false, 0);
                        //var dims = Util.GetModelDimensions(veh.Model);
                        //Editor.RingData.HeightOffset = 1f;
                        //Editor.MarkerData.HeightOffset = 1f;
                    });
                };

            }

            #endregion

            #region Cars
            {
                var item = new NativeMenuItem("Cars");
                var dict = StaticData.VehicleData.Database.ToDictionary(k => k.Key, k => k.Value.Select(x => x.Item1).ToArray());
                var menu = new CategorySelectionMenu(dict, "Vehicle");
                menu.Build("Land");
                Children.Add(menu);
                AddItem(item);
                BindMenuToItem(menu, item);

                item.Activated += (men, itm) =>
                {
                    Editor.RingData.Color = Color.MediumPurple;
                    Editor.RingData.Type = RingType.HorizontalSplitArrowCircle;
                    Editor.MarkerData.Display = true;
                    Editor.MarkerData.MarkerType = "prop_mp_placement";
                    Editor.RingData.HeightOffset = 1f;
                    Editor.MarkerData.HeightOffset = 1f;
                    GameFiber.StartNew(delegate
                    {
                        var hash =
                            StaticData.VehicleData.Database[menu.CurrentSelectedCategory].First(
                                tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;
                        var veh = new Vehicle(Util.RequestModel(hash), Game.LocalPlayer.Character.Position);

                        veh.IsPositionFrozen = true;
                        Editor.MarkerData.RepresentedBy = veh;
                        NativeFunction.CallByName<uint>("SET_ENTITY_COLLISION", veh.Handle.Value, false, 0);
                    });
                };

                menu.OnMenuClose += (men) =>
                {
                    if(Editor.MarkerData.RepresentedBy != null && Editor.MarkerData.RepresentedBy.IsValid())
                        Editor.MarkerData.RepresentedBy.Delete();

                    Editor.MarkerData.RepresentedBy = null;
                    Editor.MarkerData.MarkerType = null;
                    Editor.MarkerData.Display = false;
                    Editor.MarkerData.HeightOffset = 0f;
                    Editor.RingData.HeightOffset = 0f;
                    Editor.RingData.Color = Color.Gray;
                    Editor.RingData.Type = RingType.HorizontalCircleSkinny;
                };

                menu.SelectionChanged += (sender, eventargs) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        var heading = 0f;
                        if (Editor.MarkerData.RepresentedBy != null && Editor.MarkerData.RepresentedBy.IsValid())
                        {
                            heading = Editor.MarkerData.RepresentedBy.Heading;
                            Editor.MarkerData.RepresentedBy.Delete();
                        }


                        var hash =
                            StaticData.VehicleData.Database[menu.CurrentSelectedCategory].First(
                                tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;
                        var veh = new Vehicle(Util.RequestModel(hash), Game.LocalPlayer.Character.Position);
                        veh.Heading = heading;
                        veh.IsPositionFrozen = true;
                        Editor.MarkerData.RepresentedBy = veh;
                        NativeFunction.CallByName<uint>("SET_ENTITY_COLLISION", veh.Handle.Value, false, 0);
                        //var dims = Util.GetModelDimensions(veh.Model);
                        //Editor.RingData.HeightOffset = 1f;
                        //Editor.MarkerData.HeightOffset = 1f;
                    });
                };
            }
            #endregion

            #region Pickups
            {
                var item = new NativeMenuItem("Pickups");
                var dict = StaticData.WeaponsData.Database.ToDictionary(k => k.Key, k => k.Value.Select(x => x.Item1).ToArray());
                var menu = new CategorySelectionMenu(dict, "Weapon");
                
                menu.Build("Pistols");
                Children.Add(menu);
                AddItem(item);
                BindMenuToItem(menu, item);

                item.Activated += (men, itm) =>
                {
                    Editor.RingData.Color = Color.MediumPurple;
                    Editor.RingData.Type = RingType.HorizontalSplitArrowCircle;
                    Editor.MarkerData.Display = true;
                    Editor.MarkerData.MarkerType = "prop_mp_placement";
                    Editor.MarkerData.RepresentationHeightOffset = 1f;
                    GameFiber.StartNew(delegate
                    {
                        var hash =
                            StaticData.WeaponsData.Database[menu.CurrentSelectedCategory].First(
                                tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;
                        
                        var pos = Game.LocalPlayer.Character.Position;
                        var veh =
                            World.GetEntityByHandle<Rage.Object>(NativeFunction.CallByName<uint>("CREATE_WEAPON_OBJECT", hash,
                                                                                                 9999, pos.X, pos.Y, pos.Z,
                                                                                                 true, 3f));
                        Editor.PlacedWeaponHash = hash;
                        veh.IsPositionFrozen = true;
                        Editor.MarkerData.RepresentedBy = veh;
                        NativeFunction.CallByName<uint>("SET_ENTITY_COLLISION", veh.Handle.Value, false, 0);
                    });
                };

                menu.OnMenuClose += (men) =>
                {
                    if (Editor.MarkerData.RepresentedBy != null && Editor.MarkerData.RepresentedBy.IsValid())
                        Editor.MarkerData.RepresentedBy.Delete();

                    Editor.MarkerData.RepresentedBy = null;
                    Editor.MarkerData.MarkerType = null;
                    Editor.MarkerData.Display = false;
                    Editor.MarkerData.HeightOffset = 0f;
                    Editor.MarkerData.RepresentationHeightOffset = 0f;
                    Editor.RingData.HeightOffset = 0f;
                    Editor.RingData.Color = Color.Gray;
                    Editor.RingData.Type = RingType.HorizontalCircleSkinny;
                    Editor.PlacedWeaponHash = 0;
                };

                menu.SelectionChanged += (sender, eventargs) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        var heading = 0f;
                        if (Editor.MarkerData.RepresentedBy != null && Editor.MarkerData.RepresentedBy.IsValid())
                        {
                            heading = Editor.MarkerData.RepresentedBy.Heading;
                            Editor.MarkerData.RepresentedBy.Delete();
                        }


                        var hash =
                            StaticData.WeaponsData.Database[menu.CurrentSelectedCategory].First(
                                tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;
                        
                        var pos = Game.LocalPlayer.Character.Position;
                        var veh =
                            World.GetEntityByHandle<Rage.Object>(NativeFunction.CallByName<uint>("CREATE_WEAPON_OBJECT", hash,
                                                                                                 9999, pos.X, pos.Y, pos.Z,
                                                                                                 true, 3f));

                        veh.Heading = heading;
                        veh.IsPositionFrozen = true;
                        Editor.PlacedWeaponHash = hash;
                        Editor.MarkerData.RepresentedBy = veh;
                        NativeFunction.CallByName<uint>("SET_ENTITY_COLLISION", veh.Handle.Value, false, 0);
                        //var dims = Util.GetModelDimensions(veh.Model);
                        //Editor.RingData.HeightOffset = 1f;
                        //Editor.MarkerData.HeightOffset = 1f;
                    });
                };
            }
            #endregion

            #region Objects
            {
                {
                    var item = new NativeMenuItem("Objects");
                    var dict = StaticData.ObjectData.Database.ToDictionary(k => k.Key, k => k.Value.Select(x => x.Item1).ToArray());
                    var menu = new CategorySelectionMenu(dict, "Model");

                    menu.Build("Ramps");
                    Children.Add(menu);
                    AddItem(item);
                    BindMenuToItem(menu, item);

                    item.Activated += (men, itm) =>
                    {
                        Editor.RingData.Color = Color.MediumPurple;
                        Editor.RingData.Type = RingType.HorizontalSplitArrowCircle;
                        Editor.MarkerData.Display = true;
                        Editor.MarkerData.MarkerType = "prop_mp_placement";
                        Editor.MarkerData.RepresentationHeightOffset = 1f;
                        GameFiber.StartNew(delegate
                        {
                            var hash =
                                StaticData.ObjectData.Database[menu.CurrentSelectedCategory].First(
                                    tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;

                            var pos = Game.LocalPlayer.Character.Position;
                            var veh = new Rage.Object(Util.RequestModel(hash), pos);
                            veh.IsPositionFrozen = true;
                            Editor.MarkerData.RepresentedBy = veh;
                            NativeFunction.CallByName<uint>("SET_ENTITY_COLLISION", veh.Handle.Value, false, 0);
                        });
                    };

                    menu.OnMenuClose += (men) =>
                    {
                        if (Editor.MarkerData.RepresentedBy != null && Editor.MarkerData.RepresentedBy.IsValid())
                            Editor.MarkerData.RepresentedBy.Delete();

                        Editor.MarkerData.RepresentedBy = null;
                        Editor.MarkerData.MarkerType = null;
                        Editor.MarkerData.Display = false;
                        Editor.MarkerData.HeightOffset = 0f;
                        Editor.MarkerData.RepresentationHeightOffset = 0f;
                        Editor.RingData.HeightOffset = 0f;
                        Editor.RingData.Color = Color.Gray;
                        Editor.RingData.Type = RingType.HorizontalCircleSkinny;
                    };

                    menu.SelectionChanged += (sender, eventargs) =>
                    {
                        GameFiber.StartNew(delegate
                        {
                            var heading = 0f;
                            if (Editor.MarkerData.RepresentedBy != null && Editor.MarkerData.RepresentedBy.IsValid())
                            {
                                heading = Editor.MarkerData.RepresentedBy.Heading;
                                Editor.MarkerData.RepresentedBy.Delete();
                            }


                            var hash =
                                StaticData.ObjectData.Database[menu.CurrentSelectedCategory].First(
                                    tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;

                            var pos = Game.LocalPlayer.Character.Position;
                            var veh = new Rage.Object(Util.RequestModel(hash), pos);

                            veh.Heading = heading;
                            veh.IsPositionFrozen = true;
                            Editor.MarkerData.RepresentedBy = veh;
                            NativeFunction.CallByName<uint>("SET_ENTITY_COLLISION", veh.Handle.Value, false, 0);
                            //var dims = Util.GetModelDimensions(veh.Model);
                            //Editor.RingData.HeightOffset = 1f;
                            //Editor.MarkerData.HeightOffset = 1f;
                        });
                    };
                }
            }
            #endregion


            RefreshIndex();
        }

        public void Process()
        {
            Children.ForEach(m =>
            {
                m.ProcessControl();
                m.Draw();
            });
        }
    }
}