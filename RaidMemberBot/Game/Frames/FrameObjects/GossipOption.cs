using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game.Frames.FrameObjects
{
    /// <summary>
    ///     An object representing a Gossip Option of an ingame Gossip Dialog
    /// </summary>
    public abstract class GossipOption
    {
        internal GossipOption(string parText, GossipTypes parType)
        {
            Type = parType;
            Text = parText;
        }

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
        public string Text { get; }
    }
}
