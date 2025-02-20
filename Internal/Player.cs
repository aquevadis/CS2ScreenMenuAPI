using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2ScreenMenuAPI.Internal
{
    public static class CCSPlayer
    {
        public static bool IsValidPlayer(CCSPlayerController? p)
        {
            return p != null && p.IsValid && !p.IsBot && !p.IsHLTV && p.Connected == PlayerConnectedState.PlayerConnected;
        }

        public static CCSPlayerPawn? GetPlayerPawn(this CCSPlayerController player)
        {
            return player.PlayerPawn.Value;
        }
        public static CCSPlayerPawnBase? GetPlayerPawnBase(this CCSPlayerController player)
        {
            return player.GetPlayerPawn() as CCSPlayerPawnBase;
        }

        public static void InitializePlayerWorldText(CCSPlayerController player)
        {
            if (player == null) return;

            

            WorldTextManager.Create(player, "");
        }

        public static CCSGOViewModel? EnsureCustomView(this CCSPlayerController player, int index)
        {
            CCSPlayerPawnBase? pPawnBase = player.GetPlayerPawnBase();
            if (pPawnBase == null)
            {
                Console.WriteLine("[EnsureCustomView] Player base pawn is null");
                return null;
            }

            Console.WriteLine("[EnsureCustomView] Player base pawn is valid");

            // If the pawn is dead, try to get the observer's pawn.
            if (pPawnBase.LifeState == (byte)LifeState_t.LIFE_DEAD)
            {
                Console.WriteLine("[EnsureCustomView] Pawn is dead");
                if (player.ControllingBot)
                {
                    Console.WriteLine("[EnsureCustomView] Player is a bot; cannot ensure custom view");
                    return null;
                }
                else
                {
                    var observerServices = player.PlayerPawn.Value?.ObserverServices;
                    if (observerServices == null)
                    {
                        Console.WriteLine("[EnsureCustomView] ObserverServices is null");
                        return null;
                    }

                    var observerPawn = observerServices.ObserverTarget;
                    if (observerPawn == null || !observerPawn.IsValid)
                    {
                        Console.WriteLine("[EnsureCustomView] ObserverTarget is null or invalid");
                        return null;
                    }

                    // Try to cast the observer pawn to a CCSPlayerPawn.
                    var obsPawn = observerPawn.Value as CCSPlayerPawn;
                    if (obsPawn == null)
                    {
                        Console.WriteLine("[EnsureCustomView] ObserverPawn is not a valid CCSPlayerPawn");
                        return null;
                    }

                    var observerController = obsPawn.OriginalController;
                    if (observerController == null || !observerController.IsValid)
                    {
                        Console.WriteLine("[EnsureCustomView] OriginalController is null or invalid");
                        return null;
                    }

                    // Use the observer controller's index to find the observer.
                    uint origIndex = observerController.Value.Index;
                    if (origIndex == 0)
                    {
                        Console.WriteLine("[EnsureCustomView] OriginalController index is 0");
                        return null;
                    }
                    uint observerIndex = origIndex - 1;

                    // Assume Utilities.GetPlayers() returns all CCSPlayerController instances.
                    var allPlayers = Utilities.GetPlayers();
                    var observer = allPlayers.FirstOrDefault(p => p.Index == observerIndex);
                    if (observer == null)
                    {
                        Console.WriteLine("[EnsureCustomView] Observer not found with index " + observerIndex);
                        return null;
                    }

                    pPawnBase = observer.PlayerPawn.Value;
                    if (pPawnBase == null)
                    {
                        Console.WriteLine("[EnsureCustomView] Observer's pawn is null");
                        return null;
                    }
                }
            }

            if (pPawnBase.ViewModelServices == null)
            {
                Console.WriteLine("[EnsureCustomView] ViewModelServices is null");
                return null;
            }

            // Compute the handle for the view model using the schema offset.
            var handle = new CHandle<CCSGOViewModel>(
                (IntPtr)(pPawnBase.ViewModelServices.Handle +
                         Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel") + 4));

            if (!handle.IsValid)
            {
                Console.WriteLine("[EnsureCustomView] ViewModel handle is invalid, creating new predicted_viewmodel");
                CCSGOViewModel? viewmodel = Utilities.CreateEntityByName<CCSGOViewModel>("predicted_viewmodel");
                if (viewmodel == null)
                {
                    Console.WriteLine("[EnsureCustomView] Failed to create predicted_viewmodel");
                    return null;
                }

                viewmodel.DispatchSpawn();
                handle.Raw = viewmodel.EntityHandle.Raw;
                Utilities.SetStateChanged(pPawnBase, "CCSPlayerPawnBase", "m_pViewModelServices");
            }
            else
            {
                Console.WriteLine("[EnsureCustomView] Found valid view model handle");
            }

            return handle.Value;
        }
    
        /// <summary>
        /// Get the observing player of another player
        /// </summary>
        /// <param name="playerPawn">The playerPawn that is being observed</param>
        /// <returns>The playerPawn that is currently spectating the player</returns>
        public static CBasePlayerPawn? GetObservingPlayerOfPlayer(this CBasePlayerPawn playerPawn)
        {
            if (playerPawn is null || playerPawn.IsValid is not true) return null;

            var observerServices = playerPawn?.ObserverServices;
            if (observerServices is null) return null;

            var observerPawn = observerServices.ObserverTarget?.Value?.As<CCSPlayerPawn>();
            if (observerPawn is null || observerPawn.IsValid is not true) return null;

            var observerController = observerPawn.OriginalController.Value;
            if (observerController is null || observerController.IsValid is not true) return null;

            var observerPlayer = Utilities.GetPlayerFromIndex((int)observerController.Index - 1);
            if (observerPlayer is null || observerPlayer.IsValid is not true) return null;

            var observerPlayerPawn = observerPlayer.PlayerPawn.Value;

            return observerPlayerPawn;
        }
        

    }
}