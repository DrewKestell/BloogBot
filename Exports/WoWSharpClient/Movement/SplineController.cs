using GameData.Core.Enums;
using GameData.Core.Models;
using WoWSharpClient.Models;

namespace WoWSharpClient.Movement
{
    /// <summary>Immutable data copied straight from SMSG_MONSTER_MOVE.</summary>
    public sealed class Spline(ulong owner, uint id, uint t0, SplineFlags flags,
                  IReadOnlyList<Position> pts, uint durationMs)
    {
        public ulong OwnerGuid { get; } = owner;
        public uint Id { get; } = id;
        public uint StartMs { get; } = t0;
        public SplineFlags Flags { get; } = flags;
        public IReadOnlyList<Position> Points { get; } = pts;
        public uint DurationMs { get; } = durationMs;

        internal float SegmentMs => (Points.Count <= 1) ? 0 : DurationMs / (float)(Points.Count - 1);
    }

    /// <summary>Per-tick state machine that walks along the spline.</summary>
    /// <summary>Per-tick state machine that walks along one spline.</summary>
    internal sealed class ActiveSpline(Spline s)
    {
        public Spline Spline { get; } = s;
        private float _elapsed;   // ms since server timestamp
        private int _seg;       // current segment index

        /// <summary>Advance <paramref name="dtMs"/> and return the new position.</summary>
        public Position Step(float dtMs)
        {
            _elapsed += dtMs;

            while (_seg + 1 < Spline.Points.Count &&
                   _elapsed >= (_seg + 1) * Spline.SegmentMs)
                _seg++;

            if (_seg + 1 >= Spline.Points.Count)
                return Spline.Points[^1];

            float u = (_elapsed - _seg * Spline.SegmentMs) / Spline.SegmentMs;
            var a = Spline.Points[_seg];
            var b = Spline.Points[_seg + 1];

            // manual lerp (no MathF.Lerp before .NET 8)
            float x = a.X + (b.X - a.X) * u;
            float y = a.Y + (b.Y - a.Y) * u;
            float z = a.Z + (b.Z - a.Z) * u;
            return new Position(x, y, z);
        }

        public bool Finished => _seg + 1 >= Spline.Points.Count;
    }

    /// <summary>Central registry that drives every active spline each frame.</summary>
    public sealed class SplineController
    {
        private readonly Dictionary<ulong, ActiveSpline> _active = [];

        public void AddOrUpdate(Spline s) => _active[s.OwnerGuid] = new ActiveSpline(s);

        public void Remove(ulong guid) => _active.Remove(guid);

        public void Update(float dtMs)
        {
            foreach (var (guid, active) in _active.ToArray())
            {
                if (active.Finished) { _active.Remove(guid); continue; }

                WoWUnit? woWUnit = WoWSharpObjectManager.Instance.Objects.OfType<WoWUnit>().FirstOrDefault(x => x.Guid == guid);
                if (woWUnit != null)
                    woWUnit.Position = active.Step(dtMs);
                else
                    _active.Remove(guid); // object vanished
            }
        }
    }

    /// <summary>Global helper (lazy-init singleton).</summary>
    public static class Splines
    {
        public static readonly SplineController Instance = new();
    }
}
