using System.Collections.Generic;
using System.Globalization;
using MissionCreator.Editor.NestedMenus;
using MissionCreator.SerializableData.Cutscenes;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.CutsceneEditor
{
    public class CreateCutsceneMenu : UIMenu, INestedMenu
    {
        public CutsceneUi GrandParent { get; set; }
        public List<UIMenu> Children { get; set; }
        public SerializableCutscene CurrentCutscene { get; set; }

        public CreateCutsceneMenu(CutsceneUi grandpa) : base("Cutscene Creator", "CREATE A CUTSCENE")
        {
            Children = new List<UIMenu>();
            MouseControlsEnabled = false;
            ResetKey(Common.MenuControls.Up);
            ResetKey(Common.MenuControls.Down);
            SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
            SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);

            CurrentCutscene = new SerializableCutscene();
            GrandParent = grandpa;
            Build();
        }


        public void Build()
        {
            #region Name
            {
                var item = new UIMenuItem("Name");
                item.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                AddItem(item);
                item.Activated += (sender, selectedItem) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        ResetKey(Common.MenuControls.Back);
                        Editor.Editor.DisableControlEnabling = true;

                        string title = Util.GetUserInput();
                        if (string.IsNullOrEmpty(title))
                        {
                            item.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                            item.SetRightLabel("");
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            Editor.Editor.DisableControlEnabling = false;
                            CurrentCutscene.Name = null;
                            MenuItems[3].Enabled = false;
                            return;
                        }
                        Editor.Editor.DisableControlEnabling = false;
                        item.SetRightBadge(UIMenuItem.BadgeStyle.None);
                        CurrentCutscene.Name = title;
                        selectedItem.SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);

                        if (CurrentCutscene.Length > 0 && !string.IsNullOrEmpty(CurrentCutscene.Name))
                            MenuItems[3].Enabled = true;
                    });
                };
            }
            #endregion

            #region Play at Objective

            {
                CurrentCutscene.PlayAt = 0;
                var item = new UIMenuListItem("Play at Objective", StaticData.StaticLists.ObjectiveIndexList, 0);
                AddItem(item);
                item.OnListChanged += (sender, index) =>
                {
                    CurrentCutscene.PlayAt = index;
                };
            }
            #endregion

            #region Duration
            {
                var item = new UIMenuItem("Duration in Seconds");
                item.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                AddItem(item);
                item.Activated += (sender, selectedItem) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        ResetKey(Common.MenuControls.Back);
                        Editor.Editor.DisableControlEnabling = true;

                        string title = Util.GetUserInput();
                        if (string.IsNullOrEmpty(title))
                        {
                            item.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                            item.SetRightLabel("");
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            CurrentCutscene.Length = 0;
                            Editor.Editor.DisableControlEnabling = false;
                            return;
                        }
                        int len;
                        if (!int.TryParse(title, NumberStyles.Integer, CultureInfo.InvariantCulture, out len))
                        {
                            item.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                            item.SetRightLabel("");
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            CurrentCutscene.Length = 0;
                            Editor.Editor.DisableControlEnabling = false;
                            Game.DisplayNotification("Integer not in correct format.");
                            return;
                        }

                        if (len <= 0)
                        {
                            item.SetRightBadge(UIMenuItem.BadgeStyle.Alert);
                            item.SetRightLabel("");
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            CurrentCutscene.Length = 0;
                            Editor.Editor.DisableControlEnabling = false;
                            Game.DisplayNotification("Duration must be more than 0");
                            MenuItems[3].Enabled = false;
                            return;
                        }

                        Editor.Editor.DisableControlEnabling = false;
                        item.SetRightBadge(UIMenuItem.BadgeStyle.None);
                        CurrentCutscene.Length = len*1000;
                        selectedItem.SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                        if (CurrentCutscene.Length > 0 && !string.IsNullOrEmpty(CurrentCutscene.Name))
                            MenuItems[3].Enabled = true;
                    });
                };
            }
            #endregion

            #region Continue

            {
                var item = new UIMenuItem("Continue");
                AddItem(item);
                item.Enabled = false;
                item.Activated += (sender, selectedItem) =>
                {
                    Editor.Editor.CurrentMission.Cutscenes.Add(CurrentCutscene);
                    GrandParent.EnterCutsceneEditor(CurrentCutscene);
                    Visible = false;
                };
            }
            #endregion

            RefreshIndex();
        }


        public void Process()
        {
            
        }
    }
}