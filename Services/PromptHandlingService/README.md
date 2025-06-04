# PromptHandlingService Overview

The **PromptHandlingService** module is responsible for automatically handling in-game prompts and dialog windows so that the BloogBot can run uninterrupted. In World of Warcraft, certain events (like NPC interactions or system requests) trigger UI prompts that normally require player input. This service intercepts those prompts and responds programmatically – for example, accepting a resurrection offer or declining a party invite – without human intervention. By centralizing prompt logic here, BloogBot ensures smoother automation and avoids duplicating prompt-handling code across the bot’s AI routines.

**Key Purpose:** PromptHandlingService monitors events such as NPC gossip dialogs, party or guild invitations, duel requests, resurrection offers, and other confirmation pop-ups. It automatically makes the appropriate choice (accepting or declining) based on the bot’s needs. This benefits **users** (the bot will not stall waiting for input) and **developers** (common prompt logic is in one place, making it easier to maintain and extend).

## Directory Structure

The `/Services/PromptHandlingService` directory contains the implementation of this feature. Its structure is organized into a main service class and individual prompt handler components:

* **`PromptHandlingService.cs`** – The central service class that initializes prompt handling. It hooks into the game’s event system and delegates events to specific handlers. This is essentially the controller that listens for any “prompt” events.
* **`IPromptHandler.cs`** – *(If present)* An interface or base class defining a prompt handler’s contract. For example, it may define a property for the event name and a method like `HandlePrompt(object[] args)` that each handler must implement.
* **`GroupInvitePromptHandler.cs`** – A handler for party/group invite requests. It listens for the `PARTY_INVITE_REQUEST` event and responds (by declining or accepting as configured).
* **`DuelRequestPromptHandler.cs`** – A handler for duel challenges (`DUEL_REQUESTED` event), automatically declining unwelcome duel requests.
* **`ResurrectionPromptHandler.cs`** – A handler for resurrection prompts (when another player attempts to resurrect the bot, via `RESURRECT_REQUEST` event). This typically **accepts** the resurrect so the bot revives immediately.
* **`SummonPromptHandler.cs`** – A handler for warlock summons (`CONFIRM_SUMMON` event), e.g. to accept or decline a summon. (By default, the bot might decline summons to avoid sudden relocations, but this could be configurable.)
* **`BindPromptHandler.cs`** – A handler for innkeeper “bind hearthstone” confirmations (`CONFIRM_BINDER` event), accepting the prompt to bind the hearthstone at an inn if the bot initiates it.
* **(Other handlers)** – The service can be extended with additional handlers. For instance, one could add a **GuildInvitePromptHandler** for guild invites (`GUILD_INVITE_REQUEST` event) or **LootBindPromptHandler** for “bind on pickup” loot confirmations. The design encourages adding new handlers as needed without touching core logic.

*Note:* Not every file may exist if some prompt types are not yet handled in this implementation – the above list covers typical prompts that the service is meant to manage. Each prompt handler is focused on one category of prompt for clarity and single-responsibility.

## Using the PromptHandlingService

**For Bot Users:** This service works automatically. There is no direct user interaction required – it runs in the background once the bot is started. As a user, you benefit from not having to manually click dialog options. For example, if someone invites your bot to a group, the service will immediately decline the invite for you. If a friendly player tries to resurrect your character, the service accepts the resurrection so your bot is quickly back to life. In short, PromptHandlingService removes the need for you to babysit these dialogs.

**For Developers:** The PromptHandlingService is initialized during the bot’s startup. You typically don’t need to call it manually. For instance, when BloogBot injects into the WoW client or starts running, it will call something like `PromptHandlingService.Initialize()` (either from the main program or Bot loader). This sets up event subscriptions. After that, the service will react to game events on its own. Developers can trust that common prompts are handled and focus on AI logic, knowing that the service will press “Yes” or “No” on the appropriate pop-ups.

Integration with the rest of the system is straightforward. The service leverages BloogBot’s existing game event pipeline. BloogBot’s core uses a `SignalEventManager` to capture WoW client events and route them into C# events. PromptHandlingService attaches to these signals to catch relevant events. For example, BloogBot already captures events like opening an NPC dialog or merchant window – when the WoW client fires a `GOSSIP_SHOW` event, BloogBot creates a `DialogFrame` object with available options and triggers an internal event. PromptHandlingService builds on this system by watching for events that indicate a prompt requiring input.

