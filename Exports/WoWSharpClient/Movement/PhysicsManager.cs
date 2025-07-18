// -----------------------------------------------------------------------------
// PhysicsManager.cs – client-side Z-physics & XY gate with diagnostics
// -----------------------------------------------------------------------------

using BotRunner.Clients;
using GameData.Core.Models;
using Microsoft.Extensions.Logging;
using Pathfinding;
using System.Numerics;
using System.Text.Json;
using WoWSharpClient.Models;

namespace WoWSharpClient.Movement
{
    public sealed class PhysicsManager(PathfindingClient nav, ILogger<PhysicsManager>? logger = null)
    {
        /* ───── Tunables ───── */
        private const float Gravity = 19.29f, TerminalVel = 60f;
        private const float LiquidG = 4f, LiquidTermVel = 8f;
        private const float GroundSnap = 0.05f;          // 5 cm, tighter clamp
        private const float StepHeight = 0.90f;
        private const float WalkableClimb = 0.90f;
        private const float MaxSlopeDeg = 50f;
        private const float SlideSlopeDeg = 65f, SlideSpeed = 4f;
        private const float ZBias = 0.0f;

        /* Detour flags / areas */
        private const uint PolyWalk = 0x01, PolyDoor = 0x02, PolyDisabled = 0x10;
        private static readonly uint WalkFlags = PolyWalk | PolyDoor;

        private static readonly HashSet<NavTerrain> WalkableAreas =
        [
            NavTerrain.NavGround
        ];

        /* Runtime */
        private readonly PathfindingClient _nav = nav;
        private readonly ILogger<PhysicsManager>? _log = logger;

        public float VerticalVelocity { get; private set; }
        public long FallStartMs { get; private set; }

        /* ═════════════════════════ ApplyPhysics ═════════════════════════ */
        public PhysicsState ApplyPhysics(WoWLocalPlayer pl, float dt, long now, uint map)
        {
            if (dt <= 0) return new();

            Position feet = pl.Position;
            Debug("PHY.IN", new { dt, pos = V(feet) });

            TerrainProbeResponse probe = _nav.ProbeTerrain(map, feet, pl.Race);
            Debug("NAV.PROBE", new { probe.GroundZ, probe.LiquidZ, overlaps = probe.Overlaps?.Count });

            /* no poly coverage */
            if (probe.Overlaps is null || probe.Overlaps.Count == 0)
                return HandleNoPoly(feet, dt, probe);

            /* nearest walkable poly */
            (NavPolyHit poly, Vector3 cpt)? nearest =
                GetNearestWalkPoly(probe, new Vector3(feet.X, feet.Y, feet.Z));

            float groundZ = SelectGroundZ(nearest, probe, feet.Z, out string gMethod);
            Debug("GROUND.SELECT", new { method = gMethod, groundZ });

            bool grounded = !float.IsNaN(groundZ) && Math.Abs(feet.Z - groundZ) < GroundSnap;
            bool inLiquid = !float.IsNaN(probe.LiquidZ) &&
                            feet.Z < probe.LiquidZ && probe.LiquidZ > groundZ;
            float slope = nearest is { poly: var p } ? ComputeSlopeDeg(p) : 0;
            Debug("STATE.FLAGS", new { grounded, inLiquid, slopeDeg = slope });

            if (!grounded && !float.IsNaN(groundZ) && groundZ - feet.Z <= StepHeight)
            { grounded = true; VerticalVelocity = 0; }

            float desiredZ = grounded ? groundZ + ZBias : feet.Z;
            Vector2 slide = Vector2.Zero;

            if (grounded)
            {
                VerticalVelocity = 0;
                FallStartMs = 0;
                if (slope > SlideSlopeDeg && nearest is { poly: var pol })
                    slide = ComputeDownhillVec(pol) * SlideSpeed * dt;
            }
            else
            {
                if (FallStartMs == 0) FallStartMs = now;
                float g = inLiquid ? LiquidG : Gravity;
                float term = inLiquid ? LiquidTermVel : TerminalVel;

                VerticalVelocity = Math.Clamp(VerticalVelocity - g * dt, -term, term);
                desiredZ += VerticalVelocity * dt;

                if (!float.IsNaN(groundZ) && desiredZ < groundZ)
                { desiredZ = groundZ + ZBias; grounded = true; VerticalVelocity = 0; FallStartMs = 0; }
            }
            Debug("Z.CLAMP", new
            {
                feetZ = feet.Z,
                groundZ,
                desiredZ,
                verticalVelocity = VerticalVelocity,
                grounded,
                slopeDeg = slope
            });

            return new PhysicsState
            {
                DesiredZ = desiredZ,
                Grounded = grounded,
                InLiquid = inLiquid,
                VerticalVelocity = VerticalVelocity,
                SlideDelta = slide,
                GroundZ = groundZ
            };
        }

