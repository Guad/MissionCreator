using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MissionCreator.SerializableData.Objectives;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.Editor.NestedMenus
{
    public class VehicleObjectivePropertiesMenu : UIMenu, INestedMenu
    {
        public VehicleObjectivePropertiesMenu() : base("", "VEHICLE OBJECTIVE PROPERTIES", new Point(0, -107))
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

        public void BuildFor(SerializableData.Objectives.SerializableVehicleObjective actor)
        {
            Clear();

            #region SpawnAfter
            {
                var item = new UIMenuListItem("Spawn After Objective", StaticData.StaticLists.NumberMenu, actor.SpawnAfter);

                item.OnListChanged += (sender, index) =>
                {
                    actor.SpawnAfter = index;
                };

                AddItem(item);
            }
            #endregion

            #region ObjectiveIndex
            {
                var item = new UIMenuListItem("Objective Index", StaticData.StaticLists.ObjectiveIndexList, actor.ActivateAfter);

                item.OnListChanged += (sender, index) =>
                {
                    actor.ActivateAfter = index;


                    if (string.IsNullOrEmpty(Editor.CurrentMission.ObjectiveNames[actor.ActivateAfter]))
                    {
                        MenuItems[2].SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                        MenuItems[2].SetRightLabel("");
                    }
                    else
                    {
                        var title = Editor.CurrentMission.ObjectiveNames[actor.ActivateAfter];
                        MenuItems[2].SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        MenuItems[2].SetRightBadge(UIMenuItem.BadgeStyle.None);
                    }
                };

                AddItem(item);
            }
            #endregion 
            // TODO: Change NumberMenu to max num of objectives in mission

            // Note: if adding items before weapons, change item order in VehiclePropertiesMenu
            
            #region Objective Name
            {
                var item = new UIMenuItem("Objective Name");
                if (string.IsNullOrEmpty(Editor.CurrentMission.ObjectiveNames[actor.ActivateAfter]))
                    item.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                else
                {
                    var title = Editor.CurrentMission.ObjectiveNames[actor.ActivateAfter];
                    item.SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                }

                item.Activated += (sender, selectedItem) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        ResetKey(Common.MenuControls.Back);
                        Editor.DisableControlEnabling = true;
                        string title = Util.GetUserInput(this);
                        if (string.IsNullOrEmpty(title))
                        {
                            item.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                            Editor.CurrentMission.ObjectiveNames[actor.ActivateAfter] = "";
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            Editor.DisableControlEnabling = false;
                            return;
                        }
                        item.SetRightBadge(UIMenuItem.BadgeStyle.None);
                        title = Regex.Replace(title, "-=", "~");
                        Editor.CurrentMission.ObjectiveNames[actor.ActivateAfter] = title;
                        selectedItem.SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                    });
                };
                AddItem(item);
            }
            #endregion

            #region Health
            {
                var listIndex = actor.Health == 0
                    ? StaticData.StaticLists.VehicleHealthChoses.FindIndex(n => n == (dynamic)1000)
                    : StaticData.StaticLists.VehicleHealthChoses.FindIndex(n => n == (dynamic)actor.Health);

                var item = new UIMenuListItem("Health", StaticData.StaticLists.VehicleHealthChoses, listIndex);

                item.OnListChanged += (sender, index) =>
                {
                    int newAmmo = int.Parse(((UIMenuListItem)sender).IndexToItem(index).ToString(), CultureInfo.InvariantCulture);
                    actor.Health = newAmmo;
                };

                AddItem(item);
            }
            #endregion
            
            #region Passengers
            {
                var item = new UIMenuItem("Occupants");
                AddItem(item);
                if (((Vehicle)actor.GetVehicle()).HasOccupants)
                {
                    var newMenu = new UIMenu("", "OCCUPANTS", new Point(0, -107));
                    newMenu.MouseControlsEnabled = false;
                    newMenu.SetBannerType(new ResRectangle());
                    var occupants = ((Vehicle)actor.GetVehicle()).Occupants;
                    for (int i = 0; i < occupants.Length; i++)
                    {
                        var ped = occupants[i];
                        var type = Editor.GetEntityType(ped);
                        if (type == Editor.EntityType.NormalActor)
                        {
                            var act = Editor.CurrentMission.Actors.FirstOrDefault(a => a.GetEntity().Handle.Value == ped.Handle.Value);
                            if (act == null) continue;
                            var routedItem = new UIMenuItem(i == 0 ? "Driver" : "Passenger #" + i);
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
                            var routedItem = new UIMenuItem(i == 0 ? "Objective Driver" : "Objective Passenger #" + i);
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

            #region Show Health Bar
            {
                var item = new UIMenuCheckboxItem("Show Healthbar", actor.ShowHealthBar);
                AddItem(item);

                item.CheckboxEvent += (sender, @checked) =>
                {
                    actor.ShowHealthBar = @checked;
                    MenuItems[6].Enabled = @checked;
                };
            }
            #endregion

            #region Bar Name
            {
                var item = new UIMenuItem("Healthbar Label");
                AddItem(item);

                if (!actor.ShowHealthBar)
                    item.Enabled = false;

                if (string.IsNullOrEmpty(actor.Name) && actor.ShowHealthBar)
                    actor.Name = "HEALTH";
                if (actor.ShowHealthBar)
                    item.SetRightLabel(actor.Name.Length > 20 ? actor.Name.Substring(0, 20) : actor.Name);


                item.Activated += (sender, selectedItem) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        ResetKey(Common.MenuControls.Back);
                        Editor.DisableControlEnabling = true;
                        string title = Util.GetUserInput(this);
                        if (string.IsNullOrEmpty(title))
                        {
                            actor.Name = "HEALTH";
                            item.SetRightLabel(actor.Name.Length > 20 ? actor.Name.Substring(0, 20) : actor.Name);
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            Editor.DisableControlEnabling = false;
                            return;
                        }
                        title = Regex.Replace(title, "-=", "~");
                        actor.Name = title;
                        item.SetRightLabel(actor.Name.Length > 20 ? actor.Name.Substring(0, 20) : actor.Name);
                        SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                    });
                };
            }
            #endregion

            #region Objective Type
            {
                var item = new UIMenuListItem("Objective Type", StaticData.StaticLists.ObjectiveTypeList, actor.ObjectiveType);

                item.OnListChanged += (sender, index) =>
                {
                    actor.ObjectiveType = index;
                };
                AddItem(item);
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