using System.Collections.Generic;
using System.Drawing;
using System.IO;
using MissionCreator.SerializableData;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.Editor.NestedMenus
{
    public class LoadMissionMenu : UIMenu
    {
        public LoadMissionMenu() : base("Mission Creator", "LOAD MISSION")
        {
            MouseControlsEnabled = false;
            ResetKey(Common.MenuControls.Up);
            ResetKey(Common.MenuControls.Down);
            SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
            SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);
            ReturnedData = null;
        }

        private const string basePath = "Plugins\\Missions";

        public MissionData ReturnedData { get; set; }

        public void RebuildMenu()
        {
            if (!Directory.Exists(basePath)) return;
            Clear();
            
            var filePaths = Directory.GetFiles(basePath, "*.xml");
            foreach (string path in filePaths)
            {
                var data = Editor.ReadMission(path);
                if (data == null) continue;
                var item = new UIMenuItem(data.Name, data.Description);
                AddItem(item);
                item.Activated += (sender, selectedItem) =>
                {
                    ReturnedData = data;
                    Visible = false;
                };
            }
            RefreshIndex();
        }

    }
}