        /* ══════════════ XY Collision Gate ══════════════ */
        public Vector2 ProcessHorizontalMovement(WoWLocalPlayer pl, Vector2 wantXY, uint map)
        {
            if (wantXY.LengthSquared() < 1e-6f)
                return wantXY;                    // no intent

            Position curr = pl.Position;
            Position dest = new(curr.X + wantXY.X, curr.Y + wantXY.Y, curr.Z);

            var currHit = GetNearestWalkPoly(_nav.ProbeTerrain(map, curr, pl.Race),
                                             new Vector3(curr.X, curr.Y, curr.Z));
            var destHit = GetNearestWalkPoly(_nav.ProbeTerrain(map, dest, pl.Race),
                                             new Vector3(dest.X, dest.Y, dest.Z));

            /* ----- decision matrix ----- */
            if (currHit == null && destHit == null)
            {
                Debug("XY.GATE", new { want = wantXY, allow = false, reason = "noMesh" });
                return Vector2.Zero;
            }

            if (currHit != null && destHit == null)
            {
                Debug("XY.GATE", new { want = wantXY, allow = true, reason = "sparseMesh" });
                return wantXY;                           // step off current tri
            }

            if (destHit is { Item1: var d })
            {
                if (ComputeSlopeDeg(d) > MaxSlopeDeg)
                { Debug("XY.GATE", new { want = wantXY, allow = false, reason = "steep" }); return Vector2.Zero; }

                if (currHit is { Item2: var c } &&
                    destHit is { Item2: var t } &&
                    t.Z - c.Z > StepHeight)
                { Debug("XY.GATE", new { want = wantXY, allow = false, reason = "stepTooHigh" }); return Vector2.Zero; }
            }

            Debug("XY.GATE", new { want = wantXY, allow = true, reason = "ok" });
            return wantXY;
        }

        /* ───────── helpers ───────── */

        private PhysicsState HandleNoPoly(Position feet, float dt, TerrainProbeResponse probe)
        {
            if (!float.IsNaN(probe.LiquidZ) && feet.Z < probe.LiquidZ)           // swim
            {
                VerticalVelocity = Math.Clamp(VerticalVelocity - LiquidG * dt, -LiquidTermVel, LiquidTermVel);
                return NewState(feet.Z + VerticalVelocity * dt, false, true, probe.LiquidZ);
            }
            if (!float.IsNaN(probe.GroundZ))                                     // snap to ADT
                return NewState(probe.GroundZ + ZBias, true, false, probe.GroundZ);

            VerticalVelocity = Math.Max(VerticalVelocity - Gravity * dt, -TerminalVel); // free-fall
            return NewState(feet.Z + VerticalVelocity * dt, false, false, float.NaN);
        }

