using GameData.Core.Constants;
using GameData.Core.Models;
using System;
using System.Runtime.InteropServices;

namespace PathfindingService.Repository
{
    public class Navigation
    {
        private const string DLL_NAME = "Navigation.dll";

        // ===============================
        // ESSENTIAL IMPORTS ONLY
        // ===============================

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void PreloadMap(uint mapId);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr FindPath(uint mapId, XYZ start, XYZ end, bool smoothPath, out int length);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void PathArrFree(IntPtr pathArr);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern PhysicsOutput PhysicsStep(ref PhysicsInput input);

        // ===============================
        // PUBLIC METHODS
        // ===============================
        static Navigation()
        {
            PreloadMap(0);
            PreloadMap(1);
        }

        public XYZ[] CalculatePath(uint mapId, XYZ start, XYZ end, bool smoothPath)
        {
            IntPtr pathPtr = FindPath(mapId, start, end, smoothPath, out int length);

            if (pathPtr == IntPtr.Zero || length == 0)
                return Array.Empty<XYZ>();

            try
            {
                XYZ[] path = new XYZ[length];
                for (int i = 0; i < length; i++)
                {
                    IntPtr currentPtr = IntPtr.Add(pathPtr, i * Marshal.SizeOf<XYZ>());
                    path[i] = Marshal.PtrToStructure<XYZ>(currentPtr);
                }
                return path;
            }
            finally
            {
                PathArrFree(pathPtr);
            }
        }

        public PhysicsOutput StepPhysics(PhysicsInput input, float deltaTime)
        {
            input.deltaTime = deltaTime;
            return PhysicsStep(ref input);
        }

        // For backwards compatibility - maps to CalculatePath
        public bool LineOfSight(uint mapId, XYZ from, XYZ to)
        {
            // Simple check: if path is straight line, we have LOS
            var path = CalculatePath(mapId, from, to, false);
            return path.Length == 2; // Only start and end points means direct path
        }

        // For backwards compatibility - use physics system
        public float GetGroundHeight(uint mapId, float x, float y, float z, float maxSearchDist = 50.0f)
        {
            var input = new PhysicsInput
            {
                mapId = mapId,
                x = x,
                y = y,
                z = z,
                deltaTime = 0.0f
            };

            var output = StepPhysics(input, 0.0f);
            return output.groundZ;
        }
    }

    // ===============================
    // DATA STRUCTURES
    // ===============================

    [StructLayout(LayoutKind.Sequential)]
    public struct PhysicsInput
    {
        public uint moveFlags;
        public float x, y, z;
        public float orientation;
        public float pitch;
        public float vx, vy, vz;
        public float walkSpeed;
        public float runSpeed;
        public float runBackSpeed;
        public float swimSpeed;
        public float swimBackSpeed;
        public float flightSpeed;
        public float turnSpeed;
        public ulong transportGuid;
        public float transportX, transportY, transportZ, transportO;
        public uint fallTime;
        public float height;
        public float radius;
        public bool hasSplinePath;
        public float splineSpeed;
        public IntPtr splinePoints;
        public int splinePointCount;
        public int currentSplineIndex;
        public uint mapId;
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
        public bool isGrounded;
        public bool isSwimming;
        public bool isFlying;
        public bool collided;
        public float groundZ;
        public float liquidZ;
        public float fallDistance;
        public float fallTime;
        public int currentSplineIndex;
        public float splineProgress;
    }
}