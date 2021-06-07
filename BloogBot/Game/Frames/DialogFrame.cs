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
            var vendorGuid = MemoryManager.ReadUlong((IntPtr)MemoryAddresses.DialogFrameBaseAddr);
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
