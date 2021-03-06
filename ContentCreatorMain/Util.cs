﻿using System;
using Rage;
using Rage.Native;
using System.Drawing;
using RAGENativeUI;

namespace MissionCreator
{
    public static class Util
    {
        public static bool IsGamepadEnabled => !NativeFunction.CallByHash<bool>(0xA571D46727E2B718, 0);

        public static void LoadOnlineMap()
        {
            NativeFunction.CallByHash<uint>(0x0888C3502DBBEEF5);
            NativeFunction.CallByHash<uint>(0x9BAE5AD2508DF078, 1);
        }

        public static void RemoveOnlineMap()
        {
            NativeFunction.CallByHash<uint>(0xD7C10C4A637992C9);
        }

        public static void LoadInterior(string interior)
        {
            NativeFunction.CallByName<uint>("REQUEST_IPL", interior);
        }

        public static void RemoveInterior(string interior)
        {
            NativeFunction.CallByName<uint>("REMOVE_IPL", interior);
        }

        public static string GetControlButtonId(GameControl cont)
        {
            return (string)NativeFunction.CallByHash(0x0499D7B09FC9B407, typeof (string), 2, (int) cont, 0);
        }

        public static void DrawMarker(int type, Vector3 pos, Vector3 rot, Vector3 scale, Color color)
        {
            NativeFunction.CallByName<uint>("DRAW_MARKER", type, pos.X, pos.Y, pos.Z, 0f, 0f, 0f,
                    rot.X, rot.Y, rot.Z, scale.X, scale.Y, scale.Z, (int)color.R, (int)color.G, (int)color.B, (int)color.A, false, false,
                    2, false, false, false, false);
        }

        public unsafe static void GetModelDimensions(Model model, out Vector3 minimum, out Vector3 maximum)
        {
            Vector3 min;
            Vector3 max;
            NativeFunction.CallByName<uint>("GET_MODEL_DIMENSIONS", model.Hash, &min, &max);
            minimum = min;
            maximum = max;
        }

        public static Model RequestModel(uint hash)
        {
            var model = new Model(hash);
            while (!model.IsLoaded)
            {
                model.Load();
                model.LoadCollision();
                GameFiber.Yield();
            }
            return model;
        }

        public static Model RequestModel(string hash)
        {
            var model = new Model(hash);
            while (!model.IsLoaded)
            {
                model.Load();
                model.LoadCollision();
                GameFiber.Yield();
            }
            return model;
        }

        public static bool IsPed(this Entity ent)
        {
            return NativeFunction.CallByName<bool>("IS_ENTITY_A_PED", ent.Handle.Value);
        }

        public static bool IsVehicle(this Entity ent)
        {
            return NativeFunction.CallByName<bool>("IS_ENTITY_A_VEHICLE", ent.Handle.Value);
        }

        public static bool IsObject(this Entity ent)
        {
            return NativeFunction.CallByName<bool>("IS_ENTITY_AN_OBJECT", ent.Handle.Value);
        }
        

        public static string GetUserInput(UIMenu father)
        {
            father.ResetKey(Common.MenuControls.Select);
            NativeFunction.CallByName<uint>("DISPLAY_ONSCREEN_KEYBOARD", true, "FMMC_KEY_TIP8", "", "", "", "", "", 255);
            int result = 0;
            while (result == 0)
            {
                NativeFunction.CallByName<uint>("DISABLE_ALL_CONTROL_ACTIONS", 0);
                result = NativeFunction.CallByHash<int>(0x0CF2B696BBF945AE);
                Game.DisplaySubtitle("Spin", 50);
                GameFiber.Yield();
            }
            father.SetKey(Common.MenuControls.Select, GameControl.FrontendAccept);
            if (result == 2)
                return null;
            return (string) NativeFunction.CallByName("GET_ONSCREEN_KEYBOARD_RESULT", typeof (string));
        }

        public static bool IsDisabledControlPressed(GameControl control)
        {
            return NativeFunction.CallByName<bool>("IS_DISABLED_CONTROL_PRESSED", 0, (int) control);
        }

