using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.Editor.NestedMenus
{
    public class CategorySelectionMenu : UIMenu
    {
        public Dictionary<string, string[]> Items { get; private set; }
        public string ItemName { get; set; }
        public string CurrentSelectedItem { get; set; }
        public string CurrentSelectedCategory { get; set; }

        public event EventHandler SelectionChanged;

        public CategorySelectionMenu(Dictionary<string, string[]> items, string itemName, string caption = "PLACE ENTITY") : base("Mission Creator", caption)
        {
            MouseEdgeEnabled = false;
            MouseControlsEnabled = false;
            Items = items;
            ItemName = itemName;
            ResetKey(Common.MenuControls.Up);
            ResetKey(Common.MenuControls.Down);
            SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
            SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);
        }
        
        public CategorySelectionMenu(Dictionary<string, string[]> items, string itemName, bool bannerless, string caption = "PLACE ENTITY") : base("", caption, new Point(0, -107))
        {
            MouseEdgeEnabled = false;
            MouseControlsEnabled = false;
            Items = items;
            ItemName = itemName;
            ResetKey(Common.MenuControls.Up);
            ResetKey(Common.MenuControls.Down);
            SetKey(Common.MenuControls.Up, GameControl.CellphoneUp, 0);
            SetKey(Common.MenuControls.Down, GameControl.CellphoneDown, 0);

            SetBannerType(new ResRectangle());
        }

        public virtual void Build(string type)
        {
            Clear();
            var typeList = Items.Select(pair => pair.Key).Cast<dynamic>().ToList();
            var typeItem = new UIMenuListItem("Category", typeList, typeList.FindIndex(n => n.ToString() == type));
            AddItem(typeItem);

            typeItem.OnListChanged += (sender, index) =>
            {
                string newType = ((UIMenuListItem) MenuItems[0]).IndexToItem(((UIMenuListItem) MenuItems[0]).Index).ToString();
                Build(newType);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            };

            var itemListItem = new UIMenuListItem(ItemName, Items[type].Select(s => (dynamic)s).ToList(), 0);
            AddItem(itemListItem);

            CurrentSelectedItem = (string)itemListItem.IndexToItem(0);
            CurrentSelectedCategory = type;
            itemListItem.OnListChanged += (sender, index) =>
            {
                CurrentSelectedItem = (string)itemListItem.IndexToItem(index);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            };

            RefreshIndex();
        }
        

    }
}