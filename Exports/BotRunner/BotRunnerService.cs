using BotRunner.Clients;
using Communication;
using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using Xas.FluentBehaviourTree;

namespace BotRunner
{
    public class BotRunnerService
    {
        private readonly IObjectManager _objectManager;

        private readonly CharacterStateUpdateClient _characterStateUpdateClient;
        private readonly PathfindingClient _pathfindingClient;

        private ActivitySnapshot _activitySnapshot;

        private Task _asyncBotTaskRunnerTask;

        private IBehaviourTreeNode _behaviorTree;

        public BotRunnerService(IObjectManager objectManager,
                                 CharacterStateUpdateClient characterStateUpdateClient,
                                 PathfindingClient pathfindingClient)
        {
            _objectManager = objectManager;
            _activitySnapshot = new() { AccountName = "?" };

            _pathfindingClient = pathfindingClient;
            _characterStateUpdateClient = characterStateUpdateClient;
        }

        public void Start()
        {
            if (_asyncBotTaskRunnerTask == null || _asyncBotTaskRunnerTask.IsCompleted)
                _asyncBotTaskRunnerTask = StartBotTaskRunnerAsync();
        }

        private async Task StartBotTaskRunnerAsync()
        {
            var _status = BehaviourTreeStatus.Success;

            while (true)
            {
                try
                {
                    var incomingActivityMemberState = _characterStateUpdateClient.SendMemberStateUpdate(_activitySnapshot);
                    if (_behaviorTree == null || _status != BehaviourTreeStatus.Running)
                    {
                        if (_objectManager.LoginScreen.IsLoggedIn)
                        {
                            if (_objectManager.RealmSelectScreen.CurrentRealm != null)
                            {
                                if (_objectManager.CharacterSelectScreen.HasReceivedCharacterList)
                                {
                                    if (_objectManager.CharacterSelectScreen.CharacterSelects.Count > 0)
                                    {
                                        if (_objectManager.HasEnteredWorld)
                                        {
                                            if (_objectManager.Players.Any(x => x.Name == "Dallawha"))
                                            {
                                                IWoWUnit woWUnit = _objectManager.Units.First(x => x.Name == "Dallawha");

                                                float pathingDistance = _pathfindingClient.GetPathingDistance(_objectManager.Player.MapId, _objectManager.Player.Position, woWUnit.Position);
                                                float directDistance = _objectManager.Player.Position.DistanceTo(woWUnit.Position);

                                                //Console.WriteLine($"[BOT] Target: Dallawha | PathDist: {pathingDistance:F2} | DirectDist: {directDistance:F2} | PlayerPos: ({_objectManager.Player.Position.X:F2}, {_objectManager.Player.Position.Y:F2}, {_objectManager.Player.Position.Z:F2}) | TargetPos: ({woWUnit.Position.X:F2}, {woWUnit.Position.Y:F2}, {woWUnit.Position.Z:F2})");

                                                if (pathingDistance > 25)
                                                {
                                                    //Console.WriteLine($"[BOT] MOVING - Distance {pathingDistance:F2} > 25, requesting path...");

                                                    Position[] positions = _pathfindingClient.GetPath(_objectManager.Player.MapId, _objectManager.Player.Position, woWUnit.Position, true);

                                                    //Console.WriteLine($"[BOT] Path received with {positions.Length} waypoints");

                                                    if (positions.Length > 0)
                                                    {
                                                        //Console.WriteLine($"[BOT] Moving to waypoint[1]: ({positions[1].X:F2}, {positions[1].Y:F2}, {positions[1].Z:F2})");
                                                        _objectManager.MoveToward(positions[1]);
                                                    }
                                                    else
                                                    {
                                                        //Console.WriteLine($"[BOT] ERROR: Path has no waypoints!");
                                                    }
                                                }
                                                else if (!_objectManager.Player.IsFacing(woWUnit))
                                                {
                                                    //Console.WriteLine($"[BOT] FACING - Distance {pathingDistance:F2} <= 25, adjusting facing...");
                                                    //Console.WriteLine($"[BOT] Current facing: {_objectManager.Player.Facing:F2}, Target direction: {Math.Atan2(woWUnit.Position.Y - _objectManager.Player.Position.Y, woWUnit.Position.X - _objectManager.Player.Position.X):F2}");
                                                    _objectManager.Face(woWUnit.Position);
                                                }
                                                else
                                                {
                                                    //Console.WriteLine($"[BOT] STOPPED - Distance {pathingDistance:F2} <= 25 and facing target");
                                                    //Console.WriteLine($"[BOT] Movement flags: {_objectManager.Player.MovementFlags}");
                                                    _objectManager.StopAllMovement();
                                                }
                                            }
                                            else
                                            {
                                                //Console.WriteLine($"[BOT] Dallawha not found in Players list. Total players: {_objectManager.Players.Count()}");
                                                //if (_objectManager.Units.Any(x => x.Name == "Dallawha"))
                                                //{
                                                //    Console.WriteLine($"[BOT] WARNING: Dallawha found in Units but not in Players!");
                                                //}
                                                _behaviorTree = BuildWaitSequence(0);
                                            }
                                        }
                                        else
                                        {
                                            _behaviorTree = BuildEnterWorldSequence(_objectManager.CharacterSelectScreen.CharacterSelects[0].Guid);
                                        }
                                    }
                                    else
                                    {

                                        Class @class = WoWNameGenerator.ParseClassCode(_activitySnapshot.AccountName.Substring(2, 2));
                                        Race race = WoWNameGenerator.ParseRaceCode(_activitySnapshot.AccountName[..2]);
                                        Gender gender = WoWNameGenerator.DetermineGender(@class);

                                        _behaviorTree = BuildCreateCharacterSequence(
                                            [
                                                WoWNameGenerator.GenerateName(race, gender),
                                                race,
                                                gender,
                                                @class,
                                                0,
                                                0,
                                                0,
                                                0,
                                                0
                                            ]
                                        );
                                    }
                                }
                                else
                                {
                                    if (!_objectManager.CharacterSelectScreen.HasRequestedCharacterList)
                                        _behaviorTree = BuildRequestCharacterSequence();
                                }
                            }
                            else
                            {
                                _behaviorTree = BuildRealmSelectionSequence();
                            }
                        }
                        else
                        {
                            _behaviorTree = BuildLoginSequence(incomingActivityMemberState.AccountName, "PASSWORD");
                        }

                        // Tick the behavior tree to execute the current task
                        _behaviorTree.Tick(new TimeData(0.1f));
                    }

                    _activitySnapshot = incomingActivityMemberState;
                    // Delay to control the frequency of task processing
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BOT RUNNER] {ex}");
                }
                await Task.Delay(100);
            }
        }

