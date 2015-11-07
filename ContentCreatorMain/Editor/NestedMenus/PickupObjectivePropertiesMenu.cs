using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.Editor.NestedMenus
{
    public class PickupObjectivePropertiesMenu : UIMenu, INestedMenu
    {
        public PickupObjectivePropertiesMenu() : base("", "PICKUP OBJECTIVE PROPERTIES", new Point(0, -107))
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

        public void BuildFor(SerializableData.Objectives.SerializablePickupObjective actor)
        {
            Clear();

            #region SpawnAfter
            {
                var item = new MenuListItem("Spawn After Objective", StaticData.StaticLists.NumberMenu, actor.SpawnAfter);

                item.OnListChanged += (sender, index) =>
                {
                    actor.SpawnAfter = index;
                };

                AddItem(item);
            }
            #endregion

            #region ObjectiveIndex
            {
                var item = new MenuListItem("Objective Index", StaticData.StaticLists.ObjectiveIndexList, actor.ActivateAfter);

                item.OnListChanged += (sender, index) =>
                {
                    actor.ActivateAfter = index;


                    if (string.IsNullOrEmpty(Editor.CurrentMission.ObjectiveNames[actor.ActivateAfter]))
                    {
                        MenuItems[2].SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                        MenuItems[2].SetRightLabel("");
                    }
                    else
                    {
                        var title = Editor.CurrentMission.ObjectiveNames[actor.ActivateAfter];
                        MenuItems[2].SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        MenuItems[2].SetRightBadge(NativeMenuItem.BadgeStyle.None);
                    }
                };

                AddItem(item);
            }
            #endregion
            // TODO: Change NumberMenu to max num of objectives in mission

            // Note: if adding items before weapons, change item order in VehiclePropertiesMenu

            #region Objective Name
            {
                var item = new NativeMenuItem("Objective Name");
                if (string.IsNullOrEmpty(Editor.CurrentMission.ObjectiveNames[actor.ActivateAfter]))
                    item.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
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
                        string title = Util.GetUserInput();
                        if (string.IsNullOrEmpty(title))
                        {
                            item.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                            Editor.CurrentMission.ObjectiveNames[actor.ActivateAfter] = "";
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            Editor.DisableControlEnabling = false;
                            return;
                        }
                        item.SetRightBadge(NativeMenuItem.BadgeStyle.None);
                        title = Regex.Replace(title, "-=", "~");
                        Editor.CurrentMission.ObjectiveNames[actor.ActivateAfter] = title;
                        selectedItem.SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                    });
                };
                AddItem(item);
            }
            #endregion

            #region Weapons
            {
                var listIndex = actor.Ammo == 0
                    ? StaticData.StaticLists.AmmoChoses.FindIndex(n => n == (dynamic) 9999)
                    : StaticData.StaticLists.AmmoChoses.FindIndex(n => n == (dynamic) actor.Ammo);
                var item = new MenuListItem("Ammo Count", StaticData.StaticLists.AmmoChoses, listIndex);

                item.OnListChanged += (sender, index) =>
                {
                    int newAmmo = int.Parse(((MenuListItem) sender).IndexToItem(index).ToString(), CultureInfo.InvariantCulture);
                    actor.Ammo = newAmmo;
                };

                AddItem(item);
            }
            #endregion

            #region Respawn
            {
                var item = new MenuCheckboxItem("Respawn", actor.Respawn);
                item.CheckboxEvent += (sender, @checked) =>
                {
                    actor.Respawn = @checked;
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