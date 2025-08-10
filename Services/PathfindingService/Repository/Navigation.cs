using GameData.Core.Models;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PathfindingService.Repository
{
    public unsafe class Navigation : IDisposable
    {
        // ============ STRUCTS ============
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PhysicsInput
        {
            // Position and orientation
            public float x, y, z;
            public float orientation;
            public float pitch;

            // Velocity
            public float vx, vy, vz;

            // Movement inputs
            public float moveForward;
            public float moveStrafe;
            public float turnRate;

            // Movement speeds
            public float walkSpeed;
            public float runSpeed;
            public float swimSpeed;
            public float flightSpeed;
            public float backSpeed;

            // State
            public uint moveFlags;
            public uint mapId;

            // Physics modifiers
            public float jumpVelocity;
            public float knockbackVx;
            public float knockbackVy;
            public float knockbackVz;

            // Collision
            public float height;
            public float radius;

            // Spline
            [MarshalAs(UnmanagedType.I1)]
            public bool hasSplinePath;
            public float splineSpeed;
            public IntPtr splinePoints;
            public int splinePointCount;
            public int currentSplineIndex;

            // Time
            public float deltaTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PhysicsOutput
        {
            public float x, y, z;
            public float orientation;
            public float pitch;
            public float vx, vy, vz;
            public uint moveFlags;

            [MarshalAs(UnmanagedType.I1)]
            public bool isGrounded;
            [MarshalAs(UnmanagedType.I1)]
            public bool isSwimming;
            [MarshalAs(UnmanagedType.I1)]
            public bool isFlying;
            [MarshalAs(UnmanagedType.I1)]
            public bool collided;
            public float groundZ;
            public float liquidZ;

            public float fallDistance;
            public float fallTime;

            public int currentSplineIndex;
            public float splineProgress;
        }

        // ============ DELEGATES ============
        // Navigation
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate XYZ* CalculatePathDelegate(uint mapId, XYZ start, XYZ end, bool straightPath, out int length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreePathArrDelegate(XYZ* pathArr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CapsuleOverlapDelegate(uint mapId, ref XYZ position, float radius, float height, out int count);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreeNavPolyArrDelegate(IntPtr ptr);

        // Physics
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate PhysicsOutput StepPhysicsDelegate(ref PhysicsInput input, float dt);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool IsGroundedDelegate(uint mapId, float x, float y, float z, float radius, float height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool IsInWaterDelegate(uint mapId, float x, float y, float z, float height);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float GetGroundHeightDelegate(uint mapId, float x, float y, float z, float searchDist);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate float GetFallDamageDelegate(float fallDistance, bool hasSafeFall);

        // Line of Sight
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool LineOfSightDelegate(uint mapId, ref XYZ from, ref XYZ to);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool VMapLineOfSightDelegate(uint mapId, float x1, float y1, float z1, float x2, float y2, float z2, bool ignoreM2);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool GetCollisionPointDelegate(uint mapId, float x1, float y1, float z1,
                                                        float x2, float y2, float z2,
                                                        out float hitX, out float hitY, out float hitZ);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void GetAreaInfoDelegate(uint mapId, float x, float y, ref float z,
                                                  out uint flags, out int adtId, out int rootId, out int groupId);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool GetLiquidLevelDelegate(uint mapId, float x, float y, float z,
                                                     out float liquidLevel, out float liquidFloor);

        // Utility
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PreloadMapDelegate(uint mapId);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool IsInitializedDelegate();

        // ============ FIELDS ============
        private readonly IntPtr moduleHandle;

        // Navigation
        private readonly CalculatePathDelegate calculatePath;
        private readonly FreePathArrDelegate freePathArr;
        private readonly CapsuleOverlapDelegate capsuleOverlap;
        private readonly FreeNavPolyArrDelegate freeNavPolyArr;

        // Physics
        private readonly StepPhysicsDelegate stepPhysics;
        private readonly IsGroundedDelegate isGrounded;
        private readonly IsInWaterDelegate isInWater;
        private readonly GetGroundHeightDelegate getGroundHeight;
        private readonly GetFallDamageDelegate getFallDamage;

        // Line of Sight
        private readonly LineOfSightDelegate lineOfSight;
        private readonly VMapLineOfSightDelegate vmapLineOfSight;
        private readonly GetCollisionPointDelegate getCollisionPoint;
        private readonly GetAreaInfoDelegate getAreaInfo;
        private readonly GetLiquidLevelDelegate getLiquidLevel;

        // Utility
        private readonly PreloadMapDelegate preloadMap;
        private readonly IsInitializedDelegate isInitialized;

        private bool disposed;

        // ============ CONSTRUCTOR ============
        public Navigation()
        {
            var dllPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "Navigation.dll");

            moduleHandle = LoadLibrary(dllPath);
            if (moduleHandle == IntPtr.Zero)
                throw new FileNotFoundException("Failed to load Navigation.dll", dllPath);

            T GetDelegate<T>(string name, bool required = true) where T : Delegate
            {
                var ptr = GetProcAddress(moduleHandle, name);
                if (ptr == IntPtr.Zero && required)
                    throw new EntryPointNotFoundException($"Failed to find {name}");
                return ptr != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer<T>(ptr) : null;
            }

            // Navigation
            calculatePath = GetDelegate<CalculatePathDelegate>("CalculatePath");
            freePathArr = GetDelegate<FreePathArrDelegate>("FreePathArr");
            capsuleOverlap = GetDelegate<CapsuleOverlapDelegate>("CapsuleOverlap", false);
            freeNavPolyArr = GetDelegate<FreeNavPolyArrDelegate>("FreeNavPolyArr", false);

            // Physics
            stepPhysics = GetDelegate<StepPhysicsDelegate>("StepPhysics");
            isGrounded = GetDelegate<IsGroundedDelegate>("IsGrounded");
            isInWater = GetDelegate<IsInWaterDelegate>("IsInWater");
            getGroundHeight = GetDelegate<GetGroundHeightDelegate>("GetGroundHeight");
            getFallDamage = GetDelegate<GetFallDamageDelegate>("GetFallDamage");

            // Line of Sight
            lineOfSight = GetDelegate<LineOfSightDelegate>("LineOfSight");
            vmapLineOfSight = GetDelegate<VMapLineOfSightDelegate>("VMapLineOfSight", false);
            getCollisionPoint = GetDelegate<GetCollisionPointDelegate>("GetCollisionPoint");
            getAreaInfo = GetDelegate<GetAreaInfoDelegate>("GetAreaInfo", false);
            getLiquidLevel = GetDelegate<GetLiquidLevelDelegate>("GetLiquidLevel", false);

            // Utility
            preloadMap = GetDelegate<PreloadMapDelegate>("PreloadMap");
            isInitialized = GetDelegate<IsInitializedDelegate>("IsInitialized");
        }

        // ============ NAVIGATION METHODS ============
        public unsafe XYZ[] CalculatePath(uint mapId, XYZ start, XYZ end, bool straightPath = false)
        {
            var ptr = calculatePath(mapId, start, end, straightPath, out int len);
            var path = new XYZ[len];
            for (int i = 0; i < len; ++i)
                path[i] = ptr[i];
            freePathArr(ptr);
            return path;
        }

        public NavPoly[] GetCapsuleOverlap(uint mapId, XYZ position, float radius, float height)
        {
            if (capsuleOverlap == null || freeNavPolyArr == null)
                return Array.Empty<NavPoly>();

            var ptr = capsuleOverlap(mapId, ref position, radius, height, out int count);
            if (ptr == IntPtr.Zero || count == 0)
                return Array.Empty<NavPoly>();

            var polys = new NavPoly[count];
            var size = Marshal.SizeOf<NavPoly>();
            for (int i = 0; i < count; i++)
            {
                polys[i] = Marshal.PtrToStructure<NavPoly>(ptr + i * size);
            }
            freeNavPolyArr(ptr);
            return polys;
        }

        // ============ PHYSICS METHODS ============
        public PhysicsOutput StepPhysics(PhysicsInput input, float deltaTime)
        {
            PreloadMap(input.mapId);
            return stepPhysics(ref input, deltaTime);
        }

        public bool IsGrounded(uint mapId, float x, float y, float z, float radius = 0.3f, float height = 2.0f)
        {
            return isGrounded(mapId, x, y, z, radius, height);
        }

        public bool IsInWater(uint mapId, float x, float y, float z, float height = 2.0f)
        {
            return isInWater(mapId, x, y, z, height);
        }

        public float GetGroundHeight(uint mapId, float x, float y, float z, float searchDistance = 4.0f)
        {
            PreloadMap(mapId);
            return getGroundHeight(mapId, x, y, z, searchDistance);
        }

        public float GetFallDamage(float fallDistance, bool hasSafeFall = false)
        {
            return getFallDamage(fallDistance, hasSafeFall);
        }

        // ============ LINE OF SIGHT METHODS ============
        public bool LineOfSight(uint mapId, XYZ from, XYZ to)
        {
            return lineOfSight(mapId, ref from, ref to);
        }

        public bool VMapLineOfSight(uint mapId, float x1, float y1, float z1, float x2, float y2, float z2, bool ignoreM2 = false)
        {
            if (vmapLineOfSight != null)
                return vmapLineOfSight(mapId, x1, y1, z1, x2, y2, z2, ignoreM2);
            return true;
        }

        public bool GetCollisionPoint(uint mapId, float x1, float y1, float z1,
                                      float x2, float y2, float z2,
                                      out float hitX, out float hitY, out float hitZ)
        {
            PreloadMap(mapId);
            return getCollisionPoint(mapId, x1, y1, z1, x2, y2, z2, out hitX, out hitY, out hitZ);
        }

        public void GetAreaInfo(uint mapId, float x, float y, ref float z,
                               out uint flags, out int adtId, out int rootId, out int groupId)
        {
            if (getAreaInfo != null)
            {
                getAreaInfo(mapId, x, y, ref z, out flags, out adtId, out rootId, out groupId);
            }
            else
            {
                flags = 0;
                adtId = -1;
                rootId = -1;
                groupId = -1;
            }
        }

        public bool GetLiquidLevel(uint mapId, float x, float y, float z,
                                   out float liquidLevel, out float liquidFloor)
        {
            if (getLiquidLevel != null)
                return getLiquidLevel(mapId, x, y, z, out liquidLevel, out liquidFloor);

            liquidLevel = -100000.0f;
            liquidFloor = -100000.0f;
            return false;
        }

        // ============ UTILITIES ============
        public void PreloadMap(uint mapId)
        {
            preloadMap(mapId);
        }

        public bool IsReady()
        {
            return isInitialized();
        }

        // ============ HELPER METHODS ============
        public static PhysicsInput CreateInput(float x, float y, float z, float orientation, uint mapId)
        {
            return new PhysicsInput
            {
                x = x,
                y = y,
                z = z,
                orientation = orientation,
                pitch = 0,
                vx = 0,
                vy = 0,
                vz = 0,
                moveForward = 0,
                moveStrafe = 0,
                turnRate = 0,
                walkSpeed = 2.5f,
                runSpeed = 7.0f,
                swimSpeed = 4.72f,
                flightSpeed = 7.0f,
                backSpeed = 4.5f,
                moveFlags = 0,
                mapId = mapId,
                jumpVelocity = 0,
                knockbackVx = 0,
                knockbackVy = 0,
                knockbackVz = 0,
                height = 2.0f,
                radius = 0.3f,
                hasSplinePath = false,
                splineSpeed = 0,
                splinePoints = IntPtr.Zero,
                splinePointCount = 0,
                currentSplineIndex = 0,
                deltaTime = 0.016f
            };
        }

        // ============ CLEANUP ============
        public void Dispose()
        {
            if (!disposed)
            {
                if (moduleHandle != IntPtr.Zero)
                    FreeLibrary(moduleHandle);
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~Navigation() => Dispose();

        // ============ P/INVOKE ============
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);
    }
}