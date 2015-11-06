using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

            #region Title
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
            #endregion

            #region Description
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
            #endregion

            #region Author
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
            #endregion

            #region Weather
            {
                var item = new MenuListItem("Weather", StaticData.StaticLists.WeatherTypes,
                    StaticData.StaticLists.WeatherTypes.IndexOf(data.Weather.ToString()));
                AddItem(item);

                item.OnListChanged += (sender, index) =>
                {
                    data.Weather = Enum.Parse(typeof (WeatherType), item.IndexToItem(index).ToString());
                };
            }
            #endregion

            #region Time of Day
            {
                var item = new MenuListItem("Time", StaticData.StaticLists.TimesList,
                    StaticData.StaticLists.TimesList.IndexOf(
                        StaticData.StaticLists.TimeTranslation.FirstOrDefault(p => p.Value == data.Time).Key));
                AddItem(item);
                
                item.OnListChanged += (sender, index) =>
                {
                    data.Time = StaticData.StaticLists.TimeTranslation[item.IndexToItem(index).ToString()];
                };
            }
            #endregion

            #region Time Limit
            {
                var item = new MenuCheckboxItem("Time Limit", data.TimeLimit.HasValue);
                AddItem(item);

                var inputItem = new NativeMenuItem("Seconds");
                AddItem(inputItem);

                if (data.TimeLimit.HasValue)
                {
                    if(data.TimeLimit.Value == 0)
                        inputItem.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                    else
                        inputItem.SetRightLabel(data.TimeLimit.Value.ToString());
                }
                else
                    inputItem.Enabled = false;

                inputItem.Activated += (sender, selectedItem) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        ResetKey(Common.MenuControls.Back);
                        Editor.DisableControlEnabling = true;
                        string title = Util.GetUserInput();
                        if (string.IsNullOrEmpty(title))
                        {
                            inputItem.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                            data.TimeLimit = 0;
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            Editor.DisableControlEnabling = false;
                            return;
                        }
                        int seconds;
                        if (!int.TryParse(title, NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds))
                        {
                            Game.DisplayNotification("~h~ERROR~h~: That is not a valid number.");
                            inputItem.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                            data.TimeLimit = 0;
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            Editor.DisableControlEnabling = false;
                            return;
                        }

                        if (seconds == 0)
                        {
                            Game.DisplayNotification("~h~ERROR~h~: Time limit must be more than 0");
                            inputItem.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                            data.TimeLimit = 0;
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            Editor.DisableControlEnabling = false;
                            return;
                        }

                        data.TimeLimit = seconds;
                        inputItem.SetRightBadge(NativeMenuItem.BadgeStyle.None);
                        inputItem.SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                        Editor.DisableControlEnabling = false;
                    });
                };

                item.CheckboxEvent += (sender, @checked) =>
                {
                    if (!@checked)
                    {
                        data.TimeLimit = null;
                        inputItem.Enabled = false;
                        inputItem.SetRightBadge(NativeMenuItem.BadgeStyle.None);
                        inputItem.SetRightLabel("");
                    }
                    else
                    {
                        inputItem.Enabled = true;
                        inputItem.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                        inputItem.SetRightLabel("");
                    }
                };
            }
            #endregion

            #region Max Wanted
            {
                var item = new MenuListItem("Maximum Wanted Level", StaticData.StaticLists.WantedList, data.MaxWanted);
                AddItem(item);

                item.OnListChanged += (sender, index) =>
                {
                    data.MaxWanted = index;
                };
            }
            #endregion

            #region Min Wanted
            {
                var item = new MenuListItem("Minimum Wanted Level", StaticData.StaticLists.WantedList, data.MinWanted);
                AddItem(item);

                item.OnListChanged += (sender, index) =>
                {
                    data.MinWanted = index;
                };
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