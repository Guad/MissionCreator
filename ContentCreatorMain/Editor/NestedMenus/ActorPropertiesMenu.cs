using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using MissionCreator.Waypoints;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator.Editor.NestedMenus
{
    public class ActorPropertiesMenu : UIMenu, INestedMenu
    {
        public ActorPropertiesMenu() : base("", "ACTOR PROPERTIES", new Point(0, -107))
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

        public void BuildFor(SerializableData.SerializablePed actor)
        {
            Clear();

            #region SpawnAfter
            {
                var item = new UIMenuListItem("Spawn After Objective", StaticData.StaticLists.NumberMenu, actor.SpawnAfter);

                item.OnListChanged += (sender, index) =>
                {
                    actor.SpawnAfter = index;
                };

                AddItem(item);
            }
            #endregion 

            #region RemoveAfter
            {
                var item = new UIMenuListItem("Remove After Objective", StaticData.StaticLists.RemoveAfterList, actor.RemoveAfter);

                item.OnListChanged += (sender, index) =>
                {
                    actor.RemoveAfter = index;
                };

                AddItem(item);
            }
            #endregion 
            // TODO: Change NumberMenu to max num of objectives in mission

            // Note: if adding items before weapons, change item order in VehiclePropertiesMenu

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

            #region Accuracy
            {
                var listIndex = StaticData.StaticLists.AccuracyList.FindIndex(n => n == (dynamic)actor.Accuracy);
                var item = new UIMenuListItem("Accuracy", StaticData.StaticLists.AccuracyList, listIndex);

                item.OnListChanged += (sender, index) =>
                {
                    int newAmmo = int.Parse(((UIMenuListItem)sender).IndexToItem(index).ToString(), CultureInfo.InvariantCulture);
                    actor.Accuracy = newAmmo;
                };

                AddItem(item);
            }
            #endregion

            #region Relationship
            {
                var item = new UIMenuListItem("Relationship", StaticData.StaticLists.RelationshipGroups, actor.RelationshipGroup);

                item.OnListChanged += (sender, index) =>
                {
                    actor.RelationshipGroup = index;
                };

                AddItem(item);
            }
            #endregion

            #region Behaviour
            {
                var wpyItem = new UIMenuItem("Waypoints");

                {
                    var waypMenu = new WaypointEditor(actor);
                    BindMenuToItem(waypMenu.CreateWaypointMenu, wpyItem);

                    Vector3 camPos = new Vector3();
                    Rotator camRot = new Rotator();

                    wpyItem.Activated += (sender, selectedItem) =>
                    {
                        camPos = Editor.MainCamera.Position;
                        camRot = Editor.MainCamera.Rotation;

                        waypMenu.Enter();
                        Editor.WaypointEditor = waypMenu;
                    };

                    waypMenu.OnEditorExit += (sender, args) =>
                    {
                        Editor.WaypointEditor = null;
                        Editor.DisableControlEnabling = true;
                        if (camPos != new Vector3())
                        {
                            Editor.MainCamera.Position = camPos;
                            Editor.MainCamera.Rotation = camRot;
                        }
                    };
                }

                if (actor.Behaviour != 4) // Follow Waypoints
                    wpyItem.Enabled = false;

                var item = new UIMenuListItem("Behaviour", StaticData.StaticLists.Behaviour, actor.Behaviour);

                item.OnListChanged += (sender, index) =>
                {
                    actor.Behaviour = index;
                    wpyItem.Enabled = index == 4;
                };

                AddItem(item);
                AddItem(wpyItem);
            }
            #endregion

            #region FailOnDeath
            {
                var item = new UIMenuCheckboxItem("Mission Fail On Death", actor.FailMissionOnDeath);
                item.CheckboxEvent += (sender, @checked) =>
                {
                    actor.FailMissionOnDeath = @checked;
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