// DllMain.cpp - Fixed Navigation DLL Entry Point
#include "Navigation.h"
#include "VMapClient.h"
#include "VMapFactory.h"
#include "PhysicsEngine.h"
#include "PhysicsBridge.h"
#include "MapLoader.h"

#define NOMINMAX
#include <windows.h>
#include <iostream>
#include <memory>
#include <mutex>
#include <filesystem>
#include <vector>
#include <exception>

// Global instances
static std::unique_ptr<VMapClient> g_vmapClient = nullptr;
static std::unique_ptr<MapLoader> g_mapLoader = nullptr;
static bool g_initialized = false;
static std::mutex g_initMutex;

void InitializeAllSystems()
{
    std::lock_guard<std::mutex> lock(g_initMutex);

    if (g_initialized)
        return;

    try
    {
        std::cout << "[Navigation] Initializing systems..." << std::endl;

        // Initialize MapLoader first (optional, for terrain data)
        try
        {
            g_mapLoader = std::make_unique<MapLoader>();
            std::vector<std::string> mapPaths = { "maps/", "Data/maps/", "../Data/maps/" };

            for (const auto& path : mapPaths)
            {
                if (std::filesystem::exists(path))
                {
                    if (g_mapLoader->Initialize(path))
                    {
                        std::cout << "[Navigation] MapLoader initialized: " << path << std::endl;
                        break;
                    }
                }
            }

            if (!g_mapLoader->IsInitialized())
            {
                std::cout << "[Navigation] Warning: MapLoader not initialized - terrain data unavailable" << std::endl;
            }
        }
        catch (const std::exception& e)
        {
            std::cerr << "[Navigation] MapLoader initialization error: " << e.what() << std::endl;
            // Continue without MapLoader - it's optional
        }

        // Initialize Physics Engine
        try
        {
            if (auto* physics = PhysicsEngine::Instance())
            {
                physics->Initialize();
                physics->SetVMapHeightEnabled(true);
                physics->SetVMapIndoorCheckEnabled(true);
                physics->SetVMapLOSEnabled(true);
                std::cout << "[Navigation] Physics engine initialized" << std::endl;
            }
        }
        catch (const std::bad_alloc& e)
        {
            std::cerr << "[Navigation] Physics engine bad_alloc: " << e.what() << std::endl;
            std::cerr << "[Navigation] Insufficient memory for physics engine" << std::endl;
            // Continue without physics - critical but we can still provide basic navigation
        }
        catch (const std::exception& e)
        {
            std::cerr << "[Navigation] Physics engine error: " << e.what() << std::endl;
        }

        // Initialize Navigation (NavMesh) - this is critical
        try
        {
            if (auto* navigation = Navigation::GetInstance())
            {
                navigation->Initialize();
                std::cout << "[Navigation] Navigation system initialized" << std::endl;
            }
        }
        catch (const std::exception& e)
        {
            std::cerr << "[Navigation] Navigation system error: " << e.what() << std::endl;
        }

        // Initialize VMAP client - handle carefully as this is where bad_alloc often occurs
        try
        {
            // First check if vmaps directory exists
            std::vector<std::string> vmapPaths = { "vmaps/", "Data/vmaps/", "../Data/vmaps/" };
            std::string vmapPath;

            for (const auto& path : vmapPaths)
            {
                if (std::filesystem::exists(path))
                {
                    vmapPath = path;
                    std::cout << "[Navigation] Found VMAP directory: " << path << std::endl;
                    break;
                }
            }

            if (vmapPath.empty())
            {
                std::cout << "[Navigation] Warning: No VMAP directory found - VMAP features disabled" << std::endl;
                std::cout << "[Navigation] To enable VMAP, extract map files to one of these locations:" << std::endl;
                for (const auto& path : vmapPaths)
                {
                    std::cout << "  - " << std::filesystem::absolute(path) << std::endl;
                }
            }
            else
            {
                // Try to create VMapClient with explicit path
                g_vmapClient = std::make_unique<VMapClient>(vmapPath);

                // Initialize in a separate step to catch any errors
                if (g_vmapClient)
                {
                    g_vmapClient->initialize();
                    std::cout << "[Navigation] VMAP client initialized" << std::endl;
                }
            }
        }
        catch (const std::bad_alloc& e)
        {
            std::cerr << "[Navigation] VMAP bad_alloc: " << e.what() << std::endl;
            std::cerr << "[Navigation] Insufficient memory for VMAP - feature disabled" << std::endl;
            g_vmapClient.reset();  // Clear the pointer
        }
        catch (const std::exception& e)
        {
            std::cerr << "[Navigation] VMAP initialization error: " << e.what() << std::endl;
            g_vmapClient.reset();  // Clear the pointer
        }

        g_initialized = true;
        std::cout << "[Navigation] System initialization complete" << std::endl;

        // Report status
        std::cout << "[Navigation] Status:" << std::endl;
        std::cout << "  - Navigation (NavMesh): " << (Navigation::GetInstance() ? "OK" : "FAILED") << std::endl;
        std::cout << "  - Physics Engine: " << (PhysicsEngine::Instance() ? "OK" : "FAILED") << std::endl;
        std::cout << "  - VMAP System: " << (g_vmapClient ? "OK" : "DISABLED") << std::endl;
        std::cout << "  - Map Loader: " << (g_mapLoader && g_mapLoader->IsInitialized() ? "OK" : "DISABLED") << std::endl;
    }
    catch (const std::exception& e)
    {
        std::cerr << "[Navigation] Critical initialization error: " << e.what() << std::endl;
        g_initialized = true; // Mark as initialized to prevent retry
    }
    catch (...)
    {
        std::cerr << "[Navigation] Unknown critical initialization error" << std::endl;
        g_initialized = true; // Mark as initialized to prevent retry
    }
}

