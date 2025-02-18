using CounterStrikeSharp.API.Core;

namespace CS2ScreenMenuAPI.Internal
{
    internal class MenuOption(string text, Action<CCSPlayerController, IMenuOption> onSelect, bool disabled = false) : IMenuOption
    {
        public string Text { get; set; } = text;
        public bool Disabled { get; set; } = disabled;
        public Action<CCSPlayerController, IMenuOption> OnSelect { get; set; } = onSelect;
    }
}