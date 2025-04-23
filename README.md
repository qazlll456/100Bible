git clone https://github.com/yourusername/100Bible.git

# 100Bible Plugin for CounterStrikeSharp

A CounterStrikeSharp plugin that broadcasts Bible verses in-game, supporting multiple languages, configurable message filters, and broadcast modes.

## Features

- Broadcasts 100 Bible verses in English and Traditional Chinese with customizable intervals.
- Supports ```Sequential```, ```FullRandom```, and ```ShuffleRandom``` message selection modes.
- Public or private broadcast modes, toggled via ```!100bible```.
- Filter messages by ID ranges (```MessageGroups```) or text content (```TextSearches```).
- Multi-language support (English, Traditional Chinese).
- Color-coded messages for visual appeal.
- Admin commands for self-check and configuration reload (```css_100bible```).

## Requirements

- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) (latest version).
- CS2 server with .NET Core 3.1 support.
- Write permissions for ```csgo/addons/counterstrikesharp/plugins/100Bible/language/``` to generate language files.

## Installation

1. Clone or download this repository (see above).
2. Build the project:
   ```bash
   dotnet build -c Release
   ```
3. Copy the built plugin to your CS2 server:
   - Copy ```100Bible/bin/Release/netcoreapp3.1/100Bible.dll``` to ```csgo/addons/counterstrikesharp/plugins/100Bible/```.
   - Copy ```100Bible/configs/config.json``` to ```csgo/addons/counterstrikesharp/plugins/100Bible/configs/```.
4. Start your CS2 server. The plugin will generate ```english.json``` and ```t-chinese.json``` in ```csgo/addons/counterstrikesharp/plugins/100Bible/language/``` on first run.

## Configuration

Edit ```csgo/addons/counterstrikesharp/plugins/100Bible/configs/config.json``` to customize the plugin:

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
- **RandomMode**: Message selection mode (```Sequential```, ```FullRandom```, or ```ShuffleRandom```).
- **LanguageFile**: Language file to use (e.g., ```english.json```, ```t-chinese.json```).
- **BroadcastMode**: ```public``` (all players) or ```private``` (subscribed players only).
- **MessageGroups**: Array of ID ranges (e.g., ```["5-10"]``` for messages with IDs 5-10).
- **TextSearches**: Array of text filters (e.g., ```["John"]``` for verses containing "John").

> **Note**: JSON does not support comments. Ensure ```config.json``` is valid JSON without comments.

## Commands

### Admin Commands
Run in the server console or by admin players in-game:

- ```css_100bible```: Performs a self-check, displaying configuration and language file status.
- ```css_100bible reload```: Reloads ```config.json``` and language files without restarting the server.

### Player Commands
Run in-game via chat:

- ```!100bible```: Toggles subscription to private mode broadcasts (if ```BroadcastMode``` is ```private```).
- ```!100bible language list```: Lists available language files.
- ```!100bible language <name>```: Sets player language (e.g., ```t-chinese```).

## Language Files

- Located in ```csgo/addons/counterstrikesharp/plugins/100Bible/language/```.
- Default files: ```english.json``` and ```t-chinese.json``` (100 verses each).
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
- Source files: ```CommandManager.cs```, ```ConfigManager.cs```, ```BroadcastManager.cs```, ```LanguageManager.cs```, ```ChatFormatter.cs```, ```BiblePlugin.cs```.
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
