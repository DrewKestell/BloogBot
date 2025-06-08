# Westworld of Warcraft (WWoW)

**Westworld of Warcraft (WWoW)** is a simulation platform that transforms a World of Warcraft–like server into a living world populated by both real players and AI-driven bots. Inspired by HBO’s *Westworld*, this project aims to create AI-controlled characters **indistinguishable from human players** in behavior. In WWoW, AI agents roam the game world, quest, fight monsters, interact with the environment (and eventually with players) just as a human would – providing a rich testbed for **agent-based AI** in a complex game environment.

## Purpose and Vision

WWoW’s primary goal is to explore advanced **agentic AI behavior** in an open-world MMORPG setting. By blurring the lines between human and bot players, we can study and push the boundaries of:

* **Human-Bot Indistinguishability:** Can an AI-controlled player mimic human playstyles, decision-making, and even social interaction convincingly? WWoW is a playground to find out.
* **Game Environment Simulation:** Populate a game server with autonomous agents to simulate a lively game world even with few or no human participants. This can be used for game testing, AI research, or simply creating a unique gameplay experience.
* **Agent-Based AI Techniques:** The project serves as a research platform for developing and testing AI techniques (state machines, reinforcement learning, planning, etc.) in a real-time environment. The WoW game world provides a complex, dynamic environment for AI agents to navigate and learn.

Ultimately, WWoW is about **intellectual exploration** – understanding how to build AI agents that can live and behave in a virtual world, not about exploiting the game. (Indeed, WWoW **does not support or work on modern official WoW servers**, and is intended for private/research use on legacy versions.)

## Features and Current Functionality

WWoW is built upon an open-source WoW bot framework (originally known as *BloogBot*), and extends it into a full simulation platform. The current features include:

* **Automated Player Characters:** WWoW’s bots can create and control WoW characters to fight monsters, level up, and travel the world **without human input**. Each bot runs *in-process* with the game client, giving it direct control over the character’s actions and responses.
* **Multi-Expansion Support:** The platform currently works with WoW Classic era game clients – Vanilla (1.12.1), Burning Crusade (2.4.3), and Wrath of the Lich King (3.3.5). This corresponds to popular private server versions (e.g. Kronos, TurtleWoW, Atlantiss, Warmane) and any self-hosted server based on MaNGOS/TrinityCore for those expansions. (Modern retail WoW is not supported due to vastly different code and anti-cheat mechanisms.)
* **Questing and Combat AI:** Bots come with class-specific “profiles” that dictate their combat rotations and behaviors. For example, a Frost Mage bot knows how to cast frost spells, kite enemies, and use food/drink to recover mana. All provided class profiles (e.g. FrostMageBot, etc.) are functional and can be customized. The AI uses a state-machine approach – e.g. states for *Idle*, *Patrol*, *Combat*, *Rest*, *Dead* – to manage behavior. Bots will engage hostile mobs, use spells/abilities, loot corpses, and even retreat or rest when needed.
* **Navigation and Pathfinding:** To move convincingly and avoid obstacles, WWoW uses a navigation mesh system. You’ll generate **movement maps (navmeshes)** from the game data, which the bot uses for pathfinding. Bots can intelligently move toward targets, roam between towns and wilderness, and navigate around terrain features. The pathfinding is based on compiled game world data (similar to Recast/Detour algorithms) to ensure the AI knows where it can walk or where obstacles are.
* **Hotspots and Grinding Areas:** WWoW defines **hotspots** – locations in the game world optimal for certain level ranges or objectives. Each hotspot includes waypoints for the bot to patrol, the level range of monsters, and references to nearby NPCs (like innkeepers or vendors). Bots can switch between hotspots as they level up, creating a progression (e.g. starting in a newbie zone, then moving to tougher areas as they gain levels).
* **NPC Interaction (Vendors/Repair):** Bots can interact with non-player characters for basic needs. For example, when gear durability gets low or bags are full, a bot will travel to a repair vendor or shop to sell junk and restock ammunition. Innkeepers are known to the bot as well, potentially for setting hearthstones or buying food/drink. This ensures the AI can sustain itself over long play sessions without human help.
* **Death and Recovery:** If a bot character dies (for example, overwhelmed by enemies), it will automatically handle corpse retrieval and resurrection. It uses the navmesh to navigate from the graveyard back to its corpse. (If the area is too dangerous or the death point is unreachable, the bot may respawn at a spirit healer after a timeout.)
* **Persistence and Data Logging:** WWoW can log and persist world data via a database. By default, it can use an **SQLite database** stored locally (no setup required) to remember things like discovered NPCs, hotspots, blacklisted troublesome mobs, etc. Alternatively, it supports **Microsoft SQL/Azure SQL** for cloud data storage. On first run, the necessary tables (for tracking NPCs, hotspots, bot commands, etc.) are automatically created in the database. This persistent data allows bots to “remember” important info across sessions and enables analysis of bot behavior over time.
* **Discord Integration (optional):** WWoW includes optional Discord bot integration. If enabled with your Discord Bot Token and server info, the system can send notifications or accept simple commands via a Discord channel. This can be used to monitor your AI players remotely (e.g. get notified of level-ups or deaths) or to issue commands like pausing the bots. (Discord integration can be turned off in settings if not needed.)
* **Stealth Operation:** The bots run inside the WoW client process itself, which not only gives them direct access to game functions but also makes them harder to detect by the game’s anti-bot measures. In fact, WWoW disables the legacy Warden anti-cheat in older clients upon injection. The approach is similar to how some cheat bots work, but here it is purposed for creating a believable simulation rather than gaining unfair advantage in competitive play.
* **Extensibility:** The architecture is modular. You can create new **bot profiles** (for classes or even custom behaviors) by implementing the AI logic for that profile (e.g., how a Warrior fights vs. how a Mage fights). You can also extend the system with new types of agents or scripts – for instance, creating a “QuestingBot” that completes quests, or a “ChatBot” that engages in in-game chat using AI. Developers can use the provided API of the bot to get information about the game state (e.g. player health, nearby units, inventory) and take actions (move, attack, cast spells, etc.).

