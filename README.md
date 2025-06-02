<<<<<<< HEAD
# BloogBot Project Overview
=======
# BloogBot Project Overview (Root README)
>>>>>>> a3cfe7c (Update README.md)

**BloogBot** is a modular **World of Warcraft bot** framework designed for developers to extend and customize. It operates by injecting a .NET bot into the WoW game client, enabling automated gameplay on legacy WoW versions (Vanilla 1.12.1, TBC 2.4.3, WotLK 3.3.5). This project is composed of several components, each handling a different aspect of the bot’s functionality. This overview will describe the architecture at a high level and provide links to detailed documentation for each part.

## System Architecture

The BloogBot system is divided into the following components:

* **BloogBot (Core Bot Engine)** – *Runs inside the WoW client.* This is the main C# application that implements the bot’s brain: the state machine, plugin system, in-game UI, and high-level decision logic. It coordinates everything from combat rotations to movement and integrates all other components. *(See [BloogBot/README.md](BloogBot/README.md) for developer details.)*

* **Bootstrapper (Launcher)** – *Runs externally.* A small C# utility that launches the WoW client and injects the Loader into it automatically. This frees the developer from manual injection steps. Configure the game path in a JSON file, run the Bootstrapper, and it will start WoW and load the bot. *(See [Bootstrapper/README.md](Bootstrapper/README.md) for details.)*

* **Loader (Native DLL Injector)** – *Injected into WoW by Bootstrapper.* A C++ DLL that starts the .NET CLR within the WoW process and invokes the core bot’s entry point. Essentially, this bridges the unmanaged game process with our managed code. *(See [Loader/README.md](Loader/README.md).)*

* **Navigation (Pathfinding Library)** – *Native helper inside WoW.* A C++ library using Recast/Detour to compute paths on WoW’s terrain. The bot calls this to get waypoints for movement, so it can avoid obstacles and navigate the game world intelligently (requires precomputed movement maps). *(See [Navigation/README.md](Navigation/README.md).)*

* **FastCall (Calling Convention Helper)** – *Native helper inside WoW.* A C++ library that provides functions for the bot to call game routines that use fastcall/thiscall conventions which .NET can’t directly invoke. This is mainly needed for the Vanilla client. *(See [FastCall/README.md](FastCall/README.md).)*

* **Bot Class Modules** (e.g., `ArcaneMageBot`, `FuryWarriorBot`, etc.) – *Plugins loaded by the core.* Separate C# class libraries, one per class or specialization, that define how the bot plays that class (combat rotations, buffs, etc.). The core engine loads these at runtime so it can support multiple classes without hardcoding their logic. Developers can create new modules or modify existing ones to change the bot’s behavior for a class. *(See [Bots/ClassNameBot/README.md](Bots/ArcaneMageBot/README.md) for an example with ArcaneMage.)*

Below is a diagram of how these components interact:

```
[Developer Machine]         [Game Process (WoW.exe)]
Bootstrapper.exe  --->  (Launch WoW) 
                      --->  Inject Loader.dll  --+--> Loader initializes .NET runtime
                                                 |    and starts BloogBot Core
                                                 v
                                         BloogBot.exe (Core Bot, runs state machine & UI)
                                          |__ Navigation.dll (for pathfinding)
                                          |__ FastCall.dll  (for special game calls)
                                          |__ [ArcaneMageBot.dll, ...] (class plugins)
```

1. **Bootstrapper** starts WoW and injects **Loader**.
2. **Loader** loads the .NET runtime and launches **BloogBot (Core)** inside WoW.
3. **BloogBot Core** then loads all class **Bot Modules** (plugins), opens the bot UI, and begins logic. It calls into **Navigation** for movement and uses **FastCall** for certain game function calls.
4. The bot plays the game autonomously, following the behaviors defined in the class module for your character’s class.

## Features

* **Multiple Expansion Support:** BloogBot can attach to Vanilla, TBC, and WotLK clients. It abstracts differences in memory structures and functions by selecting the appropriate offsets and function pointers for each version. Note that it’s not compatible with modern retail WoW (and is not intended for use there).
* **State-Driven Behavior:** The bot uses a stack-based state machine to handle complex behavior (combat, resting, traveling, etc.) without getting stuck. It has fail-safes (kill-switches) to stop if something unexpected occurs (teleport detection, long inactivity, etc.).
* **In-Game GUI:** A WPF GUI is rendered within the WoW process, giving real-time control and feedback. You can start/stop the bot, see status (current target, state, etc.), adjust some settings, and observe logs.
* **Discord Integration:** Optionally, the bot can send notifications to a Discord channel (for events like level-up or if the bot stops due to a potential issue). This is configured via the botSettings and can be turned off.
* **Data Persistence:** The bot can connect to an external database or use local files to save data like hotspots (grind locations), blacklisted mobs, and travel paths. A SQL schema is provided (for Azure SQL, as used by the developer), but you can adapt it or disable the DB requirement by toggling settings.
* **Plugin System:** All class-specific logic is in modular plugins. This means the core doesn’t need changes to add a new class or improve an existing one – simply develop a new plugin DLL. This modular approach keeps the core generic and makes the project extensible.
* **Pathfinding:** Using Navigation meshes from the game data, the bot moves in a human-like way, avoiding walls, taking paths up hills, etc. You must generate the `mmaps` from the game client files (see Navigation README for instructions), which is a one-time setup.
* **Memory Reading/Writing:** The bot uses internal game APIs when possible (like using Lua calls or calling functions to interact), making it more reliable. Direct memory reads are used for info not available via API (like object positions, health, etc.), and memory writes are minimal (mostly to call functions or simulate input).

