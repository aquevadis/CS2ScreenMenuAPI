using System.Drawing;
using CounterStrikeSharp.API.Core;
using CS2ScreenMenuAPI.Config;
using CS2ScreenMenuAPI.Enums;

namespace CS2ScreenMenuAPI
{
    public class ScreenMenu : BaseMenu
    {
        private readonly BasePlugin _plugin;
        private readonly MenuConfig _config;
        public bool IsSubMenu { get; set; } = false;
        public bool HasExitOption { get; set; } = true;
        public ScreenMenu? ParentMenu { get; set; } = null;
        public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Nothing;
        public int OptionsCount => MenuOptions.Count;
        public Color TextColor
        {
            get => _config.DefaultSettings.TextColor;
            set => _config.DefaultSettings.TextColor = value;
        }
        public string FontName
        {
            get => _config.DefaultSettings.MenuFont;
            set => _config.DefaultSettings.MenuFont = value;
        }
        public MenuType MenuType
        {
            get => _config.DefaultSettings.MenuType;
            set => _config.DefaultSettings.MenuType = value;
        }
        public float MenuPositionX
        {
            get => _config.DefaultSettings.MenuPositionX;
            set => _config.DefaultSettings.MenuPositionX = value;
        }
        public float MenuPositionY
        {
            get => _config.DefaultSettings.MenuPositionY;
            set => _config.DefaultSettings.MenuPositionY = value;
        }

        public ScreenMenu(string title, BasePlugin plugin) : base(title)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            _config = new MenuConfig();
            _config.Initialize();
        }

        public void Open(CCSPlayerController player)
        {
            MenuAPI.OpenMenu(_plugin, player, this);
        }
    }
}