namespace VMAP
{
    /// <summary>Bit‑flags that describe a VMAP model instance – kept 1:1 with the native enum.</summary>
    [Flags]
    public enum ModelFlags : uint
    {
        MOD_M2 = 1,
        MOD_WORLDSPAWN = 1 << 1,
        MOD_HAS_BOUND = 1 << 2,
        MOD_NO_BREAK_LOS = 1 << 3 | MOD_M2
    }
}