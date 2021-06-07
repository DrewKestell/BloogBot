# BloogBot

BloogBot is an in-process bot for the original Vanilla (v 1.12.1) and Burning Crusade (v 2.4.3) clients.

I have written extensively about the project [on my website](https://drewkestell.us/Article/6/Chapter/1).

A few important notes:
- This is a hobby project, and as such, the quality of the code is as you'd expect. If you find bugs, fix 'em (and submit a PR)!
- This does **NOT WORK ON RETAIL**. As mentioned in the writing on my website, the purpose of this project is intellectual exploration, not exploitation. I have no interest in monetizing the bot for current versions of the WoW client. And in fact, Blizzard's anticheat has likely gotten so sophisticated that it's beyond my technical ability. So this bot will only work on the old versions of the WoW client. It'll work on the various Vanilla/Burning Crusade private servers out there, or on a MaNGOS install you set up yourself.
- I used to have two completely separate code bases - one for v1.12.1, and one for v2.4.3 of the WoW client. I recently merged them into a single codebase, but I have only thoroughly tested v2.4.3 (Burning Crusade). The Vanilla client is likely broken at this point, but it shouldn't be too hard to get it working again. I'll try to fix it as time permits.
- There are a few external dependencies that you'll need to wire up if you want to compile and run this yourself:
  - You'll need to compile movemaps to facilitate the bot's navigation through the game world. See [this article from my website](https://drewkestell.us/Article/6/Chapter/20) for mroe info
  - You'll need to modify some values in `botSettings.json` (alternatively, you can find the code that references these and disable it if you don't care about the functionality):
    - DatabasePath
    - DiscordBotToken
    - DiscordGuildId
    - DiscordRoleId
    - DiscordChannelId
- The code, in its current state, depends on an Azure SQL database with a few tables created. Sorry, but you can't connect to mine. So to run this yourself, you'll have to either disable/modify that code, or create the required Azure infrastructure yourself. `Repository.cs` is a good place to start if you want to understand which tables need to be created (and their schemas). Eventually, I want to create an [ARM Template](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/overview) to simplify deploying all the required cloud infrastructure, but it doesn't exist yet.
- All of the bot profiles (ie: FrostMageBot, etc) should _mostly_ work. But due to emulation inconsistencies across the various private servers out there, you may run into issues. I suggest creating your own bot profile and experimenting with creating your own combat rotation yourself.

## Getting Started

- To get started working with this yourself:
  - Clone the repo and open BloogBot.sln in Visual Studio (v2019 ideally)
  - Build the solution - if you get compiler errors, you're likely missing some SDK / framework dependencies. Check the errors, consult Google, and use the Visual Studio installer to install any missing dependencies. For example, you'll definitely need some C++ Build Tools if you don't have them installed already.
  - Create required Azure infrastructure
  - Install version 1.12.1 or 2.4.3 of the WoW client (note that 1.12.1 is likely broken with the current code, and needs some work to bring it back to life)
  - Update values in `bootstrapperSettings.json` and `botSettings.json`
  - Generate movemaps and dump them into <repo>\Bot\mmaps
  - Set Bootstrapper as your startup project, and fire up your debugger. You should see Wow.exe launch, and then you'll be prompted to attach a debugger to Visual Studio. To learn more about the overall flow of how the bot attached to the WoW process, [read my website](https://drewkestell.us/Article/6/Chapter/1)
