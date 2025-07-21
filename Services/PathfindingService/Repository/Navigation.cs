using GameData.Core.Models;
using System.Reflection;
using System.Runtime.InteropServices;
using Position = GameData.Core.Models.Position;

namespace PathfindingService.Repository
{
    public unsafe class Navigation
    {
        /* ─────────────── Structs ─────────────── */

        [StructLayout(LayoutKind.Sequential)]
        public struct NavPoly
        {
            public ulong RefId;
            public uint Area;
            public uint Flags;
            public uint VertCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public XYZ[] Verts;
        }
        // Newly added for physics bridge:
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PhysicsInput
        {
            // MovementInfoUpdate core
            public uint movementFlags;

            // Position & orientation
            public float posX;
            public float posY;
            public float posZ;
            public float facing;

            // Transport
            public ulong transportGuid;
            public float transportOffsetX;
            public float transportOffsetY;
            public float transportOffsetZ;
            public float transportOrientation;

            // Swimming
            public float swimPitch;

            // Falling / jumping
            public uint fallTime;
            public float jumpVerticalSpeed;
            public float jumpCosAngle;
            public float jumpSinAngle;
            public float jumpHorizontalSpeed;

            // Spline elevation
            public float splineElevation;

            // MovementBlockUpdate speeds
            public float walkSpeed;
            public float runSpeed;
            public float runBackSpeed;
            public float swimSpeed;
            public float swimBackSpeed;

            // Current velocity
            public float velX;
            public float velY;
            public float velZ;

            // Collision & world
            public float radius;
            public float height;
            public float gravity;

            // Terrain fallbacks
            public float adtGroundZ;
            public float adtLiquidZ;

            // Context
            public uint mapId;
        }

        // PhysicsOutput.cs
        [StructLayout(LayoutKind.Sequential)]
        public struct PhysicsOutput
        {
            public float newPosX, newPosY, newPosZ;
            public float newVelX, newVelY, newVelZ;
            public uint movementFlags;
        }

        /* ─────────────── Native delegates ─────────────── */

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XYZ* CalculatePathDelegate(uint mapId, XYZ start, XYZ end,
                                                    bool straightPath, out int length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreePathArrDelegate(XYZ* pathArr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool LineOfSightDelegate(uint mapId, XYZ from, XYZ to);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CapsuleOverlapDelegate(uint mapId, XYZ position,
                                                       float radius, float height, out int count);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreeNavPolyArrDelegate(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PhysicsOutput StepPhysicsDelegate(ref PhysicsInput input, float dt);

        /* ─────────────── Function pointers ─────────────── */

        private readonly CalculatePathDelegate calculatePath;
        private readonly FreePathArrDelegate freePathArr;
        private readonly LineOfSightDelegate lineOfSight;
        private readonly CapsuleOverlapDelegate capsuleOverlap;
        private readonly FreeNavPolyArrDelegate freeNavPolyArr;
        private readonly StepPhysicsDelegate stepPhysics;

        /* ─────────────── Constructor: bind all exports ─────────────── */

        private readonly AdtGroundZLoader _adtGroundZLoader;

        public Navigation()
        {
            var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var dllPath = Path.Combine(binFolder, "Navigation.dll");
            var mod = WinProcessImports.LoadLibrary(dllPath);

            if (mod == IntPtr.Zero)
                throw new FileNotFoundException("Failed to load Navigation.dll", dllPath);

            calculatePath = Marshal.GetDelegateForFunctionPointer<CalculatePathDelegate>(
                WinProcessImports.GetProcAddress(mod, "CalculatePath"));
            freePathArr = Marshal.GetDelegateForFunctionPointer<FreePathArrDelegate>(
                WinProcessImports.GetProcAddress(mod, "FreePathArr"));
            lineOfSight = Marshal.GetDelegateForFunctionPointer<LineOfSightDelegate>(
                WinProcessImports.GetProcAddress(mod, "LineOfSight"));
            capsuleOverlap = Marshal.GetDelegateForFunctionPointer<CapsuleOverlapDelegate>(
                WinProcessImports.GetProcAddress(mod, "CapsuleOverlap"));
            freeNavPolyArr = Marshal.GetDelegateForFunctionPointer<FreeNavPolyArrDelegate>(
                WinProcessImports.GetProcAddress(mod, "FreeNavPolyArr"));
            stepPhysics = Marshal.GetDelegateForFunctionPointer<StepPhysicsDelegate>(
                WinProcessImports.GetProcAddress(mod, "StepPhysics"));

            _adtGroundZLoader = new AdtGroundZLoader([Path.Combine(binFolder, @"Data\terrain.MPQ")]);
        }

        public Position[] CalculatePath(uint mapId, Position start, Position end, bool straightPath)
        {
            var ptr = calculatePath(mapId, start.ToXYZ(), end.ToXYZ(), straightPath, out int len);
            var path = new Position[len];
            for (int i = 0; i < len; ++i)
                path[i] = new Position(ptr[i]);
            freePathArr(ptr);
            return path;
        }

        public bool IsLineOfSight(uint mapId, Position from, Position to)
        {
            return lineOfSight(mapId, from.ToXYZ(), to.ToXYZ());
        }

        public PhysicsOutput StepPhysics(PhysicsInput input,
            float dt)
        {
            _adtGroundZLoader.TryGetZ((int)input.mapId, input.posX, input.posY, out float adtGroundZ, out float adtLiquidZ);

            input.adtGroundZ = adtGroundZ;
            input.adtLiquidZ = adtLiquidZ;

            return stepPhysics(ref input, dt);
        }
    }
}
