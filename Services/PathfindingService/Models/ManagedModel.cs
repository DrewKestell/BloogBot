namespace PathfindingService.Repository
{
    using System;
    using VMAP;

    // Weak reference holder for a WorldModel. Manages persistent reference if needed.
    public class ManagedModel
    {
        private readonly WeakReference<WorldModel> _weak;
        private readonly WorldModel? _keepAlive;
        internal ManagedModel(WorldModel? m, bool managed)
        {
            if (m != null) _weak = new WeakReference<WorldModel>(m);
            if (managed) _keepAlive = m;
        }
        internal bool TryGet(out WorldModel? m)
        {
            m = null;
            return _weak != null && _weak.TryGetTarget(out m);
        }
    }
}
