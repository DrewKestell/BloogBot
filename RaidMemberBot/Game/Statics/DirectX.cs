using RaidMemberBot.Constants;
using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Matrix = SharpDX.Matrix;
using Vector2 = SharpDX.Vector2;
using Vector3 = SharpDX.Vector3;

namespace RaidMemberBot.Game.Statics
{
    /// <summary>
    /// Struct with camera details
    /// </summary>
    public struct Camera
    {
        public float AspectRatio { get; set; }
        public float FieldOfView { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }
        public Location CameraLocation { get; set; }
        public Matrix ProjectionMatrix { get; set; }
        public Matrix Matrix { get; set; }
        public Matrix ViewMatrix { get; set; }
        public Viewport Viewport { get; set; }
    }

    /// <summary>
    /// DirectX related stuff
    /// </summary>
    public class DirectX
    {
        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();
        private readonly int _mtId;
        private int _lastFrameTick;
        private int _timeBetweenFrame;
        private int _waitTilNextFrame;
        private Device _device;
        private IntPtr _lastDeviceptr;

        private Direct3D9EndScene _endSceneOriginal;
        private Direct3D9EndScene _endSceneDetour;

        private DirectX()
        {
            IntPtr vTable = GetEndScene.Instance.ToVTablePointer();
            _endSceneOriginal = Memory.Reader.RegisterDelegate<Direct3D9EndScene>(vTable.ReadAs<IntPtr>());
            _endSceneDetour = new Direct3D9EndScene(EndSceneHook);

            IntPtr addrToDetour = Marshal.GetFunctionPointerForDelegate(_endSceneDetour);

            _mtId = Process.GetCurrentProcess().Threads[0].Id;

            Hack directXvTableHook = new Hack(vTable, BitConverter.GetBytes((int)addrToDetour), "DirectXVTableHook");
            directXvTableHook.Apply();
        }

        /// <summary>
        /// Instance
        /// </summary>
        public static DirectX Instance { get; } = new DirectX();

        internal async void Execute(Action parAction, int inMs = 0)
        {
            if (inMs != 0)
                await Task.Delay(inMs);
            _actionQueue.Enqueue(parAction);
        }

        private int EndSceneHook(IntPtr parDevice)
        {
            if (WinImports.GetCurrentThreadId() != _mtId)
                return _endSceneOriginal(parDevice);
            if (_lastDeviceptr != parDevice)
            {
                _lastDeviceptr = parDevice;
                _device = new Device(parDevice);
            }
            OnEndSceneExecution?.Invoke(_lastDeviceptr);

            Action tmpAction;
            if (_actionQueue.TryDequeue(out tmpAction))
                tmpAction.Invoke();
            try
            {
                if (_lastFrameTick != 0)
                {
                    _timeBetweenFrame = Environment.TickCount - _lastFrameTick;
                    if (_timeBetweenFrame < 15)
                    {
                        int newCount = Environment.TickCount;
                        _waitTilNextFrame = 15 - _timeBetweenFrame;
                        newCount += _waitTilNextFrame;
                        while (Environment.TickCount < newCount)
                        {
                        }
                    }
                }
                _lastFrameTick = Environment.TickCount;
            }
            catch
            {
                // ignored
            }
            return _endSceneOriginal(parDevice);
        }

        internal RawViewport Viewport => _device.Viewport;

        /// <summary>
        ///     EndScene delegate
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int Direct3D9EndScene(IntPtr device);

        /// <summary>
        /// EndScene delegate
        /// </summary>
        /// <param name="devicePtr">Pointer to the device - Can be used to draw in WoW</param>
        /// <returns></returns>
        public delegate void EndSceneDelegate(IntPtr devicePtr);

        /// <summary>
        /// Will be fired whenever EndScene is executed
        /// </summary>
        public event EndSceneDelegate OnEndSceneExecution;

        /// <summary>
        /// Retrieve camera infos
        /// </summary>
        /// <returns></returns>
        public Camera GetCurrentCamera()
        {
            IntPtr basePtr = (Memory.Reader.ImageBase + 0x0074B2BC).ReadAs<IntPtr>().Add(0x000065B8).ReadAs<IntPtr>();
            float aspectRatio = basePtr.Add(0x44).ReadAs<float>();
            float fov = basePtr.Add(0x40).ReadAs<float>();
            float nearClip = basePtr.Add(0x38).ReadAs<float>();
            float farClip = basePtr.Add(0x3C).ReadAs<float>();
            _XYZ xyz = basePtr.Add(0x8).ReadAs<_XYZ>();
            Vector3 eye = new Vector3(xyz.X, xyz.Y, xyz.Z);

            IntPtr mat = basePtr.Add(0x14);
            Matrix matrix = new Matrix()
            {
                M11 = mat.ReadAs<float>(),
                M12 = mat.Add(4).ReadAs<float>(),
                M13 = mat.Add(8).ReadAs<float>(),
                M21 = mat.Add(12).ReadAs<float>(),
                M22 = mat.Add(16).ReadAs<float>(),
                M23 = mat.Add(20).ReadAs<float>(),
                M31 = mat.Add(24).ReadAs<float>(),
                M32 = mat.Add(28).ReadAs<float>(),
                M33 = mat.Add(32).ReadAs<float>(),
            };
            Matrix projectionMatrix = Matrix.PerspectiveFovRH(fov * 0.6f, aspectRatio, nearClip, farClip);
            Vector3 lookAt = new Vector3(eye.X + matrix.M11, eye.Y + matrix.M12, eye.Z + matrix.M13);
            Vector3 up = new Vector3(0, 0, 1);
            RawViewport viewport = Viewport;
            Matrix viewMatrix = Matrix.LookAtRH(eye, lookAt, up);

            return new Camera
            {
                Matrix = matrix,
                Viewport = viewport,
                AspectRatio = aspectRatio,
                CameraLocation = new Location(xyz.X, xyz.Y, xyz.Z),
                FarClip = farClip,
                FieldOfView = fov,
                NearClip = nearClip,
                ProjectionMatrix = projectionMatrix,
                ViewMatrix = viewMatrix
            };
        }

        /// <summary>
        /// Transforms the given world coordinates to screen coordinates
        /// </summary>
        /// <param name="worldLocation"></param>
        /// <returns></returns>
        public Vector2 World2Screen(Location worldLocation)
        {
            Matrix worldMatrix = Matrix.Identity;
            Camera cam = GetCurrentCamera();
            Vector3 pos = new Vector3(worldLocation.X, worldLocation.Y, worldLocation.Z);
            Vector3 projected = Vector3.Project(pos, 0f, 0f, cam.Viewport.Width, cam.Viewport.Height, cam.NearClip, cam.FarClip, worldMatrix * cam.ViewMatrix * cam.ProjectionMatrix);
            return new Vector2(projected.X, projected.Y);
        }
    }
}
