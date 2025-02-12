using CounterStrikeSharp.API.Core;

namespace CS2ScreenMenuAPI
{
    public interface IMenuInstance
    {
        void NextPage();
        void PrevPage();
        void Reset();
        void Close();
        void Display();
    }
}