## Setup Guide (For Developers)

1. **Clone and Open Solution:** Open `BloogBot.sln` in Visual Studio 2022 (required for C++17 support and .NET 4.6.1). Ensure you have C++ Desktop development workload installed (for native components).
2. **Build the Solution:** Use *Debug|x86* or *Release|x86*. All projects should build. This yields the necessary binaries in the `Bot\` output folder.
3. **Prepare Game Client:** You need a 32-bit WoW client for version 1.12.1, 2.4.3, or 3.3.5a. Obtain the client and ensure you can run it. **Important:** Use a local server or a permitted environment – do not use this on official servers.
4. **Generate Navigation Meshes:** If you plan to use Navigation (recommended), generate the movement maps:

   * Use a WoW map extraction tool to produce `mmaps` (movement maps) for your game version. (See the blog or documentation on extracting mmaps. Typically, you’d run `Ad.exe` to extract maps/vmaps, then `MovemapGenerator.exe` on those to get mmaps.)
   * Once generated, copy the `mmaps/` directory next to the bot’s binaries (so Bot\mmaps\*.mmap and \*.mmtile files). The bot will load these at runtime for pathfinding.
5. **Configure `bootstrapperSettings.json`:** Put the path to your WoW client exe. Example:

   ```json
   { "PathToWoW": "D:\\Games\\WoW-1.12.1\\WoW.exe" }
   ```

   You can also set WoW windowed mode in its config to easily see the bot UI.
6. **Configure `botSettings.json`:** Adjust settings for your scenario:

   * `CurrentBotName` to the profile matching your class (e.g., `"Arcane Mage"`). The available names correspond to the Name each Bot plugin exports.
   * Set `DiscordBotEnabled` false (unless you have a bot token to use).
   * If using the DB, put connection info; otherwise, you might set `DatabaseType` to `"Local"` and it will use a local SQLite or JSON (depending on implementation).
   * Review targeting and loot settings (level range, creature types to attack, loot quality to pick up, etc.) which you can tweak for your needs.
7. **Run the Bot:** Start the Bootstrapper (through Visual Studio or by running `Bootstrapper.exe`). It will launch WoW, inject the bot, and you should see the BloogBot UI appear in-game after a few seconds.
8. **Attach Debugger (optional):** If you want to step through code, attach VS to the WoW process once it’s running and loaded (Visual Studio may prompt you as the Bootstrapper includes a debug attach utility). Then you can set breakpoints in, say, your class module or core logic.
9. **Start Bot from UI:** In the BloogBot window, select your bot profile and hit Start. Watch the console for logs (e.g., “Bot started” message). The character should start to behave according to the programmed logic – e.g., buff itself, look for enemies, move and attack, etc.
10. **Monitoring:** Keep an eye on the game and bot output. The UI will display info like “Current State: CombatState” or “Target: \[Mob Name]” and logs of actions (e.g., casting spells, looting items). If anything goes wrong (e.g., an exception in bot logic), it will likely log it and possibly stop the bot for safety.

For more detailed information on each component or to contribute, refer to the component-specific README files linked above. The original author has also written extensive articles on their website about this project’s development, which can provide deeper insight into design decisions and WoW internals.

**Important Notices:**

* This project is intended for **educational and private server use**. It explicitly does not support nor condone use on official servers (retail), and likely will not work there due to both ethical reasons and advanced anti-cheat measures.
* The code quality is as expected for a hobby project; developers are encouraged to **fix bugs and submit improvements**. The architecture is designed to be extensible: you can add new features or support new classes by following the established patterns.
* Setting up the bot requires some technical steps (compiling, extracting game data). Ensure you follow each step; missing data (like no mmaps or incorrect offsets for a different client build) will result in suboptimal behavior or errors.
* **Join the Community:** The project has a Discord server for discussion and support. Developer collaboration can greatly help in maintaining the offsets, improving combat rotations, and sharing new plugins (maybe someone wrote a RogueBot or a Gatherer plugin, etc.).

Happy hacking and may your DPS be high and pathfinding be true! This project opens a window into how game automation works at a low level, and with it, you can experiment and extend a fully functional WoW bot on your own terms. Enjoy exploring and extending BloogBot’s capabilities.
