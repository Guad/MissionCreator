using System.Collections.Generic;
using MissionCreator.SerializableData;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.Editor.NestedMenus
{
    public class InteriorsMenu : UIMenu
    {
        public InteriorsMenu(MissionData data) : base("Mission Creator", "INTERIORS")
        {
            MouseEdgeEnabled = false;
            MouseControlsEnabled = false;
            Display(data);
            ResetKey(Common.MenuControls.Up);
            ResetKey(Common.MenuControls.Down);
            SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
            SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);
        }

        public void Display(MissionData data)
        {
            Clear();
            foreach (var pair in StaticData.IPLData.Database)
            {
                var item = new MenuCheckboxItem(pair.Key, data.Interiors.Contains(pair.Key), pair.Value.Item1 ? "This interior requires the online map to load." : "");
                AddItem(item);

                item.CheckboxEvent += (sender, @checked) =>
                {
                    if (@checked)
                    {
                        if (!data.Interiors.Contains(pair.Key))
                            data.Interiors.Add(pair.Key);

                        if (pair.Value.Item1)
                        {
                            Util.LoadOnlineMap();
                        }

                        foreach (string s in pair.Value.Item2)
                        {
                            Util.LoadInterior(s);
                        }

                        foreach (var s in pair.Value.Item3)
                        {
                            Util.RemoveInterior(s);
                        }
                    }
                    else
                    {
                        data.Interiors.Remove(pair.Key);

                        foreach (string s in pair.Value.Item3)
                        {
                            Util.LoadInterior(s);
                        }

                        foreach (var s in pair.Value.Item2)
                        {
                            Util.RemoveInterior(s);
                        }
                    }
                };
            }
            RefreshIndex();
        }
    }
}