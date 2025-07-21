// PhysicsEngine.h
#pragma once
#include "PhysicsBridge.h"
#include "MoveMap.h"                 // for MMAP::MMapFactory & MMapManager
#include <DetourNavMeshQuery.h>      // from Detour
#include <unordered_map>

class PhysicsEngine
{
public:
    static PhysicsEngine* Instance();

    /// Must be called once on DLL load (you can preload all maps here).
    bool Initialize();

    /// Main step function: applies one frame of physics and returns updated state.
    PhysicsOutput Step(const PhysicsInput& in, float dt);

private:
    static PhysicsEngine* s_singletonInstance;
    PhysicsEngine() = default;
    ~PhysicsEngine();

    // Non-copyable
    PhysicsEngine(const PhysicsEngine&) = delete;
    PhysicsEngine& operator=(const PhysicsEngine&) = delete;

    struct MapContext
    {
        const dtNavMeshQuery* query;
    };

    std::unordered_map<uint32_t, MapContext> _maps;
    MapContext* GetOrLoadMap(uint32_t mapId);
};
