using RaidMemberBot.Constants;
using RaidMemberBot.Mem;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RaidMemberBot
{
    public sealed class ThreadSynchronizer
    {
        private const int GWL_WNDPROC = -4;
        private const int WM_USER = 0x0400;
        private readonly WinImports.WindowProc _newCallback;
        private readonly IntPtr _oldCallback;
        private readonly ConcurrentQueue<Action> InvokeQueue = new ConcurrentQueue<Action>();
        private readonly ConcurrentQueue<Delegate> InvokeReturnFunction = new ConcurrentQueue<Delegate>();
        private readonly ConcurrentQueue<object> InvokeReturnValue = new ConcurrentQueue<object>();
        private readonly int mtId;
        private int _hWnd;
        private int _invokeIdentifier = 0;

        private ThreadSynchronizer()
        {
            WinImports.EnumWindows(WindowProc, IntPtr.Zero);
            _newCallback = WndProc; // Pins WndProc - will not be garbage collected.
            _oldCallback = WinImports.SetWindowLong((IntPtr)_hWnd, GWL_WNDPROC,
                Marshal.GetFunctionPointerForDelegate(_newCallback));
            mtId = Process.GetCurrentProcess().Threads[0].Id;
        }

        /// <summary>
        ///     Access to the MainThread instance
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static ThreadSynchronizer Instance { get; } = new ThreadSynchronizer();

        private bool WindowProc(IntPtr hWnd, IntPtr lParam)
        {
            int procId;
            WinImports.GetWindowThreadProcessId(hWnd, out procId);
            if (procId != Memory.Reader.Process.Id) return true;
            if (!WinImports.IsWindowVisible(hWnd)) return true;
            var l = WinImports.GetWindowTextLength(hWnd);
            if (l == 0) return true;
            var builder = new StringBuilder(l + 1);
            WinImports.GetWindowText(hWnd, builder, builder.Capacity);
            if (builder.ToString() == "World of Warcraft")
                _hWnd = (int)hWnd;
            return true;
        }

        private void SendUserMessage(UserMessage parMessage)
        {
            WinImports.SendMessage(_hWnd, WM_USER, (int)parMessage, 0);
        }

        /// <summary>
        ///     Execute the function in the mainthread
        /// </summary>
        /// <param name="parDelegate">The function</param>
        /// <returns>object</returns>
        /// <remarks>
        ///     <code>
        ///  Lua.Instance.Execute("DoEmote('dance')");
        ///  Lua.Instance.Execute("DoEmote('dance')");
        ///  
        ///  MainThread.Instance.Invoke(() =>
        ///  {
        ///      Lua.Instance.Execute("DoEmote('dance')");
        ///      Lua.Instance.Execute("DoEmote('dance')");
        ///  });
        ///  </code>
        ///     <br />
        ///     First two Execute calls will require two invokes while the second two require just one.<br />
        ///     While it doesnt make a difference for two calls it can mean the difference between a second and a ms.<br />
        ///     Whenever you call multiple methods that will invoke the MainThread you are free to use the invoke method
        /// </remarks>
        public T Invoke<T>(Func<T> parDelegate)
        {
            if (WinImports.GetCurrentThreadId() == mtId)
                return parDelegate();
            InvokeReturnFunction.Enqueue(parDelegate);
            SendUserMessage(UserMessage.RunDelegateReturn);
            object ret;
            if (InvokeReturnValue.TryDequeue(out ret))
            {
                return (T)ret;
            }
            return default(T);
        }

        /// <summary>
        ///     Execute the function in the mainthread. See also <see cref="Invoke" />
        /// </summary>
        /// <param name="parDelegate">The function</param>
        public void Invoke(Action parDelegate)
        {
            if (WinImports.GetCurrentThreadId() == mtId)
            {
                parDelegate();
                return;
            }
            InvokeQueue.Enqueue(parDelegate);
            SendUserMessage(UserMessage.RunDelegate);
        }

        private int WndProc(IntPtr hWnd, int Msg, int wParam, int lParam)
        {
            if (Msg != WM_USER) return WinImports.CallWindowProc(_oldCallback, hWnd, Msg, wParam, lParam);
            var tmpMsg = (UserMessage)wParam;
            switch (tmpMsg)
            {
                case UserMessage.RunDelegateReturn:
                    Delegate result;
                    if (InvokeReturnFunction.TryDequeue(out result))
                    {
                        var invokeTarget = result;
                        InvokeReturnValue.Enqueue(invokeTarget.DynamicInvoke());
                    }
                    return 0;

                case UserMessage.RunDelegate:
                    Action action;
                    if (InvokeQueue.TryDequeue(out action))
                    {
                        action.Invoke();
                    }
                    return 0;
            }
            return WinImports.CallWindowProc(_oldCallback, hWnd, Msg, wParam, lParam);
        }

        /// <summary>
        ///     Schedule an action to be called on the mainthread in a specific interval
        /// </summary>
        public sealed class Updater
        {
            private readonly Action action;

            private readonly int runEach;

            /// <summary>
            ///     Initializes a new instance of the <see cref="Updater" /> class.
            /// </summary>
            /// <param name="parAction">The Action</param>
            /// <param name="parRunEach">The Time to pause between each Action invoke</param>
            public Updater(Action parAction, int parRunEach)
            {
                action = parAction;
                runEach = parRunEach;
                Pulse();
            }

            /// <summary>
            ///     Determines whether the updater is running
            /// </summary>
            public bool IsRunning { get; private set; }

            /// <summary>
            ///     Executes the action of the updater in the MainThread
            /// </summary>
            public void Invoke()
            {
                Instance.Invoke(action);
            }

            private async void Pulse()
            {
                while (IsRunning)
                {
                    Instance.Invoke(action);
                    await Task.Delay(runEach);
                }
            }

            /// <summary>
            ///     Starts the updater instance
            /// </summary>
            public void Start()
            {
                if (IsRunning) return;
                IsRunning = true;
                Pulse();
            }

            /// <summary>
            ///     Stops the updater instance
            /// </summary>
            public void Stop()
            {
                IsRunning = false;
            }
        }

        private enum UserMessage
        {
            RunDelegateReturn,
            RunDelegate
        }
    }
}
