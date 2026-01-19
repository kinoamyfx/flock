using Arcadia.Core.Items;
using Arcadia.Core.Logging;
using Npgsql;

namespace Arcadia.Server.Persistence.Postgres;

public sealed class PostgresItemStore : IItemStore
{
    private readonly PostgresDatabase _db;

    public PostgresItemStore(PostgresDatabase db)
    {
        _db = db;
    }

    public async Task PutPlayerInventoryAsync(string playerId, InventorySnapshot snapshot, CancellationToken cancellationToken)
    {
        const string deleteSql = """
                                 delete from item_instances
                                 where owner_kind = 0 and owner_id = @owner_id;
                                 """;

        const string upsertSql = """
                                 insert into item_instances(item_id, template_id, quantity, owner_kind, owner_id, slot_kind)
                                 values (@item_id, @template_id, @quantity, 0, @owner_id, @slot_kind)
                                 on conflict(item_id) do update set
                                   template_id = excluded.template_id,
                                   quantity = excluded.quantity,
                                   owner_kind = excluded.owner_kind,
                                   owner_id = excluded.owner_id,
                                   slot_kind = excluded.slot_kind;
                                 """;

        try
        {
            await using var conn = await _db.OpenAsync(cancellationToken);
            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            await using (var deleteCmd = new NpgsqlCommand(deleteSql, conn, tx))
            {
                deleteCmd.Parameters.AddWithValue("owner_id", playerId);
                await deleteCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var stack in snapshot.Carried)
            {
                await UpsertItemAsync(conn, tx, playerId, stack, slotKind: ItemSlotKind.Carried, cancellationToken);
            }

            foreach (var stack in snapshot.SafeBox)
            {
                await UpsertItemAsync(conn, tx, playerId, stack, slotKind: ItemSlotKind.SafeBox, cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ArcadiaLog.Error(nameof(PostgresItemStore), nameof(PutPlayerInventoryAsync), "PersistInventoryFailed", ex, ("PlayerId", playerId));
            throw;
        }

        return;

        async Task UpsertItemAsync(NpgsqlConnection conn, NpgsqlTransaction tx, string ownerId, ItemStack stack, ItemSlotKind slotKind, CancellationToken ct)
        {
            await using var cmd = new NpgsqlCommand(upsertSql, conn, tx);
            cmd.Parameters.AddWithValue("item_id", stack.ItemId.Value);
            cmd.Parameters.AddWithValue("template_id", stack.TemplateId);
            cmd.Parameters.AddWithValue("quantity", stack.Quantity);
            cmd.Parameters.AddWithValue("owner_id", ownerId);
            cmd.Parameters.AddWithValue("slot_kind", (short)slotKind);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }

    public async Task<InventorySnapshot> GetPlayerInventoryAsync(string playerId, CancellationToken cancellationToken)
    {
        const string sql = """
                           select item_id, template_id, quantity, slot_kind
                           from item_instances
                           where owner_kind = 0 and owner_id = @owner_id;
                           """;

        var carried = new List<ItemStack>();
        var safe = new List<ItemStack>();

        await using var conn = await _db.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("owner_id", playerId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var itemId = new ItemId(reader.GetGuid(0));
            var templateId = reader.GetString(1);
            var quantity = reader.GetInt32(2);
            var slotKind = (ItemSlotKind)reader.GetInt16(3);

            var stack = new ItemStack(itemId, templateId, quantity);
            if (slotKind == ItemSlotKind.SafeBox)
            {
                safe.Add(stack);
            }
            else if (slotKind == ItemSlotKind.Carried)
            {
                carried.Add(stack);
            }
        }

        return new InventorySnapshot(carried, safe);
    }

    public async Task<(Guid LootId, IReadOnlyList<ItemStack> Dropped)> DropAllCarriedToNewLootAsync(
        string playerId,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken)
    {
        const string insertLootSql = """
                                     insert into loot_containers(loot_id, created_at_utc)
                                     values (@loot_id, @created_at_utc)
                                     on conflict(loot_id) do nothing;
                                     """;

        const string moveSql = """
                               update item_instances
                               set owner_kind = 1, owner_id = @loot_id_text, slot_kind = 2
                               where owner_kind = 0 and owner_id = @player_id and slot_kind = 0
                               returning item_id, template_id, quantity;
                               """;

        try
        {
            await using var conn = await _db.OpenAsync(cancellationToken);
            await using var tx = await conn.BeginTransactionAsync(cancellationToken);

            var lootId = Guid.NewGuid();
            await using (var insertLootCmd = new NpgsqlCommand(insertLootSql, conn, tx))
            {
                insertLootCmd.Parameters.AddWithValue("loot_id", lootId);
                insertLootCmd.Parameters.AddWithValue("created_at_utc", createdAtUtc.UtcDateTime);
                await insertLootCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            var dropped = new List<ItemStack>();
            await using (var moveCmd = new NpgsqlCommand(moveSql, conn, tx))
            {
                moveCmd.Parameters.AddWithValue("loot_id_text", lootId.ToString("N"));
                moveCmd.Parameters.AddWithValue("player_id", playerId);

                await using var reader = await moveCmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var itemId = new ItemId(reader.GetGuid(0));
                    var templateId = reader.GetString(1);
                    var quantity = reader.GetInt32(2);
                    dropped.Add(new ItemStack(itemId, templateId, quantity));
                }
            }

            await tx.CommitAsync(cancellationToken);
            return (lootId, dropped);
        }
        catch (Exception ex)
        {
            ArcadiaLog.Error(
                nameof(PostgresItemStore),
                nameof(DropAllCarriedToNewLootAsync),
                "DropAllCarriedFailed",
                ex,
                ("PlayerId", playerId),
                ("CreatedAtUtc", createdAtUtc.UtcDateTime.ToString("O")));
            throw;
        }
    }
}
