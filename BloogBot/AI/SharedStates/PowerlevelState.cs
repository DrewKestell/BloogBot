using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.AI.SharedStates
{
    class PowerlevelState : IBotState
    {
        static readonly string[] playerEmotes = { "amaze", "applaud", "bark", "beckon", "belch", "blink", "blow", "blush", "bonk", "bounce", "bow", 
            "bravo", "cheer", "chew", "chicken", "chuckle", "clap", "comfort", "cuddle", "curtsey", "dance", "drool", "excited", "fidget",
            "flex", "followme", "gaze", "guffaw", "happy", "highfive", "hug", "impatient", "kiss", "knuckles", "laugh", "listen", "love",
            "massage", "moan", "moo", "nosepick", "pat", "pizza", "ponder", "purr", "roar", "salute", "sexy", "shimmy", "silly", "train",
            "smile", "smirk", "snicker", "sniff", "soothe", "stare", "tickle", "whistle", "wave" };

        static readonly string[] targetEmotes = { "angry", "bark", "beckon", "bite", "cackle", "flex", "glare", "plead", "roar", "rude", 
            "scowl", "openfire", "slap", "snarl", "snicker", "spit", "taunt", "tease", "threaten", "yawn" };

        static readonly Random random = new Random();

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;

        WoWPlayer targetPlayer;

        int range;
        bool started;
        int startTime;
        int startDelay;
        int playerStopDistance;
        double searchFrequency;
        double jumpFrequency;
        bool initialized;

        public PowerlevelState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (!initialized)
            {
                playerStopDistance = random.Next(3, 15);
                searchFrequency = random.NextDouble() * 0.05;
                jumpFrequency = random.NextDouble() * 0.005;
                range = 30 + (ObjectManager.GetTalentRank(3, 11) * 3);
                initialized = true;
            }

            if (random.NextDouble() < jumpFrequency)
                player.LuaCall("Jump()");

            if (random.NextDouble() < 0.0025)
                Emote();

            targetPlayer = ObjectManager.Players.FirstOrDefault(p => p.Name == container.BotSettings.PowerlevelPlayerName);
            if (targetPlayer != null)
            {
                var target = ObjectManager.Units
                    .FirstOrDefault(u =>
                        u.Guid == targetPlayer.TargetGuid
                        && u.HealthPercent > 0
                        && (u.UnitReaction == UnitReaction.Hostile || u.UnitReaction == UnitReaction.Neutral)
                    )
                    ?? ObjectManager.Aggressors.FirstOrDefault();

                if (target == null || (player.TargetGuid != target.Guid && player.TargetGuid != targetPlayer.Guid))
                    player.SetTarget(targetPlayer.Guid);

                if (target != null)
                {
                    if (!started)
                    {
                        startTime = Environment.TickCount;
                        startDelay = random.Next(200, 2000);
                        started = true;
                    }

                    if (!started || Environment.TickCount - startTime < startDelay)
                        return;

                    if (player.Position.DistanceTo(target.Position) > range)
                    {
                        if (player.TargetGuid != target.Guid)
                            player.SetTarget(target.Guid);

                        var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
                        player.MoveToward(nextWaypoint);
                    }
                    else
                    {
                        initialized = false;
                        started = false;
                        botStates.Push(container.CreatePowerlevelCombatState(botStates, container, target, targetPlayer));
                        return;
                    }
                }
                else
                {
                    if (player.Position.DistanceTo(targetPlayer.Position) > playerStopDistance)
                    {
                        var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, targetPlayer.Position, false);
                        player.MoveToward(nextWaypoint);
                    }
                    else
                    {
                        player.StopAllMovement();

                        if (random.NextDouble() < searchFrequency)
                        {
                            var newFacing = (random.NextDouble() * (Math.PI * 2)) - 0.2;
                            player.SetFacing((float)newFacing);
                        }
                    }
                }
            }
            else
            {
                // TODO: what do we do when the player can't be found (they're too far away, whatever)
            }
        }

        void Emote()
        {
            string emote;
            if (player.TargetGuid == targetPlayer.Guid)
                emote = playerEmotes[random.Next(0, playerEmotes.Length - 1)];
            else
                emote = targetEmotes[random.Next(0, targetEmotes.Length - 1)];

            player.LuaCall($"DoEmote('{emote}')");
        }
    }
}
