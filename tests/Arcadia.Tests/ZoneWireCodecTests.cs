using System.Text;
using Arcadia.Core.Net.Zone;

namespace Arcadia.Tests;

public sealed class ZoneWireCodecTests
{
    [Fact]
    public void EncodeDecode_Hello_ShouldRoundTrip()
    {
        var hello = new ZoneHello("token-p1", "dev");
        var bytes = ZoneWireCodec.EncodeHello(hello);

        Assert.True(ZoneWireCodec.TryDecode(bytes, out var env));
        Assert.Equal(ZoneWireMessageType.Hello, env.Type);
        Assert.True(ZoneWireCodec.TryGetHello(env, out var decoded));
        Assert.Equal("token-p1", decoded.AuthToken);
        Assert.Equal("dev", decoded.ClientVersion);
    }

    [Fact]
    public void TryDecode_InvalidJson_ShouldReturnFalse()
    {
        var bytes = Encoding.UTF8.GetBytes("not-json");
        Assert.False(ZoneWireCodec.TryDecode(bytes, out _));
    }
}
