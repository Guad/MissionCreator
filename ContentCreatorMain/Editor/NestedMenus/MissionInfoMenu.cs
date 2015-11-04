using System.Collections.Generic;
using ContentCreator.SerializableData;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace ContentCreator.Editor.NestedMenus
{
    public class MissionInfoMenu : UIMenu, INestedMenu
    {
        public MissionInfoMenu(MissionData data) : base("Content Creator", "MISSION DETAILS")
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
            {
                var item = new NativeMenuItem("Title");
                if(string.IsNullOrEmpty(data.Name))
                    item.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                else
                    item.SetRightLabel(data.Name);

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
                            Editor.CurrentMission.Name = "";
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            Editor.DisableControlEnabling = false;
                            return;
                        }
                        item.SetRightBadge(NativeMenuItem.BadgeStyle.None);
                        Editor.CurrentMission.Name = title;
                        selectedItem.SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                        Editor.DisableControlEnabling = false;
                    });
                };
                AddItem(item);
            }

            {
                var item = new NativeMenuItem("Description");
                if (string.IsNullOrEmpty(data.Description))
                    item.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                else
                    item.SetRightLabel(data.Description);

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
                            Editor.CurrentMission.Description = "";
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            Editor.DisableControlEnabling = false;
                            return;
                        }
                        item.SetRightBadge(NativeMenuItem.BadgeStyle.None);
                        Editor.CurrentMission.Description = title;
                        selectedItem.SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                        Editor.DisableControlEnabling = false;
                    });
                };
                AddItem(item);
            }

            {
                var item = new NativeMenuItem("Author");
                if (string.IsNullOrEmpty(data.Author))
                {
                    var name = (string)NativeFunction.CallByHash(0x198D161F458ECC7F, typeof(string));
                    if (!string.IsNullOrEmpty(name) && name != "UNKNOWN")
                    {
                        item.SetRightLabel(name.Length > 20 ? name.Substring(0, 20) + "..." : name);
                        Editor.CurrentMission.Author = name;
                    }
                    else
                    {
                        item.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                    }
                }
                else
                    item.SetRightLabel(data.Author);

                

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
                            Editor.CurrentMission.Author = "";
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            Editor.DisableControlEnabling = false;
                            return;
                        }
                        item.SetRightBadge(NativeMenuItem.BadgeStyle.None);
                        Editor.CurrentMission.Author = title;
                        selectedItem.SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                        Editor.DisableControlEnabling = false;
                    });
                };
                AddItem(item);
            }
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