**How it integrates:** If you look at the bot’s main loop or initialization, you’ll see that the PromptHandlingService hooks into event notifications. It might subscribe to events via `SignalEventManager.OnNewSignalEvent` or a similar mechanism to get callbacks whenever WoW raises an event. Once initialized, it runs passively – you won’t typically call it from your combat routines or states. Instead, it listens globally and intervenes only when needed.

## Internal Design and Workflow

**Event-Driven Handling:** Internally, PromptHandlingService uses an event-driven design to catch prompts as soon as they appear. The WoW client triggers specific event names for various prompts. The service listens for those event names (e.g., `"PARTY_INVITE_REQUEST"`, `"DUEL_REQUESTED"`, `"RESURRECT_REQUEST"`, `"CONFIRM_SUMMON"`, etc.) through BloogBot’s event hook. When a matching event occurs, the service immediately invokes the corresponding handler logic. This design means the bot reacts in real-time to prompts, rather than scanning periodically for UI changes.

Under the hood, BloogBot’s memory hooking system converts WoW UI events into C# events. For example, when an NPC interaction opens a gossip dialog, the game fires `GOSSIP_SHOW` – BloogBot catches this and calls an internal method `OpenDialogFrame()` which creates a list of dialog options and raises an `OnDialogOpened` event. PromptHandlingService uses the same pattern for prompt events: when a `"PARTY_INVITE_REQUEST"` occurs (someone invited the bot to a party), the service’s handler will automatically call the WoW API to decline the invite. When a `"RESURRECT_REQUEST"` happens, the service calls the function to accept the resurrection.

**Automatic Responses via WoW API:** Each prompt handler uses the game’s Lua API (or direct function calls) to perform the appropriate action. BloogBot’s `ObjectManager.Player.LuaCall(...)` method is often used to execute in-game Lua functions from C# code. The PromptHandlingService takes advantage of this to click the “buttons” on prompts. For instance:

* For **NPC Gossip dialogs**: The service (or the bot state logic) will call `SelectGossipOption(index)` to choose a conversation option. Originally, states like *BuyItemsState* or *SellItemsState* did this manually – e.g., selecting the vendor option after talking to a vendor NPC. Now, PromptHandlingService can handle such selection globally for any generic NPC interaction. It finds the first gossip option of the required type and selects it via a Lua call.
* For **Party Invites**: The service executes `DeclineGroup()` (a WoW API call) to politely decline the invite. This prevents the bot from joining a party. (Staying solo is usually desired to avoid human players detecting the bot or interfering.)
* For **Duel Requests**: It calls `CancelDuel()` to decline the duel challenge. This is instant and avoids the bot getting stuck in a duel countdown.
* For **Resurrection Prompts**: It calls `AcceptResurrect()` so that if a nearby healer tries to resurrect the bot, it immediately accepts instead of waiting. The bot will revive on the spot without having to run back from the graveyard.
* For **Summon Requests**: (If implemented) it would call `ConfirmSummon()` (or decline, depending on design) to handle a warlock summoning the bot. By default, the bot might decline unsolicited summons for safety.
* For **Bind Confirmation**: It calls `ConfirmBinder()` to confirm setting the new home location when the bot talks to an innkeeper to bind its hearthstone.
* For **Loot confirmation** (Bind on pickup): If the bot is looting a soulbound item that brings up an “OK/Cancel” confirmation (older WoW versions do this for unique or binding items), the service can call `ConfirmLootSlot()` for that item, ensuring the loot is picked up.

Each of these calls is done only when appropriate – the service checks that the event corresponding to the prompt occurred (and any necessary conditions from the event args). The design avoids spamming these actions; it reacts **only** to real events from the game.

**Control Flow:** When an event arrives, the PromptHandlingService (via either a giant switch or a dispatch table of handlers) routes it to the correct handler code. For example:

