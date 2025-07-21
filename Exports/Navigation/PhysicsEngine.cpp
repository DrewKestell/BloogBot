// PhysicsEngine.cpp
#include "PhysicsEngine.h"
#include <iostream>
#include <iomanip>   // std::hex / std::dec
#include <cmath>
#include <DetourCommon.h>
#include "Navigation.h"
#include <filesystem>
#include "VMapManager2.h"

static inline float ClampF(float v, float lo, float hi) { return v < lo ? lo : (v > hi ? hi : v); }

static constexpr float TERMINAL_VELOCITY_YDPS = 60.0f;   // vanilla cap
static constexpr float GROUND_EPS = 0.05f;   // snap distance (yd)
static constexpr float SLOPE_MAX_COS = 0.6f;    // 53.13° walkable

using MF = MovementFlags;
using std::cout;
using std::endl;

PhysicsEngine* PhysicsEngine::s_singletonInstance = NULL;

PhysicsEngine* PhysicsEngine::Instance()
{
    if (s_singletonInstance == NULL)
        s_singletonInstance = new PhysicsEngine();
    return s_singletonInstance;
}

PhysicsEngine::~PhysicsEngine()
{
    // no need to free queries; MMAP manager owns them
}


bool PhysicsEngine::Initialize()
{
    //std::puts("\n[PE] ================================================");
    //std::puts("[PE]  PhysicsEngine::Initialize  (vmaps + mmaps)");
    //std::puts("[PE] ================================================"); std::fflush(stdout);

    //auto& vman = wow::vmap::VMapManager2::instance();
    //std::puts("[PE]  VMapManager instance OK"); std::fflush(stdout);

    //namespace fs = std::filesystem;
    //// FIX: fully-qualify the static call
    //fs::path root = wow::vmap::VMapManager2::GetVmapsRootString();
    //std::printf("[PE]  vmaps root = %s\n", root.string().c_str()); std::fflush(stdout);

    //auto preloadVMap = [&](uint32_t mapId)
    //    {
    //        if (!fs::exists(root))
    //        {
    //            std::puts("[PE]  vmaps directory NOT FOUND!\n");
    //            return;
    //        }

    //        size_t loaded = 0, tested = 0;
    //        try
    //        {
    //            for (int x = 0; x < 64; ++x)
    //                for (int y = 0; y < 64; ++y)
    //                {
    //                    if (tested == 0)
    //                    {
    //                        std::printf("[PE]  map %u first probe (%02d,%02d)\n", mapId, x, y);
    //                        std::fflush(stdout);
    //                    }
    //                    ++tested;
    //                    if (vman.loadMapTile(mapId, x, y))
    //                        ++loaded;
    //                }
    //        }
    //        catch (const std::exception& e)
    //        {
    //            std::printf("[PE]  C++ EXCEPTION in loop: %s\n", e.what());
    //            std::fflush(stdout);
    //            throw;
    //        }
    //        catch (...)  // CORRECTED from `catch (.)` :contentReference[oaicite:2]{index=2}
    //        {
    //            std::puts("[PE]  *** NATIVE SEH EXCEPTION in loop ***");
    //            std::fflush(stdout);
    //            throw;
    //        }

    //        std::printf("[PE]  Map %u  tilesLoaded=%zu / 4096\n", mapId, loaded);
    //        std::fflush(stdout);
    //    };

    //try
    //{
    //    preloadVMap(0);
    //    preloadVMap(1);
    //    preloadVMap(389);
    //}
    //catch (...)  // CORRECTED from `catch (.)` :contentReference[oaicite:3]{index=3}
    //{
    //    std::puts("[PE]  Initialize aborted by exception.\n"); std::fflush(stdout);
    //    return false;
    //}

    // navmesh preload unchanged
    GetOrLoadMap(0);
    GetOrLoadMap(1);
    GetOrLoadMap(389);

    std::puts("[PE]  Initialize complete\n"); std::fflush(stdout);
    return true;
}

PhysicsEngine::MapContext* PhysicsEngine::GetOrLoadMap(uint32_t mapId)
{
    auto it = _maps.find(mapId);
    if (it != _maps.end())
        return &it->second;

    // Ask Navigation for the pre-loaded query:
    const dtNavMeshQuery* q = Navigation::GetInstance()->GetQueryForMap(mapId);
    if (!q)
        return nullptr;

    MapContext ctx;
    ctx.query = q;
    _maps[mapId] = ctx;
    return &_maps[mapId];
}

