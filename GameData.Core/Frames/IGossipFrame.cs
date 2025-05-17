using GameData.Core.Enums;

namespace GameData.Core.Frames
{
    public interface IGossipFrame
    {
        bool IsOpen { get; }
        void Close();
        ulong NPCGuid { get; }
        void SelectGossipOption(int parOptionIndex);
        void SelectFirstGossipOfType(DialogType type);
        List<GossipOption> Options { get; }
        List<QuestOption> Quests { get; }
    }
    /// <summary>
    ///     An object representing a Gossip Option of an ingame Gossip Dialog
    /// </summary>
    public abstract class GossipOption
    {
        /// <summary>
        ///     The type of the Gossip Item
        /// </summary>
        /// <value>
        ///     The type.
        /// </value>
        public GossipTypes Type { get; }

        /// <summary>
        ///     The text of the Gossip Item
        /// </summary>
        /// <value>
        ///     The text.
        /// </value>
        public string Text { get; } = string.Empty;
    }
}
