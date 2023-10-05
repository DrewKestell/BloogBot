using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Game.Frames.FrameObjects;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using System;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game.Frames
{
    /// <summary>
    ///     Representing the currently open QuestGreeting Frame
    /// </summary>
    public sealed class QuestGreetingFrame
    {
        private static volatile QuestGreetingFrame _instance;
        private static readonly object lockObject = new object();
        private static volatile bool _isOpen;
        private static volatile bool _abort;

        private readonly List<QuestOption> _Quests = new List<QuestOption>();

        private QuestGreetingFrame()
        {
            _Quests.Clear();

            var availableQuestsCount = 0xBE0834.ReadAs<int>();
            var activeQuestsCount = 0xBE0838.ReadAs<int>();

            for (var i = 0; i < availableQuestsCount; i++)
            {
                var id = (0xBDFE60 + 0x4c * i).ReadAs<int>();
                var name = (0xBDFE68 + 0x4c * i).ReadString();
                _Quests.Add(new QuestOptionInterface(id, name, QuestGossipState.Available));
            }

            for (var i = 0; i < activeQuestsCount; i++)
            {
                var id = (0xBDE690 + 0x4c * i).ReadAs<int>();
                var name = (0xBDE698 + 0x4c * i).ReadString();
                _Quests.Add(new QuestOptionInterface(id, name, QuestGossipState.Accepted));
            }
        }

        /// <summary>
        ///     Access to the current Gossip-Frame-Object
        /// </summary>
        /// <value>
        ///     The frame.
        /// </value>
        public static QuestGreetingFrame Instance => _instance;

        /// <summary>
        ///     Tells if there is an open Gossip-Frame right now
        /// </summary>
        /// <value>
        /// </value>
        public static bool IsOpen => _isOpen;

        /// <summary>
        ///     Guid of the NPC the QuestGreeting belongs to
        /// </summary>
        public ulong NpcGuid => ObjectManager.Instance.Player.QuestNpcGuid;

        /// <summary>
        ///     A list of all quest options the current Gossip-Frame offers
        /// </summary>
        /// <value>
        ///     The quests.
        /// </value>
        public IReadOnlyList<QuestOption> Quests => _Quests;

        internal static event EventHandler OnQuestGreetingFrameOpen;
        internal static event EventHandler OnQuestGreetingFrameClosed;

        internal static void Create()
        {
            lock (lockObject)
            {
                _isOpen = false;
                _abort = false;

                var tmp = new QuestGreetingFrame();
                if (_abort || ObjectManager.Instance.Player.QuestNpcGuid == 0) return;

                _instance = tmp;
                _isOpen = true;
                OnQuestGreetingFrameOpen?.Invoke(tmp, new EventArgs());
            }
        }

        internal static void Destroy()
        {
            _abort = true;
            lock (lockObject)
            {
                _isOpen = false;
                OnQuestGreetingFrameClosed?.Invoke(null, new EventArgs());
            }
        }

        /// <summary>
        ///     Accepts the quest with the specified id
        /// </summary>
        /// <param name="parId">The Id.</param>
        public void AcceptQuest(int parId)
        {
            foreach (var q in _Quests)
                if (parId == q.Id)
                {
                    Functions.AcceptQuest(NpcGuid, parId);
                    break;
                }
        }

        /// <summary>
        ///     Completes the quest with the specified id
        /// </summary>
        /// <param name="parId">The Id.</param>
        public void CompleteQuest(int parId)
        {
            foreach (var q in _Quests)
                if (parId == q.Id)
                {
                    Functions.CompleteQuest(NpcGuid, parId);
                    break;
                }
        }

        private class QuestOptionInterface : QuestOption
        {
            internal QuestOptionInterface(int parId, string parText, QuestGossipState parState)
                : base(parId, parText, parState)
            {
            }
        }
    }
}