        public static bool IsDisabledControlJustPressed(GameControl control)
        {
            return NativeFunction.CallByName<bool>("IS_DISABLED_CONTROL_JUST_PRESSED", 0, (int)control);
        }

        public static bool IsDisabledControlJustReleased(GameControl control)
        {
            return NativeFunction.CallByName<bool>("IS_DISABLED_CONTROL_JUST_RELEASED", 0, (int)control);
        }

        public static Vector3 ForwardVector(this Vector3 vector, float yaw)
        {
            Vector3 right;
            float cos = (float)Math.Cos(yaw + Math.PI / 2.0f);
            right.X = (180f / (float)Math.PI) * cos;
            right.Y = 0f;
            float sin = (float)Math.Sin(yaw + Math.PI / 2.0f);
            right.Z = (180f / (float)Math.PI) * sin;
            return CrossWith(vector, right);
        }

        public static Vector3 CrossWith(Vector3 left, Vector3 right)
        {
            Vector3 result;
            result.X = left.Y * right.Z - left.Z * right.Y;
            result.Y = left.Z * right.X - left.X * right.Z;
            result.Z = left.X * right.Y - left.Y * right.X;
            return result;
        }

        public static bool WorldToScreenRel(Vector3 worldCoords, out Vector2 screenCoords)
        {
            var output = World.ConvertWorldPositionToScreenPosition(worldCoords);
            output = new Vector2(output.X / Game.Resolution.Width, output.Y / Game.Resolution.Height);
            screenCoords = new Vector2((output.X - 0.5f) * 2, (output.Y - 0.5f) * 2);
            return true;
        }

        public static Vector3 ScreenRelToWorld(Vector3 camPos, Vector3 camRot, Vector2 coord)
        {
            var camForward = RotationToDirection(camRot);
            var rotUp = camRot + new Vector3(10, 0, 0);
            var rotDown = camRot + new Vector3(-10, 0, 0);
            var rotLeft = camRot + new Vector3(0, 0, -10);
            var rotRight = camRot + new Vector3(0, 0, 10);

            var camRight = RotationToDirection(rotRight) - RotationToDirection(rotLeft);
            var camUp = RotationToDirection(rotUp) - RotationToDirection(rotDown);

            var rollRad = -DegToRad(camRot.Y);

            var camRightRoll = camRight * (float)Math.Cos(rollRad) - camUp * (float)Math.Sin(rollRad);
            var camUpRoll = camRight * (float)Math.Sin(rollRad) + camUp * (float)Math.Cos(rollRad);

            var point3D = camPos + camForward * 10.0f + camRightRoll + camUpRoll;
            Vector2 point2D;
            if (!WorldToScreenRel(point3D, out point2D)) return camPos + camForward * 10.0f;
            var point3DZero = camPos + camForward * 10.0f;
            Vector2 point2DZero;
            if (!WorldToScreenRel(point3DZero, out point2DZero)) return camPos + camForward * 10.0f;

            const double eps = 0.001;
            if (Math.Abs(point2D.X - point2DZero.X) < eps || Math.Abs(point2D.Y - point2DZero.Y) < eps) return camPos + camForward * 10.0f;
            var scaleX = (coord.X - point2DZero.X) / (point2D.X - point2DZero.X);
            var scaleY = (coord.Y - point2DZero.Y) / (point2D.Y - point2DZero.Y);
            var point3Dret = camPos + camForward * 10.0f + camRightRoll * scaleX + camUpRoll * scaleY;
            return point3Dret;
        }

        public static Vector3 RotationToDirection(Vector3 rotation)
        {
            var z = DegToRad(rotation.Z);
            var x = DegToRad(rotation.X);
            var num = Math.Abs(Math.Cos(x));
            return new Vector3
            {
                X = (float)(-Math.Sin(z) * num),
                Y = (float)(Math.Cos(z) * num),
                Z = (float)Math.Sin(x)
            };
        }