**Technical Highlights:** Under the hood, WWoW is written in C#/.NET 4.6 with some C++ components. A custom **bootstrapper** program uses Windows API calls to launch the WoW client and **inject a DLL** into it. This DLL bootstraps a .NET runtime inside the game process and loads the bot’s managed code, effectively “plugging in” our AI agent into the game’s memory space. Once injected, the bot uses memory reading/writing and function calls to control the game (e.g. to move the character or cast spells directly via game functions). The project also makes use of third-party libraries and data:

* **Navigation Meshes:** Precomputed move maps (you’ll generate these from WoW’s game data) to allow pathfinding.
* **Newtonsoft JSON:** Used for configuration files and profile definitions.
* **Azure Cloud Services:** (Optional) for data storage and potentially analytics – e.g. Azure SQL database and planned integration with Azure AI services (see the Roadmap).
* **Discord .NET libraries:** For the Discord integration feature.

Finally, note that WWoW remains a **work-in-progress hobby project** – while many features work, there may be quirks or bugs, and not all WoW game content is handled yet. It’s already capable of basic leveling and combat autonomously, but making bots truly **human-like** in all aspects (grouping, chatting, complex quest logic, PvP tactics, etc.) is an ongoing effort.

## Installation and Setup

Follow these steps to get started with WWoW on your own machine:

