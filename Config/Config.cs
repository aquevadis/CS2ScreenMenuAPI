using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace CS2ScreenMenuAPI.Config
{
    public class MenuConfig
    {
        private const string CONFIG_FILE = "config.jsonc";
        private string _configPath = string.Empty;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
        };

        public Buttons_Config Buttons { get; set; } = new Buttons_Config();
        public Buttons_Info ButtonsInfo { get; set; } = new Buttons_Info();

        public MenuConfig() { }

        public void Initialize()
        {
            _configPath = Path.Combine(
                Server.GameDirectory,
                "csgo",
                "addons",
                "counterstrikesharp",
                "shared",
                "CS2ScreenMenuAPI",
                CONFIG_FILE);

            string directory = Path.GetDirectoryName(_configPath)!;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            LoadConfig();
        }

        private void LoadConfig()
        {
            if (!File.Exists(_configPath))
            {
                CreateDefaultConfig();
                return;
            }

            try
            {
                var jsonContent = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<MenuConfig>(jsonContent, _jsonOptions);
                if (config != null)
                {
                    Buttons = config.Buttons;
                    ButtonsInfo = config.ButtonsInfo;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading menu configuration: {ex.Message}");
                CreateDefaultConfig();
            }
        }

        private void CreateDefaultConfig()
        {
            var configContent = @"{
    /* 
        Menu configuration file.
        Adjust your button settings and display texts as needed.
    */
    ""Buttons"": {
        ""ScrollUpButton"": ""W"",
        ""ScrollDownButton"": ""S"",
        ""SelectButton"": ""E""
    },
    ""ButtonsInfo"": {
        ""ScrollInfo"": ""[W/S] Scroll"",
        ""SelectInfo"": ""[E] Select""
    }
    /* 
        Buttons mapping:
        
        Alt1       - Alt1
        Alt2       - Alt2
        Attack     - Attack
        Attack2    - Attack2
        Attack3    - Attack3
        Bullrush   - Bullrush
        Cancel     - Cancel
        Duck       - Duck
        Grenade1   - Grenade1
        Grenade2   - Grenade2
        Space      - Jump
        Left       - Left
        W          - Forward
        A          - Moveleft
        S          - Back
        D          - Moveright
        E          - Use
        R          - Reload
        F          - (Custom) 0x800000000
        Shift      - Speed
        Right      - Right
        Run        - Run
        Walk       - Walk
        Weapon1    - Weapon1
        Weapon2    - Weapon2
        Zoom       - Zoom
        Tab        - (Custom) 8589934592
    */
}";
            try
            {
                string directory = Path.GetDirectoryName(_configPath)!;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(_configPath, configContent);
                LoadConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating default configuration: {ex.Message}");
            }
        }

        public void SaveConfig()
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(this, _jsonOptions);
                File.WriteAllText(_configPath, jsonContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving menu configuration: {ex.Message}");
            }
        }
    }

    public class Buttons_Config
    {
        public string ScrollUpButton { get; set; } = "W";
        public string ScrollDownButton { get; set; } = "S";
        public string SelectButton { get; set; } = "E";
    }

    public class Buttons_Info
    {
        public string ScrollInfo { get; set; } = "[W/S] Scroll";
        public string SelectInfo { get; set; } = "[E] Select";
    }
}
