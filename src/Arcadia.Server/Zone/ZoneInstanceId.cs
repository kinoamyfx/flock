namespace Arcadia.Server.Zone;

public readonly record struct ZoneInstanceId(Guid Value)
{
    public static ZoneInstanceId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("N");
}

