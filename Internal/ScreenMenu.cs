using System.Drawing;
using CounterStrikeSharp.API.Core;
using CS2ScreenMenuAPI.Enums;

namespace CS2ScreenMenuAPI
{
    public class ScreenMenu : BaseMenu
    {
        private readonly BasePlugin _plugin;
        public Color TextColor { get; set; } = Color.OrangeRed;
        public string FontName { get; set; } = "Verdana Bold";
        public bool IsSubMenu { get; set; } = false;
        public bool HasExitOption { get; set; } = true;
        public ScreenMenu? ParentMenu { get; set; } = null;
        public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Nothing;

        public ScreenMenu(string title, BasePlugin plugin) : base(title)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }

        public void Open(CCSPlayerController player)
        {
            MenuAPI.OpenMenu(_plugin, player, this);
        }
    }
}