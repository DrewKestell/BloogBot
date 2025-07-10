using System;
using System.Collections.Generic;
using System.IO;
using GameData.Core.Enums;
using GameData.Core.Models;
using WoWSharpClient.Models;
using WoWSharpClient.Utils;

namespace WoWSharpClient.Parsers
{
    /// <summary>
    ///   Serialises and deserialises the vanilla‑era MovementInfo blocks exactly
    ///   as MaNGOS 1.12 expects them (see MovementHandler.cpp).
    /// </summary>
    public static class MovementPacketHandler
    {
        /* --------------------------------------------------------------------
         *  BUILDERS                                                           */
        /* ------------------------------------------------------------------ */

        /// <summary>
        /// Basic teleport ACK – no movement‑block, just GUID + counter + time.
        /// </summary>
        public static byte[] BuildMoveTeleportAckPayload(ulong guid,
                                                        uint movementCounter,
                                                        uint timestamp)
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);

            w.Write(guid);
            w.Write(movementCounter);
            w.Write(timestamp);
            return ms.ToArray();
        }

        /*=== NEW: 3‑arg overload (full control) ============================*/
        public static byte[] BuildMovementInfoBuffer(WoWLocalPlayer p,
                                                     uint clientTimeMs,
                                                     uint fallTimeMs)
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);

            /* 1) Flags + timestamp */
            w.Write((uint)p.MovementFlags);
            w.Write(clientTimeMs);

            /* 2) Position */
            w.Write(p.Position.X);
            w.Write(p.Position.Y);
            w.Write(p.Position.Z);
            w.Write(p.Facing);

            /* 3) Transport */
            if (p.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_ONTRANSPORT) &&
                p.Transport != null)
            {
                w.Write(p.Transport.Guid);
                w.Write(p.Transport.Position.X);
                w.Write(p.Transport.Position.Y);
                w.Write(p.Transport.Position.Z);
                w.Write(p.Transport.Facing);
            }

            /* 4) Swim pitch */
            if (p.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SWIMMING))
                w.Write(p.SwimPitch);

            /* 5) fallTime – always present */
            w.Write(fallTimeMs);

            /* 6) Jump */
            if (p.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_JUMPING))
            {
                w.Write(p.JumpVerticalSpeed);
                w.Write(p.JumpCosAngle);
                w.Write(p.JumpSinAngle);
                w.Write(p.JumpHorizontalSpeed);
            }

            /* 7) Spline elevation */
            if (p.MovementFlags.HasFlag(MovementFlags.MOVEFLAG_SPLINE_ELEVATION))
                w.Write(p.SplineElevation);

            return ms.ToArray();
        }

        /*=== Legacy wrapper (2‑args) ======================================*/
        public static byte[] BuildMovementInfoBuffer(WoWLocalPlayer p,
                                                     uint clientTimeMs)
            => BuildMovementInfoBuffer(p,
                                       clientTimeMs,
                                       (uint)p.FallTime);   // ← cast fixes CS1503

        /* Ack helper used by *_SPEED/ROOT_CHANGE_ACK opcodes */
        internal static byte[] BuildForceMoveAck(WoWLocalPlayer player,
                                                 uint movementCounter,
                                                 uint clientTimeMs)
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);

            ReaderUtils.WritePackedGuid(w, player.Guid);
            w.Write(movementCounter);
            w.Write(BuildMovementInfoBuffer(player,
                                            clientTimeMs,
                                            (uint)player.FallTime)); // ← cast fixes CS1503
            return ms.ToArray();
        }

        /* --------------------------------------------------------------------
         *  PARSERS                                                           */
        /* ------------------------------------------------------------------ */

        public static MovementInfoUpdate ParseMovementBlock(BinaryReader r)
        {
            var info = new MovementInfoUpdate
            {
                MovementFlags = (MovementFlags)r.ReadUInt32(),
                LastUpdated = r.ReadUInt32(),

                X = r.ReadSingle(),
                Y = r.ReadSingle(),
                Z = r.ReadSingle(),
                Facing = r.ReadSingle(),
                MovementBlockUpdate = new MovementBlockUpdate()
            };

            /* ----- Transport --------------------------------------------- */
            if (info.HasTransport)
            {
                info.TransportGuid = r.ReadUInt64();          // uint64
                float tx = r.ReadSingle();
                float ty = r.ReadSingle();
                float tz = r.ReadSingle();
                info.TransportOffset = new Position(tx, ty, tz);
                info.TransportOrientation = r.ReadSingle();
                info.TransportLastUpdated = r.ReadUInt32();
            }

            /* ----- Swim pitch ------------------------------------------- */
            if (info.IsSwimming)
                info.SwimPitch = r.ReadSingle();

            /* fallTime always present                                      */
            info.FallTime = r.ReadUInt32();

            /* ----- Jump block ------------------------------------------- */
            if (info.IsFalling)
            {
                info.JumpVerticalSpeed = r.ReadSingle();
                info.JumpCosAngle = r.ReadSingle();
                info.JumpSinAngle = r.ReadSingle();
                info.JumpHorizontalSpeed = r.ReadSingle();
            }

            /* ----- Spline elevation ------------------------------------- */
            if (info.HasSplineElevation)
                info.SplineElevation = r.ReadSingle();

            /* ----- Speeds block (walk|run|swim…) ------------------------ */
            info.MovementBlockUpdate.WalkSpeed = r.ReadSingle();
            info.MovementBlockUpdate.RunSpeed = r.ReadSingle();
            info.MovementBlockUpdate.RunBackSpeed = r.ReadSingle();
            info.MovementBlockUpdate.SwimSpeed = r.ReadSingle();
            info.MovementBlockUpdate.SwimBackSpeed = r.ReadSingle();
            info.MovementBlockUpdate.TurnRate = r.ReadSingle();

            /* ----- Optional spline path --------------------------------- */
            if (info.HasSpline)
            {
                var flags = (SplineFlags)r.ReadUInt32();
                info.MovementBlockUpdate.SplineFlags = flags;

                if (flags.HasFlag(SplineFlags.FinalPoint))
                {
                    float x = r.ReadSingle();
                    float y = r.ReadSingle();
                    float z = r.ReadSingle();
                    info.MovementBlockUpdate.SplineFinalPoint = new Position(x, y, z);
                }
                else if (flags.HasFlag(SplineFlags.FinalTarget))
                {
                    info.MovementBlockUpdate.SplineTargetGuid = r.ReadUInt64();
                }
                else if (flags.HasFlag(SplineFlags.FinalOrientation))
                {
                    info.MovementBlockUpdate.SplineFinalOrientation = r.ReadSingle();
                }

                info.MovementBlockUpdate.SplineTimePassed = r.ReadInt32();
                info.MovementBlockUpdate.SplineDuration = r.ReadInt32();
                info.MovementBlockUpdate.SplineId = r.ReadUInt32();

                uint nodeCount = r.ReadUInt32();
                var nodes = new List<Position>((int)nodeCount);
                for (int i = 0; i < nodeCount; ++i)
                {
                    float x = r.ReadSingle();
                    float y = r.ReadSingle();
                    float z = r.ReadSingle();
                    nodes.Add(new Position(x, y, z));
                }
                info.MovementBlockUpdate.SplineNodes = nodes;

                float fx = r.ReadSingle();
                float fy = r.ReadSingle();
                float fz = r.ReadSingle();
                info.MovementBlockUpdate.SplineFinalDestination = new Position(fx, fy, fz);
            }

            return info;
        }

        /// <summary>
        /// Parser for short form (no speed block, no spline).
        /// </summary>
        public static MovementInfoUpdate ParseMovementInfo(BinaryReader r)
        {
            var info = new MovementInfoUpdate
            {
                MovementFlags = (MovementFlags)r.ReadUInt32(),
                LastUpdated = r.ReadUInt32(),
                X = r.ReadSingle(),
                Y = r.ReadSingle(),
                Z = r.ReadSingle(),
                Facing = r.ReadSingle()
            };

            if (info.HasTransport)
            {
                info.TransportGuid = r.ReadUInt64();
                float tx = r.ReadSingle();
                float ty = r.ReadSingle();
                float tz = r.ReadSingle();
                info.TransportOffset = new Position(tx, ty, tz);
                info.TransportOrientation = r.ReadSingle();
                info.TransportLastUpdated = r.ReadUInt32();
            }

            if (info.IsSwimming)
                info.SwimPitch = r.ReadSingle();

            info.FallTime = r.ReadUInt32();

            if (info.IsFalling)
            {
                info.JumpVerticalSpeed = r.ReadSingle();
                info.JumpCosAngle = r.ReadSingle();
                info.JumpSinAngle = r.ReadSingle();
                info.JumpHorizontalSpeed = r.ReadSingle();
            }

            if (info.HasSplineElevation)
                info.SplineElevation = r.ReadSingle();

            return info;
        }
    }
}
