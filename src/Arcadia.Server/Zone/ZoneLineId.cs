namespace Arcadia.Server.Zone;

public readonly record struct ZoneLineId(int Value)
{
    public override string ToString() => Value.ToString();
}