```csharp
private static void OnGameEvent(string eventName, object[] args)
{
    switch (eventName)
    {
        case "PARTY_INVITE_REQUEST":
            // A player invited us to a party
            string inviter = (string)args[0];
            ObjectManager.Player.LuaCall("DeclineGroup()");
            Logger.Log($"Declined group invite from {inviter}");
            break;
        case "DUEL_REQUESTED":
            ObjectManager.Player.LuaCall("CancelDuel()");
            Logger.Log("Declined a duel request.");
            break;
        case "RESURRECT_REQUEST":
            ObjectManager.Player.LuaCall("AcceptResurrect()");
            Logger.Log("Accepted a resurrection from a teammate.");
            break;
        // ... other cases ...
    }
}
```

*(The above is a simplified illustration – in the actual code each case may be handled in its own class method or handler object, and there may be additional checks or logging.)*

This control flow ensures each prompt is handled consistently. The use of WoW’s Lua API calls means we rely on Blizzard’s own UI functions to do things like press dialog buttons, which is safer and less error-prone than manipulating game memory directly. For instance, selecting a gossip option uses the same Lua function that a UI click would use, as shown in the DialogFrame logic. Similarly, declining invites or accepting resurrections are done via official API calls.

**Design Decisions:** The PromptHandlingService was designed to be modular and easy to extend:

* *Centralized vs. Distributed:* Previously, logic for handling prompts (like vendor gossip) was scattered in various bot states. For example, the buy/sell states had to check for a gossip dialog and then select the vendor option themselves. This service centralizes all such logic, making the code DRY and easier to update. If Blizzard changes something about how an invite works, you have one place to update the handling.
* *Event-driven:* The service uses event hooks instead of continuously polling for UI changes. This is efficient and immediate. As soon as WoW signals an event, the service reacts. The event-driven approach is evident in BloogBot’s core (e.g., catching `GOSSIP_SHOW` and opening frames), and PromptHandlingService follows that pattern for new events (like invites or duels). This means minimal delay in responding – important for things like declining a duel before the timer expires.
* *Non-intrusive Integration:* The service doesn’t disrupt the normal bot behavior; it runs in parallel. For example, if the bot is fighting or moving, and a prompt comes in, the service handles the prompt in the background without stopping the combat state. It uses thread synchronization to ensure any game API calls happen on the game’s main thread context (BloogBot already uses `ThreadSynchronizer.RunOnMainThread` for game calls to avoid multi-threading issues with game memory).
* *Simple Configuration:* Many of the prompt responses are hard-coded for now (for good reason – e.g., always decline random group invites). This simplicity means fewer configuration knobs for users to mis-set. However, the code is organized such that adding conditions or settings (like “Auto-accept guild invite from specific friend”) would be straightforward to implement in one place.

## Extending and Customizing PromptHandlingService

One of the advantages of isolating prompt logic in this service is how easy it is to extend. **Developers** can add support for new prompt types or customize behavior by modifying or subclassing the handlers here.

**Adding a New Prompt Handler:** Suppose a new type of prompt needs handling (e.g., a **guild invite**). To support it, you would do the following:

1. **Identify the WoW event** that corresponds to the prompt. (For a guild invite, the event is `GUILD_INVITE_REQUEST`, which provides the inviting player and guild name as arguments.)
2. **Create a handler** class (e.g., `GuildInvitePromptHandler`) in the PromptHandlingService directory. Implement the appropriate interface or pattern. For example, if using an `IPromptHandler` interface, implement the `CanHandle` and `HandlePrompt` methods:

   ```csharp
   public class GuildInvitePromptHandler : IPromptHandler
   {
       public string EventName => "GUILD_INVITE_REQUEST";
       public void HandlePrompt(object[] args)
       {
           string invitingPlayer = (string)args[0];
           string guildName = (string)args[1];
           // Decline the guild invite
           ObjectManager.Player.LuaCall("DeclineGuild()");
           Logger.Log($"Declined guild invite from {invitingPlayer} of guild {guildName}.");
       }
   }
   ```
3. **Register the handler** with the PromptHandlingService. If the service uses a list of handlers, you would add an instance of `GuildInvitePromptHandler` to that list during initialization. If it uses a switch statement, add a new case for `"GUILD_INVITE_REQUEST"` calling the new logic.
4. **Test it out** – run the bot and simulate the event (have a character send a guild invite to the bot) to ensure the service catches and handles it.

