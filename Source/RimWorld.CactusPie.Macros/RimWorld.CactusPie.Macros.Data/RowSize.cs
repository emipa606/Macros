namespace RimWorld.CactusPie.Macros.Data;

public readonly struct RowSize
{
    public float Width { get; }

    public float Height { get; }

    public RowSize(float width, float height)
    {
        Width = width;
        Height = height;
    }
}