using BotRunner.Clients;
using BotRunner.Constants;
using BotRunner.Interfaces;
using Communication;
using PathfindingService.Client;
using Xas.FluentBehaviourTree;
using Action = BotRunner.Constants.Action;

namespace BotRunner
{
    public class BotRunner
    {
        private readonly List<ActivitySnapshot> _currentActivitySnapshots;
        private readonly IObjectManager _objectManager;
        private readonly IWoWEventHandler _woWEventHandler;
        private readonly BotContext _botContext;
        private readonly ActivityMemberUpdateClient _activityMemberUpdateClient;
        private readonly PathfindingClient _pathfindingClient;

        private readonly Task _asyncBotTaskRunnerTask;
        private readonly Task _asyncServerFeedbackTask;

        private IBehaviourTreeNode _behaviorTree;

        public BotRunner(IObjectManager objectManager,
                         IWoWEventHandler wowEventHandler)
        {
            _objectManager = objectManager;
            _woWEventHandler = wowEventHandler;

            _currentActivitySnapshots = [];

            _botContext = new BotContext(_objectManager, _woWEventHandler);

            _asyncServerFeedbackTask = StartServerFeedbackAsync();
            _asyncBotTaskRunnerTask = StartBotTaskRunnerAsync();
        }
        private async Task StartServerFeedbackAsync()
        {
            while (true)
            {
                try
                {
                    var incomingActivityMemberState = _activityMemberUpdateClient.SendMemberStateUpdate(_currentActivitySnapshots);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[BOT RUNNER] Error in server feedback task: {e.Message}");
                }

                await Task.Delay(100);
            }
        }