// ===============================
// NAVIGATION EXPORTS
// ===============================

extern "C" __declspec(dllexport) XYZ* CalculatePath(unsigned int mapId, XYZ start, XYZ end, bool straightPath, int* length)
{
    if (!g_initialized)
        InitializeAllSystems();

    auto* navigation = Navigation::GetInstance();
    if (navigation)
        return navigation->CalculatePath(mapId, start, end, straightPath, length);

    *length = 0;
    return nullptr;
}

extern "C" __declspec(dllexport) void FreePathArr(XYZ* pathArr)
{
    auto* navigation = Navigation::GetInstance();
    if (navigation)
        navigation->FreePathArr(pathArr);
}

extern "C" __declspec(dllexport) NavPoly* CapsuleOverlap(uint32_t mapId, const XYZ& position,
    float radius, float height, int* count)
{
    if (!g_initialized)
        InitializeAllSystems();

    auto* navigation = Navigation::GetInstance();
    if (!navigation)
    {
        *count = 0;
        return nullptr;
    }

    auto polys = navigation->CapsuleOverlap(mapId, position, radius, height);
    *count = static_cast<int>(polys.size());

    if (*count == 0)
        return nullptr;

    NavPoly* result = new NavPoly[*count];
    for (int i = 0; i < *count; i++)
    {
        result[i].refId = polys[i].refId;
        result[i].area = polys[i].area;
        result[i].flags = polys[i].flags;
        result[i].vertCount = polys[i].vertCount;
        for (uint32_t v = 0; v < polys[i].vertCount && v < 6; v++)
        {
            result[i].verts[v] = polys[i].verts[v];
        }
    }

    return result;
}

extern "C" __declspec(dllexport) void FreeNavPolyArr(NavPoly* ptr)
{
    delete[] ptr;
}

// ===============================
// PHYSICS EXPORTS
// ===============================

extern "C" __declspec(dllexport) PhysicsOutput StepPhysics(const PhysicsInput& input, float deltaTime)
{
    if (!g_initialized)
        InitializeAllSystems();

    if (auto* physics = PhysicsEngine::Instance())
        return physics->Step(input, deltaTime);

    // Return passthrough if physics isn't available
    PhysicsOutput output = {};
    output.x = input.x;
    output.y = input.y;
    output.z = input.z;
    output.orientation = input.orientation;
    output.pitch = input.pitch;
    output.vx = input.vx;
    output.vy = input.vy;
    output.vz = input.vz;
    output.moveFlags = input.moveFlags;
    output.isGrounded = false;
    output.isSwimming = false;
    output.isFlying = (input.moveFlags & MOVEFLAG_FLYING) != 0;
    output.groundZ = -100000.0f;
    output.liquidZ = -100000.0f;
    return output;
}

extern "C" __declspec(dllexport) bool IsGrounded(uint32_t mapId, float x, float y, float z, float radius, float height)
{
    if (!g_initialized)
        InitializeAllSystems();

    if (auto* physics = PhysicsEngine::Instance())
        return physics->IsGrounded(mapId, x, y, z, radius, height);

    return false;
}

