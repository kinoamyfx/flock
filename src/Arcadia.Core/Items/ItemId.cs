namespace Arcadia.Core.Items;

public readonly record struct ItemId(Guid Value)
{
    public static ItemId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("N");
}

