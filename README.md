git clone https://github.com/yourusername/100Bible.git

# 100Bible Plugin for CounterStrikeSharp

A CounterStrikeSharp plugin that broadcasts Bible verses in-game, supporting multiple languages, configurable message filters, and broadcast modes.

## Features

- Broadcasts 100 Bible verses in English and Traditional Chinese with customizable intervals.
- Supports 0Sequential0, 0FullRandom0, and 0ShuffleRandom0 message selection modes.
- Public or private broadcast modes, toggled via 0!100bible0.
- Filter messages by ID ranges (0MessageGroups0) or text content (0TextSearches0).
- Multi-language support (English, Traditional Chinese).
- Color-coded messages for visual appeal.
- Admin commands for self-check and configuration reload (0css_100bible0).

## Requirements

- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) (latest version).
- CS2 server with .NET Core 3.1 support.
- Write permissions for 0csgo/addons/counterstrikesharp/plugins/100Bible/language/0 to generate language files.

## Installation

1. Clone or download this repository (see above).
2. Build the project:
   0bash
   dotnet build -c Release
   0
3. Copy the built plugin to your CS2 server:
   - Copy 0100Bible/bin/Release/netcoreapp3.1/100Bible.dll0 to 0csgo/addons/counterstrikesharp/plugins/100Bible/0.
   - Copy 0100Bible/configs/config.json0 to 0csgo/addons/counterstrikesharp/plugins/100Bible/configs/0.
4. Start your CS2 server. The plugin will generate 0english.json0 and 0t-chinese.json0 in 0csgo/addons/counterstrikesharp/plugins/100Bible/language/0 on first run.

## Configuration

Edit 0csgo/addons/counterstrikesharp/plugins/100Bible/configs/config.json0 to customize the plugin:

```json
{
  "BroadcastInterval": 5,
  "RandomMode": "Sequential",
  "LanguageFile": "english.json",
  "BroadcastMode": "public",
  "MessageGroups": ["5-10"],
  "TextSearches": []
}
```

### Configuration Options

- **BroadcastInterval**: Time (in seconds) between broadcasts (e.g., 05 for every 5 seconds).
- **RandomMode**: Message selection mode (0Sequential0, 0FullRandom0, or 0ShuffleRandom0).
- **LanguageFile**: Language file to use (e.g., 0english.json0, 0t-chinese.json0).
- **BroadcastMode**: 0public0 (all players) or 0private0 (subscribed players only).
- **MessageGroups**: Array of ID ranges (e.g., 0["5-10"]0 for messages with IDs 5-10).
- **TextSearches**: Array of text filters (e.g., 0["John"]0 for verses containing "John").

> **Note**: JSON does not support comments. Ensure 0config.json0 is valid JSON without comments.

## Commands

### Admin Commands
Run in the server console or by admin players in-game:

- 0css_100bible0: Performs a self-check, displaying configuration and language file status.
- 0css_100bible reload0: Reloads 0config.json0 and language files without restarting the server.

### Player Commands
Run in-game via chat:

- 0!100bible0: Toggles subscription to private mode broadcasts (if 0BroadcastMode0 is 0private0).
- 0!100bible language list0: Lists available language files.
- 0!100bible language <name>0: Sets player language (e.g., 0t-chinese0).

## Language Files

- Located in 0csgo/addons/counterstrikesharp/plugins/100Bible/language/0.
- Default files: 0english.json0 and 0t-chinese.json0 (100 verses each).
- Format:
  ```json
  {
    "MessagePrefix": "{green}100Bible {0}: {white}{1}",
    "Messages": [
      { "Id": 1, "Text": "{white}For God so loved the world... - John 3:16 - KJV" },
      // ... 100 verses
    ]
  }
  ```

## Development

- Built with .NET Core 3.1 and CounterStrikeSharp.
- Source files: 0CommandManager.cs0, 0ConfigManager.cs0, 0BroadcastManager.cs0, 0LanguageManager.cs0, 0ChatFormatter.cs0, 0BiblePlugin.cs0.
- To build:
  ```bash
  dotnet build -c Release
  ```

## License

MIT License. See [LICENSE](LICENSE) for details.

## Contributing

Feel free to open issues or submit pull requests on GitHub.

## Credits

Developed by qazlll456 with assistance from Grok (xAI).