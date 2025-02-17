using System.Text;

namespace PromptHandlingService.Predefined.GMCommands
{
    public class GMCommandConstructionFunction(IPromptRunner promptRunner) : PromptFunctionBase(promptRunner)
    {
        // Static method to handle command execution
        public static async Task<string> GetGMCommand(IPromptRunner promptRunner, GMCommandContext context, CancellationToken cancellationToken)
        {
            var gmCommandFunction = new GMCommandConstructionFunction(promptRunner)
            {
                CommandContext = context
            };
            await gmCommandFunction.CompleteAsync(cancellationToken);
            return gmCommandFunction.ConstructedCommand;
        }

        // Context class to hold command and parameters
        public class GMCommandContext
        {
            public string Command { get; set; } = string.Empty;

            public override string ToString()
            {
                StringBuilder sb = new();
                sb.AppendLine($"Request: {Command}");

                return sb.ToString();
            }
        }

        public GMCommandContext CommandContext
        {
            get => GetParameter<GMCommandContext>();
            set
            {
                SetParameter(value: value);
                ResetChat();
            }
        }

        // Private field to store the constructed command
        private string? _constructedCommand;

        // Public accessor to retrieve the constructed command
        public string ConstructedCommand => _constructedCommand ?? throw new NullReferenceException("The GM Command has not been set. Call 'CompleteAsync' to set the command");