Because the service funnels all these events in one place, adding a new case or new handler does not require changes scattered all over the codebase. This modular design makes maintenance easier. Contributors should follow the existing patterns for consistency (for instance, similar logging format, using `LuaCall` for interacting with the game, etc.).

**Customization:** If you want to change the default behavior (say, allow party invites from a specific player or accept guild invites under certain conditions), you can modify the logic in the relevant handler. For example, in `GroupInvitePromptHandler`, instead of unconditionally declining, you could add a check against a whitelist of friend names and accept if the inviter is on that list. Pseudocode inside the handler might look like:

```csharp
if (whitelist.Contains(inviter))
    ObjectManager.Player.LuaCall("AcceptGroup()");
else
    ObjectManager.Player.LuaCall("DeclineGroup()");
```

This way, advanced users or developers can tweak how prompts are handled according to their scenario. (Currently such configurations would be done by editing code or possibly via a config file if the service is extended to read settings.)

## Example Scenarios

To illustrate how PromptHandlingService works in practice, here are a few common scenarios:

* **Vendor Interaction (Gossip Prompt):** Your bot needs to buy items from a vendor. The bot’s AI moves to the vendor and interacts. WoW opens a gossip dialog with options (e.g., “Browse goods”, “I’d like to train” etc.). BloogBot raises a `GOSSIP_SHOW` event internally, and the PromptHandlingService (or the state logic) selects the “vendor” option. In earlier code, the BuyItems state did:

  ```csharp
  // In BuyItemsState
  dialogFrame.SelectFirstGossipOfType(player, DialogType.vendor);
  ```

  which calls the WoW API to select the vendor option. With PromptHandlingService, this selection can happen automatically for any state that opens a gossip frame, centralizing the logic of choosing the correct gossip option. The result: the merchant window opens without the bot needing manual input.

* **Party Invitation:** Another player tries to invite your bot to a party. The WoW event `PARTY_INVITE_REQUEST` fires. PromptHandlingService catches this event and immediately calls `DeclineGroup()`, closing the invite popup. The invite is refused within a fraction of a second, and (optionally) a log message notes that the invite was declined. Your bot continues fighting or whatever it was doing, unaffected. The user (inviter) might just see that you declined – which is normal behavior if someone doesn’t want to join, and doesn’t raise suspicion.

* **Duel Challenge:** A nearby player targets the bot and requests a duel. The `DUEL_REQUESTED` event triggers. The service responds by calling `CancelDuel()`, which declines the duel challenge. This happens so fast that the challenging player sees the duel as refused almost instantly. The bot avoids being stuck in a duel countdown or accidentally engaging in PvP. Again, a log entry can be produced for record-keeping (e.g., “Declined duel from PlayerX”).

* **Resurrection Offer:** Your bot died and released spirit, and a friendly priest in the vicinity tries to resurrect it (perhaps a friend or second bot). WoW raises `RESURRECT_REQUEST`. PromptHandlingService handles this by calling `AcceptResurrect()`. The confirmation dialog “PlayerX wants to resurrect you – Accept?” is answered yes automatically. The bot is resurrected on the spot, saving it travel time. The service might also clear the bot’s “dead state” or otherwise notify the AI that it’s alive again (the core Bot logic likely notices the player is no longer a ghost via game state). From the user’s perspective, the bot just instantly takes the resurrect.

* **Summon (Teleport) Prompt:** (If enabled) A warlock summons your bot to a location. When the summon is initiated, WoW shows a dialog “You are being summoned to X location, accept?” with a prompt (`CONFIRM_SUMMON`). Depending on configuration, PromptHandlingService can accept or decline. By default it might decline to prevent random teleportation. If the bot is part of a multi-bot team and you *want* it to accept, you could change that logic. In either case, the prompt will not linger – the service will choose within seconds. For example, it may decline by calling the same function the “No” button would, keeping the bot in its current location.

Each of these examples demonstrates the hands-off automation provided by PromptHandlingService. The bot seamlessly navigates these situations that typically require player clicks or keystrokes. This improves reliability for users running the bot AFK and simplifies development (no need to script these repetitive UI interactions in every bot routine).