        public static Vector3 DirectionToRotation(Vector3 direction)
        {
            direction.Normalize();

            var x = Math.Atan2(direction.Z, direction.Y);
            var y = 0;
            var z = -Math.Atan2(direction.X, direction.Y);

            return new Vector3
            {
                X = (float)RadToDeg(x),
                Y = (float)RadToDeg(y),
                Z = (float)RadToDeg(z)
            };
        }

        public static double DegToRad(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        public static double RadToDeg(double deg)
        {
            return deg * 180.0 / Math.PI;
        }

        public static double BoundRotationDeg(double angleDeg)
        {
            var twoPi = (int)(angleDeg / 360);
            var res = angleDeg - twoPi * 360;
            if (res < 0) res += 360;
            return res;
        }

        public static Vector3 RaycastEverything(Vector2 screenCoord, Camera cam, Entity ignore)
        {
            var camPos = cam.Position;
            var camRot = cam.Rotation;
            const float raycastToDist = 100.0f;
            const float raycastFromDist = 1f;

            var target3D = ScreenRelToWorld(camPos, new Vector3(camRot.Pitch, camRot.Roll, camRot.Yaw), screenCoord);
            var source3D = camPos;

            
            var dir = (target3D - source3D);
            dir.Normalize();
            
            var raycastResults = World.TraceLine(source3D + dir * raycastFromDist,
                source3D + dir * raycastToDist,
                (TraceFlags)(1 | 16 | 256 | 2 | 4 | 8)// | peds + vehicles
                , ignore);

            if (raycastResults.Hit)
            {
                return raycastResults.HitPosition;
            }

            return camPos + dir * raycastToDist;
        }

        public static Vector3 RaycastEverything(Vector2 screenCoord, Vector3 camPos, Vector3 camRot, Entity toIgnore)
        {
            const float raycastToDist = 100.0f;
            const float raycastFromDist = 1f;

            var target3D = ScreenRelToWorld(camPos, camRot, screenCoord);
            var source3D = camPos;

            Entity ignoreEntity = toIgnore;

            var dir = (target3D - source3D);
            dir.Normalize();
            var raycastResults = World.TraceLine(source3D + dir * raycastFromDist,
                source3D + dir * raycastToDist,
                (TraceFlags)(1 | 16 | 256 | 2 | 4 | 8)// | peds + vehicles
                , ignoreEntity);

            if (raycastResults.Hit)
            {
                return raycastResults.HitPosition;
            }

            return camPos + dir * raycastToDist;
        }

        public static Entity RaycastEntity(Vector2 screenCoord, Vector3 camPos, Vector3 camRot, Entity ignore)
        {
            const float raycastToDist = 100.0f;
            const float raycastFromDist = 1f;

            var target3D = ScreenRelToWorld(camPos, camRot, screenCoord);
            var source3D = camPos;

            
            var dir = (target3D - source3D);
            dir.Normalize();
            var raycastResults = World.TraceLine(source3D + dir * raycastFromDist,
                source3D + dir * raycastToDist,
                (TraceFlags)(1 | 16 | 256 | 2 | 4 | 8)// | peds + vehicles
                , ignore);

            return raycastResults.HitEntity;
        }

        public static float Clamp(this float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float LinearLerp(float time, float startVal, float endValue, float duration)
        {
            var change = endValue - startVal;
            return change*time/duration + startVal;
        }

        public static float QuadraticLerp(float time, float startVal, float endValue, float duration)
        {
            var change = endValue - startVal;

            time /= duration/2;
            if (time < 1) return change/2*time*time + startVal;
            time--;
            return -change/2*(time*(time - 2) - 1) + startVal;
        }

        public static Vector3 LerpVector(Vector3 vector, Vector3 end, Func<float, float, float, float, float> lerp, float time, float duration)
        {
            return new Vector3
            {
                X = lerp(time, vector.X, end.X, duration),
                Y = lerp(time, vector.Y, end.Y, duration),
                Z = lerp(time, vector.Z, end.Z, duration)
            };
        }
    }
}