using System;
using System.IO;
using System.Text;
using System.Reflection;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CS2ScreenMenuAPI.Buttons;
using CS2ScreenMenuAPI.Config;

namespace CS2ScreenMenuAPI.Internal
{
    internal class ScreenMenuInstance : IMenuInstance
    {
        private CPointWorldText? _hudText;
        private readonly BasePlugin _plugin;
        private readonly CCSPlayerController _player;
        private readonly ScreenMenu _menu;
        private const int NUM_PER_PAGE = 6;

        private int CurrentSelection = 0;
        private int CurrentPage = 0;
        private PlayerButtons _oldButtons;
        private readonly MenuConfig _config;
        private bool _useHandled = false;
        private bool _menuJustOpened = true;
        private int _ticksSinceOpen = 0;

        private readonly Listeners.OnTick _onTickDelegate;
        private readonly Listeners.CheckTransmit _checkTransmitDelegate;
        private readonly Listeners.OnEntityDeleted _onEntityDeletedDelegate;
        private readonly BasePlugin.GameEventHandler<EventRoundStart> _onRoundStartDelegate;

        public ScreenMenuInstance(BasePlugin plugin, CCSPlayerController player, ScreenMenu menu)
        {
            _plugin = plugin;
            _player = player;
            _menu = menu;

            _config = new MenuConfig();
            _config.Initialize();

            Reset();
            _oldButtons = player.Buttons;
            _useHandled = true;
            _menuJustOpened = true;
            _ticksSinceOpen = 0;

            _onTickDelegate = new Listeners.OnTick(Update);
            _checkTransmitDelegate = new Listeners.CheckTransmit(CheckTransmitListener);
            _onEntityDeletedDelegate = new Listeners.OnEntityDeleted(OnEntityDeleted);
            _onRoundStartDelegate = new BasePlugin.GameEventHandler<EventRoundStart>(OnRoundStart);

            RegisterListeners();
        }


        private void RegisterListeners()
        {
            _plugin.RegisterListener<Listeners.OnTick>(_onTickDelegate);
            _plugin.RegisterListener<Listeners.CheckTransmit>(_checkTransmitDelegate);
            _plugin.RegisterListener<Listeners.OnEntityDeleted>(_onEntityDeletedDelegate);
            _plugin.RegisterEventHandler<EventRoundStart>(_onRoundStartDelegate, HookMode.Pre);
        }

        private void OnEntityDeleted(CEntityInstance entity)
        {
            uint entityIndex = entity.Index;
            if (WorldTextManager.WorldTextOwners.ContainsKey(entityIndex))
            {
                WorldTextManager.WorldTextOwners.Clear();
            }
        }

        private void CheckTransmitListener(CCheckTransmitInfoList infoList)
        {
            foreach ((CCheckTransmitInfo info, CCSPlayerController? client) in infoList)
            {
                if (client is null || !client.IsValid)
                    continue;

                foreach (var kvp in WorldTextManager.WorldTextOwners)
                {
                    uint worldTextIndex = kvp.Key;
                    CCSPlayerController owner = kvp.Value;

                    if (client.Slot != owner.Slot)
                    {
                        info.TransmitEntities.Remove((int)worldTextIndex);
                    }
                }
            }
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            if (MenuAPI.GetActiveMenu(_player) != this)
                return HookResult.Continue;

            _plugin.AddTimer(0.1f, RecreateHud);
            return HookResult.Continue;
        }

        private void Update()
        {
            if (MenuAPI.GetActiveMenu(_player) != this)
                return;

            var currentButtons = _player.Buttons;
            _ticksSinceOpen++;

            if (_ticksSinceOpen < 3)
            {
                if (_hudText == null || !_hudText.IsValid)
                    Display();
            }

            if (_menuJustOpened)
            {
                _menuJustOpened = false;
                _oldButtons = currentButtons;
                return;
            }

            HandleButtons(currentButtons);
            _oldButtons = currentButtons;
        }

        private void MoveSelection(int direction)
        {
            int totalLines = GetTotalLines();
            int selectableCount = Math.Min(NUM_PER_PAGE, _menu.MenuOptions.Count - (CurrentPage * NUM_PER_PAGE));
            int newSelection = CurrentSelection;
            int attempts = 0;

            do
            {
                newSelection = (newSelection + direction + totalLines) % totalLines;
                if (newSelection < selectableCount)
                {
                    if (!_menu.MenuOptions[CurrentPage * NUM_PER_PAGE + newSelection].Disabled)
                        break;
                }
                else
                {
                    break;
                }
                attempts++;
            }
            while (attempts < totalLines);

            CurrentSelection = newSelection;
        }

