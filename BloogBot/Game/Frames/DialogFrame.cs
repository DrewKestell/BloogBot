using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;

namespace BloogBot.Game.Frames
{
    public class DialogFrame
    {
        public DialogFrame()
        {
            if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
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
            else
            {
                var vendorGuid = MemoryManager.ReadUlong((IntPtr)MemoryAddresses.DialogFrameBase);
                if (vendorGuid == 0)
                    return;

                var dialogOptionCount = Convert.ToInt32(ObjectManager.Player.LuaCallWithResults($"{{0}} = GetNumGossipOptions()")[0]);

                var script = "";
                for (var i = 0; i < dialogOptionCount; i++)
                {
                    var startingIndex = i * 2;

                    script += "{" + startingIndex + "}, ";
                    script += "{" + (startingIndex + 1) + "}";

                    if (i + 1 == dialogOptionCount)
                        script += " = GetGossipOptions()";
                    else
                        script += ", ";
                }

                var dialogOptions = ObjectManager.Player.LuaCallWithResults(script);

                for (var i = 0; i < dialogOptionCount; i++)
                {
                    var startingIndex = i * 2;

                    var type = (DialogType)Enum.Parse(typeof(DialogType), dialogOptions[startingIndex + 1]);
                    DialogOptions.Add(new DialogOption(type));
                }
            }
        }

        public IList<DialogOption> DialogOptions { get; } = new List<DialogOption>();

        public void CloseDialogFrame(WoWPlayer player) => player.LuaCall("CloseGossip()");

        public void SelectFirstGossipOfType(WoWPlayer player, DialogType type)
        {
            for (var i = 0; i < DialogOptions.Count; i++)
            {
                if (DialogOptions[i].Type != type) continue;
                player.LuaCall("SelectGossipOption(" + (i + 1) + ")");
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