// ============================================================================
//  PhysicsEngine::Step ‒ vanilla-style movement with wall-slide support
//  • User-input flags (FWD / STRAFE / SPRINT …) are **never** cleared here –
//    the client keeps sending them while the key is held / auto-run is on.
//  • When movement is blocked head-on, horizontal velocity is zeroed but
//    flags remain; when only partially blocked, velocity is the slide vector.
//  • Engine state flags (SWIMMING / FALLING) are managed by the server.
// ============================================================================

// helper: concise hex print
static void LogFlags(const char* tag, uint32_t f)
{
    std::cout << tag << " 0x" << std::hex << f << std::dec << '\n';
}

// ============================================================================
//  PhysicsEngine::Step  – 2025‑07‑19
//  • Yaw (facing) is read‑only; TURN flags are ignored by physics.
//  • Transport: X/Y stay in world space; deck height used as ground.
//  • Wall/ceiling blocking via Navigation::CapsuleOverlapSweep.
//  • Ground‑snap FIX: feet snap to groundZ (no +height bump).
// ============================================================================
// Helper – collision query via Navigation capsule sweep
static bool CapsuleBlocked(uint32_t mapId, const XYZ& a, const XYZ& b, float radius, float height)
{
    if (auto* nav = Navigation::GetInstance())
        for (auto& hit : nav->CapsuleOverlapSweep(mapId, a, b, radius, height, 0.001f))
            if (!(hit.flags & 0x01)) return true;               // any blocking triangle
    return false;
}

