using System.Text.Json;

namespace Arcadia.Core.Net.Zone;

public sealed record ZoneWireEnvelope(
    ZoneWireMessageType Type,
    JsonElement Payload
);

