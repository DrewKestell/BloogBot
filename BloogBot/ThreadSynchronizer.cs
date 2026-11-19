using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BloogBot
{
    static public class ThreadSynchronizer
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        static extern int CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr handle, out uint processId);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int SendMessage(
            int hWnd,
            uint Msg,
            int wParam,
            int lParam
        );

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentProcessId();

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        delegate int WindowProc(IntPtr hWnd, int Msg, int wParam, int lParam);

        sealed class WorkItem
        {
            readonly Action action;
            volatile bool done;

            public WorkItem(Action action)
            {
                this.action = action;
            }

            public void Execute()
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                }
                finally
                {
                    lock (this)
                    {
                        done = true;
                        Monitor.Pulse(this);
                    }
                }
            }

            public void Wait()
            {
                if (done)
                {
                    return;
                }

                lock (this)
                {
                    while (!done)
                    {
                        Monitor.Wait(this);
                    }
                }
            }
        }

        static readonly object queueLock = new object();
        static readonly Queue<WorkItem> actionQueue = new Queue<WorkItem>();

        const int GWL_WNDPROC = -4;
        const int WM_USER = 0x0400;
        static readonly IntPtr oldCallback;
        static readonly WindowProc newCallback;
        static int windowHandle;
        static uint mainThreadId;
        static readonly uint currentProcessId;

        static ThreadSynchronizer()
        {
            currentProcessId = GetCurrentProcessId();
            EnumWindows(FindWindowProc, IntPtr.Zero);

            if (windowHandle == 0 || mainThreadId == 0)
            {
                throw new InvalidOperationException("Failed to find World of Warcraft window and main thread ID.");
            }

            newCallback = WndProc;
            oldCallback = SetWindowLong((IntPtr)windowHandle, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(newCallback));
        }

        static public void RunOnMainThread(Action action)
        {
            if (GetCurrentThreadId() == mainThreadId)
            {
                action();
                return;
            }

            var item = new WorkItem(action);
            lock (queueLock)
            {
                actionQueue.Enqueue(item);
            }
            SendUserMessage();
            item.Wait();
        }

        static public T RunOnMainThread<T>(Func<T> function)
        {
            if (GetCurrentThreadId() == mainThreadId)
            {
                return function();
            }

            T result = default(T);
            RunOnMainThread(() =>
            {
                result = function();
            });
            return result;
        }

        static int WndProc(IntPtr hWnd, int msg, int wParam, int lParam)
        {
            if (msg != WM_USER)
            {
                return CallWindowProc(oldCallback, hWnd, msg, wParam, lParam);
            }

            while (true)
            {
                WorkItem item;
                lock (queueLock)
                {
                    if (actionQueue.Count == 0)
                    {
                        break;
                    }
                    item = actionQueue.Dequeue();
                }
                item.Execute();
            }
            return 0;
        }

        static bool FindWindowProc(IntPtr hWnd, IntPtr lParam)
        {
            var threadId = GetWindowThreadProcessId(hWnd, out uint procId);
            if (procId != currentProcessId)
            {
                return true;
            }
            if (!IsWindowVisible(hWnd))
            {
                return true;
            }
            var l = GetWindowTextLength(hWnd);
            if (l == 0)
            {
                return true;
            }
            var builder = new StringBuilder(l + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            if (builder.ToString() == "World of Warcraft")
            {
                windowHandle = (int)hWnd;
                mainThreadId = threadId;
                return false;
            }
            return true;
        }

        static void SendUserMessage() => SendMessage(windowHandle, WM_USER, 0, 0);
    }
}
