using System.Collections.Generic;
using MissionCreator.SerializableData;
using MissionCreator.SerializableData.Cutscenes;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.CutsceneEditor
{
    public class CutsceneUi
    {
        public UIMenu CutsceneMenus;
        public bool IsInCutsceneEditor;
        public SerializableCutscene CurrentCutscene;

        private List<UIMenu> _children;

        public CutsceneUi()
        {
            #region NativeUI Init
            {
                CutsceneMenus = new UIMenu("Cutscene Creator", "CUTSCENES");
                CutsceneMenus.MouseControlsEnabled = false;
                CutsceneMenus.ResetKey(Common.MenuControls.Up);
                CutsceneMenus.ResetKey(Common.MenuControls.Down);
                CutsceneMenus.SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
                CutsceneMenus.SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);

                CutsceneMenus.OnMenuClose += sender =>
                {
                    Editor.Editor.CameraClampMax = -30f;
                    IsInCutsceneEditor = false;
                    CurrentCutscene = null;
                };
            }
            #endregion

            _children = new List<UIMenu>();
        }

        public void Enter()
        {
            Editor.Editor.CameraClampMax = -2f;
            RebuildCutsceneMenu();
        }

        public void RebuildCutsceneMenu()
        {
            CutsceneMenus.Clear();
            _children.Clear();

            {
                var menu = new CreateCutsceneMenu(this);
                var item = new NativeMenuItem("Create Cutscene");
                CutsceneMenus.AddItem(item);
                CutsceneMenus.BindMenuToItem(menu, item);
                _children.Add(menu);
            }

            foreach (var cutscene in Editor.Editor.CurrentMission.Cutscenes)
            {
                var item = new NativeMenuItem(cutscene.Name);
                CutsceneMenus.AddItem(item);
                item.Activated += (sender, selectedItem) =>
                {
                    IsInCutsceneEditor = true;
                    CurrentCutscene = cutscene;
                };
            }

            CutsceneMenus.RefreshIndex();
        }

        public void Process()
        {
            CutsceneMenus.ProcessControl();
            CutsceneMenus.Draw();

            _children.ForEach(m =>
            {
                m.ProcessControl();
                m.Draw();
            });
        }
    }
}