        private void HandleButtons(PlayerButtons currentButtons)
        {
            if (Buttons.Buttons.ButtonMapping.TryGetValue(_config.Buttons.ScrollUpButton, out var scrollUpButton))
            {
                if (((_oldButtons & scrollUpButton) == 0) && ((currentButtons & scrollUpButton) != 0))
                {
                    MoveSelection(-1);
                    Display();
                }
            }

            if (Buttons.Buttons.ButtonMapping.TryGetValue(_config.Buttons.ScrollDownButton, out var scrollDownButton))
            {
                if (((_oldButtons & scrollDownButton) == 0) && ((currentButtons & scrollDownButton) != 0))
                {
                    MoveSelection(1);
                    Display();
                }
            }

            if (Buttons.Buttons.ButtonMapping.TryGetValue(_config.Buttons.SelectButton, out var selectButton))
            {
                if (((_oldButtons & selectButton) == 0) && ((currentButtons & selectButton) != 0))
                {
                    if (!_useHandled)
                    {
                        HandleSelection();
                        Display();
                        _useHandled = true;
                    }
                }
                else if ((currentButtons & selectButton) == 0)
                {
                    _useHandled = false;
                }
            }
        }

        public void Display()
        {
            var builder = new StringBuilder();
            string pad = new string('\u00A0', 30);

            BuildMenuText(builder, pad);

            string menuText = builder.ToString();

            if (_hudText == null)
            {
                _hudText = WorldTextManager.Create(
                    _player,
                    menuText,
                    35,
                    _menu.TextColor,
                    _menu.FontName,
                    -5.5f,
                    2.8f,
                    true,
                    true
                );
            }

            if (_hudText != null && _hudText.IsValid)
            {
                _hudText.AcceptInput("SetMessage", _hudText, _hudText, menuText);
            }
        }

        private void BuildMenuText(StringBuilder builder, string pad)
        {
            builder.AppendLine("\u200B");
            builder.AppendLine($" {" "}{_menu.Title}");
            builder.AppendLine("\u200B");

            int currentOffset = CurrentPage * NUM_PER_PAGE;
            int selectable = Math.Min(NUM_PER_PAGE, _menu.MenuOptions.Count - currentOffset);

            BuildOptionsList(builder, currentOffset, selectable);
            BuildNavigationOptions(builder, selectable);

            builder.AppendLine();
            builder.AppendLine(" " + _config.ButtonsInfo.ScrollInfo + "\n" + " " + _config.ButtonsInfo.SelectInfo);
            builder.AppendLine(pad);
        }

        private void BuildOptionsList(StringBuilder builder, int currentOffset, int selectable)
        {
            for (int i = 0; i < selectable; i++)
            {
                var option = _menu.MenuOptions[currentOffset + i];
                string prefix = (CurrentSelection == i) ? "> " : "  ";
                string displayText = option.Disabled ? $"{option.Text} (Disabled)" : option.Text;
                builder.AppendLine($"{prefix}{i + 1}. {displayText}");
            }
        }

        private void BuildNavigationOptions(StringBuilder builder, int selectable)
        {
            builder.AppendLine("\u200B");

            if (CurrentPage == 0)
            {
                if (_menu.IsSubMenu)
                {
                    string prefix = (CurrentSelection == selectable) ? "> " : "  ";
                    builder.AppendLine($"{prefix}7. Back");

                    if (_menu.MenuOptions.Count > NUM_PER_PAGE)
                    {
                        prefix = (CurrentSelection == selectable + 1) ? "> " : "  ";
                        builder.AppendLine($"{prefix}8. Next");
                    }

                    prefix = (CurrentSelection == selectable + (_menu.MenuOptions.Count > NUM_PER_PAGE ? 2 : 1)) ? "> " : "  ";
                    builder.AppendLine($"{prefix}9. Close");
                }
                else
                {
                    if (_menu.MenuOptions.Count > NUM_PER_PAGE)
                    {
                        string prefix = (CurrentSelection == selectable) ? "> " : "  ";
                        builder.AppendLine($"{prefix}8. Next");
                    }

                    string closePrefix = (CurrentSelection == selectable + (_menu.MenuOptions.Count > NUM_PER_PAGE ? 1 : 0)) ? "> " : "  ";
                    builder.AppendLine($"{closePrefix}9. Close");
                }
            }
            else
            {
                string prefix = (CurrentSelection == selectable) ? "> " : "  ";
                builder.AppendLine($"{prefix}7. Back");

                if ((_menu.MenuOptions.Count - CurrentPage * NUM_PER_PAGE) > NUM_PER_PAGE)
                {
                    prefix = (CurrentSelection == selectable + 1) ? "> " : "  ";
                    builder.AppendLine($"{prefix}8. Next");
                }

                prefix = (CurrentSelection == selectable + ((_menu.MenuOptions.Count - CurrentPage * NUM_PER_PAGE) > NUM_PER_PAGE ? 2 : 1)) ? "> " : "  ";
                builder.AppendLine($"{prefix}9. Close");
            }
        }

