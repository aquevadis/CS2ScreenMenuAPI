using CounterStrikeSharp.API.Core;

namespace CS2ScreenMenuAPI.Internal
{
    public abstract class BaseMenu(string title)
    {
        public string Title { get; set; } = title;
        internal List<MenuOption> MenuOptions { get; } = [];

        public void AddOption(string text, Action<CCSPlayerController, IMenuOption> callback, bool disabled = false)
        {
            MenuOptions.Add(new MenuOption(text, callback, disabled));
        }
    }
}