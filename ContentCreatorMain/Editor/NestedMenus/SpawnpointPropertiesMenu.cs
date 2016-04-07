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
    public class SpawnpointPropertiesMenu : UIMenu, INestedMenu
    {
        public SpawnpointPropertiesMenu() : base("", "SPAWNPOINT PROPERTIES", new Point(0, -107))
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

        public void BuildFor(SerializableData.SerializableSpawnpoint actor)
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
                
                var item = new UIMenuItem("Weapon");
                var dict = StaticData.WeaponsData.Database.ToDictionary(k => k.Key, k => k.Value.Select(x => x.Item1).ToArray());
                var menu = new CategorySelectionMenu(dict, "Weapon", true, "SELECT WEAPON");
                menu.Build("Melee");
                Children.Add(menu);
                AddItem(item);
                BindMenuToItem(menu, item);
                
                menu.SelectionChanged += (sender, eventargs) =>
                {
                    GameFiber.StartNew(delegate
                    {
                        var hash = StaticData.WeaponsData.Database[menu.CurrentSelectedCategory].First(
                                tuple => tuple.Item1 == menu.CurrentSelectedItem).Item2;
                        NativeFunction.CallByName<uint>("REMOVE_ALL_PED_WEAPONS", actor.GetEntity().Handle.Value, true);
                        ((Ped) actor.GetEntity()).Inventory.GiveNewWeapon(hash, (short)(actor.WeaponAmmo == 0 ? 9999 : actor.WeaponAmmo), true);
                        actor.WeaponHash = hash;
                    });
                };
            }

            {
                var listIndex = actor.WeaponAmmo == 0
                    ? StaticData.StaticLists.AmmoChoses.FindIndex(n => n == (dynamic) 9999)
                    : StaticData.StaticLists.AmmoChoses.FindIndex(n => n == (dynamic) actor.WeaponAmmo);
                var item = new UIMenuListItem("Ammo Count", StaticData.StaticLists.AmmoChoses, listIndex);

                item.OnListChanged += (sender, index) =>
                {
                    int newAmmo = int.Parse(((UIMenuListItem) sender).IndexToItem(index).ToString(), CultureInfo.InvariantCulture);
                    actor.WeaponAmmo = newAmmo;
                    if(actor.WeaponHash == 0) return;
                    NativeFunction.CallByName<uint>("REMOVE_ALL_PED_WEAPONS", actor.GetEntity().Handle.Value, true);
                    ((Ped)actor.GetEntity()).Inventory.GiveNewWeapon(actor.WeaponHash, (short)newAmmo, true);
                };

                AddItem(item);
            }
            #endregion

            #region Health
            {
                var listIndex = actor.Health == 0
                    ? StaticData.StaticLists.HealthArmorChoses.FindIndex(n => n == (dynamic)200)
                    : StaticData.StaticLists.HealthArmorChoses.FindIndex(n => n == (dynamic)actor.Health);
                var item = new UIMenuListItem("Health", StaticData.StaticLists.HealthArmorChoses, listIndex);

                item.OnListChanged += (sender, index) =>
                {
                    int newAmmo = int.Parse(((UIMenuListItem)sender).IndexToItem(index).ToString(), CultureInfo.InvariantCulture);
                    actor.Health = newAmmo;
                };

                AddItem(item);
            }
            #endregion

            #region Armor
            {
                var listIndex = StaticData.StaticLists.HealthArmorChoses.FindIndex(n => n == (dynamic)actor.Armor);
                var item = new UIMenuListItem("Armor", StaticData.StaticLists.HealthArmorChoses, listIndex);

                item.OnListChanged += (sender, index) =>
                {
                    int newAmmo = int.Parse(((UIMenuListItem)sender).IndexToItem(index).ToString(), CultureInfo.InvariantCulture);
                    actor.Armor = newAmmo;
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