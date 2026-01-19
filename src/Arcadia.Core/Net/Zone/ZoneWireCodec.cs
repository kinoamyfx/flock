using System.Text;
using System.Text.Json;

namespace Arcadia.Core.Net.Zone;

public static class ZoneWireCodec
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static byte[] EncodeHello(ZoneHello hello) => Encode(ZoneWireMessageType.Hello, hello);
    public static byte[] EncodeWelcome(ZoneWelcome welcome) => Encode(ZoneWireMessageType.Welcome, welcome);
    public static byte[] EncodeError(ZoneError error) => Encode(ZoneWireMessageType.Error, error);
    public static byte[] EncodeMoveIntent(ZoneMoveIntent intent) => Encode(ZoneWireMessageType.MoveIntent, intent);
    public static byte[] EncodePickupIntent(ZonePickupIntent intent) => Encode(ZoneWireMessageType.PickupIntent, intent);
    public static byte[] EncodeEvacIntent(ZoneEvacIntent intent) => Encode(ZoneWireMessageType.EvacIntent, intent);
    public static byte[] EncodeDebugKillSelf() => Encode(ZoneWireMessageType.DebugKillSelf, new { });
    public static byte[] EncodeSnapshot(ZoneSnapshot snapshot) => Encode(ZoneWireMessageType.Snapshot, snapshot);
    public static byte[] EncodeLootSpawned(ZoneLootSpawned msg) => Encode(ZoneWireMessageType.LootSpawned, msg);
    public static byte[] EncodeLootPicked(ZoneLootPicked msg) => Encode(ZoneWireMessageType.LootPicked, msg);
    public static byte[] EncodeEvacStatus(ZoneEvacStatus msg) => Encode(ZoneWireMessageType.EvacStatus, msg);

    public static bool TryDecode(ReadOnlySpan<byte> data, out ZoneWireEnvelope envelope)
    {
        envelope = default!;
        try
        {
            var json = Encoding.UTF8.GetString(data);
            var env = JsonSerializer.Deserialize<ZoneWireEnvelope>(json, JsonOptions);
            if (env is null)
            {
                return false;
            }

            envelope = env;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryGetHello(in ZoneWireEnvelope envelope, out ZoneHello hello) => TryGetPayload(envelope, ZoneWireMessageType.Hello, out hello);
    public static bool TryGetWelcome(in ZoneWireEnvelope envelope, out ZoneWelcome welcome) => TryGetPayload(envelope, ZoneWireMessageType.Welcome, out welcome);
    public static bool TryGetError(in ZoneWireEnvelope envelope, out ZoneError error) => TryGetPayload(envelope, ZoneWireMessageType.Error, out error);
    public static bool TryGetMoveIntent(in ZoneWireEnvelope envelope, out ZoneMoveIntent intent) => TryGetPayload(envelope, ZoneWireMessageType.MoveIntent, out intent);
    public static bool TryGetPickupIntent(in ZoneWireEnvelope envelope, out ZonePickupIntent intent) => TryGetPayload(envelope, ZoneWireMessageType.PickupIntent, out intent);
    public static bool TryGetEvacIntent(in ZoneWireEnvelope envelope, out ZoneEvacIntent intent) => TryGetPayload(envelope, ZoneWireMessageType.EvacIntent, out intent);
    public static bool IsDebugKillSelf(in ZoneWireEnvelope envelope) => envelope.Type == ZoneWireMessageType.DebugKillSelf;
    public static bool TryGetSnapshot(in ZoneWireEnvelope envelope, out ZoneSnapshot snapshot) => TryGetPayload(envelope, ZoneWireMessageType.Snapshot, out snapshot);
    public static bool TryGetLootSpawned(in ZoneWireEnvelope envelope, out ZoneLootSpawned msg) => TryGetPayload(envelope, ZoneWireMessageType.LootSpawned, out msg);
    public static bool TryGetLootPicked(in ZoneWireEnvelope envelope, out ZoneLootPicked msg) => TryGetPayload(envelope, ZoneWireMessageType.LootPicked, out msg);
    public static bool TryGetEvacStatus(in ZoneWireEnvelope envelope, out ZoneEvacStatus msg) => TryGetPayload(envelope, ZoneWireMessageType.EvacStatus, out msg);

    private static byte[] Encode<T>(ZoneWireMessageType type, T payload)
    {
        // Why: MVP 用 JSON 便于抓包与调试；后续可换成二进制协议（不改业务语义）。
        // Context: ENet 仅是传输层，协议层必须可替换与版本化。
        // Attention: Payload 体积要可控；频繁同步应走 snapshot/delta 的二进制格式，避免 JSON 开销。
        var payloadElement = JsonSerializer.SerializeToElement(payload!, JsonOptions);
        var env = new ZoneWireEnvelope(type, payloadElement);
        var json = JsonSerializer.Serialize(env, JsonOptions);
        return Encoding.UTF8.GetBytes(json);
    }

    private static bool TryGetPayload<T>(in ZoneWireEnvelope envelope, ZoneWireMessageType expectedType, out T payload)
    {
        payload = default!;
        if (envelope.Type != expectedType)
        {
            return false;
        }

        try
        {
            payload = envelope.Payload.Deserialize<T>(JsonOptions)!;
            return payload is not null;
        }
        catch
        {
            return false;
        }
    }
}
