using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using MissionCreator.SerializableData;
using MissionCreator.SerializableData.Cutscenes;
using MissionCreator.SerializableData.Objectives;
using MissionCreator.SerializableData.Waypoints;
using MissionCreator.UI;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace MissionCreator
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

        private Model _oldModel;
        private Vector3 _oldPos;

        private void LoadInteriors()
        {
            var wait = CurrentMission.Interiors.Any(x => StaticData.IPLData.Database[x].Item1);
            
            foreach (string interior in CurrentMission.Interiors)
            {
                if(!StaticData.IPLData.Database.ContainsKey(interior)) continue;

                if(StaticData.IPLData.Database[interior].Item1)
                    Util.LoadOnlineMap();

                foreach (string s in StaticData.IPLData.Database[interior].Item2)
                {
                    Util.LoadInterior(s);
                }

                foreach (string s in StaticData.IPLData.Database[interior].Item3)
                {
                    Util.RemoveInterior(s);
                }
            }

          if(wait)
                GameFiber.Sleep(5000);
        }

        private void UnloadInteriors()
        {
            bool hasOnlineMap = false;
            foreach (string interior in CurrentMission.Interiors)
            {
                if (!StaticData.IPLData.Database.ContainsKey(interior)) continue;

                if (!hasOnlineMap && StaticData.IPLData.Database[interior].Item1)
                    hasOnlineMap = true;

                foreach (string s in StaticData.IPLData.Database[interior].Item3)
                {
                    Util.LoadInterior(s);
                }

                foreach (string s in StaticData.IPLData.Database[interior].Item2)
                {
                    Util.RemoveInterior(s);
                }
            }

            if(hasOnlineMap)
                Util.RemoveOnlineMap();
        }

        public void Load(MissionData mission)
        {
            CurrentStage = -1;
            CurrentObjectives = new List<SerializableObjective>();
            CurrentMission = mission;
            IsMissionPlaying = true;

            if (mission.Objectives.Count == 0)
            {
                Game.DisplayNotification("No spawnpoint found for stage 0.");
                AbortMission();
                return;
            }

            _oldModel = Game.LocalPlayer.Model;
            _oldPos = Game.LocalPlayer.Character.Position;
            
            GameFiber.StartNew(delegate
            {
                Game.FadeScreenOut(1000, true);
                LoadInteriors();

                World.Weather = CurrentMission.Weather;
                World.TimeOfDay = new TimeSpan(CurrentMission.Time, 0, 0);

                var res = UIMenu.GetScreenResolutionMantainRatio();

                var name = new ResText(CurrentMission.Name, new Point((int)res.Width - 100, (int)res.Height - 100), 0.7f, Color.WhiteSmoke, Common.EFont.HouseScript, ResText.Alignment.Right);
                name.Outline = true;

                GameFiber.StartNew(delegate
                {
                    DateTime start = DateTime.Now;
                    while (DateTime.Now.Subtract(start).TotalMilliseconds < 10000)
                    {
                        name.Draw();
                        GameFiber.Yield();
                    }
                });

                var startTime = Game.GameTime;

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

                    if (CurrentMission.TimeLimit.HasValue)
                    {
                        var elapsed = TimeSpan.FromMilliseconds(Convert.ToDouble((CurrentMission.TimeLimit.Value*1000) - (Game.GameTime - startTime)));
                        if(TimerBars != null)
                            TimerBars.UpdateValue("GLOBAL_TIME", "TIME", false, string.Format("{0:D2}:{1:D2}.{2:D3}", elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds));
                    }

                    if (CurrentMission.TimeLimit.HasValue && (Game.GameTime - startTime) > CurrentMission.TimeLimit*1000)
                    {
                        FailMission(reason: "You have run out of time.");
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

                    TimerBars?.Draw();
                    GameFiber.Yield();
                }

                UnloadInteriors();

            });

        }

        public void AbortMission(bool death = false)
        {
            GameFiber.StartNew(delegate
            {
                Game.LocalPlayer.Model = _oldModel;
                if (death)
                    Game.LocalPlayer.Character.Kill();
                else
                    Game.LocalPlayer.Character.Position = _oldPos - new Vector3(0, 0, 1);
            });

            Game.DisplaySubtitle("", 100);
            IsMissionPlaying = false;
            Game.FadeScreenIn(1000);
        }

        public void FailMission(bool death = false, string reason = "")
        {
            if (!death)
            {
                var screen = new MissionFailedScreen(reason);
                screen.Show();
                while (!screen.HasPressedContinue)
                {
                    screen.Draw();
                    GameFiber.Yield();
                }
            }
            AbortMission(death);
        }

        public void SucceedMission()
        {
            var missPassedScreen = new MissionPassedScreen(CurrentMission.Name, 100, MissionPassedScreen.Medal.Gold);
            missPassedScreen.AddItem("Author", CurrentMission.Author, MissionPassedScreen.TickboxState.None);
            missPassedScreen.Show();
            while (!missPassedScreen.HasPressedContinue)
            {
                missPassedScreen.Draw();
                GameFiber.Yield();
            }
            Game.FadeScreenOut(1000, true);
            AbortMission();
        }

        private void AdvanceStage()
        {
            TimerBars = new TimerBars();
            CurrentStage++;
            CurrentObjectives.Clear();


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
                    while (IsMissionPlaying && (veh.RemoveAfter == 0 || veh.RemoveAfter > CurrentStage))
                    {
                        if (veh.FailMissionOnDeath && newv.IsDead)
                        {
                            FailMission(reason: "The vehicle has been destroyed.");
                        }
                        GameFiber.Yield();
                    }

                    if(newv.IsValid())
                        newv.Delete();
                });
            }

            foreach (var veh in CurrentMission.Objectives.OfType<SerializableVehicleObjective>().Where(v => v.SpawnAfter == CurrentStage))
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

                var hasActivated = false;

                if (veh.ActivateAfter == CurrentStage)
                {
                    CurrentObjectives.Add(veh);
                    hasActivated = true;
                }

                GameFiber.StartNew(delegate
                {
                    if(!hasActivated)
                    {
                        while (CurrentStage != veh.ActivateAfter && IsMissionPlaying)
                        {
                            GameFiber.Yield();
                        }
                        CurrentObjectives.Add(veh);
                    }


                    var blip = newv.AttachBlip();
                    

                    if(veh.ObjectiveType == 0)
                    {
                        blip.Color = Color.DarkRed;
                        while (!newv.IsDead && IsMissionPlaying)
                        {
                            if (veh.ShowHealthBar)
                            {
                                TimerBars.UpdateValue(newv.Handle.Value.ToString(), veh.Name, true, (100f*newv.Health / veh.Health).ToString("###") + "%");
                            }
                            GameFiber.Yield();
                        }
                        TimerBars.UpdateValue(newv.Handle.Value.ToString(), veh.Name, true, "0%");
                    }

                    if (veh.ObjectiveType == 1)
                    {
                        blip.Color = Color.CornflowerBlue;
                        while (!Game.LocalPlayer.Character.IsInVehicle(newv, false) && IsMissionPlaying)
                        {
                            GameFiber.Yield();
                        }
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
                Game.LocalPlayer.Character.Position = sp.Position - new Vector3(0,0,1);
                Game.LocalPlayer.Character.Rotation = sp.Rotation;
                Game.LocalPlayer.Model = Util.RequestModel(sp.ModelHash);

                if (sp.WeaponHash != 0)
                    Game.LocalPlayer.Character.Inventory.GiveNewWeapon(sp.WeaponHash, (short)sp.WeaponAmmo, true);

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
                    var ped = new Ped(Util.RequestModel(actor.ModelHash), actor.Position - new Vector3(0,0,1f), actor.Rotation.Yaw);
                    
                    ped.Rotation = actor.Rotation;
                    ped.Accuracy = actor.Accuracy;

                    var blip = ped.AttachBlip();
                    blip.Scale = 0.6f;

                    if (actor.WeaponHash != 0)
                        ped.Inventory.GiveNewWeapon(actor.WeaponHash, (short)actor.WeaponAmmo, true);

                    ped.Health = actor.Health;
                    ped.Armor = actor.Armor;

                    if (actor.RelationshipGroup == 0)
                    {
                        ped.RelationshipGroup = StaticData.RelationshipGroups.Groups[1];
                        blip.Color = Color.DodgerBlue;

                        NativeFunction.CallByName<uint>("REMOVE_PED_FROM_GROUP", ped.Handle.Value);
                        NativeFunction.CallByName<uint>("SET_PED_AS_GROUP_MEMBER", ped.Handle.Value, NativeFunction.CallByName<int>("GET_PED_GROUP_INDEX", Game.LocalPlayer.Character.Handle.Value));
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
                    NativeFunction.CallByName<uint>("SET_PED_FIRING_PATTERN", ped.Handle.Value, 0xC6EE6B4C);

                    if (actor.Behaviour == 3)
                        ped.Tasks.FightAgainstClosestHatedTarget(100f);
                    else if (actor.Behaviour == 2)
                        NativeFunction.CallByName<uint>("TASK_GUARD_CURRENT_POSITION", ped.Handle.Value, 15f, 10f, true);
                    else if (actor.Behaviour == 0)
                        ped.Tasks.Clear();
                    else if (actor.Behaviour == 4)
                    {
                        GameFiber.StartNew(delegate
                        {
                            var wpyList = new List<SerializableWaypoint>(actor.Waypoints);
                            SerializableWaypoint currentWaypoint;
                            if (wpyList.Count > 0)
                                currentWaypoint = wpyList[0];

                            while (ped.IsValid() && ped.Exists() && ped.IsAlive && IsMissionPlaying && wpyList.Count > 0)
                            {
                                if (wpyList.Count == 0) break;
                                currentWaypoint = wpyList[0];
                                Task pedTask = null;
                                switch (currentWaypoint.Type)
                                {
                                    case WaypointTypes.Drive:
                                        if (ped.IsInAnyVehicle(true))
                                        {
                                            pedTask = ped.Tasks.DriveToPosition(currentWaypoint.Position,
                                                currentWaypoint.VehicleSpeed, (VehicleDrivingFlags)currentWaypoint.DrivingStyle);
                                        }
                                        break;
                                    case WaypointTypes.Run:
                                        {
                                            var heading = 0f;
                                            if (wpyList.Count >= 2)
                                            {
                                                heading =
                                                    Util.DirectionToRotation(wpyList[1].Position - currentWaypoint.Position)
                                                        .Z;
                                            }

                                            pedTask = ped.Tasks.FollowNavigationMeshToPosition(currentWaypoint.Position,
                                                heading, 2f, 0.3f, currentWaypoint.Duration == 0 ? -1 : (int)currentWaypoint.Duration);

                                        }
                                        break;
                                    case WaypointTypes.Walk:
                                        {
                                            var heading = 0f;
                                            if (wpyList.Count >= 2)
                                            {
                                                heading =
                                                    Util.DirectionToRotation(wpyList[1].Position - currentWaypoint.Position)
                                                        .Z;
                                            }

                                            pedTask = ped.Tasks.FollowNavigationMeshToPosition(currentWaypoint.Position,
                                                heading, 1f, 0.3f, currentWaypoint.Duration == 0 ? -1 : (int)currentWaypoint.Duration);

                                        }
                                        break;
                                    case WaypointTypes.ExitVehicle:
                                        if (ped.IsInAnyVehicle(true))
                                            pedTask = ped.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                                        break;
                                    case WaypointTypes.EnterVehicle:
                                        Vehicle[] vehs = World.GetAllVehicles().Where(v =>
                                        {
                                            if (v != null && v.IsValid())
                                                return v.Model.Hash == currentWaypoint.VehicleTargetModel;
                                            return false;
                                        }).OrderBy(v => (v.Position - ped.Position).Length()).ToArray();

                                        if (vehs.Any())
                                        {
                                            if ((vehs[0].Position - ped.Position).Length() > 10f)
                                            {
                                                pedTask = ped.Tasks.FollowNavigationMeshToPosition(vehs[0].Position, 0f,
                                                    3f,
                                                    5f);
                                                pedTask.WaitForCompletion(10000);
                                            }
                                            var seat = vehs[0].GetFreeSeatIndex();
                                            if (seat.HasValue)
                                                pedTask = ped.Tasks.EnterVehicle(vehs[0], seat.Value);
                                        }
                                        break;
                                    case WaypointTypes.Wait:
                                        pedTask = ped.Tasks.StandStill(currentWaypoint.Duration);
                                        break;
                                    case WaypointTypes.Wander:
                                        pedTask = ped.Tasks.Wander();
                                        break;
                                    case WaypointTypes.CruiseWithVehicle:
                                        if (ped.IsInAnyVehicle(false))
                                            pedTask = ped.Tasks.CruiseWithVehicle(ped.CurrentVehicle,
                                                currentWaypoint.VehicleSpeed, (VehicleDrivingFlags)currentWaypoint.DrivingStyle);

                                        
                                        break;
                                    case WaypointTypes.Shoot:
                                        pedTask = null;
                                        NativeFunction.CallByName<uint>("TASK_SHOOT_AT_COORD", ped.Handle.Value,
                                            currentWaypoint.Position.X, currentWaypoint.Position.Y,
                                            currentWaypoint.Position.Z, currentWaypoint.Duration, 0xC6EE6B4C);
                                        GameFiber.Sleep(currentWaypoint.Duration);
                                        break;
                                    case WaypointTypes.Animation:
                                        pedTask = ped.Tasks.PlayAnimation(currentWaypoint.AnimDict,
                                            currentWaypoint.AnimName, 8f, AnimationFlags.None);
                                        break;
                                }
                                pedTask?.WaitForCompletion(currentWaypoint.Duration == 0 ? -1 : currentWaypoint.Duration);
                                if (wpyList.Count > 0)
                                    wpyList.RemoveAt(0);
                                GameFiber.Yield();
                            }
                        });
                    }

                    while (IsMissionPlaying && (actor.RemoveAfter == 0 || actor.RemoveAfter > CurrentStage) && !ped.IsDead)
                    {
                        GameFiber.Yield();
                    }

                    if (actor.FailMissionOnDeath && ped.IsDead)
                    {
                        FailMission(reason: "An ally has died.");
                    }

                    if (blip.IsValid())
                        blip.Delete();

                    while(IsMissionPlaying && (actor.RemoveAfter == 0 || actor.RemoveAfter > CurrentStage))
                        GameFiber.Yield();

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

                    while (IsMissionPlaying && (o.RemoveAfter == 0 || o.RemoveAfter > CurrentStage))
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
                    var obj = NativeFunction.CallByName<uint>("CREATE_PICKUP_ROTATE", pickup.PickupHash, pickup.Position.X,
                        pickup.Position.Y, pickup.Position.Z, pickup.Rotation.Pitch, pickup.Rotation.Roll, pickup.Rotation.Yaw,
                        1, pickup.Ammo, 2, 1, 0);

                    int counter = 0;
                    while (IsMissionPlaying && (pickup.RemoveAfter == 0 || pickup.RemoveAfter > CurrentStage))
                    {
                        var alpha = 40 * (Math.Sin(Util.DegToRad(counter % 180)));
                        Util.DrawMarker(28, pickup.Position, new Vector3(), new Vector3(0.75f, 0.75f, 0.75f), Color.FromArgb((int)alpha, 10, 10, 230));
                        counter += 5;
                        if (counter >= 360)
                            counter = 0;

                        if ((pickup.Position - Game.LocalPlayer.Character.Position).Length() < 1f && pickup.Respawn)
                        {
                            NativeFunction.CallByName<uint>("REMOVE_PICKUP", obj);
                            while((pickup.Position - Game.LocalPlayer.Character.Position).Length() < 3f)
                            { GameFiber.Yield();}
                            obj = NativeFunction.CallByName<uint>("CREATE_PICKUP_ROTATE", pickup.PickupHash, pickup.Position.X,
                            pickup.Position.Y, pickup.Position.Z, pickup.Rotation.Pitch, pickup.Rotation.Roll, pickup.Rotation.Yaw,
                            1, pickup.Ammo, 2, 1, 0);
                        }
                        else if ((pickup.Position - Game.LocalPlayer.Character.Position).Length() < 1f && !pickup.Respawn)
                            break;
                        GameFiber.Yield();
                    }
                    NativeFunction.CallByName<uint>("REMOVE_PICKUP", obj);
                });
            }

            foreach (var actor in CurrentMission.Objectives.OfType<SerializableActorObjective>().Where(v => v.SpawnAfter == CurrentStage))
            {
                var ped = new Ped(Util.RequestModel(actor.ModelHash), actor.Position - new Vector3(0, 0, 1f), actor.Rotation.Yaw);

                ped.Rotation = actor.Rotation;
                ped.Accuracy = actor.Accuracy;

                
                if (actor.WeaponHash != 0)
                    ped.Inventory.GiveNewWeapon(actor.WeaponHash, (short)actor.WeaponAmmo, true);

                NativeFunction.CallByName<uint>("SET_PED_FIRING_PATTERN", ped.Handle.Value, 0xC6EE6B4C);

                ped.Health = actor.Health;
                ped.MaxHealth = actor.Health;
                ped.Armor = actor.Armor;

                if (actor.RelationshipGroup == 0)
                {
                    NativeFunction.CallByName<uint>("REMOVE_PED_FROM_GROUP", ped.Handle.Value);
                    NativeFunction.CallByName<uint>("SET_PED_AS_GROUP_MEMBER", ped.Handle.Value, NativeFunction.CallByName<int>("GET_PED_GROUP_INDEX", Game.LocalPlayer.Character.Handle.Value));
                    ped.RelationshipGroup = StaticData.RelationshipGroups.Groups[1];
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

                var hasActivated = false;

                if (actor.ActivateAfter == CurrentStage)
                {
                    CurrentObjectives.Add(actor);
                    hasActivated = true;
                }

                GameFiber.StartNew(delegate
                {
                    
                    if(!hasActivated)
                    {
                        while (CurrentStage != actor.ActivateAfter && IsMissionPlaying)
                        {
                            GameFiber.Yield();
                        }

                        CurrentObjectives.Add(actor);
                    }

                    var blip = ped.AttachBlip();
                    blip.Scale = 0.6f;
                    blip.Color = Color.DarkRed;

                    ped.BlockPermanentEvents = false;

                    if (actor.Behaviour == 3)
                        ped.Tasks.FightAgainstClosestHatedTarget(100f);
                    else if (actor.Behaviour == 2)
                        NativeFunction.CallByName<uint>("TASK_GUARD_CURRENT_POSITION", ped.Handle.Value, 15f, 10f, true);
                    else if(actor.Behaviour == 0)
                        ped.Tasks.Clear();
                    else if (actor.Behaviour == 4)
                    {
                        GameFiber.StartNew(delegate
                        {
                            var wpyList = new List<SerializableWaypoint>(actor.Waypoints);
                            SerializableWaypoint currentWaypoint;
                            if(wpyList.Count > 0)
                                currentWaypoint = wpyList[0];

                            while (ped.IsValid() && ped.Exists() && ped.IsAlive && IsMissionPlaying && wpyList.Count > 0)
                            {
                                if (wpyList.Count == 0) break;
                                currentWaypoint = wpyList[0];
                                Task pedTask = null;
                                switch (currentWaypoint.Type)
                                {
                                    case WaypointTypes.Drive:
                                        if (ped.IsInAnyVehicle(true))
                                        {
                                            pedTask = ped.Tasks.DriveToPosition(currentWaypoint.Position,
                                                currentWaypoint.VehicleSpeed, (VehicleDrivingFlags)currentWaypoint.DrivingStyle);
                                        }
                                        break;
                                    case WaypointTypes.Run:
                                        {
                                            var heading = 0f;
                                            if (wpyList.Count >= 2)
                                            {
                                                heading =
                                                    Util.DirectionToRotation(wpyList[1].Position - currentWaypoint.Position)
                                                        .Z;
                                            }

                                            pedTask = ped.Tasks.FollowNavigationMeshToPosition(currentWaypoint.Position,
                                                heading, 2f, 0.3f, currentWaypoint.Duration == 0 ? -1 : (int)currentWaypoint.Duration);

                                        }
                                        break;
                                    case WaypointTypes.CruiseWithVehicle:
                                        if (ped.IsInAnyVehicle(true))
                                            pedTask = ped.Tasks.CruiseWithVehicle(ped.CurrentVehicle,
                                                currentWaypoint.VehicleSpeed, (VehicleDrivingFlags)currentWaypoint.DrivingStyle);
                                        break;
                                    case WaypointTypes.Walk:
                                        {
                                            var heading = 0f;
                                            if (wpyList.Count >= 2)
                                            {
                                                heading =
                                                    Util.DirectionToRotation(wpyList[1].Position - currentWaypoint.Position)
                                                        .Z;
                                            }

                                            pedTask = ped.Tasks.FollowNavigationMeshToPosition(currentWaypoint.Position,
                                                heading, 1f, 0.3f, currentWaypoint.Duration == 0 ? -1 : (int)currentWaypoint.Duration);

                                        }
                                        break;
                                    case WaypointTypes.ExitVehicle:
                                        if (ped.IsInAnyVehicle(true))
                                            pedTask = ped.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                                        break;
                                    case WaypointTypes.EnterVehicle:
                                        Vehicle[] vehs = World.GetAllVehicles().Where(v =>
                                        {
                                            if (v != null && v.IsValid())
                                                return v.Model.Hash == currentWaypoint.VehicleTargetModel;
                                            return false;
                                        }).OrderBy(v => (v.Position - ped.Position).Length()).ToArray();

                                        if (vehs.Any())
                                        {
                                            if ((vehs[0].Position - ped.Position).Length() > 10f)
                                            {
                                                pedTask = ped.Tasks.FollowNavigationMeshToPosition(vehs[0].Position, 0f,
                                                    3f,
                                                    5f);
                                                pedTask.WaitForCompletion(10000);
                                            }
                                            var seat = vehs[0].GetFreeSeatIndex();
                                            if (seat.HasValue)
                                                pedTask = ped.Tasks.EnterVehicle(vehs[0], seat.Value);
                                        }
                                        break;
                                    case WaypointTypes.Wait:
                                        pedTask = ped.Tasks.StandStill(currentWaypoint.Duration);
                                        break;
                                    case WaypointTypes.Wander:
                                        pedTask = ped.Tasks.Wander();
                                        break;
                                    case WaypointTypes.Shoot:
                                        pedTask = null;
                                        NativeFunction.CallByName<uint>("TASK_SHOOT_AT_COORD", ped.Handle.Value,
                                            currentWaypoint.Position.X, currentWaypoint.Position.Y,
                                            currentWaypoint.Position.Z, currentWaypoint.Duration, 0xC6EE6B4C);
                                        GameFiber.Sleep(currentWaypoint.Duration);
                                        break;
                                    case WaypointTypes.Animation:
                                        pedTask = ped.Tasks.PlayAnimation(currentWaypoint.AnimDict,
                                            currentWaypoint.AnimName, 8f, AnimationFlags.None);
                                        break;
                                }
                                pedTask?.WaitForCompletion(currentWaypoint.Duration == 0 ? -1 : currentWaypoint.Duration);
                                if(wpyList.Count > 0)
                                    wpyList.RemoveAt(0);
                                GameFiber.Yield();
                            }
                        });
                    }

                    while (!ped.IsDead && IsMissionPlaying)
                    {
                        if (actor.ShowHealthBar)
                        {
                            TimerBars.UpdateValue(ped.Handle.Value.ToString(), actor.Name, true, (100f*ped.Health/actor.Health).ToString("###") + "%");
                        }
                        GameFiber.Yield();
                    }

                    if (actor.ShowHealthBar)
                    {
                        TimerBars.UpdateValue(ped.Handle.Value.ToString(), actor.Name, true, "0%");
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
                    var obj = NativeFunction.CallByName<uint>("CREATE_PICKUP_ROTATE", pickup.PickupHash, pickup.Position.X,
                        pickup.Position.Y, pickup.Position.Z, pickup.Rotation.Pitch, pickup.Rotation.Roll, pickup.Rotation.Yaw,
                        1, pickup.Ammo, 2, 1, 0);

                    var blip = new Blip(pickup.Position);
                    blip.Scale = 0.7f;
                    blip.Color = Color.DodgerBlue;

                    var counter = 0;
                    while ((pickup.Position - Game.LocalPlayer.Character.Position).Length() > 1f && IsMissionPlaying)
                    {
                        var alpha = 40 * (Math.Sin(Util.DegToRad(counter % 180)));
                        Util.DrawMarker(28, pickup.Position, new Vector3(), new Vector3(0.75f, 0.75f, 0.75f), Color.FromArgb((int)alpha, 230, 10, 10));
                        counter += 5;
                        if (counter >= 360)
                            counter = 0;
                        GameFiber.Yield();
                    }

                    if(blip != null && blip.IsValid())
                        blip.Delete();
                    
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


            if (CurrentMission.Cutscenes.Any(c => c.PlayAt == CurrentStage))
            {
                var origPos = Game.LocalPlayer.Character.Position;
                var origRot = Game.LocalPlayer.Character.Rotation;

                Game.FadeScreenIn(100, true);

                Game.LocalPlayer.Character.Opacity = 0f;
                Game.LocalPlayer.Character.IsPositionFrozen = true;

                var cutscene = CurrentMission.Cutscenes.First(c => c.PlayAt == CurrentStage);
                var camLeft = new List<SerializableCamera>(cutscene.Cameras);
                var subLeft = new List<SerializableSubtitle>(cutscene.Subtitles);

                var startTime = Game.GameTime;
                Camera mainCam = null;

                SerializableCamera currentCam = null;
                uint lerpStart = 0;


                while ((Game.GameTime - startTime) < cutscene.Length)
                {
                    var ct = (Game.GameTime - startTime);

                    if (camLeft.Any())
                    {
                        if (camLeft[0].PositionInTime <= ct)
                        {
                            if (mainCam == null || !mainCam.IsValid())
                            {
                                Camera.DeleteAllCameras();
                                mainCam = new Camera(true);
                            }

                            Game.LocalPlayer.HasControl = false;
                            mainCam.Position = cutscene.Cameras[0].Position;
                            mainCam.Rotation = cutscene.Cameras[0].Rotation;
                            currentCam = camLeft[0];
                            camLeft.RemoveAt(0);
                            lerpStart = Game.GameTime;
                            Game.LocalPlayer.Character.Position = mainCam.Position;
                        }
                        else if (currentCam != null)
                        {
                            // Advance cam pos
                            if (currentCam.InterpolationStyle == InterpolationStyle.Linear)
                            {
                                mainCam.Position = Util.LerpVector(currentCam.Position, camLeft[0].Position,
                                    Util.LinearLerp, Game.GameTime - lerpStart,
                                    camLeft[0].PositionInTime - currentCam.PositionInTime);

                                mainCam.Rotation =
                                    Util.LerpVector(currentCam.Rotation.ToVector(), camLeft[0].Rotation.ToVector(),
                                        Util.LinearLerp, Game.GameTime - lerpStart,
                                        camLeft[0].PositionInTime - currentCam.PositionInTime).ToRotator();
                                Game.LocalPlayer.Character.Position = mainCam.Position;
                            }
                            else if (currentCam.InterpolationStyle == InterpolationStyle.Smooth)
                            {
                                mainCam.Position = Util.LerpVector(currentCam.Position, camLeft[0].Position,
                                    Util.QuadraticLerp, Game.GameTime - lerpStart,
                                    camLeft[0].PositionInTime - currentCam.PositionInTime);

                                mainCam.Rotation =
                                    Util.LerpVector(currentCam.Rotation.ToVector(), camLeft[0].Rotation.ToVector(),
                                        Util.QuadraticLerp, Game.GameTime - lerpStart,
                                        camLeft[0].PositionInTime - currentCam.PositionInTime).ToRotator();
                                Game.LocalPlayer.Character.Position = mainCam.Position;
                            }
                        }
                    }
                    else if(currentCam != null && mainCam != null)
                    {
                        mainCam.Position = currentCam.Position;
                        mainCam.Rotation = currentCam.Rotation;
                        Game.LocalPlayer.Character.Position = mainCam.Position;
                    }

                    if (subLeft.Any())
                    {
                        if (subLeft[0].PositionInTime <= ct)
                        {
                            Game.DisplaySubtitle(subLeft[0].Content, subLeft[0].DurationInMs);
                            subLeft.RemoveAt(0);
                        }
                    }
                    GameFiber.Yield();
                }

                mainCam.Active = false;
                Game.LocalPlayer.HasControl = true;
                Game.LocalPlayer.Character.IsPositionFrozen = false;
                Game.LocalPlayer.Character.Position = origPos;
                Game.LocalPlayer.Character.Rotation = origRot;
                Game.LocalPlayer.Character.Opacity = 1f;
                Game.FadeScreenOut(100);
            }


            if (!string.IsNullOrEmpty(CurrentMission.ObjectiveNames[CurrentStage]))
            {
                Game.DisplaySubtitle(CurrentMission.ObjectiveNames[CurrentStage], 10000);
            }

            Game.FadeScreenIn(1);
        }

        public void Tick()
        {
            
        }
    }
}