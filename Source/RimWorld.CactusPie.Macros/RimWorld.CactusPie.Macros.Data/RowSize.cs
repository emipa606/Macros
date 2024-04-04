namespace RimWorld.CactusPie.Macros.Data;

public readonly struct RowSize(float width, float height)
{
    public float Width { get; } = width;

    public float Height { get; } = height;
}