// ─────────────────────────────────────────────────────────────────────────────
//  Main step – vanilla-style movement with VMap static collision + navmesh
// ─────────────────────────────────────────────────────────────────────────────
// ---------------------------------------------------------------------------
//  PhysicsEngine::Step  –  VMap-first movement & collision
// ---------------------------------------------------------------------------
PhysicsOutput PhysicsEngine::Step(const PhysicsInput& in, float dt)
{
    using MF = MovementFlags;
    constexpr float  GROUND_EPS = 0.05f;
    constexpr uint32_t FALL_MASK = MF::MOVEFLAG_JUMPING | MF::MOVEFLAG_FALLINGFAR;
    auto& vman = wow::vmap::VMapManager2::instance();

    /* ── 1) clone / sanitise flags ─────────────────────────────────────── */
    uint32_t flags = in.movementFlags;
    const bool onTrans = flags & MF::MOVEFLAG_ONTRANSPORT;
    const bool rooted = flags & MF::MOVEFLAG_ROOT;
    if (onTrans) flags &= ~MF::MOVEFLAG_SWIMMING;

    /* ── 2) basic facing vectors ───────────────────────────────────────── */
    const float cosO = std::cosf(in.facing);
    const float sinO = std::sinf(in.facing);
    const float fwdX = cosO, fwdY = sinO;
    const float rgtX = -sinO, rgtY = cosO;

    /* ── 3) VMap ground probe (primary) ────────────────────────────────── */
    float groundZ = wow::vmap::VMAP_INVALID_HEIGHT;
    bool  vmapOk = false;
    if (!onTrans)
    {
        groundZ = vman.getHeight(in.mapId,
            in.posX, in.posY, in.posZ + 5.f);
        vmapOk = (groundZ != wow::vmap::VMAP_INVALID_HEIGHT);

        std::printf("[Step] map %u @ (%.1f, %.1f)  vmapZ=%s\n",
            in.mapId, in.posX, in.posY,
            vmapOk ? std::to_string(groundZ).c_str() : "INVALID");
    }

    /* ── 4) final fallback to cached ADT height ────────────────────────── */
    if (!vmapOk) groundZ = in.adtGroundZ;

    const bool grounded = (in.posZ <= groundZ + GROUND_EPS);

    /* ── 5) liquid probe (still ADT) ───────────────────────────────────── */
    const bool inLiquid = (in.posZ < in.adtLiquidZ) &&
        (in.adtLiquidZ > groundZ + 0.1f);

    /* ── 6) movement state ---------------------------------------------- */
    enum class State { GROUND, AIR, SWIM };
    State state = State::GROUND;
    if (!onTrans && inLiquid) state = State::SWIM;
    else if (!grounded)       state = State::AIR;

    /* ── 7) horizontal intention ---------------------------------------- */
    float vx = 0.f, vy = 0.f, vz = in.velZ;
    auto Add = [&](float dx, float dy, float sp) { vx += dx * sp; vy += dy * sp; };

    if (!rooted)
    {
        const bool  walk = flags & MF::MOVEFLAG_WALK_MODE;
        const float runFwd = walk ? in.walkSpeed : in.runSpeed;
        const float runBack = walk ? in.walkSpeed : in.runBackSpeed;
        const float swimFwd = in.swimSpeed;
        const float swimBack = in.swimBackSpeed;

        if (state == State::SWIM)
        {
            if (flags & MF::MOVEFLAG_FORWARD)      Add(fwdX, fwdY, swimFwd);
            if (flags & MF::MOVEFLAG_BACKWARD)     Add(-fwdX, -fwdY, swimBack);
            if (flags & MF::MOVEFLAG_STRAFE_RIGHT) Add(rgtX, rgtY, swimFwd);
            if (flags & MF::MOVEFLAG_STRAFE_LEFT)  Add(-rgtX, -rgtY, swimFwd);
        }
        else
        {
            if (flags & MF::MOVEFLAG_FORWARD)      Add(fwdX, fwdY, runFwd);
            if (flags & MF::MOVEFLAG_BACKWARD)     Add(-fwdX, -fwdY, runBack);
            if (flags & MF::MOVEFLAG_STRAFE_RIGHT) Add(rgtX, rgtY, runFwd);
            if (flags & MF::MOVEFLAG_STRAFE_LEFT)  Add(-rgtX, -rgtY, runFwd);
        }
    }

    /* normalise diagonal */
    float hLen = std::sqrt(vx * vx + vy * vy);
    if (hLen > 0.f)
    {
        float cap = (state == State::SWIM) ? in.swimSpeed :
            ((flags & MF::MOVEFLAG_BACKWARD) ? in.runBackSpeed : in.runSpeed);
        if (hLen > cap) { float s = cap / hLen; vx *= s; vy *= s; }
    }

    /* ── 8) vertical physics -------------------------------------------- */
    switch (state)
    {
    case State::GROUND:
        vz = 0.f; flags &= ~FALL_MASK;
        if (flags & MF::MOVEFLAG_JUMPING)
        {
            vz = (in.jumpVerticalSpeed > 0 ? in.jumpVerticalSpeed : 8.2f);
            state = State::AIR;
        }
        break;

    case State::AIR:
        vz -= in.gravity * dt;
        vz = ClampF(vz, -TERMINAL_VELOCITY_YDPS, TERMINAL_VELOCITY_YDPS);
        if (!(flags & MF::MOVEFLAG_JUMPING)) flags |= MF::MOVEFLAG_FALLINGFAR;
        break;

    case State::SWIM:
        vz = 0.f;
        if (flags & MF::MOVEFLAG_JUMPING)    vz += in.swimSpeed;
        if (flags & MF::MOVEFLAG_PITCH_DOWN) vz -= in.swimSpeed;
        break;
    }

    /* ── 9) tentative move ---------------------------------------------- */
    float px = in.posX + vx * dt;
    float py = in.posY + vy * dt;
    float pz = in.posZ + vz * dt;

    /* ── 10) static wall check via VMap ---------------------------------- */
    {
        float hx, hy, hz;
        bool hit = vman.getObjectHitPos(in.mapId,
            in.posX, in.posY, in.posZ,
            px, py, pz,
            hx, hy, hz, in.radius);

        if (hit)
        {
            std::printf("[Step] VMap blocked  (%.1f,%.1f)->(%.1f,%.1f)\n",
                in.posX, in.posY, px, py);
            px = in.posX; py = in.posY;
            vx = vy = 0.f;
        }
    }

    /* ── 11) clamp to ground / water ------------------------------------ */
    if (state == State::AIR && pz <= groundZ + GROUND_EPS)
    {
        pz = groundZ; vz = 0.f; state = State::GROUND; flags &= ~FALL_MASK;
    }

    if (state == State::SWIM)
    {
        float surf = in.adtLiquidZ - in.height;
        if (!(flags & (MF::MOVEFLAG_WATERWALKING | MF::MOVEFLAG_HOVER)))
        {
            if (pz > surf) pz = surf;
            if (pz < groundZ) { pz = groundZ; state = State::GROUND; flags &= ~MF::MOVEFLAG_SWIMMING; }
        }
        else pz = in.adtLiquidZ;
    }

    /* ── 12) compose output --------------------------------------------- */
    PhysicsOutput out;
    out.newPosX = px; out.newPosY = py; out.newPosZ = pz;
    out.newVelX = vx; out.newVelY = vy; out.newVelZ = vz;
    out.movementFlags = flags;
    return out;
}