        private async Task StartBotTaskRunnerAsync()
        {
            var _status = BehaviourTreeStatus.Success;

            while (true)
            {
                try
                {
                    if (_behaviorTree == null || _status != BehaviourTreeStatus.Running)
                    {
                        // Call the decision engine to get the next set of actions with parameters
                        var actionMap = await _botContext.GetNextActionsWithParamsAsync();

                        // Dynamically rebuild the behavior tree based on the action map
                        _behaviorTree = BuildBehaviorTreeFromActions(actionMap);

                    }
                    // Tick the behavior tree to execute the current task
                    _behaviorTree.Tick(new TimeData(0.1f));

                    // Delay to control the frequency of task processing
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BOT RUNNER] {ex}");
                }
            }
        }
        private IBehaviourTreeNode BuildBehaviorTreeFromActions(List<(Action, List<object>)> actionMap)
        {
            var builder = new BehaviourTreeBuilder();

            // Iterate over the action map and build sequences for each action with its parameters
            foreach (var actionEntry in actionMap)
            {
                switch (actionEntry.Item1)
                {
                    case Action.Wait:
                        builder.Splice(BuildWaitSequence((float)actionEntry.Item2[0]));
                        break;
                    case Action.GoTo:
                        builder.Splice(BuildGoToSequence((float)actionEntry.Item2[0], (float)actionEntry.Item2[1], (float)actionEntry.Item2[2], (float)actionEntry.Item2[3]));
                        break;
                    case Action.InteractWith:
                        builder.Splice(BuildInteractWithSequence((ulong)actionEntry.Item2[0]));
                        break;

                    case Action.SelectGossip:
                        builder.Splice(BuildSelectGossipSequence((int)actionEntry.Item2[0]));
                        break;

                    case Action.SelectTaxiNode:
                        builder.Splice(BuildSelectTaxiNodeSequence((int)actionEntry.Item2[0]));
                        break;

                    case Action.AcceptQuest:
                        builder.Splice(AcceptQuestSequence);
                        break;
                    case Action.DeclineQuest:
                        builder.Splice(DeclineQuestSequence);
                        break;
                    case Action.SelectReward:
                        builder.Splice(BuildSelectRewardSequence((int)actionEntry.Item2[0]));
                        break;
                    case Action.CompleteQuest:
                        builder.Splice(CompleteQuestSequence);
                        break;

                    case Action.TrainSkill:
                        builder.Splice(BuildTrainSkillSequence((int)actionEntry.Item2[0]));
                        break;
                    case Action.TrainTalent:
                        builder.Splice(BuildLearnTalentSequence((int)actionEntry.Item2[0]));
                        break;

                    case Action.OfferTrade:
                        builder.Splice(BuildOfferTradeSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case Action.OfferGold:
                        builder.Splice(BuildOfferMoneySequence((int)actionEntry.Item2[0]));
                        break;
                    case Action.OfferItem:
                        builder.Splice(BuildOfferItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1], (int)actionEntry.Item2[2], (int)actionEntry.Item2[3]));
                        break;
                    case Action.AcceptTrade:
                        builder.Splice(AcceptTradeSequence);
                        break;
                    case Action.DeclineTrade:
                        builder.Splice(DeclineTradeSequence);
                        break;
                    case Action.EnchantTrade:
                        builder.Splice(BuildOfferEnchantSequence((int)actionEntry.Item2[0]));
                        break;
                    case Action.LockpickTrade:
                        builder.Splice(OfferLockpickSequence);
                        break;

                    case Action.PromoteLeader:
                        builder.Splice(BuildPromoteLeaderSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case Action.PromoteAssistant:
                        builder.Splice(BuildPromoteAssistantSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case Action.PromoteLootManager:
                        builder.Splice(BuildPromoteLootManagerSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case Action.SetGroupLoot:
                        builder.Splice(BuildSetGroupLootSequence((GroupLootSetting)actionEntry.Item2[0]));
                        break;
                    case Action.AssignLoot:
                        builder.Splice(BuildAssignLootSequence((int)actionEntry.Item2[0], (ulong)actionEntry.Item2[1]));
                        break;

                    case Action.LootRollNeed:
                        builder.Splice(BuildLootRollNeedSequence((int)actionEntry.Item2[0]));
                        break;
                    case Action.LootRollGreed:
                        builder.Splice(BuildLootRollGreedSequence((int)actionEntry.Item2[0]));
                        break;
                    case Action.LootPass:
                        builder.Splice(BuildLootPassSequence((int)actionEntry.Item2[0]));
                        break;

                    case Action.SendGroupInvite:
                        builder.Splice(BuildSendGroupInviteSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case Action.AcceptGroupInvite:
                        builder.Splice(AcceptGroupInviteSequence);
                        break;
                    case Action.DeclineGroupInvite:
                        builder.Splice(DeclineGroupInviteSequence);
                        break;
                    case Action.KickPlayer:
                        builder.Splice(BuildKickPlayerSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case Action.LeaveGroup:
                        builder.Splice(LeaveGroupSequence);
                        break;
                    case Action.DisbandGroup:
                        builder.Splice(DisbandGroupSequence);
                        break;
                    case Action.StopAttack:
                        builder.Splice(StopAttackSequence);
                        break;
                    case Action.CastSpell:
                        builder.Splice(BuildCastSpellSequence((int)actionEntry.Item2[0], (ulong)actionEntry.Item2[1]));
                        break;
                    case Action.StopCast:
                        builder.Splice(StopCastSequence);
                        break;

                    case Action.UseItem:
                        builder.Splice(BuildUseItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1], (ulong)actionEntry.Item2[2]));
                        break;
                    case Action.EquipItem:
                        builder.Splice(BuildEquipItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1], (EquipSlot)actionEntry.Item2[2]));
                        break;
                    case Action.UnequipItem:
                        builder.Splice(BuildUnequipItemSequence((EquipSlot)actionEntry.Item2[0]));
                        break;
                    case Action.DestroyItem:
                        builder.Splice(BuildDestroyItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1], (int)actionEntry.Item2[2]));
                        break;
                    case Action.MoveItem:
                        builder.Splice(BuildMoveItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1], (int)actionEntry.Item2[2], (int)actionEntry.Item2[3], (int)actionEntry.Item2[4]));
                        break;
                    case Action.SplitStack:
                        builder.Splice(BuildSplitStackSequence((int)actionEntry.Item2[0], 
                            (int)actionEntry.Item2[1], 
                            (int)actionEntry.Item2[2], 
                            (int)actionEntry.Item2[3], 
                            (int)actionEntry.Item2[4]));
                        break;

                    case Action.BuyItem:
                        builder.Splice(BuildBuyItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1]));
                        break;
                    case Action.BuybackItem:
                        builder.Splice(BuildBuybackItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1]));
                        break;
                    case Action.SellItem:
                        builder.Splice(BuildSellItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1], (int)actionEntry.Item2[2]));
                        break;
                    case Action.RepairItem:
                        builder.Splice(BuildRepairItemSequence((int)actionEntry.Item2[0]));
                        break;
                    case Action.RepairAllItems:
                        builder.Splice(RepairAllItemsSequence);
                        break;

                    case Action.DismissBuff:
                        builder.Splice(BuildDismissBuffSequence((string)actionEntry.Item2[0]));
                        break;

                    case Action.Resurrect:
                        builder.Splice(ResurrectSequence);
                        break;

                    case Action.Craft:
                        builder.Splice(BuildCraftSequence((int)actionEntry.Item2[0]));
                        break;

                    case Action.Login:
                        builder.Splice(BuildLoginSequence((string)actionEntry.Item2[0], (string)actionEntry.Item2[1]));
                        break;
                    case Action.Logout:
                        builder.Splice(LogoutSequence);
                        break;
                    case Action.CreateCharacter:
                        builder.Splice(BuildCreateCharacterSequence(actionEntry.Item2));
                        break;
                    case Action.DeleteCharacter:
                        builder.Splice(BuildDeleteCharacterSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case Action.EnterWorld:
                        builder.Splice(BuildEnterWorldSequence((ulong)actionEntry.Item2[0]));
                        break;

                    default:
                        break;
                }
            }

            return builder.Build();
        }
        private IBehaviourTreeNode BuildWaitSequence(float duration) => new BehaviourTreeBuilder()
                .Sequence("Wait Sequence")
                    .Do("Wait", time => BehaviourTreeStatus.Success)
                .End()
                .Build();
        /// <summary>
        /// Sequence to move the bot to a specific location using given coordinates (x, y, z) and a range (f).
        /// </summary>
        /// <param name="x">The x-coordinate of the destination.</param>
        /// <param name="y">The y-coordinate of the destination.</param>
        /// <param name="z">The z-coordinate of the destination.</param>
        /// <param name="f">The allowed range/facing tolerance for reaching the destination.</param>
        /// <returns>IBehaviourTreeNode that manages moving the bot to the specified location.</returns>
        private IBehaviourTreeNode BuildGoToSequence(float x, float y, float z, float f) => new BehaviourTreeBuilder()
            .Sequence("GoTo Sequence")
                // Move the bot to the location
                .Do("Move to Location", time => {
                    if (_botContext.ObjectManager.Player.Facing >= f - 0.1f && _botContext.ObjectManager.Player.Facing <= f + 0.1f)
                    {
                        if (_botContext.ObjectManager.Player.Position.DistanceTo(new PathfindingService.Models.Position(x, y, z)) < 1)
                            return BehaviourTreeStatus.Success;
                        else
                            _botContext.ObjectManager.Player.MoveToward(new PathfindingService.Models.Position(x, y, z), f);
                    }
                    else
                        _botContext.ObjectManager.Player.SetFacing(f);

                    return BehaviourTreeStatus.Running;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to interact with a specific target based on its GUID.
        /// </summary>
        /// <param name="guid">The GUID of the target to interact with.</param>
        /// <returns>IBehaviourTreeNode that manages interacting with the specified target.</returns>
        private IBehaviourTreeNode BuildInteractWithSequence(ulong guid) => new BehaviourTreeBuilder()
            .Sequence("Interact With Sequence")
                .Splice(CheckForTarget(guid))
                // Ensure the target is valid for interaction
                .Condition("Has Valid Target", time => _botContext.ObjectManager.Player.TargetGuid == guid)

                // Perform the interaction
                .Do("Interact with Target", time =>
                {
                    _botContext.ObjectManager.GameObjects.First(x => x.Guid == guid).Interact();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Property to check if the player has a target, and if not, sets the target to the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID of the target to set.</param>
        /// <returns>IBehaviourTreeNode that checks for and sets a target if needed.</returns>
        private IBehaviourTreeNode CheckForTarget(ulong guid) => new BehaviourTreeBuilder()
            .Sequence("Check for Target")
                // Check if the player already has a target
                .Condition("Has Target", time => _botContext.ObjectManager.Player != null
                                                 && _botContext.ObjectManager.Player.TargetGuid != 0)
                // If no target, set the target to the provided GUID
                .Do("Set Target", time =>
                {
                    if (_botContext.ObjectManager.Player.TargetGuid == 0)
                    {
                        _botContext.ObjectManager.Player.SetTarget(guid);
                    }
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to select a gossip option from an NPC's menu.
        /// </summary>
        /// <param name="selection">The index of the gossip option to select.</param>
        /// <returns>IBehaviourTreeNode that manages selecting a gossip option.</returns>
        private IBehaviourTreeNode BuildSelectGossipSequence(int selection) => new BehaviourTreeBuilder()
            .Sequence("Select Gossip Sequence")
                // Ensure the bot has a valid target with gossip options
                .Condition("Has Valid Gossip Target", time => _botContext.ObjectManager.GossipFrame.IsOpen 
                                                            && _botContext.ObjectManager.GossipFrame.Options.Count > 0)

                // Select the gossip option
                .Do("Select Gossip Option", time =>
                {
                    _botContext.ObjectManager.GossipFrame.SelectGossipOption(selection);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to select a taxi node (flight path) for fast travel.
        /// </summary>
        /// <param name="nodeId">The ID of the taxi node to select.</param>
        /// <returns>IBehaviourTreeNode that manages selecting the taxi node.</returns>
        private IBehaviourTreeNode BuildSelectTaxiNodeSequence(int nodeId) => new BehaviourTreeBuilder()
            .Sequence("Select Taxi Node Sequence")
                // Ensure the bot has access to the selected taxi node
                .Condition("Has Taxi Node Unlocked", time => _botContext.ObjectManager.TaxiFrame.HasNodeUnlocked(nodeId))

                // Ensure the bot has enough gold for the flight
                .Condition("Has Enough Gold", time => _botContext.ObjectManager.Player.Copper > _botContext.ObjectManager.TaxiFrame.Nodes[nodeId].Cost)

                // Select the taxi node
                .Do("Select Taxi Node", time =>
                {
                    _botContext.ObjectManager.TaxiFrame.SelectNode(nodeId);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to accept a quest from an NPC. This checks if the quest is available and the bot meets the prerequisites.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages accepting the quest.</returns>
        private IBehaviourTreeNode AcceptQuestSequence => new BehaviourTreeBuilder()
            .Sequence("Accept Quest Sequence")
                // Ensure the bot can accept the quest (e.g., meets level requirements)
                .Condition("Can Accept Quest", time => _botContext.ObjectManager.QuestFrame.IsOpen)

                // Accept the quest from the NPC
                .Do("Accept Quest", time =>
                {
                    _botContext.ObjectManager.QuestFrame.AcceptQuest();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to decline a quest offered by an NPC.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages declining the quest.</returns>
        private IBehaviourTreeNode DeclineQuestSequence => new BehaviourTreeBuilder()
            .Sequence("Decline Quest Sequence")
                // Ensure the bot can decline the quest
                .Condition("Can Decline Quest", time => _botContext.ObjectManager.QuestFrame.IsOpen)

                // Decline the quest
                .Do("Decline Quest", time =>
                {
                    _botContext.ObjectManager.QuestFrame.DeclineQuest();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to select a reward from a completed quest.
        /// </summary>
        /// <param name="rewardIndex">The index of the reward to select.</param>
        /// <returns>IBehaviourTreeNode that manages selecting the quest reward.</returns>
        private IBehaviourTreeNode BuildSelectRewardSequence(int rewardIndex) => new BehaviourTreeBuilder()
            .Sequence("Select Reward Sequence")
                // Ensure the bot is able to select a reward
                .Condition("Can Select Reward", time => _botContext.ObjectManager.QuestFrame.IsOpen)

                // Select the specified reward
                .Do("Select Reward", time =>
                {
                    _botContext.ObjectManager.QuestFrame.CompleteQuest(rewardIndex);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to complete a quest and turn it in to an NPC.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages completing the quest.</returns>
        private IBehaviourTreeNode CompleteQuestSequence => new BehaviourTreeBuilder()
            .Sequence("Complete Quest Sequence")
                // Ensure the bot can complete the quest
                .Condition("Can Complete Quest", time => _botContext.ObjectManager.QuestFrame.IsOpen)

                // Complete the quest
                .Do("Complete Quest", time =>
                {
                    _botContext.ObjectManager.QuestFrame.CompleteQuest();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to train a specific skill from a trainer NPC.
        /// </summary>
        /// <param name="spellIndex">The index of the skill or spell to train.</param>
        /// <returns>IBehaviourTreeNode that manages training the skill.</returns>
        private IBehaviourTreeNode BuildTrainSkillSequence(int spellIndex) => new BehaviourTreeBuilder()
            .Sequence("Train Skill Sequence")
                // Ensure the bot is at a trainer NPC
                .Condition("Is At Trainer", time => _botContext.ObjectManager.TrainerFrame.IsOpen)

                // Ensure the bot has enough gold to train the skill
                .Condition("Has Enough Gold", time => _botContext.ObjectManager.Player.Copper > _botContext.ObjectManager.TrainerFrame.Spells.ElementAt(spellIndex).Cost)

                // Train the skill
                .Do("Train Skill", time =>
                {
                    _botContext.ObjectManager.TrainerFrame.TrainSpell(spellIndex);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to train a specific talent. This checks if the bot has enough resources and is eligible to train the talent.
        /// </summary>
        /// <param name="talentSpellId">The ID of the talent spell to train.</param>
        /// <returns>IBehaviourTreeNode that manages training the talent.</returns>
        private IBehaviourTreeNode BuildLearnTalentSequence(int talentSpellId) => new BehaviourTreeBuilder()
            .Sequence("Train Talent Sequence")
                // Ensure the bot is eligible to train the talent
                .Condition("Can Train Talent", time => _botContext.ObjectManager.TalentFrame.TalentPointsAvailable > 1)

                // Train the talent
                .Do("Train Talent", time =>
                {
                    _botContext.ObjectManager.TalentFrame.LearnTalent(talentSpellId);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        private IBehaviourTreeNode BuildBuyItemSequence(int slotId, int quantity) => new BehaviourTreeBuilder()
                .Sequence("BuyItem Sequence")
                    .Do("Buy Item", time =>
                    {
                        _botContext.ObjectManager.MerchantFrame.BuyItem(slotId, quantity);
                        return BehaviourTreeStatus.Success;
                    })
                .End()
                .Build();
        private IBehaviourTreeNode BuildBuybackItemSequence(int slotId, int quantity) => new BehaviourTreeBuilder()
                .Sequence("BuybackItem Sequence")
                    .Do("Buy Item", time =>
                    {
                        _botContext.ObjectManager.MerchantFrame.BuybackItem(slotId, quantity);
                        return BehaviourTreeStatus.Success;
                    })
                .End()
                .Build();
        private IBehaviourTreeNode BuildSellItemSequence(int bagId, int slotId, int quantity) => new BehaviourTreeBuilder()
                .Sequence("SellItem Sequence")
                    .Do("Sell Item", time =>
                    {
                        _botContext.ObjectManager.MerchantFrame.SellItem(bagId, slotId, quantity);
                        return BehaviourTreeStatus.Success;
                    })
                .End()
                .Build();
        /// <summary>
        /// Sequence to stop any active auto-attacks, including melee, ranged, and wand.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages stopping auto-attacks.</returns>
        private IBehaviourTreeNode StopAttackSequence => new BehaviourTreeBuilder()
            .Sequence("Stop Attack Sequence")
                // Check if any auto-attack (melee, ranged, or wand) is active
                .Condition("Is Any Auto-Attack Active", time => _botContext.ObjectManager.Player.IsAutoAttacking)

                // Disable all auto-attacks
                .Do("Stop All Auto-Attacks", time =>
                {
                    _botContext.ObjectManager.Player.StopAttack();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to cast a specific spell. This checks if the bot has sufficient resources,
        /// if the spell is off cooldown, and if the target is in range before casting the spell.
        /// </summary>
        /// <param name="spellId">The ID of the spell to cast.</param>
        /// <returns>IBehaviourTreeNode that manages casting a spell.</returns>
        private IBehaviourTreeNode BuildCastSpellSequence(int spellId, ulong targetGuid) => new BehaviourTreeBuilder()
            .Sequence("Cast Spell Sequence")
                // Ensure the bot has a valid target
                .Splice(CheckForTarget(targetGuid))

                // Ensure the bot has enough resources to cast the spell
                .Condition("Can Cast Spell", time => _botContext.ObjectManager.Player.CanCastSpell(spellId, targetGuid))

                // Cast the spell
                .Do("Cast Spell", time =>
                {
                    _botContext.ObjectManager.Player.CastSpell(spellId);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to stop the current spell cast. This will stop any spell the bot is currently casting.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages stopping a spell cast.</returns>
        private IBehaviourTreeNode StopCastSequence => new BehaviourTreeBuilder()
            .Sequence("Stop Cast Sequence")
                // Ensure the bot is currently casting a spell
                .Condition("Is Casting", time => _botContext.ObjectManager.Player.IsCasting || _botContext.ObjectManager.Player.IsChanneling)

                // Stop the current spell cast
                .Do("Stop Spell Cast", time =>
                {
                    _botContext.ObjectManager.Player.StopCasting();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to resurrect the bot or another target.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages the resurrection process.</returns>
        private IBehaviourTreeNode ResurrectSequence => new BehaviourTreeBuilder()
            .Sequence("Resurrect Sequence")
                // Ensure the bot or target can be resurrected
                .Condition("Can Resurrect", time => _botContext.ObjectManager.Player.InGhostForm && _botContext.ObjectManager.Player.CanResurrect)

                // Perform the resurrection action
                .Do("Resurrect", time =>
                {
                    _botContext.ObjectManager.Player.AcceptResurrect();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to offer a trade to another player or NPC.
        /// </summary>
        /// <param name="targetGuid">The GUID of the target with whom to trade.</param>
        /// <returns>IBehaviourTreeNode that manages offering a trade.</returns>
        private IBehaviourTreeNode BuildOfferTradeSequence(ulong targetGuid) => new BehaviourTreeBuilder()
            .Sequence("Offer Trade Sequence")
                // Ensure the bot has a valid trade target
                .Condition("Has Valid Trade Target", time => _botContext.ObjectManager.Player.Position.DistanceTo(_botContext.ObjectManager.Players.First(x => x.Guid == targetGuid).Position) < 5.33f)

                // Offer trade to the target
                .Do("Offer Trade", time =>
                {
                    _botContext.ObjectManager.Players.First(x => x.Guid == targetGuid).OfferTrade();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to offer money in a trade to another player or NPC.
        /// </summary>
        /// <param name="copperCount">The amount of money (in copper) to offer in the trade.</param>
        /// <returns>IBehaviourTreeNode that manages offering money in the trade.</returns>
        private IBehaviourTreeNode BuildOfferMoneySequence(int copperCount) => new BehaviourTreeBuilder()
            .Sequence("Offer Money Sequence")
                // Ensure the bot has a valid trade window open
                .Condition("Trade Window Valid", time => _botContext.ObjectManager.TradeFrame.IsOpen)
                
                // Ensure the bot has enough money to offer
                .Condition("Has Enough Money", time => _botContext.ObjectManager.Player.Copper > copperCount)

                // Offer money in the trade
                .Do("Offer Money", time =>
                {
                    _botContext.ObjectManager.TradeFrame.OfferMoney(copperCount);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to offer an item in a trade to another player or NPC.
        /// </summary>
        /// <param name="bagId">The bag ID where the item is stored.</param>
        /// <param name="slotId">The slot ID where the item is located.</param>
        /// <param name="quantity">The quantity of the item to offer.</param>
        /// <param name="tradeWindowSlot">The slot in the trade window to place the item.</param>
        /// <returns>IBehaviourTreeNode that manages offering the item in the trade.</returns>
        private IBehaviourTreeNode BuildOfferItemSequence(int bagId, int slotId, int quantity, int tradeWindowSlot) => new BehaviourTreeBuilder()
            .Sequence("Offer Item Sequence")
                // Ensure the bot has a valid trade window open
                .Condition("Trade Window Valid", time => _botContext.ObjectManager.TradeFrame.IsOpen)

                // Ensure the bot has the item and quantity to offer
                .Condition("Has Item to Offer", time => _botContext.ObjectManager.Player.GetContainedItem(bagId, slotId).Quantity >= quantity)

                // Offer the item in the trade window
                .Do("Offer Item", time =>
                {
                    _botContext.ObjectManager.TradeFrame.OfferItem(bagId, slotId, quantity, tradeWindowSlot);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to accept a trade with another player or NPC.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages accepting the trade.</returns>
        private IBehaviourTreeNode AcceptTradeSequence => new BehaviourTreeBuilder()
            .Sequence("Accept Trade Sequence")
                // Ensure the bot has a valid trade window open
                .Condition("Trade Window Valid", time => _botContext.ObjectManager.TradeFrame.IsOpen)

                // Accept the trade
                .Do("Accept Trade", time =>
                {
                    _botContext.ObjectManager.TradeFrame.AcceptTrade();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to decline a trade with another player or NPC.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages declining the trade.</returns>
        private IBehaviourTreeNode DeclineTradeSequence => new BehaviourTreeBuilder()
            .Sequence("Decline Trade Sequence")
                // Ensure the trade window is valid
                .Condition("Trade Window Valid", time => _botContext.ObjectManager.TradeFrame.IsOpen)

                // Decline the trade
                .Do("Decline Trade", time =>
                {
                    _botContext.ObjectManager.TradeFrame.DeclineTrade();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to offer an enchantment in a trade to another player or NPC.
        /// </summary>
        /// <param name="enchantId">The ID of the enchantment to offer.</param>
        /// <returns>IBehaviourTreeNode that manages offering the enchantment in the trade.</returns>
        private IBehaviourTreeNode BuildOfferEnchantSequence(int enchantId) => new BehaviourTreeBuilder()
            .Sequence("Offer Enchant Sequence")
                // Ensure the trade window is valid
                .Condition("Trade Window Valid", time => _botContext.ObjectManager.TradeFrame.IsOpen)

                //// Ensure the bot has the correct enchantment to offer
                //.Condition("Has Enchant Available", time => _botContext.ObjectManager.HasEnchantAvailable(enchantId))

                // Offer the enchantment in the trade
                .Do("Offer Enchant", time =>
                {
                    _botContext.ObjectManager.TradeFrame.OfferEnchant(enchantId);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to offer a lockpicking service in a trade.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages offering lockpicking in a trade.</returns>
        private IBehaviourTreeNode OfferLockpickSequence => new BehaviourTreeBuilder()
            .Sequence("Lockpick Trade Sequence")
                // Ensure the bot has the ability to lockpick
                .Condition("Can Lockpick", time => _botContext.ObjectManager.Player.Class == Class.Rogue)

                // Offer lockpicking in the trade
                .Do("Offer Lockpick", time =>
                {
                    _botContext.ObjectManager.TradeFrame.OfferLockpick();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to promote another player to group leader.
        /// </summary>
        /// <param name="playerGuid">The GUID of the player to promote to leader.</param>
        /// <returns>IBehaviourTreeNode that manages promoting the player to group leader.</returns>
        private IBehaviourTreeNode BuildPromoteLeaderSequence(ulong playerGuid) => new BehaviourTreeBuilder()
            .Sequence("Promote Leader Sequence")
                // Ensure the bot is in a group with the specified player
                .Condition("Is In Group with Player", time => _botContext.ObjectManager.PartyMembers.Any(x => x.Guid == playerGuid))

                // Promote the player to group leader
                .Do("Promote Leader", time =>
                {
                    _botContext.ObjectManager.PromoteLeader(playerGuid);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to promote another player to group assistant.
        /// </summary>
        /// <param name="playerGuid">The GUID of the player to promote to assistant.</param>
        /// <returns>IBehaviourTreeNode that manages promoting the player to group assistant.</returns>
        private IBehaviourTreeNode BuildPromoteAssistantSequence(ulong playerGuid) => new BehaviourTreeBuilder()
            .Sequence("Promote Assistant Sequence")
                // Ensure the bot is in a group with the specified player
                .Condition("Is In Group with Player", time => _botContext.ObjectManager.PartyMembers.Any(x => x.Guid == playerGuid))

                // Promote the player to group assistant
                .Do("Promote Assistant", time =>
                {
                    _botContext.ObjectManager.PromoteAssistant(playerGuid);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to promote another player to loot manager in the group.
        /// </summary>
        /// <param name="playerGuid">The GUID of the player to promote to loot manager.</param>
        /// <returns>IBehaviourTreeNode that manages promoting the player to loot manager.</returns>
        private IBehaviourTreeNode BuildPromoteLootManagerSequence(ulong playerGuid) => new BehaviourTreeBuilder()
            .Sequence("Promote Loot Manager Sequence")
                // Ensure the bot is in a group with the specified player
                .Condition("Is In Group with Player", time => _botContext.ObjectManager.PartyMembers.Any(x => x.Guid == playerGuid))

                // Promote the player to loot manager
                .Do("Promote Loot Manager", time =>
                {
                    _botContext.ObjectManager.PromoteLootManager(playerGuid);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to set group loot rules for distributing loot in a group.
        /// </summary>
        /// <param name="setting">The group loot setting to apply (e.g., free-for-all, round-robin).</param>
        /// <returns>IBehaviourTreeNode that manages setting the group loot rules.</returns>
        private IBehaviourTreeNode BuildSetGroupLootSequence(GroupLootSetting setting) => new BehaviourTreeBuilder()
            .Sequence("Set Group Loot Sequence")
                // Ensure the bot is in a group and has permission to change loot rules
                .Condition("Can Set Loot Rules", time => _botContext.ObjectManager.PartyLeaderGuid == _botContext.ObjectManager.Player.Guid)

                // Set the group loot rule
                .Do("Set Group Loot", time =>
                {
                    _botContext.ObjectManager.SetGroupLoot(setting);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to assign specific loot to a player in the group.
        /// </summary>
        /// <param name="itemId">The ID of the loot item to assign.</param>
        /// <param name="playerGuid">The GUID of the player to assign the loot to.</param>
        /// <returns>IBehaviourTreeNode that manages assigning the loot.</returns>
        private IBehaviourTreeNode BuildAssignLootSequence(int itemId, ulong playerGuid) => new BehaviourTreeBuilder()
            .Sequence("Assign Loot Sequence")
                // Ensure the bot has permission to assign loot
                .Condition("Can Assign Loot", time => _botContext.ObjectManager.PartyMembers.Any(x => x.Guid == playerGuid))

                // Assign the loot to the specified player
                .Do("Assign Loot", time =>
                {
                    _botContext.ObjectManager.AssignLoot(itemId, playerGuid);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to roll "Need" on a specific loot item during group loot distribution.
        /// </summary>
        /// <param name="itemId">The ID of the item to roll "Need" on.</param>
        /// <returns>IBehaviourTreeNode that manages rolling "Need" for the item.</returns>
        private IBehaviourTreeNode BuildLootRollNeedSequence(int itemId) => new BehaviourTreeBuilder()
            .Sequence("Loot Roll Need Sequence")
                // Ensure the bot can roll "Need" on the item
                .Condition("Can Roll Need", time => _botContext.ObjectManager.HasLootRollWindow(itemId))

                // Roll "Need" for the item
                .Do("Roll Need", time =>
                {
                    _botContext.ObjectManager.LootRollNeed(itemId);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to roll "Greed" on a specific loot item during group loot distribution.
        /// </summary>
        /// <param name="itemId">The ID of the item to roll "Greed" on.</param>
        /// <returns>IBehaviourTreeNode that manages rolling "Greed" for the item.</returns>
        private IBehaviourTreeNode BuildLootRollGreedSequence(int itemId) => new BehaviourTreeBuilder()
            .Sequence("Loot Roll Greed Sequence")
                // Ensure the bot can roll "Greed" on the item
                .Condition("Can Roll Greed", time => _botContext.ObjectManager.HasLootRollWindow(itemId))

                // Roll "Greed" for the item
                .Do("Roll Greed", time =>
                {
                    _botContext.ObjectManager.LootRollGreed(itemId);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to pass on a specific loot item during group loot distribution.
        /// </summary>
        /// <param name="itemId">The ID of the item to pass on.</param>
        /// <returns>IBehaviourTreeNode that manages passing on the item.</returns>
        private IBehaviourTreeNode BuildLootPassSequence(int itemId) => new BehaviourTreeBuilder()
            .Sequence("Loot Pass Sequence")
                // Ensure the bot can pass on the item
                .Condition("Can Pass Loot", time => _botContext.ObjectManager.HasLootRollWindow(itemId))

                // Pass on the loot item
                .Do("Pass Loot", time =>
                {
                    _botContext.ObjectManager.LootPass(itemId);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to send a group invite to another player.
        /// </summary>
        /// <param name="playerGuid">The GUID of the player to invite to the group.</param>
        /// <returns>IBehaviourTreeNode that manages sending the group invite.</returns>
        private IBehaviourTreeNode BuildSendGroupInviteSequence(ulong playerGuid) => new BehaviourTreeBuilder()
            .Sequence("Send Group Invite Sequence")
                // Ensure the player is not already in a group and can be invited
                .Condition("Can Send Group Invite", time => !_botContext.ObjectManager.PartyMembers.Any(x => x.Guid == playerGuid))

                // Send the group invite
                .Do("Send Group Invite", time =>
                {
                    _botContext.ObjectManager.InviteToGroup(playerGuid);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to accept a group invite from another player.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages accepting the group invite.</returns>
        private IBehaviourTreeNode AcceptGroupInviteSequence => new BehaviourTreeBuilder()
            .Sequence("Accept Group Invite Sequence")
                // Ensure the bot has a pending invite to accept
                .Condition("Has Pending Invite", time => _botContext.ObjectManager.HasPendingGroupInvite())

                // Accept the group invite
                .Do("Accept Group Invite", time =>
                {
                    _botContext.ObjectManager.AcceptGroupInvite();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to decline a group invite from another player.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages declining the group invite.</returns>
        private IBehaviourTreeNode DeclineGroupInviteSequence => new BehaviourTreeBuilder()
            .Sequence("Decline Group Invite Sequence")
                // Ensure the bot has a pending invite to decline
                .Condition("Has Pending Invite", time => _botContext.ObjectManager.HasPendingGroupInvite())

                // Decline the group invite
                .Do("Decline Group Invite", time =>
                {
                    _botContext.ObjectManager.DeclineGroupInvite();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to kick a player from the group.
        /// </summary>
        /// <param name="playerGuid">The GUID of the player to kick from the group.</param>
        /// <returns>IBehaviourTreeNode that manages kicking the player from the group.</returns>
        private IBehaviourTreeNode BuildKickPlayerSequence(ulong playerGuid) => new BehaviourTreeBuilder()
            .Sequence("Kick Player Sequence")
                // Ensure the bot has permission to kick players and the target is valid
                .Condition("Can Kick Player", time => _botContext.ObjectManager.Player.Guid == _botContext.ObjectManager.PartyLeaderGuid)

                // Kick the player from the group
                .Do("Kick Player", time =>
                {
                    _botContext.ObjectManager.KickPlayer(playerGuid);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to leave the current group.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages leaving the group.</returns>
        private IBehaviourTreeNode LeaveGroupSequence => new BehaviourTreeBuilder()
            .Sequence("Leave Group Sequence")
                // Ensure the bot is in a group
                .Condition("Is In Group", time => _botContext.ObjectManager.PartyLeaderGuid != 0)

                // Leave the group
                .Do("Leave Group", time =>
                {
                    _botContext.ObjectManager.LeaveGroup();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to disband the current group the bot is leading.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages disbanding the group.</returns>
        private IBehaviourTreeNode DisbandGroupSequence => new BehaviourTreeBuilder()
            .Sequence("Disband Group Sequence")
                // Ensure the bot is the leader of the group
                .Condition("Is Group Leader", time => _botContext.ObjectManager.Player.Guid == _botContext.ObjectManager.PartyLeaderGuid)

                // Disband the group
                .Do("Disband Group", time =>
                {
                    _botContext.ObjectManager.DisbandGroup();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to use an item, either on the bot or a target.
        /// </summary>
        /// <param name="fromBag">The bag the item is in.</param>
        /// <param name="fromSlot">The slot the item is in.</param>
        /// <param name="targetGuid">The GUID of the target on which to use the item (optional).</param>
        /// <returns>IBehaviourTreeNode that manages using the item.</returns>
        private IBehaviourTreeNode BuildUseItemSequence(int fromBag, int fromSlot, ulong targetGuid) => new BehaviourTreeBuilder()
            .Sequence("Use Item Sequence")
                // Ensure the bot has the item available to use
                .Condition("Has Item", time => _botContext.ObjectManager.Player.GetContainedItem(fromBag, fromSlot) != null)

                // Use the item on the target (or self if target is null)
                .Do("Use Item", time =>
                {
                    _botContext.ObjectManager.Player.UseItem(fromBag, fromSlot, targetGuid);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to move an item from one bag and slot to another bag and slot.
        /// </summary>
        /// <param name="fromBag">The source bag ID.</param>
        /// <param name="fromSlot">The source slot ID.</param>
        /// <param name="toBag">The destination bag ID.</param>
        /// <param name="toSlot">The destination slot ID.</param>
        /// <returns>IBehaviourTreeNode that manages moving the item.</returns>
        private IBehaviourTreeNode BuildMoveItemSequence(int fromBag, int fromSlot, int quantity, int toBag, int toSlot) => new BehaviourTreeBuilder()
            .Sequence("Move Item Sequence")
                // Ensure the bot has the item in the source slot
                .Condition("Has Item to Move", time => _botContext.ObjectManager.Player.GetContainedItem(fromBag, fromSlot).Quantity >= quantity)

                // Move the item to the destination slot
                .Do("Move Item", time =>
                {
                    _botContext.ObjectManager.Player.PickupContainedItem(fromBag, fromSlot, quantity);
                    _botContext.ObjectManager.Player.PlaceItemInContainer(toBag, toSlot);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();

        /// <summary>
        /// Sequence to destroy an item from the inventory.
        /// </summary>
        /// <param name="itemId">The ID of the item to destroy.</param>
        /// <param name="quantity">The quantity of the item to destroy.</param>
        /// <returns>IBehaviourTreeNode that manages destroying the item.</returns>
        private IBehaviourTreeNode BuildDestroyItemSequence(int bagId, int slotId, int quantity) => new BehaviourTreeBuilder()
            .Sequence("Destroy Item Sequence")
                // Ensure the bot has the item and quantity available to destroy
                .Condition("Has Item to Destroy", time => _botContext.ObjectManager.Player.GetContainedItem(bagId, slotId) != null)

                // Destroy the item
                .Do("Destroy Item", time =>
                {
                    _botContext.ObjectManager.Player.DestroyItemInContainer(bagId, slotId, quantity);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to equip an item from a bag.
        /// </summary>
        /// <param name="bag">The bag where the item is located.</param>
        /// <param name="slot">The slot in the bag where the item is located.</param>
        /// <returns>IBehaviourTreeNode that manages equipping the item.</returns>
        private IBehaviourTreeNode BuildEquipItemSequence(int bag, int slot) => new BehaviourTreeBuilder()
            .Sequence("Equip Item Sequence")
                // Ensure the bot has the item to equip
                .Condition("Has Item", time => _botContext.ObjectManager.Player.GetContainedItem(bag, slot) != null)

                // Equip the item into the designated equipment slot
                .Do("Equip Item", time =>
                {
                    _botContext.ObjectManager.Player.EquipItem(bag, slot);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();

        /// <summary>
        /// Sequence to equip an item from a bag into a specific equipment slot.
        /// </summary>
        /// <param name="bag">The bag where the item is located.</param>
        /// <param name="slot">The slot in the bag where the item is located.</param>
        /// <param name="equipSlot">The equipment slot to place the item into.</param>
        /// <returns>IBehaviourTreeNode that manages equipping the item.</returns>
        private IBehaviourTreeNode BuildEquipItemSequence(int bag, int slot, EquipSlot equipSlot) => new BehaviourTreeBuilder()
            .Sequence("Equip Item Sequence")
                // Ensure the bot has the item to equip
                .Condition("Has Item", time => _botContext.ObjectManager.Player.GetContainedItem(bag, slot) != null)

                // Equip the item into the designated equipment slot
                .Do("Equip Item", time =>
                {
                    _botContext.ObjectManager.Player.EquipItem(bag, slot, equipSlot);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to unequip an item from a specific equipment slot and place it in the inventory.
        /// </summary>
        /// <param name="equipSlot">The equipment slot from which to unequip the item.</param>
        /// <returns>IBehaviourTreeNode that manages unequipping the item.</returns>
        private IBehaviourTreeNode BuildUnequipItemSequence(EquipSlot equipSlot) => new BehaviourTreeBuilder()
            .Sequence("Unequip Item Sequence")
                // Ensure there is an item in the specified equipment slot
                .Condition("Has Item Equipped", time => _botContext.ObjectManager.Player.GetEquippedItem(equipSlot) != null)

                // Unequip the item from the specified equipment slot
                .Do("Unequip Item", time =>
                {
                    _botContext.ObjectManager.Player.UnequipItem(equipSlot);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to split a stack of items into two slots in the inventory.
        /// </summary>
        /// <param name="bag">The bag where the stack is located.</param>
        /// <param name="slot">The slot where the stack is located.</param>
        /// <param name="quantity">The quantity to move to a new slot.</param>
        /// <param name="destinationBag">The destination bag for the split stack.</param>
        /// <param name="destinationSlot">The destination slot for the split stack.</param>
        /// <returns>IBehaviourTreeNode that manages splitting the item stack.</returns>
        private IBehaviourTreeNode BuildSplitStackSequence(int bag, int slot, int quantity, int destinationBag, int destinationSlot) => new BehaviourTreeBuilder()
            .Sequence("Split Stack Sequence")
                // Ensure the bot has the stack of items available
                .Condition("Has Item Stack", time => _botContext.ObjectManager.Player.GetContainedItem(bag, slot).Quantity >= quantity)

                // Split the stack into the destination slot
                .Do("Split Stack", time =>
                {
                    _botContext.ObjectManager.Player.SplitStack(bag, slot, quantity, destinationBag, destinationSlot);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to repair a specific item in the inventory.
        /// </summary>
        /// <param name="repairSlot">The slot where the item is located for repair.</param>
        /// <param name="cost">The cost in copper to repair the item.</param>
        /// <returns>IBehaviourTreeNode that manages repairing the item.</returns>
        private IBehaviourTreeNode BuildRepairItemSequence(int repairSlot) => new BehaviourTreeBuilder()
            .Sequence("Repair Item Sequence")
                // Ensure the bot has enough money to repair the item
                .Condition("Can Afford Repair", time => _botContext.ObjectManager.Player.Copper > _botContext.ObjectManager.MerchantFrame.RepairCost((EquipSlot)repairSlot))

                // Repair the item in the specified slot
                .Do("Repair Item", time =>
                {
                    _botContext.ObjectManager.MerchantFrame.RepairByEquipSlot((EquipSlot)repairSlot);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to repair all damaged items in the inventory.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages repairing all items.</returns>
        private IBehaviourTreeNode RepairAllItemsSequence => new BehaviourTreeBuilder()
            .Sequence("Repair All Items Sequence")
                // Ensure the bot has enough money to repair all items
                .Condition("Can Afford Full Repair", time => _botContext.ObjectManager.Player.Copper > _botContext.ObjectManager.MerchantFrame.TotalRepairCost)

                // Repair all damaged items
                .Do("Repair All Items", time =>
                {
                    _botContext.ObjectManager.MerchantFrame.RepairAll();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to dismiss a currently active buff.
        /// </summary>
        /// <param name="buffSlot">The slot or index of the buff to dismiss.</param>
        /// <returns>IBehaviourTreeNode that manages dismissing the buff.</returns>
        private IBehaviourTreeNode BuildDismissBuffSequence(string buff) => new BehaviourTreeBuilder()
            .Sequence("Dismiss Buff Sequence")
                // Ensure the bot has the buff in the specified slot
                .Condition("Has Buff", time => _botContext.ObjectManager.Player.HasBuff(buff))

                // Dismiss the buff
                .Do("Dismiss Buff", time =>
                {
                    _botContext.ObjectManager.Player.DismissBuff(buff);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to craft an item using a specific craft recipe or slot.
        /// </summary>
        /// <param name="craftSlotId">The ID of the crafting recipe or slot to use.</param>
        /// <returns>IBehaviourTreeNode that manages crafting the item.</returns>
        private IBehaviourTreeNode BuildCraftSequence(int craftSlotId) => new BehaviourTreeBuilder()
            .Sequence("Craft Sequence")
                // Ensure the bot can craft the item
                .Condition("Can Craft Item", time => _botContext.ObjectManager.CraftFrame.HasMaterialsNeeded(craftSlotId))

                // Perform the crafting action
                .Do("Craft Item", time =>
                {
                    _botContext.ObjectManager.CraftFrame.Craft(craftSlotId);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to log the bot into the game.
        /// </summary>
        /// <param name="username">The bot's username.</param>
        /// <param name="password">The bot's password.</param>
        /// <returns>IBehaviourTreeNode that manages the login process.</returns>
        private IBehaviourTreeNode BuildLoginSequence(string username, string password) => new BehaviourTreeBuilder()
            .Sequence("Login Sequence")
                // Ensure the bot is on the login screen
                .Condition("Is On Login Screen", time => _botContext.ObjectManager.LoginScreen.IsOpen)

                // Input credentials
                .Do("Input Credentials", time =>
                {
                    _botContext.ObjectManager.LoginScreen.Login(username, password);
                    return BehaviourTreeStatus.Success;
                })

                // Wait in server queue if necessary
                .Condition("In Server Queue", time => _botContext.ObjectManager.LoginScreen.InQueue)
                .Do("Wait In Queue", time => {
                    if (_botContext.ObjectManager.LoginScreen.QueuePosition > 0)
                        return BehaviourTreeStatus.Running;
                    return BehaviourTreeStatus.Success;
                })

                // Select the first available realm
                .Condition("On Realm Selection Screen", time => _botContext.ObjectManager.RealmSelectScreen.IsOpen)
                .Do("Select Realm", time =>
                {
                    _botContext.ObjectManager.RealmSelectScreen.SelectRealm(0);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to log the bot out of the game.
        /// </summary>
        /// <returns>IBehaviourTreeNode that manages the logout process.</returns>
        private IBehaviourTreeNode LogoutSequence => new BehaviourTreeBuilder()
            .Sequence("Logout Sequence")
                // Ensure the bot can log out (not in combat, etc.)
                .Condition("Can Log Out", time => !_botContext.ObjectManager.LoginScreen.IsOpen)

                // Perform the logout action
                .Do("Log Out", time =>
                {
                    _botContext.ObjectManager.Player.Logout();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to create a new character with specified name, race, and class.
        /// </summary>
        /// <param name="parameters">A list containing the name, race, and class of the new character.</param>
        /// <returns>IBehaviourTreeNode that manages creating the character.</returns>
        private IBehaviourTreeNode BuildCreateCharacterSequence(List<object> parameters) => new BehaviourTreeBuilder()
            .Sequence("Create Character Sequence")
                // Ensure the bot is on the character creation screen
                .Condition("On Character Creation Screen", time => _botContext.ObjectManager.CharacterSelectScreen.IsOpen)

                // Create the new character with the specified details
                .Do("Create Character", time =>
                {
                    var name = (string)parameters[0];
                    var race = (Race)parameters[1];
                    var gender = (Gender)parameters[2];
                    var characterClass = (Class)parameters[3];
                    var skinColor = (int)parameters[4];
                    var face = (int)parameters[5];
                    var hairStyle = (int)parameters[6];
                    var hairColor = (int)parameters[7];
                    var miscAttribute = (int)parameters[8];

                    _botContext.ObjectManager.CharacterSelectScreen.CreateCharacter(race, gender, characterClass, skinColor, face, hairStyle, hairColor, miscAttribute, name);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to delete an existing character based on character ID.
        /// </summary>
        /// <param name="characterId">The ID of the character to delete.</param>
        /// <returns>IBehaviourTreeNode that manages deleting the character.</returns>
        private IBehaviourTreeNode BuildDeleteCharacterSequence(ulong characterId) => new BehaviourTreeBuilder()
            .Sequence("Delete Character Sequence")
                // Ensure the bot is on the character selection screen
                .Condition("On Character Select Screen", time => _botContext.ObjectManager.CharacterSelectScreen.IsOpen)

                // Delete the specified character
                .Do("Delete Character", time =>
                {
                    _botContext.ObjectManager.CharacterSelectScreen.DeleteCharacter(characterId);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to enter the game world with a selected character.
        /// </summary>
        /// <param name="characterGuid">The GUID of the character to enter the world with.</param>
        /// <returns>IBehaviourTreeNode that manages entering the game world.</returns>
        private IBehaviourTreeNode BuildEnterWorldSequence(ulong characterGuid) => new BehaviourTreeBuilder()
            .Sequence("Enter World Sequence")
                // Ensure the bot is on the character select screen
                .Condition("On Character Select Screen", time => _botContext.ObjectManager.CharacterSelectScreen.IsOpen)

                // Enter the world with the specified character
                .Do("Enter World", time =>
                {
                    _botContext.ObjectManager.CharacterSelectScreen.EnterWorld(characterGuid);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
    }
}