# BloogBot

Join the [BloogBot Discord Server](https://discord.gg/S4tvykaGcJ) to chat with other folks hacking on BloogBot!

BloogBot is an in-process bot for the Vanilla (v 1.12.1), Burning Crusade (v 2.4.3), and Wrath of the Lich King (v 3.3.5) clients.

I have written extensively about the project [on my website](https://drewkestell.us/Article/6/Chapter/1).

*IMPORTANT NOTE*: Due to implementation differences between the various WoW server emulators out there (MaNGOS, TrinityCore, AzerothCore, etc), I can't promise the bot will work consistently across all servers. As of 12/3/2022, I've successfully tested the bot against the following servers:

- Kronos (Vanilla 1.12.1)
- TurtleWoW (Vanilla 1.12.1)
- Atlantiss (TBC 2.4.3)
- Warmane (WotLK 3.3.5)

A few more important notes:
- This is a hobby project, and as such, the quality of the code is as you'd expect. If you find bugs, fix 'em (and submit a PR)!
- This does **NOT WORK ON RETAIL**. As mentioned in the writing on my website, the purpose of this project is intellectual exploration, not exploitation. I have no interest in monetizing the bot for current versions of the WoW client. And in fact, Blizzard's anticheat has likely gotten so sophisticated that it's beyond my technical ability. So this bot will only work on the old versions of the WoW client. It'll work on the various Vanilla/TBC/WotLK private servers out there, or on a MaNGOS install you set up yourself.
- I used to have two completely separate code bases - one for v1.12.1, and one for v2.4.3 of the WoW client. I recently merged them into a single codebase. TBC should be fairly stable. Vanilla and WotLK have been tested with a few basic scenarios using a few class profiles, but there are likely bugs that need to be fixed.
- There are a few external dependencies that you'll need to wire up if you want to compile and run this yourself:
  - You'll need to compile movemaps to facilitate the bot's navigation through the game world. See [this article from my website](https://drewkestell.us/Article/6/Chapter/20) for more info
  - You'll need to modify some values in `botSettings.json`. Note that you can disable Discord integration by setting `"DiscordBotEnabled"` to `false` in botSettings.json.
    - DatabasePath
    - DiscordBotToken
    - DiscordGuildId
    - DiscordRoleId
    - DiscordChannelId
- The code, in its current state, depends on an Azure SQL database with a few tables created. Sorry, but you can't connect to mine. So to run this yourself, you'll have to either disable/modify that code, or create the required Azure infrastructure yourself. `Repository.cs` is a good place to start if you want to understand which tables need to be created (and their schemas). Eventually, I want to create an [ARM Template](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/overview) to simplify deploying all the required cloud infrastructure, but it doesn't exist yet. In the meantime, you can reference SqlSchema.SQL in the repo root to see the schema for all the tables you'll need.'
- All of the bot profiles (ie: FrostMageBot, etc) should _mostly_ work. But due to emulation inconsistencies across the various private servers out there, you may run into issues. I suggest creating your own bot profile and experimenting with creating your own combat rotation yourself.
- The Vanilla implementation assumes you have the Auto-Attack spell in your far right spot on your first, default action bar.

## Getting Started

- To get started working with this yourself:
  - Clone the repo and open BloogBot.sln in Visual Studio (v2022 ideally)
  - Build the solution - if you get compiler errors, you're likely missing some SDK / framework dependencies. Check the errors, consult Google, and use the Visual Studio installer to install any missing dependencies. For example, you'll definitely need some C++ Build Tools if you don't have them installed already.
  - Create required Azure infrastructure (alternatively, you can use a local sql database, or sqlite). Add your connection string to botSettings.json and a script should run to scaffold the necessary tables the first time you run the bot. Ask in Discord if this doesn't work for you.
  - Install version 1.12.1, 2.4.3, or 3.5.5 of the WoW client.
  - Update values in `bootstrapperSettings.json` and `botSettings.json`
  - Generate movemaps and dump them into <repo>\Bot\mmaps (see FAQ for details).
  - Set Bootstrapper as your startup project, and fire up your debugger. You should see Wow.exe launch, and then you'll be prompted to attach a debugger to Visual Studio. To learn more about the overall flow of how the bot attached to the WoW process, [read my website](https://drewkestell.us/Article/6/Chapter/1)
  - View the documentation in the Docs folder to learn more.
  - Watch the [tutorial video](https://www.youtube.com/watch?v=g3jYHiajQdk).
  - Read the [FAQ](https://github.com/DrewKestell/BloogBot/blob/main/Docs/FAQ.md) for common troubleshooting answers.
  - Join the Discord and ask for help if you're stuck.

## Motivation
  
To explain why I did this, I refer to the first chapter of my website:

> "Low-level programming is good for the programmer's soul." - John Carmack

> I love video games. I remember when my dad brought home a Commadore 64, but I was still too young to use it without his help. From there, our first console was a Nintendo Entertainment System. But things really got serious when we got our first PC. Some of my fondest memories in gaming come from games that ran on MS-DOS. Of course we had Doom, which was fantastic. But I especially loved this "1001 Games" CD. Plenty of the games were trash, and some didn't run on our hardware, but there was a seemingly infinite amount of entertainment to be had. Eventually my mind was blown by titles like Quake, Warcraft, Diablo, and Ultima Online, all of which had incredibly innovative multiplayer experiences.

> Cheaters have existed for as long as games have. Anybody that has played a multiplayer game has likely been exposed to a hacker taking some form or another. These hackers, and the tools they used, had always seemed to me like an enigmatic underbelly of the internet. There's nothing worse than getting wrecked by a cheater in a competitive match. But disdain wasn't the only emotion those experiences evoked. They also made me curious. How did these cheats work? Who was building them? I had experimented with Diablo trainers and the like when I was younger, but the knowledge to learn how to create something like that was far beyond my capabilities.

> Having made a career of Web Development, with about 5 years of programming experience under my belt, I decided to take another crack at it. After some initial research, I started poking around on some forums, and I was blown away by just how sophisticated some of these techniques truly are, and I gained a new respect for these hackers. So much of what they were talking about was still way over my head. There was some seriously low level computer science concepts involved. Abstraction is a powerful thing - I was amazed at how far I had gotten in my career without truly understanding some of the fundamental concepts that formed the foundation of every piece of software I built. It's easy to take for granted just how good our high level programming languages are, and how impressive modern compilers have become.

> I found some great resources - the x86 Assembly Wikibook helped me understand the basics of CPU architecture, and the journey your source code takes before it's executed by your CPU. The x86 Disassembly Wikibook is a great crash course in disassembly and reverse engineering. Ownedcore has some fantastic discussions about concepts such as DLL injection, assembly injection, interoperability, function detouring, and interacting with the Windows API. Learncpp has deep and thorough explanations of how pointers and memory management work (admittedly, I've read the book twice and my C++ still stinks, but some fundamental knowledge helps a lot with reverse engineering).

> After many late nights, and plenty of borrowed code from all over the web, I had built a fully functional World of Warcraft bot capable of fighting monsters for hours on end without any human interaction. Having made it this far, it's astonishing how much there is to learn. Many of these concepts were totally foreign to me coming from a background in Web Development, and I've barely scratched the surface. But the process has been extremely gratifying, and I believe without a doubt that it has made me a better developer. Thinking about how to make a bot behave more like a human was also an interesting glimpse into the world of AI.

> I had started and stopped this intellectual journey a number of times before I found a project that kept my interest for long enough to make any significant progress. Without a formal education in computer science, the task was incredibly daunting. So my goal is to try to distill some of the lessons I've learned over the past year, hopefully making the journey a little easier for anybody else that feels the same way.

> DISCLAIMER: I have serious ethical concerns about cheating in gaming. Especially with the blossoming world of eSports and competitive gaming, using hacks in games is guaranteed to ruin the experience for other players. That being said, I think there's a difference between a bot in World of Warcraft who runs around fighting with the AI, and an aim bot in Counter-Strike that instantly destroys other players. I chose World of Warcraft not only because it's a game I loved when I was younger, but also because it's fairly (not completely) harmless to other players. I made a conscious effort to design the bot in such a way that it won't damage the experience of other players on the server. I also tested the bot exclusively on a free, private server not run by Blizzard. Ultimately my motivation was curiosity, not to "get ahead" in the game, and I think that's an important distinction. Unfortunately, cheating in gaming isn't going anywhere. The bright side is that while the techniques discussed in this article can most certainly be used to create hacks, the same concepts can be used to inform the design of anticheat software, so I think it's information worth sharing.

> Before taking a look under the hood of the WoW bot, we'll first examine a rudamentary game engine I wrote in C++ called BloogsQuest that we'll use as a contrived example to explore some fundamental concepts that are important to understand before diving fully into bot development.

> I also want to mention that the code snippets found here will be truncated, and will most certainly deviate from best practices in software development. This is less a step-by-step tutorial in bot development, and more an exploration of the high level concepts involved. Some prerequisite knowledge of C# and C++ are necessary, including understanding pointers, but I'll do my best to explain things as they come up.
  
___

![image](https://user-images.githubusercontent.com/6411339/120980933-f7368a80-c72b-11eb-97ec-d82dd02094dc.png)

![image](https://user-images.githubusercontent.com/6411339/120980947-fbfb3e80-c72b-11eb-8aa1-35d24ebd5310.png)
