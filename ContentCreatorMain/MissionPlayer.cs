using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ContentCreator.SerializableData;
using ContentCreator.SerializableData.Objectives;
using ContentCreator.UI;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace ContentCreator
{
    public class MissionPlayer
    {
        public MissionPlayer()
        {
        }

        #region Public Vars
        public bool IsMissionPlaying { get; set; }
        public MissionData CurrentMission { get; set; }

        public int CurrentStage { get; set; }
        public List<SerializableObjective> CurrentObjectives;

        public TimerBars TimerBars;
        #endregion

        private Group _playerGroup;

        public void Load(MissionData mission)
        {
            CurrentStage = -1;
            CurrentObjectives = new List<SerializableObjective>();
            CurrentMission = mission;
            IsMissionPlaying = true;

            _playerGroup = new Group(Game.LocalPlayer.Character);

            if (mission.Objectives.Count == 0)
            {
                Game.DisplayNotification("No spawnpoint found for stage 0.");
                AbortMission();
                return;
            }

            World.Weather = CurrentMission.Weather;
            World.TimeOfDay = new TimeSpan(CurrentMission.Time, 0, 0);

            GameFiber.StartNew(delegate
            {
                Game.FadeScreenOut(1000, true);
                
                while (IsMissionPlaying)
                {
                    Game.MaxWantedLevel = CurrentMission.MaxWanted;
                    if (Game.LocalPlayer.WantedLevel < CurrentMission.MinWanted)
                        Game.LocalPlayer.WantedLevel = CurrentMission.MinWanted;

                    if (Game.LocalPlayer.Character.IsDead)
                    {
                        FailMission(true);
                        break;
                    }

                    if (CurrentObjectives.Count == 0)
                    {
                        AdvanceStage();
                        if (!CurrentMission.Objectives.Any(o => o.ActivateAfter >= CurrentStage))
                        {
                            SucceedMission();
                        }
                    }

                    TimerBars.Draw();
                    GameFiber.Yield();
                }
            });

        }

        public void AbortMission(bool death = false)
        {
            GameFiber.StartNew(delegate
            {
                Game.LocalPlayer.Model = Util.RequestModel(0xD7114C9);
                if(death)
                    Game.LocalPlayer.Character.Kill();
            });
            Game.DisplayNotification("Mission over.");
            IsMissionPlaying = false;
            Game.FadeScreenIn(100);
        }

        public void FailMission(bool death = false)
        {
            Game.DisplayNotification("Mission Failed!");
            AbortMission(death);
        }

        public void SucceedMission()
        {
            Game.DisplayNotification("Mission Successful!");
            AbortMission();
        }

        private void AdvanceStage()
        {
            TimerBars = new TimerBars();
            CurrentStage++;
            CurrentObjectives.Clear();
        if (CurrentMission.Cutscenes.Any(c => c.PlayAt == CurrentStage))
            {
                // TODO: Implement cutscenes.
            }

            foreach (var veh in CurrentMission.Vehicles.Where(v => v.SpawnAfter == CurrentStage))
            {
                var newv = new Vehicle(Util.RequestModel(veh.ModelHash), veh.Position)
                {
                    PrimaryColor = Color.FromArgb((int)veh.PrimaryColor.X, (int)veh.PrimaryColor.Y,
                    (int)veh.PrimaryColor.Z),
                    SecondaryColor = Color.FromArgb((int)veh.SecondaryColor.X, (int)veh.SecondaryColor.Y,
                    (int)veh.SecondaryColor.Z),
                };
                newv.Health = veh.Health;
                newv.Rotation = veh.Rotation;
                GameFiber.StartNew(delegate
                {
                    while (IsMissionPlaying && (veh.RemoveAfter == 0 || veh.RemoveAfter < CurrentStage))
                    {
                        if (veh.FailMissionOnDeath && newv.IsDead)
                        {
                            FailMission();
                        }
                        GameFiber.Yield();
                    }

                    if(newv.IsValid())
                        newv.Delete();
                });
            }

            foreach (var veh in CurrentMission.Objectives.OfType<SerializableVehicleObjective>().Where(v => v.ActivateAfter == CurrentStage))
            {
                CurrentObjectives.Add(veh);
                var newv = new Vehicle(Util.RequestModel(veh.ModelHash), veh.Position)
                {
                    PrimaryColor = Color.FromArgb((int)veh.PrimaryColor.X, (int)veh.PrimaryColor.Y,
                    (int)veh.PrimaryColor.Z),
                    SecondaryColor = Color.FromArgb((int)veh.SecondaryColor.X, (int)veh.SecondaryColor.Y,
                    (int)veh.SecondaryColor.Z),
                };
                newv.Health = veh.Health;
                newv.Rotation = veh.Rotation;
                var blip = newv.AttachBlip();
                blip.Color = Color.CornflowerBlue;
                GameFiber.StartNew(delegate
                {
                    /*
                    while (!newv.IsDead)
                    {
                        if (veh.ShowHealthBar)
                        {
                            TimerBars.UpdateValue(newv.Handle.Value.ToString(), veh.Name, true, (float)newv.Health / veh.Health);
                        }
                        GameFiber.Yield();
                    }*/

                    while (!Game.LocalPlayer.Character.IsInVehicle(newv, false) && IsMissionPlaying)
                    {
                        GameFiber.Yield();
                    }
                    CurrentObjectives.Remove(veh);

                    if(blip.IsValid())
                        blip.Delete();

                    while (IsMissionPlaying)
                        GameFiber.Yield();


                    if (newv.IsValid())
                        newv.Delete();

                });
            }

            if (CurrentMission.Spawnpoints.Any(s => s.SpawnAfter == CurrentStage))
            {
                var sp = CurrentMission.Spawnpoints.First(s => s.SpawnAfter == CurrentStage);
                Game.FadeScreenOut(100, true);
                Game.LocalPlayer.Character.Position = sp.Position;
                Game.LocalPlayer.Character.Rotation = sp.Rotation;
                Game.LocalPlayer.Model = Util.RequestModel(sp.ModelHash);

                if (sp.WeaponHash != 0)
                    Game.LocalPlayer.Character.GiveNewWeapon(sp.WeaponHash, sp.WeaponAmmo, true);

                Game.LocalPlayer.Character.Health = sp.Health;
                Game.LocalPlayer.Character.Armor = sp.Armor;
                    
                if (sp.SpawnInVehicle)
                {
                    var vehList = Game.LocalPlayer.Character.GetNearbyVehicles(15).OrderBy(v => (Game.LocalPlayer.Character.Position - v.Position).Length());
                    Game.LocalPlayer.Character.WarpIntoVehicle(vehList.ToList()[0], sp.VehicleSeat);
                }
                    
                Game.FadeScreenIn(500, false);
            }

            foreach (var actor in CurrentMission.Actors.Where(v => v.SpawnAfter == CurrentStage))
            {
                GameFiber.StartNew(delegate
                {
                    var ped = new Ped(Util.RequestModel(actor.ModelHash), actor.Position, actor.Rotation.Yaw);
                        
                    ped.Rotation = actor.Rotation;

                    var blip = ped.AttachBlip();
                    blip.Scale = 0.6f;

                    if (actor.WeaponHash != 0)
                        ped.GiveNewWeapon(actor.WeaponHash, actor.WeaponAmmo, true);

                    ped.Health = actor.Health;
                    ped.Armor = actor.Armor;

                    if (actor.RelationshipGroup == 0)
                    {
                        _playerGroup.AddMember(ped);
                        blip.Color = Color.DodgerBlue;
                    }
                    else
                    {
                        ped.RelationshipGroup = StaticData.RelationshipGroups.Groups[actor.RelationshipGroup];
                        if(actor.RelationshipGroup == 4 || actor.RelationshipGroup == 5)
                            blip.Color = Color.DarkRed;
                    }

                    if (actor.SpawnInVehicle)
                    {
                        var vehList = ped.GetNearbyVehicles(15).OrderBy(v => (ped.Position - v.Position).Length());
                        ped.WarpIntoVehicle(vehList.ToList()[0], actor.VehicleSeat);
                    }

                    ped.BlockPermanentEvents = false;

                    if (actor.Behaviour == 3)
                        ped.Tasks.FightAgainstClosestHatedTarget(100f);
                    else if (actor.Behaviour == 2)
                        NativeFunction.CallByName<uint>("TASK_GUARD_CURRENT_POSITION", ped.Handle.Value, 10, 10, 1);

                    while (IsMissionPlaying && (actor.RemoveAfter == 0 || actor.RemoveAfter < CurrentStage))
                    {
                        if (actor.FailMissionOnDeath && ped.IsDead)
                        {
                            FailMission();
                        }
                        GameFiber.Yield();
                    }
                    if(blip.IsValid())
                        blip.Delete();

                    if(ped.IsValid())
                        ped.Delete();
                });
            }

            foreach (var o in CurrentMission.Objects.Where(v => v.SpawnAfter == CurrentStage))
            {
                GameFiber.StartNew(delegate
                {
                    var prop = new Rage.Object(Util.RequestModel(o.ModelHash), o.Position);
                    prop.Position = o.Position;
                    prop.Rotation = o.Rotation;

                    while (IsMissionPlaying && (o.RemoveAfter == 0 || o.RemoveAfter < CurrentStage))
                    {
                        GameFiber.Yield();
                    }

                    if(prop.IsValid())
                        prop.Delete();
                });
            }

            foreach (var pickup in CurrentMission.Pickups.Where(v => v.SpawnAfter == CurrentStage))
            {
                GameFiber.StartNew(delegate
                {
                    var obj = NativeFunction.CallByName<uint>("CREATE_PICKUP", pickup.PickupHash, pickup.Position.X,
                        pickup.Position.Y, pickup.Position.Z, -1, pickup.Ammo, 1, 0);

                    while (IsMissionPlaying && (pickup.RemoveAfter == 0 || pickup.RemoveAfter < CurrentStage))
                    {
                        if (NativeFunction.CallByName<bool>("HAS_PICKUP_BEEN_COLLECTED", obj) && pickup.Respawn)
                        {
                            NativeFunction.CallByName<uint>("REMOVE_PICKUP", obj);
                            obj = NativeFunction.CallByName<uint>("CREATE_PICKUP", pickup.PickupHash, pickup.Position.X,
                            pickup.Position.Y, pickup.Position.Z, -1, pickup.Ammo, 1, 0);
                        }
                        else if (NativeFunction.CallByName<bool>("HAS_PICKUP_BEEN_COLLECTED", obj) && !pickup.Respawn)
                            break;
                        GameFiber.Yield();
                    }
                    NativeFunction.CallByName<uint>("REMOVE_PICKUP", obj);
                });
            }

            foreach (var actor in CurrentMission.Objectives.OfType<SerializableActorObjective>().Where(v => v.ActivateAfter == CurrentStage))
            {
                CurrentObjectives.Add(actor);

                var ped = new Ped(Util.RequestModel(actor.ModelHash), actor.Position, actor.Rotation.Yaw);

                ped.Rotation = actor.Rotation;

                var blip = ped.AttachBlip();
                blip.Scale = 0.6f;
                blip.Color = Color.DarkRed;

                if (actor.WeaponHash != 0)
                    ped.GiveNewWeapon(actor.WeaponHash, actor.WeaponAmmo, true);

                ped.Health = actor.Health;
                ped.MaxHealth = actor.Health;
                ped.Armor = actor.Armor;

                if (actor.RelationshipGroup == 0)
                {
                    _playerGroup.AddMember(ped);
                }
                else
                {
                    ped.RelationshipGroup = StaticData.RelationshipGroups.Groups[actor.RelationshipGroup];
                }

                if (actor.SpawnInVehicle)
                {
                    var vehList = ped.GetNearbyVehicles(15).OrderBy(v => (ped.Position - v.Position).Length());
                    ped.WarpIntoVehicle(vehList.ToList()[0], actor.VehicleSeat);
                }

                ped.BlockPermanentEvents = false;

                if (actor.Behaviour == 3)
                    ped.Tasks.FightAgainstClosestHatedTarget(100f);
                else if (actor.Behaviour == 2)
                    NativeFunction.CallByName<uint>("TASK_GUARD_CURRENT_POSITION", ped.Handle.Value, 10, 10, 1);

                GameFiber.StartNew(delegate
                {
                    while (!ped.IsDead && IsMissionPlaying)
                    {
                        if (actor.ShowHealthBar)
                        {
                            TimerBars.UpdateValue(ped.Handle.Value.ToString(), actor.Name, true, (float)ped.Health/actor.Health);
                        }
                        GameFiber.Yield();
                    }

                    CurrentObjectives.Remove(actor);
                    if(blip.IsValid())
                        blip.Delete();

                    while(IsMissionPlaying)
                        GameFiber.Yield();
                        

                    if (ped.IsValid())
                        ped.Delete();
                });
            }

            foreach (var pickup in CurrentMission.Objectives.OfType<SerializablePickupObjective>().Where(v => v.ActivateAfter == CurrentStage))
            {
                CurrentObjectives.Add(pickup);
                GameFiber.StartNew(delegate
                {
                    var obj = NativeFunction.CallByName<uint>("CREATE_PICKUP", pickup.PickupHash, pickup.Position.X,
                        pickup.Position.Y, pickup.Position.Z, -1, pickup.Ammo, 1, 0);

                    while (!NativeFunction.CallByName<bool>("HAS_PICKUP_BEEN_COLLECTED", obj) && IsMissionPlaying)
                    {
                        GameFiber.Yield();
                    }

                    NativeFunction.CallByName<uint>("REMOVE_PICKUP", obj);
                    CurrentObjectives.Remove(pickup);
                });
            }

            foreach (var mark in CurrentMission.Objectives.OfType<SerializableMarker>().Where(v => v.ActivateAfter == CurrentStage))
            {
                CurrentObjectives.Add(mark);
                GameFiber.StartNew(delegate
                {
                    var bColor = Color.FromArgb(mark.Alpha, (int) mark.Color.X, (int) mark.Color.Y, (int) mark.Color.Z);
                    var blip = new Blip(mark.Position);
                    blip.Color = bColor;
                    blip.EnableRoute(bColor);
                    while ((mark.Position - Game.LocalPlayer.Character.Position).Length() > 1.5f && IsMissionPlaying)
                    {
                        Util.DrawMarker(mark.Type, mark.Position, new Vector3(mark.Rotation.Pitch, mark.Rotation.Roll, mark.Rotation.Yaw), mark.Scale,
                        bColor);
                        GameFiber.Yield();
                    }
                    if(blip.IsValid())
                        blip.Delete();
                    CurrentObjectives.Remove(mark);
                });
            }

            if (!string.IsNullOrEmpty(CurrentMission.ObjectiveNames[CurrentStage]))
            {
                Game.DisplaySubtitle(CurrentMission.ObjectiveNames[CurrentStage], 10000);
            }
        }

        public void Tick()
        {
            
        }
    }
}