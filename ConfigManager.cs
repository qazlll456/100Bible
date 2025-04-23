using CounterStrikeSharp.API;
using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BiblePlugin
{
    public class ConfigManager
    {
        private readonly string _moduleDirectory;
        public Config? Config { get; private set; }

        public ConfigManager(string moduleDirectory)
        {
            _moduleDirectory = moduleDirectory ?? throw new ArgumentNullException(nameof(moduleDirectory));
        }

        public bool LoadConfig()
        {
            var configPath = Path.Combine(_moduleDirectory, "configs/config.json");
            Server.PrintToConsole($"Attempting to load config from: {configPath}");

            try
            {
                if (!File.Exists(configPath))
                {
                    Server.PrintToConsole("Config file not found. Generating default config.json.");
                    GenerateDefaultConfig(configPath);
                    return true;
                }

                var json = File.ReadAllText(configPath);
                // Strip comments (// and /* */) and normalize whitespace
                json = Regex.Replace(json, @"//.*?\n|/\*.*?\*/", "", RegexOptions.Singleline);
                json = Regex.Replace(json, @"\s+", " ");
                Server.PrintToConsole($"Config file content (after comment stripping): {json}");

                var config = JsonSerializer.Deserialize<Config>(json);
                if (config == null)
                {
                    Server.PrintToConsole("Error: config.json is invalid JSON. Using in-memory default config.");
                    Config = CreateDefaultConfig();
                    return true;
                }

                Config = config;
                Server.PrintToConsole($"Raw config loaded: BroadcastInterval={config.BroadcastInterval}, RandomMode={config.RandomMode}, LanguageFile={config.LanguageFile}, BroadcastMode={config.BroadcastMode}, MessageGroups=[{string.Join(", ", config.MessageGroups ?? Array.Empty<string>())}], TextSearches=[{string.Join(", ", config.TextSearches ?? Array.Empty<string>())}]");

                // Validate config
                if (config.BroadcastInterval <= 0)
                {
                    Server.PrintToConsole("Error: BroadcastInterval must be positive. Using default 30.");
                    config.BroadcastInterval = 30;
                }
                else
                {
                    Server.PrintToConsole($"Validated BroadcastInterval: {config.BroadcastInterval}");
                }

                if (string.IsNullOrEmpty(config.LanguageFile))
                {
                    Server.PrintToConsole("Error: LanguageFile is empty. Using default english.json.");
                    config.LanguageFile = "english.json";
                }
                else
                {
                    Server.PrintToConsole($"Validated LanguageFile: {config.LanguageFile}");
                }

                if (!config.BroadcastMode.Equals("public", StringComparison.OrdinalIgnoreCase) &&
                    !config.BroadcastMode.Equals("private", StringComparison.OrdinalIgnoreCase))
                {
                    Server.PrintToConsole("Error: BroadcastMode must be 'public' or 'private'. Using default public.");
                    config.BroadcastMode = "public";
                }
                else
                {
                    Server.PrintToConsole($"Validated BroadcastMode: {config.BroadcastMode}");
                }

                if (!new[] { "Sequential", "FullRandom", "ShuffleRandom" }.Contains(config.RandomMode))
                {
                    Server.PrintToConsole("Error: Invalid RandomMode. Using default Sequential.");
                    config.RandomMode = "Sequential";
                }
                else
                {
                    Server.PrintToConsole($"Validated RandomMode: {config.RandomMode}");
                }

                if (config.MessageGroups != null && config.MessageGroups.Length > 0)
                {
                    var validGroups = new List<string>();
                    foreach (var group in config.MessageGroups)
                    {
                        if (string.IsNullOrWhiteSpace(group))
                        {
                            Server.PrintToConsole($"Warning: Empty MessageGroup entry. Skipping.");
                            continue;
                        }
                        // Support both "start-end" and "start,end" formats
                        var normalizedGroup = group.Replace(",", "-");
                        if (Regex.IsMatch(normalizedGroup, @"^\d+-\d+$"))
                        {
                            var parts = normalizedGroup.Split('-').Select(int.Parse).ToArray();
                            if (parts[0] >= 1 && parts[1] >= parts[0])
                            {
                                validGroups.Add(normalizedGroup);
                                Server.PrintToConsole($"Validated MessageGroup: {normalizedGroup} (original: {group})");
                            }
                            else
                            {
                                Server.PrintToConsole($"Warning: Invalid range {group}. Must start at 1 and be ascending.");
                            }
                        }
                        else
                        {
                            Server.PrintToConsole($"Warning: Invalid MessageGroup format {group}. Must be 'start-end' or 'start,end' (e.g., '5-10' or '5,10').");
                        }
                    }
                    config.MessageGroups = validGroups.ToArray();
                    Server.PrintToConsole($"Final MessageGroups: [{string.Join(", ", config.MessageGroups)}]");
                }
                else
                {
                    config.MessageGroups = Array.Empty<string>();
                    Server.PrintToConsole("No MessageGroups specified.");
                }

                if (config.TextSearches != null && config.TextSearches.Length > 0)
                {
                    var validSearches = new List<string>();
                    foreach (var search in config.TextSearches)
                    {
                        if (!string.IsNullOrWhiteSpace(search))
                        {
                            validSearches.Add(search);
                            Server.PrintToConsole($"Validated TextSearch: {search}");
                        }
                        else
                        {
                            Server.PrintToConsole($"Warning: Empty TextSearch entry. Skipping.");
                        }
                    }
                    config.TextSearches = validSearches.ToArray();
                    Server.PrintToConsole($"Final TextSearches: [{string.Join(", ", config.TextSearches)}]");
                }
                else
                {
                    config.TextSearches = Array.Empty<string>();
                    Server.PrintToConsole("No TextSearches specified.");
                }

                Server.PrintToConsole("Config loaded and validated successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"Error: Failed to load config.json ({ex.Message}). Using in-memory default config.");
                Config = CreateDefaultConfig();
                return false; // Indicate failure to trigger reload error
            }
        }

        private void GenerateDefaultConfig(string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                Config = CreateDefaultConfig();

                var commentedConfig = @"// 100Bible Plugin Configuration
// This file configures the 100Bible plugin for CounterStrikeSharp.
// Ensure all fields are correctly set to avoid errors.
// WARNING: JSON does not support comments. Remove all comments (// lines) before saving, or the config may fail to load.

// BroadcastInterval: Time (in seconds) between message broadcasts.
// - Must be a positive integer (e.g., 30 for every 30 seconds).
// - Default: 30
""BroadcastInterval"": 30,

// RandomMode: How messages are selected for broadcast.
// - ""Sequential"": Messages are shown in order (ID 1, 2, 3, ...).
// - ""FullRandom"": Randomly selects any message, may repeat.
// - ""ShuffleRandom"": Shuffles all messages, cycles through once, then reshuffles.
// - Default: ""Sequential""
""RandomMode"": ""Sequential"",

// LanguageFile: The language file to use for messages (located in plugins/100Bible/language/).
// - Must be a valid .json file (e.g., ""english.json"", ""t-chinese.json"").
// - Default: ""english.json""
""LanguageFile"": ""english.json"",

// BroadcastMode: Determines who receives messages.
// - ""public"": All players see messages in chat.
// - ""private"": Only subscribed players (via !100bible command) see messages.
// - Default: ""public""
""BroadcastMode"": ""public"",

// MessageGroups: Filters messages by ID range.
// - Use format ""start-end"" or ""start,end"" (e.g., ""5-10"" or ""5,10"" for messages ID 5 to 10).
// - Empty array ([]) includes all messages if no TextSearches are specified.
// - Example: [""1-10"", ""20-30""] broadcasts messages with IDs 1-10 and 20-30.
// - Default: []
""MessageGroups"": [],

// TextSearches: Filters messages by text content.
// - Use a word (e.g., ""John"" to match messages containing ""John"", like John 3:16).
// - Empty array ([]) includes all messages if no MessageGroups are specified.
// - Example: [""John"", ""Psalm""] broadcasts messages containing ""John"" or ""Psalm"".
// - Default: []
""TextSearches"": []
";
                File.WriteAllText(path, "{\n" + commentedConfig + "\n}");
                Server.PrintToConsole("Generated default config.json with comments. WARNING: Remove comments before editing to ensure valid JSON.");
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"Error: Failed to create default config.json ({ex.Message}). Using in-memory default.");
                Config = CreateDefaultConfig();
            }
        }

        private Config CreateDefaultConfig()
        {
            var defaultConfig = new Config
            {
                BroadcastInterval = 30,
                RandomMode = "Sequential",
                LanguageFile = "english.json",
                BroadcastMode = "public",
                MessageGroups = Array.Empty<string>(),
                TextSearches = Array.Empty<string>()
            };
            Server.PrintToConsole($"Created in-memory default config: BroadcastInterval={defaultConfig.BroadcastInterval}, MessageGroups=[{string.Join(", ", defaultConfig.MessageGroups)}]");
            return defaultConfig;
        }
    }

    public class Config
    {
        public int BroadcastInterval { get; set; }
        public string RandomMode { get; set; } = "Sequential";
        public string LanguageFile { get; set; } = "english.json";
        public string BroadcastMode { get; set; } = "public";
        public string[] MessageGroups { get; set; } = Array.Empty<string>();
        public string[] TextSearches { get; set; } = Array.Empty<string>();
    }
}