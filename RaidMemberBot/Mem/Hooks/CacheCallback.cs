using System;
using System.Runtime.InteropServices;

namespace RaidMemberBot.Mem.Hooks
{
    internal sealed class CacheCallbacks
    {
        private static readonly object lockObject = new object();

        private static readonly Lazy<CacheCallbacks> _instance =
            new Lazy<CacheCallbacks>(() => new CacheCallbacks());


        private CacheCallbackDelegate _itemCacheCallbackDelegate;
        private CacheCallbackDelegate _questCacheCallbackDelegate;

        private CacheCallbacks()
        {
            InitQuestCacheCallback();
            InitItemCacheCallback();
        }

        internal static CacheCallbacks Instance => _instance.Value;

        internal IntPtr ItemCallbackPtr { get; private set; }
        internal IntPtr QuestCallbackPtr { get; private set; }

        private void InitQuestCacheCallback()
        {
            _questCacheCallbackDelegate = QuestCacheCallbackHookFunc;
            var addrToDetour = Marshal.GetFunctionPointerForDelegate(_questCacheCallbackDelegate);

            string[] asmCode =
            {
                //"pushfd",
                //"pushad",
                //"push ecx",
                "call " + (uint) addrToDetour,
                //"popad",
                //"popfd",

                "retn 0x8"
            };
            QuestCallbackPtr = Memory.InjectAsm(asmCode, "QuestCacheCallbackDetour");
        }

        private void InitItemCacheCallback()
        {
            // Pointer the delegate to our c# function
            _itemCacheCallbackDelegate = ItemCacheCallbackHookFunc;
            // get PTR for our c# function
            var addrToDetour = Marshal.GetFunctionPointerForDelegate(_itemCacheCallbackDelegate);
            // Alloc space for the ASM part of our detour
            string[] asmCode =
            {
                //"pushfd",
                //"pushad",
                "call " + (uint) addrToDetour,
                //"popad",
                //"popfd",
                //"call 0x4FB0F0",
                "retn 0x8"
            };
            // Inject the asm code which calls our c# function
            ItemCallbackPtr = Memory.InjectAsm(asmCode, "ItemCacheCallbackDetour");
        }

        internal event CacheCallbackEventHandler OnNewItemCacheCallback;
        internal event CacheCallbackEventHandler OnNewQuestCacheCallback;

        private void OnNewItemCallbackEvent(int parItemId)
        {
            OnNewItemCacheCallback?.Invoke(parItemId);
        }

        private void OnNewQuestCallbackEvent(int questsId)
        {
            OnNewQuestCacheCallback?.Invoke(questsId);
        }

        private void ItemCacheCallbackHookFunc(int itemId)
        {
            OnNewItemCallbackEvent(itemId);
        }

        private void QuestCacheCallbackHookFunc(int questId)
        {
            OnNewQuestCallbackEvent(questId);
        }

        internal delegate void CacheCallbackEventHandler(int id);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void CacheCallbackDelegate(int parItemId);
    }
}
