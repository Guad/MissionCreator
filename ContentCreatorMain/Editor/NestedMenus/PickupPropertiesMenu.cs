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
    public class PickupPropertiesMenu : UIMenu, INestedMenu
    {
        public PickupPropertiesMenu() : base("", "PICKUP PROPERTIES", new Point(0, -107))
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

        public void BuildFor(SerializableData.SerializablePickup actor)
        {
            Clear();

            #region SpawnAfter
            {
                var item = new UIMenuListItem("Spawn Before Objective", StaticData.StaticLists.NumberMenu, actor.SpawnAfter);

                item.OnListChanged += (sender, index) =>
                {
                    actor.SpawnAfter = index;
                };

                AddItem(item);
            }
            #endregion 
            
            #region Weapons
            {
                var listIndex = actor.Ammo == 0
                    ? StaticData.StaticLists.AmmoChoses.FindIndex(n => n == (dynamic) 9999)
                    : StaticData.StaticLists.AmmoChoses.FindIndex(n => n == (dynamic) actor.Ammo);
                var item = new UIMenuListItem("Ammo Count", StaticData.StaticLists.AmmoChoses, listIndex);

                item.OnListChanged += (sender, index) =>
                {
                    int newAmmo = int.Parse(((UIMenuListItem) sender).IndexToItem(index).ToString(), CultureInfo.InvariantCulture);
                    actor.Ammo = newAmmo;
                };

                AddItem(item);
            }
            #endregion

            #region Respawn
            {
                var item = new UIMenuCheckboxItem("Respawn", actor.Respawn);
                item.CheckboxEvent += (sender, @checked) =>
                {
                    actor.Respawn = @checked;
                };
                AddItem(item);
            }
            #endregion

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