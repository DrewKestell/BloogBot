using BotRunner.Interfaces;

namespace WoWSharpClient.Frames
{
    public class DialogFrame : IDialogFrame
    {
        public IList<DialogOption> DialogOptions { get; } = [];

        public void CloseDialogFrame() { }

        public void SelectFirstGossipOfType(DialogType type)
        {

        }
    }

    public class DialogOption
    {
        internal DialogOption(DialogType type) => Type = type;

        public DialogType Type { get; }
    }
}
