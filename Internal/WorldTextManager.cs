﻿using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2ScreenMenuAPI.Internal
{
    internal static class WorldTextManager
    {
        internal static Dictionary<uint, CCSPlayerController> WorldTextOwners = [];

        internal static CPointWorldText? Create(
            CCSPlayerController player,
            string text,
            float size = 35,
            Color? color = null,
            string font = "",
            float shiftX = 0f,
            float shiftY = 0f,
            bool drawBackground = true,
            float backgroundHeight = 0.2f,
            float backgroundWidth = 0.15f
        )
        {
            Console.WriteLine($"[WorldTextManager] Creating text for player {player.PlayerName}");

            // Use our extension method to get (or create) the custom view.
            CCSGOViewModel? viewmodel = player.EnsureCustomView(0);
            if (viewmodel == null)
            {
                Console.WriteLine("[WorldTextManager] Failed to get viewmodel");
                return null;
            }

            var pawn = player.PlayerPawn?.Value!;
            QAngle eyeAng;

            if (pawn.LifeState is (byte)LifeState_t.LIFE_DEAD) {

                pawn = (CCSPlayerPawn)pawn.As<CBasePlayerPawn>();

                var observerServices = pawn?.ObserverServices;
                if (observerServices is null) return null;

                var observerPawn = observerServices.ObserverTarget?.Value?.As<CCSPlayerPawn>();
                if (observerPawn is null || observerPawn.IsValid is not true) return null;

                var observerController = observerPawn.OriginalController.Value;
                if (observerController is null || observerController.IsValid is not true) return null;

                var observerPlayer = Utilities.GetPlayerFromIndex((int)observerController.Index - 1);
                if (observerPlayer is null || observerPlayer.IsValid is not true) return null;

                var observerPlayerPawn = observerPlayer.PlayerPawn.Value;
                
                eyeAng = observerPlayerPawn?.EyeAngles!;
                pawn = observerPlayerPawn;

            }
            else {
                 eyeAng = pawn.As<CCSPlayerPawn>().EyeAngles;
            }

            CPointWorldText worldText = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext")!;
            worldText.MessageText = text;
            worldText.Enabled = true;
            worldText.FontSize = size;
            worldText.Fullbright = true;
            worldText.Color = color ?? Color.Aquamarine;
            worldText.WorldUnitsPerPx = (0.25f / 1050) * size;
            worldText.FontName = font;
            worldText.JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
            worldText.JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER;
            worldText.ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;

            if (drawBackground)
            {
                worldText.DrawBackground = true;
                worldText.BackgroundBorderHeight = backgroundHeight;
                worldText.BackgroundBorderWidth = backgroundWidth;
            }

            QAngle eyeAngles = eyeAng;
            Vector forward = new(), right = new(), up = new();
            NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, right.Handle, up.Handle);

            Vector offset = new();
            offset += forward * 7;
            offset += right * shiftX;
            offset += up * shiftY;
            QAngle angles = new()
            {
                Y = eyeAngles.Y + 270,
                Z = 90 - eyeAngles.X,
                X = 0
            };

            worldText.DispatchSpawn();
            worldText.Teleport(pawn.AbsOrigin! + offset + new Vector(0, 0, pawn.ViewOffset.Z), angles, null);

            worldText.AcceptInput("SetParent", viewmodel, null, "!activator");

            WorldTextOwners[worldText.Index] = player;

            return worldText;
        }
    }
}
