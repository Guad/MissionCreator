using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace ContentCreator.Editor.NestedMenus
{
    public class MarkerPropertiesMenu : UIMenu, INestedMenu
    {
        public MarkerPropertiesMenu() : base("", "CHECKPOINT PROPERTIES", new Point(0, -107))
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

        public void BuildFor(SerializableData.Objectives.SerializableMarker actor)
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
                var item = new MenuListItem("Objective Index", StaticData.StaticLists.ObjectiveIndexList, actor.ActivateAfter - 1);

                item.OnListChanged += (sender, index) =>
                {
                    actor.ActivateAfter = index + 1;


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

            #region Color

            {
                var col = Color.FromArgb(actor.Alpha, (int) actor.Color.X, (int) actor.Color.Y, (int) actor.Color.Z).ToKnownColor();
                var idx = StaticData.StaticLists.KnownColors.IndexOf(col);
                var item = new MenuListItem("Color", StaticData.StaticLists.KnownColors, idx == -1 ? 0 : idx);

                item.OnListChanged += (sender, index) =>
                {
                    var newCol = (Color)Color.FromKnownColor(StaticData.StaticLists.KnownColors[index]);
                    actor.Alpha = newCol.A;
                    actor.Color = new Vector3(newCol.R, newCol.G, newCol.B);
                };

                AddItem(item);
            }
            #endregion

            // TODO: Change NumberMenu to max num of objectives in mission
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