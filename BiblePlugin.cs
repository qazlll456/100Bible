using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Collections.Generic;

namespace BiblePlugin
{
    public class BiblePlugin : BasePlugin
    {
        public override string ModuleName => "100bible";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "qazlll456 with Grok";

        private ConfigManager? _configManager;
        private LanguageManager? _languageManager;
        private BroadcastManager? _broadcastManager;
        private CommandManager? _commandManager;
        private ChatFormatter? _chatFormatter;
        private bool _isRunning = false;
        public readonly Dictionary<ulong, bool> PlayerSubscriptions = new();
        public readonly Dictionary<ulong, string> PlayerLanguagePreferences = new();

        public override void Load(bool hotReload)
        {
            _configManager = new ConfigManager(ModuleDirectory);
            _languageManager = new LanguageManager(ModuleDirectory);
            _chatFormatter = new ChatFormatter();
            _broadcastManager = new BroadcastManager(this, _chatFormatter);
            _commandManager = new CommandManager(this, _configManager, _languageManager, _broadcastManager);

            if (!_configManager.LoadConfig() || !_languageManager.LoadLanguage(_configManager.Config?.LanguageFile ?? "english.json"))
            {
                Server.PrintToConsole("Error: Plugin stopped due to configuration issues. Fix and reload.");
                return;
            }

            _isRunning = true;
            if (_languageManager.Language != null)
            {
                _broadcastManager.StartBroadcastTimer(_configManager.Config!, _languageManager.Language);
            }
            _commandManager.RegisterCommands();

            // Register disconnect event
            RegisterListener<Listeners.OnClientDisconnect>(OnClientDisconnect);

            Server.PrintToConsole("100bible by qazlll456 with Grok loaded successfully!");
        }

        public override void Unload(bool hotReload)
        {
            if (_isRunning)
            {
                _broadcastManager?.StopBroadcastTimer();
                _commandManager?.UnregisterCommands();
            }
            base.Unload(hotReload);
        }

        private void OnClientDisconnect(int clientSlot)
        {
            var player = Utilities.GetPlayerFromSlot(clientSlot);
            if (player != null && player.SteamID != 0)
            {
                PlayerSubscriptions.Remove(player.SteamID);
                PlayerLanguagePreferences.Remove(player.SteamID);
            }
        }

        public LanguageManager GetLanguageManager()
        {
            return _languageManager ?? throw new InvalidOperationException("LanguageManager not initialized.");
        }

        public bool IsRunning => _isRunning;
    }
}