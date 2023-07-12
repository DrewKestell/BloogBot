# FAQ

## 1. Is there a compiled version of BloogBot available for download?
No. You'll have to clone the repo and compile it yourself. Follow the instructions in the git repo README.

## 2. Does BloogBot work on retail/classic WoW?
No. Currently BloogBot is only tested on Vanilla/TBC/WotLK legacy clients. See the git repo README for more details.

## 3. Why does BloogBot depend on a database?
The bot uses the concept of "hotspots", which are a collection of positions in the game world, that the bot bounces between to grind mobs. You have to create a hotspot using the bot's UI, then select a hotspot, before starting the bot. Hotspots are stored in a database. The reason I decided to do this was so my friends and I could all share the same grinding hotspots by connecting to a cloud database. In theory, the bot could also work with a local database, or even a local .json or .xml file, but this would require some changes to Repository.cs and likely some other spots.

## 4. Can I use BlootBog with a local database (like SQLite) or a .json file?
Today the bot supports SQLite and TSql (local sql database or Azure sql will work). The repository was written in such a way that it should be easy to add new database providers, but it will require writing a new repository subclass.

## 5. I cloned the repo and installed all required dependenies, but the application doesn't start correctly.
Make sure you have Bootstrapper.cs set as your startup project in Visual Studio. Make sure you're running Visual Studio as administrator. Make sure all projects have compiled successfully (sometimes the C++ projects don't compile when you compile the solution, make sure the C++ projects are compiled.). Make sure you have all required dependencies installed (VS2022, C++ build tools, .NET 4.6.1 runtime and SDK). Try debugging the Loader library. If you still can't get it to work, ask in Discord.

## 6. I'm a programming noob, I want a bot, is BloogBot for me?
Probably not. Please go read the README in the git repo. The purpose of this project is to learn about reverse engineering and game development, not to get ahead in WoW. If you're looking for a community to ask questions and learn more about this kind of programming, you've come to the right place.

## 7. Is the author of BloogBot available for contract work?
Nope.

## 8. How can I proceed with the "Create required Azure Infrastructure" step in the git readme?
In its current state in the master branch, BloogBot depends on a database to store data (hotspots, npcs, etc) that are required by the bot at runtime. A connection string to this database is configured in botSettings.json. Repository.cs connects to this database and queries to retrieve the required data at runtime. See https://github.com/DrewKestell/BloogBot/blob/main/SqlSchema.SQL for a script to create the required tables with the appropriate schemas. You can either use an Azure SQL database or host your own database on a local machine. Note that different versions of SQL have different schemas for connection string, and not all versions of SQL are supported by the Nuget package used in BloogBot to connect to SQL. This part of the project requires some programming experience - if you need help, ask in Discord.

UPDATE: BloogBot now supports different types of databases. Review the code around the repository classes.

## 9. Is BloogBot detectable by Warden?
The currently implentation hooks MemoryScan, PageScan, and ModuleScan. Based on my experience and observation, it seems to be undetectable via Warden on most private servers. However, I have not seen the anticheat code running on these servers, and based on rumors I've read, the bigger servers are using anticheat code that not only relies on Warden, but also relies on analytical / heuristic detection strategies that are very difficult (impossible?) to circumvent. TLDR: use this at your own risk, botting is NEVER safe. Never use this on an account you care about.

## 10. What should I do with the settings `DiscordBotToken`, `DiscordGuildId`, `DiscordRoleId`, `DiscordChannelId`?
BloogBot has Discord integration that does things like notify you when you level up, when your bot is stuck, when you find a blue item, etc. To learn how to build a simple Discord bot, you'll need to create a Discord account, and start reading the docs [here](https://discord.com/developers/docs/getting-started). If you don't care about this, you can set DiscordBotEnabled to false in botSettings.json. The bot doesn't depend on any of this to function properly.

## 11. How do I generate movemaps?
Start by reading [this](https://drewkestell.us/Article/6/Chapter/20) to learn how to do it yourself. If you're stuck, you can downloaded the pregenerated movemaps from my google drive [here](https://drive.google.com/file/d/1w8EH25diV0A_sbFBUw063oWIafqg5fOP/view?usp=drive_link). These were generated using the WotLK client. Note that the movemap generator utility from Mangos that I used has a bunch of parameters to tweak how movemaps are generated, like the max walk angle, offmesh links, etc. So you may get better results by generating them yourself.

## 12. Does BloogBot provide an auto-rotation feature?
No. It would be easy to write one inside of BloogBot though.

## 13. How do I get help?
Watch the [tutorial video](https://www.youtube.com/watch?v=g3jYHiajQdk), then join the [Discord server](https://discord.gg/S4tvykaGcJ).

## 14. The bot is crashing randomly, how do I debug?
The best way is to wrap the entire bots execution loop in a try catch. In your catch you can dump the exception and stack trace to the console. There are certain kinds of exceptions that are tricker to catch. Some are impossible, like stack overflow. Access violation exception is possible to catch but requires you to annotate your code with an attribute. Read more [here](Well, the best way is to wrap the entire bots execution loop in a try catch. In your catch you can dump the exception and stack trace to the console. There are certain kinds of exceptions that are tricker to catch. Some are impossible, like stack overflow. Access violation exception is  possible to catch but requires you to annotate your code with an attribute. Read more [here](https://stackoverflow.com/questions/7392783/list-of-exceptions-that-cant-be-caught-in-net) and [here](https://stackoverflow.com/questions/3469368/how-to-handle-accessviolationexception). You can also attach the debugger to visual studio while your bot is running and wait for it to crash. Just note that in visual studio you can configure which exceptions will break execution, and not all may be selected by default. So you might have to go into the debugger settings and select them all.

## 15. Navigation is not working, I'm getting errors generating movement paths, etc.
Make sure you generated movemaps using the same version of recast/detour that I have included in BloogBot. Newer versions of Mangos may use newer versions of recast/detour, and if you use those Mangos map gen utilities, Bloog Bot's navigation system won't work due to breaking changes in the recast/detour APIs.