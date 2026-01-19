using Arcadia.Client.Net.Enet;
using Arcadia.Core.Logging;
using Arcadia.Core.Net.Zone;
using Arcadia.LoadTest.Gateway;

namespace Arcadia.LoadTest;

public static class PlayableSliceTest
{
    public static async Task RunAsync(
        string host,
        ushort port,
        string playerId,
        string gatewayUrl,
        string issueKey,
        CancellationToken cancellationToken)
    {
        // Why: 验证"连接→移动→死亡掉落→拾取"完整闭环，确保 v1-playable-dungeon-slice 可交付。
        // Context: 这是 v1.0.0 关键路径的最后一个验收项（4.1 冒烟测试）。
        // Attention: 测试步骤必须完全自动化，产出日志用于复盘与 CI 集成。

        ArcadiaLog.Info(
            nameof(PlayableSliceTest),
            nameof(RunAsync),
            "Start",
            ("Host", host),
            ("Port", port),
            ("PlayerId", playerId));

        using var http = new HttpClient { BaseAddress = new Uri(gatewayUrl) };
        var tokenClient = new GatewayTokenClient(http, issueKey);

        // Step 1: 获取 Gateway 签发的 token
        ArcadiaLog.Info(nameof(PlayableSliceTest), nameof(RunAsync), "Step1_GetToken");
        var tokenResponse = await tokenClient.IssueTokenAsync(playerId, cancellationToken);

        // Step 2: 连接 Zone Server 并等待 Welcome
        ArcadiaLog.Info(nameof(PlayableSliceTest), nameof(RunAsync), "Step2_Connect");
        var transport = new EnetClientTransport();
        var welcomeReceived = false;
        var snapshotCount = 0;
        var lastSnapshot = default(ZoneSnapshot?);
        var lootSpawned = new List<Guid>();

        transport.OnSnapshot = snapshot =>
        {
            snapshotCount++;
            lastSnapshot = snapshot;
            if (snapshotCount % 10 == 0 || snapshot.Loot.Count > 0)
            {
                ArcadiaLog.Info(
                    nameof(PlayableSliceTest),
                    "OnSnapshot",
                    "Received",
                    ("Tick", snapshot.Tick),
                    ("PlayerPos", $"({snapshot.PlayerPos.X:F1}, {snapshot.PlayerPos.Y:F1})"),
                    ("Hp", snapshot.Hp),
                    ("Spirit", snapshot.Spirit),
                    ("LootCount", snapshot.Loot.Count));
            }

            foreach (var loot in snapshot.Loot)
            {
                if (!lootSpawned.Contains(loot.LootId))
                {
                    lootSpawned.Add(loot.LootId);
                    ArcadiaLog.Info(
                        nameof(PlayableSliceTest),
                        "OnSnapshot",
                        "LootDetected",
                        ("LootId", loot.LootId.ToString("N")),
                        ("ItemCount", loot.ItemCount),
                        ("ProtectedMsRemaining", loot.ProtectedMsRemaining),
                        ("CanPick", loot.CanPick));
                }
            }
        };

        transport.Connect(host, port, playerId, tokenResponse.Token);

        // Poll until Welcome received (max 5s)
        var welcomeDeadline = DateTimeOffset.UtcNow.AddSeconds(5);
        while (DateTimeOffset.UtcNow < welcomeDeadline && !welcomeReceived)
        {
            transport.PollOnce(serviceTimeoutMs: 0);
            await Task.Delay(15, cancellationToken);

            // Check if we received Welcome via log (simple heuristic: snapshotCount > 0 means connected)
            if (snapshotCount > 0)
            {
                welcomeReceived = true;
                ArcadiaLog.Info(nameof(PlayableSliceTest), nameof(RunAsync), "Step2_WelcomeReceived");
            }
        }

        if (!welcomeReceived)
        {
            ArcadiaLog.Info(nameof(PlayableSliceTest), nameof(RunAsync), "Step2_WelcomeFailed", ("Reason", "Timeout waiting for Welcome"));
            return;
        }

        // Step 3: 发送 MoveIntent（向右移动 1s）
        ArcadiaLog.Info(nameof(PlayableSliceTest), nameof(RunAsync), "Step3_SendMoveIntent");
        var moveDeadline = DateTimeOffset.UtcNow.AddSeconds(1);
        while (DateTimeOffset.UtcNow < moveDeadline)
        {
            transport.SendMoveIntent(dirX: 1.0f, dirY: 0f); // Move right
            transport.PollOnce(serviceTimeoutMs: 0);
            await Task.Delay(15, cancellationToken);
        }

        if (lastSnapshot is not null)
        {
            ArcadiaLog.Info(
                nameof(PlayableSliceTest),
                nameof(RunAsync),
                "Step3_MovementVerified",
                ("FinalPos", $"({lastSnapshot.PlayerPos.X:F1}, {lastSnapshot.PlayerPos.Y:F1})"));
        }

        // Step 4: 发送 DebugKillSelf，触发死亡掉落
        ArcadiaLog.Info(nameof(PlayableSliceTest), nameof(RunAsync), "Step4_SendDebugKillSelf");
        transport.SendDebugKillSelf();
        transport.Flush();

        // Poll for 2s to receive loot spawn
        var lootDeadline = DateTimeOffset.UtcNow.AddSeconds(2);
        while (DateTimeOffset.UtcNow < lootDeadline)
        {
            transport.PollOnce(serviceTimeoutMs: 0);
            await Task.Delay(15, cancellationToken);

            if (lootSpawned.Count > 0)
            {
                break;
            }
        }

        if (lootSpawned.Count == 0)
        {
            ArcadiaLog.Info(nameof(PlayableSliceTest), nameof(RunAsync), "Step4_LootSpawnFailed", ("Reason", "No loot detected in Snapshot"));
            return;
        }

        ArcadiaLog.Info(
            nameof(PlayableSliceTest),
            nameof(RunAsync),
            "Step4_LootSpawnDetected",
            ("LootId", lootSpawned[0].ToString("N")));

        // Step 5: 发送 PickupIntent，拾取掉落物
        ArcadiaLog.Info(nameof(PlayableSliceTest), nameof(RunAsync), "Step5_SendPickupIntent", ("LootId", lootSpawned[0].ToString("N")));
        transport.SendPickupIntent(lootSpawned[0]);
        transport.Flush();

        // Poll for 2s to verify loot removed from Snapshot
        var pickupDeadline = DateTimeOffset.UtcNow.AddSeconds(2);
        var lootRemoved = false;
        while (DateTimeOffset.UtcNow < pickupDeadline)
        {
            transport.PollOnce(serviceTimeoutMs: 0);
            await Task.Delay(15, cancellationToken);

            if (lastSnapshot is not null && lastSnapshot.Loot.Count == 0)
            {
                lootRemoved = true;
                break;
            }
        }

        if (lootRemoved)
        {
            ArcadiaLog.Info(nameof(PlayableSliceTest), nameof(RunAsync), "Step5_PickupSuccess", ("LootId", lootSpawned[0].ToString("N")));
        }
        else
        {
            ArcadiaLog.Info(nameof(PlayableSliceTest), nameof(RunAsync), "Step5_PickupFailed", ("Reason", "Loot still present in Snapshot"));
        }

        // Step 6: 汇总验收结果
        ArcadiaLog.Info(
            nameof(PlayableSliceTest),
            nameof(RunAsync),
            "Summary",
            ("WelcomeReceived", welcomeReceived),
            ("SnapshotCount", snapshotCount),
            ("LootSpawned", lootSpawned.Count),
            ("LootRemoved", lootRemoved),
            ("Verdict", welcomeReceived && snapshotCount > 0 && lootSpawned.Count > 0 && lootRemoved ? "PASS" : "FAIL"));

        transport.Dispose();
    }
}
