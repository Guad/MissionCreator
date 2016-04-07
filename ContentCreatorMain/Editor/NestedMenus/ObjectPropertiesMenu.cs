using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.Editor.NestedMenus
{
    public class ObjectPropertiesMenu : UIMenu, INestedMenu
    {
        public ObjectPropertiesMenu() : base("", "OBJECT PROPERTIES", new Point(0, -107))
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

        public void BuildFor(SerializableData.SerializableObject obj)
        {
            Clear();

            #region SpawnAfter
            {
                var item = new UIMenuListItem("Spawn After Objective", StaticData.StaticLists.NumberMenu, obj.SpawnAfter);

                item.OnListChanged += (sender, index) =>
                {
                    obj.SpawnAfter = index;
                };

                AddItem(item);
            }
            #endregion 

            #region RemoveAfter
            {
                var item = new UIMenuListItem("Remove After Objective", StaticData.StaticLists.RemoveAfterList, obj.RemoveAfter);

                item.OnListChanged += (sender, index) =>
                {
                    obj.RemoveAfter = index;
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