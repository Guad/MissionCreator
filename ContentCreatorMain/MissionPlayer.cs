using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using MissionCreator.SerializableData;
using MissionCreator.SerializableData.Objectives;
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

                GameFiber.StartNew(delegate
                {
                    while (CurrentStage != veh.ActivateAfter && IsMissionPlaying)
                    {
                        GameFiber.Yield();
                    }
                    CurrentObjectives.Add(veh);

                    var blip = newv.AttachBlip();
                    

                    if(veh.ObjectiveType == 0)
                    {
                        blip.Color = Color.DarkRed;
                        while (!newv.IsDead)
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
                    var ped = new Ped(Util.RequestModel(actor.ModelHash), actor.Position - new Vector3(0,0,1f), actor.Rotation.Yaw);
                    
                    ped.Rotation = actor.Rotation;
                    ped.Accuracy = actor.Accuracy;

                    var blip = ped.AttachBlip();
                    blip.Scale = 0.6f;

                    if (actor.WeaponHash != 0)
                        ped.GiveNewWeapon(actor.WeaponHash, actor.WeaponAmmo, true);

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

                    if (actor.Behaviour == 3)
                        ped.Tasks.FightAgainstClosestHatedTarget(100f);
                    else if (actor.Behaviour == 2)
                        NativeFunction.CallByName<uint>("TASK_GUARD_CURRENT_POSITION", ped.Handle.Value, 10, 10, 1);
                    else if (actor.Behaviour == 0)
                        ped.Tasks.Clear();

                    while (IsMissionPlaying && (actor.RemoveAfter == 0 || actor.RemoveAfter > CurrentStage))
                    {
                        if (actor.FailMissionOnDeath && ped.IsDead)
                        {
                            FailMission(reason: "An ally has died.");
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
                    ped.GiveNewWeapon(actor.WeaponHash, actor.WeaponAmmo, true);

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

                GameFiber.StartNew(delegate
                {
                    while (CurrentStage != actor.ActivateAfter && IsMissionPlaying)
                    {
                        GameFiber.Yield();
                    }

                    CurrentObjectives.Add(actor);
                    var blip = ped.AttachBlip();
                    blip.Scale = 0.6f;
                    blip.Color = Color.DarkRed;

                    ped.BlockPermanentEvents = false;

                    if (actor.Behaviour == 3)
                        ped.Tasks.FightAgainstClosestHatedTarget(100f);
                    else if (actor.Behaviour == 2)
                        NativeFunction.CallByName<uint>("TASK_GUARD_CURRENT_POSITION", ped.Handle.Value, 10, 10, 1);
                    else if(actor.Behaviour == 0)
                        ped.Tasks.Clear();

                
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