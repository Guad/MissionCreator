using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using MissionCreator.SerializableData.Objectives;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.Editor.NestedMenus
{
    public class VehiclePropertiesMenu : UIMenu, INestedMenu
    {
        public VehiclePropertiesMenu() : base("", "VEHICLE PROPERTIES", new Point(0, -107))
        {
            Children = new List<UIMenu>();
            SetBannerType(new ResRectangle());
            MouseControlsEnabled = false;
            ResetKey(Common.MenuControls.Up);
            ResetKey(Common.MenuControls.Down);
            SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
            SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);
        }

        public List<UIMenu> Children { get; set; }

        public void BuildFor(SerializableData.SerializableVehicle veh)
        {
            Clear();

            #region SpawnAfter
            {
                var item = new MenuListItem("Spawn After Objective", StaticData.StaticLists.NumberMenu, veh.SpawnAfter);

                item.OnListChanged += (sender, index) =>
                {
                    veh.SpawnAfter = index;
                };

                AddItem(item);
            }
            #endregion 

            #region RemoveAfter
            {
                var item = new MenuListItem("Remove After Objective", StaticData.StaticLists.RemoveAfterList, veh.RemoveAfter);

                item.OnListChanged += (sender, index) =>
                {
                    veh.RemoveAfter = index;
                };

                AddItem(item);
            }
            #endregion
            // TODO: Change NumberMenu to max num of objectives in mission

            #region Health
            {
                var listIndex = veh.Health == 0
                    ? StaticData.StaticLists.VehicleHealthChoses.FindIndex(n => n == (dynamic)1000)
                    : StaticData.StaticLists.VehicleHealthChoses.FindIndex(n => n == (dynamic)veh.Health);

                var item = new MenuListItem("Health", StaticData.StaticLists.VehicleHealthChoses, listIndex);

                item.OnListChanged += (sender, index) =>
                {
                    int newAmmo = int.Parse(((MenuListItem)sender).IndexToItem(index).ToString(), CultureInfo.InvariantCulture);
                    veh.Health = newAmmo;
                };

                AddItem(item);
            }
            #endregion

            #region FailOnDeath
            {
                var item = new MenuCheckboxItem("Mission Fail On Death", veh.FailMissionOnDeath);
                item.CheckboxEvent += (sender, @checked) =>
                {
                    veh.FailMissionOnDeath = @checked;
                };
                AddItem(item);
            }
            #endregion


            #region Passengers
            {
                var item = new NativeMenuItem("Occupants");
                AddItem(item);
                if (((Vehicle)veh.GetEntity()).HasOccupants)
                {
                    var newMenu = new UIMenu("", "OCCUPANTS", new Point(0, -107));
                    newMenu.MouseControlsEnabled = false;
                    newMenu.SetBannerType(new ResRectangle());
                    var occupants = ((Vehicle)veh.GetEntity()).Occupants;
                    for (int i = 0; i < occupants.Length; i++)
                    {
                        var ped = occupants[i];
                        var type = Editor.GetEntityType(ped);
                        if (type == Editor.EntityType.NormalActor)
                        {
                            var act = Editor.CurrentMission.Actors.FirstOrDefault(a => a.GetEntity().Handle.Value == ped.Handle.Value);
                            if (act == null) continue;
                            var routedItem = new NativeMenuItem(i == 0 ? "Driver" : "Passenger #" + i);
                            routedItem.Activated += (sender, selectedItem) =>
                            {
                                Editor.DisableControlEnabling = true;
                                Editor.EnableBasicMenuControls = true;
                                var propMenu = new ActorPropertiesMenu();
                                propMenu.BuildFor(act);
                                propMenu.MenuItems[2].Enabled = false;
                                propMenu.OnMenuClose += _ =>
                                {
                                    newMenu.Visible = true;
                                };

                                newMenu.Visible = false;
                                propMenu.Visible = true;
                                GameFiber.StartNew(delegate
                                {
                                    while (propMenu.Visible)
                                    {
                                        propMenu.ProcessControl();
                                        propMenu.Draw();
                                        propMenu.Process();
                                        GameFiber.Yield();
                                    }
                                });

                            };
                            newMenu.AddItem(routedItem);
                        }
                        else if (type == Editor.EntityType.ObjectiveActor)
                        {
                            var act = Editor.CurrentMission.Objectives
                                .OfType<SerializableActorObjective>()
                                .FirstOrDefault(a => a.GetPed().Handle.Value == ped.Handle.Value);
                            if (act == null) continue;
                            var routedItem = new NativeMenuItem(i == 0 ? "Objective Driver" : "Objective Passenger #" + i);
                            routedItem.Activated += (sender, selectedItem) =>
                            {
                                Editor.DisableControlEnabling = true;
                                Editor.EnableBasicMenuControls = true;
                                var propMenu = new ActorObjectivePropertiesMenu();
                                propMenu.BuildFor(act);
                                propMenu.MenuItems[2].Enabled = false;
                                propMenu.OnMenuClose += _ =>
                                {
                                    newMenu.Visible = true;
                                };

                                newMenu.Visible = false;
                                propMenu.Visible = true;
                                GameFiber.StartNew(delegate
                                {
                                    while (propMenu.Visible)
                                    {
                                        propMenu.ProcessControl();
                                        propMenu.Draw();
                                        propMenu.Process();
                                        GameFiber.Yield();
                                    }
                                });

                            };
                            newMenu.AddItem(routedItem);
                        }

                    }
                    BindMenuToItem(newMenu, item);
                    newMenu.RefreshIndex();
                    Children.Add(newMenu);
                }
                else
                {
                    item.Enabled = false;
                }

            }
            #endregion

            RefreshIndex();
        }

        public void Process()
        {
            Children.ForEach(x =>
            {
                x.ProcessControl();
                x.Draw();
            });
        }
    }
}