        private int GetTotalLines()
        {
            int currentOffset = CurrentPage * NUM_PER_PAGE;
            int selectable = Math.Min(NUM_PER_PAGE, _menu.MenuOptions.Count - currentOffset);
            int navCount = 0;

            if (_menu.IsSubMenu)
            {
                navCount = (CurrentPage == 0)
                    ? (_menu.MenuOptions.Count > NUM_PER_PAGE ? 3 : 2)
                    : ((_menu.MenuOptions.Count - currentOffset) > NUM_PER_PAGE ? 3 : 2);
            }
            else
            {
                navCount = (CurrentPage == 0)
                    ? (_menu.MenuOptions.Count > NUM_PER_PAGE ? 2 : 1)
                    : ((_menu.MenuOptions.Count - currentOffset) > NUM_PER_PAGE ? 3 : 2);
            }

            return selectable + navCount;
        }

        private void HandleSelection()
        {
            int currentOffset = CurrentPage * NUM_PER_PAGE;
            int selectable = Math.Min(NUM_PER_PAGE, _menu.MenuOptions.Count - currentOffset);

            if (CurrentSelection < selectable)
            {
                int optionIndex = currentOffset + CurrentSelection;
                if (optionIndex < _menu.MenuOptions.Count)
                {
                    var option = _menu.MenuOptions[optionIndex];
                    if (!option.Disabled)
                    {
                        option.OnSelect(_player, option);
                    }
                }
            }
            else
            {
                HandleNavigationSelection(selectable);
            }
        }

        private void HandleNavigationSelection(int selectable)
        {
            int currentOffset = CurrentPage * NUM_PER_PAGE;
            int navIndex = CurrentSelection - selectable;

            if (CurrentPage == 0)
            {
                if (_menu.IsSubMenu)
                {
                    if (navIndex == 0) // Back
                    {
                        if (_menu.ParentMenu != null)
                        {
                            Close();
                            MenuAPI.OpenMenu(_plugin, _player, _menu.ParentMenu);
                        }
                    }
                    else if (navIndex == 1 && _menu.MenuOptions.Count > NUM_PER_PAGE) // Next
                    {
                        NextPage();
                    }
                    else // Close
                    {
                        Close();
                    }
                }
                else
                {
                    if (navIndex == 0 && _menu.MenuOptions.Count > NUM_PER_PAGE) // Next
                    {
                        NextPage();
                    }
                    else // Close
                    {
                        Close();
                    }
                }
            }
            else
            {
                if (navIndex == 0) // Back
                {
                    PrevPage();
                }
                else if (navIndex == 1 && (_menu.MenuOptions.Count - currentOffset) > NUM_PER_PAGE) // Next
                {
                    NextPage();
                }
                else // Close
                {
                    Close();
                }
            }
        }

        public void NextPage()
        {
            if ((CurrentPage + 1) * NUM_PER_PAGE < _menu.MenuOptions.Count)
            {
                CurrentPage++;
                CurrentSelection = 0;
                Display();
            }
        }

        public void PrevPage()
        {
            if (CurrentPage > 0)
            {
                CurrentPage--;
                CurrentSelection = 0;
                Display();
            }
        }

        public void Reset()
        {
            CurrentPage = 0;
            CurrentSelection = 0;
            Display();
        }

        public void Close()
        {
            if (_hudText != null)
            {
                _hudText.Enabled = false;
                _hudText.AcceptInput("Kill", _hudText);
                WorldTextManager.WorldTextOwners.Clear();
            }
            MenuAPI.RemoveActiveMenu(_player);
            UnregisterListeners();
        }

        private void UnregisterListeners()
        {
            _plugin.RemoveListener("OnTick", _onTickDelegate);
            _plugin.RemoveListener("CheckTransmit", _checkTransmitDelegate);
            _plugin.RemoveListener("OnEntityDeleted", _onEntityDeletedDelegate);
        }

        private void RecreateHud()
        {
            _hudText = null;
            Display();
        }
    }
}
