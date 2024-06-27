using WoWActivityMember.Mem;
using static WoWActivityMember.Constants.Enums;

namespace WoWActivityMember.Game.Frames
{
    public class DialogFrame
    {
        public DialogFrame()
        {
            var currentItem = (IntPtr)0xBBBE90;
            while ((int)currentItem < 0xBC3F50)
            {
                if (MemoryManager.ReadInt((currentItem + 0x800)) == -1) break;
                var optionType = MemoryManager.ReadInt((currentItem + 0x808));

                DialogOptions.Add(new DialogOption((DialogType)optionType));
                currentItem = IntPtr.Add(currentItem, 0x80C);
            }
        }

        public IList<DialogOption> DialogOptions { get; } = new List<DialogOption>();

        public void CloseDialogFrame() => Functions.LuaCall("CloseGossip()");

        public void SelectFirstGossipOfType(DialogType type)
        {
            for (var i = 0; i < DialogOptions.Count; i++)
            {
                if (DialogOptions[i].Type != type) continue;
                Functions.LuaCall("SelectGossipOption(" + (i + 1) + ")");
                return;
            }
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