        private static PhysicsState NewState(float z, bool grd, bool liq, float gZ) =>
            new()
            {
                DesiredZ = z,
                Grounded = grd,
                InLiquid = liq,
                VerticalVelocity = 0,
                SlideDelta = Vector2.Zero,
                GroundZ = gZ
            };
        private static float AverageZ(NavPolyHit p) => p.Verts.Count == 0 ? float.NaN : p.Verts.Average(v => v.Z);
        private static float SelectGroundZ((NavPolyHit poly, Vector3 cpt)? nearest,
                                    TerrainProbeResponse probe, float currZ, out string method)
        {
            if (nearest is { } n) { method = "polyClosest"; return n.cpt.Z; }
            if (!float.IsNaN(probe.GroundZ)) { method = "fieldGroundZ"; return probe.GroundZ; }

            method = "polyAvgFallback";
            return probe.Overlaps.Select(AverageZ)
                                 .OrderBy(z => Math.Abs(z - currZ))
                                 .FirstOrDefault(float.NaN);
        }

        private static bool IsWalkable(NavPolyHit poly) =>
                    poly.Flags.HasFlag(NavPolyFlag.PolyFlagWalk) &&
                    !poly.Flags.HasFlag(NavPolyFlag.PolyFlagDisabled);

        private static (NavPolyHit poly, Vector3 cpt)? GetNearestWalkPoly(TerrainProbeResponse probe,
                                                           Vector3 sample)
        {
            float best = float.MaxValue;
            (NavPolyHit poly, Vector3 cpt) bestHit = default;

            int idx = 0;
            foreach (var poly in probe.Overlaps)
            {
                if (!IsWalkable(poly))
                { Debug("POLY.SKIP", new { idx, reason = "notWalkable", poly.Flags, poly.Area }); idx++; continue; }

                var verts = poly.Verts.Where(v => v.X * v.X + v.Y * v.Y + v.Z * v.Z > 1e-8f)
                                      .Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();

                Debug("POLY.VERTS", new
                {
                    idx,
                    count = verts.Length,
                    verts = verts.Select(v => new { v.X, v.Y, v.Z }).ToArray()
                });
                if (verts.Length < 3)
                { Debug("POLY.SKIP", new { idx, reason = "degenerate" }); idx++; continue; }

                Vector3 cpt = ClosestPointOnPolyXY(verts, sample, out bool over);
                float dist = over
                           ? Math.Max(0, Math.Abs(sample.Z - cpt.Z) - WalkableClimb)
                           : Vector2.Distance(new(sample.X, sample.Y), new(cpt.X, cpt.Y));

                Debug("POLY.TEST", new { idx, dist, over, slope = ComputeSlopeDeg(poly) });

                if (dist < best) { best = dist; bestHit = (poly, cpt); }
                idx++;
            }

            if (best < float.MaxValue)
                Debug("POLY.PICK", new { bestDist = best, cpt = new { bestHit.cpt.X, bestHit.cpt.Y, bestHit.cpt.Z } });
            else
                Debug("POLY.PICK", new { result = "none" });

            return best < float.MaxValue ? bestHit : null;
        }

        /* point-in-poly & closest-point helpers */
        private static bool PtInPoly(Vector2 p, IReadOnlyList<Vector3> v)
        {
            bool inside = false;
            for (int i = 0, j = v.Count - 1; i < v.Count; j = i++)
            {
                var a = v[i]; var b = v[j];
                if (((a.Y > p.Y) != (b.Y > p.Y)) &&
                    p.X < (b.X - a.X) * (p.Y - a.Y) / (b.Y - a.Y) + a.X)
                    inside = !inside;
            }

            return inside;
        }

