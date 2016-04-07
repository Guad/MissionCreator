using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MissionCreator.SerializableData;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.Editor.NestedMenus
{
    public class ObjectivePlacementMenu : UIMenu, INestedMenu
    {
        public ObjectivePlacementMenu(MissionData data) : base("MissionCreator", "CREATE OBJECTIVE")
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
            #region Actors
            {
                var item = new UIMenuItem("Actors");

                var dict = StaticData.PedData.Database.ToDictionary(k => k.Key, k => k.Value.Select(x => x.Item1).ToArray());
                var menu = new CategorySelectionMenu(dict, "Skin");
                menu.Build("Cops and Army");
                Children.Add(menu);
                AddItem(item);
                BindMenuToItem(menu, item);

                item.Activated += (men, itm) =>
                {
                    Editor.IsPlacingObjective = true;
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
                    Editor.IsPlacingObjective = false;
                };

                menu.SelectionChanged += (sender, eventargs) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        menu.ResetKey(Common.MenuControls.Right);
                        menu.ResetKey(Common.MenuControls.Left);
                        var heading = 0f;
                        if (Editor.MarkerData.RepresentedBy != null && Editor.MarkerData.RepresentedBy.IsValid())
                        {
                            heading = Editor.MarkerData.RepresentedBy.Heading;
                            Editor.MarkerData.RepresentedBy.Model.Dismiss();
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
                        menu.SetKey(Common.MenuControls.Left, GameControl.CellphoneLeft, 0);
                        menu.SetKey(Common.MenuControls.Right, GameControl.CellphoneRight, 0);
                    });
                };

            }
            #endregion

            #region Cars
            {
                var item = new UIMenuItem("Cars");
                var dict = StaticData.VehicleData.Database.ToDictionary(k => k.Key, k => k.Value.Select(x => x.Item1).ToArray());
                var menu = new CategorySelectionMenu(dict, "Vehicle");
                menu.Build("Muscle");
                Children.Add(menu);
                AddItem(item);
                BindMenuToItem(menu, item);

                item.Activated += (men, itm) =>
                {
                    Editor.IsPlacingObjective = true;
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
                    Editor.IsPlacingObjective = false;
                };

                menu.SelectionChanged += (sender, eventargs) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        menu.ResetKey(Common.MenuControls.Right);
                        menu.ResetKey(Common.MenuControls.Left);
                        var heading = 0f;
                        if (Editor.MarkerData.RepresentedBy != null && Editor.MarkerData.RepresentedBy.IsValid())
                        {
                            heading = Editor.MarkerData.RepresentedBy.Heading;
                            Editor.MarkerData.RepresentedBy.Model.Dismiss();
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
                        Vector3 max;
                        Vector3 min;
                        Util.GetModelDimensions(veh.Model, out max, out min);
                        Editor.RingData.HeightOffset = min.Z - max.Z + 0.2f;
                        Editor.MarkerData.HeightOffset = min.Z - max.Z + 0.2f;

                        menu.SetKey(Common.MenuControls.Left, GameControl.CellphoneLeft, 0);
                        menu.SetKey(Common.MenuControls.Right, GameControl.CellphoneRight, 0);
                    });
                };
            }
            #endregion
            
            #region Pickups
            {
                var item = new UIMenuItem("Pickups");
                var dict = StaticData.PickupData.Database.ToDictionary(k => k.Key, k => k.Value.Select(x => x.Item1).ToArray());
                var menu = new CategorySelectionMenu(dict, "Weapon");

                menu.Build("Pistols");
                Children.Add(menu);
                AddItem(item);
                BindMenuToItem(menu, item);

                item.Activated += (men, itm) =>
                {
                    Editor.RingData.Color = Color.MediumPurple;
                    Editor.RingData.Type = RingType.HorizontalSplitArrowCircle;
                    Editor.MarkerData.Display = false;
                    Editor.MarkerData.RepresentationHeightOffset = 1f;
                    GameFiber.StartNew(delegate
                    {
                        var hash =
                            StaticData.PickupData.Database[menu.CurrentSelectedCategory].First(
                                tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;

                        var pos = Game.LocalPlayer.Character.Position;
                        var veh = new Rage.Object(Util.RequestModel("prop_mp_repair"), pos);
                        Editor.PlacedWeaponHash = hash;
                        veh.IsPositionFrozen = true;
                        Editor.MarkerData.RepresentedBy = veh;
                        NativeFunction.CallByName<uint>("SET_ENTITY_COLLISION", veh.Handle.Value, false, 0);
                        Editor.IsPlacingObjective = true;
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
                    Editor.IsPlacingObjective = false;
                };

                menu.SelectionChanged += (sender, eventargs) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        var hash =
                           StaticData.PickupData.Database[menu.CurrentSelectedCategory].First(
                               tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;
                        Editor.PlacedWeaponHash = hash;
                    });
                };
            }
            #endregion

            #region Checkpoints
            {
                var item = new UIMenuItem("Checkpoint");
                var dict = StaticData.CheckpointData.Database.ToDictionary(k => k.Key, k => k.Value.Select(x => x.Item1).ToArray());
                var menu = new CategorySelectionMenu(dict, "Checkpoint");

                menu.Build("Marker");
                Children.Add(menu);
                AddItem(item);
                BindMenuToItem(menu, item);

                item.Activated += (men, itm) =>
                {
                    Editor.RingData.Color = Color.MediumPurple;
                    Editor.RingData.Type = RingType.HorizontalSplitArrowCircle;
                    Editor.MarkerData.Display = false;
                    Editor.MarkerData.RepresentationHeightOffset = 0f;
                    Editor.IsPlacingObjective = true;
                    GameFiber.StartNew(delegate
                    {
                        var hash = StaticData.CheckpointData.Database[menu.CurrentSelectedCategory].First(
                                tuple => tuple.Item1 == menu.CurrentSelectedItem);

                        var pos = Game.LocalPlayer.Character.Position;
                        var veh = new Rage.Object(Util.RequestModel("prop_mp_icon_shad_sm"), pos);
                        veh.Opacity = 0f;
                        veh.IsPositionFrozen = true;
                        Editor.MarkerData.RepresentedBy = veh;
                        Editor.ObjectiveMarkerId = hash.Item2;
                        Editor.MarkerData.HeightOffset = hash.Item3;
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
                    Editor.IsPlacingObjective = false;
                    Editor.ObjectiveMarkerId = null;
                };

                menu.SelectionChanged += (sender, eventargs) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        var hash =
                            StaticData.CheckpointData.Database[menu.CurrentSelectedCategory].First(
                                tuple => tuple.Item1 == menu.CurrentSelectedItem);

                        var pos = Game.LocalPlayer.Character.Position;
                        
                        Editor.ObjectiveMarkerId = hash.Item2;
                        Editor.MarkerData.HeightOffset = hash.Item3;
                        //var dims = Util.GetModelDimensions(veh.Model);
                        //Editor.RingData.HeightOffset = 1f;
                        //Editor.MarkerData.HeightOffset = 1f;
                    });
                };
            }
            #endregion
            /*
            {
                var item = new UIMenuItem("Timer");
                AddItem(item);
            }*/
            RefreshIndex();
        }

        public void Process()
        {
            ProcessControl();
            Draw();
            Children.ForEach(m =>
            {
                m.ProcessControl();
                m.Draw();
            });
        }
    }
}