        // System prompt for AI-based functionality
        protected override string SystemPrompt =>   "You are a World of Warcraft 1.12.1 GM command selector. " +
                                                    "You interpret user requests into . " +
                                                    "Replace placeholders with known values. If a value is unknown or unclear, leave it as a placeholder. " +
                                                    "Ensure numeric placeholders do not contain any alphabetical characters. " +
                                                    "All command templates are followed by a tab ('\t') and a brief description. " +
                                                    "Use the provided list of GM commands to generate the appropriate response:```" +
                                                    ".account create $account $password\tCreate account and set password to it.\n" +
                                                    ".account set gmlevel [#accountId|$accountName] #level\tSet the security level for targeted player (can't be used at self) or for #accountId or $accountName to a level of #level.; #level may range from 0 to 3.\n" +
                                                    ".additem #itemid/[#itemname]/#shift-click-item-link #itemcount\tAdds the specified number of items of id #itemid (or exact (!) name $itemname in brackets, or link created by shift-click at item in inventory or recipe) to your or selected character inventory. If #itemcount is omitted, only one item will be added.\n" +
                                                    ".additemset #itemsetid\tAdd items from itemset of id #itemsetid to your or selected character inventory. Will add by one example each item from itemset.\n" +
                                                    ".cast #spellid [triggered]\tCast #spellid to selected target. If no target selected cast to self. If [triggered] or part provided then spell casted with triggered flag.\n" +
                                                    ".cast back #spellid [triggered]\tSelected target will cast #spellid to your character. If [triggered] or part provided then spell casted with triggered flag.\n" +
                                                    ".cast dist #spellid [#dist [triggered]]\tYou will cast spell to point at distance #dist. If [triggered] or part provided then spell casted with triggered flag. Not all spells can be casted as area spells.\n" +
                                                    ".cast self #spellid [triggered]\tCast #spellid by target at target itself. If [triggered] or part provided then spell casted with triggered flag.\n" +
                                                    ".cast target #spellid [triggered]\tSelected target will cast #spellid to his victim. If [triggered] or part provided then spell casted with triggered flag.\n" +
                                                    ".character level [$playername] [#level]\tSet the level of character with $playername (or the selected if not name provided) by #numberoflevels Or +1 if no #numberoflevels provided). If #numberoflevels is omitted, the level will be increase by 1. If #numberoflevels is 0, the same level will be restarted.\n" +
                                                    ".go [$playername|pointlink|#x #y #z [#mapid]]\tTeleport your character to point with coordinates of player $playername, or coordinates of one from shift-link types: player, tele, taxinode, creature/creature_entry, gameobject/gameobject_entry, or explicit #x #y #z #mapid coordinates.\n" +
                                                    ".go xy #x #y [#mapid]\tTeleport player to point with (#x,#y) coordinates at ground(water) level at map #mapid or same map if #mapid not provided.\n" +
                                                    ".go xyz #x #y #z [#mapid]\tTeleport player to point with (#x,#y,#z) coordinates at ground(water) level at map #mapid or same map if #mapid not provided.\n" +
                                                    ".go zonexy #x #y [#zone]\tTeleport player to point with (#x,#y) client coordinates at ground(water) level in zone #zoneid or current zone if #zoneid not provided. You can look up zone using .lookup area $namepart\n" +
                                                    ".groupgo [$charactername]\tTeleport the given character and his group to you. Teleported only online characters but original selected group member can be offline.\n" +
                                                    ".honor add $amount\tAdd a certain amount of honor (gained today) to the selected player.\n" +
                                                    ".honor updatekills\tForce the yesterday's honor kill fields to be updated with today's data, which will get reset for the selected player.\n" +
                                                    ".instance unbind all\tAll of the selected player's binds will be cleared.; .instance unbind #mapid; Only the specified #mapid instance will be cleared.\n" +
                                                    ".learn #spell [all]\tSelected character learn a spell of id #spell. If [all] provided then all ranks learned.\n" +
                                                    ".maxskill\tSets all skills of the targeted player to their maximum VALUES for its current level.\n" +
                                                    ".modify currency $id $amount\tAdd $amount points of currency $id to the selected player.\n" +
                                                    ".modify faction #factionid #flagid #npcflagid #dynamicflagid\tModify the faction and flags of the selected creature. Without arguments, display the faction and flags of the selected creature.\n" +
                                                    ".modify money #money; .money #money\t\tAdd or remove money to the selected player. If no player is selected, modify your money. #gold can be negative to remove money.\n" +
                                                    ".modify rep #repId (#repvalue | $rankname [#delta])\tSets the selected players reputation with faction #repId to #repvalue or to $reprank. If the reputation rank name is provided, the resulting reputation will be the lowest reputation for that rank plus the delta amount, if specified.\n" +
                                                    ".notify $MessageToBroadcast\tSend a global message to all players online in screen.\n" +
                                                    ".npc add #creatureid\tSpawn a creature by the given template id of #creatureid.\n" +
                                                    ".quest add #quest_id\t\tAdd to character quest log quest #quest_id. Quest started from item can't be added by this command but correct .additem call provided in command output.\n" +
                                                    ".quest complete #questid\tMark all quest objectives as completed for target character active quest. After this target character can go and get quest reward.\n" +
                                                    ".repairitems\tRepair all selected player's items.\n" +
                                                    ".reset level [$playername]\tReset level to 1 including reset stats and talents. Equipped items with greater level requirement can be lost.\n" +
                                                    ".reset spells [$playername]\tRemoves all non-original spells from spellbook. $playername can be name of offline character.\n" +
                                                    ".reset stats [$playername]\tResets(recalculate) all stats of the targeted player to their original VALUES at current level.\n" +
                                                    ".reset talents [$playername]\tRemoves all talents (current spec) of the targeted player or pet or named player. With player talents also will be reset talents for all character's pets if any.\n" +
                                                    ".revive\tRevive the selected player. If no player is selected, it will revive you.\n" +
                                                    ".server idlerestart #delay\tRestart the server after #delay seconds if no active connections are present (no players). Use #exit_code or 2 as program exist code.\n" +
                                                    ".server idlerestart cancel\tCancel the restart/shutdown timer if any.\n" +
                                                    ".server idleshutdown #delay [#exit_code]\tShut the server down after #delay seconds if no active connections are present (no players). Use #exit_code or 0 as program exist code.\n" +
                                                    ".server restart #delay\tRestart the server after #delay seconds. Use #exit_code or 2 as program exist code.\n" +
                                                    ".server restart cancel\tCancel the restart/shutdown timer if any.\n" +
                                                    ".server set motd $MOTD\tSet server Message of the day.\n" +
                                                    ".server shutdown #delay [#exit_code]\tShut the server down after #delay seconds. Use #exit_code or 0 as program exit code.\n" +
                                                    ".server shutdown cancel\tCancel the restart/shutdown timer if any.\n" +
                                                    ".setskill #skill #level [#max]\tSet a skill of id #skill with a current skill value of #level and a maximum value of #max (or equal current maximum if not provide) for the selected character. If no character is selected, you learn the skill.\n" +
                                                    ".tele #location\nTeleport player to a given location.\t" +
                                                    ".tele group #location\nTeleport a selected player and his group members to a given location.\t" +
                                                    ".tele name [#playername] #location\tTeleport the given character to a given location. Character can be offline.\n" +
                                                    "```";

        // Complete the command construction process
        public override async Task CompleteAsync(CancellationToken cancellationToken)
        {
            _constructedCommand = await RunChatAsync("Given the following user request, what are the GM Command templates that should be executed?\r\n" +
                                                    $"```{CommandContext.Command}```\r\n\r\n" +
                                                    $"You should only output the chosen commands separated by a newline. No yappin", cancellationToken);


        }

        protected override void InitializeChat() { }
    }
}
