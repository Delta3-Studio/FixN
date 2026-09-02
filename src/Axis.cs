using System.Diagnostics.Contracts;

namespace FixN;

[Flags]
public enum Axis : byte
{
    None = 0,
    X = 1,
    Y = 2,
    Z = 4,

    XYZ = X | Y | Z,
    XY = X | Y,
    XZ = X | Z,
    YZ = Y | Z,
}

public static class AxisEx
{
    extension(Axis value)
    {
        [MImpl(MOpt.AggressiveInlining)]
        public Vec3 ToVec3() =>
            value switch
            {
                Axis.None => Vec3.Zero,
                Axis.X => Vec3.UnitX,
                Axis.Y => Vec3.UnitY,
                Axis.Z => Vec3.UnitZ,
                Axis.XYZ => Vec3.One,
                Axis.XY => Vec3.UnitXY,
                Axis.XZ => Vec3.UnitXZ,
                Axis.YZ => Vec3.UnitYZ,
                _ => new(value),
            };

        [Pure, MImpl(MOpt.AggressiveInlining)]
        public bool Has(Axis flag) => flag is not Axis.None && (value & flag) == flag;

        [Pure, MImpl(MOpt.AggressiveInlining)]
        Fix GetUnit(Axis axis) => value.Has(axis) ? Fix.One : Fix.Zero;

        public bool HasX
        {
            [MImpl(MOpt.AggressiveInlining)]
            get => value.Has(Axis.X);
        }

        public bool HasY
        {
            [MImpl(MOpt.AggressiveInlining)]
            get => value.Has(Axis.Y);
        }

        public bool HasZ
        {
            [MImpl(MOpt.AggressiveInlining)]
            get => value.Has(Axis.Z);
        }

        public Fix UnitX
        {
            [MImpl(MOpt.AggressiveInlining)]
            get => value.GetUnit(Axis.X);
        }

        public Fix UnitY
        {
            [MImpl(MOpt.AggressiveInlining)]
            get => value.GetUnit(Axis.Y);
        }

        public Fix UnitZ
        {
            [MImpl(MOpt.AggressiveInlining)]
            get => value.GetUnit(Axis.Z);
        }
    }
}
