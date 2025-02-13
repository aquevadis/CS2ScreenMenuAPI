using CounterStrikeSharp.API.Core;
using CS2ScreenMenuAPI.Internal;

namespace CS2ScreenMenuAPI
{
    public static class MenuAPI
    {
        private static readonly Dictionary<IntPtr, IMenuInstance> ActiveMenus = new();

        public static void OpenMenu(BasePlugin plugin, CCSPlayerController player, ScreenMenu menu)
        {
            if (player == null || !player.IsValid)
                throw new ArgumentException("Player is null or invalid", nameof(player));

            CloseActiveMenu(player);
            ActiveMenus[player.Handle] = new ScreenMenuInstance(plugin, player, menu);
            ActiveMenus[player.Handle].Display();
        }

        public static void OpenSubMenu(BasePlugin plugin, CCSPlayerController player, ScreenMenu menu)
        {
            if (player == null || !player.IsValid)
                throw new ArgumentException("Player is null or invalid", nameof(player));

            if (ActiveMenus.TryGetValue(player.Handle, out var activeMenu))
            {
                activeMenu.Close();
            }
            ActiveMenus[player.Handle] = new ScreenMenuInstance(plugin, player, menu);
            ActiveMenus[player.Handle].Display();
        }

        public static void CloseActiveMenu(CCSPlayerController player)
        {
            if (player == null || !player.IsValid) return;

            if (ActiveMenus.TryGetValue(player.Handle, out var menu))
            {
                menu.Close();
                ActiveMenus.Remove(player.Handle);
            }
        }

        public static void RemoveActiveMenu(CCSPlayerController player)
        {
            if (player == null || !player.IsValid)
                return;

            ActiveMenus.Remove(player.Handle);
        }

        public static IMenuInstance? GetActiveMenu(CCSPlayerController player)
        {
            return player != null && player.IsValid && ActiveMenus.TryGetValue(player.Handle, out var menu) ? menu : null;
        }
    }
}