extern "C" __declspec(dllexport) bool IsInWater(uint32_t mapId, float x, float y, float z, float height)
{
    if (!g_initialized)
        InitializeAllSystems();

    if (auto* physics = PhysicsEngine::Instance())
        return physics->IsInWater(mapId, x, y, z, height);

    return false;
}

extern "C" __declspec(dllexport) float GetGroundHeight(uint32_t mapId, float x, float y, float z, float searchDist)
{
    if (!g_initialized)
        InitializeAllSystems();

    if (auto* physics = PhysicsEngine::Instance())
        return physics->GetHeight(mapId, x, y, z, true, searchDist);

    return PhysicsConstants::INVALID_HEIGHT_VALUE;
}

extern "C" __declspec(dllexport) float GetFallDamage(float fallDistance, bool hasSafeFall)
{
    if (!g_initialized)
        InitializeAllSystems();

    if (auto* physics = PhysicsEngine::Instance())
        return physics->GetFallDamage(fallDistance, hasSafeFall);

    return 0.0f;
}

// ===============================
// LINE OF SIGHT EXPORTS
// ===============================

extern "C" __declspec(dllexport) bool LineOfSight(uint32_t mapId, const XYZ& start, const XYZ& end)
{
    if (!g_initialized)
        InitializeAllSystems();

    // Check NavMesh LOS
    auto* navigation = Navigation::GetInstance();
    if (navigation)
        return navigation->IsLineOfSight(mapId, start, end);

    return true;  // No collision data = clear line of sight
}

extern "C" __declspec(dllexport) bool VMapLineOfSight(uint32_t mapId,
    float x1, float y1, float z1,
    float x2, float y2, float z2, bool ignoreM2)
{
    if (!g_initialized)
        InitializeAllSystems();

    if (g_vmapClient)
        return g_vmapClient->isLineOfSight(mapId, x1, y1, z1, x2, y2, z2, ignoreM2);

    return true;  // No collision data = clear line of sight
}

extern "C" __declspec(dllexport) bool GetCollisionPoint(uint32_t mapId,
    float x1, float y1, float z1,
    float x2, float y2, float z2,
    float* hitX, float* hitY, float* hitZ)
{
    if (!g_initialized)
        InitializeAllSystems();

    if (g_vmapClient)
        return g_vmapClient->getCollisionPoint(mapId, x1, y1, z1, x2, y2, z2, *hitX, *hitY, *hitZ);

    return false;
}

extern "C" __declspec(dllexport) void GetAreaInfo(uint32_t mapId, float x, float y, float* z,
    uint32_t* flags, int32_t* adtId, int32_t* rootId, int32_t* groupId)
{
    if (!g_initialized)
        InitializeAllSystems();

    if (g_vmapClient)
        g_vmapClient->getAreaInfo(mapId, x, y, *z, *flags, *adtId, *rootId, *groupId);
    else
    {
        *flags = 0;
        *adtId = -1;
        *rootId = -1;
        *groupId = -1;
    }
}

extern "C" __declspec(dllexport) bool GetLiquidLevel(uint32_t mapId, float x, float y, float z,
    float* liquidLevel, float* liquidFloor)
{
    if (!g_initialized)
        InitializeAllSystems();

    if (g_vmapClient)
        return g_vmapClient->getLiquidLevel(mapId, x, y, z, *liquidLevel, *liquidFloor);

    return false;
}

// ===============================
// UTILITY EXPORTS
// ===============================

extern "C" __declspec(dllexport) void PreloadMap(uint32_t mapId)
{
    if (!g_initialized)
        InitializeAllSystems();

    if (g_vmapClient)
    {
        try { g_vmapClient->preloadMap(mapId); }
        catch (...) {}
    }
}

extern "C" __declspec(dllexport) bool IsInitialized()
{
    return g_initialized;
}

// DLL Entry Point
BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    if (ul_reason_for_call == DLL_PROCESS_ATTACH)
    {
        SetConsoleOutputCP(CP_UTF8);
        std::cout << "[Navigation] Navigation.dll loaded" << std::endl;
    }
    else if (ul_reason_for_call == DLL_PROCESS_DETACH)
    {
        // Clean up if needed
        if (lpReserved == nullptr)  // FreeLibrary was called (not process termination)
        {
            g_vmapClient.reset();
            g_mapLoader.reset();
            PhysicsEngine::Destroy();
        }
    }
    return TRUE;
}