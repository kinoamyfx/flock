namespace Arcadia.Core.Items;

public sealed record ItemStack(
    ItemId ItemId,
    string TemplateId,
    int Quantity
);

