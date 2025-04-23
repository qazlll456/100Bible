using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BiblePlugin
{
    public class CommandManager
    {
        private readonly BiblePlugin _plugin;
        private readonly ConfigManager _configManager;
        private readonly LanguageManager _languageManager;
        private readonly BroadcastManager _broadcastManager;

        public CommandManager(BiblePlugin plugin, ConfigManager configManager, LanguageManager languageManager, BroadcastManager broadcastManager)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _languageManager = languageManager ?? throw new ArgumentNullException(nameof(languageManager));
            _broadcastManager = broadcastManager ?? throw new ArgumentNullException(nameof(broadcastManager));
        }

        public void RegisterCommands()
        {
            _plugin.AddCommand("css_100bible", "Performs a self-check or reloads the 100Bible plugin", OnBibleAdminCommand);
            _plugin.AddCommand("!100bible", "Manages 100bible settings or toggles private mode", OnBibleCommand);
        }

        public void UnregisterCommands()
        {
            _plugin.RemoveCommand("css_100bible", OnBibleAdminCommand);
            _plugin.RemoveCommand("!100bible", OnBibleCommand);
        }

        private void OnBibleAdminCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (!IsValidAdmin(player))
            {
                string message = "You do not have permission to use this command.";
                if (player != null)
                    player.PrintToChat(message);
                Server.PrintToConsole(message);
                return;
            }

            if (command.ArgCount > 1 && command.GetArg(1).ToLower() == "reload")
            {
                Server.PrintToConsole("Reloading 100Bible plugin configuration...");
                bool success = _configManager.LoadConfig() && _languageManager.LoadLanguage(_configManager.Config?.LanguageFile ?? "english.json");
                string message;

                if (success && _configManager.Config != null && _languageManager.Language != null)
                {
                    _broadcastManager.StartBroadcastTimer(_configManager.Config, _languageManager.Language);
                    message = "100Bible plugin reloaded successfully.";
                }
                else
                {
                    message = "Error: Reload failed, check config or language files. Keeping previous state.";
                }

                if (player != null)
                    player.PrintToChat(message);
                Server.PrintToConsole(message);
                return;
            }

            // Self-check logic
            var languageDir = Path.Combine(_plugin.ModuleDirectory, "language");
            var languageFiles = Directory.Exists(languageDir) ? Directory.GetFiles(languageDir, "*.json") : Array.Empty<string>();
            var report = new System.Text.StringBuilder("100bible Self-Check Report:\n");
            report.AppendLine($"- config.json: {(_configManager.Config != null ? "Found, valid" : "Not found or invalid")}");
            report.AppendLine("- Language files:");

            foreach (var file in languageFiles)
            {
                try
                {
                    var language = JsonSerializer.Deserialize<Language>(File.ReadAllText(file));
                    var fileName = Path.GetFileName(file);
                    var messageCount = language?.Messages?.Length ?? 0;
                    var hasEmptyTexts = language?.Messages?.Any(m => string.IsNullOrEmpty(m.Text)) ?? true;
                    var hasValidIds = language?.Messages?.All(m => m.Id >= 1) ?? false;
                    var hasDuplicates = language?.Messages?.GroupBy(m => m.Id).Any(g => g.Count() > 1) ?? true;
                    report.AppendLine($"  - {fileName}: {messageCount} messages, {(hasEmptyTexts ? "has empty texts" : "no empty texts")}, {(hasValidIds && !hasDuplicates ? "valid IDs, no duplicates" : "invalid IDs or duplicates")}");
                }
                catch
                {
                    report.AppendLine($"  - {Path.GetFileName(file)}: Invalid JSON");
                }
            }

            report.AppendLine($"- Broadcast interval: {(_configManager.Config?.BroadcastInterval ?? 0)} seconds");
            report.AppendLine($"- Random mode: {(_configManager.Config?.RandomMode ?? "Unknown")}");
            report.AppendLine($"- Broadcast mode: {(_configManager.Config?.BroadcastMode ?? "Unknown")}");
            report.AppendLine($"- Message groups: {(_configManager.Config?.MessageGroups != null ? string.Join(", ", _configManager.Config.MessageGroups) : "None")}");
            report.AppendLine($"- Text searches: {(_configManager.Config?.TextSearches != null ? string.Join(", ", _configManager.Config.TextSearches) : "None")}");
            report.AppendLine($"- Plugin status: {(_plugin.IsRunning ? "Running" : "Stopped")}");

            if (player != null)
                player.PrintToConsole(report.ToString());
            else
                Server.PrintToConsole(report.ToString());
        }

        private void OnBibleCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || !player.IsValid || player.IsBot)
                return;

            if (command.ArgCount > 1)
            {
                string arg = command.GetArg(1).ToLower();
                if (arg == "language")
                {
                    if (command.ArgCount > 2 && command.GetArg(2).ToLower() == "list")
                    {
                        var languageDir = Path.Combine(_plugin.ModuleDirectory, "language");
                        var languages = Directory.Exists(languageDir)
                            ? Directory.GetFiles(languageDir, "*.json").Select(f => Path.GetFileNameWithoutExtension(f) ?? "").Where(f => !string.IsNullOrEmpty(f)).ToList()
                            : new List<string>();
                        if (languages.Count == 0)
                        {
                            player.PrintToChat("No language files found.");
                        }
                        else
                        {
                            player.PrintToChat($"Available languages: {string.Join(", ", languages)}");
                        }
                        return;
                    }
                    else if (command.ArgCount > 2)
                    {
                        string lang = command.GetArg(2).ToLower();
                        string langFile = lang + ".json";
                        var langPath = Path.Combine(_plugin.ModuleDirectory, "language", langFile);
                        if (File.Exists(langPath))
                        {
                            _plugin.PlayerLanguagePreferences[player.SteamID] = langFile;
                            player.PrintToChat($"Language set to {lang}.");
                        }
                        else
                        {
                            _plugin.PlayerLanguagePreferences[player.SteamID] = "english.json";
                            player.PrintToChat($"Error: Language {lang} not found. Set to english.");
                        }
                        return;
                    }
                }
            }

            if (_configManager.Config?.BroadcastMode.Equals("private", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                bool isSubscribed = _plugin.PlayerSubscriptions.ContainsKey(player.SteamID) && _plugin.PlayerSubscriptions[player.SteamID];
                _plugin.PlayerSubscriptions[player.SteamID] = !isSubscribed;
                player.PrintToChat($"100bible messages {(isSubscribed ? "disabled" : "enabled")}.");
            }
            else
            {
                player.PrintToChat("100bible is in public mode; no toggle needed.");
            }
        }

        private bool IsValidAdmin(CCSPlayerController? player)
        {
            // Allow server console (player == null) or non-bot players (temporary fallback)
            // TODO: Replace with CounterStrikeSharp admin flag check, e.g., player.HasPermission("@css/admin")
            return player == null || (player.IsValid && !player.IsBot);
        }
    }
}