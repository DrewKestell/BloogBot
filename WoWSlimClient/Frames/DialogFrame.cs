using static WoWSlimClient.Models.Enums;

namespace WoWSlimClient.Frames
{
    public class DialogFrame
    {       

        public IList<DialogOption> DialogOptions { get; } = new List<DialogOption>();

        public void CloseDialogFrame() { }

        public void SelectFirstGossipOfType(DialogType type)
        {
            
        }
    }

    public class DialogOption
    {
        internal DialogOption(DialogType type)
        {
            Type = type;
        }

        public DialogType Type { get; }
    }
}