        private static Vector3 ClosestPointOnPolyXY(IReadOnlyList<Vector3> v, Vector3 pos, out bool over)
        {
            static float PlaneZ(IReadOnlyList<Vector3> t, float x, float y)
            {
                var n = Vector3.Normalize(Vector3.Cross(t[1] - t[0], t[2] - t[0]));
                float d = -Vector3.Dot(n, t[0]);
                return Math.Abs(n.Z) < 1e-5f ? t[0].Z : (-(n.X * x + n.Y * y + d) / n.Z);
            }

            Vector2 pt = new(pos.X, pos.Y);

            if (PtInPoly(pt, v))
            { over = true; return new(pos.X, pos.Y, PlaneZ(v, pos.X, pos.Y)); }

            over = false; float best = float.MaxValue; Vector3 bestPt = default;
            for (int i = 0, j = v.Count - 1; i < v.Count; j = i++)
            {
                Vector3 a = v[j], b = v[i];
                Vector2 ab = new(b.X - a.X, b.Y - a.Y);
                Vector2 ap = pt - new Vector2(a.X, a.Y);

                float t = Math.Clamp(Vector2.Dot(ap, ab) / ab.LengthSquared(), 0, 1);
                float x = a.X + ab.X * t, y = a.Y + ab.Y * t;
                float z = PlaneZ(v, x, y);

                float d2 = (x - pt.X) * (x - pt.X) + (y - pt.Y) * (y - pt.Y);
                if (d2 < best) { best = d2; bestPt = new(x, y, z); }
            }
            Debug("POLY.CLOSEST", new
            {
                input = new { pos.X, pos.Y },
                over,
                best = new { bestPt.X, bestPt.Y, bestPt.Z }
            });
            return bestPt;
        }

        private static float ComputeSlopeDeg(NavPolyHit p)
        {
            var verts = p.Verts
                .Where(v => v.X * v.X + v.Y * v.Y + v.Z * v.Z > 1e-8f)
                .Select(v => new Vector3(v.X, v.Y, v.Z))
                .ToList();

            if (verts.Count < 3)
                return 0;

            Vector3 normalSum = Vector3.Zero;
            for (int i = 1; i < verts.Count - 1; ++i)
            {
                Vector3 a = verts[0], b = verts[i], c = verts[i + 1];
                var n = Vector3.Cross(b - a, c - a);
                if (n.LengthSquared() > 1e-6f)
                    normalSum += Vector3.Normalize(n);
            }

            if (normalSum == Vector3.Zero)
                return 0;

            var avg = Vector3.Normalize(normalSum);
            float slope = MathF.Acos(Math.Clamp(Math.Abs(avg.Z), -1f, 1f)) * 57.29578f;
            Debug("POLY.SLOPE", new { slope, avg });
            return slope;
        }

        private static Vector2 ComputeDownhillVec(NavPolyHit p)
        {
            var verts = p.Verts
                .Where(v => v.X * v.X + v.Y * v.Y + v.Z * v.Z > 1e-8f)
                .Select(v => new Vector3(v.X, v.Y, v.Z))
                .ToList();

            if (verts.Count < 3)
                return Vector2.Zero;

            Vector3 normalSum = Vector3.Zero;
            for (int i = 1; i < verts.Count - 1; ++i)
            {
                Vector3 a = verts[0], b = verts[i], c = verts[i + 1];
                var n = Vector3.Cross(b - a, c - a);
                if (n.LengthSquared() > 1e-6f)
                    normalSum += Vector3.Normalize(n);
            }

            if (normalSum == Vector3.Zero)
                return Vector2.Zero;

            var avg = Vector3.Normalize(normalSum);
            Vector2 downhill = new(avg.X, avg.Y); // flatten to XY

            // Negate downhill vector to go in direction of descent
            return downhill.LengthSquared() < 1e-6f ? Vector2.Zero : Vector2.Normalize(-downhill);
        }

        private static object V(Position p) => new { p.X, p.Y, p.Z };
        private static void Debug(string tag, object payload) =>
            Console.WriteLine($"[PHY][{tag}] {JsonSerializer.Serialize(payload)}");
    }

    public record PhysicsState
    {
        public float DesiredZ { get; init; }
        public bool Grounded { get; init; }
        public bool InLiquid { get; init; }
        public float VerticalVelocity { get; init; }
        public Vector2 SlideDelta { get; init; }
        public float GroundZ { get; init; }
    }
}