        private IBehaviourTreeNode BuildBehaviorTreeFromActions(List<(CharacterAction, List<object>)> actionMap)
        {
            var builder = new BehaviourTreeBuilder();

            // Iterate over the action map and build sequences for each action with its parameters
            foreach (var actionEntry in actionMap)
            {
                switch (actionEntry.Item1)
                {
                    case CharacterAction.Wait:
                        builder.Splice(BuildWaitSequence((float)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.GoTo:
                        builder.Splice(BuildGoToSequence((float)actionEntry.Item2[0], (float)actionEntry.Item2[1], (float)actionEntry.Item2[2], (float)actionEntry.Item2[3]));
                        break;
                    case CharacterAction.InteractWith:
                        builder.Splice(BuildInteractWithSequence((ulong)actionEntry.Item2[0]));
                        break;

                    case CharacterAction.SelectGossip:
                        builder.Splice(BuildSelectGossipSequence((int)actionEntry.Item2[0]));
                        break;

                    case CharacterAction.SelectTaxiNode:
                        builder.Splice(BuildSelectTaxiNodeSequence((int)actionEntry.Item2[0]));
                        break;

                    case CharacterAction.AcceptQuest:
                        builder.Splice(AcceptQuestSequence);
                        break;
                    case CharacterAction.DeclineQuest:
                        builder.Splice(DeclineQuestSequence);
                        break;
                    case CharacterAction.SelectReward:
                        builder.Splice(BuildSelectRewardSequence((int)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.CompleteQuest:
                        builder.Splice(CompleteQuestSequence);
                        break;

                    case CharacterAction.TrainSkill:
                        builder.Splice(BuildTrainSkillSequence((int)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.TrainTalent:
                        builder.Splice(BuildLearnTalentSequence((int)actionEntry.Item2[0]));
                        break;

                    case CharacterAction.OfferTrade:
                        builder.Splice(BuildOfferTradeSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.OfferGold:
                        builder.Splice(BuildOfferMoneySequence((int)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.OfferItem:
                        builder.Splice(BuildOfferItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1], (int)actionEntry.Item2[2], (int)actionEntry.Item2[3]));
                        break;
                    case CharacterAction.AcceptTrade:
                        builder.Splice(AcceptTradeSequence);
                        break;
                    case CharacterAction.DeclineTrade:
                        builder.Splice(DeclineTradeSequence);
                        break;
                    case CharacterAction.EnchantTrade:
                        builder.Splice(BuildOfferEnchantSequence((int)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.LockpickTrade:
                        builder.Splice(OfferLockpickSequence);
                        break;

                    case CharacterAction.PromoteLeader:
                        builder.Splice(BuildPromoteLeaderSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.PromoteAssistant:
                        builder.Splice(BuildPromoteAssistantSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.PromoteLootManager:
                        builder.Splice(BuildPromoteLootManagerSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.SetGroupLoot:
                        builder.Splice(BuildSetGroupLootSequence((GroupLootSetting)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.AssignLoot:
                        builder.Splice(BuildAssignLootSequence((int)actionEntry.Item2[0], (ulong)actionEntry.Item2[1]));
                        break;

                    case CharacterAction.LootRollNeed:
                        builder.Splice(BuildLootRollNeedSequence((int)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.LootRollGreed:
                        builder.Splice(BuildLootRollGreedSequence((int)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.LootPass:
                        builder.Splice(BuildLootPassSequence((int)actionEntry.Item2[0]));
                        break;

                    case CharacterAction.SendGroupInvite:
                        builder.Splice(BuildSendGroupInviteSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.AcceptGroupInvite:
                        builder.Splice(AcceptGroupInviteSequence);
                        break;
                    case CharacterAction.DeclineGroupInvite:
                        builder.Splice(DeclineGroupInviteSequence);
                        break;
                    case CharacterAction.KickPlayer:
                        builder.Splice(BuildKickPlayerSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.LeaveGroup:
                        builder.Splice(LeaveGroupSequence);
                        break;
                    case CharacterAction.DisbandGroup:
                        builder.Splice(DisbandGroupSequence);
                        break;
                    case CharacterAction.StopAttack:
                        builder.Splice(StopAttackSequence);
                        break;
                    case CharacterAction.CastSpell:
                        builder.Splice(BuildCastSpellSequence((int)actionEntry.Item2[0], (ulong)actionEntry.Item2[1]));
                        break;
                    case CharacterAction.StopCast:
                        builder.Splice(StopCastSequence);
                        break;

                    case CharacterAction.UseItem:
                        builder.Splice(BuildUseItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1], (ulong)actionEntry.Item2[2]));
                        break;
                    case CharacterAction.EquipItem:
                        builder.Splice(BuildEquipItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1], (EquipSlot)actionEntry.Item2[2]));
                        break;
                    case CharacterAction.UnequipItem:
                        builder.Splice(BuildUnequipItemSequence((EquipSlot)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.DestroyItem:
                        builder.Splice(BuildDestroyItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1], (int)actionEntry.Item2[2]));
                        break;
                    case CharacterAction.MoveItem:
                        builder.Splice(BuildMoveItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1], (int)actionEntry.Item2[2], (int)actionEntry.Item2[3], (int)actionEntry.Item2[4]));
                        break;
                    case CharacterAction.SplitStack:
                        builder.Splice(BuildSplitStackSequence((int)actionEntry.Item2[0],
                            (int)actionEntry.Item2[1],
                            (int)actionEntry.Item2[2],
                            (int)actionEntry.Item2[3],
                            (int)actionEntry.Item2[4]));
                        break;

                    case CharacterAction.BuyItem:
                        builder.Splice(BuildBuyItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1]));
                        break;
                    case CharacterAction.BuybackItem:
                        builder.Splice(BuildBuybackItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1]));
                        break;
                    case CharacterAction.SellItem:
                        builder.Splice(BuildSellItemSequence((int)actionEntry.Item2[0], (int)actionEntry.Item2[1], (int)actionEntry.Item2[2]));
                        break;
                    case CharacterAction.RepairItem:
                        builder.Splice(BuildRepairItemSequence((int)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.RepairAllItems:
                        builder.Splice(RepairAllItemsSequence);
                        break;

                    case CharacterAction.DismissBuff:
                        builder.Splice(BuildDismissBuffSequence((string)actionEntry.Item2[0]));
                        break;

                    case CharacterAction.Resurrect:
                        builder.Splice(ResurrectSequence);
                        break;

                    case CharacterAction.Craft:
                        builder.Splice(BuildCraftSequence((int)actionEntry.Item2[0]));
                        break;

                    case CharacterAction.Login:
                        builder.Splice(BuildLoginSequence((string)actionEntry.Item2[0], (string)actionEntry.Item2[1]));
                        break;
                    case CharacterAction.Logout:
                        builder.Splice(LogoutSequence);
                        break;
                    case CharacterAction.CreateCharacter:
                        builder.Splice(BuildCreateCharacterSequence(actionEntry.Item2));
                        break;
                    case CharacterAction.DeleteCharacter:
                        builder.Splice(BuildDeleteCharacterSequence((ulong)actionEntry.Item2[0]));
                        break;
                    case CharacterAction.EnterWorld:
                        builder.Splice(BuildEnterWorldSequence((ulong)actionEntry.Item2[0]));
                        break;

                    default:
                        break;
                }
            }

            return builder.Build();
        }
        private static IBehaviourTreeNode BuildWaitSequence(float duration) => new BehaviourTreeBuilder()
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
                .Do("Move to Location", time =>
                {
                    if (_objectManager.Player.Facing >= f - 0.1f && _objectManager.Player.Facing <= f + 0.1f)
                    {
                        if (_objectManager.Player.Position.DistanceTo(new Position(x, y, z)) < 1)
                            return BehaviourTreeStatus.Success;
                        else
                            _objectManager.MoveToward(new Position(x, y, z), f);
                    }
                    else
                        _objectManager.SetFacing(f);

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
                .Condition("Has Valid Target", time => _objectManager.Player.TargetGuid == guid)

                // Perform the interaction
                .Do("Interact with Target", time =>
                {
                    _objectManager.GameObjects.First(x => x.Guid == guid).Interact();
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
                .Condition("Has Target", time => _objectManager.Player != null
                                                 && _objectManager.Player.TargetGuid != 0)
                // If no target, set the target to the provided GUID
                .Do("Set Target", time =>
                {
                    if (_objectManager.Player.TargetGuid == 0)
                    {
                        _objectManager.SetTarget(guid);
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
                .Condition("Has Valid Gossip Target", time => _objectManager.GossipFrame.IsOpen
                                                            && _objectManager.GossipFrame.Options.Count > 0)

                // Select the gossip option
                .Do("Select Gossip Option", time =>
                {
                    _objectManager.GossipFrame.SelectGossipOption(selection);
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
                .Condition("Has Taxi Node Unlocked", time => _objectManager.TaxiFrame.HasNodeUnlocked(nodeId))

                // Ensure the bot has enough gold for the flight
                .Condition("Has Enough Gold", time => _objectManager.Player.Copper > _objectManager.TaxiFrame.Nodes[nodeId].Cost)

                // Select the taxi node
                .Do("Select Taxi Node", time =>
                {
                    _objectManager.TaxiFrame.SelectNode(nodeId);
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
                .Condition("Can Accept Quest", time => _objectManager.QuestFrame.IsOpen)

                // Accept the quest from the NPC
                .Do("Accept Quest", time =>
                {
                    _objectManager.QuestFrame.AcceptQuest();
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
                .Condition("Can Decline Quest", time => _objectManager.QuestFrame.IsOpen)

                // Decline the quest
                .Do("Decline Quest", time =>
                {
                    _objectManager.QuestFrame.DeclineQuest();
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
                .Condition("Can Select Reward", time => _objectManager.QuestFrame.IsOpen)

                // Select the specified reward
                .Do("Select Reward", time =>
                {
                    _objectManager.QuestFrame.CompleteQuest(rewardIndex);
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
                .Condition("Can Complete Quest", time => _objectManager.QuestFrame.IsOpen)

                // Complete the quest
                .Do("Complete Quest", time =>
                {
                    _objectManager.QuestFrame.CompleteQuest();
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
                .Condition("Is At Trainer", time => _objectManager.TrainerFrame.IsOpen)

                // Ensure the bot has enough gold to train the skill
                .Condition("Has Enough Gold", time => _objectManager.Player.Copper > _objectManager.TrainerFrame.Spells.ElementAt(spellIndex).Cost)

                // Train the skill
                .Do("Train Skill", time =>
                {
                    _objectManager.TrainerFrame.TrainSpell(spellIndex);
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
                .Condition("Can Train Talent", time => _objectManager.TalentFrame.TalentPointsAvailable > 1)

                // Train the talent
                .Do("Train Talent", time =>
                {
                    _objectManager.TalentFrame.LearnTalent(talentSpellId);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        private IBehaviourTreeNode BuildBuyItemSequence(int slotId, int quantity) => new BehaviourTreeBuilder()
                .Sequence("BuyItem Sequence")
                    .Do("Buy Item", time =>
                    {
                        _objectManager.MerchantFrame.BuyItem(slotId, quantity);
                        return BehaviourTreeStatus.Success;
                    })
                .End()
                .Build();
        private IBehaviourTreeNode BuildBuybackItemSequence(int slotId, int quantity) => new BehaviourTreeBuilder()
                .Sequence("BuybackItem Sequence")
                    .Do("Buy Item", time =>
                    {
                        _objectManager.MerchantFrame.BuybackItem(slotId, quantity);
                        return BehaviourTreeStatus.Success;
                    })
                .End()
                .Build();
        private IBehaviourTreeNode BuildSellItemSequence(int bagId, int slotId, int quantity) => new BehaviourTreeBuilder()
                .Sequence("SellItem Sequence")
                    .Do("Sell Item", time =>
                    {
                        _objectManager.MerchantFrame.SellItem(bagId, slotId, quantity);
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
                .Condition("Is Any Auto-Attack Active", time => _objectManager.Player.IsAutoAttacking)

                // Disable all auto-attacks
                .Do("Stop All Auto-Attacks", time =>
                {
                    _objectManager.StopAttack();
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
                .Condition("Can Cast Spell", time => _objectManager.CanCastSpell(spellId, targetGuid))

                // Cast the spell
                .Do("Cast Spell", time =>
                {
                    _objectManager.CastSpell(spellId);
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
                .Condition("Is Casting", time => _objectManager.Player.IsCasting || _objectManager.Player.IsChanneling)

                // Stop the current spell cast
                .Do("Stop Spell Cast", time =>
                {
                    _objectManager.StopCasting();
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
                .Condition("Can Resurrect", time => _objectManager.Player.InGhostForm && _objectManager.Player.CanResurrect)

                // Perform the resurrection action
                .Do("Resurrect", time =>
                {
                    _objectManager.AcceptResurrect();
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
                .Condition("Has Valid Trade Target", time => _objectManager.Player.Position.DistanceTo(_objectManager.Players.First(x => x.Guid == targetGuid).Position) < 5.33f)

                // Offer trade to the target
                .Do("Offer Trade", time =>
                {
                    _objectManager.Players.First(x => x.Guid == targetGuid).OfferTrade();
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
                .Condition("Trade Window Valid", time => _objectManager.TradeFrame.IsOpen)

                // Ensure the bot has enough money to offer
                .Condition("Has Enough Money", time => _objectManager.Player.Copper > copperCount)

                // Offer money in the trade
                .Do("Offer Money", time =>
                {
                    _objectManager.TradeFrame.OfferMoney(copperCount);
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
                .Condition("Trade Window Valid", time => _objectManager.TradeFrame.IsOpen)

                // Ensure the bot has the item and quantity to offer
                .Condition("Has Item to Offer", time => _objectManager.GetContainedItem(bagId, slotId).Quantity >= quantity)

                // Offer the item in the trade window
                .Do("Offer Item", time =>
                {
                    _objectManager.TradeFrame.OfferItem(bagId, slotId, quantity, tradeWindowSlot);
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
                .Condition("Trade Window Valid", time => _objectManager.TradeFrame.IsOpen)

                // Accept the trade
                .Do("Accept Trade", time =>
                {
                    _objectManager.TradeFrame.AcceptTrade();
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
                .Condition("Trade Window Valid", time => _objectManager.TradeFrame.IsOpen)

                // Decline the trade
                .Do("Decline Trade", time =>
                {
                    _objectManager.TradeFrame.DeclineTrade();
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
                .Condition("Trade Window Valid", time => _objectManager.TradeFrame.IsOpen)

                //// Ensure the bot has the correct enchantment to offer
                //.Condition("Has Enchant Available", time => _objectManager.HasEnchantAvailable(enchantId))

                // Offer the enchantment in the trade
                .Do("Offer Enchant", time =>
                {
                    _objectManager.TradeFrame.OfferEnchant(enchantId);
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
                .Condition("Can Lockpick", time => _objectManager.Player.Class == Class.Rogue)

                // Offer lockpicking in the trade
                .Do("Offer Lockpick", time =>
                {
                    _objectManager.TradeFrame.OfferLockpick();
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
                .Condition("Is In Group with Player", time => _objectManager.PartyMembers.Any(x => x.Guid == playerGuid))

                // Promote the player to group leader
                .Do("Promote Leader", time =>
                {
                    _objectManager.PromoteLeader(playerGuid);
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
                .Condition("Is In Group with Player", time => _objectManager.PartyMembers.Any(x => x.Guid == playerGuid))

                // Promote the player to group assistant
                .Do("Promote Assistant", time =>
                {
                    _objectManager.PromoteAssistant(playerGuid);
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
                .Condition("Is In Group with Player", time => _objectManager.PartyMembers.Any(x => x.Guid == playerGuid))

                // Promote the player to loot manager
                .Do("Promote Loot Manager", time =>
                {
                    _objectManager.PromoteLootManager(playerGuid);
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
                .Condition("Can Set Loot Rules", time => _objectManager.PartyLeaderGuid == _objectManager.Player.Guid)

                // Set the group loot rule
                .Do("Set Group Loot", time =>
                {
                    _objectManager.SetGroupLoot(setting);
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
                .Condition("Can Assign Loot", time => _objectManager.PartyMembers.Any(x => x.Guid == playerGuid))

                // Assign the loot to the specified player
                .Do("Assign Loot", time =>
                {
                    _objectManager.AssignLoot(itemId, playerGuid);
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
                .Condition("Can Roll Need", time => _objectManager.HasLootRollWindow(itemId))

                // Roll "Need" for the item
                .Do("Roll Need", time =>
                {
                    _objectManager.LootRollNeed(itemId);
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
                .Condition("Can Roll Greed", time => _objectManager.HasLootRollWindow(itemId))

                // Roll "Greed" for the item
                .Do("Roll Greed", time =>
                {
                    _objectManager.LootRollGreed(itemId);
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
                .Condition("Can Pass Loot", time => _objectManager.HasLootRollWindow(itemId))

                // Pass on the loot item
                .Do("Pass Loot", time =>
                {
                    _objectManager.LootPass(itemId);
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
                .Condition("Can Send Group Invite", time => !_objectManager.PartyMembers.Any(x => x.Guid == playerGuid))

                // Send the group invite
                .Do("Send Group Invite", time =>
                {
                    _objectManager.InviteToGroup(playerGuid);
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
                .Condition("Has Pending Invite", time => _objectManager.HasPendingGroupInvite())

                // Accept the group invite
                .Do("Accept Group Invite", time =>
                {
                    _objectManager.AcceptGroupInvite();
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
                .Condition("Has Pending Invite", time => _objectManager.HasPendingGroupInvite())

                // Decline the group invite
                .Do("Decline Group Invite", time =>
                {
                    _objectManager.DeclineGroupInvite();
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
                .Condition("Can Kick Player", time => _objectManager.Player.Guid == _objectManager.PartyLeaderGuid)

                // Kick the player from the group
                .Do("Kick Player", time =>
                {
                    _objectManager.KickPlayer(playerGuid);
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
                .Condition("Is In Group", time => _objectManager.PartyLeaderGuid != 0)

                // Leave the group
                .Do("Leave Group", time =>
                {
                    _objectManager.LeaveGroup();
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
                .Condition("Is Group Leader", time => _objectManager.Player.Guid == _objectManager.PartyLeaderGuid)

                // Disband the group
                .Do("Disband Group", time =>
                {
                    _objectManager.DisbandGroup();
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
                .Condition("Has Item", time => _objectManager.GetContainedItem(fromBag, fromSlot) != null)

                // Use the item on the target (or self if target is null)
                .Do("Use Item", time =>
                {
                    _objectManager.UseItem(fromBag, fromSlot, targetGuid);
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
                .Condition("Has Item to Move", time => _objectManager.GetContainedItem(fromBag, fromSlot).Quantity >= quantity)

                // Move the item to the destination slot
                .Do("Move Item", time =>
                {
                    _objectManager.PickupContainedItem(fromBag, fromSlot, quantity);
                    _objectManager.PlaceItemInContainer(toBag, toSlot);
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
                .Condition("Has Item to Destroy", time => _objectManager.GetContainedItem(bagId, slotId) != null)

                // Destroy the item
                .Do("Destroy Item", time =>
                {
                    _objectManager.DestroyItemInContainer(bagId, slotId, quantity);
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
                .Condition("Has Item", time => _objectManager.GetContainedItem(bag, slot) != null)

                // Equip the item into the designated equipment slot
                .Do("Equip Item", time =>
                {
                    _objectManager.EquipItem(bag, slot);
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
                .Condition("Has Item", time => _objectManager.GetContainedItem(bag, slot) != null)

                // Equip the item into the designated equipment slot
                .Do("Equip Item", time =>
                {
                    _objectManager.EquipItem(bag, slot, equipSlot);
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
                .Condition("Has Item Equipped", time => _objectManager.GetEquippedItem(equipSlot) != null)

                // Unequip the item from the specified equipment slot
                .Do("Unequip Item", time =>
                {
                    _objectManager.UnequipItem(equipSlot);
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
                .Condition("Has Item Stack", time => _objectManager.GetContainedItem(bag, slot).Quantity >= quantity)

                // Split the stack into the destination slot
                .Do("Split Stack", time =>
                {
                    _objectManager.SplitStack(bag, slot, quantity, destinationBag, destinationSlot);
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
                .Condition("Can Afford Repair", time => _objectManager.Player.Copper > _objectManager.MerchantFrame.RepairCost((EquipSlot)repairSlot))

                // Repair the item in the specified slot
                .Do("Repair Item", time =>
                {
                    _objectManager.MerchantFrame.RepairByEquipSlot((EquipSlot)repairSlot);
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
                .Condition("Can Afford Full Repair", time => _objectManager.Player.Copper > _objectManager.MerchantFrame.TotalRepairCost)

                // Repair all damaged items
                .Do("Repair All Items", time =>
                {
                    _objectManager.MerchantFrame.RepairAll();
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
                .Condition("Has Buff", time => _objectManager.Player.HasBuff(buff))

                // Dismiss the buff
                .Do("Dismiss Buff", time =>
                {
                    _objectManager.Player.DismissBuff(buff);
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
                .Condition("Can Craft Item", time => _objectManager.CraftFrame.HasMaterialsNeeded(craftSlotId))

                // Perform the crafting action
                .Do("Craft Item", time =>
                {
                    _objectManager.CraftFrame.Craft(craftSlotId);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();

        /// <summary>
        /// Sequence to log the bot into the server to get a session key.
        /// </summary>
        /// <param name="username">The bot's username.</param>
        /// <param name="password">The bot's password.</param>
        /// <returns>IBehaviourTreeNode that manages the login process.</returns>
        private IBehaviourTreeNode BuildLoginSequence(string username, string password) => new BehaviourTreeBuilder()
            .Sequence("Login Sequence")
                // Ensure the bot is on the login screen
                .Condition("Is On Login Screen", time => _objectManager.LoginScreen.IsOpen)

                // Input credentials
                .Do("Input Credentials", time =>
                {
                    if (_objectManager.LoginScreen.IsLoggedIn) return BehaviourTreeStatus.Success;

                    _objectManager.LoginScreen.Login(username, password);
                    return BehaviourTreeStatus.Success;
                })

                // Select the first available realm
                .Condition("Waiting in queue", time => _objectManager.LoginScreen.IsLoggedIn)
                .Do("Select Realm", time =>
                {
                    if (_objectManager.LoginScreen.QueuePosition > 0)
                        return BehaviourTreeStatus.Running;
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();

        /// <summary>
        /// Sequence to log the bot into the server to get a session key.
        /// </summary>
        /// <param name="username">The bot's username.</param>
        /// <param name="password">The bot's password.</param>
        /// <returns>IBehaviourTreeNode that manages the login process.</returns>
        private IBehaviourTreeNode BuildRealmSelectionSequence() => new BehaviourTreeBuilder()
            .Sequence("Realm Selection Sequence")
                // Select the first available realm
                .Condition("On Realm Selection Screen", time => _objectManager.RealmSelectScreen.IsOpen && _objectManager.LoginScreen.IsLoggedIn)
                .Do("Select Realm", time =>
                {
                    if (_objectManager.RealmSelectScreen.CurrentRealm != null) return BehaviourTreeStatus.Success;

                    _objectManager.RealmSelectScreen.SelectRealm(_objectManager.RealmSelectScreen.GetRealmList()[0]);
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
                .Condition("Can Log Out", time => !_objectManager.LoginScreen.IsOpen)

                // Perform the logout action
                .Do("Log Out", time =>
                {
                    _objectManager.Logout();
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
        /// <summary>
        /// Sequence to create a new character with specified name, race, and class.
        /// </summary>
        /// <param name="parameters">A list containing the name, race, and class of the new character.</param>
        /// <returns>IBehaviourTreeNode that manages creating the character.</returns>
        private IBehaviourTreeNode BuildRequestCharacterSequence() => new BehaviourTreeBuilder()
            .Sequence("Create Character Sequence")
                // Ensure the bot is on the character creation screen
                .Condition("On Character Creation Screen", time => _objectManager.CharacterSelectScreen.IsOpen)

                // Create the new character with the specified details
                .Do("Request Character List", time =>
                {
                    _objectManager.CharacterSelectScreen.RefreshCharacterListFromServer();
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
                .Condition("On Character Creation Screen", time => _objectManager.CharacterSelectScreen.IsOpen)

                // Create the new character with the specified details
                .Do("Create Character", time =>
                {
                    var name = (string)parameters[0];
                    var race = (Race)parameters[1];
                    var gender = (Gender)parameters[2];
                    var characterClass = (Class)parameters[3];

                    _objectManager.CharacterSelectScreen.CreateCharacter(name, race, gender, characterClass, 0, 0, 0, 0, 0, 0);
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
                .Condition("On Character Select Screen", time => _objectManager.CharacterSelectScreen.IsOpen)

                // Delete the specified character
                .Do("Delete Character", time =>
                {
                    _objectManager.CharacterSelectScreen.DeleteCharacter(characterId);
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
                .Condition("On Character Select Screen", time => _objectManager.CharacterSelectScreen.IsOpen)

                // Enter the world with the specified character
                .Do("Enter World", time =>
                {
                    _objectManager.EnterWorld(characterGuid);
                    return BehaviourTreeStatus.Success;
                })
            .End()
            .Build();
    }
}