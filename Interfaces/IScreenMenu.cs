using System.Drawing;
using CounterStrikeSharp.API.Core;

namespace CS2ScreenMenuAPI.Interfaces
{
    public interface IScreenMenu
    {
        string Title { get; set; }
        Color TextColor { get; set; }
        string FontName { get; set; }
        bool IsSubMenu { get; set; }
        bool HasExitOption { get; set; } 
        IScreenMenu? ParentMenu { get; set; }
        void AddOption(string text, Action<CCSPlayerController, IMenuOption> callback, bool disabled);
        void Open(CCSPlayerController player);
    }
}