1. **Prerequisites:**

   * **Windows PC** – WWoW runs on Windows and interfaces with the Windows WoW game client.
   * **World of Warcraft Game Client (Legacy):** You need a WoW client of one of the supported versions (1.12.1, 2.4.3, or 3.3.5a). You can use any vanilla/TBC/WotLK client, such as those from popular private servers or from your own MaNGOS/Trinity installation. Make sure you have the client executable (usually `WoW.exe`) and game data. (WWoW does *not* supply the game files.)
   * **Visual Studio 2022** (Community or higher) with C# and C++ workloads installed. The project is a Visual Studio solution that includes C# and C++ projects. Having the latest .NET Framework SDK and C++ build tools is required to compile the bot and injector.
   * **.NET Framework 4.6.1** targeting pack (Visual Studio should have this by default, as that’s the target framework for the C# projects).
   * (Optional) **Azure SQL or Local SQL Server** if you plan to use a cloud or external database for data logging. This is optional – by default WWoW can run with a local embedded database.

2. **Clone the Repository:** Download or clone the `WWoW` project (currently in the `QiMata/BloogBot` repo, will be renamed to WWoW). Open the solution file `BloogBot.sln` in Visual Studio.

3. **Build the Solution:** Restore any NuGet packages if prompted, then build the solution (set configuration to Debug or Release as desired). If the build fails, read the errors – you might be missing some components. For example, install any C++ platform SDKs or .NET targeting packs that Visual Studio suggests. A successful build will produce several binaries, including the injector executable and the bot DLL.

4. **Configure Settings:** Before running, configure the necessary settings:

   * **WoW Path:** Locate the file `Bootstrapper/bootstrapperSettings.json`. Edit the `"PathToWoW"` field to point to your WoW client’s executable file (for example: `"C:\\Games\\WoW-1.12.1\\WoW.exe"`). This is the program the injector will launch.
   * **Bot Settings:** Open `BloogBot/botSettings.json`. Here you can configure:

     * `DatabaseType` and `DatabasePath` – for database logging. For easiest setup, use `"DatabaseType": "sqlite"`, and WWoW will create a local `db.db` file automatically on first run. If you prefer to use SQL Server/Azure, set `"DatabaseType": "mssql"` and put your connection string in `DatabasePath` (e.g., `"DatabasePath": "Server=myserver.database.windows.net;Database=MyWoW;User Id=...;Password=...;"`). The bot will auto-create the necessary tables in whichever database you point it to.
     * `DiscordBotEnabled` and Discord tokens/IDs – if you want to enable the Discord integration, set this to `true` and provide your bot token and the IDs for the guild, channel, and role it should use. For initial setup you can keep this disabled (false).
     * Other behavior tweaks: `Food`/`Drink` item names (your character will use these for regen), `TargetingIncludedNames` or `ExcludedNames` (to focus or avoid certain mobs by name), `CreatureType...` booleans to filter target types (beasts, undead, etc.), `LootQuality` filters, and so on. These let you customize what the bot will attack or loot. The defaults are reasonable for general leveling.
     * **Choose a Profile:** You should also decide which bot profile (class script) to run. By default, the solution might be set up with a particular class (e.g., FrostMageBot). If you want to use a different class, make sure its project is built and adjust the startup as needed (or compile your own profile DLL). Profiles are typically separate class libraries that the main bot loads. For simplicity, stick to one of the included profiles to start (mage, priest, etc.) – you’ll need a WoW character of that class to use it.
   * **Movemaps/Navmesh Data:** Follow the instructions on generating navmesh (movement map) data for the game world. In short, you’ll use a tool (the original author provides one in an article) to process WoW’s map files (MPQ data or client data) and produce a set of mesh files or a database that the bot uses for navigation. Place the resulting navigation data where the bot can access it. (Typically, `DatabasePath` might point to a folder or DB for nav data if using a separate nav system, or the nav data might be baked into the bot’s database tables – consult the documentation for details. In the current setup, nav meshes are likely read from the Azure/SQL database or from files generated by the earlier mentioned process.)
   * **Action Bar Setup:** (For Vanilla 1.12 clients) Ensure your character’s **Auto-Attack ability is placed in the rightmost slot of your main action bar**. This is a quirk of the bot: it expects auto-attack there to initiate combat properly in Vanilla. For TBC/WotLK clients, auto-attack might be automatically handled, but it’s good practice to slot it as described if applicable.

5. **Launch the WWoW Bot:** You have two ways to start:

   * **From Visual Studio:** Set the startup project to `Bootstrapper` (this is the injector executable). Press F5 (or Run). This will load the bootstrapper, which in turn launches the WoW client and injects the bot. You should see the WoW game window appear – log in to your server of choice and enter the world with your character. The bot will usually begin operation once your character is in the world.
   * **From Command Line/Explorer:** After building, navigate to the output `Bin` folder and run `Bootstrapper.exe` directly. Make sure `Loader.dll` and `BloogBot.exe` (the bot DLL and main module) are in the same folder along with the `botSettings.json` and other necessary files. Running `Bootstrapper.exe` will open the WoW client as configured and inject the bot similarly.

6. **In-Game Behavior:** Once in-game, if everything is set up, the bot will typically take a few seconds to initialize. You might see your character start to move on its own. By default, the bot will load the profile for the class corresponding to your character. It will target the nearest suitable enemy and begin its combat rotation. You can watch as it kills mobs, loots them, rests to eat/drink when low on health or mana, and continues. If you have multiple hotspots configured (and stored in the database), the bot will move to the next area when appropriate (e.g. if it has outleveled the current spot).

   * You can still **override or control the character** at any time by pressing keys – but note the bot might assume control back quickly. It’s recommended to let the AI drive to observe its behavior. If you need to stop the bot, you can simply close the WoW client or use the Discord command (if configured) or stop the Bootstrapper.
   * Monitor the console/logs: The bot may output status info to a console or log file (depending on implementation). In Visual Studio’s output or a log window, you might see messages like “Targeting X”, “Casting Frostbolt”, “Moving to hotspot Y”, or errors if something goes wrong. This can help debug if the bot isn’t doing what you expect.

7. **Troubleshooting Setup:**

   * If the WoW process launches but the bot doesn’t seem active, ensure that your game client version is correct and was detected. The bot checks the client’s version on startup to apply the correct memory offsets for that expansion. If it fails to recognize it, it might not inject properly (the default config is for Vanilla if not detected). Make sure you’re using a supported build (check the `.exe` file version matches exactly 1.12.1 (5875), 2.4.3 (8606), or 3.3.5 (12340)).
   * If you get errors about missing DLLs or references, double-check that all subprojects in the solution built correctly. The C++ `Loader.dll` is especially important – if it fails to compile, the injection won’t work. You may need to install the Visual C++ v142 toolset or Windows SDK.
   * Database connection issues: If using Azure/local SQL and the bot crashes on startup, it might be unable to connect to your DB. You can disable DB usage by switching to SQLite mode to test if that’s the problem. With SQLite, ensure the program has write permission in its folder to create `db.db`.
   * Discord issues: If Discord integration is on and misconfigured, the bot might hang or error on startup (waiting for Discord client). If so, disable it (`DiscordBotEnabled: false`) and try again, then configure properly.
   * Anti-virus or firewall: Injecting into another process can trigger security software. Ensure your setup isn’t blocking the bootstrapper. It might help to run Visual Studio or the .exe as Administrator.

Once the setup is confirmed, you effectively have your own small “Westworld” in Azeroth! The AI character will carry on battling and leveling. You can even set up multiple bots (by running multiple game clients and injectors) to populate the world with a party of AI adventurers.

## Usage Examples

What can you do with WWoW? Here are a few scenarios to illustrate how the platform might be used:

* **Single AI Adventurer:** Run one bot on a private server. For example, start a Human Mage at level 1 in Northshire Abbey. The WWoW bot will automatically accept the introductory quests (if questing logic is present or via grinding it will level up), kill wolves and kobolds, loot them, and level up. It will use Frost Nova and Fireball according to its FrostMage profile logic. As it gains levels, it may move to new areas (e.g. Goldshire) following the configured hotspots. You can observe this character progressing through the world hands-free. To a nearby human observer, the character looks just like any other player going about their leveling journey.
* **Party of Bots (PvE Exploration):** You could configure multiple bots to run together, simulating a full party. For instance, run a Warrior bot as a tank, a Priest bot as a healer, and several DPS bots. With additional scripting, they could even coordinate (e.g., assisting the warrior’s target). They could collectively take on harder content like dungeons or elite monsters. This is excellent for testing how AI agents can cooperate and fulfill MMORPG group roles.
* **Mixed Reality Server:** Host a small WoW server where a few human friends play alongside bot players. The bots might fill the world to make it lively: some could be grinding in the fields, others could be wandering in town. As a human player, you can trade with them, group up, or duel them. Ideally, the bots behave naturally enough that your friends might not immediately realize that “Nightelf Hunter Jane” over there is AI-controlled. This scenario brings the *Westworld* concept to life – a blend of real and AI characters sharing a virtual world.
* **AI Behavior Research:** Use WWoW as a research environment. For example, you could log all bot actions and state transitions to the database for offline analysis (thanks to the data logging feature). Researchers could analyze this data to find patterns or train machine learning models. One might replace the built-in decision-making with a custom AI (e.g., a reinforcement learning agent that learns to optimize XP gain, or a language model that generates in-game chat messages to make the bot seem more social). WWoW provides the scaffolding to plug in such experimental AI modules within a real game world.
* **Headless Simulation:** Although currently the bots use the actual game client for all rendering and physics (since they run in the client), one could imagine running many bots on a server in a headless fashion for load testing or large-scale simulations. For instance, spawn 50 AI players on a test realm to see how the server and game economy react. (Achieving this would require further development to allow multiple clients or a headless mode, see Roadmap.)

**Interacting with Bots:** At present, bot characters will respond to the game world (combat, NPCs) but have limited social interaction with players. They won’t initiate chat on their own (unless you extend their code to do so). However, one could extend WWoW to give bots conversational abilities using AI (for example, integrate an NLP model so they can respond to whispers or messages). Part of the vision is to eventually have bots that *talk* and *group* like players. For now, you might use Discord or the console to issue commands to bots (e.g., telling a bot to pause, or go to a certain location). As a player, you can also try to `'/follow'` a bot or invite it to a party (if programmed, bots could accept invites). These interactions are areas for future improvement.

In summary, WWoW usage can range from a passive observer (watching your AI toon do its thing), to an active participant in a mixed world, to a developer tweaking AI algorithms. It’s a sandbox – feel free to experiment!

## Development and Contribution Guidelines

Contributions to WWoW (Westworld of Warcraft) are welcome! This project is at the intersection of game development, AI, and systems programming – there are many ways to improve it. If you’d like to help:

* **Project Structure:** Begin by familiarizing yourself with the code layout. Key components include:

  * `BloogBot` (core bot logic library in C#) – contains game object models, state machines, navigation, combat logic, etc.
  * `Bootstrapper` (C# injector exe) – launches the game and injects the bot.
  * `Loader` (C++ DLL) – responsible for starting the .NET runtime inside WoW’s process.
  * Profile projects (e.g. `FrostMageBot`, `HolyPriestBot`, etc.) – each is a DLL implementing a specific class rotation/behavior.
  * `Navigation` (if present) – might include tools for pathfinding or imported navmesh data.
  * `Docs` – any documentation or SQL schema files (e.g., `SqlSchema.SQL` and `SqliteSchema.SQL` define the database structure used).

* **Coding Guidelines:** We follow standard C# coding styles for the managed code and typical C++ practices for the injector. Ensure any new code is well-documented and tested. Because this is a hobby/research project, we value clarity and experimentation over strict style – but try to match the general structure of existing code for consistency. For example, if adding a new bot behavior, see how existing states and profiles are implemented.

* **Submitting Changes:**

  1. Fork the repository (or work on a feature branch if you have access).
  2. Create an Issue if your contribution is significant or changes behavior, to discuss with maintainers and community first. This is especially recommended for big features (e.g. “Implement group coordination AI” or “Integrate GPT-4 for chatbots”).
  3. Make your changes in a new branch, with descriptive commit messages. If fixing a bug, reference any issue number in commits.
  4. Ensure the project builds and runs after your changes. Ideally test with at least one WoW client to confirm nothing broke (e.g., the bot can still inject and move/fight).
  5. Submit a Pull Request to the **main branch**. Describe what your PR does, and any steps to test it. Include screenshots or logs if appropriate (for example, showing a new feature in action).
  6. Be patient for review. Since this is a personal/opensource project, reviews might not be immediate. Community members or maintainers might provide feedback or ask for adjustments.

* **Contribution Ideas:** Not sure what to work on? Some sought-after contributions:

  * New **Class Profiles** or improvement to existing ones (better combat rotations, support for more spells, smarter tactics).
  * **Questing AI:** enabling bots to complete quests (requires parsing quest logs, navigating to objectives, handling quest items).
  * **Social AI:** give bots the ability to respond to player interactions – e.g. chat back when spoken to, join groups, participate in trades or auctions.
  * **Advanced Navigation:** improvements to the pathfinding – maybe integrating dynamic avoidance (not walking into crowds of enemies recklessly) or using transport (fly/boat).
  * **Performance and Scalability:** optimizing the code to run more bots per machine. Perhaps allowing multiple bots in one process or headless operation with a virtual game world.
  * **Bug Fixes:** If you encounter crashes or bugs (e.g., certain abilities not working, or bot getting stuck), tracking down and fixing those is extremely valuable.
  * **Documentation:** Enhancing documentation, tutorials, or even writing research papers/blogs on experiments done with WWoW.

* **Community and Support:** We encourage you to join the conversation. You can use the project’s GitHub Issues for support questions or the discussion forum (if enabled). Additionally, the original BloogBot had a Discord server for hacking on the project – WWoW users may find support there or we might establish a dedicated WWoW Discord if interest grows. Sharing your use-cases and successes (or failures) will help shape the project’s direction!

* **License:** The project is open-source (MIT License). Any contributions must be compatible with this license. By contributing, you agree that your code will be MIT-licensed as well. This permissive license allows both academic and commercial use, as long as copyright notices are maintained.

We appreciate any form of contribution, be it code, ideas, or simply bug reports. Let’s build this AI-driven world together!

## Roadmap

WWoW is an ambitious project, and there’s a lot on the horizon. Here’s a high-level roadmap of where we plan to go next, including integration with cutting-edge AI and data platforms:

* **Short Term (Current Focus):**

  * **Stability and Core Features:** Iron out remaining bugs in basic bot behaviors (combat, navigation, staying alive). Ensure each class profile can at least level from 1 to 20 unattended as a proof of concept. Improve the database of hotspots and NPCs for more zones, so bots can travel world-wide.
  * **AI Foundry Integration (Phase 1):** Begin connecting WWoW to **Azure AI Foundry**, an Azure service for building and managing AI agents. Initially, this might involve using Foundry to manage our bot profiles or agent logic in a more modular way. For example, offloading certain decision-making processes to an AI Foundry agent that can be updated or trained externally. This could let us experiment with more advanced AI controllers without embedding everything in the game client.
  * **Data Logging with Microsoft Fabric:** Leverage **Microsoft Fabric** (Microsoft’s unified analytics platform) to collect and analyze gameplay data from WWoW. Every action an AI bot takes, every event (kill, death, item looted, interaction) could be funneled into a Fabric data pipeline. This will allow for big-data analysis of AI behavior. We plan to create dashboards and reports (using Fabric’s data warehousing and Power BI integration) to visualize how bots perform over time or identify patterns (e.g., places where bots die frequently or inefficient paths taken).
  * **Agent Behavior Enhancements:** Implement more nuanced behavior rules: for instance, making bots choose between grinding vs. questing, or having different “personalities” (aggressive vs. cautious playstyles). This may tie in with AI Foundry if we treat each bot as an agent with a profile that can be tweaked or even a learning agent updated through Azure.

* **Mid Term (Next Steps):**

  * **Conversational Bots:** Integrate NLP models (possibly via Azure OpenAI Service or similar) so that bots can engage in basic conversation. The goal is for an AI player to respond to a hello or even answer simple questions (“Where is the blacksmith?”) in a plausible way. AI Foundry could host a language model agent that WWoW bots query when they need to generate a chat response. This will significantly increase human-bot indistinguishability if done well.
  * **Group Dynamics:** Teach bots to cooperate. Using data from Fabric about how human parties tackle content, we can script or train bots to assume roles in a party (tank, healer, DPS coordination). This includes understanding threat mechanics, assisting each other, and possibly even forming ad-hoc groups with players or other bots when facing tough challenges.
  * **Event and Quest Simulation:** Expand the bot’s capabilities beyond combat: for example, participating in in-game economies (auction house auto-buying/selling using economic agents), joining PvP battlegrounds with rudimentary tactics, or dynamically generating quest-like tasks for bots to “pursue” (giving them goals beyond just grinding). This might involve integrating with game server APIs or using external logic to feed objectives to bots.
  * **Scalability & Cloud Gaming:** Investigate running larger-scale simulations. This could involve orchestrating multiple game clients in the cloud (perhaps using virtualization or a custom headless server that mimics a client). Microsoft Fabric could help coordinate this by provisioning compute for bot instances and aggregating their data. Imagine a fully automated test server with 100+ bot players populating the world – this could be used to test server load or emergent behaviors when many AI agents interact.
  * **Microsoft Fabric Data Agent:** Utilize Fabric’s Data Agent to stream data directly from the game environment (via the bots) into Fabric’s lakehouse. This real-time data could then be used to adjust agent behavior on the fly. For example, if analysis shows a particular grinding spot is overcrowded (many bots converging), an AI coordinator (maybe hosted in AI Foundry) could assign some bots to move to different areas. This begins to resemble a higher-level “AI director” managing the population of bots.

* **Long Term (Vision):**

  * **True Westworld Experience:** Achieve a state where a human can join a WWoW server and truly not tell if others are bots or people. This means polishing all aspects of bot behavior: natural movement (no bottish jittering or super-linear paths), human-like decision delays and mistakes, engaging conversation, perhaps even creative play (like occasionally doing silly things, or participating in server events). It’s a Turing Test in Azeroth.
  * **Learning Agents:** Incorporate reinforcement learning or other advanced AI that *learns* from the environment. We could use the logged data (via Fabric) to train models that optimize how bots play (for example, learning the optimal grinding spots or combat strategies). Over time, bots could become smarter and adapt to player tactics (in PvP scenarios, for instance).
  * **Cross-Platform / Other Games:** While currently built around World of Warcraft, the principles of WWoW could extend to other virtual worlds. A long-term possibility is to abstract the “Agent in MMORPG” core and apply it to different game environments (imagine Westworld-like simulations in other MMO games or open-world games). Microsoft Fabric’s analytics and Azure AI Foundry’s agent management could make it easier to plug into new worlds by swapping out environment data and retraining AI models.
  * **Community-Driven Worlds:** We hope to involve the community in hosting WWoW servers where bot and human interactions can be observed in the wild. Insights from these could drive further development. Perhaps competitions or Turing-test like events could be held, challenging players to identify bots – pushing us to improve them further.

The roadmap above is ambitious, and not set in stone. As an open-source project, progress will depend on contributions and discoveries along the way. Integrating **AI Foundry** and **Microsoft Fabric** is an exciting avenue – it brings enterprise-grade AI and data tools into the mix, which can supercharge the intelligence of our agents and our understanding of them. We especially look forward to how **agentic behavior** can evolve: from scripted state machines to autonomous, adaptive agents that truly feel “alive” in the game world.

Stay tuned for updates in the repository. If you’re interested in any of these roadmap areas, please reach out or jump in – help us build the Westworld of Warcraft!

## Getting Help and Further Information

For newcomers, the amount of moving parts (WoW clients, AI code, databases, etc.) can be daunting. Here are some resources and tips:

* **Documentation:** We are working on more documentation in the `Docs/` folder. You can find a basic FAQ and perhaps guides there (e.g., a FAQ might address common setup issues).
* **Original Project Writings:** Much of WWoW’s core is based on the BloogBot project by Drew Kestell, who wrote a series of blog posts on how it works. Those articles are a great way to understand the technical inner workings (memory hacking, pathfinding, etc.). We plan to curate and include some of that information in our documentation as well.
* **Community Forums/Discord:** As mentioned, consider joining the discussion via our (to-be-formed) Discord or via the original BloogBot Discord. There may be others out there working on similar projects or who have used BloogBot/WWoW and can share knowledge.
* **Issues on GitHub:** If you run into problems, you can search the issue tracker to see if it’s known, or open a new issue describing your situation. We (the maintainers) or community members will do our best to help.
* **Safety and Ethics:** Remember, this project is for learning and fun. Please use it responsibly. Do **not** use WWoW (or its predecessor BloogBot) to disrupt real servers or cheat in live games – not only is that against terms of service of virtually all games, but this project is intentionally limited to older clients and private settings to avoid those concerns. We encourage experimentation in controlled environments where everyone involved consents to bots being present.
* **Have Fun:** At its heart, WWoW is a labor of love merging gaming and AI. Whether you’re here to build Skynet for Azeroth or just to watch a Rogue bot foolishly try to pickpocket a dragon, we hope you enjoy the journey. Feel free to share stories of cool (or hilarious) things your bots did!

---

*Welcome to Westworld of Warcraft. Build an army of AI heroes, explore new horizons in AI-driven gameplay, and help us create a virtual world where the line between player and program blurs.*

Happy adventuring, both to the humans and the algorithms!&#x20;
