using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Game.Statics;
using System;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game.Frames
{
    /// <summary>
    ///     Represents a Quest-Frame (Frame where the player either clicks Continue, Complete or Accept)
    /// </summary>
    public sealed class QuestFrame
    {
        private QuestFrame()
        {
            var rewardCount = 0;
            while (true)
            {
                var id = (0xbdf02c + (rewardCount + rewardCount * 8) * 4).ReadAs<int>();
                if (id == 0) break;
                rewardCount++;
            }
            RewardCount = rewardCount;
        }

        /// <summary>
        ///     Tells whether a Quest-Frame is open or not
        /// </summary>
        /// <value>
        /// </value>
        public static bool IsOpen => Instance != null && Instance.QuestFrameState != QuestFrameState.Greeting;

        /// <summary>
        ///     Access to the Quest-Frame
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static QuestFrame Instance { get; private set; }

        /// <summary>
        ///     The type of the currently open Quest-Frame
        /// </summary>
        /// <value>
        ///     The state of the quest frame.
        /// </value>
        public QuestFrameState QuestFrameState
        {
            get
            {
                var tmp = 0xBE0818.ReadAs<int>();
                return (QuestFrameState)tmp;
            }
        }

        /// <summary>
        ///     Guid of the NPC the QuestFrame belongs to
        /// </summary>
        public ulong NpcGuid => ObjectManager.Instance.Player.QuestNpcGuid;

        /// <summary>
        ///     The ID of the quest belonging to the currently open Quest-Frame
        /// </summary>
        /// <value>
        ///     The quest frame identifier.
        /// </value>
        public int QuestFrameId
        {
            get
            {
                if (QuestFrameState == QuestFrameState.Greeting) return 0;
                return 0xBE081C.ReadAs<int>();
            }
        }

        /// <summary>
        ///     Count of rewards you are able to pick from
        /// </summary>
        public int RewardCount { get; }

        internal static event EventHandler OnQuestFrameOpen;
        internal static event EventHandler OnQuestFrameClosed;

        internal static void Destroy()
        {
            Instance = null;
            OnQuestFrameClosed?.Invoke(null, new EventArgs());
        }

        internal static void Create()
        {
            Instance = new QuestFrame();
            OnQuestFrameOpen?.Invoke(Instance, new EventArgs());
        }

        /// <summary>
        ///     Proceeds the Quest-Frame (either with Accept or Continue)
        /// </summary>
        /// <returns></returns>
        public bool Proceed()
        {
            switch (QuestFrameState)
            {
                case QuestFrameState.Accept:
                    Lua.Instance.Execute("QuestFrameAcceptButton:Click()");
                    return true;

                case QuestFrameState.Continue:
                    Lua.Instance.Execute("QuestFrameCompleteButton:Click()");
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Completes the Quest belonging to the currently open Quest-Frame and chooses a reward
        /// </summary>
        /// <param name="parReward">The reward index (starting at 0)</param>
        public void Complete(int? parReward = null)
        {
            if (QuestFrameState != QuestFrameState.Complete) return;
            if (parReward == null)
            {
                Lua.Instance.Execute($"GetQuestReward(0);");
                return;
            }
            if (parReward < 0 || parReward >= RewardCount) return;
            Lua.Instance.Execute($"GetQuestReward({parReward});");
        }
    }
}
