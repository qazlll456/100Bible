using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BiblePlugin
{
    public class BroadcastManager
    {
        private readonly BiblePlugin _plugin;
        private readonly ChatFormatter _chatFormatter;
        private CounterStrikeSharp.API.Modules.Timers.Timer? _broadcastTimer;
        private int _currentMessageIndex = 0;
        private List<int> _shuffledIndices = new();
        private readonly Random _random = new();

        public BroadcastManager(BiblePlugin plugin, ChatFormatter chatFormatter)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            _chatFormatter = chatFormatter ?? throw new ArgumentNullException(nameof(chatFormatter));
        }

        public void StartBroadcastTimer(Config config, Language? language)
        {
            if (config == null || language == null)
            {
                Server.PrintToConsole("Error: Invalid config or language. Broadcast timer not started.");
                return;
            }

            Server.PrintToConsole($"Starting broadcast with config: BroadcastInterval={config.BroadcastInterval}, MessageGroups=[{string.Join(", ", config.MessageGroups ?? Array.Empty<string>())}], TextSearches=[{string.Join(", ", config.TextSearches ?? Array.Empty<string>())}]");

            StopBroadcastTimer();
            var filteredMessages = GetFilteredMessages(config, language);
            if (filteredMessages.Count == 0)
            {
                Server.PrintToConsole("Error: No messages match MessageGroups or TextSearches. Broadcasting skipped.");
                return;
            }

            ResetMessageOrder(config, filteredMessages);
            Server.PrintToConsole($"Starting broadcast timer with interval: {config.BroadcastInterval} seconds, {filteredMessages.Count} messages (IDs: {string.Join(", ", filteredMessages.Select(m => m.Id))}).");
            _broadcastTimer = new CounterStrikeSharp.API.Modules.Timers.Timer((float)config.BroadcastInterval, () => BroadcastMessage(config, language, filteredMessages), CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);
        }

        public void StopBroadcastTimer()
        {
            _broadcastTimer?.Kill();
            Server.PrintToConsole("Broadcast timer stopped.");
        }

        private void BroadcastMessage(Config config, Language language, List<Message> filteredMessages)
        {
            if (!_plugin.IsRunning || filteredMessages == null || filteredMessages.Count == 0)
            {
                Server.PrintToConsole("Broadcast skipped: Plugin not running or no messages.");
                return;
            }

            var message = GetNextMessage(config, filteredMessages);
            if (message == null || string.IsNullOrEmpty(message.Text))
            {
                Server.PrintToConsole($"Error: Invalid message at index {_currentMessageIndex}. Skipping.");
                _currentMessageIndex = (_currentMessageIndex + 1) % filteredMessages.Count;
                return;
            }

            if (config.BroadcastMode.Equals("public", StringComparison.OrdinalIgnoreCase))
            {
                var formattedMessage = _chatFormatter.FormatMessage(language.MessagePrefix, message.Id, message.Text);
                Server.PrintToChatAll(formattedMessage);
                Server.PrintToConsole($"Broadcast sent to all players: ID={message.Id}, Text={message.Text}");
            }
            else
            {
                int sentCount = 0;
                foreach (var player in Utilities.GetPlayers())
                {
                    if (player?.SteamID == null || !_plugin.PlayerSubscriptions.TryGetValue(player.SteamID, out bool isSubscribed) || !isSubscribed)
                        continue;

                    var langFile = _plugin.PlayerLanguagePreferences.TryGetValue(player.SteamID, out var pref) ? pref : config.LanguageFile;
                    var playerLang = _plugin.GetLanguageManager().GetPlayerLanguage(langFile) ?? language;
                    var formattedMessage = _chatFormatter.FormatMessage(playerLang.MessagePrefix, message.Id, message.Text);
                    player.PrintToChat(formattedMessage);
                    sentCount++;
                }
                Server.PrintToConsole($"Broadcast sent to {sentCount} subscribed players: ID={message.Id}, Text={message.Text}");
            }
        }

        private List<Message> GetFilteredMessages(Config config, Language language)
        {
            var filtered = new List<Message>();
            if (language.Messages == null || language.Messages.Length == 0)
            {
                Server.PrintToConsole("Error: No messages available in language.");
                return filtered;
            }

            Server.PrintToConsole($"Processing filters: MessageGroups=[{string.Join(", ", config.MessageGroups ?? Array.Empty<string>())}], TextSearches=[{string.Join(", ", config.TextSearches ?? Array.Empty<string>())}]");
            var messageIds = new HashSet<int>();
            bool hasFilters = (config.MessageGroups != null && config.MessageGroups.Any(g => !string.IsNullOrWhiteSpace(g))) || (config.TextSearches != null && config.TextSearches.Any(s => !string.IsNullOrWhiteSpace(s)));

            if (hasFilters)
            {
                // Handle MessageGroups (ID ranges)
                if (config.MessageGroups != null && config.MessageGroups.Length > 0)
                {
                    foreach (var group in config.MessageGroups)
                    {
                        if (string.IsNullOrWhiteSpace(group))
                        {
                            Server.PrintToConsole($"Warning: Empty MessageGroup entry. Skipping.");
                            continue;
                        }
                        var normalizedGroup = group.Replace(",", "-");
                        if (Regex.IsMatch(normalizedGroup, @"^\d+-\d+$"))
                        {
                            var parts = normalizedGroup.Split('-').Select(int.Parse).ToArray();
                            var matches = language.Messages.Where(m => m.Id >= parts[0] && m.Id <= parts[1] && !messageIds.Contains(m.Id)).OrderBy(m => m.Id).ToList();
                            filtered.AddRange(matches);
                            messageIds.UnionWith(matches.Select(m => m.Id));
                            Server.PrintToConsole($"Applied MessageGroup {normalizedGroup} (original: {group}): {matches.Count} messages (IDs: {string.Join(", ", matches.Select(m => m.Id))})");
                        }
                        else
                        {
                            Server.PrintToConsole($"Warning: Invalid MessageGroup format {group}. Must be 'start-end' or 'start,end' (e.g., '5-10' or '5,10').");
                        }
                    }
                }

                // Handle TextSearches (text content)
                if (config.TextSearches != null && config.TextSearches.Length > 0)
                {
                    foreach (var search in config.TextSearches)
                    {
                        if (!string.IsNullOrWhiteSpace(search))
                        {
                            var matches = language.Messages.Where(m => m.Text.Contains(search, StringComparison.OrdinalIgnoreCase) && !messageIds.Contains(m.Id)).OrderBy(m => m.Id).ToList();
                            filtered.AddRange(matches);
                            messageIds.UnionWith(matches.Select(m => m.Id));
                            Server.PrintToConsole($"Applied TextSearch {search}: {matches.Count} messages (IDs: {string.Join(", ", matches.Select(m => m.Id))})");
                        }
                        else
                        {
                            Server.PrintToConsole($"Warning: Empty TextSearch entry. Skipping.");
                        }
                    }
                }
            }

            // If no messages matched filters, log warning but don't fall back to all messages
            if (filtered.Count == 0 && hasFilters)
            {
                Server.PrintToConsole("Warning: No messages matched specified MessageGroups or TextSearches. No messages will be broadcast.");
            }
            else if (!hasFilters)
            {
                filtered.AddRange(language.Messages.OrderBy(m => m.Id));
                Server.PrintToConsole($"No filters applied, using all {filtered.Count} messages (IDs: {string.Join(", ", filtered.Select(m => m.Id))})");
            }

            Server.PrintToConsole($"Final filtered messages: {filtered.Count} messages (IDs: {string.Join(", ", filtered.Select(m => m.Id))})");
            return filtered;
        }

        private void ResetMessageOrder(Config config, List<Message> filteredMessages)
        {
            _currentMessageIndex = 0;
            _shuffledIndices.Clear();

            if (config.RandomMode == "ShuffleRandom")
            {
                _shuffledIndices = Enumerable.Range(0, filteredMessages.Count).OrderBy(_ => _random.Next()).ToList();
            }
            Server.PrintToConsole($"Message order reset for RandomMode={config.RandomMode}, ShuffledIndices=[{string.Join(", ", _shuffledIndices)}]");
        }

        private Message? GetNextMessage(Config config, List<Message> filteredMessages)
        {
            if (filteredMessages.Count == 0)
            {
                Server.PrintToConsole("No messages available to broadcast.");
                return null;
            }

            int index;
            if (config.RandomMode == "FullRandom")
            {
                index = _random.Next(0, filteredMessages.Count);
            }
            else if (config.RandomMode == "ShuffleRandom")
            {
                if (_currentMessageIndex >= _shuffledIndices.Count)
                {
                    ResetMessageOrder(config, filteredMessages);
                }
                index = _shuffledIndices[_currentMessageIndex];
                _currentMessageIndex++;
            }
            else // Sequential
            {
                index = _currentMessageIndex;
                _currentMessageIndex = (_currentMessageIndex + 1) % filteredMessages.Count;
            }

            var message = filteredMessages[index];
            Server.PrintToConsole($"Selected message: ID={message.Id}, Index={index}, TotalMessages={filteredMessages.Count}");
            return message;
        }
    }
}