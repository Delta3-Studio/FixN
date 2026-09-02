namespace FixN;

static class StableHash
{
    const int Seed = unchecked((int)2166136261);
    const int Prime = 16777619;

    [MImpl(MOpt.AggressiveInlining)]
    public static int Combine(int a)
    {
        var hash = Seed;
        hash = (hash ^ a) * Prime;
        return hash;
    }

    [MImpl(MOpt.AggressiveInlining)]
    public static int Combine(int a, int b)
    {
        var hash = Seed;
        hash = (hash ^ a) * Prime;
        hash = (hash ^ b) * Prime;
        return hash;
    }

    [MImpl(MOpt.AggressiveInlining)]
    public static int Combine(int a, int b, int c)
    {
        var hash = Seed;
        hash = (hash ^ a) * Prime;
        hash = (hash ^ b) * Prime;
        hash = (hash ^ c) * Prime;
        return hash;
    }

    [MImpl(MOpt.AggressiveInlining)]
    public static int Combine(int a, int b, int c, int d)
    {
        var hash = Seed;
        hash = (hash ^ a) * Prime;
        hash = (hash ^ b) * Prime;
        hash = (hash ^ c) * Prime;
        hash = (hash ^ d) * Prime;
        return hash;
    }
}
