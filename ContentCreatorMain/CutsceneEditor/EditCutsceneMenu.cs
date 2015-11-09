using System.Collections.Generic;
using System.Globalization;
using MissionCreator.Editor.NestedMenus;
using MissionCreator.SerializableData.Cutscenes;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.CutsceneEditor
{
    public class EditCutsceneMenu : UIMenu, INestedMenu
    {
        public CutsceneUi GrandParent { get; set; }
        public List<UIMenu> Children { get; set; }

        public EditCutsceneMenu(CutsceneUi grandpa) : base("Cutscene Creator", "EDIT CUTSCENE")
        {
            Children = new List<UIMenu>();
            MouseControlsEnabled = false;
            ResetKey(Common.MenuControls.Up);
            ResetKey(Common.MenuControls.Down);
            SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
            SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);

            GrandParent = grandpa;
        }


        public void Build(SerializableCutscene cutscene)
        {
            Clear();

            #region Name
            {
                var item = new NativeMenuItem("Name");
                item.SetRightLabel(cutscene.Name);
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
                            item.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                            item.SetRightLabel("");
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            Editor.Editor.DisableControlEnabling = false;
                            cutscene.Name = null;
                            MenuItems[3].Enabled = false;
                            return;
                        }
                        Editor.Editor.DisableControlEnabling = false;
                        item.SetRightBadge(NativeMenuItem.BadgeStyle.None);
                        cutscene.Name = title;
                        selectedItem.SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);

                        if (cutscene.Length > 0 && !string.IsNullOrEmpty(cutscene.Name))
                            MenuItems[3].Enabled = true;
                    });
                };
            }
            #endregion

            #region Play at Objective

            {
                var item = new MenuListItem("Play at Objective", StaticData.StaticLists.ObjectiveIndexList, cutscene.PlayAt);
                AddItem(item);
                item.OnListChanged += (sender, index) =>
                {
                    cutscene.PlayAt = index;
                };
            }
            #endregion

            #region Duration
            {
                var item = new NativeMenuItem("Duration in Seconds");
                item.SetRightLabel(((int)(cutscene.Length/1000f)).ToString());
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
                            item.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                            item.SetRightLabel("");
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            cutscene.Length = 0;
                            Editor.Editor.DisableControlEnabling = false;
                            MenuItems[3].Enabled = false;
                            return;
                        }
                        int len;
                        if (!int.TryParse(title, NumberStyles.Integer, CultureInfo.InvariantCulture, out len))
                        {
                            item.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                            item.SetRightLabel("");
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            cutscene.Length = 0;
                            Editor.Editor.DisableControlEnabling = false;
                            MenuItems[3].Enabled = false;
                            Game.DisplayNotification("Integer not in correct format.");
                            return;
                        }

                        if (len <= 0)
                        {
                            item.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
                            item.SetRightLabel("");
                            SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                            cutscene.Length = 0;
                            Editor.Editor.DisableControlEnabling = false;
                            Game.DisplayNotification("Duration must be more than 0");
                            MenuItems[3].Enabled = false;
                            return;
                        }

                        Editor.Editor.DisableControlEnabling = false;
                        item.SetRightBadge(NativeMenuItem.BadgeStyle.None);
                        cutscene.Length = len*1000;
                        selectedItem.SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                        if (cutscene.Length > 0 && !string.IsNullOrEmpty(cutscene.Name))
                            MenuItems[3].Enabled = true;
                    });
                };
            }
            #endregion

            #region Edit

            {
                var item = new NativeMenuItem("Continue");
                AddItem(item);
                if (cutscene.Length == 0 || string.IsNullOrEmpty(cutscene.Name))
                    MenuItems[3].Enabled = false;

                item.Activated += (sender, selectedItem) =>
                {
                    GrandParent.EnterCutsceneEditor(cutscene);
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