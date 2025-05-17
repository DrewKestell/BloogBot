using ForegroundBotRunner.Mem;
using GameData.Core.Enums;

namespace ForegroundBotRunner.Frames
{
    public class DialogFrame
    {
        public DialogFrame()
        {
            var currentItem = (nint)0xBBBE90;
            while ((int)currentItem < 0xBC3F50)
            {
                if (MemoryManager.ReadInt(currentItem + 0x800) == -1) break;
                var optionType = MemoryManager.ReadInt(currentItem + 0x808);

                DialogOptions.Add(new DialogOption((DialogType)optionType));
                currentItem = nint.Add(currentItem, 0x80C);
            }
        }

        public IList<DialogOption> DialogOptions { get; } = [];

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
        internal DialogOption(DialogType type) => Type = type;

        public DialogType Type { get; }
    }
}
