using System.Collections.Generic;
using ContentCreator.Editor.NestedMenus;
using ContentCreator.SerializableData.Cutscenes;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace ContentCreator.CutsceneEditor
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
                var item = new NativeMenuItem("Name");
                item.SetRightBadge(NativeMenuItem.BadgeStyle.Alert);
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
                            return;
                        }
                        Editor.Editor.DisableControlEnabling = false;
                        item.SetRightBadge(NativeMenuItem.BadgeStyle.None);
                        CurrentCutscene.Name = title;
                        selectedItem.SetRightLabel(title.Length > 20 ? title.Substring(0, 20) + "..." : title);
                        SetKey(Common.MenuControls.Back, GameControl.CellphoneCancel, 0);
                    });
                };
            }
            #endregion
        }


        public void Process()
        {
            
        }
    }
}