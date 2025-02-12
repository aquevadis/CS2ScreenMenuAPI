using CounterStrikeSharp.API.Core;

namespace CS2ScreenMenuAPI
{
    internal class MenuOption : IMenuOption
    {
        public string Text { get; set; }
        public bool Disabled { get; set; }
        public Action<CCSPlayerController, IMenuOption> OnSelect { get; set; }

        public MenuOption(string text, Action<CCSPlayerController, IMenuOption> onSelect, bool disabled = false)
        {
            Text = text;
            OnSelect = onSelect;
            Disabled = disabled;
        }
    }
}