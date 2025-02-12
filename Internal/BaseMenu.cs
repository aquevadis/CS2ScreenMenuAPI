using CounterStrikeSharp.API.Core;

namespace CS2ScreenMenuAPI
{
    public abstract class BaseMenu
    {
        public string Title { get; set; }
        internal List<MenuOption> MenuOptions { get; } = new();

        protected BaseMenu(string title)
        {
            Title = title;
        }

        public void AddOption(string text, Action<CCSPlayerController, IMenuOption> callback, bool disabled = false)
        {
            MenuOptions.Add(new MenuOption(text, callback, disabled));
        }

    }
}