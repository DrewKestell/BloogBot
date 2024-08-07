using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ActivityForegroundMember.Mem
{
    static public class ThreadSynchronizer
    {
        [DllImport("user32.dll")]
        private static extern nint SetWindowLong(nint hWnd, int nIndex, nint dwNewLong);

        [DllImport("user32.dll")]
        private static extern int CallWindowProc(nint lpPrevWndFunc, nint hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(nint handle, out int processId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(nint hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(nint hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

        [DllImport("user32.dll")]
        private static extern int SendMessage(
            int hWnd,
            uint Msg,
            int wParam,
            int lParam
        );

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

        private delegate int WindowProc(nint hWnd, int Msg, int wParam, int lParam);

        private static readonly Queue<Action> actionQueue = new();
        private static readonly Queue<Delegate> delegateQueue = new();
        private static readonly Queue<object> returnValueQueue = new();
        private const int GWL_WNDPROC = -4;
        private const int WM_USER = 0x0400;
        private static readonly nint oldCallback;
        private static readonly WindowProc newCallback;
        private static int windowHandle;

        static ThreadSynchronizer()
        {
            EnumWindows(FindWindowProc, nint.Zero);
            newCallback = WndProc;
            oldCallback = SetWindowLong(windowHandle, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(newCallback));
        }

        static public void RunOnMainThread(Action action)
        {
            if (GetCurrentThreadId() == Process.GetCurrentProcess().Threads[0].Id)
            {
                action();
                return;
            }
            actionQueue.Enqueue(action);
            SendUserMessage();
        }

        static public T RunOnMainThread<T>(Func<T> function)
        {
            if (GetCurrentThreadId() == Process.GetCurrentProcess().Threads[0].Id)
                return function();

            delegateQueue.Enqueue(function);
            SendUserMessage();
            return (T)returnValueQueue.Dequeue();
        }

        private static int WndProc(nint hWnd, int msg, int wParam, int lParam)
        {
            try
            {
                if (msg != WM_USER) return CallWindowProc(oldCallback, hWnd, msg, wParam, lParam);

                while (actionQueue.Count > 0)
                    actionQueue.Dequeue()?.Invoke();
                while (delegateQueue.Count > 0)
                {
                    var invokeTarget = delegateQueue.Dequeue();
                    returnValueQueue.Enqueue(invokeTarget?.DynamicInvoke());
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[THREAD]{e.Message} {e.StackTrace}");
            }

            return CallWindowProc(oldCallback, hWnd, msg, wParam, lParam);
        }

        private static bool FindWindowProc(nint hWnd, nint lParam)
        {
            GetWindowThreadProcessId(hWnd, out int procId);
            if (procId != Environment.ProcessId) return true;
            if (!IsWindowVisible(hWnd)) return true;
            var l = GetWindowTextLength(hWnd);
            if (l == 0) return true;
            var builder = new StringBuilder(l + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            if (builder.ToString() == "World of Warcraft")
                windowHandle = (int)hWnd;
            return true;
        }

        private static void SendUserMessage() => SendMessage(windowHandle, WM_USER, 0, 0);
    }
}
